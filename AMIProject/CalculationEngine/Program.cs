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
        public static ServiceHost svc1 = null;

        static void Main(string[] args)
        {
         //   CalculationEngine ce = new CalculationEngine();

            Start();
            Console.ReadKey(true);
            Stop();
        }

        public static void Start()
        {
            svc = new ServiceHost(CalculationEngine.Instance);
            var binding = new NetTcpBinding();
            svc.AddServiceEndpoint(typeof(ICalculationDuplexClient), binding, new Uri("net.tcp://localhost:10006/CalculationEngine/Client"));

            svc1 = new ServiceHost(CalculationEngine.Instance);
            var binding1 = new NetTcpBinding();
            svc1.AddServiceEndpoint(typeof(ICalculationEngine), binding1, new Uri("net.tcp://localhost:10050/ICalculationEngine/Calculation"));

            svc.Open();
            svc1.Open();
            Console.WriteLine("CalculationEngine waiting for request..");
        }

        public static void Stop()
        {
            svc.Close();
            svc1.Close();
            Console.WriteLine("CalculationEngine server stopped");
        }
    }
}
