using FTN.Common;
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
    public interface ICalculationEngine
    {
        [OperationContract]
        void DataFromScada(List<DynamicMeasurement> measurements);

        [OperationContract]
        void EnlistDelta(Delta delta);

        [OperationContract]
        void Prepare();

        [OperationContract]
        void Commit();

        [OperationContract]
        void Rollback();
    }
}
