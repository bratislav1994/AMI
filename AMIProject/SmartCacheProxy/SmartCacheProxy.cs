﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using CommonMS;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Common.ClassesForAlarmDB;
using FTN.Common;

namespace SmartCacheProxy
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal sealed class SmartCacheProxy : StatelessService, ISmartCacheDuplexForClient, IModelForDuplex
    {
        private List<IModelForDuplex> clients;
        private WcfCommunicationClientFactory<ISmartCacheMS> wcfClientFactory;
        private WcfSmartCache proxy;
        private StatelessServiceContext context;
        private object lockObjectForClients = new object();

        public SmartCacheProxy(StatelessServiceContext context)
            : base(context)
        {
        }

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

            var clientListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<ISmartCacheDuplexForClient>(context, this,
            bindingClient, new EndpointAddress("net.tcp://" + host + ":10400/SmartCache/Client/")), "ClientListener");

            Binding listenerBinding = WcfUtility.CreateTcpClientBinding();
            listenerBinding.ReceiveTimeout = TimeSpan.MaxValue;

            var serviceListener = new ServiceInstanceListener((context) =>
            new WcfCommunicationListener<IModelForDuplex>(
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
            "SmartCacheListener"
            );

            return new ServiceInstanceListener[] { clientListener, serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ConnectToSmartCache();
        }

        private void ConnectToSmartCache()
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
            wcfClientFactory = new WcfCommunicationClientFactory<ISmartCacheMS>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //

            if (Clients == null)
            {
                this.Clients = new List<IModelForDuplex>();
            }

            proxy = new WcfSmartCache(
                            wcfClientFactory,
                            new Uri("fabric:/TransactionCoordinatorMS/SmartCache"),
                            new ServicePartitionKey(1),
                            "SmartCacheProxyListener");

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

        public void Subscribe()
        {
            List<IModelForDuplex> clientsForDeleting = new List<IModelForDuplex>();

            ServiceEventSource.Current.ServiceMessage(this.Context, "Subscribe(SCProxy) on " + this.Context.NodeContext.NodeName.ToString() + ", no. of clients = " + Clients.Count);

            lock (lockObjectForClients)
            {
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

            ServiceEventSource.Current.ServiceMessage(this.Context, "Subscribe(SCProxy) done on " + this.Context.NodeContext.NodeName.ToString() + ", no. of clients = " + Clients.Count);
        }

        public List<DynamicMeasurement> GetLastMeas(List<long> gidsInTable)
        {
            List<DynamicMeasurement> retVal = new List<DynamicMeasurement>();

            while (true)
            {
                try
                {
                    retVal = proxy.InvokeWithRetry(client => client.Channel.GetLastMeas(gidsInTable));
                    break;
                }
                catch
                {
                    ConnectToSmartCache();
                }
            }

            return retVal;
        }

        public void Connect(ServiceInfo serviceInfo)
        {
            throw new NotImplementedException();
        }

        public void NewDeltaApplied()
        {
            throw new NotImplementedException();
        }

        public void SendMeasurements(List<DynamicMeasurement> measurements)
        {
            List<IModelForDuplex> clientsForDeleting = new List<IModelForDuplex>();
            //ServiceEventSource.Current.ServiceMessage(this.Context, "SendMeasurement(SCProxy) on " + this.Context.NodeContext.NodeName.ToString() + ", no. of clients = " + Clients.Count);

            lock (lockObjectForClients)
            {
                foreach (IModelForDuplex client in clients)
                {
                    try
                    {
                        client.SendMeasurements(measurements);
                    }
                    catch
                    {
                        clientsForDeleting.Add(client);
                    }
                }

                foreach (IModelForDuplex client in clientsForDeleting)
                {
                    clients.Remove(client);
                }
            }
            //ServiceEventSource.Current.ServiceMessage(this.Context, "SendMeasurement(SCProxy) done on " + this.Context.NodeContext.NodeName.ToString() + ", no. of clients = " + Clients.Count);
        }

        public void SendAlarm(DeltaForAlarm delta)
        {
            List<IModelForDuplex> clientsForDeleting = new List<IModelForDuplex>();
            //ServiceEventSource.Current.ServiceMessage(this.Context, "SendAlarm(SCProxy) on " + this.Context.NodeContext.NodeName.ToString() + ", no. of clients = " + Clients.Count);

            lock (lockObjectForClients)
            {
                foreach (IModelForDuplex client in clients)
                {
                    try
                    {
                        client.SendAlarm(delta);
                    }
                    catch
                    {
                        clientsForDeleting.Add(client);
                    }
                }

                foreach (IModelForDuplex client in clientsForDeleting)
                {
                    clients.Remove(client);
                }
            }
            //ServiceEventSource.Current.ServiceMessage(this.Context, "SendAlarm(SCProxy) done on " + this.Context.NodeContext.NodeName.ToString() + ", no. of clients = " + Clients.Count);
        }

        public bool PingClient()
        {
            return true;
        }

        public void Ping()
        {
            new Thread(() => PingService()).Start();
        }

        private void PingService()
        {
            //ServiceEventSource.Current.ServiceMessage(this.Context, "PingService(SCProxy) on " + this.Context.NodeContext.NodeName.ToString());

            while (true)
            {
                try
                {
                    proxy.InvokeWithRetry(client => client.Channel.Ping());
                    break;
                }
                catch
                {
                    ConnectToSmartCache();
                }
            }
            //ServiceEventSource.Current.ServiceMessage(this.Context, "PingService(SCProxy) done on " + this.Context.NodeContext.NodeName.ToString());
        }
    }
}
