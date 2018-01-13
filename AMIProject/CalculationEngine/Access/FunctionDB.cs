﻿using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngine.Access
{
    public class FunctionDB
    {
        private static object lockObj = new object();

        public FunctionDB()
        { }

        public bool AddMeasurement(DynamicMeasurement measurement)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var meas = access.History.Where(x => x.PsrRef == measurement.PsrRef).FirstOrDefault();

                    if (meas == null)
                    {
                        access.History.Add(measurement);
                        int i = access.SaveChanges();

                        if (i > 0)
                        {

                        }
                        else
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }

        public bool AddMeasurements(List<DynamicMeasurement> measurements)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (DynamicMeasurement m in measurements)
                    {
                        if (m.CurrentP == -1 || m.CurrentQ == -1 || m.CurrentV == -1)
                        {
                            var lastMeas = access.History.Where(x => x.PsrRef == m.PsrRef).OrderByDescending(x => x.TimeStamp).FirstOrDefault();

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
                                Console.WriteLine("m" + m.TimeStamp);

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
                            Console.WriteLine(m.TimeStamp);
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

        public List<DynamicMeasurement> GetMeasForHour(DateTime from, DateTime to)
        {
            List<DynamicMeasurement> measurements = new List<DynamicMeasurement>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (var meas in access.History.Where(x => x.TimeStamp >= from && x.TimeStamp <= to).ToList())
                    {
                        measurements.Add(meas);
                    }

                    return measurements;
                }
            }
        }

        public bool AddStatisticsForHour(Statistics statisticsForHour)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    //  var meas = access.History.Where(x => x.PsrRef == measurement.PsrRef).FirstOrDefault();

                    //   if (meas == null)
                    //   {
                    access.StatisticsForHour.Add(statisticsForHour);
                    int i = access.SaveChanges();

                    if (i > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    //      }

                    //  return true;
                }
            }
        }
    }
}
