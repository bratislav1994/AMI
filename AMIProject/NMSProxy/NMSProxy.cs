using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using FTN.Common;
using CommonMS;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;

namespace NMSProxy
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class NMSProxy : StatelessService, IModelForDuplex, INetworkModelGDAContractDuplexClient
    {
        private List<IModelForDuplex> clients;
        private WcfCommunicationClientFactory<INetworkModelGDAContractDuplexClient> wcfClientFactory;
        private WcfNMS proxy;
        private StatelessServiceContext context;

        public NMSProxy(StatelessServiceContext context)
            : base(context)
        { }
        
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var clientListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<INetworkModelGDAContractDuplexClient>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10110/NMSProxy/Client/")), "ClientListener");

            var serviceListener = new ServiceInstanceListener((context) => 
                                    new WcfCommunicationListener<IModelForDuplex>
                                    (
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
                                    "NMSListener"
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
            wcfClientFactory = new WcfCommunicationClientFactory<INetworkModelGDAContractDuplexClient>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //

            if (Clients == null)
            {
                this.Clients = new List<IModelForDuplex>();
            }

            proxy = new WcfNMS(
                            wcfClientFactory,
                            new Uri("fabric:/NetworkModelServiceMS/NMS"),
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

        public void ConnectClient()
        {
            this.Clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());
        }

        public void Ping()
        {
            proxy.InvokeWithRetry(client => client.Channel.Ping());
        }

        public Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds)
        {
            return Task.FromResult<ResourceDescription>(proxy.InvokeWithRetryAsync(client => client.Channel.GetValues(resourceId, propIds)).Result);
        }

        public Task<List<long>> GetGlobalIds()
        {
            return Task.FromResult<List<long>>(proxy.InvokeWithRetryAsync(client => client.Channel.GetGlobalIds()).Result);
        }

        public Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            return Task.FromResult<int>(proxy.InvokeWithRetryAsync(client => client.Channel.GetExtentValues(entityType, propIds)).Result);
        }

        public Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
        {
            return Task.FromResult<int>(proxy.InvokeWithRetryAsync(client => client.Channel.GetRelatedValues(source, propIds, association)).Result);
        }

        public Task<List<ResourceDescription>> IteratorNext(int n, int id)
        {
            return Task.FromResult<List<ResourceDescription>>(proxy.InvokeWithRetryAsync(client => client.Channel.IteratorNext(n, id)).Result);
        }

        public Task<bool> IteratorRewind(int id)
        {
            return Task.FromResult<bool>(proxy.InvokeWithRetryAsync(client => client.Channel.IteratorRewind(id)).Result);
        }

        public Task<int> IteratorResourcesTotal(int id)
        {
            return Task.FromResult<int>(proxy.InvokeWithRetryAsync(client => client.Channel.IteratorResourcesTotal(id)).Result);
        }

        public Task<int> IteratorResourcesLeft(int id)
        {
            return Task.FromResult<int>(proxy.InvokeWithRetryAsync(client => client.Channel.IteratorResourcesLeft(id)).Result);
        }

        public Task<bool> IteratorClose(int id)
        {
            return Task.FromResult<bool>(proxy.InvokeWithRetryAsync(client => client.Channel.IteratorClose(id)).Result);
        }

        public void SendAlarm(FTN.Common.ClassesForAlarmDB.DeltaForAlarm delta)
        {
            throw new NotImplementedException();
        }

        public void SendMeasurements(List<FTN.Services.NetworkModelService.DataModel.DynamicMeasurement> measurements)
        {
            throw new NotImplementedException();
        }

        public void NewDeltaApplied()
        {
            try
            {
                this.Clients.ForEach(x => x.NewDeltaApplied());
            }
            catch
            {
            }
        }

        public void Connect(string traceID, string serviceName)
        {
            throw new NotImplementedException();
        }
    }
}
