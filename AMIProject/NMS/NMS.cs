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
        private IReliableDictionary<string, string> proxies;
        private WcfNMSProxy proxy;
        private WcfCommunicationClientFactory<IModelForDuplex> factory;

        private NetworkModel nm = null;
        private List<ServiceHost> hosts = null;

        private bool firstContactDB = true;
        private IDatabaseForNMS dbProxy;

        private ITransactionDuplexNMS proxyCoordinator;
        private bool firstTimeCoordinator = true;
        private GenericDataAccess gda = null;

        public NMS(StatefulServiceContext context)
            : base(context)
        {
            this.ConnectToDb();
            this.nm = new NetworkModel();
            this.nm.Initialize(ReadAllDeltas());
            this.gda = new GenericDataAccess();
            this.Run();
            this.gda.NetworkModel = nm;
            ResourceIterator.NetworkModel = nm;
            //svcScript = new ServiceHost(gda);
            //svcScript.AddServiceEndpoint(typeof(INMSForScript),
            //                        new NetTcpBinding(),
            //                        new Uri("net.tcp://localhost:10011/NetworkModelService/FillingScript"));
        }

        #region nm

        private void ConnectToDb()
        {
            while (true)
            {
                try
                {
                    this.DBProxy.Connect();
                    break;
                }
                catch
                {
                    FirstContactDB = true;
                    Thread.Sleep(1000);
                }
            }
        }

        private void SaveDelta(Delta delta)
        {
            DBProxy.SaveDelta(delta);
        }

        private List<Delta> ReadAllDeltas()
        {
            return DBProxy.ReadDelta();
        }

        private void InformClients()
        {
            proxy.InvokeWithRetry(client => client.Channel.NewDeltaApplied());
        }

        #endregion

        #region gda

        public void Run()
        {
            while (true)
            {
                try
                {
                    this.ProxyCoordinator.ConnectNMS();
                    break;
                }
                catch
                {
                    FirstTimeCoordinator = true;
                    Thread.Sleep(1000);
                }
            }
        }

        public ITransactionDuplexNMS ProxyCoordinator
        {
            get
            {
                if (FirstTimeCoordinator)
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    DuplexChannelFactory<ITransactionDuplexNMS> factory = new DuplexChannelFactory<ITransactionDuplexNMS>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress(/*"net.tcp://localhost:10003/TransactionCoordinator/NMS"*/"net.tcp://localhost:10102/TransactionCoordinatorProxy/NMS/"));
                    proxyCoordinator = factory.CreateChannel();
                    FirstTimeCoordinator = false;
                }

                return proxyCoordinator;
            }

            set
            {
                proxyCoordinator = value;
            }
        }

        public bool FirstTimeCoordinator
        {
            get
            {
                return firstTimeCoordinator;
            }

            set
            {
                firstTimeCoordinator = value;
            }
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

        public IDatabaseForNMS DBProxy
        {
            get
            {
                if (FirstContactDB)
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    binding.SendTimeout = TimeSpan.FromMinutes(5);
                    ChannelFactory<IDatabaseForNMS> factoryDB = new ChannelFactory<IDatabaseForNMS>(binding,
                                                                                        new EndpointAddress("net.tcp://localhost:10009/Database/NMS"));
                    dbProxy = factoryDB.CreateChannel();
                    FirstContactDB = false;
                }

                return dbProxy;
            }

            set
            {
                dbProxy = value;
            }
        }

        public bool FirstContactDB
        {
            get
            {
                return firstContactDB;
            }

            set
            {
                firstContactDB = value;
            }
        }

        public void ConnectClient()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetExtentValues(FTN.Common.ModelCode entityType, List<FTN.Common.ModelCode> propIds)
        {
            return this.gda.GetExtentValues(entityType, propIds);
        }

        public Task<List<long>> GetGlobalIds()
        {
            return this.gda.GetGlobalIds();
        }

        public Task<int> GetRelatedValues(long source, List<FTN.Common.ModelCode> propIds, FTN.Common.Association association)
        {
            return this.gda.GetRelatedValues(source, propIds, association);
        }

        public Task<FTN.Common.ResourceDescription> GetValues(long resourceId, List<FTN.Common.ModelCode> propIds)
        {
            return this.gda.GetValues(resourceId, propIds);
        }

        public Task<bool> IteratorClose(int id)
        {
            return this.gda.IteratorClose(id);
        }

        public Task<List<FTN.Common.ResourceDescription>> IteratorNext(int n, int id)
        {
            return this.gda.IteratorNext(n, id);
        }

        public Task<int> IteratorResourcesLeft(int id)
        {
            return this.gda.IteratorResourcesLeft(id);
        }

        public Task<int> IteratorResourcesTotal(int id)
        {
            return this.gda.IteratorResourcesTotal(id);
        }

        public Task<bool> IteratorRewind(int id)
        {
            return this.gda.IteratorRewind(id);
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

        public void NewDeltaApplied()
        {
            proxy.InvokeWithRetry(client => client.Channel.NewDeltaApplied());
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
