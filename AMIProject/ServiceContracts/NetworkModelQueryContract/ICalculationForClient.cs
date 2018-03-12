using FTN.Common;
using FTN.Common.ClassesForAlarmDB;
using FTN.Common.Filter;
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
    [ServiceContract]
    public interface ICalculationForClient
    {
        [OperationContract]
        void ConnectClient();

        [OperationContract]
        Tuple<List<Statistics>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, ResolutionType resolution);

        [OperationContract]
        int GetTotalPageCount();

        [OperationContract]
        List<ResolvedAlarm> GetResolvedAlarms(int startIndes, int range);

        [OperationContract]
        Tuple<List<HourAggregation>, Statistics> GetMeasurementsForChartViewByFilter(List<long> gids, Filter filter);

        [OperationContract]
        void Ping();
    }
}
