﻿using System;
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
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal sealed class NMSProxy : StatelessService, IModelForDuplex, INetworkModelGDAContractDuplexClient
    {
        private List<IModelForDuplex> clients;
        private WcfCommunicationClientFactory<INMSMicroService> wcfClientFactory;
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
            string host = Context.NodeContext.IPAddressOrFQDN;
            
            var clientListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<INetworkModelGDAContractDuplexClient>(context, this,
            new NetTcpBinding(), new EndpointAddress("net.tcp://" + host + ":10200/NMSProxy/Client/")), "ClientListener");

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
            ConnectToNMS();

            if (Clients == null)
            {
                this.Clients = new List<IModelForDuplex>();
            }
        }

        private void ConnectToNMS()
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
            wcfClientFactory = new WcfCommunicationClientFactory<INMSMicroService>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            
            proxy = new WcfNMS(
                            wcfClientFactory,
                            new Uri("fabric:/TransactionCoordinatorMS/NMS"),
                            new ServicePartitionKey(1),
                            "NMSProxyListener");

            proxy.InvokeWithRetry(client => client.Channel.Connect(new ServiceInfo(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString(), ServiceType.STATELESS)));
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
            List<IModelForDuplex> clientsForDeleting = new List<IModelForDuplex>();

            foreach (IModelForDuplex client in this.Clients)
            {
                try
                {
                    if (!client.PingClient())
                    {
                        clientsForDeleting.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    clientsForDeleting.Add(client);
                }
            }

            clientsForDeleting.ForEach(x => Clients.Remove(x));

            this.Clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());

        }

        public void Ping()
        {
            while (true)
            {
                try
                {
                    proxy.InvokeWithRetry(client => client.Channel.Ping());
                    return;
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.GetValues(resourceId, propIds));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public List<long> GetGlobalIds()
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.GetGlobalIds());
                }
                catch
                {
                    ConnectToNMS();
                }
            }
            
        }

        public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.GetExtentValues(entityType, propIds));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public int GetRelatedValues(List<long> source, List<ModelCode> propIds, Association association)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.GetRelatedValues(source, propIds, association));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public List<ResourceDescription> IteratorNext(int n, int id)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.IteratorNext(n, id));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public bool IteratorRewind(int id)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.IteratorRewind(id));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public int IteratorResourcesTotal(int id)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.IteratorResourcesTotal(id));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public int IteratorResourcesLeft(int id)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.IteratorResourcesLeft(id));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
        }

        public bool IteratorClose(int id)
        {
            while (true)
            {
                try
                {
                    return proxy.InvokeWithRetry(client => client.Channel.IteratorClose(id));
                }
                catch
                {
                    ConnectToNMS();
                }
            }
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
            List<IModelForDuplex> clientsForDeleting = new List<IModelForDuplex>();

            foreach (IModelForDuplex client in this.Clients)
            {
                try
                {
                    if (!client.PingClient())
                    {
                        clientsForDeleting.Add(client);
                    }
                    else
                    {
                        client.NewDeltaApplied();
                    }
                }
                catch (Exception ex)
                {
                    clientsForDeleting.Add(client);
                }
            }

            clientsForDeleting.ForEach(x => Clients.Remove(x));
        }

        public bool PingClient()
        {
            throw new NotImplementedException();
        }
    }
}
