﻿using System;
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
            string host = Context.NodeContext.IPAddressOrFQDN;
            NetTcpBinding bindingClient = new NetTcpBinding();
            bindingClient.ReceiveTimeout = TimeSpan.MaxValue;
            bindingClient.MaxReceivedMessageSize = Int32.MaxValue;
            bindingClient.MaxBufferSize = Int32.MaxValue;

            var adapterListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ITransactionCoordinator>(context, this,
             bindingClient, new EndpointAddress("net.tcp://" + host + ":10300/TransactionCoordinatorProxy/Adapter/")), "AdapterListener");

            NetTcpBinding bindingScada = new NetTcpBinding();
            bindingScada.ReceiveTimeout = TimeSpan.MaxValue;
            bindingScada.MaxReceivedMessageSize = Int32.MaxValue;
            bindingScada.MaxBufferSize = Int32.MaxValue;

            var scadaListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ITransactionDuplexScada>(context, this,
            bindingScada, new EndpointAddress("net.tcp://" + host + ":10301/TransactionCoordinatorProxy/Scada/")), "ScadaListener");

            Binding listenerBinding = WcfUtility.CreateTcpClientBinding();
            listenerBinding.ReceiveTimeout = TimeSpan.MaxValue;

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
            listenerBinding: listenerBinding
        ),
        "TransactionCoordinatorListener"
    );

            return new ServiceInstanceListener[] { adapterListener, scadaListener, serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ConnectToTC();
        }

        private void ConnectToTC()
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.SendTimeout = TimeSpan.MaxValue;
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
                            new ServicePartitionKey(1),
                            "TransactionCoordinatorProxyListener");

            proxy.InvokeWithRetry(client => client.Channel.Connect(new ServiceInfo(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString(), FTN.Common.ServiceType.STATELESS)));
        }

        public string ApplyDelta(Delta delta)
        {
            while (true)
            {
                try
                {
                    return (proxy.InvokeWithRetry(client => client.Channel.ApplyDelta(delta)));
                }
                catch
                {
                    ConnectToTC();
                }
            }
        }

        public void ConnectScada()
        {
            scada = OperationContext.Current.GetCallbackChannel<IScada>();
        }

        public bool EnlistMeasScada(List<ResourceDescription> measurements)
        {
            try
            {
                scada.EnlistMeas(measurements);
                return true; 
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool PrepareScada()
        {
            try
            {
                return scada.Prepare();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void CommitScada()
        {
            try
            {
                scada.Commit();
            }
            catch (Exception ex)
            {
            }
        }

        public void RollbackScada()
        {
            try
            {
                scada.Rollback();
            }
            catch (Exception ex)
            {
            }
        }

        public void Connect(ServiceInfo serviceInfo)
        {
            throw new NotImplementedException();
        }

        public void Ping()
        {
            while (true)
            {
                try
                {
                    proxy.InvokeWithRetry(client => client.Channel.Ping());
                    break;
                }
                catch
                {
                    ConnectToTC();
                }
            }
        }

        public void PingFromScada()
        {
            while (true)
            {
                try
                {
                    proxy.InvokeWithRetry(client => client.Channel.Ping());
                    break;
                }
                catch
                {
                    ConnectToTC();
                }
            }
        }
    }
}
