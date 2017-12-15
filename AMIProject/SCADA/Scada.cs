using AMISimulator;
using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;
using FTN.Services.NetworkModelService;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Threading;
using FTN.Common.Logger;
using FTN.Services.NetworkModelService.DataModel;

namespace SCADA
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Scada : IScada, IScadaDuplexSimulator, IDisposable
    {
        bool firstTimeSimulator = true;
        bool firstTimeCoordinator = true;
        bool firstTimeCE = true;
        Dictionary<int, ResourceDescription> measurements;
        SOEHandler handler;
        IDNP3Manager mgr;
        ITransactionDuplexScada proxyCoordinator;
        int cnt = 0;
        private List<ResourceDescription> measurementsToEnlist;
        private Dictionary<int, ResourceDescription> copyMeasurements;
        private List<DynamicMeasurement> resourcesToSend;
        private object lockObject = new object();
        private Thread sendingThread;
        ICalculationEngine proxyCE;
        private List<ISimulator> simulators;
        
        public ITransactionDuplexScada ProxyCoordinator
        {
            get
            {
                if (firstTimeCoordinator)
                {
                    Logger.LogMessageToFile(string.Format("SCADA.Scada.ProxyCoordinator; line: {0}; Create channel between Scada and Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    DuplexChannelFactory<ITransactionDuplexScada> factory = new DuplexChannelFactory<ITransactionDuplexScada>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10002/TransactionCoordinator/Scada"));
                    proxyCoordinator = factory.CreateChannel();
                    firstTimeCoordinator = false;
                }
                Logger.LogMessageToFile(string.Format("SCADA.Scada.ProxyCoordinator; line: {0}; Channel SCADA-Coordinator is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return proxyCoordinator;
            }

            set
            {
                proxyCoordinator = value;
            }
        }

        public ICalculationEngine ProxyCE
        {
            get
            {
                if (firstTimeCE)
                {
                    Logger.LogMessageToFile(string.Format("SCADA.Scada.ProxyCE; line: {0}; Create channel between Scada and CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    ChannelFactory<ICalculationEngine> factory = new ChannelFactory<ICalculationEngine>(new NetTcpBinding(),
                                                                                        new EndpointAddress("net.tcp://localhost:10050/ICalculationEngine/Calculation"));
                    proxyCE = factory.CreateChannel();

                    firstTimeCE = false;
                }
                Logger.LogMessageToFile(string.Format("SCADA.Scada.ProxyCE; line: {0}; Channel SCADA-CE is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return proxyCE;
            }

            set
            {
                proxyCE = value;
            }
        }

        public Scada()
        {
            measurements = new Dictionary<int, ResourceDescription>();
            copyMeasurements = new Dictionary<int, ResourceDescription>();
            resourcesToSend = new List<DynamicMeasurement>();
            simulators = new List<ISimulator>();
            handler = new SOEHandler(ref measurements, resourcesToSend, ref lockObject);
            mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());
            var channel = mgr.AddTCPClient("outstation", LogLevels.NORMAL | LogLevels.APP_COMMS, ChannelRetry.Default, "127.0.0.1", 20000, ChannelListener.Print());

            var config = new MasterStackConfig();
            config.link.localAddr = 1;
            config.link.remoteAddr = 10;

            var master = channel.AddMaster("master", handler, DefaultMasterApplication.Instance, config);
            config.master.disableUnsolOnStartup = false;

            var integrityPoll = master.AddClassScan(ClassField.AllClasses, TimeSpan.MaxValue, TaskConfig.Default);
            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("SCADA.Scada; line: {0}; Scada try to connect with Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    ProxyCoordinator.ConnectScada();
                    Logger.LogMessageToFile(string.Format("SCADA.Scada; line: {0}; Scada is connected to the Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("SCADA.Scada; line: {0}; Scada faild to connect with Coordinator. CATCH", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    firstTimeCoordinator = true;
                    Thread.Sleep(1000);
                }
            }

            Deserialize(measurements);

            master.Enable();
            sendingThread = new Thread(() => CheckIfThereIsSomethingToSned());
            sendingThread.Start();

        }

        public void StartIssueCommands()
        {
            /*Console.WriteLine("Enter a command");

            while (true)
            {
                switch (Console.ReadLine())
                {
                    case "a":
                        ProxySimulator.AddMeasurement();
                        break;
                    case "x":
                        return;
                    default:
                        break;
                }
            }*/
        }

        public void EnlistMeas(List<ResourceDescription> meas)
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Start the EnlistMeas function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            this.measurementsToEnlist = meas;
            foreach (KeyValuePair<int, ResourceDescription> kvp in this.measurements)
            {
                this.copyMeasurements.Add(kvp.Key, kvp.Value);
            }
            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Finish the EnlistMeas function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public bool Prepare()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Start the Prepare function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ResourceDescription> Ps = new List<ResourceDescription>();
            List<ResourceDescription> Qs = new List<ResourceDescription>();
            List<ResourceDescription> Vs = new List<ResourceDescription>();

            if(this.simulators.Count == 0)
            {
                return false;
            }

            foreach (ResourceDescription rd in measurementsToEnlist)
            {
                TC57CIM.IEC61970.Meas.Analog m = new TC57CIM.IEC61970.Meas.Analog();
                m.RD2Class(rd);

                switch (m.UnitSymbol)
                {
                    case UnitSymbol.P:
                        Ps.Add(rd);
                        break;
                    case UnitSymbol.Q:
                        Qs.Add(rd);
                        break;
                    case UnitSymbol.V:
                        Vs.Add(rd);
                        break;
                    default:
                        return false;
                }
            }

            if (Ps.Count != Qs.Count || Ps.Count != Vs.Count || Qs.Count != Vs.Count)
            {
                return false;
            }

            for (int i = 0; i < Ps.Count; i++)
            {
                int index = simulators[0].AddMeasurement();
                if (index != -1)
                {
                    this.copyMeasurements.Add(index, Ps[i]);
                    ++cnt;
                }
                else
                {
                    return false;
                }
                index = simulators[0].AddMeasurement();
                if (index != -1)
                {
                    this.copyMeasurements.Add(index, Qs[i]);
                    ++cnt;
                }
                else
                {
                    return false;
                }
                
                index = simulators[0].AddMeasurement();
                if (index != -1)
                {
                    this.copyMeasurements.Add(index, Vs[i]);
                    ++cnt;
                }
                else
                {
                    return false;
                }
            }
            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Finish the EnlistMeas function successful", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            return true;
        }

        public void Commit()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Commit; line: {0}; Start the Commit function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            this.measurements.Clear();
            foreach (KeyValuePair<int, ResourceDescription> kvp in this.copyMeasurements)
            {
                this.measurements.Add(kvp.Key, kvp.Value);
            }
            Serialize(measurements);
            cnt = 0;
            this.copyMeasurements.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Commit; line: {0}; Finish the Commit function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public void Rollback()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Start the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            this.simulators[0].Rollback(cnt);
            cnt = 0;
            this.copyMeasurements.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Finish the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private void CheckIfThereIsSomethingToSned()
        {
            while (true)
            {
                lock(lockObject)
                {
                    if (handler.resourcesToSend.Count > 0)
                    {
                        Logger.LogMessageToFile(string.Format("SCADA.Scada.CheckIfThereIsSomethingToSned; line: {0}; Scada sends data to client if it has data to send", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                        Console.WriteLine("List changed, here will be code for sending measurements to Calculation Engine...");
                        ProxyCE.DataFromScada(handler.resourcesToSend);

                        handler.resourcesToSend.Clear();
                    }
                }

                Thread.Sleep(200);
            }
        }

        public void Dispose()
        {
            sendingThread.Abort();
        }

        private void Serialize(Dictionary<int, ResourceDescription> measurements)
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Serialize; line: {0}; Start the Serialize function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            TextWriter tw = new StreamWriter("Measurements.xml");
            List<ClassForSerialization> meas = new List<ClassForSerialization>(measurements.Count);
            foreach(int key in measurements.Keys)
            {
                meas.Add(new ClassForSerialization(key, measurements[key]));
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<ClassForSerialization>));
            serializer.Serialize(tw, meas);
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Serialize; line: {0}; Finish the Serialize function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private void Deserialize(Dictionary<int, ResourceDescription> measurements)
        {
            try
            {
                Logger.LogMessageToFile(string.Format("SCADA.Scada.Deserialize; line: {0}; Start the Deserialize function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

                TextReader tr = new StreamReader(File.OpenRead("Measurements.xml"));
                measurements.Clear();
                XmlSerializer serializer = new XmlSerializer(typeof(List<ClassForSerialization>));
                List<ClassForSerialization> meas = (List<ClassForSerialization>)serializer.Deserialize(tr);

                foreach (ClassForSerialization cfs in meas)
                {
                    measurements.Add(cfs.Key, cfs.Rd);
                }
                Logger.LogMessageToFile(string.Format("SCADA.Scada.Deserialize; line: {0}; Finish the Deserialize function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            }
            catch
            {
                Logger.LogMessageToFile(string.Format("SCADA.Scada.Deserialize; line: {0}; Deserialization failed", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            }
        }

        public void Connect()
        {
            this.simulators.Add(OperationContext.Current.GetCallbackChannel<ISimulator>());
        }

        public int GetNumberOfPoints()
        {
            return this.measurements.Count();
        }
    }
}
