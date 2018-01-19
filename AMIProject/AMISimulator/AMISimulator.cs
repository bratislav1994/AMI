using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel;
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
        private Dictionary<long, EnergyConsumerForScada> consumers;

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
                            binding.SendTimeout = TimeSpan.FromMinutes(3);
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
            consumers = new Dictionary<long, EnergyConsumerForScada>();
            measurements = new Dictionary<int, Measurement>();
            this.numberOfInstalledPoints = 0;
            lockObject = new object();
            IDNP3Manager mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());
            config = new OutstationStackConfig();
            outstation = InitializeOutstation(config, mgr);
        }

        private void InitHousehold()
        {
            householdConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumption.Add(i, 0);
            }

            householdConsumption[0] = 5 / 100;
            householdConsumption[1] = 5 / 100;
            householdConsumption[2] = 5 / 100;
            householdConsumption[3] = 5 / 100;
            householdConsumption[4] = 5 / 100;
            householdConsumption[5] = 7 / 100;
            householdConsumption[6] = 14 / 100;
            householdConsumption[7] = 25 / 100;
            householdConsumption[8] = 17 / 100;
            householdConsumption[9] = 12 / 100;
            householdConsumption[10] = 12 / 100;
            householdConsumption[11] = 12 / 100;
            householdConsumption[12] = 12 / 100;
            householdConsumption[13] = 11 / 100;
            householdConsumption[14] = 14 / 100;
            householdConsumption[15] = 15 / 100;
            householdConsumption[16] = 45 / 100;
            householdConsumption[17] = 55 / 100;
            householdConsumption[18] = 75 / 100;
            householdConsumption[19] = 60 / 100;
            householdConsumption[20] = 50 / 100;
            householdConsumption[21] = 52 / 100;
            householdConsumption[22] = 33 / 100;
            householdConsumption[23] = 10 / 100;
        }

        private void InitShoppingCenter()
        {
            shoppingCenterConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumption.Add(i, 0);
            }

            shoppingCenterConsumption[0] = 5 / 100;
            shoppingCenterConsumption[1] = 5 / 100;
            shoppingCenterConsumption[2] = 5 / 100;
            shoppingCenterConsumption[3] = 5 / 100;
            shoppingCenterConsumption[4] = 5 / 100;
            shoppingCenterConsumption[5] = 5 / 100;
            shoppingCenterConsumption[6] = 35 / 100;
            shoppingCenterConsumption[7] = 45 / 100;
            shoppingCenterConsumption[8] = 75 / 100;
            shoppingCenterConsumption[9] = 80 / 100;
            shoppingCenterConsumption[10] = 80 / 100;
            shoppingCenterConsumption[11] = 80 / 100;
            shoppingCenterConsumption[12] = 80 / 100;
            shoppingCenterConsumption[13] = 80 / 100;
            shoppingCenterConsumption[14] = 80 / 100;
            shoppingCenterConsumption[15] = 80 / 100;
            shoppingCenterConsumption[16] = 80 / 100;
            shoppingCenterConsumption[17] = 80 / 100;
            shoppingCenterConsumption[18] = 80 / 100;
            shoppingCenterConsumption[19] = 80 / 100;
            shoppingCenterConsumption[20] = 80 / 100;
            shoppingCenterConsumption[21] = 80 / 100;
            shoppingCenterConsumption[22] = 55 / 100;
            shoppingCenterConsumption[23] = 10 / 100;
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
                    ((IContextChannel)ProxyScada).OperationTimeout = TimeSpan.FromMinutes(1);
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
            List<EnergyConsumerForScada> cons = ProxyScada.GetConsumersFromScada(address);
            numberOfInstalledPoints = measForScada.Count;

            foreach (MeasurementForScada m in measForScada)
            {
                this.measurements.Add(m.Index, m.Measurement);
            }

            foreach (EnergyConsumerForScada ec in cons)
            {
                this.consumers.Add(ec.GlobalId, ec); 
            }

            channel = mgr.AddTCPServer("master" + address, LogLevels.NORMAL, ChannelRetry.Default, ipAddress, (ushort)(basePort + address), ChannelListener.Print());
            config.outstation.config.allowUnsolicited = true;
            config.outstation.config.unsolClassMask.Class0 = true;
            config.outstation.config.unsolClassMask.Class1 = true;
            config.outstation.config.unsolClassMask.Class2 = true;
            config.outstation.config.unsolClassMask.Class3 = true;
            config.link.localAddr = (ushort)address;
            config.link.remoteAddr = 1;

            var outstation = channel.AddOutstation("outstation" + address, RejectingCommandHandler.Instance, DefaultOutstationApplication.Instance, config);
            outstation.Enable();

            return outstation;
        }

        public void SendPointValues()
        {
            SendPointValues(this.config, this.outstation);
        }

        private void SendPointValues(OutstationStackConfig config, IOutstation outstation)
        {
            Random rnd = new Random();
            while (channel.GetChannelStatistics().NumOpen == 0)
            { }

            Console.WriteLine("Press enter to start sending process");
            Console.ReadKey();

            while (true)
            {
                for (int i = 0; i < numberOfInstalledPoints; i++)
                {
                    if (i % 3 == 0)
                    {
                        ChangeSet changeset = new ChangeSet();
                        changeset.Update(new Automatak.DNP3.Interface.Analog(rnd.Next(measurements[i].MinRawValue, measurements[i].MaxRawValue), 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                        outstation.Load(changeset);
                    }
                    else if (i % 3 == 1)
                    {
                        ChangeSet changeset = new ChangeSet();
                        changeset.Update(new Automatak.DNP3.Interface.Analog(rnd.Next(measurements[i].MinRawValue, measurements[i].MaxRawValue), 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                        outstation.Load(changeset);
                    }
                    else
                    {
                        ChangeSet changeset = new ChangeSet();
                        changeset.Update(new Automatak.DNP3.Interface.Analog(rnd.Next(measurements[i].MinRawValue, measurements[i].MaxRawValue), 1, DateTime.Now), (ushort)(config.databaseTemplate.analogs[i].index));
                        outstation.Load(changeset);
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

        public void Rollback(int decrease, List<long> conGidsForSimulator)
        {
            lock (lockObject)
            {
                this.numberOfInstalledPoints -= decrease;
                conGidsForSimulator.ForEach(x => consumers.Remove(x));
            }
        }

        public bool Ping()
        {
            return true;
        }
    }
}
