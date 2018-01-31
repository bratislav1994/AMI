using FTN.Common.ClassesForAlarmDB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngine.Access
{
    public class AccessAlarmDB : DbContext
    {
        public AccessAlarmDB() : base("alarmDB") { }

        public DbSet<AlarmActiveDB> ActiveAlarm { get; set; }
        public DbSet<AlarmResolvedDB> ResolvedAlarm { get; set; }
    }
}
