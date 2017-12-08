using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;

namespace FTN.ServiceContracts
{
    [ServiceContract]
    public interface IScadaForDuplex
    {
        [OperationContract]
        bool Prepare(List<ResourceDescription> measurements);
    }
}
