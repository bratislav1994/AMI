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

namespace CalculationEngine.Access
{
    public class AccessDB : DbContext
    {
        public AccessDB() : base("tsDB") { }

        public DbSet<DynamicMeasurement> History { get; set; }
        public DbSet<Statistics> StatisticsForHour { get; set; }

    }
}
