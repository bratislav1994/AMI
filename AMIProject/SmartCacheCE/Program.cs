using FTN.Common.Logger;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SmartCacheCE
{
    class Program
    {
        private static ServiceHost svc = null;

        static void Main(string[] args)
        {
            Console.Title = "Smart cache";
            Logger.Path = "SmartCache.txt";
            SmartCache sc = new SmartCache();
            svc = new ServiceHost(sc);
            svc.AddServiceEndpoint(typeof(ISmartCacheDuplexForClient),
                                   new NetTcpBinding(),
                                   new Uri("net.tcp://localhost:10008/SmartCache/Client"));
            svc.Open();

            Console.WriteLine("Smart cache service started.");
            Console.ReadKey();
        }
    }
}
