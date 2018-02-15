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
    internal sealed class TransactionCoordinator : StatefulService, ITransactionCoordinator
    {
        IReliableDictionary<string, string> proxies;
        private WcfTransactionCoordinatorProxy proxy;
        private WcfCommunicationClientFactory<ITransactionCoordinatorProxy> factory;


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

        public Task<bool> ApplyDelta(Delta delta)
        {
            List<ResourceDescription> dataForScada = new List<ResourceDescription>();
            List<ResourceDescription> dataForCE = new List<ResourceDescription>();
            Delta newDelta = null;
            bool CalculationEnginePrepareSuccess = false;
            bool ScadaPrepareSuccess = false;

            try
            {
                proxy.InvokeWithRetry(client => client.Channel.EnlistDeltaNMS(delta));
                newDelta = proxy.InvokeWithRetry(client => client.Channel.PrepareNMS());
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
                            type == DMSType.SUBSTATION)
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
                    proxy.InvokeWithRetry(client => client.Channel.EnlistDeltaCE(dataForCE));
                    CalculationEnginePrepareSuccess = proxy.InvokeWithRetry(client => client.Channel.PrepareCE());
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
                        proxy.InvokeWithRetry(client => client.Channel.CommitNMS());
                    }
                    catch
                    {
                        return Task.FromResult<bool>(false);
                    }

                    try
                    {
                        proxy.InvokeWithRetry(client => client.Channel.CommitCE());

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
                    proxy.InvokeWithRetry(client => client.Channel.RollbackNMS());

                    proxy.InvokeWithRetry(client => client.Channel.RollbackCE());

                    proxy.InvokeWithRetry(client => client.Channel.RollbackScada());

                    return Task.FromResult<bool>(false);
                }
            }
            else
            {
                proxy.InvokeWithRetry(client => client.Channel.RollbackNMS());

                return Task.FromResult<bool>(false);
            }
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
            factory = new WcfCommunicationClientFactory<ITransactionCoordinatorProxy>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId:traceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy= new WcfTransactionCoordinatorProxy(
                            factory,
                            new Uri(serviceName),
                            ServicePartitionKey.Singleton,
                            "TransactionCoordinatorListener");
        }
    }
}
