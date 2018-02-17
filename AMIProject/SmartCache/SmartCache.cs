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

namespace SmartCache
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class SmartCache : StatefulService, ISmartCacheDuplexForClient, ISmartCacheForCE
    {
        private IReliableDictionary<string, string> proxies;
        private WcfSmartCacheProxy proxy;
        private WcfCommunicationClientFactory<IModelForDuplex> factory;

        private ICalculationEngineDuplexSmartCache proxyCE;
        private bool firstTimeCE = true;

        private Dictionary<long, DynamicMeasurement> measurements = new Dictionary<long, DynamicMeasurement>();

        public SmartCache(StatefulServiceContext context)
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
                new WcfCommunicationListener<ISmartCacheDuplexForClient>(
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
            "SmartCacheProxyListener"
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
            proxy = new WcfSmartCacheProxy(
                            factory,
                            new Uri(serviceName),
                            ServicePartitionKey.Singleton,
                            "SmartCacheListener");
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    this.ProxyCE.Subscribe();
                    break;
                }
                catch
                {
                    FirstTimeCE = true;
                    Thread.Sleep(1000);
                }
            }
        }

        public bool FirstTimeCE
        {
            get
            {
                return firstTimeCE;
            }

            set
            {
                firstTimeCE = value;
            }
        }

        public ICalculationEngineDuplexSmartCache ProxyCE
        {
            get
            {
                if (firstTimeCE)
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    DuplexChannelFactory<ICalculationEngineDuplexSmartCache> factory = new DuplexChannelFactory<ICalculationEngineDuplexSmartCache>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress(/*"net.tcp://localhost:10007/ICalculationEngineDuplexSmartCache/SmartCache"*/ "net.tcp://localhost:10208/SmartCache/Client"));
                    proxyCE = factory.CreateChannel();
                    firstTimeCE = false;
                }

                return proxyCE;
            }

            set
            {
                proxyCE = value;
            }
        }

        public List<DynamicMeasurement> GetLastMeas(List<long> gidsInTable)
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

        public void SendAlarm(FTN.Common.ClassesForAlarmDB.DeltaForAlarm delta)
        {
            proxy.InvokeWithRetry(client => client.Channel.SendAlarm(delta));
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

            proxy.InvokeWithRetry(client => client.Channel.SendMeasurements(this.measurements.Values.ToList()));
        }

        public void Subscribe()
        {
            throw new NotImplementedException();
        }
    }
}
