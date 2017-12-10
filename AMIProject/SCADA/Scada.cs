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

namespace SCADA
{
    public class Scada : IScada, IDisposable
    {
        bool firstTimeSimulator = true;
        bool firstTimeCoordinator = true;
        bool firstTimeCE = true;
        ISimulator proxySimulator = null;
        Dictionary<int, ResourceDescription> measurements;
        SOEHandler handler;
        IDNP3Manager mgr;
        ITransactionDuplexScada proxyCoordinator;
        int cnt = 0;
        private List<ResourceDescription> measurementsToEnlist;
        private Dictionary<int, ResourceDescription> copyMeasurements = new Dictionary<int, ResourceDescription>();
        private List<ResourceDescription> resourcesToSend = new List<ResourceDescription>();
        private object lockObject = new object();
        private Thread sendingThread;
        ICalculationEngine proxyCE;

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

        public ICalculationEngine ProxyCE
        {
            get
            {
                if (firstTimeCE)
                {
                    ChannelFactory<ICalculationEngine> factory = new ChannelFactory<ICalculationEngine>(new NetTcpBinding(),
                                                                                        new EndpointAddress("net.tcp://localhost:10050/ICalculationEngine/Calculation"));
                    proxyCE = factory.CreateChannel();

                    firstTimeCE = false;
                }

                return proxyCE;
            }

            set
            {
                proxyCE = value;
            }
        }

        public Scada()
        {
            measurements = new Dictionary<int, ResourceDescription>();
            handler = new SOEHandler(ref measurements, ref resourcesToSend, ref lockObject);
            mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());
            var channel = mgr.AddTCPClient("outstation", LogLevels.NORMAL | LogLevels.APP_COMMS, ChannelRetry.Default, "127.0.0.1", 20000, ChannelListener.Print());

            var config = new MasterStackConfig();
            config.link.localAddr = 1;
            config.link.remoteAddr = 10;

            var master = channel.AddMaster("master", handler, DefaultMasterApplication.Instance, config);
            config.master.disableUnsolOnStartup = false;

            var integrityPoll = master.AddClassScan(ClassField.AllClasses, TimeSpan.MaxValue, TaskConfig.Default);
            ProxyCoordinator.ConnectScada();

            Deserialize(measurements);

            master.Enable();
            sendingThread = new Thread(() => CheckIfThereIsSomethingToSned());
            sendingThread.Start();

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
            foreach (KeyValuePair<int, ResourceDescription> kvp in this.measurements)
            {
                this.copyMeasurements.Add(kvp.Key, kvp.Value);
            }
        }

        public bool Prepare()
        {
            List<ResourceDescription> Ps = new List<ResourceDescription>();
            List<ResourceDescription> Qs = new List<ResourceDescription>();
            List<ResourceDescription> Vs = new List<ResourceDescription>();

            foreach (ResourceDescription rd in measurementsToEnlist)
            {
                TC57CIM.IEC61970.Meas.Analog m = new TC57CIM.IEC61970.Meas.Analog();
                m.RD2Class(rd);

                switch (m.UnitSymbol)
                {
                    case UnitSymbol.P:
                        Ps.Add(rd);
                        break;
                    case UnitSymbol.Q:
                        Qs.Add(rd);
                        break;
                    case UnitSymbol.V:
                        Vs.Add(rd);
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
            foreach (KeyValuePair<int, ResourceDescription> kvp in this.copyMeasurements)
            {
                this.measurements.Add(kvp.Key, kvp.Value);
            }
            Serialize(measurements);
            cnt = 0;
            this.copyMeasurements.Clear();
        }

        public void Rollback()
        {
            this.ProxySimulator.Rollback(cnt);
            cnt = 0;
            this.copyMeasurements.Clear();
        }

        private void CheckIfThereIsSomethingToSned()
        {
            while(true)
            {
                lock(lockObject)
                {
                    if(resourcesToSend.Count > 0)
                    {
                        Console.WriteLine("List changed, here will be code for sending measurements to Calculation Engine...");
                        ProxyCE.DataFromScada(resourcesToSend);

                        resourcesToSend.Clear();
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
            TextWriter tw = new StreamWriter("Measurements.xml");
            List<ClassForSerialization> meas = new List<ClassForSerialization>(measurements.Count);
            foreach(int key in measurements.Keys)
            {
                meas.Add(new ClassForSerialization(key, measurements[key]));
            }
            XmlSerializer serializer = new XmlSerializer(typeof(List<ClassForSerialization>));
            serializer.Serialize(tw, meas);
        }

        private void Deserialize(Dictionary<int, ResourceDescription> measurements)
        {
            try
            {
                TextReader tr = new StreamReader(File.OpenRead("Measurements.xml"));
                measurements.Clear();
                XmlSerializer serializer = new XmlSerializer(typeof(List<ClassForSerialization>));
                List<ClassForSerialization> meas = (List<ClassForSerialization>)serializer.Deserialize(tr);

                foreach (ClassForSerialization cfs in meas)
                {
                    measurements.Add(cfs.Key, cfs.Rd);
                }
            }
            catch { }
        }
    }
}
