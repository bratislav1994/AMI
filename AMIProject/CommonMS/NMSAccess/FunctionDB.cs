using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMS.NMSAccess
{
    public class FunctionDB
    {
        private static object lockObj = new object();

        public FunctionDB()
        { }

        public bool AddDelta(Delta delta)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var d = access.Delta.Where(x => x.Id == delta.Id).FirstOrDefault();

                    if (d == null)
                    {
                        access.Delta.Add(delta);
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

        public List<Delta> ReadDelta()
        {
            List<Delta> delta = new List<Delta>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var retVal = access.Delta.Include("InsertOperations").Include("InsertOperations.Properties").ToList();

                    foreach (var d in retVal)
                    {
                        delta.Add(d);
                    }
                }
            }

            return delta;
        }
    }
}

