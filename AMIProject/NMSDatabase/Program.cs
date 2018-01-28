using FTN.Common.Logger;
using FTN.ServiceContracts;
using NMSDatabase.Access;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NMSDatabase
{
    class Program
    {
        private static ServiceHost svc = null;

        static void Main(string[] args)
        {
            Console.Title = "NSM Database";
            Logger.Path = "NMSDatabase.txt";

            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = System.IO.Path.GetDirectoryName(executable);
            path = path.Substring(0, path.LastIndexOf("bin")) + "DB";
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AccessDB, Configuration>());

            DatabaseForNMS db = new DatabaseForNMS();
            svc = new ServiceHost(db);
            NetTcpBinding binding = new NetTcpBinding();
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            binding.MaxBufferSize = Int32.MaxValue;
            svc.AddServiceEndpoint(typeof(IDatabaseForNMS),
                                  binding,
                                   new Uri("net.tcp://localhost:10009/Database/NMS"));
            svc.Open();
            
            Console.WriteLine("NMS database service started.");
            Console.ReadKey();
        }
    }
}
