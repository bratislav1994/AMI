using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMISimulator
{
    [ServiceBehavior(InstanceContextMode =InstanceContextMode.Single)]
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

        private List<IOutstation> outStations = null;
        private List<OutstationStackConfig> configs = null;
        private static AMISimulator instance = null;

        public static AMISimulator Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AMISimulator();
                }

                return instance;
            }
        }
        
        public AMISimulator()
        {
            this.numberOfInstalledPoints = 0;

            IDNP3Manager mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());

            this.outStations = new List<IOutstation>();
            this.configs = new List<OutstationStackConfig>();
            

            channel = mgr.AddTCPServer("master", LogLevels.NORMAL, ChannelRetry.Default, ipAddress, basePort, ChannelListener.Print());

            OutstationStackConfig config = new OutstationStackConfig();
            IOutstation outstation = InitializeOutstation(config, channel, 10);

            this.outStations.Add(outstation);
            this.configs.Add(config);
        }

        private IOutstation InitializeOutstation(OutstationStackConfig config, IChannel channel, int rtuAddress)
        {
            config.databaseTemplate = new DatabaseTemplate(0, 0, (numberOfAnalogPointsP + numberOfAnalogPointsQ + numberOfAnalogPointsV), 0, 0, 0, 0, 0);
            config.link.responseTimeout = new TimeSpan(0, 0, 0, 1, 0);
            
            foreach (var analog in config.databaseTemplate.analogs)
            {
                analog.clazz = PointClass.Class2;
            }

            config.outstation.config.allowUnsolicited = true;
            config.outstation.config.unsolClassMask.Class0 = true;
            config.outstation.config.unsolClassMask.Class1 = true;
            config.outstation.config.unsolClassMask.Class2 = true;
            config.outstation.config.unsolClassMask.Class3 = true;
            config.link.localAddr = (ushort)rtuAddress;
            config.link.remoteAddr = 1;
            var outstation = channel.AddOutstation("outstation", RejectingCommandHandler.Instance, DefaultOutstationApplication.Instance, config);
            outstation.Enable();

            return outstation;
        }

        public void SendPointValues()
        {
            SendPointValues(this.configs[0], this.outStations[0]);
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
                    if(i%3 == 0)
                    {
                        ChangeSet changeset = new ChangeSet();
                        changeset.Update(new Analog(rnd.Next(70, 170), 1, DateTime.Now), config.databaseTemplate.analogs[i].index);
                        outstation.Load(changeset);
                    }
                    else if(i%3 == 1)
                    {
                        ChangeSet changeset = new ChangeSet();
                        changeset.Update(new Analog(rnd.Next(7, 77), 1, DateTime.Now), config.databaseTemplate.analogs[i].index);
                        outstation.Load(changeset);
                    }
                    else
                    {
                        ChangeSet changeset = new ChangeSet();
                        changeset.Update(new Analog(rnd.Next(210, 240), 1, DateTime.Now), config.databaseTemplate.analogs[i].index);
                        outstation.Load(changeset);
                    }
                }

                Thread.Sleep(10000);
            }
        }

        public int AddMeasurement()
        {
            this.numberOfInstalledPoints++;

            if(this.numberOfInstalledPoints > (numberOfAnalogPointsP + numberOfAnalogPointsQ + numberOfAnalogPointsV))
            {
                this.numberOfInstalledPoints--;
                return -1;
            }

            return this.numberOfInstalledPoints - 1;
        }

        public void Rollback(int decrease)
        {
            this.numberOfInstalledPoints -= decrease;
        }
    }
}
