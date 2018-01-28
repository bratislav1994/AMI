using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Meas;
using TC57CIM.IEC61970.Wires;

namespace FTN.ServiceContracts
{
    [ServiceContract]
    public interface INMSForScript
    {
        [OperationContract]
        Tuple<Dictionary<long, IdentifiedObject>, List<IdentifiedObject>> GetConsumers();
    }
}
