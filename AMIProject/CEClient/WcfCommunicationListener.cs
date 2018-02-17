using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace CEClient
{
    internal class WcfCommunicationListener<T> : ICommunicationListener
    {
        private string endpointResourceName;
        private object listenerBinding;
        private StatelessServiceContext serviceContext;
        private CEClient wcfServiceObject;

        public WcfCommunicationListener(CEClient wcfServiceObject, StatelessServiceContext serviceContext, string endpointResourceName, object listenerBinding)
        {
            this.wcfServiceObject = wcfServiceObject;
            this.serviceContext = serviceContext;
            this.endpointResourceName = endpointResourceName;
            this.listenerBinding = listenerBinding;
        }
    }
}