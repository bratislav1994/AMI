using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using FTN.Common;
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
using TC57CIM.IEC61970.Meas;
using DailyConsumptions;

namespace AMISimulator
{
    public class AMISimulator : ISimulator
    {
        private const int numberOfRTUs = 1;
        private const string ipAddress = "127.0.0.1";
        private const ushort basePort = 20000;
        private const int numberOfAnalogPointsP = 100;
        private const int numberOfAnalogPointsQ = 100;
        private const int numberOfAnalogPointsV = 100;
        private int numberOfInstalledPoints;
        private IChannel channel = null;
        private int address = 0;
        private IOutstation outstation = null;
        private OutstationStackConfig config = null;
        private object lockObject;
        private IScadaDuplexSimulator proxyScada;
        private bool firstContact = true;
        private DuplexChannelFactory<IScadaDuplexSimulator> factory;
        private Dictionary<int, Measurement> measurements;
        private const int timeToSleep = 30000;
        private Dictionary<long, EnergyConsumerForScada> consumers;
        private Dictionary<long, BaseVoltageForScada> baseVoltages;
        private Dictionary<long, PowerTransformerForScada> powerTransformers;
        private Dictionary<long, SubstationForScada> substations;
        private static Random rnd = new Random();
        private Dictionary<long, double> activePowers;
        private CommandHandler handler;
        private Dictionary<int, double> voltagesForPowerTransformers;
        private Dictionary<long, int> indexForPowerTransformers;
        private object lockObjectForCommand = new object();
        private object lockObjectForAdding = new object();
        private bool addingInProgress = false;
        private bool commandInProgress = false;

        public IScadaDuplexSimulator ProxyScada
        {
            get
            {
                if (firstContact)
                {
                    while (true)
                    {
                        try
                        {
                            NetTcpBinding binding = new NetTcpBinding();
                            binding.SendTimeout = TimeSpan.FromSeconds(3);
                            binding.MaxReceivedMessageSize = Int32.MaxValue;
                            binding.ReceiveTimeout = TimeSpan.MaxValue;
                            binding.MaxBufferSize = Int32.MaxValue;
                            factory = new DuplexChannelFactory<IScadaDuplexSimulator>(new InstanceContext(this),
                                                                                    binding,
                                                                                    new EndpointAddress("net.tcp://localhost:10000/Scada/Simulator"));

                            proxyScada = factory.CreateChannel();
                            firstContact = false;
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(2000);
                        }
                    }
                }

                return proxyScada;
            }

            set
            {
                proxyScada = value;
            }
        }

        public AMISimulator()
        {
            voltagesForPowerTransformers = new Dictionary<int, double>();
            indexForPowerTransformers = new Dictionary<long, int>();
            consumers = new Dictionary<long, EnergyConsumerForScada>();
            baseVoltages = new Dictionary<long, BaseVoltageForScada>();
            powerTransformers = new Dictionary<long, PowerTransformerForScada>();
            substations = new Dictionary<long, SubstationForScada>();
            measurements = new Dictionary<int, Measurement>();
            activePowers = new Dictionary<long, double>();
            this.numberOfInstalledPoints = 0;
            lockObject = new object();
            IDNP3Manager mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());
            config = new OutstationStackConfig();
            outstation = InitializeOutstation(config, mgr);
        }
        
