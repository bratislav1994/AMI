using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automatak.DNP3.Interface;
using Automatak.DNP3.Adapter;
using System.ServiceModel;
using FTN.ServiceContracts;

namespace AMISimulator
{
    class Program
    {
        private static ServiceHost svc = null;

        static void Main(string[] args)
        {
            svc = new ServiceHost(AMISimulator.Instance);
            svc.AddServiceEndpoint(typeof(ISimulator),
                                    new NetTcpBinding(),
                                    new Uri("net.tcp://localhost:10100/AMISimulator/Simulator"));
            svc.Open();
            AMISimulator.Instance.SendPointValues();
        }
    }
}
