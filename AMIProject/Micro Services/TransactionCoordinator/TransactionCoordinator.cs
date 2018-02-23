﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using FTN.Common;
using CommonMS;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;

namespace TransactionCoordinator
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TransactionCoordinator : StatefulService, ITransactionCoordinator, ITransactionDuplexNMS, ITransactionDuplexCE
    {
        IReliableDictionary<string, ServiceInfo> proxies;
        private WcfTransactionCoordinatorProxy proxy;
        private WcfCommunicationClientFactory<ITransactionCoordinatorProxy> factory;

        IReliableDictionary<string, ServiceInfo> NMSs;
        private WcfNMSDelta proxyNMS;
        private WcfCommunicationClientFactory<INetworkModel> factoryNMS;

        IReliableDictionary<string, ServiceInfo> CEs;
        private WcfCEDelta proxyCE;
        private WcfCommunicationClientFactory<ICalculationEngine> factoryCE;


        public TransactionCoordinator(StatefulServiceContext context)
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
            //PROXY
            var serviceListener = new ServiceReplicaListener((context) =>
        new WcfCommunicationListener<ITransactionCoordinator>(
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
        "TransactionCoordinatorProxyListener"
    );
            //NMS 
            var NMSListener = new ServiceReplicaListener((context) =>
        new WcfCommunicationListener<ITransactionDuplexNMS>(
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
            //CE
            var CEListener = new ServiceReplicaListener((context) =>
        new WcfCommunicationListener<ITransactionDuplexCE>(
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
        "CEListener"
    );

            return new ServiceReplicaListener[] { serviceListener, NMSListener, CEListener };
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

            proxies = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceInfo>>("ProxiesForTransactionCoordinator");
            NMSs = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceInfo>>("NMSChannels");
            CEs = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceInfo>>("CEChannels");
        }

        public Task<bool> ApplyDelta(Delta delta)
        {
            List<ResourceDescription> dataForScada = new List<ResourceDescription>();
            List<ResourceDescription> dataForCE = new List<ResourceDescription>();
            Delta newDelta = null;
            bool CalculationEnginePrepareSuccess = false;
            bool ScadaPrepareSuccess = false;

            try
            {
                proxyNMS.InvokeWithRetry(client => client.Channel.EnlistDelta(delta));
                newDelta = proxyNMS.InvokeWithRetry(client => client.Channel.Prepare());
            }
            catch
            {
                return Task.FromResult<bool>(false);
            }

            if (newDelta != null)
            {
                foreach (ResourceDescription rd in newDelta.InsertOperations)
                {
                    DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);
                    if (type == DMSType.ANALOG || type == DMSType.ENERGYCONS || type == DMSType.BASEVOLTAGE ||
                        type == DMSType.POWERTRANSFORMER || type == DMSType.SUBSTATION)
                    {
                        dataForScada.Add(rd);
                    }
                    if (type == DMSType.ENERGYCONS || type == DMSType.GEOREGION || type == DMSType.SUBGEOREGION ||
                            type == DMSType.SUBSTATION || type == DMSType.BASEVOLTAGE)
                    {
                        dataForCE.Add(rd);
                    }
                }
                try
                {
                    proxy.InvokeWithRetry(client => client.Channel.EnlistMeasScada(dataForScada));
                }
                catch
                {
                    return Task.FromResult<bool>(false);
                }
                

                try
                {
                    proxyCE.InvokeWithRetry(client => client.Channel.EnlistMeas(dataForCE));
                    CalculationEnginePrepareSuccess = proxyCE.InvokeWithRetry(client => client.Channel.Prepare());
                }
                catch
                {
                    return Task.FromResult<bool>(false);
                }

                try
                {
                    ScadaPrepareSuccess = proxy.InvokeWithRetry(client => client.Channel.PrepareScada());
                }
                catch
                {
                    return Task.FromResult<bool>(false);
                }

                if (ScadaPrepareSuccess && CalculationEnginePrepareSuccess)
                {
                    try
                    {
                        proxyNMS.InvokeWithRetry(client => client.Channel.Commit());
                    }
                    catch
                    {
                        return Task.FromResult<bool>(false);
                    }

                    try
                    {
                        proxyCE.InvokeWithRetry(client => client.Channel.Commit());

                    }
                    catch
                    {
                        return Task.FromResult<bool>(false);
                    }

                    try
                    {
                        proxy.InvokeWithRetry(client => client.Channel.CommitScada());
                    }
                    catch
                    {
                        return Task.FromResult<bool>(false);
                    }

                    return Task.FromResult<bool>(true);
                }
                else
                {
                    proxyNMS.InvokeWithRetry(client => client.Channel.Rollback());

                    proxyCE.InvokeWithRetry(client => client.Channel.Rollback());

                    proxy.InvokeWithRetry(client => client.Channel.RollbackScada());

                    return Task.FromResult<bool>(false);
                }
            }
            else
            {
                proxyNMS.InvokeWithRetry(client => client.Channel.Rollback());

                return Task.FromResult<bool>(false);
            }
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

                if (!result.HasValue)
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
            factory = new WcfCommunicationClientFactory<ITransactionCoordinatorProxy>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: serviceInfo.TraceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy= new WcfTransactionCoordinatorProxy(
                            factory,
                            new Uri(serviceInfo.ServiceName),
                            ServicePartitionKey.Singleton,
                            "TransactionCoordinatorListener");
        }

        public async void ConnectNMS(ServiceInfo serviceInfo)
        {
            while (NMSs == null)
            {
                Thread.Sleep(1000);
            }

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await NMSs.TryGetValueAsync(tx, serviceInfo.TraceID);

                if (!result.HasValue)
                {
                    await NMSs.AddAsync(tx, serviceInfo.TraceID, serviceInfo);
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
            factoryNMS = new WcfCommunicationClientFactory<INetworkModel>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: serviceInfo.TraceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxyNMS = new WcfNMSDelta(
                            factoryNMS,
                            new Uri(serviceInfo.ServiceName),
                            new ServicePartitionKey(1),
                            "TransactionCoordinatorListener");
        }

        public async void ConnectCE(ServiceInfo serviceInfo)
        {
            while (CEs == null)
            {
                Thread.Sleep(1000);
            }

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await CEs.TryGetValueAsync(tx, serviceInfo.TraceID);

                if (!result.HasValue)
                {
                    await CEs.AddAsync(tx, serviceInfo.TraceID, serviceInfo);
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
            factoryCE = new WcfCommunicationClientFactory<ICalculationEngine>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: serviceInfo.TraceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxyCE = new WcfCEDelta(
                            factoryCE,
                            new Uri(serviceInfo.ServiceName),
                            ServicePartitionKey.Singleton,
                            "CETransactionCoordinatorListener");
        }
    }
}