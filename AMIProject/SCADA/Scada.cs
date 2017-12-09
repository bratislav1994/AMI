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

namespace SCADA
{
    public class Scada : IScada
    {
        bool firstTimeSimulator = true;
        bool firstTimeCoordinator = true;
        ISimulator proxySimulator = null;
        Dictionary<int, Measurement> measurements;
        SOEHandler handler;
        IDNP3Manager mgr;
        ITransactionDuplexScada proxyCoordinator;
        int cnt = 0;
        private List<ResourceDescription> measurementsToEnlist;
        private Dictionary<int, Measurement> copyMeasurements = new Dictionary<int, Measurement>();

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

        public ITransactionDuplexScada ProxyCoordinator
        {
            get
            {
                if (firstTimeCoordinator)
                {
                    DuplexChannelFactory<ITransactionDuplexScada> factory = new DuplexChannelFactory<ITransactionDuplexScada>(
                    new InstanceContext(this),
                        new NetTcpBinding(),
                        new EndpointAddress("net.tcp://localhost:10002/TransactionCoordinator/Scada"));
                    proxyCoordinator = factory.CreateChannel();
                    firstTimeCoordinator = false;
                }
                return proxyCoordinator;
            }

            set
            {
                proxyCoordinator = value;
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
            ProxyCoordinator.ConnectScada();

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

        public void EnlistMeas(List<ResourceDescription> meas)
        {
            this.measurementsToEnlist = meas;
            foreach (KeyValuePair<int, Measurement> kvp in this.measurements)
            {
                this.copyMeasurements.Add(kvp.Key, kvp.Value);
            }
        }

        public bool Prepare()
        {
            List<Measurement> Ps = new List<Measurement>();
            List<Measurement> Qs = new List<Measurement>();
            List<Measurement> Vs = new List<Measurement>();

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
                int index = ProxySimulator.AddMeasurement();
                if (index != -1)
                {
                    this.copyMeasurements.Add(index, Ps[i]);
                    ++cnt;
                }
                else
                {
                    return false;
                }
                index = ProxySimulator.AddMeasurement();
                if (index != -1)
                {
                    this.copyMeasurements.Add(index, Qs[i]);
                    ++cnt;
                }
                else
                {
                    return false;
                }
                index = ProxySimulator.AddMeasurement();
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

            return true;
        }

        public void Commit()
        {
            this.measurements.Clear();
            foreach (KeyValuePair<int, Measurement> kvp in this.copyMeasurements)
            {
                this.measurements.Add(kvp.Key, kvp.Value);
            }

            cnt = 0;
            this.copyMeasurements.Clear();
        }

        public void Rollback()
        {
            this.ProxySimulator.Rollback(cnt);
            cnt = 0;
            this.copyMeasurements.Clear();
        }
    }
}
