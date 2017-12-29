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
        public FunctionDB()
        { }

        public bool AddSimulator(WrapperDB listMeas)
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

        public bool AddMeasurement(List<MeasurementForScada> listMeas)
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

        public int GetWrapperId(int rtuAddress)
        {
            using (var access = new AccessDB())
            {
                var retVal = access.WrapperMeas.Where(x => x.RtuAddress == rtuAddress).FirstOrDefault();

                return retVal == null ? -1 : retVal.IdDB;
            }
        }

        public List<WrapperDB> ReadMeas()
        {
            List<WrapperDB> measurements = new List<WrapperDB>();

            using (var access = new AccessDB())
            {
                foreach (var meas in access.WrapperMeas.Include("ListOfMeasurements").Include("ListOfMeasurements.Measurement").ToList())
                {
                    measurements.Add(meas);
                }

                return measurements;
            }
        }
    }
}
