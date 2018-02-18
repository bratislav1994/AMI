﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMS.Access
{
    public class ConfigurationDB : DbMigrationsConfiguration<CommonMS.Access.AccessDB>
    {
        public ConfigurationDB()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "DB";
        }
    }
}
