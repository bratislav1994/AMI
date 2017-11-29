using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automatak.DNP3.Interface;
using Automatak.DNP3.Adapter;

namespace AMISimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            IDNP3Manager mgr = DNP3ManagerFactory.CreateManager(1, new PrintingLogAdapter());

            var channel = mgr.AddTCPServer("server", LogLevels.NORMAL, ChannelRetry.Default, "0.0.0.0", 20000, ChannelListener.Print());

            var config = new OutstationStackConfig();

            // configure the various measurements in our database
            config.databaseTemplate = new DatabaseTemplate(4, 1, 1, 1, 1, 1, 1, 0);
            config.databaseTemplate.binaries[0].clazz = PointClass.Class2;
            // ....           
            config.outstation.config.allowUnsolicited = true;

            // Optional: setup your stack configuration here
            config.link.localAddr = 10;
            config.link.remoteAddr = 1;

            var outstation = channel.AddOutstation("outstation", RejectingCommandHandler.Instance, DefaultOutstationApplication.Instance, config);

            outstation.Enable(); // enable communications

            Console.WriteLine("Press <Enter> to change a value");
            bool binaryValue = false;
            double analogValue = 0;
            while (true)
            {
                switch (Console.ReadLine())
                {
                    case ("x"):
                        return ;
                    default:
                        {
                            binaryValue = !binaryValue;
                            ++analogValue;

                            // create a changeset and load it 
                            var changeset = new ChangeSet();
                            changeset.Update(new Binary(binaryValue, 1, DateTime.Now), 0);
                            changeset.Update(new Analog(analogValue, 1, DateTime.Now), 0);
                            outstation.Load(changeset);
                        }
                        break;
                }
            }
        }
    }
}
