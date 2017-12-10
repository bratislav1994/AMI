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
    public class CalculationEngine : ICalculationDuplexClient
    {
        private static CalculationEngine instance;
        private List<IModelForDuplex> clients;
        private List<IModelForDuplex> clientsForDeleting;

        private Thread poziv;

        public CalculationEngine()
        {
            clients = new List<IModelForDuplex>();
            clientsForDeleting = new List<IModelForDuplex>();

            //nit koja glumi scadu tj dobija kao preko nje podatke iz scade --> brisace se
            poziv = new Thread(() => Slanje());
            poziv.Start();
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

        //ovo je fju koja ce povezivati scadu sa ce, primice podatke od scade i u njoj treba da prosledi te podatke clientu
        public void ReceiveData(List<ResourceDescription> measurements)
        {
            if(measurements.Count != 0)
            {
                Console.WriteLine("Receive data from SCADA");
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

        public void Slanje()
        {
            while (true)
            {
                if (clients.Count > 0)
                {
                    List<ResourceDescription> meas = new List<ResourceDescription>();
                    meas.Add(new ResourceDescription(123));
                    ReceiveData(meas);
                    break;
                }
                else
                {
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
