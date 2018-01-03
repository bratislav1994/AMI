﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using System.ServiceModel;
using AMISimulator;
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

        static int Main(string[] args)
        {
            String aaa = System.IO.Directory.GetCurrentDirectory();
            int index = aaa.LastIndexOf("bin");
            string aaaa = aaa.Substring(0, index);
            aaaa += "DB";
            AppDomain.CurrentDomain.SetData("DataDirectory", aaaa);
            Console.Title = "SCADA";
            Logger.Path = "Scada.txt";

            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = System.IO.Path.GetDirectoryName(executable);
            path = path.Substring(0, path.LastIndexOf("bin")) + "DB";
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AccessDB, Configuration>());

            Scada scada = new Scada();
            svc = new ServiceHost(scada);
            svc.AddServiceEndpoint(typeof(IScadaDuplexSimulator),
                                   new NetTcpBinding(),
                                   new Uri("net.tcp://localhost:10100/Scada/Simulator"));
            svc.Open();

            return 0;
        }
    }
}