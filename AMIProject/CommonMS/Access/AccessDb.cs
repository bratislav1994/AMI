using FTN.Common.ClassesForAlarmDB;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
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

        public DbSet<GeographicalRegionDb> GeoRegions { get; set; }
        public DbSet<SubGeographicalRegionDb> SubGeoRegions { get; set; }
        public DbSet<SubstationDb> Substations { get; set; }
        public DbSet<EnergyConsumerDb> Consumers { get; set; }
        public DbSet<BaseVoltageDb> BaseVoltages { get; set; }
        public DbSet<ActiveAlarm> ActiveAlarm { get; set; }
        public DbSet<ResolvedAlarm> ResolvedAlarm { get; set; }
    }
}
