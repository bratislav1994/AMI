using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using FTN.ServiceContracts;
using FTN.Common;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using CommonMS.NMSAccess;

namespace NMSDB
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class NMSDB : StatelessService, IDatabaseForNMS
    {
        private FunctionDB dataBaseAdapter = new FunctionDB();

        public NMSDB(StatelessServiceContext context)
            : base(context)
        { }
        
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var serviceListener = new ServiceInstanceListener((context) =>
                                    new WcfCommunicationListener<IDatabaseForNMS>
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

            return new ServiceInstanceListener[] { serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            //long iterations = 0;

            //while (true)
            //{
            //    cancellationToken.ThrowIfCancellationRequested();

            //    ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

            //    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            //}
        }

        public void Ping()
        {
            return;
        }

        public List<Delta> ReadDelta()
        {
            return dataBaseAdapter.ReadDelta();
        }

        public Task<bool> SaveDelta(Delta delta)
        {
            return Task.FromResult<bool>(dataBaseAdapter.AddDelta(delta));
        }
    }
}
