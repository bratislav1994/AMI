using FTN.Common.Logger;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FTN.Common.ClassesForAlarmDB;

namespace SmartCacheCE
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SmartCache : ISmartCacheDuplexForClient, ISmartCacheForCE
    {
        private List<IModelForDuplex> clients = new List<IModelForDuplex>();
        private List<IModelForDuplex> clientsForDeleting = new List<IModelForDuplex>();
        private Dictionary<long, DynamicMeasurement> measurements = new Dictionary<long, DynamicMeasurement>();
        private ICalculationEngineDuplexSmartCache proxyCE;
        private bool firstTimeCE = true;
        private object lockObjectForClient = new object();

        public SmartCache()
        {
            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("SC.SmartCache; line: {0}; SM try to connect to CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    this.ProxyCE.Subscribe();
                    Logger.LogMessageToFile(string.Format("SC.SmartCache; line: {0}; SM is connected to the CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("SC.SmartCache; line: {0}; SM failed to connect to CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    firstTimeCE = true;
                    Thread.Sleep(1000);
                }
            }
        }

        public ICalculationEngineDuplexSmartCache ProxyCE
        {
            get
            {
                if (firstTimeCE)
                {
                    Logger.LogMessageToFile(string.Format("SC.SmartCache.ProxyCE; line: {0}; Create channel between SC and CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    DuplexChannelFactory<ICalculationEngineDuplexSmartCache> factory = new DuplexChannelFactory<ICalculationEngineDuplexSmartCache>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10007/ICalculationEngineDuplexSmartCache/SmartCache"));
                    proxyCE = factory.CreateChannel();
                    firstTimeCE = false;
                }

                Logger.LogMessageToFile(string.Format("SC.SmartCache.ProxyCE; line: {0}; Channel SC-CE is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

                return proxyCE;
            }

            set
            {
                proxyCE = value;
            }
        }

        public void SendMeasurements(Dictionary<long, DynamicMeasurement> measurements)
        {
            Logger.LogMessageToFile(string.Format("SC.SmartCache.SendMeasurements; line: {0}; Measurements received", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            //this.measurements.Clear();

            foreach (KeyValuePair<long, DynamicMeasurement> kvp in measurements)
            {
                if (this.measurements.ContainsKey(kvp.Key))
                {
                    this.measurements[kvp.Key] = kvp.Value;
                }
                else
                {
                    this.measurements.Add(kvp.Key, kvp.Value);
                }
            }

            clientsForDeleting.Clear();

            foreach (IModelForDuplex client in clients)
            {
                try
                {
                    lock (lockObjectForClient)
                    {
                        client.SendMeasurements(this.measurements.Values.ToList());
                    }
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
            Logger.LogMessageToFile(string.Format("SC.SmartCache.SendMeasurements; line: {0}; Measurements sent", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        public void Subscribe()
        {
            lock (lockObjectForClient)
            {
                this.clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());
            }
        }

        public List<DynamicMeasurement> GetLastMeas(List<long> gidsInTable)
        {
            lock (lockObjectForClient)
            {
                List<DynamicMeasurement> retVal = new List<DynamicMeasurement>();
                foreach (long gid in gidsInTable)
                {
                    if (measurements.ContainsKey(gid))
                    {
                        retVal.Add(measurements[gid]);
                    }
                }

                return retVal;
            }
        }

        public void SendAlarm(DeltaForAlarm delta)
        {
            Logger.LogMessageToFile(string.Format("SC.SmartCache.SendAlarm; line: {0}; Alarms sent", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            clientsForDeleting.Clear();

            foreach (IModelForDuplex client in clients)
            {
                try
                {
                    lock (lockObjectForClient)
                    {
                        client.SendAlarm(delta);
                    }
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
            Logger.LogMessageToFile(string.Format("SC.SmartCache.SendAlarm; line: {0}; Alarms sent", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }
    }
}
