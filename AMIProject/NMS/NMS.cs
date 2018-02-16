using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using CommonMS;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using FTN.Services.NetworkModelService;
using System.ServiceModel;

namespace NMS
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class NMS : StatefulService, INetworkModelGDAContractDuplexClient
    {
        private IReliableDictionary<string, string> proxies;
        private WcfNMSProxy proxy;
        private WcfCommunicationClientFactory<IModelForDuplex> factory;

        private NetworkModel nm = null;
        private List<ServiceHost> hosts = null;
        private ServiceHost svcDuplexClient = null;
        private ServiceHost svcScript = null;

        public NMS(StatefulServiceContext context)
            : base(context)
        { }
        
        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            var serviceListener = new ServiceReplicaListener((context) =>
                                  new WcfCommunicationListener<INetworkModelGDAContractDuplexClient>
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
                                    "NMSProxyListener"
                                );

            return new ServiceReplicaListener[] { serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            proxies = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>("Proxies");
        }

        public void ConnectClient()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetExtentValues(FTN.Common.ModelCode entityType, List<FTN.Common.ModelCode> propIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<long>> GetGlobalIds()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetRelatedValues(long source, List<FTN.Common.ModelCode> propIds, FTN.Common.Association association)
        {
            throw new NotImplementedException();
        }

        public Task<FTN.Common.ResourceDescription> GetValues(long resourceId, List<FTN.Common.ModelCode> propIds)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IteratorClose(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<FTN.Common.ResourceDescription>> IteratorNext(int n, int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> IteratorResourcesLeft(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> IteratorResourcesTotal(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IteratorRewind(int id)
        {
            throw new NotImplementedException();
        }

        public async void Ping()
        {
            return;
        }

        public async void Connect(string traceID, string serviceName)
        {
            while (proxies == null)
            {
                Thread.Sleep(1000);
            }

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await proxies.TryGetValueAsync(tx, traceID);

                if (result.HasValue)
                {
                    await proxies.AddAsync(tx, traceID, serviceName);
                }

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }

            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            factory = new WcfCommunicationClientFactory<IModelForDuplex>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: traceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy = new WcfNMSProxy(
                            factory,
                            new Uri(serviceName),
                            ServicePartitionKey.Singleton,
                            "NMSListener");
        }
    }
}
