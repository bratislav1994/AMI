using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngine.Access
{
    public class ConfigurationAlarm : DbMigrationsConfiguration<AccessAlarmDB>
    {
        public ConfigurationAlarm()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "ALARMDB";
        }
    }
}
