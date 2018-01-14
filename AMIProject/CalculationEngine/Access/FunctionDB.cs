using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace CalculationEngine.Access
{
    public class FunctionDB
    {
        private static object lockObj = new object();
        private System.Threading.Timer timer;

        public FunctionDB()
        { }

        public void DoUndone()
        {
            List<long> numberOfUniqueConsumers = new List<long>();
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var lastMeasCollect = access.History.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                    if (lastMeasCollect != null)
                    {
                        var lastMeasMinutes = access.AggregationForMiuntes.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                        if (lastMeasMinutes != null)
                        {
                            DateTime temp = lastMeasMinutes.TimeStamp.AddMinutes(1);
                            if (DateTime.Compare(temp, lastMeasCollect.TimeStamp) < 0) // postoje merenja u collect tabeli koje treba upisati u minutnu
                            {
                                List<DynamicMeasurement> toBeWritten = access.History.Where(x => DateTime.Compare(temp, x.TimeStamp) > 0 && DateTime.Compare(lastMeasMinutes.TimeStamp, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
                                Dictionary<long, DynamicMeasurement> toBeWrittenDic = new Dictionary<long, DynamicMeasurement>();

                                foreach (DynamicMeasurement dm in toBeWritten)
                                {
                                    DynamicMeasurement value = null;
                                    if (!toBeWrittenDic.TryGetValue(dm.PsrRef, out value))
                                    {
                                        toBeWrittenDic[dm.PsrRef] = dm;
                                    }
                                }

                                numberOfUniqueConsumers = toBeWrittenDic.Keys.ToList();

                                var tempList = access.History.Where(x => DateTime.Compare(temp, x.TimeStamp) < 0).ToList(); // merenja koja treba da se upisu u bazu (minutna tabela)
                                Dictionary<long, List<DynamicMeasurement>> measurementsFromCollect = CreateDictionary(tempList);
                                toBeWritten.AddRange(tempList);
                                List<MinuteAggregation> aggregations = CreateMinuteAggregationsWhenMinuteTableIsNotEmpty(temp, toBeWrittenDic, measurementsFromCollect);

                                foreach(MinuteAggregation ma in aggregations)
                                {
                                    access.AggregationForMiuntes.Add(ma);
                                }

                                access.SaveChanges();
                            }
                            else
                            {
                                List<DynamicMeasurement> toBeWritten = access.History.Where(x => x.TimeStamp.Year == lastMeasCollect.TimeStamp.Year &&
                                x.TimeStamp.Month == lastMeasCollect.TimeStamp.Month &&
                                x.TimeStamp.Day == lastMeasCollect.TimeStamp.Day &&
                                x.TimeStamp.Hour == lastMeasCollect.TimeStamp.Hour &&
                                x.TimeStamp.Minute == lastMeasCollect.TimeStamp.Minute &&
                                x.TimeStamp.Second == lastMeasCollect.TimeStamp.Second).ToList(); //moguc bug u buducnosti!
                                Dictionary<long, DynamicMeasurement> toBeWrittenDic = new Dictionary<long, DynamicMeasurement>();

                                foreach (DynamicMeasurement dm in toBeWritten)
                                {
                                    DynamicMeasurement value = null;
                                    if (!toBeWrittenDic.TryGetValue(dm.PsrRef, out value))
                                    {
                                        toBeWrittenDic[dm.PsrRef] = dm;
                                    }
                                }

                                numberOfUniqueConsumers = toBeWrittenDic.Keys.ToList();
                            }
                        }
                        else
                        {
                            var toBeWritten1 = access.History.ToList();
                            Dictionary<long, List<DynamicMeasurement>> measurementsFromCollect1 = CreateDictionary(toBeWritten1);
                            numberOfUniqueConsumers = measurementsFromCollect1.Keys.ToList();
                            List<MinuteAggregation> aggregations = CreateMinuteAggregationsWhenMinuteTableIsEmpty(measurementsFromCollect1);

                            foreach (MinuteAggregation ma in aggregations)
                            {
                                access.AggregationForMiuntes.Add(ma);
                            }

                            access.SaveChanges();
                        }

                        DateTime dt = DateTime.Now;
                        DateTime fromLastMeasCollect = lastMeasCollect.TimeStamp;

                        while (DateTime.Compare(fromLastMeasCollect, dt) < 0)
                        {
                            for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                            {
                                MinuteAggregation ma = new MinuteAggregation();
                                ma.PsrRef = numberOfUniqueConsumers[i];
                                ma.TimeStamp = RoundUp(fromLastMeasCollect, TimeSpan.FromMinutes(1));
                                access.AggregationForMiuntes.Add(ma);
                            }
                            fromLastMeasCollect = fromLastMeasCollect.AddMinutes(1);
                        }
                        access.SaveChanges();
                    }
                }
            }
        }
        
        public void StartThreads()
        {
            DateTime argument = RoundUp(DateTime.Now, TimeSpan.FromMinutes(1));
            this.SetUpTimer(argument);
        }

        public bool AddMeasurements(List<DynamicMeasurement> measurements)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (DynamicMeasurement m in measurements)
                    {
                        var lastMeas = access.History.Where(x => x.PsrRef == m.PsrRef).OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                        if (lastMeas != null && (m.CurrentP == -1 || m.CurrentQ == -1 || m.CurrentV == -1))
                        {
                            if ( (Math.Abs((double)(m.TimeStamp - lastMeas.TimeStamp).TotalSeconds)) < 1)
                            {
                                if (m.CurrentP != -1)
                                {
                                    lastMeas.CurrentP = m.CurrentP;
                                }

                                if (m.CurrentQ != -1)
                                {
                                    lastMeas.CurrentQ = m.CurrentQ;
                                }

                                if (m.CurrentV != -1)
                                {
                                    lastMeas.CurrentV = m.CurrentV;
                                }

                                access.Entry(lastMeas).State = System.Data.Entity.EntityState.Modified;

                                int i = access.SaveChanges();

                                if (i > 0)
                                {
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                Console.WriteLine(m.TimeStamp);
                                if (m.CurrentP == -1)
                                {
                                    m.CurrentP = lastMeas.CurrentP;
                                }

                                if (m.CurrentQ == -1)
                                {
                                    m.CurrentQ = lastMeas.CurrentQ;
                                }

                                if (m.CurrentV == -1)
                                {
                                    m.CurrentV = lastMeas.CurrentV;
                                }

                                access.History.Add(m);

                                int i = access.SaveChanges();

                                if (i > 0)
                                {
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            access.History.Add(m);

                            int i = access.SaveChanges();

                            if (i > 0)
                            {
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }
        }

        public List<DynamicMeasurement> GetMeasForChart(List<long> gids, DateTime from, DateTime to)
        {
            List<DynamicMeasurement> measurements = new List<DynamicMeasurement>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (var meas in access.History.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp <= to).ToList())
                    {
                        measurements.Add(meas);
                    }

                    return measurements;
                }
            }
        }

        /*public bool AddGeoRegions(List<GeographicalRegion> geoRegions)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (GeographicalRegion gr in geoRegions)
                    {
                        access.GeoRegions.Add(gr);
                    }

                    int i = access.SaveChanges();

                    if (i == 0)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        public bool AddSubGeoRegions(List<SubGeographicalRegion> subGeoRegions)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (SubGeographicalRegion sgr in subGeoRegions)
                    {
                        access.SubGeoRegions.Add(sgr);
                    }

                    int i = access.SaveChanges();

                    if (i == 0)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        public bool AddSubstations(List<Substation> substations)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (Substation ss in substations)
                    {
                        access.Substations.Add(ss);
                    }

                    int i = access.SaveChanges();

                    if (i == 0)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        public bool AddConsumers(List<EnergyConsumer> consumers)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (EnergyConsumer ec in consumers)
                    {
                        access.Consumers.Add(ec);
                    }

                    int i = access.SaveChanges();

                    if (i == 0)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        public Dictionary<long, EnergyConsumer> ReadConsumers()
        {
            Dictionary<long, EnergyConsumer> retVal = new Dictionary<long, EnergyConsumer>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var consumers = access.Consumers.ToList();

                    foreach (EnergyConsumer ec in consumers)
                    {
                        retVal.Add(ec.GlobalId, ec);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, Substation> ReadSubstations()
        {
            Dictionary<long, Substation> retVal = new Dictionary<long, Substation>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var substations = access.Substations.ToList();

                    foreach (Substation ss in substations)
                    {
                        retVal.Add(ss.GlobalId, ss);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, SubGeographicalRegion> ReadSubGeoRegions()
        {
            Dictionary<long, SubGeographicalRegion> retVal = new Dictionary<long, SubGeographicalRegion>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var subGeoRegions = access.SubGeoRegions.ToList();

                    foreach (SubGeographicalRegion sgr in subGeoRegions)
                    {
                        retVal.Add(sgr.GlobalId, sgr);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, GeographicalRegion> ReadGeoRegions()
        {
            Dictionary<long, GeographicalRegion> retVal = new Dictionary<long, GeographicalRegion>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var geoRegions = access.GeoRegions.ToList();

                    foreach (GeographicalRegion gr in geoRegions)
                    {
                        retVal.Add(gr.GlobalId, gr);
                    }
                }

                return retVal;
            }
        }*/
        #region private methods

        private void SetUpTimer(DateTime argument)
        {
            DateTime current = DateTime.Now;
            DateTime alertTime = argument.AddMilliseconds(-10);
            TimeSpan timeToGo = alertTime.TimeOfDay - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                return;//time already passed
            }
            this.timer = new System.Threading.Timer(x =>
            {
                this.AggregateForMinute(argument);
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private void AggregateForMinute(DateTime to)
        {
            DateTime from = to.AddMinutes(-1);
            DateTime argForSetUpTimer = to.AddMinutes(1);

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var measurements = access.History.Where(x => DateTime.Compare(x.TimeStamp, to) < 0 && DateTime.Compare(x.TimeStamp, from) > 0).ToList();
                    var minuteAggregation = access.AggregationForMiuntes.FirstOrDefault();

                    if (minuteAggregation == null)
                    {
                        List<MinuteAggregation> aggregations = CreateMinuteAggregationsWhenMinuteTableIsEmpty(CreateDictionary(measurements));

                        foreach (MinuteAggregation ma in aggregations)
                        {
                            access.AggregationForMiuntes.Add(ma);
                        }

                        access.SaveChanges();
                    }
                    else
                    {
                        DateTime beforeFrom = from.AddMinutes(-1);
                        List<DynamicMeasurement> previousMeasurements = access.History.Where(x => DateTime.Compare(from, x.TimeStamp) > 0 && DateTime.Compare(beforeFrom, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
                        Dictionary<long, DynamicMeasurement> previousMeasurementsDic = new Dictionary<long, DynamicMeasurement>();

                        foreach (DynamicMeasurement dm in previousMeasurements)
                        {
                            DynamicMeasurement value = null;
                            if (!previousMeasurementsDic.TryGetValue(dm.PsrRef, out value))
                            {
                                previousMeasurementsDic[dm.PsrRef] = dm;
                            }
                        }

                        List<MinuteAggregation> aggregations = CreateMinuteAggregationsWhenMinuteTableIsNotEmpty(from, previousMeasurementsDic, CreateDictionary(measurements));

                        foreach (MinuteAggregation ma in aggregations)
                        {
                            access.AggregationForMiuntes.Add(ma);
                        }
                        access.SaveChanges();
                    }
                }
            }

            SetUpTimer(argForSetUpTimer);
        }

        private List<MinuteAggregation> CreateMinuteAggregationsWhenMinuteTableIsNotEmpty(DateTime temp, Dictionary<long, DynamicMeasurement> toBeWrittenDic, Dictionary<long, List<DynamicMeasurement>> measurementsFromCollect)
        {
            List<MinuteAggregation> retVal = new List<MinuteAggregation>();
            foreach (KeyValuePair<long, List<DynamicMeasurement>> kvp in measurementsFromCollect)
            {
                MinuteAggregation ma = new MinuteAggregation();
                ma.PsrRef = kvp.Key;
                ma.MinP = kvp.Value.Min(x => x.CurrentP);
                ma.MinQ = kvp.Value.Min(x => x.CurrentQ);
                ma.MinV = kvp.Value.Min(x => x.CurrentV);
                ma.MaxP = kvp.Value.Max(x => x.CurrentP);
                ma.MaxQ = kvp.Value.Max(x => x.CurrentQ);
                ma.MaxV = kvp.Value.Max(x => x.CurrentV);
                ma.AvgP = kvp.Value.Average(x => x.CurrentP);
                ma.AvgQ = kvp.Value.Average(x => x.CurrentQ);
                ma.AvgV = kvp.Value.Average(x => x.CurrentV);
                ma.TimeStamp = temp;

                ma.IntegralP += (toBeWrittenDic[kvp.Key].CurrentP * (((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[0].CurrentP - toBeWrittenDic[kvp.Key].CurrentP))) / 2;
                ma.IntegralQ += (toBeWrittenDic[kvp.Key].CurrentQ * (((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[0].CurrentQ - toBeWrittenDic[kvp.Key].CurrentQ))) / 2;
                ma.IntegralV += (toBeWrittenDic[kvp.Key].CurrentV * (((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[0].CurrentV - toBeWrittenDic[kvp.Key].CurrentV))) / 2;

                for (int i = 1; i < kvp.Value.Count; i++)
                {
                    ma.IntegralP += (kvp.Value[i].CurrentP * (((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[i].CurrentP - kvp.Value[i - 1].CurrentP))) / 2;
                    ma.IntegralQ += (kvp.Value[i].CurrentQ * (((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[i].CurrentQ - kvp.Value[i - 1].CurrentQ))) / 2;
                    ma.IntegralV += (kvp.Value[i].CurrentV * (((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[i].CurrentV - kvp.Value[i - 1].CurrentV))) / 2;
                }

                retVal.Add(ma);
            }

            return retVal;
        }

        private List<MinuteAggregation> CreateMinuteAggregationsWhenMinuteTableIsEmpty(Dictionary<long, List<DynamicMeasurement>> measurementsFromCollect1)
        {
            List<MinuteAggregation> retVal = new List<MinuteAggregation>();
            foreach (KeyValuePair<long, List<DynamicMeasurement>> kvp in measurementsFromCollect1)
            {
                MinuteAggregation ma = new MinuteAggregation();
                ma.PsrRef = kvp.Key;
                ma.MinP = kvp.Value.Min(x => x.CurrentP);
                ma.MinQ = kvp.Value.Min(x => x.CurrentQ);
                ma.MinV = kvp.Value.Min(x => x.CurrentV);
                ma.MaxP = kvp.Value.Max(x => x.CurrentP);
                ma.MaxQ = kvp.Value.Max(x => x.CurrentQ);
                ma.MaxV = kvp.Value.Max(x => x.CurrentV);
                ma.AvgP = kvp.Value.Average(x => x.CurrentP);
                ma.AvgQ = kvp.Value.Average(x => x.CurrentQ);
                ma.AvgV = kvp.Value.Average(x => x.CurrentV);
                ma.TimeStamp = RoundDown(kvp.Value[0].TimeStamp, TimeSpan.FromMinutes(1));

                for (int i = 1; i < kvp.Value.Count; i++)
                {
                    ma.IntegralP += (kvp.Value[i].CurrentP * (((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[i].CurrentP - kvp.Value[i - 1].CurrentP))) / 2;
                    ma.IntegralQ += (kvp.Value[i].CurrentQ * (((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[i].CurrentQ - kvp.Value[i - 1].CurrentQ))) / 2;
                    ma.IntegralV += (kvp.Value[i].CurrentV * (((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[i].TimeStamp - kvp.Value[i - 1].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[i].CurrentV - kvp.Value[i - 1].CurrentV))) / 2;
                }

                retVal.Add(ma);
            }

            return retVal;
        }

        private Dictionary<long, List<DynamicMeasurement>> CreateDictionary(List<DynamicMeasurement> toBeWritten1)
        {
            Dictionary<long, List<DynamicMeasurement>> retVal = new Dictionary<long, List<DynamicMeasurement>>();

            foreach (DynamicMeasurement dm in toBeWritten1)
            {
                List<DynamicMeasurement> value = null;

                if (!retVal.TryGetValue(dm.PsrRef, out value))
                {
                    retVal[dm.PsrRef] = new List<DynamicMeasurement>();
                }

                retVal[dm.PsrRef].Add(dm);
            }

            return retVal;
        }

        DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }

        private DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks / d.Ticks) * d.Ticks);
        }
        #endregion private methods
    }
}