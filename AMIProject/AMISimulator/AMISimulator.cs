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
        private Dictionary<int, float> householdConsumption;
        private Dictionary<int, float> shoppingCenterConsumption;
        private Dictionary<int, float> firmConsumption;
        private Dictionary<long, EnergyConsumerForScada> consumers;
        private Dictionary<long, BaseVoltageForScada> baseVoltages;
        private Dictionary<long, PowerTransformerForScada> powerTransformers;
        private Dictionary<long, SubstationForScada> substations;
        private static Random rnd = new Random();
        private Dictionary<long, double> activePowers;
        private CommandHandler handler;

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
                            binding.SendTimeout = TimeSpan.FromMinutes(5);
                            binding.MaxReceivedMessageSize = Int32.MaxValue;
                            binding.MaxBufferSize = Int32.MaxValue;
                            factory = new DuplexChannelFactory<IScadaDuplexSimulator>(new InstanceContext(this),
                                                                                    binding,
                                                                                    new EndpointAddress("net.tcp://localhost:10100/Scada/Simulator"));

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
            this.InitHousehold();
            this.InitShoppingCenter();
            this.InitFirm();
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

        private void InitFirm()
        {
            firmConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumption.Add(i, 0);
            }

            firmConsumption[0] = (float)5 / 100;
            firmConsumption[1] = (float)5 / 100;
            firmConsumption[2] = (float)5 / 100;
            firmConsumption[3] = (float)5 / 100;
            firmConsumption[4] = (float)5 / 100;
            firmConsumption[5] = (float)5 / 100;
            firmConsumption[6] = (float)35 / 100;
            firmConsumption[7] = (float)45 / 100;
            firmConsumption[8] = (float)75 / 100;
            firmConsumption[9] = (float)100 / 100;
            firmConsumption[10] = (float)80 / 100;
            firmConsumption[11] = (float)80 / 100;
            firmConsumption[12] = (float)80 / 100;
            firmConsumption[13] = (float)80 / 100;
            firmConsumption[14] = (float)80 / 100;
            firmConsumption[15] = (float)80 / 100;
            firmConsumption[16] = (float)80 / 100;
            firmConsumption[17] = (float)62 / 100;
            firmConsumption[18] = (float)55 / 100;
            firmConsumption[19] = (float)40 / 100;
            firmConsumption[20] = (float)10 / 100;
            firmConsumption[21] = (float)10 / 100;
            firmConsumption[22] = (float)10 / 100;
            firmConsumption[23] = (float)10 / 100;
        }

        private void InitHousehold()
        {
            householdConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumption.Add(i, 0);
            }

            householdConsumption[0] = (float)5 / 100;
            householdConsumption[1] = (float)5 / 100;
            householdConsumption[2] = (float)5 / 100;
            householdConsumption[3] = (float)5 / 100;
            householdConsumption[4] = (float)5 / 100;
            householdConsumption[5] = (float)7 / 100;
            householdConsumption[6] = (float)14 / 100;
            householdConsumption[7] = (float)25 / 100;
            householdConsumption[8] = (float)17 / 100;
            householdConsumption[9] = (float)12 / 100;
            householdConsumption[10] = (float)12 / 100;
            householdConsumption[11] = (float)12 / 100;
            householdConsumption[12] = (float)12 / 100;
            householdConsumption[13] = (float)11 / 100;
            householdConsumption[14] = (float)14 / 100;
            householdConsumption[15] = (float)15 / 100;
            householdConsumption[16] = (float)45 / 100;
            householdConsumption[17] = (float)55 / 100;
            householdConsumption[18] = (float)100 / 100;
            householdConsumption[19] = (float)60 / 100;
            householdConsumption[20] = (float)50 / 100;
            householdConsumption[21] = (float)52 / 100;
            householdConsumption[22] = (float)33 / 100;
            householdConsumption[23] = (float)10 / 100;
        }

        private void InitShoppingCenter()
        {
            shoppingCenterConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumption.Add(i, 0);
            }

            shoppingCenterConsumption[0] = (float)5 / 100;
            shoppingCenterConsumption[1] = (float)5 / 100;
            shoppingCenterConsumption[2] = (float)5 / 100;
            shoppingCenterConsumption[3] = (float)5 / 100;
            shoppingCenterConsumption[4] = (float)5 / 100;
            shoppingCenterConsumption[5] = (float)5 / 100;
            shoppingCenterConsumption[6] = (float)35 / 100;
            shoppingCenterConsumption[7] = (float)45 / 100;
            shoppingCenterConsumption[8] = (float)75 / 100;
            shoppingCenterConsumption[9] = (float)100 / 100;
            shoppingCenterConsumption[10] = (float)80 / 100;
            shoppingCenterConsumption[11] = (float)80 / 100;
            shoppingCenterConsumption[12] = (float)80 / 100;
            shoppingCenterConsumption[13] = (float)80 / 100;
            shoppingCenterConsumption[14] = (float)80 / 100;
            shoppingCenterConsumption[15] = (float)80 / 100;
            shoppingCenterConsumption[16] = (float)80 / 100;
            shoppingCenterConsumption[17] = (float)80 / 100;
            shoppingCenterConsumption[18] = (float)80 / 100;
            shoppingCenterConsumption[19] = (float)80 / 100;
            shoppingCenterConsumption[20] = (float)80 / 100;
            shoppingCenterConsumption[21] = (float)80 / 100;
            shoppingCenterConsumption[22] = (float)55 / 100;
            shoppingCenterConsumption[23] = (float)10 / 100;
        }

        private IOutstation InitializeOutstation(OutstationStackConfig config, IDNP3Manager mgr)
        {
            config.databaseTemplate = new DatabaseTemplate(0, 0, (numberOfAnalogPointsP + numberOfAnalogPointsQ + numberOfAnalogPointsV), 0, 0, 0, 0, 0);
            config.link.responseTimeout = new TimeSpan(0, 0, 0, 1, 0);

            foreach (var analog in config.databaseTemplate.analogs)
            {
                analog.clazz = PointClass.Class2;
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

            foreach (MeasurementForScada m in measForScada)
            {
                this.measurements.Add(m.Index, m.Measurement);
            }

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

            handler = new CommandHandler();

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
                activePowers.Clear();

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
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = consumers[measurements[i].PowerSystemResourceRef].PMax * householdConsumption[now.Minute % 24] + rnd.Next(-5, 5);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                activePowers.Add(measurements[i].PowerSystemResourceRef, valueToSend);
                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 2, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.SHOPPING_CENTER)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = consumers[measurements[i].PowerSystemResourceRef].PMax * shoppingCenterConsumption[now.Minute % 24] + rnd.Next(-5, 5);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                activePowers.Add(measurements[i].PowerSystemResourceRef, valueToSend);
                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 2, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.FIRM)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = consumers[measurements[i].PowerSystemResourceRef].PMax * firmConsumption[now.Minute % 24] + rnd.Next(-5, 5);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                activePowers.Add(measurements[i].PowerSystemResourceRef, valueToSend);
                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 2, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                        }
                        else if (measurements[i].UnitSymbol == UnitSymbol.Q)
                        {
                            if (type == ConsumerType.HOUSEHOLD)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = consumers[measurements[i].PowerSystemResourceRef].QMax * householdConsumption[now.Minute % 24] + rnd.Next(-5, 5);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 2, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.SHOPPING_CENTER)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = consumers[measurements[i].PowerSystemResourceRef].QMax * shoppingCenterConsumption[now.Minute % 24] + rnd.Next(-5, 5);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 2, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.FIRM)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = consumers[measurements[i].PowerSystemResourceRef].QMax * firmConsumption[now.Minute % 24] + rnd.Next(-5, 5);

                                if (valueToSend < 0)
                                {
                                    valueToSend = 0;
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 2, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                        }
                        else if (measurements[i].UnitSymbol == UnitSymbol.V)
                        {
                            double voltageLoss = 0.0007;

                            if (type == ConsumerType.HOUSEHOLD)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = -1;

                                if ((activePowers[measurements[i].PowerSystemResourceRef] / consumers[measurements[i].PowerSystemResourceRef].PMax) < 0.1)
                                {
                                    valueToSend = baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage + baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage * voltageLoss * 100;
                                }
                                else
                                {
                                    valueToSend = baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage - baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage * voltageLoss * ((activePowers[measurements[i].PowerSystemResourceRef] / consumers[measurements[i].PowerSystemResourceRef].PMax) * 100);
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.SHOPPING_CENTER)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = -1;

                                if ((activePowers[measurements[i].PowerSystemResourceRef] / consumers[measurements[i].PowerSystemResourceRef].PMax) < 0.1)
                                {
                                    valueToSend = baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage + baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage * voltageLoss * 100;
                                }
                                else
                                {
                                    valueToSend = baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage - baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage * voltageLoss * ((activePowers[measurements[i].PowerSystemResourceRef] / consumers[measurements[i].PowerSystemResourceRef].PMax) * 100);
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                            else if (type == ConsumerType.FIRM)
                            {
                                ChangeSet changeset = new ChangeSet();
                                double valueToSend = -1;

                                if ((activePowers[measurements[i].PowerSystemResourceRef] / consumers[measurements[i].PowerSystemResourceRef].PMax) < 0.1)
                                {
                                    valueToSend = baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage + baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage * voltageLoss * 100;
                                }
                                else
                                {
                                    valueToSend = baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage - baseVoltages[consumers[measurements[i].PowerSystemResourceRef].BaseVoltageId].NominalVoltage * voltageLoss * ((activePowers[measurements[i].PowerSystemResourceRef] / consumers[measurements[i].PowerSystemResourceRef].PMax) * 100);
                                }

                                changeset.Update(new Automatak.DNP3.Interface.Analog(valueToSend, 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                                outstation.Load(changeset);
                            }
                        }
                    }
                }

                Thread.Sleep(timeToSleep);
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
                                                else if (powerTransformers.ContainsKey(x)) { powerTransformers.Remove(x); } });
                analogIndexesForSimulator.ForEach(x => { if (measurements.ContainsKey(x)) { measurements.Remove(x); } });
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
    }
}
