using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    [ServiceContract]
    public interface ICEScada
    {
        [OperationContract]
        void Connect(ServiceInfo serviceInfo);

        [OperationContract]
        void DataFromScada(Dictionary<long, DynamicMeasurement> measurements);

        [OperationContract]
        void Ping();
    }
}
