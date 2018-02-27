using CommonMS.Access;
using FTN.Common.ClassesForAlarmDB;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMS.Access
{
    public class DB
    {
        private static object lockObj = new object();
        private static object lockObjAlarm = new object();
        private static DB instance;
        
        public DB()
        {
        }

        public static DB Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DB();
                }
                return instance;
            }

            set
            {
                instance = value;
            }
        }
        
        public bool AddGeoRegions(List<GeographicalRegionDb> geoRegions)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (GeographicalRegionDb gr in geoRegions)
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

        public bool AddSubGeoRegions(List<SubGeographicalRegionDb> subGeoRegions)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (SubGeographicalRegionDb sgr in subGeoRegions)
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

        public bool AddSubstations(List<SubstationDb> substations)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (SubstationDb ss in substations)
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

        public bool AddConsumers(List<EnergyConsumerDb> consumers)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (EnergyConsumerDb ec in consumers)
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

        public bool AddAddBaseVoltages(List<BaseVoltageDb> baseVoltages)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    foreach (BaseVoltageDb bv in baseVoltages)
                    {
                        access.BaseVoltages.Add(bv);
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

        public Dictionary<long, EnergyConsumerDb> ReadConsumers()
        {
            Dictionary<long, EnergyConsumerDb> retVal = new Dictionary<long, EnergyConsumerDb>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var consumers = access.Consumers.ToList();

                    foreach (EnergyConsumerDb ec in consumers)
                    {
                        retVal.Add(ec.GlobalId, ec);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, EnergyConsumerDb> ReadConsumersByID(List<long> gids)
        {
            Dictionary<long, EnergyConsumerDb> retVal = new Dictionary<long, EnergyConsumerDb>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var consumers = access.Consumers.Where(x => gids.Any(y => y == x.GlobalId)).ToList();

                    foreach (EnergyConsumerDb ec in consumers)
                    {
                        retVal.Add(ec.GlobalId, ec);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, SubstationDb> ReadSubstations()
        {
            Dictionary<long, SubstationDb> retVal = new Dictionary<long, SubstationDb>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var substations = access.Substations.ToList();

                    foreach (SubstationDb ss in substations)
                    {
                        retVal.Add(ss.GlobalId, ss);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, SubGeographicalRegionDb> ReadSubGeoRegions()
        {
            Dictionary<long, SubGeographicalRegionDb> retVal = new Dictionary<long, SubGeographicalRegionDb>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var subGeoRegions = access.SubGeoRegions.ToList();

                    foreach (SubGeographicalRegionDb sgr in subGeoRegions)
                    {
                        retVal.Add(sgr.GlobalId, sgr);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, GeographicalRegionDb> ReadGeoRegions()
        {
            Dictionary<long, GeographicalRegionDb> retVal = new Dictionary<long, GeographicalRegionDb>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var geoRegions = access.GeoRegions.ToList();

                    foreach (GeographicalRegionDb gr in geoRegions)
                    {
                        retVal.Add(gr.GlobalId, gr);
                    }
                }

                return retVal;
            }
        }

        public Dictionary<long, BaseVoltageDb> ReadBaseVoltages()
        {
            Dictionary<long, BaseVoltageDb> retVal = new Dictionary<long, BaseVoltageDb>();

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    var baseVoltages = access.BaseVoltages.ToList();

                    foreach (BaseVoltageDb gr in baseVoltages)
                    {
                        retVal.Add(gr.GlobalId, gr);
                    }
                }

                return retVal;
            }
        }

        public string ReadGeoregionNameByPSRRef(long psrRef)
        {
            string retVal = "";

            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    try
                    {
                        EnergyConsumerDb consumer = access.Consumers.Where(x => x.GlobalId == psrRef).FirstOrDefault();
                        SubstationDb substation = access.Substations.Where(x => x.GlobalId == consumer.EqContainerID).FirstOrDefault();
                        SubGeographicalRegionDb subgeoregion = access.SubGeoRegions.Where(x => x.GlobalId == substation.SubGeoRegionID).FirstOrDefault();
                        GeographicalRegionDb georegion = access.GeoRegions.Where(x => x.GlobalId == subgeoregion.GeoRegionID).FirstOrDefault();

                        retVal = georegion.Name;
                    }
                    catch
                    {
                        return "";
                    }
                }

                return retVal;
            }
        }

        public bool GeoregionsNeedToRefresh(int lastUpdate)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    return access.GeoRegions.Count() != lastUpdate;
                }
            }
        }

        public bool SubgeoregionsNeedToRefresh(int lastUpdate)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    return access.SubGeoRegions.Count() != lastUpdate;
                }
            }
        }

        public bool SubstationsNeedToRefresh(int lastUpdate)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    return access.Substations.Count() != lastUpdate;
                }
            }
        }


        public bool ConsumersNeedToRefresh(int lastUpdate)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    return access.Consumers.Count() != lastUpdate;
                }
            }
        }

        public bool BaseVoltagesNeedToRefresh(int lastUpdate)
        {
            lock (lockObj)
            {
                using (var access = new AccessDB())
                {
                    return access.BaseVoltages.Count() != lastUpdate;
                }
            }
        }

        #region alarm

        public bool AddActiveAlarm(ActiveAlarm activeAlarm)
        {
            lock (lockObjAlarm)
            {
                using (var access = new AccessDB())
                {
                    access.ActiveAlarm.Add(activeAlarm);
                    int i = access.SaveChanges();

                    if (i > 0)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public bool AddResolvedAlarm(ResolvedAlarm resolvedAlarm)
        {
            lock (lockObjAlarm)
            {
                using (var access = new AccessDB())
                {
                    access.ResolvedAlarm.Add(resolvedAlarm);
                    int i = access.SaveChanges();

                    if (i > 0)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public bool DeleteActiveAlarm(ActiveAlarm alarm)
        {
            lock (lockObjAlarm)
            {
                using (var access = new AccessDB())
                {
                    var a = access.ActiveAlarm.Where(x => x.Id == alarm.Id).FirstOrDefault();

                    if (a != null)
                    {
                        access.Entry(a).State = System.Data.Entity.EntityState.Deleted;
                    }

                    int i = access.SaveChanges();

                    if (i > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public Dictionary<long, ActiveAlarm> ReadActiveAlarm()
        {
            Dictionary<long, ActiveAlarm> retVal = new Dictionary<long, ActiveAlarm>();

            lock (lockObjAlarm)
            {
                using (var access = new AccessDB())
                {
                    var alarm = access.ActiveAlarm.ToList();

                    foreach (ActiveAlarm a in alarm)
                    {
                        retVal.Add(a.Id, a);
                    }
                }

                return retVal;
            }
        }

        public List<ResolvedAlarm> ReadResolvedAlarm()
        {
            List<ResolvedAlarm> retVal = new List<ResolvedAlarm>();

            lock (lockObjAlarm)
            {
                using (var access = new AccessDB())
                {
                    retVal = access.ResolvedAlarm.ToList();
                }

                return retVal;
            }
        }

        #endregion
    }
}
