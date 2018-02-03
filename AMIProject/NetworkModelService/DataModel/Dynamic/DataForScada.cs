using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.Dynamic
{
    [DataContract]
    [KnownType(typeof(EnergyConsumerForScada))]
    [KnownType(typeof(MeasurementForScada))]
    [KnownType(typeof(PowerTransformerForScada))]
    [KnownType(typeof(BaseVoltageForScada))]
    [KnownType(typeof(SubstationForScada))]
    public class DataForScada
    {
    }
}