        private IOutstation InitializeOutstation(OutstationStackConfig config, IDNP3Manager mgr)
        {
            config.databaseTemplate = new DatabaseTemplate(0, 0, (numberOfAnalogPointsP + numberOfAnalogPointsQ + numberOfAnalogPointsV), 0, 0, 0, 0, 0);
            config.link.responseTimeout = new TimeSpan(0, 0, 0, 10, 0);

            foreach (var analog in config.databaseTemplate.analogs)
            {
                analog.clazz = PointClass.Class2;
                analog.eventVariation = EventAnalogVariation.Group32Var8;
            }

            Console.WriteLine("Connecting to scada...");

            while (true)
            {
                try
                {
                    address = ProxyScada.Connect();
                    ((IContextChannel)ProxyScada).OperationTimeout = TimeSpan.FromMinutes(5);
                    break;
                }
                catch
                {
                    firstContact = true;
                    Thread.Sleep(2000);
                }
            }

            Console.WriteLine("Connected to scada");
            List<MeasurementForScada> measForScada = ProxyScada.GetNumberOfPoints(address);
            List<DataForScada> dataFromScada = ProxyScada.GetDataFromScada(address);
            numberOfInstalledPoints = measForScada.Count;
            
            foreach (DataForScada d in dataFromScada)
            {
                if (d is EnergyConsumerForScada)
                {
                    this.consumers.Add(((EnergyConsumerForScada)d).GlobalId, (EnergyConsumerForScada)d);
                }
                else if (d is BaseVoltageForScada)
                {
                    this.baseVoltages.Add(((BaseVoltageForScada)d).GlobalId, (BaseVoltageForScada)d);
                }
                else if (d is PowerTransformerForScada)
                {
                    this.powerTransformers.Add(((PowerTransformerForScada)d).GlobalId, (PowerTransformerForScada)d);
                }
                else if (d is SubstationForScada)
                {
                    this.substations.Add(((SubstationForScada)d).GlobalId, (SubstationForScada)d);
                }
            }

            foreach (MeasurementForScada m in measForScada)
            {
                if (m.Measurement.SignalDirection == Direction.READWRITE)
                {
                    PowerTransformerForScada pt = powerTransformers[m.Measurement.PowerSystemResourceRef];
                    double nominalVol = baseVoltages[pt.BaseVoltageId].NominalVoltage;

                    if (!voltagesForPowerTransformers.ContainsKey(m.Index))
                    {
                        voltagesForPowerTransformers.Add(m.Index, nominalVol);
                    }

                    if (!indexForPowerTransformers.ContainsKey(pt.GlobalId))
                    {
                        indexForPowerTransformers.Add(pt.GlobalId, m.Index);
                    }
                }

                this.measurements.Add(m.Index, m.Measurement);
            }

            handler = new CommandHandler(this);

            channel = mgr.AddTCPServer("master" + address, LogLevels.NORMAL, ChannelRetry.Default, ipAddress, (ushort)(basePort + address), ChannelListener.Print());
            config.outstation.config.allowUnsolicited = true;
            config.outstation.config.unsolClassMask.Class0 = true;
            config.outstation.config.unsolClassMask.Class1 = true;
            config.outstation.config.unsolClassMask.Class2 = true;
            config.outstation.config.unsolClassMask.Class3 = true;
            config.link.localAddr = (ushort)address;
            config.link.remoteAddr = 1;

            var outstation = channel.AddOutstation("outstation" + address, handler, DefaultOutstationApplication.Instance, config);
            outstation.Enable();

            return outstation;
        }

        public void SendPointValues()
        {
            SendPointValues(this.config, this.outstation);
        }

        private void SendPointValues(OutstationStackConfig config, IOutstation outstation)
        {
            while (channel.GetChannelStatistics().NumOpen == 0)
            { }

            Console.WriteLine("Press enter to start sending process");
            Console.ReadKey();

            while (true)
            {
                this.activePowers.Clear();
                lock (lockObjectForAdding)
                {
                    lock (lockObjectForCommand)
                    {
                        this.Simulation(config, outstation);
                    }
                }

                Thread.Sleep(timeToSleep);
            }
        }

