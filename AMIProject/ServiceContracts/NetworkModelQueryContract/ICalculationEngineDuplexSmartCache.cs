using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    [ServiceContract(CallbackContract = typeof(ISmartCacheForCE))]
    public interface ICalculationEngineDuplexSmartCache
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe();
    }
}
