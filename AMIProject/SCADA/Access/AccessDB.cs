using FTN.Common;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;

namespace SCADA.Access
{
    public class AccessDB : DbContext
    {
        public AccessDB() : base("measDB") { }

        public DbSet<WrapperDB> WrapperMeas { get; set; }
        public DbSet<MeasurementForScada> MeasurementForScada { get; set; }
        public DbSet<Measurement> Measurement { get; set; }
        public DbSet<EnergyConsumerForScada> Consumers { get; set; }
        public DbSet<BaseVoltageForScada> BaseVoltages { get; set; }
        public DbSet<PowerTransformerForScada> PowerTransformers { get; set; }
        public DbSet<SubstationForScada> Substations { get; set; }
    }
}
