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
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Client;
using System.ServiceModel.Channels;
using System.ServiceModel;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Common;

namespace SmartCache
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class SmartCache : StatefulService, ISmartCacheMS, ISmartCacheForCE
    {
        private IReliableDictionary<string, string> proxies;
        private WcfSmartCacheProxy proxy;
        private WcfCommunicationClientFactory<IModelForDuplex> factory;

        private WcfCESmartCache proxyCE;
        private WcfCommunicationClientFactory<ICalculationEngineDuplexSmartCache> factoryCE;

        private Dictionary<long, DynamicMeasurement> measurements = new Dictionary<long, DynamicMeasurement>();
        private object lockObjectForClient = new object();

        public SmartCache(StatefulServiceContext context)
            : base(context)
        {
            Run();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            Binding listenerBinding = WcfUtility.CreateTcpClientBinding();
            listenerBinding.ReceiveTimeout = TimeSpan.MaxValue;

            var serviceListener = new ServiceReplicaListener((context) =>
                new WcfCommunicationListener<ISmartCacheMS>(
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
            "SmartCacheProxyListener"
            );

            Binding listenerBindingCE = WcfUtility.CreateTcpClientBinding();
            listenerBindingCE.ReceiveTimeout = TimeSpan.MaxValue;

            var CEListener = new ServiceReplicaListener((context) =>
                new WcfCommunicationListener<ISmartCacheForCE>(
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
                listenerBinding: listenerBindingCE
                ),
            "CEListener"
            );

            return new ServiceReplicaListener[] { serviceListener, CEListener };
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
                    await proxies.AddAsync(tx, serviceInfo.TraceID, serviceInfo.ServiceName);
                }

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }

            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            factory = new WcfCommunicationClientFactory<IModelForDuplex>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: serviceInfo.TraceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy = new WcfSmartCacheProxy(
                            factory,
                            new Uri(serviceInfo.ServiceName),
                            ServicePartitionKey.Singleton,
                            "SmartCacheListener");
        }

        public void Run()
        {
            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            factoryCE = new WcfCommunicationClientFactory<ICalculationEngineDuplexSmartCache>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxyCE = new WcfCESmartCache(
                            factoryCE,
                            new Uri("fabric:/TransactionCoordinatorMS/CEScada"),
                            new ServicePartitionKey(1),
                            "SmartCacheListener");

            proxyCE.InvokeWithRetry(client => client.Channel.Subscribe(new ServiceInfo(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString(), ServiceType.STATEFFUL)));
        }

        public List<DynamicMeasurement> GetLastMeas(List<long> gidsInTable)
        {
            lock (lockObjectForClient)
            {
                List<DynamicMeasurement> retVal = new List<DynamicMeasurement>();
                foreach (long gid in gidsInTable)
                {
                    if (measurements.ContainsKey(gid))
                    {
                        retVal.Add(measurements[gid]);
                    }
                }

                return retVal;
            }
        }

        public void SendAlarm(FTN.Common.ClassesForAlarmDB.DeltaForAlarm delta)
        {
            lock (lockObjectForClient)
            {
                proxy.InvokeWithRetry(client => client.Channel.SendAlarm(delta));
            }
        }

        public void SendMeasurements(Dictionary<long, DynamicMeasurement> measurements)
        {
            foreach (KeyValuePair<long, DynamicMeasurement> kvp in measurements)
            {
                if (this.measurements.ContainsKey(kvp.Key))
                {
                    this.measurements[kvp.Key] = kvp.Value;
                }
                else
                {
                    this.measurements.Add(kvp.Key, kvp.Value);
                }
            }

            lock (lockObjectForClient)
            {
                proxy.InvokeWithRetry(client => client.Channel.SendMeasurements(this.measurements.Values.ToList()));
            }
        }
    }
}