        private void Simulation(OutstationStackConfig config, IOutstation outstation)
        {
            while (addingInProgress)
            {
                Thread.Sleep(2000);
            }

            if (!commandInProgress)
            {
                ChangeSet changeset = new ChangeSet();

                for (int i = 0; i < numberOfInstalledPoints; i++)
                {
                    if (measurements[i].SignalDirection == Direction.READ)
                    {
                        ConsumerType type = consumers[measurements[i].PowerSystemResourceRef].Type;
                        DateTime now = DateTime.Now;

                        if (measurements[i].UnitSymbol == UnitSymbol.P)
                        {
                            if (type == ConsumerType.HOUSEHOLD)
                            {

                                double valueToSend = DailyConsumption.GetPQHouseHold(now, rnd, consumers[measurements[i].PowerSystemResourceRef].PMax);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                activePowers.Add(measurements[i].PowerSystemResourceRef, valueToSend);
                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                //outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.SHOPPING_CENTER)
                            {
                                //ChangeSet changeset = new ChangeSet();
                                double valueToSend = DailyConsumption.GetPQShoppingCenter(now, rnd, consumers[measurements[i].PowerSystemResourceRef].PMax);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                activePowers.Add(measurements[i].PowerSystemResourceRef, valueToSend);
                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                //outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.FIRM)
                            {
                                //ChangeSet changeset = new ChangeSet();
                                double valueToSend = DailyConsumption.GetPQFirm(now, rnd, consumers[measurements[i].PowerSystemResourceRef].PMax);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                activePowers.Add(measurements[i].PowerSystemResourceRef, valueToSend);
                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                //outstation.Load(changeset);
                            }
                        }
                        else if (measurements[i].UnitSymbol == UnitSymbol.Q)
                        {
                            if (type == ConsumerType.HOUSEHOLD)
                            {
                                //ChangeSet changeset = new ChangeSet();
                                double valueToSend = DailyConsumption.GetPQHouseHold(now, rnd, consumers[measurements[i].PowerSystemResourceRef].QMax);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                //outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.SHOPPING_CENTER)
                            {
                                //ChangeSet changeset = new ChangeSet();
                                double valueToSend = DailyConsumption.GetPQShoppingCenter(now, rnd, consumers[measurements[i].PowerSystemResourceRef].QMax);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                //outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.FIRM)
                            {
                                //ChangeSet changeset = new ChangeSet();
                                double valueToSend = DailyConsumption.GetPQFirm(now, rnd, consumers[measurements[i].PowerSystemResourceRef].QMax);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                //outstation.Load(changeset);
                            }
                        }
                        else if (measurements[i].UnitSymbol == UnitSymbol.V)
                        {
                            double voltageLoss = 0.0007;
                            EnergyConsumerForScada consumer = consumers[measurements[i].PowerSystemResourceRef];
                            long powerTransId = this.GetPowerTransformerForEnConsumer(consumer.EqContainerID, consumer.BaseVoltageId);
                            double currentVoltage = this.GetNominalVoltageForPowerTransformer(powerTransId);

                            //ChangeSet changeset = new ChangeSet();
                            double valueToSend = -1;

                            if ((activePowers[measurements[i].PowerSystemResourceRef] / consumer.PMax) < 0.1)
                            {
                                valueToSend = currentVoltage + currentVoltage * voltageLoss * 100;
                            }
                            else
                            {
                                valueToSend = currentVoltage - currentVoltage * voltageLoss * ((activePowers[measurements[i].PowerSystemResourceRef] / consumer.PMax) * 100);
                            }

                            changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                            //outstation.Load(changeset);
                        }
                    }
                }
                outstation.Load(changeset);
            }
        }

        public int AddMeasurement(Measurement m)
        {
            lock (lockObject)
            {
                this.numberOfInstalledPoints++;

                if (this.numberOfInstalledPoints > (numberOfAnalogPointsP + numberOfAnalogPointsQ + numberOfAnalogPointsV))
                {
                    this.numberOfInstalledPoints--;
                    return -1;
                }

                measurements.Add(numberOfInstalledPoints - 1, m);

                if (m.SignalDirection == Direction.READWRITE)
                {
                    PowerTransformerForScada pt = powerTransformers[m.PowerSystemResourceRef];
                    double nominalVol = baseVoltages[pt.BaseVoltageId].NominalVoltage;

                    if (!voltagesForPowerTransformers.ContainsKey(numberOfInstalledPoints - 1))
                    {
                        voltagesForPowerTransformers.Add(numberOfInstalledPoints - 1, nominalVol);
                    }

                    if (!indexForPowerTransformers.ContainsKey(pt.GlobalId))
                    {
                        indexForPowerTransformers.Add(pt.GlobalId, numberOfInstalledPoints - 1);
                    }
                }

                return this.numberOfInstalledPoints - 1;
            }
        }

        public bool AddConsumer(EnergyConsumerForScada ec)
        {
            if (this.consumers.ContainsKey(ec.GlobalId))
            {
                return false;
            }

            consumers.Add(ec.GlobalId, ec);

            return true;
        }

        public void Rollback(int decrease, List<long> GidsForSimulator, List<int> analogIndexesForSimulator)
        {
            lock (lockObject)
            {
                this.numberOfInstalledPoints -= decrease;
                GidsForSimulator.ForEach(x => { if (consumers.ContainsKey(x)) { consumers.Remove(x); }
                                                else if (baseVoltages.ContainsKey(x)) { baseVoltages.Remove(x); }
                                                else if (substations.ContainsKey(x)) { substations.Remove(x); }
                                                else if (powerTransformers.ContainsKey(x)) { powerTransformers.Remove(x); }
                                                if (indexForPowerTransformers.ContainsKey(x)) { indexForPowerTransformers.Remove(x); } });
                analogIndexesForSimulator.ForEach(x => { if (measurements.ContainsKey(x)) { measurements.Remove(x);
                                                         if (voltagesForPowerTransformers.ContainsKey(x)) { voltagesForPowerTransformers.Remove(x); } } });
            }
        }

        public bool Ping()
        {
            return true;
        }

        public bool AddBaseVoltage(BaseVoltageForScada bv)
        {
            if (this.baseVoltages.ContainsKey(bv.GlobalId))
            {
                return false;
            }

            baseVoltages.Add(bv.GlobalId, bv);

            return true;
        }

        public bool AddSubstation(SubstationForScada ss)
        {
            if (this.substations.ContainsKey(ss.GlobalId))
            {
                return false;
            }

            substations.Add(ss.GlobalId, ss);

            return true;
        }

        public bool AddPowerTransformer(PowerTransformerForScada pt)
        {
            if (this.powerTransformers.ContainsKey(pt.GlobalId))
            {
                return false;
            }

            powerTransformers.Add(pt.GlobalId, pt);

            return true;
        }

        private double GetNominalVoltageForPowerTransformer(long powerTransId)
        {
            return voltagesForPowerTransformers[indexForPowerTransformers[powerTransId]];
        }

        private long GetPowerTransformerForEnConsumer(long subId, long baseVolId)
        {
            foreach (PowerTransformerForScada pt in powerTransformers.Values)
            {
                if (pt.BaseVoltageId == baseVolId && pt.SubstationId == subId)
                {
                    return pt.GlobalId;
                }
            }

            return -1;
        }

        #region commands

        public bool SetNewSetPointForPowerTransformer(double delta, ushort index)
        {
            commandInProgress = true;
            double newValue = voltagesForPowerTransformers[index] + delta;
            long ptId = -1;

            foreach (KeyValuePair<long, int> kvp in indexForPowerTransformers)
            {
                if (kvp.Value == index)
                {
                    ptId = kvp.Key;
                }
            }

            if (ptId == -1 || !powerTransformers.ContainsKey(ptId))
            {
                return false;
            }

            PowerTransformerForScada pt = powerTransformers[ptId];
            float voltage = baseVoltages[pt.BaseVoltageId].NominalVoltage;
            float maxVoltage = voltage + voltage * pt.InvalidRangePercent;
            float minVoltage = voltage - voltage * pt.InvalidRangePercent;

            if (newValue < minVoltage || newValue > maxVoltage)
            {
                return false;
            }

            lock (lockObjectForCommand)
            {
                voltagesForPowerTransformers[index] = newValue;
            }

            commandInProgress = false;

            return true;
        }

        public void AddingStarted()
        {
            lock (lockObjectForAdding)
            {
                addingInProgress = true;
            }
        }

        public void AddingDone()
        {
            addingInProgress = false;
        }

        #endregion
    }
}
