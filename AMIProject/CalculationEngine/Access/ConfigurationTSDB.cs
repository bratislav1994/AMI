using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngine.Access
{
    public class ConfigurationTSDB : DbMigrationsConfiguration<AccessTSDB>
    {
        public ConfigurationTSDB()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "TSDB";
        }
    }
}
