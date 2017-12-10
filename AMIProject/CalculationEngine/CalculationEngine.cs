using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CalculationEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CalculationEngine : ICalculationEngine, ICalculationDuplexClient
    {
        private static CalculationEngine instance;
        private List<IModelForDuplex> clients;
        private List<IModelForDuplex> clientsForDeleting;
        private List<ResourceDescription> meas;

        public CalculationEngine()
        {
            clients = new List<IModelForDuplex>();
            clientsForDeleting = new List<IModelForDuplex>();

            meas = new List<ResourceDescription>();
        }

        public static CalculationEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CalculationEngine();
                }

                return instance;
            }
        }

        public void ConncetClient()
        {
            this.clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());
        }

        public void DataFromScada(List<ResourceDescription> measurements)
        {
            Console.WriteLine("Receive data from SCADA");
            this.meas = measurements;

            Console.WriteLine("Send data to client");

            clientsForDeleting.Clear();
            foreach (IModelForDuplex client in clients)
            {
                try
                {
                    client.SendMeasurements(measurements);
                }
                catch
                {
                    clientsForDeleting.Add(client);
                }
            }

            foreach (IModelForDuplex client in clientsForDeleting)
            {
                clients.Remove(client);
            }
        }
    }
}
