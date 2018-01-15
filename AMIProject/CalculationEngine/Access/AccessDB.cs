using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Meas;
using TC57CIM.IEC61970.Wires;

namespace CalculationEngine.Access
{
    public class AccessDB : DbContext
    {
        public AccessDB() : base("tsDB") { }

        public DbSet<DynamicMeasurement> Collect { get; set; }
        public DbSet<MinuteAggregation> AggregationForMinutes { get; set; }
        public DbSet<HourAggregation> AggregationForHours { get; set; }
        public DbSet<DayAggregation> AggregationForDays { get; set; }
       /* public DbSet<GeographicalRegion> GeoRegions { get; set; }
        public DbSet<SubGeographicalRegion> SubGeoRegions { get; set; }
        public DbSet<Substation> Substations { get; set; }
        public DbSet<EnergyConsumer> Consumers { get; set; }*/

    }
}
