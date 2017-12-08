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

namespace SCADA
{
    public class Scada : IScadaForDuplex
    {
        bool firstTimeSimulator = true;
        bool firstTimeNMS = true;
        ISimulator proxySimulator = null;
        Dictionary<int, Measurement> measurements;
        SOEHandler handler;
        IDNP3Manager mgr;
        INetworkModelGDAContractDuplexScada proxyNMS;

        public ISimulator ProxySimulator
        {
            get
            {
                if (firstTimeSimulator)
                {
                    ChannelFactory<ISimulator> factory = new ChannelFactory<ISimulator>(new NetTcpBinding(),
                                                                                        new EndpointAddress("net.tcp://localhost:10100/AMISimulator/Simulator"));
                    proxySimulator = factory.CreateChannel();

                    firstTimeSimulator = false;
                }

                return proxySimulator;
            }

            set
            {
                proxySimulator = value;
            }
        }

        public INetworkModelGDAContractDuplexScada ProxyNMS
        {
            get
            {
                if (firstTimeNMS)
                {
                    DuplexChannelFactory<INetworkModelGDAContractDuplexScada> factory = new DuplexChannelFactory<INetworkModelGDAContractDuplexScada>(
                    new InstanceContext(this),
                        new NetTcpBinding(),
                        new EndpointAddress("net.tcp://localhost:10002/NetworkModelService/GDADuplexScada"));
                    proxyNMS = factory.CreateChannel();
                    firstTimeNMS = false;
                }
                return proxyNMS;
            }

            set
            {
                proxyNMS = value;
            }
        }

        public Scada()
        {
            measurements = new Dictionary<int, Measurement>();
            handler = new SOEHandler(ref measurements);
            mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());
            var channel = mgr.AddTCPClient("outstation", LogLevels.NORMAL | LogLevels.APP_COMMS, ChannelRetry.Default, "127.0.0.1", 20000, ChannelListener.Print());

            var config = new MasterStackConfig();
            config.link.localAddr = 1;
            config.link.remoteAddr = 10;

            var master = channel.AddMaster("master", handler, DefaultMasterApplication.Instance, config);
            config.master.disableUnsolOnStartup = false;

            var integrityPoll = master.AddClassScan(ClassField.AllClasses, TimeSpan.MaxValue, TaskConfig.Default);
            ProxyNMS.ConnectScada();

            master.Enable();

        }

        public void StartIssueCommands()
        {
            Console.WriteLine("Enter a command");

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
            }
        }

        public bool Prepare(List<ResourceDescription> measurements)
        {
            List<Measurement> Ps = new List<Measurement>();
            List<Measurement> Qs = new List<Measurement>();
            List<Measurement> Vs = new List<Measurement>();

            foreach(ResourceDescription rd in measurements)
            {
                TC57CIM.IEC61970.Meas.Analog m = new TC57CIM.IEC61970.Meas.Analog();
                m.RD2Class(rd);

                switch(m.UnitSymbol)
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

            if(Ps.Count != Qs.Count || Ps.Count != Vs.Count || Qs.Count != Vs.Count)
            {
                return false;
            }

            for (int i = 0; i < Ps.Count; i++)
            {
                int index = ProxySimulator.AddMeasurement();
                if(index != -1)
                {
                    this.measurements.Add(index, Ps[i]);
                }
                else
                {
                    return false;
                }
                index = ProxySimulator.AddMeasurement();
                if (index != -1)
                {
                    this.measurements.Add(index, Qs[i]);
                }
                else
                {
                    return false;
                }
                index = ProxySimulator.AddMeasurement();
                if (index != -1)
                {
                    this.measurements.Add(index, Vs[i]);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
