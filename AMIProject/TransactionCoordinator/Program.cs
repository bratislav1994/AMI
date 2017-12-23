using FTN.Common.Logger;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionCoordinator
{
    class Program
    {
        public static ServiceHost svc = null;
        public static ServiceHost svc2 = null;
        public static ServiceHost svc3 = null;
        public static ServiceHost svc4 = null;

        static void Main(string[] args)
        {
            Console.Title = "Transaction coordinator";
            Logger.Path = "Coordinator.txt";
            TransactionCoordinator t = new TransactionCoordinator();
            
            Start();
            Console.ReadKey(true);
            Stop();
        }

        public static void Start()
        {
            svc = new ServiceHost(TransactionCoordinator.Instance);
            var binding = new NetTcpBinding();
            svc.AddServiceEndpoint(typeof(ITransactionDuplexScada), binding, new Uri("net.tcp://localhost:10002/TransactionCoordinator/Scada"));

            svc3 = new ServiceHost(TransactionCoordinator.Instance);
            svc3.AddServiceEndpoint(typeof(ITransactionDuplexNMS), new NetTcpBinding(), new Uri("net.tcp://localhost:10003/TransactionCoordinator/NMS"));


            svc2 = new ServiceHost(TransactionCoordinator.Instance);
            svc2.AddServiceEndpoint(typeof(ITransactionCoordinator),
                                    new NetTcpBinding(),
                                    new Uri("net.tcp://localhost:10001/TransactionCoordinator/Adapter"));

            svc4 = new ServiceHost(TransactionCoordinator.Instance);
            svc4.AddServiceEndpoint(typeof(ITransactionDuplexCE), new NetTcpBinding(), new Uri("net.tcp://localhost:10103/TransactionCoordinator/CE"));


            svc.Open();
            svc2.Open();
            svc3.Open();
            svc4.Open();
            Console.WriteLine("Transaction waiting for request..");
        }

        public static void Stop()
        {
            svc.Close();
            svc2.Close();
            svc3.Close();
            svc4.Close();
            Console.WriteLine("Transaction server stopped");
        }
    }
}
