using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
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

        public bool AddBaseVoltages(BaseVoltageForScada bv)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var cre = access.BaseVoltages.Where(x => bv.GlobalId == x.GlobalId).FirstOrDefault();

                    if (cre == null)
                    {
                        access.BaseVoltages.Add(bv);
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

        public bool AddPowerTransformers(PowerTransformerForScada pt)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var cre = access.PowerTransformers.Where(x => pt.GlobalId == x.GlobalId).FirstOrDefault();

                    if (cre == null)
                    {
                        access.PowerTransformers.Add(pt);
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

        public bool AddSubstations(SubstationForScada ss)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var cre = access.Substations.Where(x => ss.GlobalId == x.GlobalId).FirstOrDefault();

                    if (cre == null)
                    {
                        access.Substations.Add(ss);
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

        public List<BaseVoltageForScada> ReadBaseVoltages()
        {
            List<BaseVoltageForScada> baseVoltages = new List<BaseVoltageForScada>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var bvs = access.BaseVoltages.ToList();

                    foreach (var bv in bvs)
                    {
                        baseVoltages.Add(bv);
                    }

                    return baseVoltages;
                }
            }
        }

        public List<PowerTransformerForScada> ReadPowerTransformers()
        {
            List<PowerTransformerForScada> powerTransformers = new List<PowerTransformerForScada>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var pts = access.PowerTransformers.ToList();

                    foreach (var pt in pts)
                    {
                        powerTransformers.Add(pt);
                    }

                    return powerTransformers;
                }
            }
        }

        public PowerTransformerForScada GetPowerTransformerForCommanding(long baseVol, long substation)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    return access.PowerTransformers.Where(x => x.BaseVoltageId == baseVol && x.SubstationId == substation).FirstOrDefault();
                }
            }
        }

        public List<SubstationForScada> ReadSubstations()
        {
            List<SubstationForScada> substations = new List<SubstationForScada>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var sss = access.Substations.ToList();

                    foreach (var ss in sss)
                    {
                        substations.Add(ss);
                    }

                    return substations;
                }
            }
        }
    }
}
