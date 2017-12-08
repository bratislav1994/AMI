using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;

namespace SCADA
{
    [ServiceContract]
    public interface IScadaForDuplex
    {
        bool AddMeasurements(List<Measurement> measurements);
    }
}
