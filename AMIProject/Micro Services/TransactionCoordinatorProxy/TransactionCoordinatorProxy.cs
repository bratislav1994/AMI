using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using FTN.Common;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using CommonMS;
using System.Fabric.Management.ServiceModel;

namespace TransactionCoordinatorProxy
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionCoordinatorProxy : StatelessService, ITransactionCoordinatorProxy, ITransactionCoordinator, /*ITransactionDuplexCE, ITransactionDuplexNMS,*/ ITransactionDuplexScada
    {
        private ICalculationEngine ce;
        private INetworkModel nms;
        private IScada scada;
        private WcfCommunicationClientFactory<ITransactionCoordinator> wcfClientFactory;
        private WcfTransactionCoordinator proxy;
        private StatelessServiceContext context;

        public TransactionCoordinatorProxy(StatelessServiceContext context)
            : base(context)
        { }
        
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var adapterListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ITransactionCoordinator>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10100/TransactionCoordinatorProxy/Adapter/")),"AdapterListener");

            /*var CEListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ITransactionDuplexCE>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10101/TransactionCoordinatorProxy/CalculationEngine/")), "CEListener");*/
            
            /*var NMSListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ITransactionDuplexNMS>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10102/TransactionCoordinatorProxy/NMS/")), "NMSListener");*/
            
            var scadaListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ITransactionDuplexScada>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:10103/TransactionCoordinatorProxy/Scada/")), "ScadaListener");

            var serviceListener = new ServiceInstanceListener((context) =>
        new WcfCommunicationListener<ITransactionCoordinatorProxy>(
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

            return new ServiceInstanceListener[] { adapterListener, /*CEListener, NMSListener,*/ scadaListener, serviceListener };
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
            wcfClientFactory = new WcfCommunicationClientFactory<ITransactionCoordinator>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy = new WcfTransactionCoordinator(
                            wcfClientFactory,
                            new Uri("fabric:/TransactionCoordinatorMS/TransactionCoordinator"),
                            new ServicePartitionKey(1));

            proxy.InvokeWithRetry(client => client.Channel.Connect(new ServiceInfo(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString(), FTN.Common.ServiceType.STATELESS)));
        }

        public Task<bool> ApplyDelta(Delta delta)
        {
            return Task.FromResult<bool>(proxy.InvokeWithRetryAsync(
                client => client.Channel.ApplyDelta(delta)).Result);
        }

        /*public void ConnectCE()
        {
            ce = OperationContext.Current.GetCallbackChannel<ICalculationEngine>();
        }

        public void ConnectNMS()
        {
            nms = OperationContext.Current.GetCallbackChannel<INetworkModel>();
        }*/

        public void ConnectScada()
        {
            scada = OperationContext.Current.GetCallbackChannel<IScada>();
        }

        /*public void EnlistDeltaNMS(Delta delta)
        {
            nms.EnlistDelta(delta);
        }*/

        public void EnlistMeasScada(List<ResourceDescription> measurements)
        {
            scada.EnlistMeas(measurements);
        }

        /*public void EnlistDeltaCE(List<ResourceDescription> data)
        {
            ce.EnlistMeas(data);
        }*/

        /*public Delta PrepareNMS()
        {
            return nms.Prepare();
        }

        public bool PrepareCE()
        {
            return ce.Prepare();
        }*/

        public bool PrepareScada()
        {
            return scada.Prepare();
        }

        /*public void CommitNMS()
        {
            nms.Commit();
        }

        public void CommitCE()
        {
            ce.Commit();
        }
        */
        public void CommitScada()
        {
            scada.Commit();
        }

        /*public void RollbackNMS()
        {
            nms.Rollback();
        }

        public void RollbackCE()
        {
            ce.Rollback();
        }*/

        public void RollbackScada()
        {
            scada.Rollback();
        }

        public void Connect(ServiceInfo serviceInfo)
        {
            throw new NotImplementedException();
        }
    }
}
