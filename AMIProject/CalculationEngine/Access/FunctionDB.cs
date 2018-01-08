using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
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

                            if ((Math.Abs((double)(m.TimeStamp - lastMeas.TimeStamp).TotalSeconds)) < 3)
                            {
                                if (lastMeas.CurrentP == -1)
                                {
                                    lastMeas.CurrentP = m.CurrentP;
                                }

                                if (lastMeas.CurrentQ == -1)
                                {
                                    lastMeas.CurrentQ = m.CurrentQ;
                                }

                                if (lastMeas.CurrentV == -1)
                                {
                                    lastMeas.CurrentV = m.CurrentV;
                                }

                                access.Entry(lastMeas).State = System.Data.Entity.EntityState.Modified;
                            }
                            else
                            {
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
                            }
                        }
                        else
                        {
                            access.History.Add(m);
                        }

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

        public List<DynamicMeasurement> GetMeasForChart(List<long> gids, DateTime from, DateTime to)
        {
            List<DynamicMeasurement> measurements = new List<DynamicMeasurement>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (var meas in access.History.Where(x => gids.Any(y => y == x.PsrRef) && x.TimeStamp >= from && x.TimeStamp <= to && x.OperationType == OperationType.UPDATE).ToList())
                    {
                        measurements.Add(meas);
                    }

                    return measurements;
                }
            }
        }
    }
}
