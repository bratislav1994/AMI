using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common
{
    [DataContract]
    public class ServiceInfo
    {
        string traceID;
        string serviceName;
        ServiceType serviceType;

        public ServiceInfo(string traceID, string serviceName, ServiceType serviceType)
        {
            this.traceID = traceID;
            this.serviceName = serviceName;
            this.serviceType = serviceType;
        }

        [DataMember]
        public string TraceID
        {
            get
            {
                return traceID;
            }

            set
            {
                traceID = value;
            }
        }

        [DataMember]
        public string ServiceName
        {
            get
            {
                return serviceName;
            }

            set
            {
                serviceName = value;
            }
        }

        [DataMember]
        public ServiceType ServiceType
        {
            get
            {
                return serviceType;
            }

            set
            {
                serviceType = value;
            }
        }
    }
}
