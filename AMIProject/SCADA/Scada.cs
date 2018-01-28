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
using TC57CIM.IEC61970.Wires;

namespace SCADA
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Scada : IScada, IScadaDuplexSimulator, IDisposable
    {
        private Dictionary<int, RTUAddress> addressPool;
        private bool firstTimeCoordinator = true;
        private bool firstTimeCE = true;
        private Dictionary<int, List<MeasurementForScada>> measurements;
        private Dictionary<int, SOEHandler> handlers;
        private ITransactionDuplexScada proxyCoordinator;
        private List<ResourceDescription> measurementsToEnlist;
        private List<ResourceDescription> consumersToEnlist;
        private Dictionary<int, List<MeasurementForScada>> copyMeasurements;
        private Dictionary<long, EnergyConsumerForScada> enConsumers;
        private Dictionary<long, EnergyConsumerForScada> copyEnConsumers;
        private Dictionary<int, List<EnergyConsumerForScada>> energyConsumersByRtu;
        private Dictionary<int, List<EnergyConsumerForScada>> copyEnergyConsumersByRtu;
        private Dictionary<long, DynamicMeasurement> resourcesToSend;
        private object lockObject = new object();
        private Thread sendingThread;
        private ICalculationEngine proxyCE;
        private Dictionary<int, ISimulator> simulators;
        private object lockObjectForSimulators = new object();
        private const int startRtuAddress = 10;
        private const int maxRtuAddress = 20;
        private FunctionDB f = new FunctionDB();
        private Dictionary<int, IMaster> masters;
        private Dictionary<int, IChannel> channels;
        private Dictionary<int, IDNP3Manager> managers;
        private Dictionary<int, List<long>> conGidsForSimulator;

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

            energyConsumersByRtu = new Dictionary<int, List<EnergyConsumerForScada>>();
            copyEnergyConsumersByRtu = new Dictionary<int, List<EnergyConsumerForScada>>();
            conGidsForSimulator = new Dictionary<int, List<long>>();
            handlers = new Dictionary<int, SOEHandler>();
            enConsumers = new Dictionary<long, EnergyConsumerForScada>();
            copyEnConsumers = new Dictionary<long, EnergyConsumerForScada>();
            measurements = new Dictionary<int, List<MeasurementForScada>>();
            copyMeasurements = new Dictionary<int, List<MeasurementForScada>>();
            resourcesToSend = new Dictionary<long, DynamicMeasurement>();
            simulators = new Dictionary<int, ISimulator>();
            masters = new Dictionary<int, IMaster>();
            channels = new Dictionary<int, IChannel>();
            managers = new Dictionary<int, IDNP3Manager>();
            measurementsToEnlist = new List<ResourceDescription>();
            consumersToEnlist = new List<ResourceDescription>();

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

            this.ReadDataFromDB();
            sendingThread = new Thread(() => CheckIfThereIsSomethingToSned());
            sendingThread.Start();
        }

        public void EnlistMeas(List<ResourceDescription> data)
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Start the EnlistMeas function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            measurementsToEnlist.Clear();
            consumersToEnlist.Clear();

            foreach (ResourceDescription rd in data)
            {
                DMSType type = (DMSType)(rd.Id >> 32);

                switch (type)
                {
                    case DMSType.ANALOG:
                        measurementsToEnlist.Add(rd);
                        break;
                    case DMSType.ENERGYCONS:
                        consumersToEnlist.Add(rd);
                        break;
                }
            }

            foreach (KeyValuePair<int, List<MeasurementForScada>> kvp in this.measurements)
            {
                this.copyMeasurements.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, EnergyConsumerForScada> kvp in this.enConsumers)
            {
                this.copyEnConsumers.Add(kvp.Key, kvp.Value);
            }

            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Finish the EnlistMeas function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public bool Prepare()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Prepare; line: {0}; Start the Prepare function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<Measurement> Ps = new List<Measurement>();
            List<Measurement> Qs = new List<Measurement>();
            List<Measurement> Vs = new List<Measurement>();
            Dictionary<long, EnergyConsumerForScada> ConsToSimulator = new Dictionary<long, EnergyConsumerForScada>();

            lock (lockObjectForSimulators)
            {
                if (this.simulators.Count == 0)
                {
                    return false;
                }
            }

            foreach (ResourceDescription rd in consumersToEnlist)
            {
                EnergyConsumer ec = new EnergyConsumer();
                ec.RD2Class(rd);
                ec.GlobalId = rd.Id;
                EnergyConsumerForScada ecDb = new EnergyConsumerForScada(ec);
                ConsToSimulator.Add(ecDb.GlobalId, ecDb);
            }

            foreach (ResourceDescription rd in measurementsToEnlist)
            {
                TC57CIM.IEC61970.Meas.Analog m = new TC57CIM.IEC61970.Meas.Analog();
                m.RD2Class(rd);
                m.GlobalId = rd.Id;

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
                    lock (lockObjectForSimulators)
                    {
                        index = simulators[Ps[i].RtuAddress].AddMeasurement(Ps[i]);

                        if (!simulators[Ps[i].RtuAddress].AddConsumer(ConsToSimulator[Ps[i].PowerSystemResourceRef]))
                        {
                            return false;
                        }
                        else
                        {
                            if (!conGidsForSimulator.ContainsKey(Ps[i].RtuAddress))
                            {
                                conGidsForSimulator.Add(Ps[i].RtuAddress, new List<long>());
                            }

                            if (!copyEnergyConsumersByRtu.ContainsKey(Ps[i].RtuAddress))
                            {
                                copyEnergyConsumersByRtu.Add(Ps[i].RtuAddress, new List<EnergyConsumerForScada>());
                            }

                            conGidsForSimulator[Ps[i].RtuAddress].Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].GlobalId);
                            copyEnConsumers.Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].GlobalId, ConsToSimulator[Ps[i].PowerSystemResourceRef]);
                            copyEnergyConsumersByRtu[Ps[i].RtuAddress].Add(ConsToSimulator[Ps[i].PowerSystemResourceRef]);
                        }
                    }
                }
                catch (Exception)
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
                    lock (lockObjectForSimulators)
                    {
                        index = simulators[Qs[i].RtuAddress].AddMeasurement(Qs[i]);
                    }
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
                    lock (lockObjectForSimulators)
                    {
                        index = simulators[Vs[i].RtuAddress].AddMeasurement(Vs[i]);
                    }
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

            Logger.LogMessageToFile(string.Format("SCADA.Scada.Prepare; line: {0}; Finish the Prepare function successful", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            return true;
        }

        public void Commit()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Commit; line: {0}; Start the Commit function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            this.measurements.Clear();
            this.enConsumers.Clear();
            this.energyConsumersByRtu.Clear();

            foreach (KeyValuePair<long, EnergyConsumerForScada> kvp in this.copyEnConsumers)
            {
                this.enConsumers.Add(kvp.Key, kvp.Value);
                f.AddConsumers(kvp.Value);
            }

            foreach (KeyValuePair<int, List<EnergyConsumerForScada>> kvp in this.copyEnergyConsumersByRtu)
            {
                this.energyConsumersByRtu.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<int, List<MeasurementForScada>> kvp in this.copyMeasurements)
            {
                this.measurements.Add(kvp.Key, kvp.Value);
                f.AddMeasurement(kvp.Value);
            }

            foreach (KeyValuePair<int, RTUAddress> kvp in addressPool)
            {
                kvp.Value.Cnt = 0;
            }

            this.copyEnergyConsumersByRtu.Clear();
            this.copyMeasurements.Clear();
            this.copyEnConsumers.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Commit; line: {0}; Finish the Commit function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public void Rollback()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Start the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            foreach (KeyValuePair<int, RTUAddress> kvp in addressPool)
            {
                lock (lockObjectForSimulators)
                {
                    if (this.simulators.ContainsKey(kvp.Key))
                    {
                        this.simulators[kvp.Key].Rollback(addressPool[kvp.Key].Cnt, conGidsForSimulator[kvp.Key]);
                        kvp.Value.Cnt = 0;
                    }
                }
            }

            this.copyEnergyConsumersByRtu.Clear();
            this.copyMeasurements.Clear();
            this.copyEnConsumers.Clear();
            conGidsForSimulator.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Finish the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private void CheckIfThereIsSomethingToSned()
        {
            while (true)
            {
                lock (lockObject)
                {
                    foreach (SOEHandler handler in handlers.Values)
                    {
                        if (handler.HasNewMeas)
                        {
                            Logger.LogMessageToFile(string.Format("SCADA.Scada.CheckIfThereIsSomethingToSned; line: {0}; Scada sends data to client if it has data to send", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                            Console.WriteLine("List changed, here will be code for sending measurements to Calculation Engine...");
                            ProxyCE.DataFromScada(handler.resourcesToSend);
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

        public void ReadDataFromDB()
        {
            try
            {
                measurements.Clear();
                List<WrapperDB> meass = f.ReadMeas();

                foreach (WrapperDB wDB in meass)
                {
                    measurements.Add(wDB.RtuAddress, wDB.ListOfMeasurements);
                }

                enConsumers.Clear();
                List<EnergyConsumerForScada> conss = f.ReadConsumers();
                conss.ForEach(x => enConsumers.Add(x.GlobalId, x));
                List<long> writtenConsumers = new List<long>();

                foreach (KeyValuePair<int, List<MeasurementForScada>> kvp in measurements)
                {
                    foreach (MeasurementForScada m in kvp.Value)
                    {
                        if (!energyConsumersByRtu.ContainsKey(kvp.Key))
                        {
                            energyConsumersByRtu.Add(kvp.Key, new List<EnergyConsumerForScada>());
                        }

                        if (!writtenConsumers.Contains(enConsumers[m.Measurement.PowerSystemResourceRef].GlobalId))
                        {
                            energyConsumersByRtu[kvp.Key].Add(enConsumers[m.Measurement.PowerSystemResourceRef]);
                            writtenConsumers.Add(enConsumers[m.Measurement.PowerSystemResourceRef].GlobalId);
                        }
                    }
                }
            }
            catch (Exception)
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

                    lock (lockObjectForSimulators)
                    {
                        this.simulators.Add(kvp.Key, OperationContext.Current.GetCallbackChannel<ISimulator>());
                    }

                    addressPool[ret].IsConnected = true;
                    break;
                }
                else
                {
                    try
                    {
                        lock (lockObjectForSimulators)
                        {
                            this.simulators[kvp.Key].Ping();
                        }
                    }
                    catch
                    {
                        lock (lockObjectForSimulators)
                        {
                            this.simulators.Remove(kvp.Key);
                        }

                        lock (lockObject)
                        {
                            handlers.Remove(kvp.Key);
                            channels[kvp.Key].Shutdown();
                            channels.Remove(kvp.Key);
                            masters[kvp.Key].Disable();
                            masters.Remove(kvp.Key);
                            managers[kvp.Key].Shutdown();
                            managers.Remove(kvp.Key);
                        }

                        ret = kvp.Key;

                        lock (lockObjectForSimulators)
                        {
                            this.simulators.Add(kvp.Key, OperationContext.Current.GetCallbackChannel<ISimulator>());
                        }

                        break;
                    }
                }
            }

            if (ret != 0)
            {
                if (!measurements.ContainsKey(ret))
                {
                    measurements.Add(ret, new List<MeasurementForScada>());
                }

                var handler = new SOEHandler(measurements[ret], resourcesToSend, ref lockObject);
                var mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());
                var channel = mgr.AddTCPClient("outstation" + ret, LogLevels.NORMAL | LogLevels.APP_COMMS, ChannelRetry.Default, "127.0.0.1", (ushort)(20000 + ret), ChannelListener.Print());
                var config = new MasterStackConfig();
                config.link.localAddr = 1;
                config.link.remoteAddr = (ushort)ret;
                var master = channel.AddMaster("master" + ret, handler, DefaultMasterApplication.Instance, config);

                lock (lockObject)
                {
                    handlers.Add(ret, handler);
                    masters.Add(ret, master);
                    channels.Add(ret, channel);
                    managers.Add(ret, mgr);
                }

                config.master.disableUnsolOnStartup = false;
                var integrityPoll = master.AddClassScan(ClassField.AllClasses, TimeSpan.MaxValue, TaskConfig.Default);
                master.Enable();
                f.AddSimulator(new WrapperDB(ret));
            }

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

        public List<EnergyConsumerForScada> GetConsumersFromScada(int rtuAddress)
        {
            List<EnergyConsumerForScada> retVal = new List<EnergyConsumerForScada>();

            try
            {
                foreach (EnergyConsumerForScada m in energyConsumersByRtu[rtuAddress])
                {
                    retVal.Add(m);
                }
            }
            catch (KeyNotFoundException)
            {

            }

            return retVal;
        }
    }
}
