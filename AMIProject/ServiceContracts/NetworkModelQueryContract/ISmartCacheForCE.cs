using FTN.Common.ClassesForAlarmDB;
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
    public interface ISmartCacheForCE
    {
        [OperationContract]
        void SendMeasurements(Dictionary<long, DynamicMeasurement> measurements);

        [OperationContract]
        void SendAlarm(DeltaForAlarm delta);
    }
}
