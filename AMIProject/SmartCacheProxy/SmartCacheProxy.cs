using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using CommonMS;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Common.ClassesForAlarmDB;

namespace SmartCacheProxy
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class SmartCacheProxy : StatelessService, ISmartCacheDuplexForClient, IModelForDuplex
    {
        private List<IModelForDuplex> clients;
        private WcfCommunicationClientFactory<ISmartCacheDuplexForClient> wcfClientFactory;
        private WcfSmartCache proxy;
        private StatelessServiceContext context;

        public SmartCacheProxy(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var clientListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ISmartCacheDuplexForClient>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10208/SmartCache/Client")), "ClientListener");

            var serviceListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<IModelForDuplex>(
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
                listenerBinding: WcfUtility.CreateTcpListenerBinding()
            ),
            "SmartCacheListener"
            );

            return new ServiceInstanceListener[] { clientListener, serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            wcfClientFactory = new WcfCommunicationClientFactory<ISmartCacheDuplexForClient>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //

            if (Clients == null)
            {
                this.Clients = new List<IModelForDuplex>();
            }

            proxy = new WcfSmartCache(
                            wcfClientFactory,
                            new Uri("fabric:/TransactionCoordinatorMS/SmartCache"),
                            new ServicePartitionKey(1));

            proxy.InvokeWithRetry(client => client.Channel.Connect(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString()));
        }

        public List<IModelForDuplex> Clients
        {
            get
            {
                return clients;
            }

            set
            {
                clients = value;
            }
        }

        public void Subscribe()
        {
            this.Clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());
        }

        public List<DynamicMeasurement> GetLastMeas(List<long> gidsInTable)
        {
            throw new NotImplementedException();
        }

        public void Connect(string traceID, string serviceName)
        {
            throw new NotImplementedException();
        }

        public void NewDeltaApplied()
        {
            throw new NotImplementedException();
        }

        public void SendMeasurements(List<DynamicMeasurement> measurements)
        {
            try
            {
                this.Clients.ForEach(x => x.SendMeasurements(measurements));
            }
            catch
            {
            }
        }

        public void SendAlarm(DeltaForAlarm delta)
        {
            try
            {
                this.Clients.ForEach(x => x.SendAlarm(delta));
            }
            catch
            {
            }
        }
    }
}
