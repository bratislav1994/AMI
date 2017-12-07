using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using System.ServiceModel;
using AMISimulator;
using TC57CIM.IEC61970.Meas;

namespace SCADA
{
    class Program
    {
        static ICommandHeaders GetCommandHeaders()
        {
            var crob = new ControlRelayOutputBlock(ControlCode.PULSE_ON, 1, 100, 100);
            var ao = new AnalogOutputDouble64(1.37);

            return CommandSet.From(
                CommandHeader.From(IndexedValue.From(crob, 0)),
                CommandHeader.From(IndexedValue.From(ao, 1))
            );
        }

        static int Main(string[] args)
        {
            bool firstTime = true;
            ISimulator proxy = null;
            Dictionary<int, Measurement> measurements = new Dictionary<int, Measurement>();
            SOEHandler handler = new SOEHandler(ref measurements);
            
            IDNP3Manager mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());

            var channel = mgr.AddTCPClient("outstation", LogLevels.NORMAL | LogLevels.APP_COMMS, ChannelRetry.Default, "127.0.0.1", 20000, ChannelListener.Print());

            var config = new MasterStackConfig();
            config.link.localAddr = 1;
            config.link.remoteAddr = 10;

            var master = channel.AddMaster("master", handler, DefaultMasterApplication.Instance, config);
            config.master.disableUnsolOnStartup = false;
            
            var integrityPoll = master.AddClassScan(ClassField.AllClasses, TimeSpan.MaxValue, TaskConfig.Default);

            master.Enable();
            Console.WriteLine("Enter a command");

            while (true)
            {
                switch (Console.ReadLine())
                {
                    case "a":
                        if(firstTime)
                        {
                            ChannelFactory<ISimulator> factory = new ChannelFactory<ISimulator>(new NetTcpBinding(), 
                                                                                                new EndpointAddress("net.tcp://localhost:10100/AMISimulator/Simulator"));
                            proxy = factory.CreateChannel();

                            firstTime = false;
                        }
                        proxy.AddMeasurement();
                        break;
                    case "x":
                        return 0;
                    default:
                        break;
                }
            }
        }
    }
}