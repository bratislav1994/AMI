using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
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
        Tuple<List<Statistics>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, int resolution);
    }
}
