using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using System.ServiceModel;
using TC57CIM.IEC61970.Meas;
using FTN.ServiceContracts;
using FTN.Common.Logger;
using System.Data.Entity;
using SCADA.Access;

namespace SCADA
{
    class Program
    {
        private static ServiceHost svc = null;
        private static ServiceHost svc2 = null;

        static int Main(string[] args)
        {
            Console.Title = "SCADA";
            Logger.Path = "Scada.txt";

            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = System.IO.Path.GetDirectoryName(executable);
            path = path.Substring(0, path.LastIndexOf("bin")) + "DB";
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AccessDB, Configuration>());

            Scada scada = new Scada();
            svc = new ServiceHost(scada);
            var binding = new NetTcpBinding();
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.MaxBufferSize = Int32.MaxValue;
            svc.AddServiceEndpoint(typeof(IScadaDuplexSimulator),
                                   binding,
                                   new Uri("net.tcp://localhost:10000/Scada/Simulator"));
            svc.Open();

            svc2 = new ServiceHost(scada);
            var binding2 = new NetTcpBinding();
            binding2.MaxReceivedMessageSize = Int32.MaxValue;
            binding2.MaxBufferSize = Int32.MaxValue;
            binding2.ReceiveTimeout = TimeSpan.MaxValue;
            svc2.AddServiceEndpoint(typeof(IScadaForCECommand),
                                   binding2,
                                   new Uri("net.tcp://localhost:10001/Scada/CE"));
            svc2.Open();

            return 0;
        }
    }
}