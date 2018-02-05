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
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using TC57CIM.IEC61970.Core;
using AMIClient.HelperClasses;

namespace SCADA
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Scada : IScada, IScadaDuplexSimulator, IScadaForCECommand, IDisposable
    {
        private Dictionary<int, RTUAddress> addressPool;
        private bool firstTimeCoordinator = true;
        private bool firstTimeCE = true;
        private Dictionary<int, SOEHandler> handlers;
        private ITransactionDuplexScada proxyCoordinator;
        private List<ResourceDescription> measurementsToEnlist;
        private List<ResourceDescription> consumersToEnlist;
        private List<ResourceDescription> ptsToEnlist;
        private List<ResourceDescription> substationsToEnlist;
        private List<ResourceDescription> basevoltagesToEnlist;
        private Dictionary<int, List<MeasurementForScada>> measurements;
        private Dictionary<int, List<MeasurementForScada>> copyMeasurements;
        private Dictionary<long, DataForScada> allData;
        private Dictionary<long, DataForScada> copyAllData;
        private Dictionary<int, List<DataForScada>> allDataByRtu;
        private Dictionary<int, List<DataForScada>> copyAllDataByRtu;
        private Dictionary<long, DynamicMeasurement> resourcesToSend;
        private Dictionary<int, object> lockObjects = new Dictionary<int, object>();
        private Thread sendingThread;
        private ICalculationEngine proxyCE;
        private Dictionary<int, ISimulator> simulators;
        private object lockObjectForSimulators = new object();
        private object lockObject = new object();
        private const int startRtuAddress = 10;
        private const int maxRtuAddress = 20;
        private FunctionDB f = new FunctionDB();
        private Dictionary<int, IMaster> masters;
        private Dictionary<int, IChannel> channels;
        private Dictionary<int, IDNP3Manager> managers;
        private Dictionary<int, List<long>> eqGidsForSimulator;
        private Dictionary<int, List<int>> analogIndexesForSimulator;

        public ITransactionDuplexScada ProxyCoordinator
        {
            get
            {
                if (firstTimeCoordinator)
                {
                    Logger.LogMessageToFile(string.Format("SCADA.Scada.ProxyCoordinator; line: {0}; Create channel between Scada and Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
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
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    ChannelFactory<ICalculationEngine> factory = new ChannelFactory<ICalculationEngine>(binding,
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
            allData = new Dictionary<long, DataForScada>();
            allDataByRtu = new Dictionary<int, List<DataForScada>>();

            for (int i = startRtuAddress; i <= maxRtuAddress; i++)
            {
                addressPool.Add(i, new RTUAddress() { IsConnected = false, Cnt = 0 });
                allDataByRtu.Add(i, new List<DataForScada>());
            }

            handlers = new Dictionary<int, SOEHandler>();
            measurements = new Dictionary<int, List<MeasurementForScada>>();
            copyMeasurements = new Dictionary<int, List<MeasurementForScada>>();
            copyAllData = new Dictionary<long, DataForScada>();
            copyAllDataByRtu = new Dictionary<int, List<DataForScada>>();
            eqGidsForSimulator = new Dictionary<int, List<long>>();
            analogIndexesForSimulator = new Dictionary<int, List<int>>();
            resourcesToSend = new Dictionary<long, DynamicMeasurement>();
            simulators = new Dictionary<int, ISimulator>();
            masters = new Dictionary<int, IMaster>();
            channels = new Dictionary<int, IChannel>();
            managers = new Dictionary<int, IDNP3Manager>();
            measurementsToEnlist = new List<ResourceDescription>();
            consumersToEnlist = new List<ResourceDescription>();
            ptsToEnlist = new List<ResourceDescription>();
            substationsToEnlist = new List<ResourceDescription>();
            basevoltagesToEnlist = new List<ResourceDescription>();

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
            basevoltagesToEnlist.Clear();
            substationsToEnlist.Clear();
            ptsToEnlist.Clear();

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
                    case DMSType.BASEVOLTAGE:
                        basevoltagesToEnlist.Add(rd);
                        break;
                    case DMSType.SUBSTATION:
                        substationsToEnlist.Add(rd);
                        break;
                    case DMSType.POWERTRANSFORMER:
                        ptsToEnlist.Add(rd);
                        break;
                }
            }

            foreach (KeyValuePair<int, List<MeasurementForScada>> kvp in this.measurements)
            {
                this.copyMeasurements.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, DataForScada> kvp in this.allData)
            {
                this.copyAllData.Add(kvp.Key, kvp.Value);
            }

            Logger.LogMessageToFile(string.Format("SCADA.Scada.EnlistMeas; line: {0}; Finish the EnlistMeas function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public bool Prepare()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Prepare; line: {0}; Start the Prepare function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<Measurement> Ps = new List<Measurement>();
            List<Measurement> Qs = new List<Measurement>();
            List<Measurement> Vs = new List<Measurement>();
            List<Measurement> powerTransformerSignal = new List<Measurement>();
            Dictionary<long, EnergyConsumerForScada> ConsToSimulator = new Dictionary<long, EnergyConsumerForScada>();
            Dictionary<long, PowerTransformerForScada> PtsToSimulator = new Dictionary<long, PowerTransformerForScada>();
            Dictionary<long, SubstationForScada> SssToSimulator = new Dictionary<long, SubstationForScada>();
            Dictionary<long, BaseVoltageForScada> BvsToSimulator = new Dictionary<long, BaseVoltageForScada>();
            List<long> addedEq = new List<long>();

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

            foreach (ResourceDescription rd in ptsToEnlist)
            {
                PowerTransformer pt = new PowerTransformer();
                pt.RD2Class(rd);
                pt.GlobalId = rd.Id;
                PowerTransformerForScada ptDb = new PowerTransformerForScada(pt);
                PtsToSimulator.Add(ptDb.GlobalId, ptDb);
            }

            foreach (ResourceDescription rd in substationsToEnlist)
            {
                Substation ss = new Substation();
                ss.RD2Class(rd);
                ss.GlobalId = rd.Id;
                SubstationForScada ssDb = new SubstationForScada(ss);
                SssToSimulator.Add(ssDb.GlobalId, ssDb);
            }

            foreach (ResourceDescription rd in basevoltagesToEnlist)
            {
                BaseVoltage bv = new BaseVoltage();
                bv.RD2Class(rd);
                bv.GlobalId = rd.Id;
                BaseVoltageForScada bvDb = new BaseVoltageForScada(bv);
                BvsToSimulator.Add(bvDb.GlobalId, bvDb);
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
                        if (m.SignalDirection == Direction.READ)
                        {
                            Vs.Add(m);
                        }
                        else if (m.SignalDirection == Direction.READWRITE)
                        {
                            powerTransformerSignal.Add(m);
                        }
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
                            if (!addedEq.Contains(ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId) && 
                                simulators[Ps[i].RtuAddress].AddBaseVoltage(BvsToSimulator[ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId]))
                            {
                                addedEq.Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId);
                                
                                if (!eqGidsForSimulator.ContainsKey(Ps[i].RtuAddress))
                                {
                                    eqGidsForSimulator.Add(Ps[i].RtuAddress, new List<long>());
                                }

                                if (!copyAllDataByRtu.ContainsKey(Ps[i].RtuAddress))
                                {
                                    copyAllDataByRtu.Add(Ps[i].RtuAddress, new List<DataForScada>());
                                }

                                eqGidsForSimulator[Ps[i].RtuAddress].Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId);
                                copyAllData.Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId, BvsToSimulator[ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId]);
                                copyAllDataByRtu[Ps[i].RtuAddress].Add(BvsToSimulator[ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId]);
                            }
                            else
                            {
                                if (!addedEq.Contains(ConsToSimulator[Ps[i].PowerSystemResourceRef].BaseVoltageId))
                                {
                                    return false;
                                }
                            }

                            if (!addedEq.Contains(ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID) &&
                                simulators[Ps[i].RtuAddress].AddSubstation(SssToSimulator[ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID]))
                            {
                                addedEq.Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID);
                                
                                if (!eqGidsForSimulator.ContainsKey(Ps[i].RtuAddress))
                                {
                                    eqGidsForSimulator.Add(Ps[i].RtuAddress, new List<long>());
                                }

                                if (!copyAllDataByRtu.ContainsKey(Ps[i].RtuAddress))
                                {
                                    copyAllDataByRtu.Add(Ps[i].RtuAddress, new List<DataForScada>());
                                }

                                eqGidsForSimulator[Ps[i].RtuAddress].Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID);
                                copyAllData.Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID, SssToSimulator[ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID]);
                                copyAllDataByRtu[Ps[i].RtuAddress].Add(SssToSimulator[ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID]);
                            }
                            else
                            {
                                if (!addedEq.Contains(ConsToSimulator[Ps[i].PowerSystemResourceRef].EqContainerID))
                                {
                                    return false;
                                }
                            }

                            if (!eqGidsForSimulator.ContainsKey(Ps[i].RtuAddress))
                            {
                                eqGidsForSimulator.Add(Ps[i].RtuAddress, new List<long>());
                            }

                            if (!copyAllDataByRtu.ContainsKey(Ps[i].RtuAddress))
                            {
                                copyAllDataByRtu.Add(Ps[i].RtuAddress, new List<DataForScada>());
                            }

                            eqGidsForSimulator[Ps[i].RtuAddress].Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].GlobalId);
                            copyAllData.Add(ConsToSimulator[Ps[i].PowerSystemResourceRef].GlobalId, ConsToSimulator[Ps[i].PowerSystemResourceRef]);
                            copyAllDataByRtu[Ps[i].RtuAddress].Add(ConsToSimulator[Ps[i].PowerSystemResourceRef]);
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

                    if (!analogIndexesForSimulator.ContainsKey(Ps[i].RtuAddress))
                    {
                        analogIndexesForSimulator.Add(Ps[i].RtuAddress, new List<int>());
                    }

                    analogIndexesForSimulator[Ps[i].RtuAddress].Add(index);
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

                    if (!analogIndexesForSimulator.ContainsKey(Qs[i].RtuAddress))
                    {
                        analogIndexesForSimulator.Add(Qs[i].RtuAddress, new List<int>());
                    }

                    analogIndexesForSimulator[Qs[i].RtuAddress].Add(index);
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

                    if (!analogIndexesForSimulator.ContainsKey(Vs[i].RtuAddress))
                    {
                        analogIndexesForSimulator.Add(Vs[i].RtuAddress, new List<int>());
                    }

                    analogIndexesForSimulator[Vs[i].RtuAddress].Add(index);
                    this.copyMeasurements[Vs[i].RtuAddress].Add(new MeasurementForScada(a) { Index = index, Measurement = Vs[i] });
                    ++addressPool[Vs[i].RtuAddress].Cnt;
                }
                else
                {
                    return false;
                }
            }

            for (int i = 0; i < powerTransformerSignal.Count; i++)
            {
                int index = -1;

                try
                {

                    if (!addedEq.Contains(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId) &&
                    simulators[powerTransformerSignal[i].RtuAddress].AddBaseVoltage(BvsToSimulator[PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId]))
                    {
                        addedEq.Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId);

                        if (!eqGidsForSimulator.ContainsKey(powerTransformerSignal[i].RtuAddress))
                        {
                            eqGidsForSimulator.Add(powerTransformerSignal[i].RtuAddress, new List<long>());
                        }

                        if (!copyAllDataByRtu.ContainsKey(powerTransformerSignal[i].RtuAddress))
                        {
                            copyAllDataByRtu.Add(powerTransformerSignal[i].RtuAddress, new List<DataForScada>());
                        }

                        eqGidsForSimulator[powerTransformerSignal[i].RtuAddress].Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId);
                        copyAllData.Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId, BvsToSimulator[PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId]);
                        copyAllDataByRtu[powerTransformerSignal[i].RtuAddress].Add(BvsToSimulator[PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId]);
                    }
                    else
                    {
                        if (!addedEq.Contains(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].BaseVoltageId))
                        {
                            return false;
                        }
                    }

                    if (!addedEq.Contains(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId) &&
                        simulators[powerTransformerSignal[i].RtuAddress].AddSubstation(SssToSimulator[PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId]))
                    {
                        addedEq.Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId);

                        if (!eqGidsForSimulator.ContainsKey(powerTransformerSignal[i].RtuAddress))
                        {
                            eqGidsForSimulator.Add(powerTransformerSignal[i].RtuAddress, new List<long>());
                        }

                        if (!copyAllDataByRtu.ContainsKey(powerTransformerSignal[i].RtuAddress))
                        {
                            copyAllDataByRtu.Add(powerTransformerSignal[i].RtuAddress, new List<DataForScada>());
                        }

                        eqGidsForSimulator[powerTransformerSignal[i].RtuAddress].Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId);
                        copyAllData.Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId, SssToSimulator[PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId]);
                        copyAllDataByRtu[powerTransformerSignal[i].RtuAddress].Add(SssToSimulator[PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId]);
                    }
                    else
                    {
                        if (!addedEq.Contains(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].SubstationId))
                        {
                            return false;
                        }
                    }
                    
                    if (!simulators[powerTransformerSignal[i].RtuAddress].AddPowerTransformer(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef]))
                    {
                        return false;
                    }
                    else
                    {
                        if (!eqGidsForSimulator.ContainsKey(powerTransformerSignal[i].RtuAddress))
                        {
                            eqGidsForSimulator.Add(powerTransformerSignal[i].RtuAddress, new List<long>());
                        }

                        if (!copyAllDataByRtu.ContainsKey(powerTransformerSignal[i].RtuAddress))
                        {
                            copyAllDataByRtu.Add(powerTransformerSignal[i].RtuAddress, new List<DataForScada>());
                        }

                        eqGidsForSimulator[powerTransformerSignal[i].RtuAddress].Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].GlobalId);
                        copyAllData.Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef].GlobalId, PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef]);
                        copyAllDataByRtu[powerTransformerSignal[i].RtuAddress].Add(PtsToSimulator[powerTransformerSignal[i].PowerSystemResourceRef]);
                    }

                    index = simulators[powerTransformerSignal[i].RtuAddress].AddMeasurement(powerTransformerSignal[i]);
                }
                catch
                {
                    addressPool[powerTransformerSignal[i].RtuAddress].IsConnected = false;
                    return false;
                }

                int a = f.GetWrapperId(powerTransformerSignal[i].RtuAddress);

                if (index != -1 && a != -1)
                {
                    if (!this.copyMeasurements.ContainsKey(powerTransformerSignal[i].RtuAddress))
                    {
                        this.copyMeasurements.Add(powerTransformerSignal[i].RtuAddress, new List<MeasurementForScada>());
                    }

                    if (!analogIndexesForSimulator.ContainsKey(powerTransformerSignal[i].RtuAddress))
                    {
                        analogIndexesForSimulator.Add(powerTransformerSignal[i].RtuAddress, new List<int>());
                    }

                    analogIndexesForSimulator[powerTransformerSignal[i].RtuAddress].Add(index);
                    this.copyMeasurements[powerTransformerSignal[i].RtuAddress].Add(new MeasurementForScada(a) { Index = index, Measurement = powerTransformerSignal[i] });
                    ++addressPool[powerTransformerSignal[i].RtuAddress].Cnt;
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
            this.allDataByRtu.Clear();
            this.allData.Clear();

            foreach (KeyValuePair<long, DataForScada> kvp in this.copyAllData)
            {
                if (kvp.Value is EnergyConsumerForScada)
                {
                    f.AddConsumers((EnergyConsumerForScada)kvp.Value);
                }
                else if (kvp.Value is BaseVoltageForScada)
                {
                    f.AddBaseVoltages((BaseVoltageForScada)kvp.Value);
                }
                else if (kvp.Value is PowerTransformerForScada)
                {
                    f.AddPowerTransformers((PowerTransformerForScada)kvp.Value);
                }
                else if (kvp.Value is SubstationForScada)
                {
                    f.AddSubstations((SubstationForScada)kvp.Value);
                }

                this.allData.Add(kvp.Key, kvp.Value);
            }

            lock (lockObject)
            {
                foreach (KeyValuePair<int, List<DataForScada>> kvp in this.copyAllDataByRtu)
                {
                    lock (lockObjects[kvp.Key])
                    {
                        this.allDataByRtu.Add(kvp.Key, kvp.Value);
                        this.handlers[kvp.Key].UpdateData(kvp.Value);
                    }
                }
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

            this.copyMeasurements.Clear();
            this.copyAllData.Clear();
            this.copyAllDataByRtu.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Commit; line: {0}; Finish the Commit function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public void Rollback()
        {
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Start the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            foreach (KeyValuePair<int, RTUAddress> kvp in addressPool)
            {
                lock (lockObjectForSimulators)
                {
                    if (this.simulators.ContainsKey(kvp.Key) && addressPool[kvp.Key].Cnt > 0)
                    {
                        this.simulators[kvp.Key].Rollback(addressPool[kvp.Key].Cnt, eqGidsForSimulator[kvp.Key], analogIndexesForSimulator[kvp.Key]);
                        eqGidsForSimulator.Remove(kvp.Key);
                        addressPool[kvp.Key].Cnt = 0;
                    }
                }
            }
            
            this.copyMeasurements.Clear();
            this.copyAllData.Clear();
            this.copyAllDataByRtu.Clear();
            Logger.LogMessageToFile(string.Format("SCADA.Scada.Rollback; line: {0}; Finish the Rollback function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private void CheckIfThereIsSomethingToSned()
        {
            while (true)
            {
                foreach (KeyValuePair<int, SOEHandler> handler in handlers)
                {
                    lock (lockObjects[handler.Key])
                    {
                        if (handler.Value.HasNewMeas)
                        {
                            Logger.LogMessageToFile(string.Format("SCADA.Scada.CheckIfThereIsSomethingToSned; line: {0}; Scada sends data to client if it has data to send", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                            ProxyCE.DataFromScada(handler.Value.resourcesToSend);
                            handler.Value.HasNewMeas = false;
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
                this.measurements.Clear();
                List<WrapperDB> meass = f.ReadMeas();

                foreach (WrapperDB wDB in meass)
                {
                    measurements.Add(wDB.RtuAddress, wDB.ListOfMeasurements);
                }
                
                allData.Clear();
                List<EnergyConsumerForScada> conss = f.ReadConsumers();
                List<PowerTransformerForScada> pts = f.ReadPowerTransformers();
                List<BaseVoltageForScada> bvs = f.ReadBaseVoltages();
                List<SubstationForScada> sss = f.ReadSubstations();
                conss.ForEach(x => { allData.Add(x.GlobalId, x); });
                pts.ForEach(x => allData.Add(x.GlobalId, x));
                bvs.ForEach(x => allData.Add(x.GlobalId, x));
                sss.ForEach(x => allData.Add(x.GlobalId, x));
                List<long> writtenEquipments = new List<long>();

                foreach (KeyValuePair<int, List<MeasurementForScada>> kvp in measurements)
                {
                    foreach (MeasurementForScada m in kvp.Value)
                    {
                        if (!allDataByRtu.ContainsKey(kvp.Key))
                        {
                            allDataByRtu.Add(kvp.Key, new List<DataForScada>());
                        }

                        if (allData[m.Measurement.PowerSystemResourceRef] is EnergyConsumerForScada)
                        {
                            if (!writtenEquipments.Contains(((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).GlobalId))
                            {
                                allDataByRtu[kvp.Key].Add(allData[m.Measurement.PowerSystemResourceRef]);
                                writtenEquipments.Add(((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).GlobalId);
                                if (!writtenEquipments.Contains(((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).BaseVoltageId))
                                {
                                    allDataByRtu[kvp.Key].Add(allData[((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).BaseVoltageId]);
                                    writtenEquipments.Add(((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).BaseVoltageId);
                                }
                                if (!writtenEquipments.Contains(((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).EqContainerID))
                                {
                                    allDataByRtu[kvp.Key].Add(allData[((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).EqContainerID]);
                                    writtenEquipments.Add(((EnergyConsumerForScada)allData[m.Measurement.PowerSystemResourceRef]).EqContainerID);
                                }
                            }
                        }
                        else if (allData[m.Measurement.PowerSystemResourceRef] is PowerTransformerForScada)
                        {
                            if (!writtenEquipments.Contains(((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).GlobalId))
                            {
                                allDataByRtu[kvp.Key].Add(allData[m.Measurement.PowerSystemResourceRef]);
                                writtenEquipments.Add(((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).GlobalId);
                                if (!writtenEquipments.Contains(((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).BaseVoltageId))
                                {
                                    allDataByRtu[kvp.Key].Add(allData[((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).BaseVoltageId]);
                                    writtenEquipments.Add(((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).BaseVoltageId);
                                }
                                if (!writtenEquipments.Contains(((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).SubstationId))
                                {
                                    allDataByRtu[kvp.Key].Add(allData[((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).SubstationId]);
                                    writtenEquipments.Add(((PowerTransformerForScada)allData[m.Measurement.PowerSystemResourceRef]).SubstationId);
                                }
                            }
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

                if (!this.lockObjects.ContainsKey(ret))
                {
                    this.lockObjects.Add(ret, new object());
                }

                var handler = new SOEHandler(measurements[ret], resourcesToSend, this.lockObjects[ret], allDataByRtu[ret]);
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
                if (m is MeasurementForScada)
                {
                    retVal.Add((MeasurementForScada)m);
                }
            }

            return retVal;
        }

        public List<DataForScada> GetDataFromScada(int rtuAddress)
        {
            if(allDataByRtu.ContainsKey(rtuAddress))
            {
                return allDataByRtu[rtuAddress];
            }
            else
            {
                return new List<DataForScada>();
            }
        }

        #region command

        private ICommandHeaders GetCommandHeader(double delta, short index)
        {
            var ao = new AnalogOutputDouble64(delta);

            return CommandSet.From(CommandHeader.From(IndexedValue.From(ao, (ushort)index)));
        }

        public string Command(Dictionary<long, DynamicMeasurement> measurementsInAlarm)
        {
            Dictionary<int, Dictionary<long, TypeVoltage>> rtuAddresses = new Dictionary<int, Dictionary<long, TypeVoltage>>();
            string retVal = "Result for commands:\n";
            Logger.LogMessageToFile(string.Format("SCADA.Command; line: {0}; Scada receive command from CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            foreach (KeyValuePair<int, List<DataForScada>> kvp in allDataByRtu)
            {
                foreach (DataForScada ecfs in kvp.Value)
                {
                    if (ecfs is EnergyConsumerForScada)
                    {
                        EnergyConsumerForScada energyCon = (EnergyConsumerForScada)ecfs;

                        if (measurementsInAlarm.ContainsKey(energyCon.GlobalId))
                        {
                            PowerTransformerForScada pt = f.GetPowerTransformerForCommanding(energyCon.BaseVoltageId, energyCon.EqContainerID);
                            TypeVoltage typeVol = measurementsInAlarm[energyCon.GlobalId].TypeVoltage;
                            bool commandForPtAlreadyExists = false;
                            bool differentTypeVoltageOnSamePt = false;

                            if (rtuAddresses.ContainsKey(kvp.Key))
                            {
                                if (rtuAddresses[kvp.Key].ContainsKey(pt.GlobalId))
                                {
                                    commandForPtAlreadyExists = true;

                                    if (rtuAddresses[kvp.Key][pt.GlobalId] != typeVol)
                                    {
                                        differentTypeVoltageOnSamePt = true;
                                    }
                                }
                            }
                            else
                            {
                                rtuAddresses.Add(kvp.Key, new Dictionary<long, TypeVoltage>());
                            }

                            if (differentTypeVoltageOnSamePt)
                            {
                                retVal += "Command FAILED! Under and over voltage on the same power transformer[name=" + pt.Name + "], on RTU[address=" + kvp.Key + "].\n";
                            }
                            else if (!commandForPtAlreadyExists)
                            {
                                rtuAddresses[kvp.Key].Add(pt.GlobalId, typeVol);
                            }
                        }
                    }
                }
            }

            Logger.LogMessageToFile(string.Format("SCADA.Command; line: {0}; First big foreach passed", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            foreach (KeyValuePair<int, Dictionary<long, TypeVoltage>> kvp in rtuAddresses)
            {
                foreach (KeyValuePair<long, TypeVoltage> kvp2 in kvp.Value)
                {
                    PowerTransformerForScada pt = ((PowerTransformerForScada)allData[kvp2.Key]);
                    BaseVoltageForScada bv = ((BaseVoltageForScada)allData[pt.BaseVoltageId]);
                    double delta = bv.NominalVoltage;
                    short index = -1;

                    foreach (MeasurementForScada meas in measurements[kvp.Key])
                    {
                        if (meas.Measurement.PowerSystemResourceRef == kvp2.Key)
                        {
                            index = (short)meas.Index;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        retVal += "\n";
                        retVal += "RTU: " + kvp.Key + ", transformator name: " + pt.Name + ", type voltage: " + EnumDescription.GetEnumDescription(kvp2.Value) + ", ";

                        if (kvp2.Value == TypeVoltage.UNDERVOLTAGE)
                        {
                            delta = 0.01 * delta;
                        }
                        else if (kvp2.Value == TypeVoltage.OVERVOLTAGE)
                        {
                            delta = -0.01 * delta;
                        }

                        var task = masters[kvp.Key].DirectOperate(this.GetCommandHeader(delta, index), TaskConfig.Default);
                        
                        task.ContinueWith((result) =>
                        {
                        });

                        retVal += task.Result.TaskSummary;
                    }
                }
            }

            Logger.LogMessageToFile(string.Format("SCADA.Command; line: {0}; Second big foreach passed", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            return retVal;
        }
        
        #endregion
    }
}
