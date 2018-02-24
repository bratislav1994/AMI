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
    internal sealed class CEProxy : StatelessService, ICalculationForClient, ICalculationEngineForScada, IScadaForCECommand
    {
        private WcfCommunicationClientFactory<ICalculationForClient> wcfClientFactory;
        private WcfCE proxyClient;
        private WcfCommunicationClientFactory<ICEScada> wcfScadaFactory;
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
            NetTcpBinding bindingClient = new NetTcpBinding();
            bindingClient.ReceiveTimeout = TimeSpan.MaxValue;
            bindingClient.MaxReceivedMessageSize = Int32.MaxValue;
            bindingClient.MaxBufferSize = Int32.MaxValue;

            var clientListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ICalculationForClient>(context, this,
            bindingClient, new EndpointAddress("net.tcp://localhost:10100/CEProxy/Client/")), "ClientListener");

            NetTcpBinding bindingScada = new NetTcpBinding();
            bindingScada.ReceiveTimeout = TimeSpan.MaxValue;
            bindingScada.MaxReceivedMessageSize = Int32.MaxValue;
            bindingScada.MaxBufferSize = Int32.MaxValue;

            var scadaListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ICalculationEngineForScada>(context, this,
            bindingScada, new EndpointAddress("net.tcp://localhost:10101/CEProxy/Scada/")), "ScadaListener");

            Binding listenerBinding = WcfUtility.CreateTcpClientBinding();
            listenerBinding.ReceiveTimeout = TimeSpan.MaxValue;

            var serviceListener = new ServiceInstanceListener((context) =>
        new WcfCommunicationListener<IScadaForCECommand>(
            wcfServiceObject: this,
            serviceContext: context,
            //
            // The name of the endpoint configured in the ServiceManifest under the Endpoints section
            // that identifies the endpoint that the WCF ServiceHost should listen on.
            //
            endpointResourceName: "ServiceEndpoint",

            //
            // Populate the binding information that you want the service to use.
            //
            listenerBinding: listenerBinding
        ),
        "CEScadaListener"
    );

            return new ServiceInstanceListener[] { clientListener, scadaListener, serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Create binding
            Binding bindingClient = WcfUtility.CreateTcpClientBinding();
            bindingClient.ReceiveTimeout = TimeSpan.MaxValue;
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
                            new Uri("fabric:/TransactionCoordinatorMS/CEClient"),
                            ServicePartitionKey.Singleton,
                            "CEClientListener");

            Binding bindingScada = WcfUtility.CreateTcpClientBinding();
            bindingScada.ReceiveTimeout = TimeSpan.MaxValue;
            IServicePartitionResolver partitionResolverScada = ServicePartitionResolver.GetDefault();
            wcfScadaFactory = new WcfCommunicationClientFactory<ICEScada>
                (clientBinding: bindingScada, servicePartitionResolver: partitionResolverScada);
            proxyScada = new WcfCEScada(
                            wcfScadaFactory,
                            new Uri("fabric:/TransactionCoordinatorMS/CEScada"),
                            new ServicePartitionKey(1),
                            "ScadaListener");

            proxyScada.InvokeWithRetry(client => client.Channel.Connect(new ServiceInfo(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString(), ServiceType.STATELESS)));
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
            new Thread(() => ForwardMeasurements(measurements)).Start();
        }

        private void ForwardMeasurements(Dictionary<long, DynamicMeasurement> measurements)
        {
            proxyScada.InvokeWithRetry(client => client.Channel.DataFromScada(measurements));
        }

        public void Connect(string endpointName)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            binding.MaxBufferSize = Int32.MaxValue;
            ChannelFactory<IScadaForCECommand> factory = new ChannelFactory<IScadaForCECommand>(binding,
                                                                                new EndpointAddress(endpointName));
            scada = factory.CreateChannel();
        }

        public string Command(Dictionary<long, DynamicMeasurement> measurementsInAlarm)
        {
            try
            {
                return scada.Command(measurementsInAlarm);
            }
            catch
            {
                return "Scada not online";
            }
        }
    }
}
