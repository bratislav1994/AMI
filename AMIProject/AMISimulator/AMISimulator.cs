﻿using Automatak.DNP3.Adapter;
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
            measurements = new Dictionary<int, Measurement>();
            this.numberOfInstalledPoints = 0;
            lockObject = new object();
            IDNP3Manager mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());
            config = new OutstationStackConfig();
            outstation = InitializeOutstation(config, mgr);
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
            numberOfInstalledPoints = measForScada.Count;

            foreach (MeasurementForScada m in measForScada)
            {
                this.measurements.Add(m.Index, m.Measurement);
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

        public void Rollback(int decrease)
        {
            lock (lockObject)
            {
                this.numberOfInstalledPoints -= decrease;
            }
        }

        public bool Ping()
        {
            return true;
        }
    }
}
