using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using FTN.ServiceContracts;
using System.ServiceModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using FTN.Common;
using FTN.Common.Filter;
using FTN.Common.ClassesForAlarmDB;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using CommonMS;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using FTN.Services.NetworkModelService.DataModel;

namespace CEProxy
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CEProxy : StatelessService, ICalculationForClient, ICalculationEngineForScada
    {
        private WcfCommunicationClientFactory<ICalculationForClient> wcfClientFactory;
        private WcfCE proxyClient;
        private WcfCommunicationClientFactory<ICalculationEngineForScada> wcfScadaFactory;
        private WcfCEScada proxyScada;
        private IScadaForCECommand scada;

        public CEProxy(StatelessServiceContext context)
            : base(context)
        { }
        
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var clientListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ICalculationForClient>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10120/CEProxy/Client/")), "ClientListener");

            var scadaListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ICalculationEngineForScada>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10121/CEProxy/Scada/")), "ScadaListener");

            return new ServiceInstanceListener[] { clientListener, scadaListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Create binding
            Binding bindingClient = WcfUtility.CreateTcpClientBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolverClient = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            wcfClientFactory = new WcfCommunicationClientFactory<ICalculationForClient>
                (clientBinding: bindingClient, servicePartitionResolver: partitionResolverClient);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxyClient = new WcfCE(
                            wcfClientFactory,
                            new Uri("fabric:/TransactionCoordinatorMS/CE"),
                            new ServicePartitionKey(1),
                            "ClientListener");
            
            Binding bindingScada = WcfUtility.CreateTcpClientBinding();
            IServicePartitionResolver partitionResolverScada = ServicePartitionResolver.GetDefault();
            wcfScadaFactory = new WcfCommunicationClientFactory<ICalculationEngineForScada>
                (clientBinding: bindingScada, servicePartitionResolver: partitionResolverScada);
            proxyScada = new WcfCEScada(
                            wcfScadaFactory,
                            new Uri("fabric:/TransactionCoordinatorMS/CE"),
                            new ServicePartitionKey(1),
                            "ScadaListener");
        }

        public void ConnectClient()
        {
            return;
        }

        public Tuple<List<Statistics>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, ResolutionType resolution)
        {
            return proxyClient.InvokeWithRetry(client => client.Channel.GetMeasurementsForChartView(gids, from, resolution));
        }

        public Tuple<List<HourAggregation>, Statistics> GetMeasurementsForChartViewByFilter(List<long> gids, Filter filter)
        {
            return proxyClient.InvokeWithRetry(client => client.Channel.GetMeasurementsForChartViewByFilter(gids, filter));
        }

        public List<ResolvedAlarm> GetResolvedAlarms(int startIndes, int range)
        {
            return proxyClient.InvokeWithRetry(client => client.Channel.GetResolvedAlarms(startIndes, range));
        }

        public int GetTotalPageCount()
        {
            return proxyClient.InvokeWithRetry(client => client.Channel.GetTotalPageCount());
        }

        public void DataFromScada(Dictionary<long, DynamicMeasurement> measurements)
        {
            proxyScada.InvokeWithRetry(client => client.Channel.DataFromScada(measurements));
        }

        public void Connect()
        {
            scada = OperationContext.Current.GetCallbackChannel<IScadaForCECommand>();
        }
    }
}
