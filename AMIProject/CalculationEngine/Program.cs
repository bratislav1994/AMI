using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngine
{
    class Program
    {
        public static ServiceHost svc = null;

        static void Main(string[] args)
        {
            CalculationEngine ce = new CalculationEngine();

            Start();
            Console.ReadKey(true);
            Stop();
        }

        public static void Start()
        {
            svc = new ServiceHost(CalculationEngine.Instance);
            var binding = new NetTcpBinding();
            svc.AddServiceEndpoint(typeof(ICalculationDuplexClient), binding, new Uri("net.tcp://localhost:10006/CalculationEngine/Client"));
            
            svc.Open();
            Console.WriteLine("CalculationEngine waiting for request..");
        }

        public static void Stop()
        {
            svc.Close();
            Console.WriteLine("CalculationEngine server stopped");
        }
    }
}
