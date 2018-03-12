using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using FTN.Common;
using FTN.Common.Filter;
using FTN.Common.ClassesForAlarmDB;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using CommonMS.Access;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using FTN.Services.NetworkModelService.DataModel;
using System.ServiceModel.Channels;

namespace CEClient
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CEClient : StatelessService, ICalculationForClient
    {
        private List<ResolvedAlarm> listAfterRefresh;

        public CEClient(StatelessServiceContext context)
            : base(context)
        {
            var bv = DB.Instance.ReadBaseVoltages();
            using (var access = new AccessTSDB())
            {
                var ret = access.AggregationForHours.FirstOrDefault();
            }
        }
        
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            Binding listenerBinding = WcfUtility.CreateTcpClientBinding();
            listenerBinding.ReceiveTimeout = TimeSpan.MaxValue;

            var serviceListener = new ServiceInstanceListener((context) =>
        new WcfCommunicationListener<ICalculationForClient>(
            wcfServiceObject: this,
            serviceContext: context,
            //
            // The name of the endpoint configured in the ServiceManifest under the Endpoints section
            // that identifies the endpoint that the WCF ServiceHost should listen on.
            //
            endpointResourceName: "ServiceEndpoint",

            //
            // Populate the binding information that you want the service to use.
            //
            listenerBinding: listenerBinding
        ),
        "CEClientListener"
    );

            return new ServiceInstanceListener[] { serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.
            listAfterRefresh = new List<ResolvedAlarm>();
            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public void ConnectClient()
        {
            return;
        }

        public Tuple<List<Statistics>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, ResolutionType resolution)
        {
            List<Statistics> result = new List<Statistics>();

            switch (resolution)
            {
                case ResolutionType.MINUTE:
                    result = this.ReadMinuteAggregationTable(gids, from);
                    break;
                case ResolutionType.HOUR:
                    result = this.ReadHourAggregationTable(gids, from);
                    break;
                case ResolutionType.DAY:
                    result = this.ReadDayAggregationTable(gids, from);
                    break;
            }

            if (result.Count == 0)
            {
                return null;
            }
            Statistics statistics = FillStatistic(result);

            return new Tuple<List<Statistics>, Statistics>(result, statistics);
        }

        public Tuple<List<HourAggregation>, Statistics> GetMeasurementsForChartViewByFilter(List<long> gids, Filter filter)
        {
            Dictionary<long, BaseVoltageDb> baseVoltages = new Dictionary<long, BaseVoltageDb>();
            Dictionary<long, EnergyConsumerDb> amis = new Dictionary<long, EnergyConsumerDb>();
            baseVoltages = DB.Instance.ReadBaseVoltages();
            amis = DB.Instance.ReadConsumersByID(gids);
            List<HourAggregation> result = this.ReadHourAggregationTableByFilter(gids, filter);

            if (result.Count == 0)
            {
                return null;
            }

            Dictionary<int, List<HourAggregation>> hourAggByResolution = new Dictionary<int, List<HourAggregation>>(24);

            foreach (HourAggregation hourAgg in result)
            {
                hourAgg.AvgV = hourAgg.AvgV / baseVoltages[amis[hourAgg.PsrRef].BaseVoltageID].NominalVoltage;
                hourAgg.MaxV = hourAgg.MaxV / baseVoltages[amis[hourAgg.PsrRef].BaseVoltageID].NominalVoltage;
                hourAgg.MinV = hourAgg.MinV / baseVoltages[amis[hourAgg.PsrRef].BaseVoltageID].NominalVoltage;

                if (!hourAggByResolution.ContainsKey(hourAgg.TimeStamp.Hour))
                {
                    hourAggByResolution.Add(hourAgg.TimeStamp.Hour, new List<HourAggregation>());
                }

                hourAggByResolution[hourAgg.TimeStamp.Hour].Add(hourAgg);
            }

            List<HourAggregation> retVal = new List<HourAggregation>();

            foreach (KeyValuePair<int, List<HourAggregation>> kvp in hourAggByResolution)
            {
                HourAggregation hourAggregation = new HourAggregation();

                foreach (HourAggregation hourAgg in kvp.Value)
                {
                    hourAggregation.AvgP += hourAgg.AvgP;
                    hourAggregation.AvgQ += hourAgg.AvgQ;
                    hourAggregation.AvgV += hourAgg.AvgV;
                    hourAggregation.IntegralP += hourAgg.IntegralP;
                    hourAggregation.IntegralQ += hourAgg.IntegralQ;
                }

                hourAggregation.MaxP = kvp.Value.Max(x => x.MaxP);
                hourAggregation.MaxQ = kvp.Value.Max(x => x.MaxQ);
                hourAggregation.MinP = kvp.Value.Min(x => x.MinP);
                hourAggregation.MinQ = kvp.Value.Min(x => x.MinQ);
                hourAggregation.MaxV = kvp.Value.Max(x => x.MaxV);
                hourAggregation.MinV = kvp.Value.Min(x => x.MinV);

                hourAggregation.AvgP /= kvp.Value.Count;
                hourAggregation.AvgQ /= kvp.Value.Count;
                hourAggregation.AvgV /= kvp.Value.Count;
                hourAggregation.IntegralP /= kvp.Value.Count;
                hourAggregation.IntegralQ /= kvp.Value.Count;
                hourAggregation.TimeStamp = kvp.Value[0].TimeStamp;

                retVal.Add(hourAggregation);
            }

            Statistics s = FillHourAgg(retVal);

            return new Tuple<List<HourAggregation>, Statistics>(retVal, s);
        }

        public List<ResolvedAlarm> GetResolvedAlarms(int startIndex, int range)
        {
            if (startIndex + range > listAfterRefresh.Count - 1)
            {
                return listAfterRefresh.GetRange(startIndex, listAfterRefresh.Count - startIndex);
            }
            else
            {
                return listAfterRefresh.GetRange(startIndex, range);
            }
        }

        public int GetTotalPageCount()
        {
            listAfterRefresh.Clear();
            listAfterRefresh = DB.Instance.ReadResolvedAlarm();

            return listAfterRefresh.Count();
        }

        private List<Statistics> ReadMinuteAggregationTable(List<long> gids, DateTime from)
        {
            Dictionary<long, BaseVoltageDb> baseVoltages = new Dictionary<long, BaseVoltageDb>();
            Dictionary<long, EnergyConsumerDb> amis = new Dictionary<long, EnergyConsumerDb>();
            baseVoltages = DB.Instance.ReadBaseVoltages();
            amis = DB.Instance.ReadConsumersByID(gids);
            Dictionary<DateTime, Statistics> measurements = new Dictionary<DateTime, Statistics>();
            DateTime to = from.AddHours(1);

            using (var access = new AccessTSDB())
            {
                var rawMeas = access.AggregationForMinutes.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp < to).ToList();
                Dictionary<DateTime, int> cntForVoltage = new Dictionary<DateTime, int>();

                foreach (var meas in rawMeas)
                {
                    meas.AvgV = meas.AvgV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                    meas.MaxV = meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                    meas.MinV = meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;

                    if (!measurements.ContainsKey(meas.TimeStamp))
                    {
                        measurements.Add(meas.TimeStamp, meas);
                        cntForVoltage.Add(meas.TimeStamp, 1);
                    }
                    else
                    {
                        if (meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage > measurements[meas.TimeStamp].MaxV)
                        {
                            measurements[meas.TimeStamp].MaxV = meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                        }

                        if (meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage < measurements[meas.TimeStamp].MinV)
                        {
                            measurements[meas.TimeStamp].MinV = meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                        }

                        measurements[meas.TimeStamp].AvgP += meas.AvgP;
                        measurements[meas.TimeStamp].AvgQ += meas.AvgQ;
                        measurements[meas.TimeStamp].AvgV += meas.AvgV;
                        ++cntForVoltage[meas.TimeStamp];
                        measurements[meas.TimeStamp].IntegralP += meas.IntegralP;
                        measurements[meas.TimeStamp].IntegralQ += meas.IntegralQ;
                    }
                }

                foreach (KeyValuePair<DateTime, int> kvp in cntForVoltage)
                {
                    measurements[kvp.Key].AvgV /= kvp.Value;

                }

                return measurements.Values.ToList();
            }
        }

        private List<Statistics> ReadHourAggregationTable(List<long> gids, DateTime from)
        {
            Dictionary<long, BaseVoltageDb> baseVoltages = new Dictionary<long, BaseVoltageDb>();
            Dictionary<long, EnergyConsumerDb> amis = new Dictionary<long, EnergyConsumerDb>();
            baseVoltages = DB.Instance.ReadBaseVoltages();
            amis = DB.Instance.ReadConsumersByID(gids);
            Dictionary<DateTime, Statistics> measurements = new Dictionary<DateTime, Statistics>();
            DateTime to = from.AddDays(1);

            using (var access = new AccessTSDB())
            {
                var rawMeas = access.AggregationForHours.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp < to).ToList();
                Dictionary<DateTime, int> cntForVoltage = new Dictionary<DateTime, int>();

                foreach (var meas in rawMeas)
                {
                    meas.AvgV = meas.AvgV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                    meas.MaxV = meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                    meas.MinV = meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;

                    if (!measurements.ContainsKey(meas.TimeStamp))
                    {
                        measurements.Add(meas.TimeStamp, meas);
                        cntForVoltage.Add(meas.TimeStamp, 1);
                    }
                    else
                    {
                        if (meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage > measurements[meas.TimeStamp].MaxV)
                        {
                            measurements[meas.TimeStamp].MaxV = meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                        }

                        if (meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage < measurements[meas.TimeStamp].MinV)
                        {
                            measurements[meas.TimeStamp].MinV = meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                        }

                        measurements[meas.TimeStamp].AvgP += meas.AvgP;
                        measurements[meas.TimeStamp].AvgQ += meas.AvgQ;
                        measurements[meas.TimeStamp].AvgV += meas.AvgV;
                        ++cntForVoltage[meas.TimeStamp];
                        measurements[meas.TimeStamp].IntegralP += meas.IntegralP;
                        measurements[meas.TimeStamp].IntegralQ += meas.IntegralQ;
                    }
                }

                foreach (KeyValuePair<DateTime, int> kvp in cntForVoltage)
                {
                    measurements[kvp.Key].AvgV /= kvp.Value;
                }

                return measurements.Values.ToList();
            }
        }

        private List<Statistics> ReadDayAggregationTable(List<long> gids, DateTime from)
        {
            Dictionary<long, BaseVoltageDb> baseVoltages = new Dictionary<long, BaseVoltageDb>();
            Dictionary<long, EnergyConsumerDb> amis = new Dictionary<long, EnergyConsumerDb>();
            baseVoltages = DB.Instance.ReadBaseVoltages();
            amis = DB.Instance.ReadConsumersByID(gids);
            Dictionary<DateTime, Statistics> measurements = new Dictionary<DateTime, Statistics>();
            DateTime to = from.AddMonths(1);

            using (var access = new AccessTSDB())
            {
                var rawMeas = access.AggregationForDays.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp < to).ToList();
                Dictionary<DateTime, int> cntForVoltage = new Dictionary<DateTime, int>();

                foreach (var meas in rawMeas)
                {
                    meas.AvgV = meas.AvgV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                    meas.MaxV = meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                    meas.MinV = meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;

                    if (!measurements.ContainsKey(meas.TimeStamp))
                    {
                        measurements.Add(meas.TimeStamp, meas);
                        cntForVoltage.Add(meas.TimeStamp, 1);
                    }
                    else
                    {
                        if (meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage > measurements[meas.TimeStamp].MaxV)
                        {
                            measurements[meas.TimeStamp].MaxV = meas.MaxV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                        }

                        if (meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage < measurements[meas.TimeStamp].MinV)
                        {
                            measurements[meas.TimeStamp].MinV = meas.MinV / baseVoltages[amis[meas.PsrRef].BaseVoltageID].NominalVoltage;
                        }

                        measurements[meas.TimeStamp].AvgP += meas.AvgP;
                        measurements[meas.TimeStamp].AvgQ += meas.AvgQ;
                        measurements[meas.TimeStamp].AvgV += meas.AvgV;
                        ++cntForVoltage[meas.TimeStamp];
                        measurements[meas.TimeStamp].IntegralP += meas.IntegralP;
                        measurements[meas.TimeStamp].IntegralQ += meas.IntegralQ;
                    }
                }
                
                foreach (KeyValuePair<DateTime, int> kvp in cntForVoltage)
                {
                    measurements[kvp.Key].AvgV /= kvp.Value;
                }

                return measurements.Values.ToList();
            }
        }

        private List<HourAggregation> ReadHourAggregationTableByFilter(List<long> gids, Filter filter)
        {
            List<HourAggregation> ret = new List<HourAggregation>();
            bool freshList = true;

            if (!filter.ConsumerHasValue && !filter.SeasonHasValue &&
                filter.Month == -1 && filter.Day == -1)
            {
                using (var access = new AccessTSDB())
                {
                    ret = access.AggregationForHours.ToList();
                }
            }

            if (filter.SeasonHasValue)
            {
                using (var access = new AccessTSDB())
                {
                    ret = access.AggregationForHours.Where(x => x.Season == filter.Season && gids.Any(y => y == x.PsrRef) &&
                    x.TimeStamp.Year >= filter.YearFrom && x.TimeStamp.Year <= filter.YearTo).ToList();
                    freshList = false;
                }
            }

            if (filter.ConsumerHasValue && ret.Count > 0)
            {
                foreach (HourAggregation hAgg in ret.Reverse<HourAggregation>())
                {
                    if (hAgg.Type != filter.ConsumerType)
                    {
                        ret.Remove(hAgg);
                    }
                }
            }
            else if (filter.ConsumerHasValue)
            {
                using (var access = new AccessTSDB())
                {
                    ret = access.AggregationForHours.Where(x => x.Type == filter.ConsumerType && gids.Any(y => y == x.PsrRef) &&
                    x.TimeStamp.Year >= filter.YearFrom && x.TimeStamp.Year <= filter.YearTo).ToList();
                    freshList = false;
                }
            }

            if (filter.Month != -1 && ret.Count > 0)
            {
                foreach (HourAggregation hAgg in ret.Reverse<HourAggregation>())
                {
                    if (hAgg.TimeStamp.Month != filter.Month)
                    {
                        ret.Remove(hAgg);
                    }
                }
            }
            else if (filter.Month != -1)
            {
                using (var access = new AccessTSDB())
                {
                    ret = access.AggregationForHours.Where(x => gids.Any(y => y == x.PsrRef) &&
                    x.TimeStamp.Month == filter.Month &&
                    x.TimeStamp.Year >= filter.YearFrom && x.TimeStamp.Year <= filter.YearTo).ToList();
                    freshList = false;
                }
            }

            if (filter.Day != -1 && (ret.Count > 0 || !freshList))
            {
                foreach (HourAggregation hAgg in ret.Reverse<HourAggregation>())
                {
                    if (hAgg.TimeStamp.Day != filter.Day)
                    {
                        ret.Remove(hAgg);
                    }
                }
            }
            else if (filter.Day != -1)
            {
                using (var access = new AccessTSDB())
                {
                    ret = access.AggregationForHours.Where(x => gids.Any(y => y == x.PsrRef) &&
                    x.TimeStamp.Day == filter.Day &&
                    x.TimeStamp.Year >= filter.YearFrom && x.TimeStamp.Year <= filter.YearTo).ToList();
                    freshList = false;
                }
            }

            if (filter.TypeOfDayHasValue && ret.Count > 0)
            {
                if (filter.TypeOfDay.Count != 0)
                {
                    foreach (HourAggregation hAgg in ret.Reverse<HourAggregation>())
                    {
                        if (!filter.TypeOfDay.Any(x => x == hAgg.TimeStamp.DayOfWeek) || !gids.Any(x => x == hAgg.PsrRef))
                        {
                            ret.Remove(hAgg);
                        }
                    }
                }
            }

            return ret;
        }

        private static Statistics FillStatistic(List<Statistics> result)
        {
            Statistics statistics = new Statistics();
            statistics.MaxP = result.Max(x => x.AvgP);
            statistics.MaxQ = result.Max(x => x.AvgQ);
            statistics.MaxV = result.Max(x => x.MaxV);
            statistics.MinP = result.Min(x => x.AvgP);
            statistics.MinQ = result.Min(x => x.AvgQ);
            statistics.MinV = result.Min(x => x.MinV);
            statistics.AvgP = result.Average(x => x.AvgP);
            statistics.AvgQ = result.Average(x => x.AvgQ);
            statistics.AvgV = result.Average(x => x.AvgV);
            statistics.IntegralP = 0;
            statistics.IntegralQ = 0;

            for (int i = 0; i < result.Count; i++)
            {
                statistics.IntegralP += result[i].IntegralP;
                statistics.IntegralQ += result[i].IntegralQ;
            }

            return statistics;
        }

        private static Statistics FillHourAgg(List<HourAggregation> result)
        {
            Statistics statistics = new Statistics();
            statistics.MaxP = result.Max(x => x.AvgP);
            statistics.MaxQ = result.Max(x => x.AvgQ);
            statistics.MaxV = result.Max(x => x.MaxV);
            statistics.MinP = result.Min(x => x.AvgP);
            statistics.MinQ = result.Min(x => x.AvgQ);
            statistics.MinV = result.Min(x => x.MinV);
            statistics.AvgP = result.Average(x => x.AvgP);
            statistics.AvgQ = result.Average(x => x.AvgQ);
            statistics.AvgV = result.Average(x => x.AvgV);
            statistics.IntegralP = 0;
            statistics.IntegralQ = 0;

            for (int i = 0; i < result.Count; i++)
            {
                statistics.IntegralP += result[i].IntegralP;
                statistics.IntegralQ += result[i].IntegralQ;
            }

            return statistics;
        }

        public void Ping()
        {
            return;
        }
    }
}
