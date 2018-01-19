using CalculationEngine.Access;
using CalculationEngine.Class;
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
        private Dictionary<long, GeographicalRegionCE> geoRegions;
        private Dictionary<long, SubGeographicalRegionCE> subGeoRegions;
        private Dictionary<long, SubstationCE> substations;
        private Dictionary<long, EnergyConsumerCE> amis;
        private Dictionary<long, GeographicalRegionCE> geoRegionsTemp;
        private Dictionary<long, SubGeographicalRegionCE> subGeoRegionsTemp;
        private Dictionary<long, SubstationCE> substationsTemp;
        private Dictionary<long, EnergyConsumerCE> amisTemp;

        public CalculationEngine()
        {
            dataBaseAdapter = new FunctionDB();
            dataBaseAdapter.DoUndone();
            dataBaseAdapter.StartThreads();
            geoRegions = new Dictionary<long, GeographicalRegionCE>();
            subGeoRegions = new Dictionary<long, SubGeographicalRegionCE>();
            substations = new Dictionary<long, SubstationCE>();
            amis = new Dictionary<long, EnergyConsumerCE>();
            geoRegions = dataBaseAdapter.ReadGeoRegions();
            subGeoRegions = dataBaseAdapter.ReadSubGeoRegions();
            substations = dataBaseAdapter.ReadSubstations();
            amis = dataBaseAdapter.ReadConsumers();
            geoRegionsTemp = new Dictionary<long, GeographicalRegionCE>();
            subGeoRegionsTemp = new Dictionary<long, SubGeographicalRegionCE>();
            substationsTemp = new Dictionary<long, SubstationCE>();
            amisTemp = new Dictionary<long, EnergyConsumerCE>();
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

        public Tuple<List<Statistics>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, ResolutionType resolution)
        {
            List<Statistics> result = new List<Statistics>();

            switch (resolution)
            {
                case ResolutionType.MINUTE:
                    result = this.dataBaseAdapter.ReadMinuteAggregationTable(gids, from);
                    break;
                case ResolutionType.HOUR:
                    result = this.dataBaseAdapter.ReadHourAggregationTable(gids, from);
                    break;
                case ResolutionType.DAY:
                    result = this.dataBaseAdapter.ReadDayAggregationTable(gids, from);
                    break;
            }

            if (result.Count == 0)
            {
                return null;
            }

            Statistics statistics = new Statistics();
            statistics.MaxP = result.Max(x => x.AvgP);
            statistics.MaxQ = result.Max(x => x.AvgQ);
            statistics.MaxV = result.Max(x => x.AvgV);
            statistics.MinP = result.Min(x => x.AvgP);
            statistics.MinQ = result.Min(x => x.AvgQ);
            statistics.MinV = result.Min(x => x.AvgV);
            statistics.AvgP = result.Average(x => x.AvgP);
            statistics.AvgQ = result.Average(x => x.AvgQ);
            statistics.AvgV = result.Average(x => x.AvgV);
            statistics.IntegralP = 0;
            statistics.IntegralQ = 0;
            statistics.IntegralV = 0;

            for (int i = 0; i < result.Count - 1; i++)
            {
                statistics.IntegralP += result[i].IntegralP;
                statistics.IntegralQ += result[i].IntegralQ;
                statistics.IntegralV += result[i].IntegralV;
            }

            return new Tuple<List<Statistics>, Statistics>(result, statistics);
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
                        gr.GlobalId = rd.Id;
                        GeographicalRegionCE grCE = new GeographicalRegionCE(gr);
                        geoRegionsTemp.Add(rd.Id, grCE);
                        break;
                    case DMSType.SUBGEOREGION:
                        SubGeographicalRegion sgr = new SubGeographicalRegion();
                        sgr.RD2Class(rd);
                        sgr.GlobalId = rd.Id;
                        SubGeographicalRegionCE sgrCE = new SubGeographicalRegionCE(sgr);
                        subGeoRegionsTemp.Add(rd.Id, sgrCE);
                        break;
                    case DMSType.SUBSTATION:
                        Substation s = new Substation();
                        s.RD2Class(rd);
                        s.GlobalId = rd.Id;
                        SubstationCE sCE = new SubstationCE(s);
                        substationsTemp.Add(rd.Id, sCE);
                        break;
                    case DMSType.ENERGYCONS:
                        EnergyConsumer ec = new EnergyConsumer();
                        ec.RD2Class(rd);
                        ec.GlobalId = rd.Id;
                        EnergyConsumerCE ecCE = new EnergyConsumerCE(ec);
                        amisTemp.Add(rd.Id, ecCE);
                        break;
                }
            }

            return true;
        }

        public void Commit()
        {
            dataBaseAdapter.AddGeoRegions(geoRegionsTemp.Values.ToList());
            dataBaseAdapter.AddSubGeoRegions(subGeoRegionsTemp.Values.ToList());
            dataBaseAdapter.AddSubstations(substationsTemp.Values.ToList());
            dataBaseAdapter.AddConsumers(amisTemp.Values.ToList());

            foreach (KeyValuePair<long, EnergyConsumerCE> kvp in amisTemp)
            {
                amis.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, SubstationCE> kvp in substationsTemp)
            {
                substations.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, SubGeographicalRegionCE> kvp in subGeoRegionsTemp)
            {
                subGeoRegions.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, GeographicalRegionCE> kvp in geoRegionsTemp)
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

            foreach (KeyValuePair<long, SubstationCE> ss in substations)
            {
                DynamicMeasurement m = new DynamicMeasurement(ss.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in measurements)
                {
                    if (amis[meas.Key].EqContainerID == m.PsrRef)
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

            foreach (KeyValuePair<long, SubGeographicalRegionCE> sgr in subGeoRegions)
            {
                DynamicMeasurement m = new DynamicMeasurement(sgr.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in addSubstations)
                {
                    if (substations[meas.Key].SubGeoRegionID == m.PsrRef)
                    {
                        m.CurrentP += meas.Value.CurrentP;
                        m.CurrentQ += meas.Value.CurrentQ;
                        m.CurrentV += meas.Value.CurrentV;
                    }
                }

                addSubGeoRegions.Add(m.PsrRef, m);
                measurements.Add(m.PsrRef, m);
            }

            foreach (KeyValuePair<long, GeographicalRegionCE> gr in geoRegions)
            {
                DynamicMeasurement m = new DynamicMeasurement(gr.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in addSubGeoRegions)
                {
                    if (subGeoRegions[meas.Key].GeoRegionID == m.PsrRef)
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
