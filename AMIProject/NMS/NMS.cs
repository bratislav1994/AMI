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
using FTN.Common.ClassesForAlarmDB;
using FTN.Common;

namespace NMS
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class NMS : StatefulService, INetworkModelGDAContractDuplexClient, INetworkModel
    {
        private IReliableDictionary<string, ServiceInfo> proxies;
        private WcfNMSProxy proxy;
        private WcfCommunicationClientFactory<IModelForDuplex> factory;

        private WcfTransactionCoordinatorNMS proxyTransactionCoordinator;
        private WcfCommunicationClientFactory<ITransactionDuplexNMS> factoryTransactionCoordinator;

        private WcfNMSDB proxyNMSDB;
        private WcfCommunicationClientFactory<IDatabaseForNMS> factoryNMSDB;

        private NetworkModel nm = null;
        
        private GenericDataAccess gda = null;

        public NMS(StatefulServiceContext context)
            : base(context)
        {
            this.ConnectToDb();
            this.nm = new NetworkModel();
            this.nm.Initialize(ReadAllDeltas());
            this.gda = new GenericDataAccess();
            this.ConnectToTransactionCoordinator();
            this.gda.NetworkModel = nm;
            ResourceIterator.NetworkModel = nm;
        }

        #region nm
        
        private Task<bool> SaveDelta(Delta delta)
        {
            return Task.FromResult<bool>(proxyNMSDB.InvokeWithRetryAsync(client => client.Channel.SaveDelta(delta)).Result);
        }

        private List<Delta> ReadAllDeltas()
        {
            return proxyNMSDB.InvokeWithRetry(client => client.Channel.ReadDelta());
        }

        private void InformClients()
        {
            proxy.InvokeWithRetry(client => client.Channel.NewDeltaApplied());
        }

        #endregion
        
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

            var transactionCoordinatorListener = new ServiceReplicaListener((context) =>
                                  new WcfCommunicationListener<INetworkModel>
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
                                    "TransactionCoordinatorListener"
                                );

            return new ServiceReplicaListener[] { serviceListener, transactionCoordinatorListener };
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

            proxies = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceInfo>>("Proxies");
            
        }

        private void ConnectToDb()
        {
            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            factoryNMSDB = new WcfCommunicationClientFactory<IDatabaseForNMS>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxyNMSDB = new WcfNMSDB(
                            factoryNMSDB,
                            new Uri("fabric:/TransactionCoordinatorMS/NMSDB"),
                            ServicePartitionKey.Singleton, "NMSListener");

        }

        private void ConnectToTransactionCoordinator()
        {
            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            factoryTransactionCoordinator = new WcfCommunicationClientFactory<ITransactionDuplexNMS>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxyTransactionCoordinator = new WcfTransactionCoordinatorNMS(
                            factoryTransactionCoordinator,
                            new Uri("fabric:/TransactionCoordinatorMS/TransactionCoordinator"),
                            new ServicePartitionKey(1));

            proxyTransactionCoordinator.InvokeWithRetry(client => client.Channel.ConnectNMS(new ServiceInfo(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString(), FTN.Common.ServiceType.STATEFFUL)));
        }

        public void ConnectClient()
        {
            throw new NotImplementedException();
        }

        public int GetExtentValues(FTN.Common.ModelCode entityType, List<FTN.Common.ModelCode> propIds)
        {
            return this.gda.GetExtentValues(entityType, propIds);
        }

        public List<long> GetGlobalIds()
        {
            return this.gda.GetGlobalIds();
        }

        public int GetRelatedValues(long source, List<FTN.Common.ModelCode> propIds, FTN.Common.Association association)
        {
            return this.gda.GetRelatedValues(source, propIds, association);
        }

        public FTN.Common.ResourceDescription GetValues(long resourceId, List<FTN.Common.ModelCode> propIds)
        {
            return this.gda.GetValues(resourceId, propIds);
        }

        public bool IteratorClose(int id)
        {
            return this.gda.IteratorClose(id);
        }

        public List<FTN.Common.ResourceDescription> IteratorNext(int n, int id)
        {
            return this.gda.IteratorNext(n, id);
        }

        public int IteratorResourcesLeft(int id)
        {
            return this.gda.IteratorResourcesLeft(id);
        }

        public int IteratorResourcesTotal(int id)
        {
            return this.gda.IteratorResourcesTotal(id);
        }

        public bool IteratorRewind(int id)
        {
            return this.gda.IteratorRewind(id);
        }

        public async void Ping()
        {
            return;
        }

        public async void Connect(ServiceInfo serviceInfo)
        {
            while (proxies == null)
            {
                Thread.Sleep(1000);
            }

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await proxies.TryGetValueAsync(tx, serviceInfo.TraceID);

                if (result.HasValue)
                {
                    await proxies.AddAsync(tx, serviceInfo.TraceID, serviceInfo);
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
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: serviceInfo.TraceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy = new WcfNMSProxy(
                            factory,
                            new Uri(serviceInfo.ServiceName),
                            ServicePartitionKey.Singleton,
                            "NMSListener");
        }

        public void SendMeasurements(List<FTN.Services.NetworkModelService.DataModel.DynamicMeasurement> measurements)
        {
            throw new NotImplementedException();
        }

        public void SendAlarm(DeltaForAlarm delta)
        {
            throw new NotImplementedException();
        }

        #region INetworkModel

        public void EnlistDelta(Delta delta)
        {
            this.gda.EnlistDelta(delta);
        }

        public Delta Prepare()
        {
            return this.gda.Prepare();
        }

        public void Commit()
        {
            this.gda.Commit();
            this.SaveDelta(this.gda.delta);
            new Thread(() => this.InformClients()).Start();
        }

        public void Rollback()
        {
            this.gda.Rollback();
        }

        #endregion
    }
}
