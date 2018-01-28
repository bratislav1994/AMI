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
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            binding.MaxBufferSize = Int32.MaxValue;
            svc.AddServiceEndpoint(typeof(ITransactionDuplexScada), binding, new Uri("net.tcp://localhost:10002/TransactionCoordinator/Scada"));

            svc3 = new ServiceHost(TransactionCoordinator.Instance);
            NetTcpBinding binding4 = new NetTcpBinding();
            binding4.MaxReceivedMessageSize = Int32.MaxValue;
            binding4.MaxBufferSize = Int32.MaxValue;
            svc3.AddServiceEndpoint(typeof(ITransactionDuplexNMS), binding4, new Uri("net.tcp://localhost:10003/TransactionCoordinator/NMS"));


            svc2 = new ServiceHost(TransactionCoordinator.Instance);
            NetTcpBinding binding2 = new NetTcpBinding();
            binding2.MaxReceivedMessageSize = Int32.MaxValue;
            binding2.MaxBufferSize = Int32.MaxValue;
            svc2.AddServiceEndpoint(typeof(ITransactionCoordinator),
                                    binding2,
                                    new Uri("net.tcp://localhost:10001/TransactionCoordinator/Adapter"));

            svc4 = new ServiceHost(TransactionCoordinator.Instance);
            NetTcpBinding binding3 = new NetTcpBinding();
            binding3.MaxReceivedMessageSize = Int32.MaxValue;
            binding3.MaxBufferSize = Int32.MaxValue;
            svc4.AddServiceEndpoint(typeof(ITransactionDuplexCE), binding3, new Uri("net.tcp://localhost:10103/TransactionCoordinator/CE"));


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
