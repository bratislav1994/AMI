using FTN.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMS.NMSAccess
{
    public class AccessDB : DbContext
    {
        public AccessDB() : base("deltaDB") { }

        public DbSet<Delta> Delta { get; set; }
        //public DbSet<ResourceDescription> ResourceDescription { get; set; }
        //public DbSet<Property> Property { get; set; }
    }
}
