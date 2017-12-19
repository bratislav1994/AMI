using System;
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

namespace SCADA
{
    class Program
    {
        private static ServiceHost svc = null;

        static int Main(string[] args)
        {
            Logger.Path = "Scada.txt";
            Scada scada = new Scada();
            scada.StartIssueCommands();
            svc = new ServiceHost(scada);
            svc.AddServiceEndpoint(typeof(IScadaDuplexSimulator),
                                   new NetTcpBinding(),
                                   new Uri("net.tcp://localhost:10100/Scada/Simulator"));
            svc.Open();

            return 0;
        }
    }
}