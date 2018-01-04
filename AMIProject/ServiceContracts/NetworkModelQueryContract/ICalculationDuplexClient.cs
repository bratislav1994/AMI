using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    [ServiceContract(CallbackContract = typeof(IModelForDuplex))]
    public interface ICalculationDuplexClient
    {
        [OperationContract]
        void ConnectClient();

        [OperationContract]
        List<DynamicMeasurement> GetMeasurementsForChartView(long gid, DateTime from, DateTime to);
    }
}
