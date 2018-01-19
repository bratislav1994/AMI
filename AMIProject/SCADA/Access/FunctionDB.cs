using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.Access
{
    public class FunctionDB
    {
        private static object lockObj = new object();

        public FunctionDB()
        { }

        public bool AddSimulator(WrapperDB listMeas)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var wrap = access.WrapperMeas.Where(x => x.RtuAddress == listMeas.RtuAddress).FirstOrDefault();

                    if (wrap == null)
                    {
                        access.WrapperMeas.Add(listMeas);
                        int i = access.SaveChanges();

                        if (i > 0)
                        {
                            return true;
                        }

                        return false;
                    }

                    return false;
                }
            }
        }

        public bool AddMeasurement(List<MeasurementForScada> listMeas)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (MeasurementForScada m in listMeas)
                    {
                        var cre = access.MeasurementForScada.Where(x => x.MeasurementId == m.Measurement.IdDB).FirstOrDefault();

                        if (cre == null)
                        {
                            access.MeasurementForScada.Add(m);
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

        public bool AddConsumers(EnergyConsumerForScada ec)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var cre = access.Consumers.Where(x => ec.GlobalId == x.GlobalId).FirstOrDefault();

                    if (cre == null)
                    {
                        access.Consumers.Add(ec);
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

        public int GetWrapperId(int rtuAddress)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var retVal = access.WrapperMeas.Where(x => x.RtuAddress == rtuAddress).FirstOrDefault();

                    return retVal == null ? -1 : retVal.IdDB;
                }
            }
        }

        public List<WrapperDB> ReadMeas()
        {
            List<WrapperDB> measurements = new List<WrapperDB>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var measur = access.WrapperMeas.Include("ListOfMeasurements").Include("ListOfMeasurements.Measurement").ToList();

                    foreach (var meas in measur)
                    {
                        measurements.Add(meas);
                    }

                    return measurements;
                }
            }
        }

        public List<EnergyConsumerForScada> ReadConsumers()
        {
            List<EnergyConsumerForScada> consumers = new List<EnergyConsumerForScada>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var cons = access.Consumers.ToList();

                    foreach (var ec in cons)
                    {
                        consumers.Add(ec);
                    }

                    return consumers;
                }
            }
        }
    }
}
