using CalculationEngine.Access;
using FTN.Common;
using FTN.Common.Logger;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Wires;

namespace CalculationEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CalculationEngine : ICalculationEngine, ICalculationDuplexClient
    {
        private static CalculationEngine instance;
        private ITransactionDuplexCE proxyCoordinator;
        private List<IModelForDuplex> clients;
        private List<IModelForDuplex> clientsForDeleting;
        private List<ResourceDescription> meas;
        private bool firstTimeCoordinator = true;
        private Delta delta;
        private FunctionDB dataBaseAdapter;

        public CalculationEngine()
        {
            clients = new List<IModelForDuplex>();
            clientsForDeleting = new List<IModelForDuplex>();
            dataBaseAdapter = new FunctionDB();

            meas = new List<ResourceDescription>();

            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE try to connect with Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    this.ProxyCoordinator.ConnectCE();
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE is connected to the Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE faild to connect with Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    firstTimeCoordinator = true;
                    Thread.Sleep(1000);
                }
            }
        }

        public static CalculationEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CalculationEngine();
                }

                return instance;
            }
        }

        public ITransactionDuplexCE ProxyCoordinator
        {
            get
            {
                if (firstTimeCoordinator)
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine.ProxyCoordinator; line: {0}; Create channel between CE and Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    DuplexChannelFactory<ITransactionDuplexCE> factory = new DuplexChannelFactory<ITransactionDuplexCE>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10103/TransactionCoordinator/CE"));
                    proxyCoordinator = factory.CreateChannel();
                    firstTimeCoordinator = false;
                }
                Logger.LogMessageToFile(string.Format("CE.CalculationEngine.ProxyCoordinator; line: {0}; Channel CE-Coordinator is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return proxyCoordinator;
            }

            set
            {
                proxyCoordinator = value;
            }
        }

        public void ConnectClient()
        {
            this.clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());
        }

        public void EnlistMeas(List<ResourceDescription> measurements)
        {
            foreach(ResourceDescription rd in measurements)
            {
                meas.Add(rd);
            }
        }

        public bool Prepare()
        {
            return true;
        }

        public void Commit()
        {
            foreach(ResourceDescription rd in meas)
            {
                EnergyConsumer ec = new EnergyConsumer();
                ec.RD2Class(rd);
                DynamicMeasurement newMeas = new DynamicMeasurement(rd.Id);
                newMeas.OperationType = OperationType.INSERT;
                dataBaseAdapter.AddMeasurement(newMeas);
            }
            this.meas.Clear();
        }

        public void Rollback()
        {
            this.meas.Clear();
        }

        public void DataFromScada(List<DynamicMeasurement> measurements)
        {
            Logger.LogMessageToFile(string.Format("CE.CalculationEngine.DataFromScada; line: {0}; CE receive data from scada and send this data to client", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            Console.WriteLine("Receive data from SCADA");

            Console.WriteLine("Send data to client");
            foreach(DynamicMeasurement dm in measurements)
            {
                dm.OperationType = OperationType.UPDATE;
            }

            dataBaseAdapter.AddMeasurements(measurements);

            clientsForDeleting.Clear();
            foreach (IModelForDuplex client in clients)
            {
                try
                {
                    client.SendMeasurements(measurements);
                }
                catch
                {
                    clientsForDeleting.Add(client);
                }
            }

            foreach (IModelForDuplex client in clientsForDeleting)
            {
                clients.Remove(client);
            }
            Logger.LogMessageToFile(string.Format("CE.CalculationEngine.DataFromScada; line: {0}; Finish transport data SCADA-CE-Client", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }
    }
}
