using FTN.Common;
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
    public interface IModelForDuplex
    {
        [OperationContract]
        void NewDeltaApplied();

        [OperationContract]
        bool PingClient();

        [OperationContract]
        void SendMeasurements(List<DynamicMeasurement> measurements);

        [OperationContract]
        void SendAlarm(DeltaForAlarm delta);
    }
}
