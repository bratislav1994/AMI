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
using SCADA.Access;

namespace SCADA
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Scada : IScada, IScadaDuplexSimulator, IDisposable
    {
        private Dictionary<int, RTUAddress> addressPool;
        private bool firstTimeCoordinator = true;
        private bool firstTimeCE = true;
        private Dictionary<int, List<MeasurementForScada>> measurements;
        private List<SOEHandler> handlers;
        private IDNP3Manager mgr;
        private ITransactionDuplexScada proxyCoordinator;
        private List<ResourceDescription> measurementsToEnlist;
        private Dictionary<int, List<MeasurementForScada>> copyMeasurements;
        private List<DynamicMeasurement> resourcesToSend;
        private object lockObject = new object();
        private Thread sendingThread;
        private ICalculationEngine proxyCE;
        private Dictionary<int, ISimulator> simulators;
        private const int startRtuAddress = 10;
        private const int maxRtuAddress = 20;
        private FunctionDB f = new FunctionDB();

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
            addressPool = new Dictionary<int, RTUAddress>();

            for (int i = startRtuAddress; i <= maxRtuAddress; i++)
            {
                addressPool.Add(i, new RTUAddress() { IsConnected = false, Cnt = 0 });
            }

            handlers = new List<SOEHandler>();
            measurements = new Dictionary<int, List<MeasurementForScada>>();
            copyMeasurements = new Dictionary<int, List<MeasurementForScada>>();
            resourcesToSend = new List<DynamicMeasurement>();
            simulators = new Dictionary<int, ISimulator>();
            mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());

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

            ReadDataFromDB(measurements);
            //Deserialize(measurements);
            sendingThread = new Thread(() => CheckIfThereIsSomethingToSned());
            sendingThread.Start();
        }

        public void EnlistMeas(List<ResourceDescription> meas)
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Start the EnlistMeas function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            this.measurementsToEnlist = meas;

            foreach (KeyValuePair<int, List<MeasurementForScada>> kvp in this.measurements)
            {
                this.copyMeasurements.Add(kvp.Key, kvp.Value);
            }

            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Finish the EnlistMeas function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public bool Prepare()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Prepare; line: {0}; Start the Prepare function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<Measurement> Ps = new List<Measurement>();
            List<Measurement> Qs = new List<Measurement>();
            List<Measurement> Vs = new List<Measurement>();

            if (this.simulators.Count == 0)
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
                        Ps.Add(m);
                        break;
                    case UnitSymbol.Q:
                        Qs.Add(m);
                        break;
                    case UnitSymbol.V:
                        Vs.Add(m);
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
                int index = -1;

                try
                {
                    index = simulators[Ps[i].RtuAddress].AddMeasurement(Ps[i]);
                }
                catch (Exception e)
                {
                    addressPool[Ps[i].RtuAddress].IsConnected = false;
                    return false;
                }

                int a = f.GetWrapperId(Ps[i].RtuAddress);
                if (index != -1 && a != -1)
                {
                    if (!this.copyMeasurements.ContainsKey(Ps[i].RtuAddress))
                    {
                        this.copyMeasurements.Add(Ps[i].RtuAddress, new List<MeasurementForScada>());
                    }

                    this.copyMeasurements[Ps[i].RtuAddress].Add(new MeasurementForScada(a) { Index = index, Measurement = Ps[i] });
                    ++addressPool[Ps[i].RtuAddress].Cnt;
                }
                else
                {
                    return false;
                }

                try
                {
                    index = simulators[Qs[i].RtuAddress].AddMeasurement(Qs[i]);
                }
                catch
                {
                    addressPool[Qs[i].RtuAddress].IsConnected = false;
                    return false;
                }

                a = f.GetWrapperId(Qs[i].RtuAddress);
                if (index != -1 && a != -1)
                {
                    if (!this.copyMeasurements.ContainsKey(Qs[i].RtuAddress))
                    {
                        this.copyMeasurements.Add(Qs[i].RtuAddress, new List<MeasurementForScada>());
                    }

                    this.copyMeasurements[Qs[i].RtuAddress].Add(new MeasurementForScada(a) { Index = index, Measurement = Qs[i] });
                    ++addressPool[Qs[i].RtuAddress].Cnt;
                }
                else
                {
                    return false;
                }

                try
                {
                    index = simulators[Vs[i].RtuAddress].AddMeasurement(Vs[i]);
                }
                catch
                {
                    addressPool[Vs[i].RtuAddress].IsConnected = false;
                    return false;
                }

                a = f.GetWrapperId(Vs[i].RtuAddress);
                if (index != -1 && a != -1)
                {
                    if (!this.copyMeasurements.ContainsKey(Vs[i].RtuAddress))
                    {
                        this.copyMeasurements.Add(Vs[i].RtuAddress, new List<MeasurementForScada>());
                    }

                    this.copyMeasurements[Vs[i].RtuAddress].Add(new MeasurementForScada(a) { Index = index, Measurement = Vs[i] });
                    ++addressPool[Vs[i].RtuAddress].Cnt;
                }
                else
                {
                    return false;
                }
            }

            Logger.LogMessageToFile(string.Format("SCADA.Scada.Prepare; line: {0}; Finish the EnlistMeas function successful", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            return true;
        }

        public void Commit()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Commit; line: {0}; Start the Commit function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            this.measurements.Clear();

            foreach (KeyValuePair<int, List<MeasurementForScada>> kvp in this.copyMeasurements)
            {
                this.measurements.Add(kvp.Key, kvp.Value);
                f.AddMeasurement(kvp.Value);
            }

            //Serialize(measurements);
            foreach (KeyValuePair<int, RTUAddress> kvp in addressPool)
            {
                kvp.Value.Cnt = 0;
            }

            this.copyMeasurements.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Commit; line: {0}; Finish the Commit function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public void Rollback()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Start the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            foreach (KeyValuePair<int, RTUAddress> kvp in addressPool)
            {
                this.simulators[kvp.Key].Rollback(addressPool[kvp.Key].Cnt);
                kvp.Value.Cnt = 0;
            }

            this.copyMeasurements.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Finish the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private void CheckIfThereIsSomethingToSned()
        {
            while (true)
            {
                lock (lockObject)
                {
                    foreach (SOEHandler handler in handlers)
                    {
                        if (handler.HasNewMeas)
                        {
                            Logger.LogMessageToFile(string.Format("SCADA.Scada.CheckIfThereIsSomethingToSned; line: {0}; Scada sends data to client if it has data to send", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                            Console.WriteLine("List changed, here will be code for sending measurements to Calculation Engine...");
                            ProxyCE.DataFromScada(handler.resourcesToSend);
                            handler.resourcesToSend.Clear();
                            handler.HasNewMeas = false;
                        }
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

            foreach (int key in measurements.Keys)
            {
                meas.Add(new ClassForSerialization(key, measurements[key]));
            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<ClassForSerialization>));
            serializer.Serialize(tw, meas);
            tw.Close();
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
                tr.Close();

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

        public void ReadDataFromDB(Dictionary<int, List<MeasurementForScada>> measurements)
        {
            try
            {
                measurements.Clear();
                List<WrapperDB> meass = f.ReadMeas();

                foreach (WrapperDB wDB in meass)
                {
                    measurements.Add(wDB.RtuAddress, wDB.ListOfMeasurements);
                }
            }
            catch (Exception e)
            {

            }
        }

        public int Connect()
        {
            int ret = 0;

            foreach (KeyValuePair<int, RTUAddress> kvp in addressPool)
            {
                if (!kvp.Value.IsConnected)
                {
                    ret = kvp.Key;
                    this.simulators.Add(kvp.Key, OperationContext.Current.GetCallbackChannel<ISimulator>());
                    addressPool[ret].IsConnected = true;
                    break;
                }
            }

            if (!measurements.ContainsKey(ret))
            {
                measurements.Add(ret, new List<MeasurementForScada>());
            }

            var handler = new SOEHandler(measurements[ret], resourcesToSend, ref lockObject);
            handlers.Add(handler);
            var channel = mgr.AddTCPClient("outstation" + ret, LogLevels.NORMAL | LogLevels.APP_COMMS, ChannelRetry.Default, "127.0.0.1", (ushort)(20000 + ret), ChannelListener.Print());

            var config = new MasterStackConfig();
            config.link.localAddr = 1;
            config.link.remoteAddr = (ushort)ret;

            var master = channel.AddMaster("master", handler, DefaultMasterApplication.Instance, config);
            config.master.disableUnsolOnStartup = false;

            var integrityPoll = master.AddClassScan(ClassField.AllClasses, TimeSpan.MaxValue, TaskConfig.Default);

            master.Enable();

            f.AddSimulator(new WrapperDB(ret));

            return ret;
        }

        public List<MeasurementForScada> GetNumberOfPoints(int rtuAddress)
        {
            List<MeasurementForScada> retVal = new List<MeasurementForScada>();

            foreach (MeasurementForScada m in measurements[rtuAddress])
            {
                retVal.Add(m);
            }

            return retVal;
        }
    }
}
