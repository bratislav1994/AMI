using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    [ServiceContract(CallbackContract = typeof(IModelForDuplex))]
    public interface INetworkModelGDAContractDuplexClient : IBaseNetworkModelGDAContract
    {
        [OperationContract(IsOneWay = true)]
        void ConnectClient();
        
        [OperationContract(IsOneWay = true)]
        void Ping();
    }
}
