using FTN.Common.ClassesForAlarmDB;
using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMS.Access
{
    public class AccessDB : DbContext
    {
        public AccessDB() : base("DB") { }

        public DbSet<ActiveAlarm> ActiveAlarm { get; set; }
        public DbSet<ResolvedAlarm> ResolvedAlarm { get; set; }
        public DbSet<GeographicalRegionDb> GeoRegions { get; set; }
        public DbSet<SubGeographicalRegionDb> SubGeoRegions { get; set; }
        public DbSet<SubstationDb> Substations { get; set; }
        public DbSet<EnergyConsumerDb> Consumers { get; set; }
    }
}
