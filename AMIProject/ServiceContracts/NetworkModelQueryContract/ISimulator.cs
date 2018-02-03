using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
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
        bool AddBaseVoltage(BaseVoltageForScada bv);

        [OperationContract]
        bool AddSubstation(SubstationForScada ss);

        [OperationContract]
        bool AddPowerTransformer(PowerTransformerForScada pt);

        [OperationContract]
        void Rollback(int decrease, List<long> conGidsForSimulator, List<int> analogIndexesForSimulator);

        [OperationContract]
        bool Ping();
    }
}
