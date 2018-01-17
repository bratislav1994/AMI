﻿using CalculationEngine.Class;
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
        private static object lockObjH = new object();
        private static object lockObjM = new object();
        private static object lockObjD = new object();
        private Timer timer;
        private Timer timerHours;
        private Timer timerDays;
        private bool isMinuteDone = false;
        private bool isHourDone = false;
        private bool isDayDone = false;
        private DateTime lastMeasForHoursBeforeAppStart;
        private DateTime lastMeasForMinutesBeforeAppStart;

        public FunctionDB()
        {

        }

        public void DoUndone()
        {
            this.DoUndoneByMinute();
            this.DoUndoneByHour();
            this.DoUndoneByDay();
        }

        #region hour

        public void DoUndoneByHour()
        {
            List<long> numberOfUniqueConsumers = new List<long>();

            lock (lockObjM)
            {
                using (var access = new AccessDB())
                {
                    var lastMeasMinute = access.AggregationForMinutes.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                    if (lastMeasMinute != null) // Ako postoje merenja u minutnoj tabeli
                    {
                        lock (lockObjH)
                        {
                            var lastMeasHours = access.AggregationForHours.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                            if (lastMeasHours != null) // Ako satna tabela nije prazna
                            {
                                this.lastMeasForHoursBeforeAppStart = lastMeasHours.TimeStamp; // Poslednje merenje pre nego sto se upisu potencijalne nule, kako bi se za dnevnu tabelu sacuvala informacija
                                DateTime temp = lastMeasHours.TimeStamp.AddHours(1);

                                if (DateTime.Compare(temp, lastMeasMinute.TimeStamp) < 0) // postoje merenja u minutnoj tabeli koje treba upisati u satnu
                                {
                                    List<MinuteAggregation> toBeWritten = access.AggregationForMinutes.Where(x => DateTime.Compare(temp, x.TimeStamp) > 0 && DateTime.Compare(lastMeasHours.TimeStamp, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
                                    Dictionary<long, MinuteAggregation> toBeWrittenDic = new Dictionary<long, MinuteAggregation>();

                                    foreach (MinuteAggregation dm in toBeWritten) // poslednja merenja koja su vec obradjena da bi se racunao integral, moguce da nece trebati
                                    {
                                        MinuteAggregation value = null;

                                        if (!toBeWrittenDic.TryGetValue(dm.PsrRef, out value))
                                        {
                                            toBeWrittenDic[dm.PsrRef] = dm;
                                        }
                                    }

                                    numberOfUniqueConsumers = toBeWrittenDic.Keys.ToList();
                                    DateTime now = RoundDown(DateTime.Now, TimeSpan.FromHours(1));
                                    DateTime roundDownLastMinute = RoundDown(lastMeasForMinutesBeforeAppStart, TimeSpan.FromHours(1));
                                    List<MinuteAggregation> tempList = new List<MinuteAggregation>();

                                    if (DateTime.Compare(now, roundDownLastMinute) != 0) // Ukoliko je app pokrenuta u razlicitom satu u odnosu kad je ugasena, samo tada treba da se obradi sat za koji postoje minutna merenja, u suprotnom nista
                                    {
                                        DateTime roundDownLastMinutePlusOneHour = roundDownLastMinute.AddHours(1);
                                        tempList = access.AggregationForMinutes.Where(x => DateTime.Compare(roundDownLastMinute, x.TimeStamp) < 0 && DateTime.Compare(roundDownLastMinutePlusOneHour, x.TimeStamp) > 0).ToList(); // merenja koja treba da se upisu u bazu (minutna tabela)
                                        Dictionary<long, List<MinuteAggregation>> measurementsFromCollect = CreateDictionaryForHour(tempList);
                                        toBeWritten.AddRange(tempList);
                                        List<HourAggregation> aggregations = CreateHourAggregationsWhenHourTableIsNotEmpty(temp, toBeWrittenDic, measurementsFromCollect);

                                        foreach (HourAggregation ma in aggregations)
                                        {
                                            access.AggregationForHours.Add(ma);
                                        }
                                    }

                                    while (DateTime.Compare(roundDownLastMinute, now.AddHours(-1)) < 0) // Popunjavanje tabele sa nulama za sate u kojima se nije merilo nista
                                    {
                                        for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                        {
                                            HourAggregation ma = new HourAggregation();
                                            ma.PsrRef = numberOfUniqueConsumers[i];
                                            ma.TimeStamp = roundDownLastMinute.AddHours(1);
                                            access.AggregationForHours.Add(ma);
                                        }

                                        roundDownLastMinute = roundDownLastMinute.AddHours(1);
                                    }

                                    access.SaveChanges();
                                }
                                else // Ako ne postoje nova merenja, tabele su azurne sto se tice podataka iz minutne tabele, samo treba videti da li je potrebno upisati nule za preskocene sate
                                {
                                    List<MinuteAggregation> toBeWritten = access.AggregationForMinutes.Where(x => x.TimeStamp.Year == lastMeasMinute.TimeStamp.Year &&
                                    x.TimeStamp.Month == lastMeasMinute.TimeStamp.Month &&
                                    x.TimeStamp.Day == lastMeasMinute.TimeStamp.Day &&
                                    x.TimeStamp.Hour == lastMeasMinute.TimeStamp.Hour &&
                                    x.TimeStamp.Minute == lastMeasMinute.TimeStamp.Minute).ToList(); //moguc bug u buducnosti!
                                    Dictionary<long, MinuteAggregation> toBeWrittenDic = new Dictionary<long, MinuteAggregation>();

                                    foreach (MinuteAggregation dm in toBeWritten)
                                    {
                                        MinuteAggregation value = null;

                                        if (!toBeWrittenDic.TryGetValue(dm.PsrRef, out value))
                                        {
                                            toBeWrittenDic[dm.PsrRef] = dm;
                                        }
                                    }

                                    numberOfUniqueConsumers = toBeWrittenDic.Keys.ToList();

                                    DateTime dt = this.RoundDown(DateTime.Now, TimeSpan.FromHours(1));
                                    DateTime fromLastMeasCollect = this.RoundUp(lastMeasForMinutesBeforeAppStart, TimeSpan.FromHours(1));

                                    while (DateTime.Compare(fromLastMeasCollect, dt.AddHours(-1)) < 0)
                                    {
                                        for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                        {
                                            HourAggregation ma = new HourAggregation();
                                            ma.PsrRef = numberOfUniqueConsumers[i];
                                            ma.TimeStamp = fromLastMeasCollect.AddHours(1);
                                            access.AggregationForHours.Add(ma);
                                        }

                                        fromLastMeasCollect = fromLastMeasCollect.AddHours(1);
                                    }

                                    access.SaveChanges();
                                }
                            }
                            else // Ako je satna tabela prazna
                            {
                                DateTime now = RoundDown(DateTime.Now, TimeSpan.FromHours(1));
                                DateTime roundDownLastMinute = RoundDown(lastMeasForMinutesBeforeAppStart, TimeSpan.FromHours(1));
                                Dictionary<long, List<MinuteAggregation>> measurementsFromMinute1 = new Dictionary<long, List<MinuteAggregation>>();
                                List<MinuteAggregation> toBeWritten1 = new List<MinuteAggregation>();

                                if (DateTime.Compare(now, roundDownLastMinute) != 0) // app se pokrenula istog sata kad imamo i poslednje merenje, ne radi nista, u suprotnom aggregiraj merenja koja su ostala u minutnoj tabeli
                                {
                                    DateTime roundDownLastMinutePlusOneHour = roundDownLastMinute.AddHours(1); // pravi se interval za merenja iz minutne tabele
                                    toBeWritten1 = access.AggregationForMinutes.Where(x => DateTime.Compare(roundDownLastMinute, x.TimeStamp) < 0 && DateTime.Compare(x.TimeStamp, roundDownLastMinutePlusOneHour) < 0).ToList();
                                    measurementsFromMinute1 = CreateDictionaryForHour(toBeWritten1);

                                    numberOfUniqueConsumers = measurementsFromMinute1.Keys.ToList();
                                    List<HourAggregation> aggregations = CreateHourAggregationsWhenHourTableIsEmpty(measurementsFromMinute1);

                                    foreach (HourAggregation ma in aggregations)
                                    {
                                        access.AggregationForHours.Add(ma);
                                    }
                                }

                                while (DateTime.Compare(roundDownLastMinute, now.AddHours(-1)) < 0)
                                {
                                    for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                    {
                                        HourAggregation ma = new HourAggregation();
                                        ma.PsrRef = numberOfUniqueConsumers[i];
                                        ma.TimeStamp = roundDownLastMinute.AddHours(1);
                                        access.AggregationForHours.Add(ma);
                                    }

                                    roundDownLastMinute = roundDownLastMinute.AddHours(1);
                                }

                                access.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<long, List<MinuteAggregation>> CreateDictionaryForHour(List<MinuteAggregation> toBeWritten1)
        {
            Dictionary<long, List<MinuteAggregation>> retVal = new Dictionary<long, List<MinuteAggregation>>();

            foreach (MinuteAggregation dm in toBeWritten1)
            {
                List<MinuteAggregation> value = null;

                if (!retVal.TryGetValue(dm.PsrRef, out value))
                {
                    retVal[dm.PsrRef] = new List<MinuteAggregation>();
                }

                retVal[dm.PsrRef].Add(dm);
            }

            return retVal;
        }

        private List<HourAggregation> CreateHourAggregationsWhenHourTableIsNotEmpty(DateTime temp, Dictionary<long, MinuteAggregation> toBeWrittenDic, Dictionary<long, List<MinuteAggregation>> measurementsFromMinute)
        {
            List<HourAggregation> retVal = new List<HourAggregation>();

            foreach (KeyValuePair<long, List<MinuteAggregation>> kvp in measurementsFromMinute)
            {
                HourAggregation ma = new HourAggregation();
                ma.PsrRef = kvp.Key;
                ma.MinP = kvp.Value.Min(x => x.MinP);
                ma.MinQ = kvp.Value.Min(x => x.MinQ);
                ma.MinV = kvp.Value.Min(x => x.MinV);
                ma.MaxP = kvp.Value.Max(x => x.MaxP);
                ma.MaxQ = kvp.Value.Max(x => x.MaxQ);
                ma.MaxV = kvp.Value.Max(x => x.MaxV);
                ma.AvgP = kvp.Value.Average(x => x.AvgP);
                ma.AvgQ = kvp.Value.Average(x => x.AvgQ);
                ma.AvgV = kvp.Value.Average(x => x.AvgV);
                ma.TimeStamp = temp;

                ma.IntegralP += toBeWrittenDic[kvp.Key].IntegralP;
                ma.IntegralQ += toBeWrittenDic[kvp.Key].IntegralQ;
                ma.IntegralV += toBeWrittenDic[kvp.Key].IntegralV;

                for (int i = 1; i < kvp.Value.Count; i++)
                {
                    ma.IntegralP += kvp.Value[i].IntegralP;
                    ma.IntegralQ += kvp.Value[i].IntegralQ;
                    ma.IntegralV += kvp.Value[i].IntegralV;
                }

                retVal.Add(ma);
            }

            return retVal;
        }

        private List<HourAggregation> CreateHourAggregationsWhenHourTableIsEmpty(Dictionary<long, List<MinuteAggregation>> measurementsFromCollect1)
        {
            List<HourAggregation> retVal = new List<HourAggregation>();

            foreach (KeyValuePair<long, List<MinuteAggregation>> kvp in measurementsFromCollect1)
            {
                HourAggregation ma = new HourAggregation();
                ma.PsrRef = kvp.Key;
                ma.MinP = kvp.Value.Min(x => x.MinP);
                ma.MinQ = kvp.Value.Min(x => x.MinQ);
                ma.MinV = kvp.Value.Min(x => x.MinV);
                ma.MaxP = kvp.Value.Max(x => x.MaxP);
                ma.MaxQ = kvp.Value.Max(x => x.MaxQ);
                ma.MaxV = kvp.Value.Max(x => x.MaxV);
                ma.AvgP = kvp.Value.Average(x => x.AvgP);
                ma.AvgQ = kvp.Value.Average(x => x.AvgQ);
                ma.AvgV = kvp.Value.Average(x => x.AvgV);
                ma.TimeStamp = RoundDown(kvp.Value[0].TimeStamp, TimeSpan.FromHours(1));

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    ma.IntegralP += kvp.Value[i].IntegralP;
                    ma.IntegralQ += kvp.Value[i].IntegralQ;
                    ma.IntegralV += kvp.Value[i].IntegralV;
                }

                retVal.Add(ma);
            }

            return retVal;
        }

        private void SetUpTimerForHours(DateTime argument)
        {
            DateTime current = DateTime.Now;
            DateTime alertTime = argument.AddMilliseconds(-5);
            TimeSpan timeToGo = alertTime.TimeOfDay - current.TimeOfDay;

            if (timeToGo < TimeSpan.Zero)
            {
                return; //time already passed
            }

            this.timerHours = new System.Threading.Timer(x =>
            {
                this.AggregateForHour(argument);
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private void AggregateForHour(DateTime to)
        {
            this.isHourDone = false;

            while (!this.isMinuteDone)
            {
                Thread.Sleep(500);
            }

            this.isMinuteDone = false;

            DateTime from = to.AddHours(-1);
            DateTime argForSetUpTimer = to.AddHours(1);

            lock (lockObjM)
            {
                using (var access = new AccessDB())
                {
                    var measurements = access.AggregationForMinutes.Where(x => DateTime.Compare(x.TimeStamp, to) < 0 && DateTime.Compare(x.TimeStamp, from) > 0).ToList();

                    if (measurements == null || measurements.Count == 0)
                    {
                        var totalConsumers = access.Consumers.ToList();

                        if (totalConsumers != null || totalConsumers.Count != 0)
                        {
                            foreach (EnergyConsumerCE consumer in totalConsumers)
                            {
                                HourAggregation ma = new HourAggregation();
                                ma.PsrRef = consumer.GlobalId;
                                ma.TimeStamp = from;
                                access.AggregationForHours.Add(ma);
                            }

                            access.SaveChanges();
                        }
                    }
                    else
                    { 
                        lock (lockObjH)
                        {
                            var hourAggregation = access.AggregationForHours.FirstOrDefault();

                            if (hourAggregation == null)
                            {
                                List<HourAggregation> aggregations = CreateHourAggregationsWhenHourTableIsEmpty(CreateDictionaryForHour(measurements));

                                foreach (HourAggregation ma in aggregations)
                                {
                                    access.AggregationForHours.Add(ma);
                                }

                                access.SaveChanges();
                            }
                            else
                            {
                                DateTime beforeFrom = from.AddHours(-1);
                                List<MinuteAggregation> previousMeasurements = access.AggregationForMinutes.Where(x => DateTime.Compare(from, x.TimeStamp) > 0 && DateTime.Compare(beforeFrom, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
                                Dictionary<long, MinuteAggregation> previousMeasurementsDic = new Dictionary<long, MinuteAggregation>();

                                foreach (MinuteAggregation dm in previousMeasurements)
                                {
                                    MinuteAggregation value = null;

                                    if (!previousMeasurementsDic.TryGetValue(dm.PsrRef, out value))
                                    {
                                        previousMeasurementsDic[dm.PsrRef] = dm;
                                    }
                                }

                                List<HourAggregation> aggregations = CreateHourAggregationsWhenHourTableIsNotEmpty(from, previousMeasurementsDic, CreateDictionaryForHour(measurements));

                                foreach (HourAggregation ma in aggregations)
                                {
                                    access.AggregationForHours.Add(ma);
                                }

                                access.SaveChanges();
                            }
                        }
                    }
                }
            }

            this.SetUpTimerForHours(argForSetUpTimer);
            this.isHourDone = true;
        }

        public List<Statistics> ReadHourAggregationTable(List<long> gids, DateTime from)
        {
            Dictionary<DateTime, Statistics> measurements = new Dictionary<DateTime, Statistics>();
            DateTime to = from.AddDays(1);

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var rawMeas = access.AggregationForHours.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp < to).ToList();

                    foreach (var meas in rawMeas)
                    {
                        if (!measurements.ContainsKey(meas.TimeStamp))
                        {
                            measurements.Add(meas.TimeStamp, meas);
                        }
                        else
                        {
                            measurements[meas.TimeStamp].AvgP += meas.AvgP;
                            measurements[meas.TimeStamp].AvgQ += meas.AvgQ;
                            measurements[meas.TimeStamp].AvgV += meas.AvgV;
                            //measurements[meas.TimeStamp].MaxP += meas.MaxP;
                            //measurements[meas.TimeStamp].MaxQ += meas.MaxQ;
                            //measurements[meas.TimeStamp].MaxV += meas.MaxV;
                            //measurements[meas.TimeStamp].MinP += meas.MinP;
                            //measurements[meas.TimeStamp].MinQ += meas.MinQ;
                            //measurements[meas.TimeStamp].MinV += meas.MinV;
                            measurements[meas.TimeStamp].IntegralP += meas.IntegralP;
                            measurements[meas.TimeStamp].IntegralQ += meas.IntegralQ;
                            measurements[meas.TimeStamp].IntegralV += meas.IntegralV;
                        }
                    }

                    return measurements.Values.ToList();
                }
            }
        }

        #endregion

        #region days

        public void DoUndoneByDay()
        {
            List<long> numberOfUniqueConsumers = new List<long>();

            lock (lockObjH)
            {
                using (var access = new AccessDB())
                {
                    var lastMeasHour = access.AggregationForHours.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                    if (lastMeasHour != null)
                    {
                        lock (lockObjD)
                        {
                            var lastMeasDays = access.AggregationForDays.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                            if (lastMeasDays != null)
                            {
                                DateTime temp = lastMeasDays.TimeStamp.AddDays(1);

                                if (DateTime.Compare(temp, lastMeasHour.TimeStamp) < 0) // postoje merenja u minutnoj tabeli koje treba upisati u satnu
                                {
                                    List<HourAggregation> toBeWritten = access.AggregationForHours.Where(x => DateTime.Compare(temp, x.TimeStamp) > 0 && DateTime.Compare(lastMeasDays.TimeStamp, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
                                    Dictionary<long, HourAggregation> toBeWrittenDic = new Dictionary<long, HourAggregation>();

                                    foreach (HourAggregation dm in toBeWritten) // poslednja merenja koja su vec obradjena da bi se racunao integral
                                    {
                                        HourAggregation value = null;

                                        if (!toBeWrittenDic.TryGetValue(dm.PsrRef, out value))
                                        {
                                            toBeWrittenDic[dm.PsrRef] = dm;
                                        }
                                    }

                                    numberOfUniqueConsumers = toBeWrittenDic.Keys.ToList();
                                    DateTime now = RoundDown(DateTime.Now, TimeSpan.FromDays(1));
                                    DateTime roundDownLastMeasHour = RoundDown(lastMeasForHoursBeforeAppStart, TimeSpan.FromDays(1));
                                    List<HourAggregation> tempList = new List<HourAggregation>();
                                    Dictionary<long, List<HourAggregation>> measurementsFromHour;

                                    if (DateTime.Compare(now, roundDownLastMeasHour) != 0)
                                    {
                                        tempList = access.AggregationForHours.Where(x => DateTime.Compare(temp, x.TimeStamp) < 0).ToList(); // merenja koja treba da se upisu u bazu (minutna tabela)
                                        measurementsFromHour = CreateDictionaryForDay(tempList);
                                        toBeWritten.AddRange(tempList);
                                        List<DayAggregation> aggregations = CreateDayAggregationsWhenDayTableIsNotEmpty(temp, toBeWrittenDic, measurementsFromHour);

                                        foreach (DayAggregation ma in aggregations)
                                        {
                                            access.AggregationForDays.Add(ma);
                                        }

                                    }

                                    while (DateTime.Compare(roundDownLastMeasHour, now.AddHours(-1)) < 0)
                                    {
                                        for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                        {
                                            HourAggregation ma = new HourAggregation();
                                            ma.PsrRef = numberOfUniqueConsumers[i];
                                            ma.TimeStamp = roundDownLastMeasHour.AddDays(1);
                                            access.AggregationForHours.Add(ma);
                                        }

                                        roundDownLastMeasHour = roundDownLastMeasHour.AddDays(1);
                                    }

                                    access.SaveChanges();
                                }
                                else
                                {
                                    List<HourAggregation> toBeWritten = access.AggregationForHours.Where(x => x.TimeStamp.Year == lastMeasHour.TimeStamp.Year &&
                                    x.TimeStamp.Month == lastMeasHour.TimeStamp.Month &&
                                    x.TimeStamp.Day == lastMeasHour.TimeStamp.Day &&
                                    x.TimeStamp.Hour == lastMeasHour.TimeStamp.Hour).ToList(); //moguc bug u buducnosti!
                                    Dictionary<long, HourAggregation> toBeWrittenDic = new Dictionary<long, HourAggregation>();

                                    foreach (HourAggregation dm in toBeWritten)
                                    {
                                        HourAggregation value = null;

                                        if (!toBeWrittenDic.TryGetValue(dm.PsrRef, out value))
                                        {
                                            toBeWrittenDic[dm.PsrRef] = dm;
                                        }
                                    }

                                    numberOfUniqueConsumers = toBeWrittenDic.Keys.ToList();
                                    DateTime now = this.RoundDown(DateTime.Now, TimeSpan.FromDays(1));
                                    DateTime roundDownLastMeasHour = this.RoundUp(lastMeasForHoursBeforeAppStart, TimeSpan.FromHours(1));

                                    while (DateTime.Compare(roundDownLastMeasHour, now.AddHours(-1)) < 0)
                                    {
                                        for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                        {
                                            HourAggregation ma = new HourAggregation();
                                            ma.PsrRef = numberOfUniqueConsumers[i];
                                            ma.TimeStamp = roundDownLastMeasHour.AddDays(1);
                                            access.AggregationForHours.Add(ma);
                                        }

                                        roundDownLastMeasHour = roundDownLastMeasHour.AddDays(1);
                                    }

                                    access.SaveChanges();
                                }
                            }
                            else
                            {
                                DateTime now = RoundDown(DateTime.Now, TimeSpan.FromDays(1));
                                DateTime roundDownLastMeasHour = RoundDown(lastMeasHour.TimeStamp, TimeSpan.FromDays(1));
                                List<HourAggregation> toBeWritten1 = new List<HourAggregation>();
                                Dictionary<long, List<HourAggregation>> measurementsFromDay1;

                                if (DateTime.Compare(roundDownLastMeasHour, now) != 0)
                                {
                                    toBeWritten1 = access.AggregationForHours.ToList();
                                    measurementsFromDay1 = CreateDictionaryForDay(toBeWritten1);
                                    numberOfUniqueConsumers = measurementsFromDay1.Keys.ToList();
                                    List<DayAggregation> aggregations = CreateDayAggregationsWhenDayTableIsEmpty(measurementsFromDay1);

                                    foreach (DayAggregation ma in aggregations)
                                    {
                                        access.AggregationForDays.Add(ma);
                                    }
                                }

                                while (DateTime.Compare(roundDownLastMeasHour, now.AddDays(-1)) < 0)
                                {
                                    for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                    {
                                        DayAggregation ma = new DayAggregation();
                                        ma.PsrRef = numberOfUniqueConsumers[i];
                                        ma.TimeStamp = roundDownLastMeasHour.AddDays(1);
                                        access.AggregationForDays.Add(ma);
                                    }

                                    roundDownLastMeasHour = roundDownLastMeasHour.AddDays(1);
                                }

                                access.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<long, List<HourAggregation>> CreateDictionaryForDay(List<HourAggregation> toBeWritten1)
        {
            Dictionary<long, List<HourAggregation>> retVal = new Dictionary<long, List<HourAggregation>>();

            foreach (HourAggregation dm in toBeWritten1)
            {
                List<HourAggregation> value = null;

                if (!retVal.TryGetValue(dm.PsrRef, out value))
                {
                    retVal[dm.PsrRef] = new List<HourAggregation>();
                }

                retVal[dm.PsrRef].Add(dm);
            }

            return retVal;
        }

        private List<DayAggregation> CreateDayAggregationsWhenDayTableIsNotEmpty(DateTime temp, Dictionary<long, HourAggregation> toBeWrittenDic, Dictionary<long, List<HourAggregation>> measurementsFromHour)
        {
            List<DayAggregation> retVal = new List<DayAggregation>();

            foreach (KeyValuePair<long, List<HourAggregation>> kvp in measurementsFromHour)
            {
                DayAggregation ma = new DayAggregation();
                ma.PsrRef = kvp.Key;
                ma.MinP = kvp.Value.Min(x => x.MinP);
                ma.MinQ = kvp.Value.Min(x => x.MinQ);
                ma.MinV = kvp.Value.Min(x => x.MinV);
                ma.MaxP = kvp.Value.Max(x => x.MaxP);
                ma.MaxQ = kvp.Value.Max(x => x.MaxQ);
                ma.MaxV = kvp.Value.Max(x => x.MaxV);
                ma.AvgP = kvp.Value.Average(x => x.AvgP);
                ma.AvgQ = kvp.Value.Average(x => x.AvgQ);
                ma.AvgV = kvp.Value.Average(x => x.AvgV);
                ma.TimeStamp = temp;

                ma.IntegralP += toBeWrittenDic[kvp.Key].IntegralP;
                ma.IntegralQ += toBeWrittenDic[kvp.Key].IntegralQ;
                ma.IntegralV += toBeWrittenDic[kvp.Key].IntegralV;

                for (int i = 1; i < kvp.Value.Count; i++)
                {
                    ma.IntegralP += kvp.Value[i].IntegralP;
                    ma.IntegralQ += kvp.Value[i].IntegralQ;
                    ma.IntegralV += kvp.Value[i].IntegralV;
                }

                retVal.Add(ma);
            }

            return retVal;
        }

        private List<DayAggregation> CreateDayAggregationsWhenDayTableIsEmpty(Dictionary<long, List<HourAggregation>> measurementsFromCollect1)
        {
            List<DayAggregation> retVal = new List<DayAggregation>();

            foreach (KeyValuePair<long, List<HourAggregation>> kvp in measurementsFromCollect1)
            {
                DayAggregation ma = new DayAggregation();
                ma.PsrRef = kvp.Key;
                ma.MinP = kvp.Value.Min(x => x.MinP);
                ma.MinQ = kvp.Value.Min(x => x.MinQ);
                ma.MinV = kvp.Value.Min(x => x.MinV);
                ma.MaxP = kvp.Value.Max(x => x.MaxP);
                ma.MaxQ = kvp.Value.Max(x => x.MaxQ);
                ma.MaxV = kvp.Value.Max(x => x.MaxV);
                ma.AvgP = kvp.Value.Average(x => x.AvgP);
                ma.AvgQ = kvp.Value.Average(x => x.AvgQ);
                ma.AvgV = kvp.Value.Average(x => x.AvgV);
                ma.TimeStamp = RoundDown(kvp.Value[0].TimeStamp, TimeSpan.FromDays(1));

                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    ma.IntegralP += kvp.Value[i].IntegralP;
                    ma.IntegralQ += kvp.Value[i].IntegralQ;
                    ma.IntegralV += kvp.Value[i].IntegralV;
                }

                retVal.Add(ma);
            }

            return retVal;
        }

        private void SetUpTimerForDays(DateTime argument)
        {
            DateTime current = DateTime.Now;
            DateTime alertTime = argument;
            TimeSpan timeToGo = alertTime.TimeOfDay - current.TimeOfDay;

            if (timeToGo < TimeSpan.Zero)
            {
                return; //time already passed
            }

            this.timerDays = new System.Threading.Timer(x =>
            {
                this.AggregateForDay(argument);
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private void AggregateForDay(DateTime to)
        {
            isDayDone = false;

            while (!this.isHourDone)
            {
                Thread.Sleep(500);
            }

            this.isHourDone = false;

            DateTime from = to.AddDays(-1);
            DateTime argForSetUpTimer = to.AddDays(1);

            lock (lockObjM)
            {
                using (var access = new AccessDB())
                {
                    var measurements = access.AggregationForHours.Where(x => DateTime.Compare(x.TimeStamp, to) < 0 && DateTime.Compare(x.TimeStamp, from) > 0).ToList();

                    if (measurements == null || measurements.Count == 0)
                    {
                        var totalConsumers = access.Consumers.ToList();

                        if (totalConsumers != null || totalConsumers.Count != 0)
                        {
                            foreach (EnergyConsumerCE consumer in totalConsumers)
                            {
                                DayAggregation ma = new DayAggregation();
                                ma.PsrRef = consumer.GlobalId;
                                ma.TimeStamp = from;
                                access.AggregationForDays.Add(ma);
                            }

                            access.SaveChanges();
                        }
                    }
                    else
                    { 
                        lock (lockObjH)
                        {
                            var dayAggregation = access.AggregationForDays.FirstOrDefault();

                            if (dayAggregation == null)
                            {
                                List<DayAggregation> aggregations = CreateDayAggregationsWhenDayTableIsEmpty(CreateDictionaryForDay(measurements));

                                foreach (DayAggregation ma in aggregations)
                                {
                                    access.AggregationForDays.Add(ma);
                                }

                                access.SaveChanges();
                            }
                            else
                            {
                                DateTime beforeFrom = from.AddDays(-1);
                                List<HourAggregation> previousMeasurements = access.AggregationForHours.Where(x => DateTime.Compare(from, x.TimeStamp) > 0 && DateTime.Compare(beforeFrom, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
                                Dictionary<long, HourAggregation> previousMeasurementsDic = new Dictionary<long, HourAggregation>();

                                foreach (HourAggregation dm in previousMeasurements)
                                {
                                    HourAggregation value = null;

                                    if (!previousMeasurementsDic.TryGetValue(dm.PsrRef, out value))
                                    {
                                        previousMeasurementsDic[dm.PsrRef] = dm;
                                    }
                                }

                                List<DayAggregation> aggregations = CreateDayAggregationsWhenDayTableIsNotEmpty(from, previousMeasurementsDic, CreateDictionaryForDay(measurements));

                                foreach (DayAggregation ma in aggregations)
                                {
                                    access.AggregationForDays.Add(ma);
                                }

                                access.SaveChanges();
                            }
                        }
                    }
                }
            }

            this.SetUpTimerForDays(argForSetUpTimer);
            isDayDone = true;
        }

        public List<Statistics> ReadDayAggregationTable(List<long> gids, DateTime from)
        {
            Dictionary<DateTime, Statistics> measurements = new Dictionary<DateTime, Statistics>();
            DateTime to = from.AddMonths(1);

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var rawMeas = access.AggregationForDays.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp < to).ToList();

                    foreach (var meas in rawMeas)
                    {
                        if (!measurements.ContainsKey(meas.TimeStamp))
                        {
                            measurements.Add(meas.TimeStamp, meas);
                        }
                        else
                        {
                            measurements[meas.TimeStamp].AvgP += meas.AvgP;
                            measurements[meas.TimeStamp].AvgQ += meas.AvgQ;
                            measurements[meas.TimeStamp].AvgV += meas.AvgV;
                            //measurements[meas.TimeStamp].MaxP += meas.MaxP;
                            //measurements[meas.TimeStamp].MaxQ += meas.MaxQ;
                            //measurements[meas.TimeStamp].MaxV += meas.MaxV;
                            //measurements[meas.TimeStamp].MinP += meas.MinP;
                            //measurements[meas.TimeStamp].MinQ += meas.MinQ;
                            //measurements[meas.TimeStamp].MinV += meas.MinV;
                            measurements[meas.TimeStamp].IntegralP += meas.IntegralP;
                            measurements[meas.TimeStamp].IntegralQ += meas.IntegralQ;
                            measurements[meas.TimeStamp].IntegralV += meas.IntegralV;
                        }
                    }

                    return measurements.Values.ToList();
                }
            }
        }

        #endregion

        #region minutes

        public void DoUndoneByMinute()
        {
            List<long> numberOfUniqueConsumers = new List<long>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var lastMeasCollect = access.Collect.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                    if (lastMeasCollect != null)
                    {
                        lock (lockObjM)
                        {
                            var lastMeasMinutes = access.AggregationForMinutes.OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                            if (lastMeasMinutes != null)
                            {
                                this.lastMeasForMinutesBeforeAppStart = lastMeasMinutes.TimeStamp;
                                DateTime temp = lastMeasMinutes.TimeStamp.AddMinutes(1);

                                if (DateTime.Compare(temp, lastMeasCollect.TimeStamp) < 0) // postoje merenja u collect tabeli koje treba upisati u minutnu
                                {
                                    List<DynamicMeasurement> toBeWritten = access.Collect.Where(x => DateTime.Compare(temp, x.TimeStamp) > 0 && DateTime.Compare(lastMeasMinutes.TimeStamp, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
                                    Dictionary<long, DynamicMeasurement> toBeWrittenDic = new Dictionary<long, DynamicMeasurement>();

                                    foreach (DynamicMeasurement dm in toBeWritten) // poslednja merenja koja su vec obradjena da bi se racunao integral
                                    {
                                        DynamicMeasurement value = null;
                                        if (!toBeWrittenDic.TryGetValue(dm.PsrRef, out value))
                                        {
                                            toBeWrittenDic[dm.PsrRef] = dm;
                                        }
                                    }

                                    numberOfUniqueConsumers = toBeWrittenDic.Keys.ToList();
                                    DateTime now = RoundDown(DateTime.Now, TimeSpan.FromMinutes(1));
                                    DateTime roundDownLastMeasCollect = RoundDown(lastMeasCollect.TimeStamp, TimeSpan.FromHours(1));
                                    List<DynamicMeasurement> tempList = new List<DynamicMeasurement>();
                                    Dictionary<long, List<DynamicMeasurement>> measurementsFromCollect;

                                    if (DateTime.Compare(now, roundDownLastMeasCollect) != 0)
                                    {
                                        tempList = access.Collect.Where(x => DateTime.Compare(temp, x.TimeStamp) < 0).ToList(); // merenja koja treba da se upisu u bazu (minutna tabela)
                                        measurementsFromCollect = CreateDictionary(tempList);
                                        List<MinuteAggregation> aggregations = CreateMinuteAggregationsWhenMinuteTableIsNotEmpty(temp, toBeWrittenDic, measurementsFromCollect);

                                        foreach (MinuteAggregation ma in aggregations)
                                        {
                                            access.AggregationForMinutes.Add(ma);
                                        }

                                        while (DateTime.Compare(roundDownLastMeasCollect, now.AddMinutes(-1)) < 0)
                                        {
                                            for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                            {
                                                MinuteAggregation ma = new MinuteAggregation();
                                                ma.PsrRef = numberOfUniqueConsumers[i];
                                                ma.TimeStamp = roundDownLastMeasCollect.AddMinutes(1);
                                                access.AggregationForMinutes.Add(ma);
                                            }

                                            roundDownLastMeasCollect = roundDownLastMeasCollect.AddMinutes(1);
                                        }

                                        access.SaveChanges();
                                    }
                                }
                                else
                                {
                                    List<DynamicMeasurement> toBeWritten = access.Collect.Where(x => x.TimeStamp.Year == lastMeasCollect.TimeStamp.Year &&
                                    x.TimeStamp.Month == lastMeasCollect.TimeStamp.Month &&
                                    x.TimeStamp.Day == lastMeasCollect.TimeStamp.Day &&
                                    x.TimeStamp.Hour == lastMeasCollect.TimeStamp.Hour &&
                                    x.TimeStamp.Minute == lastMeasCollect.TimeStamp.Minute &&
                                    x.TimeStamp.Second == lastMeasCollect.TimeStamp.Second).ToList(); //moguc bug u buducnosti!, moze se koristiti baza za statiku
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
                                    DateTime now = RoundDown(DateTime.Now, TimeSpan.FromMinutes(1));
                                    DateTime roundDownLastMeasCollect = RoundDown(lastMeasCollect.TimeStamp, TimeSpan.FromMinutes(1));

                                    while (DateTime.Compare(roundDownLastMeasCollect, now.AddMinutes(-1)) < 0)
                                    {
                                        for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                        {
                                            MinuteAggregation ma = new MinuteAggregation();
                                            ma.PsrRef = numberOfUniqueConsumers[i];
                                            ma.TimeStamp = roundDownLastMeasCollect.AddMinutes(1);
                                            access.AggregationForMinutes.Add(ma);
                                        }

                                        roundDownLastMeasCollect = roundDownLastMeasCollect.AddMinutes(1);
                                    }

                                    access.SaveChanges();
                                }
                            }
                            else
                            {
                                DateTime now = RoundDown(DateTime.Now, TimeSpan.FromMinutes(1));
                                DateTime roundDownLastMeasCollect = RoundDown(lastMeasCollect.TimeStamp, TimeSpan.FromMinutes(1));
                                List<DynamicMeasurement> toBeWritten1 = new List<DynamicMeasurement>();
                                Dictionary<long, List<DynamicMeasurement>> measurementsFromCollect1 = new Dictionary<long, List<DynamicMeasurement>>();

                                if (DateTime.Compare(now, roundDownLastMeasCollect) != 0)
                                {
                                    toBeWritten1 = access.Collect.ToList();
                                    measurementsFromCollect1 = CreateDictionary(toBeWritten1);
                                    numberOfUniqueConsumers = measurementsFromCollect1.Keys.ToList();
                                    List<MinuteAggregation> aggregations = CreateMinuteAggregationsWhenMinuteTableIsEmpty(measurementsFromCollect1);

                                    foreach (MinuteAggregation ma in aggregations)
                                    {
                                        access.AggregationForMinutes.Add(ma);
                                    }
                                }

                                while (DateTime.Compare(roundDownLastMeasCollect, now.AddMinutes(-1)) < 0)
                                {
                                    for (int i = 0; i < numberOfUniqueConsumers.Count; i++)
                                    {
                                        MinuteAggregation ma = new MinuteAggregation();
                                        ma.PsrRef = numberOfUniqueConsumers[i];
                                        ma.TimeStamp = roundDownLastMeasCollect.AddMinutes(1);
                                        access.AggregationForMinutes.Add(ma);
                                    }

                                    roundDownLastMeasCollect = roundDownLastMeasCollect.AddMinutes(1);
                                }

                                access.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        #endregion minutes
        public void StartThreads()
        {
            DateTime now = DateTime.Now;
            DateTime argument = RoundUp(now, TimeSpan.FromMinutes(1));
            this.SetUpTimer(argument);
            DateTime dt = new DateTime(2018, 1, 15, 19, 19, 0);
            DateTime argumentH = RoundUp(now, TimeSpan.FromHours(1));
            this.SetUpTimerForHours(argumentH);
            DateTime argumentD = RoundUp(now, TimeSpan.FromDays(1));
            this.SetUpTimerForDays(argumentD);
        }

        public bool AddMeasurements(List<DynamicMeasurement> measurements)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (DynamicMeasurement m in measurements)
                    {
                        var lastMeas = access.Collect.Where(x => x.PsrRef == m.PsrRef).OrderByDescending(x => x.TimeStamp).FirstOrDefault();

                        if (lastMeas != null && (m.CurrentP == -1 || m.CurrentQ == -1 || m.CurrentV == -1))
                        {
                            if ((Math.Abs((double)(m.TimeStamp - lastMeas.TimeStamp).TotalSeconds)) < 1)
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

                                access.Collect.Add(m);

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
                            access.Collect.Add(m);

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
                    foreach (var meas in access.Collect.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp <= to).ToList())
                    {
                        measurements.Add(meas);
                    }

                    return measurements;
                }
            }
        }

        public bool AddGeoRegions(List<GeographicalRegionCE> geoRegions)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (GeographicalRegionCE gr in geoRegions)
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

        public bool AddSubGeoRegions(List<SubGeographicalRegionCE> subGeoRegions)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (SubGeographicalRegionCE sgr in subGeoRegions)
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

        public bool AddSubstations(List<SubstationCE> substations)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (SubstationCE ss in substations)
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

        public bool AddConsumers(List<EnergyConsumerCE> consumers)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (EnergyConsumerCE ec in consumers)
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

        public Dictionary<long, EnergyConsumerCE> ReadConsumers()
        {
            Dictionary<long, EnergyConsumerCE> retVal = new Dictionary<long, EnergyConsumerCE>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var consumers = access.Consumers.ToList();

                    foreach (EnergyConsumerCE ec in consumers)
                    {
                        retVal.Add(ec.GlobalId, ec);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, SubstationCE> ReadSubstations()
        {
            Dictionary<long, SubstationCE> retVal = new Dictionary<long, SubstationCE>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var substations = access.Substations.ToList();

                    foreach (SubstationCE ss in substations)
                    {
                        retVal.Add(ss.GlobalId, ss);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, SubGeographicalRegionCE> ReadSubGeoRegions()
        {
            Dictionary<long, SubGeographicalRegionCE> retVal = new Dictionary<long, SubGeographicalRegionCE>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var subGeoRegions = access.SubGeoRegions.ToList();

                    foreach (SubGeographicalRegionCE sgr in subGeoRegions)
                    {
                        retVal.Add(sgr.GlobalId, sgr);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, GeographicalRegionCE> ReadGeoRegions()
        {
            Dictionary<long, GeographicalRegionCE> retVal = new Dictionary<long, GeographicalRegionCE>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var geoRegions = access.GeoRegions.ToList();

                    foreach (GeographicalRegionCE gr in geoRegions)
                    {
                        retVal.Add(gr.GlobalId, gr);
                    }
                }

                return retVal;
            }
        }
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
            this.isMinuteDone = false;
            DateTime from = to.AddMinutes(-1);
            DateTime argForSetUpTimer = to.AddMinutes(1);

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var measurements = access.Collect.Where(x => DateTime.Compare(x.TimeStamp, to) < 0 && DateTime.Compare(x.TimeStamp, from) > 0).ToList();

                    if (measurements == null || measurements.Count == 0)
                    {
                        var totalConsumers = access.Consumers.ToList();

                        if (totalConsumers != null || totalConsumers.Count != 0)
                        {
                            foreach (EnergyConsumerCE consumer in totalConsumers)
                            {
                                MinuteAggregation ma = new MinuteAggregation();
                                ma.PsrRef = consumer.GlobalId;
                                ma.TimeStamp = from;
                                access.AggregationForMinutes.Add(ma);
                            }
                            
                            access.SaveChanges();
                        }
                    }
                    else
                    { 
                        lock (lockObjM)
                        {
                            var minuteAggregation = access.AggregationForMinutes.FirstOrDefault();

                            if (minuteAggregation == null)
                            {
                                List<MinuteAggregation> aggregations = CreateMinuteAggregationsWhenMinuteTableIsEmpty(CreateDictionary(measurements));

                                foreach (MinuteAggregation ma in aggregations)
                                {
                                    access.AggregationForMinutes.Add(ma);
                                }

                                access.SaveChanges();
                            }
                            else
                            {
                                DateTime beforeFrom = from.AddMinutes(-1);
                                List<DynamicMeasurement> previousMeasurements = access.Collect.Where(x => DateTime.Compare(from, x.TimeStamp) > 0 && DateTime.Compare(beforeFrom, x.TimeStamp) < 0).OrderByDescending(x => x.TimeStamp).ToList();
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
                                    access.AggregationForMinutes.Add(ma);
                                }

                                access.SaveChanges();
                            }
                        }
                    }
                }
            }

            SetUpTimer(argForSetUpTimer);
            this.isMinuteDone = true;
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

                if (toBeWrittenDic.Count != 0)
                {
                    ma.IntegralP += (toBeWrittenDic[kvp.Key].CurrentP * (((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[0].CurrentP - toBeWrittenDic[kvp.Key].CurrentP))) / 2;
                    ma.IntegralQ += (toBeWrittenDic[kvp.Key].CurrentQ * (((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[0].CurrentQ - toBeWrittenDic[kvp.Key].CurrentQ))) / 2;
                    ma.IntegralV += (toBeWrittenDic[kvp.Key].CurrentV * (((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds)) / 3600) + ((((float)(kvp.Value[0].TimeStamp - toBeWrittenDic[kvp.Key].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(kvp.Value[0].CurrentV - toBeWrittenDic[kvp.Key].CurrentV))) / 2;
                }

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

        public List<Statistics> ReadMinuteAggregationTable(List<long> gids, DateTime from)
        {
            Dictionary<DateTime, Statistics> measurements = new Dictionary<DateTime, Statistics>();
            DateTime to = from.AddHours(1);

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var rawMeas = access.AggregationForMinutes.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp < to).ToList();

                    foreach (var meas in rawMeas)
                    {
                        if (!measurements.ContainsKey(meas.TimeStamp))
                        {
                            measurements.Add(meas.TimeStamp, meas);
                        }
                        else
                        {
                            measurements[meas.TimeStamp].AvgP += meas.AvgP;
                            measurements[meas.TimeStamp].AvgQ += meas.AvgQ;
                            measurements[meas.TimeStamp].AvgV += meas.AvgV;
                            //measurements[meas.TimeStamp].MaxP += meas.MaxP;
                            //measurements[meas.TimeStamp].MaxQ += meas.MaxQ;
                            //measurements[meas.TimeStamp].MaxV += meas.MaxV;
                            //measurements[meas.TimeStamp].MinP += meas.MinP;
                            //measurements[meas.TimeStamp].MinQ += meas.MinQ;
                            //measurements[meas.TimeStamp].MinV += meas.MinV;
                            measurements[meas.TimeStamp].IntegralP += meas.IntegralP;
                            measurements[meas.TimeStamp].IntegralQ += meas.IntegralQ;
                            measurements[meas.TimeStamp].IntegralV += meas.IntegralV;
                        }
                    }
                    
                    return measurements.Values.ToList();
                }
            }
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