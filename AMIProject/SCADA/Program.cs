using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using System.ServiceModel;
using AMISimulator;
using TC57CIM.IEC61970.Meas;

namespace SCADA
{
    class Program
    {

        static int Main(string[] args)
        {
            Scada scada = new Scada();
            scada.StartIssueCommands();

            return 0;
        }
    }
}