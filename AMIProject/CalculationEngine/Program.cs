using CalculationEngine.Access;
using FTN.Common.Logger;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngine
{
    class Program
    {
        public static ServiceHost svc = null;
        public static ServiceHost svc1 = null;
        public static ServiceHost svc2 = null;
        public static ServiceHost svc3 = null;

        static void Main(string[] args)
        {
            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = System.IO.Path.GetDirectoryName(executable);
            path = path.Substring(0, path.LastIndexOf("bin")) + "DB";
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AccessTSDB, ConfigurationTSDB>());
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AccessDB, ConfigurationDB>());
            Console.Title = "CE";
            Logger.Path = "CalculationEngnine.txt";
            Start();
            Console.ReadKey(true);
            Stop();
        }

        public static void Start()
        {
            svc = new ServiceHost(CalculationEngine.Instance);
            var binding = new NetTcpBinding();
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            binding.MaxBufferSize = Int32.MaxValue;
            svc.AddServiceEndpoint(typeof(ICalculationForClient), binding, new Uri("net.tcp://localhost:10006/CalculationEngine/Client"));

            svc1 = new ServiceHost(CalculationEngine.Instance);
            var binding1 = new NetTcpBinding();
            binding1.MaxReceivedMessageSize = Int32.MaxValue;
            binding1.MaxBufferSize = Int32.MaxValue;
            svc1.AddServiceEndpoint(typeof(ICalculationEngine), binding1, new Uri("net.tcp://localhost:10050/ICalculationEngine/Calculation"));

            svc2 = new ServiceHost(CalculationEngine.Instance);
            var binding2 = new NetTcpBinding();
            binding2.MaxReceivedMessageSize = Int32.MaxValue;
            binding2.MaxBufferSize = Int32.MaxValue;
            svc2.AddServiceEndpoint(typeof(ICalculationEngineDuplexSmartCache), binding2, new Uri("net.tcp://localhost:10007/ICalculationEngineDuplexSmartCache/SmartCache"));

            svc3 = new ServiceHost(CalculationEngine.Instance);
            var binding3 = new NetTcpBinding();
            binding3.SendTimeout = TimeSpan.FromDays(1);
            binding3.ReceiveTimeout = TimeSpan.FromDays(1);
            svc3.AddServiceEndpoint(typeof(ICalculationEngineForScript), binding3, new Uri("net.tcp://localhost:10010/ICalculationEngineForScript/FillingScript"));

            svc.Open();
            svc1.Open();
            svc2.Open();
            svc3.Open();
            Console.WriteLine("CalculationEngine waiting for request..");
        }

        public static void Stop()
        {
            svc.Close();
            svc1.Close();
            svc2.Close();
            svc3.Close();
            Console.WriteLine("CalculationEngine server stopped");
        }
    }
}
