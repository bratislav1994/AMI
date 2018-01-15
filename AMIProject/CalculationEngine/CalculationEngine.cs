using CalculationEngine.Access;
using FTN.Common;
using FTN.Common.Logger;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace CalculationEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CalculationEngine : ICalculationEngine, ICalculationDuplexClient
    {
        private static CalculationEngine instance;
        private ITransactionDuplexCE proxyCoordinator;
        private List<IModelForDuplex> clients;
        private List<IModelForDuplex> clientsForDeleting;
        private List<ResourceDescription> meas;
        private bool firstTimeCoordinator = true;
        private FunctionDB dataBaseAdapter;
        private Dictionary<long, GeographicalRegion> geoRegions;
        private Dictionary<long, SubGeographicalRegion> subGeoRegions;
        private Dictionary<long, Substation> substations;
        private Dictionary<long, EnergyConsumer> amis;
        private Dictionary<long, GeographicalRegion> geoRegionsTemp;
        private Dictionary<long, SubGeographicalRegion> subGeoRegionsTemp;
        private Dictionary<long, Substation> substationsTemp;
        private Dictionary<long, EnergyConsumer> amisTemp;

        public CalculationEngine()
        {
            dataBaseAdapter = new FunctionDB();
            dataBaseAdapter.DoUndone();
            dataBaseAdapter.StartThreads();
            geoRegions = new Dictionary<long, GeographicalRegion>();
            subGeoRegions = new Dictionary<long, SubGeographicalRegion>();
            substations = new Dictionary<long, Substation>();
            amis = new Dictionary<long, EnergyConsumer>();
            /*geoRegions = dataBaseAdapter.ReadGeoRegions();
            subGeoRegions = dataBaseAdapter.ReadSubGeoRegions();
            substations = dataBaseAdapter.ReadSubstations();
            amis = dataBaseAdapter.ReadConsumers();*/
            geoRegionsTemp = new Dictionary<long, GeographicalRegion>();
            subGeoRegionsTemp = new Dictionary<long, SubGeographicalRegion>();
            substationsTemp = new Dictionary<long, Substation>();
            amisTemp = new Dictionary<long, EnergyConsumer>();
            clients = new List<IModelForDuplex>();
            clientsForDeleting = new List<IModelForDuplex>();
            meas = new List<ResourceDescription>();

            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE try to connect with Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    this.ProxyCoordinator.ConnectCE();
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE is connected to the Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE faild to connect with Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    firstTimeCoordinator = true;
                    Thread.Sleep(1000);
                }
            }
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

        public ITransactionDuplexCE ProxyCoordinator
        {
            get
            {
                if (firstTimeCoordinator)
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine.ProxyCoordinator; line: {0}; Create channel between CE and Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    DuplexChannelFactory<ITransactionDuplexCE> factory = new DuplexChannelFactory<ITransactionDuplexCE>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10103/TransactionCoordinator/CE"));
                    proxyCoordinator = factory.CreateChannel();
                    firstTimeCoordinator = false;
                }

                Logger.LogMessageToFile(string.Format("CE.CalculationEngine.ProxyCoordinator; line: {0}; Channel CE-Coordinator is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return proxyCoordinator;
            }

            set
            {
                proxyCoordinator = value;
            }
        }

        public void ConnectClient()
        {
            this.clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());
        }

        public Tuple<List<DynamicMeasurement>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, DateTime to)
        {
            List<DynamicMeasurement> result = new List<DynamicMeasurement>();
            result = dataBaseAdapter.GetMeasForChart(gids, from, to);

            if (result.Count == 0)
            {
                return null;
            }

            DateTime min = result.Min(x => x.TimeStamp);
            DateTime max = result.Max(x => x.TimeStamp);
            
            Dictionary<DateTime, DynamicMeasurement> temp = new Dictionary<DateTime, DynamicMeasurement>();
            
            for (DateTime t = min; t <= max; t = t.AddSeconds(3))
            {
                temp.Add(t, new DynamicMeasurement());
                temp[t].CurrentP = 0;
                temp[t].CurrentQ = 0;
                temp[t].CurrentV = 0;
                temp[t].TimeStamp = t;
            }

            DateTime dt2 = temp.Keys.Last().AddMilliseconds(-temp.Keys.Last().Millisecond);
            temp.Remove(temp.Keys.Last());
            temp.Add(dt2, new DynamicMeasurement() { CurrentP = 0, CurrentQ = 0, CurrentV = 0, TimeStamp = dt2 });
            max = max.AddMilliseconds(-max.Millisecond);

            if (DateTime.Compare(temp.Keys.Last(), max) < 0)
            {
                temp.Add(max, new DynamicMeasurement());
                temp[max].CurrentP = 0;
                temp[max].CurrentQ = 0;
                temp[max].CurrentV = 0;
                temp[max].TimeStamp = max;
            }

            foreach (DynamicMeasurement dm in result)
            {
                DateTime dt = temp.Keys.Where(x => (Math.Abs((x - dm.TimeStamp).TotalSeconds) < 1.5)).FirstOrDefault();
                temp[dt].CurrentP += dm.CurrentP;
                temp[dt].CurrentQ += dm.CurrentQ;
                temp[dt].CurrentV += dm.CurrentV;
            }

            List<DynamicMeasurement> retVal = temp.Values.ToList();

            Statistics statistics = new Statistics();
            statistics.MaxP = retVal.Max(x => x.CurrentP);
            statistics.MaxQ = retVal.Max(x => x.CurrentQ);
            statistics.MaxV = retVal.Max(x => x.CurrentV);
            statistics.MinP = retVal.Min(x => x.CurrentP);
            statistics.MinQ = retVal.Min(x => x.CurrentQ);
            statistics.MinV = retVal.Min(x => x.CurrentV);
            statistics.AvgP = retVal.Average(x => x.CurrentP);
            statistics.AvgQ = retVal.Average(x => x.CurrentQ);
            statistics.AvgV = retVal.Average(x => x.CurrentV);
            statistics.IntegralP = 0;
            statistics.IntegralQ = 0;
            statistics.IntegralV = 0;

            for (int i = 0; i < retVal.Count - 1; i++)
            {
                statistics.IntegralP += (retVal[i].CurrentP * (((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds)) / 3600) + ((((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(retVal[i + 1].CurrentP - retVal[i].CurrentP))) / 2;
                statistics.IntegralQ += (retVal[i].CurrentQ * (((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds)) / 3600) + ((((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(retVal[i + 1].CurrentQ - retVal[i].CurrentQ))) / 2;
                statistics.IntegralV += (retVal[i].CurrentV * (((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds)) / 3600) + ((((float)(retVal[i + 1].TimeStamp - retVal[i].TimeStamp).TotalSeconds) / 3600) * (Math.Abs(retVal[i + 1].CurrentV - retVal[i].CurrentV))) / 2;
            }
            
            return new Tuple<List<DynamicMeasurement>, Statistics>(retVal, statistics);
        }

        public void EnlistMeas(List<ResourceDescription> measurements)
        {
            foreach (ResourceDescription rd in measurements)
            {
                meas.Add(rd);
            }
        }

        public bool Prepare()
        {
            foreach (ResourceDescription rd in meas)
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);

                switch (type)
                {
                    case DMSType.GEOREGION:
                        GeographicalRegion gr = new GeographicalRegion();
                        gr.RD2Class(rd);
                        geoRegionsTemp.Add(rd.Id, gr);
                        break;
                    case DMSType.SUBGEOREGION:
                        SubGeographicalRegion sgr = new SubGeographicalRegion();
                        sgr.RD2Class(rd);
                        subGeoRegionsTemp.Add(rd.Id, sgr);
                        break;
                    case DMSType.SUBSTATION:
                        Substation s = new Substation();
                        s.RD2Class(rd);
                        substationsTemp.Add(rd.Id, s);
                        break;
                    case DMSType.ENERGYCONS:
                        EnergyConsumer ec = new EnergyConsumer();
                        ec.RD2Class(rd);
                        amisTemp.Add(rd.Id, ec);
                        break;
                }
            }

            return true;
        }

        public void Commit()
        {
            /*dataBaseAdapter.AddConsumers(amisTemp.Values.ToList());
            dataBaseAdapter.AddSubstations(substationsTemp.Values.ToList());
            dataBaseAdapter.AddSubGeoRegions(subGeoRegionsTemp.Values.ToList());
            dataBaseAdapter.AddGeoRegions(geoRegionsTemp.Values.ToList());*/

            foreach (KeyValuePair<long, EnergyConsumer> kvp in amisTemp)
            {
                amis.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, Substation> kvp in substationsTemp)
            {
                substations.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, SubGeographicalRegion> kvp in subGeoRegionsTemp)
            {
                subGeoRegions.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, GeographicalRegion> kvp in geoRegionsTemp)
            {
                geoRegions.Add(kvp.Key, kvp.Value);
            }

            this.amisTemp.Clear();
            this.substationsTemp.Clear();
            this.subGeoRegionsTemp.Clear();
            this.geoRegionsTemp.Clear();
            this.meas.Clear();
        }

        public void Rollback()
        {
            this.amisTemp.Clear();
            this.substationsTemp.Clear();
            this.subGeoRegionsTemp.Clear();
            this.geoRegionsTemp.Clear();
            this.meas.Clear();
        }
        
        public void DataFromScada(Dictionary<long, DynamicMeasurement> measurements)
        {
            Logger.LogMessageToFile(string.Format("CE.CalculationEngine.DataFromScada; line: {0}; CE receive data from scada and send this data to client", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            Console.WriteLine("Send data to client");

            dataBaseAdapter.AddMeasurements(measurements.Values.ToList()); 

            Dictionary<long, DynamicMeasurement> addSubstations = new Dictionary<long, DynamicMeasurement>();

            foreach (KeyValuePair<long, Substation> ss in substations)
            {
                DynamicMeasurement m = new DynamicMeasurement(ss.Key);
                
                foreach (KeyValuePair<long, DynamicMeasurement> meas in measurements)
                {
                    if (amis[meas.Key].EqContainer == m.PsrRef)
                    {
                        m.CurrentP += meas.Value.CurrentP;
                        m.CurrentQ += meas.Value.CurrentQ;
                        m.CurrentV += meas.Value.CurrentV;
                    }
                }

                addSubstations.Add(m.PsrRef, m);
            }

            foreach (KeyValuePair<long, DynamicMeasurement> kvp in addSubstations)
            {
                measurements.Add(kvp.Key, kvp.Value);
            }

            Dictionary<long, DynamicMeasurement> addSubGeoRegions = new Dictionary<long, DynamicMeasurement>();

            foreach (KeyValuePair<long, SubGeographicalRegion> sgr in subGeoRegions)
            {
                DynamicMeasurement m = new DynamicMeasurement(sgr.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in addSubstations)
                {
                    if (substations[meas.Key].SubGeoRegion == m.PsrRef)
                    {
                        m.CurrentP += meas.Value.CurrentP;
                        m.CurrentQ += meas.Value.CurrentQ;
                        m.CurrentV += meas.Value.CurrentV;
                    }
                }

                addSubGeoRegions.Add(m.PsrRef, m);
                measurements.Add(m.PsrRef, m);
            }
            
            foreach (KeyValuePair<long, GeographicalRegion> gr in geoRegions)
            {
                DynamicMeasurement m = new DynamicMeasurement(gr.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in addSubGeoRegions)
                {
                    if (subGeoRegions[meas.Key].GeoRegion == m.PsrRef)
                    {
                        m.CurrentP += meas.Value.CurrentP;
                        m.CurrentQ += meas.Value.CurrentQ;
                        m.CurrentV += meas.Value.CurrentV;
                    }
                }

                measurements.Add(m.PsrRef, m);
            }
            
            clientsForDeleting.Clear();
            foreach (IModelForDuplex client in clients)
            {
                try
                {
                    client.SendMeasurements(measurements.Values.ToList());
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

            Logger.LogMessageToFile(string.Format("CE.CalculationEngine.DataFromScada; line: {0}; Finish transport data SCADA-CE-Client", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }
    }
}
