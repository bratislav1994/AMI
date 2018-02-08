using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    [ServiceContract(CallbackContract = typeof(IModelForDuplex))]
    public interface ISmartCacheDuplexForClient
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe();

        [OperationContract]
        List<DynamicMeasurement> GetLastMeas(List<long> gidsInTable);
    }
}
