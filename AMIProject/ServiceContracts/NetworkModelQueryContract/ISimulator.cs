using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
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
    public interface ISimulator
    {
        [OperationContract]
        int AddMeasurement(Measurement m);

        [OperationContract]
        bool AddConsumer(EnergyConsumerForScada ec);

        [OperationContract]
        void Rollback(int decrease, List<long> conGidsForSimulator);

        [OperationContract]
        bool Ping();
    }
}
