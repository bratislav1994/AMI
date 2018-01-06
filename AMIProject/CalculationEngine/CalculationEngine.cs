using CalculationEngine.Access;
using FTN.Common;
using FTN.Common.Logger;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
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

        public Tuple<List<DynamicMeasurement>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, DateTime to)
        {
            List<DynamicMeasurement> result = dataBaseAdapter.GetMeasForChart(gids, from, to);
            DateTime min = result.Min(x => x.TimeStamp);
            DateTime max = result.Max(x => x.TimeStamp);
            Dictionary<DateTime, DynamicMeasurement> temp = new Dictionary<DateTime, DynamicMeasurement>();
            
            for(DateTime t=min; t<=max;t = t.AddSeconds(3))
            {
                temp.Add(t, new DynamicMeasurement());
                temp[t].CurrentP = 0;
                temp[t].CurrentQ = 0;
                temp[t].CurrentV = 0;
                temp[t].TimeStamp = t;
            }

            if(!temp.ContainsKey(max))
            {
                temp.Add(max, new DynamicMeasurement());
                temp[max].CurrentP = 0;
                temp[max].CurrentQ = 0;
                temp[max].CurrentV = 0;
                temp[max].TimeStamp = max;
            }

            foreach(DynamicMeasurement dm in result)
            {
                DateTime dt = temp.Keys.Where(x => (Math.Abs((x - dm.TimeStamp).TotalSeconds) < 1.5)).FirstOrDefault();
                temp[dt].CurrentP += dm.CurrentP;
                temp[dt].CurrentQ += dm.CurrentQ;
                temp[dt].CurrentV += dm.CurrentV;
            }

            List<DynamicMeasurement> retVal = temp.Values.ToList();

            Statistics statistics = new Statistics();
            statistics.MaxP = retVal.Max(x => x.CurrentP);
            statistics.MaxQ = retVal.Max(x => x.CurrentQ);
            statistics.MaxV = retVal.Max(x => x.CurrentV);
            statistics.MinP = retVal.Min(x => x.CurrentP);
            statistics.MinQ = retVal.Min(x => x.CurrentQ);
            statistics.MinV = retVal.Min(x => x.CurrentV);
            statistics.AvgP = retVal.Average(x => x.CurrentP);
            statistics.AvgQ = retVal.Average(x => x.CurrentQ);
            statistics.AvgV = retVal.Average(x => x.CurrentV);
            statistics.IntegralP = 0;
            statistics.IntegralQ = 0;
            statistics.IntegralV = 0;

            for(int i=0;i< retVal.Count - 1;i++)
            {
                statistics.IntegralP += (retVal[i].CurrentP * (((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds)) / 3600) + ((((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(retVal[i + 1].CurrentP - retVal[i].CurrentP))) / 2;
                statistics.IntegralQ += (retVal[i].CurrentQ * (((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds)) / 3600) + ((((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(retVal[i + 1].CurrentQ - retVal[i].CurrentQ))) / 2;
                statistics.IntegralV += (retVal[i].CurrentV * (((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds)) / 3600) + ((((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(retVal[i + 1].CurrentV - retVal[i].CurrentV))) / 2;
            }
            
            return new Tuple<List<DynamicMeasurement>, Statistics>(retVal, statistics);
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
                DynamicMeasurement newMeas = new DynamicMeasurement(rd.Id, DateTime.Now);
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
