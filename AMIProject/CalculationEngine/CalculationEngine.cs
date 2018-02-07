using CalculationEngine.Access;
using FTN.Common;
using FTN.Common.ClassesForAlarmDB;
using FTN.Common.Filter;
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
    public class CalculationEngine : ICalculationEngine, ICalculationForClient, ICalculationEngineDuplexSmartCache, ICalculationEngineForScript
    {
        private static CalculationEngine instance;
        private ITransactionDuplexCE proxyCoordinator;
        private List<ISmartCacheForCE> smartCaches;
        private List<ISmartCacheForCE> smartCachesForDeleting;
        private List<ResourceDescription> meas;
        private bool firstTimeCoordinator = true;
        private bool firstTimeScada = true;
        private TSDB timeSeriesDataBaseAdapter;
        private DB dataBaseAdapter;
        private Dictionary<long, GeographicalRegionDb> geoRegions;
        private Dictionary<long, SubGeographicalRegionDb> subGeoRegions;
        private Dictionary<long, SubstationDb> substations;
        private Dictionary<long, EnergyConsumerDb> amis;
        private Dictionary<long, GeographicalRegionDb> geoRegionsTemp;
        private Dictionary<long, SubGeographicalRegionDb> subGeoRegionsTemp;
        private Dictionary<long, SubstationDb> substationsTemp;
        private Dictionary<long, EnergyConsumerDb> amisTemp;
        private Dictionary<long, ActiveAlarm> alarmActiveDB;
        private IScadaForCECommand proxyScada;
        private List<ResolvedAlarm> listAfterRefresh;


        public CalculationEngine()
        {
            smartCaches = new List<ISmartCacheForCE>();
            smartCachesForDeleting = new List<ISmartCacheForCE>();
            dataBaseAdapter = new DB();
            timeSeriesDataBaseAdapter = new TSDB();
            timeSeriesDataBaseAdapter.DbAdapter = dataBaseAdapter;
            // ako se ove 2 linije zakomentarisu, narednih 5 se otkomentarisu i obrnuto
            //timeSeriesDataBaseAdapter.DoUndone();
            //timeSeriesDataBaseAdapter.StartThreads();
            //
            //Filler f = new Filler();
            //f.DbAdapter = dataBaseAdapter;
            //f.TimeSeriesDbAdapter = timeSeriesDataBaseAdapter;
            //f.Fill();
            //this.DoUndoneFill();
            //
            geoRegions = new Dictionary<long, GeographicalRegionDb>();
            subGeoRegions = new Dictionary<long, SubGeographicalRegionDb>();
            substations = new Dictionary<long, SubstationDb>();
            amis = new Dictionary<long, EnergyConsumerDb>();
            geoRegions = dataBaseAdapter.ReadGeoRegions();
            subGeoRegions = dataBaseAdapter.ReadSubGeoRegions();
            substations = dataBaseAdapter.ReadSubstations();
            amis = dataBaseAdapter.ReadConsumers();
            geoRegionsTemp = new Dictionary<long, GeographicalRegionDb>();
            subGeoRegionsTemp = new Dictionary<long, SubGeographicalRegionDb>();
            substationsTemp = new Dictionary<long, SubstationDb>();
            amisTemp = new Dictionary<long, EnergyConsumerDb>();
            meas = new List<ResourceDescription>();
            alarmActiveDB = new Dictionary<long, ActiveAlarm>();
            listAfterRefresh = new List<ResolvedAlarm>();
            alarmActiveDB = dataBaseAdapter.ReadActiveAlarm();

            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE try to connect to Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    this.ProxyCoordinator.ConnectCE();
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE is connected to the Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine; line: {0}; CE failed to connect to Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
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
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
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

        public IScadaForCECommand ProxyScada
        {
            get
            {
                if (firstTimeScada)
                {
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine.ProxyScada; line: {0}; Create channel between CE and Scada", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromHours(1);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    ChannelFactory<IScadaForCECommand> factory = new ChannelFactory<IScadaForCECommand>(
                        binding,
                        new EndpointAddress("net.tcp://localhost:10012/Scada/CE"));
                    proxyScada = factory.CreateChannel();
                    firstTimeScada = false;
                    Logger.LogMessageToFile(string.Format("CE.CalculationEngine.ProxyScada; line: {0}; Channel CE-Scada is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                }

                return proxyScada;
            }

            set
            {
                proxyScada = value;
            }
        }

        public void ConnectClient()
        {
            return;
        }

        public Tuple<List<Statistics>, Statistics> GetMeasurementsForChartView(List<long> gids, DateTime from, ResolutionType resolution)
        {
            List<Statistics> result = new List<Statistics>();

            switch (resolution)
            {
                case ResolutionType.MINUTE:
                    result = this.timeSeriesDataBaseAdapter.ReadMinuteAggregationTable(gids, from);
                    break;
                case ResolutionType.HOUR:
                    result = this.timeSeriesDataBaseAdapter.ReadHourAggregationTable(gids, from);
                    break;
                case ResolutionType.DAY:
                    result = this.timeSeriesDataBaseAdapter.ReadDayAggregationTable(gids, from);
                    break;
            }

            if (result.Count == 0)
            {
                return null;
            }
            Statistics statistics = FillStatistic(result);
            
            return new Tuple<List<Statistics>, Statistics>(result, statistics);
        }

        private static Statistics FillStatistic(List<Statistics> result)
        {
            Statistics statistics = new Statistics();
            statistics.MaxP = result.Max(x => x.AvgP);
            statistics.MaxQ = result.Max(x => x.AvgQ);
            statistics.MaxV = result.Max(x => x.MaxV);
            statistics.MinP = result.Min(x => x.AvgP);
            statistics.MinQ = result.Min(x => x.AvgQ);
            statistics.MinV = result.Min(x => x.MinV);
            statistics.AvgP = result.Average(x => x.AvgP);
            statistics.AvgQ = result.Average(x => x.AvgQ);
            statistics.AvgV = result.Average(x => x.AvgV);
            statistics.IntegralP = 0;
            statistics.IntegralQ = 0;

            for (int i = 0; i < result.Count - 1; i++)
            {
                statistics.IntegralP += result[i].IntegralP;
                statistics.IntegralQ += result[i].IntegralQ;
            }

            return statistics;
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
                        GeographicalRegionDb grCE = new GeographicalRegionDb(gr);
                        geoRegionsTemp.Add(rd.Id, grCE);
                        break;
                    case DMSType.SUBGEOREGION:
                        SubGeographicalRegion sgr = new SubGeographicalRegion();
                        sgr.RD2Class(rd);
                        sgr.GlobalId = rd.Id;
                        SubGeographicalRegionDb sgrCE = new SubGeographicalRegionDb(sgr);
                        subGeoRegionsTemp.Add(rd.Id, sgrCE);
                        break;
                    case DMSType.SUBSTATION:
                        Substation s = new Substation();
                        s.RD2Class(rd);
                        s.GlobalId = rd.Id;
                        SubstationDb sCE = new SubstationDb(s);
                        substationsTemp.Add(rd.Id, sCE);
                        break;
                    case DMSType.ENERGYCONS:
                        EnergyConsumer ec = new EnergyConsumer();
                        ec.RD2Class(rd);
                        ec.GlobalId = rd.Id;
                        EnergyConsumerDb ecCE = new EnergyConsumerDb(ec);
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

            foreach (KeyValuePair<long, EnergyConsumerDb> kvp in amisTemp)
            {
                amis.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, SubstationDb> kvp in substationsTemp)
            {
                substations.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, SubGeographicalRegionDb> kvp in subGeoRegionsTemp)
            {
                subGeoRegions.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<long, GeographicalRegionDb> kvp in geoRegionsTemp)
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
            int cntForVoltage = 0;
            Dictionary<long, DynamicMeasurement> addSubstations = new Dictionary<long, DynamicMeasurement>();

            timeSeriesDataBaseAdapter.AddMeasurements(measurements.Values.ToList());
            DeltaForAlarm delta = CheckAlarms(measurements.Values.ToList());

            foreach (KeyValuePair<long, SubstationDb> ss in substations)
            {
                bool toBeAdded = false;
                DynamicMeasurement m = new DynamicMeasurement(ss.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in measurements)
                {
                    if (amis[meas.Key].EqContainerID == m.PsrRef)
                    {
                        toBeAdded = true;
                        m.CurrentP += meas.Value.CurrentP;
                        m.CurrentQ += meas.Value.CurrentQ;
                        m.CurrentV += meas.Value.CurrentV;
                        ++cntForVoltage;

                        if (meas.Value.IsAlarm)
                        {
                            m.IsAlarm = true;
                        }
                    }
                }

                if (m.CurrentV > 0)
                {
                    m.CurrentV /= cntForVoltage;
                }
                cntForVoltage = 0;

                if (toBeAdded)
                {
                    addSubstations.Add(m.PsrRef, m);
                }
            }

            foreach (KeyValuePair<long, DynamicMeasurement> kvp in addSubstations)
            {
                measurements.Add(kvp.Key, kvp.Value);
            }

            Dictionary<long, DynamicMeasurement> addSubGeoRegions = new Dictionary<long, DynamicMeasurement>();

            foreach (KeyValuePair<long, SubGeographicalRegionDb> sgr in subGeoRegions)
            {
                bool toBeAdded = false;
                DynamicMeasurement m = new DynamicMeasurement(sgr.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in addSubstations)
                {
                    if (substations[meas.Key].SubGeoRegionID == m.PsrRef)
                    {
                        toBeAdded = true;
                        m.CurrentP += meas.Value.CurrentP;
                        m.CurrentQ += meas.Value.CurrentQ;
                        m.CurrentV += meas.Value.CurrentV;
                        ++cntForVoltage;

                        if (meas.Value.IsAlarm)
                        {
                            m.IsAlarm = true;
                        }
                    }
                }

                if (m.CurrentV > 0)
                {
                    m.CurrentV /= cntForVoltage;
                }
                cntForVoltage = 0;
                if (toBeAdded)
                {
                    addSubGeoRegions.Add(m.PsrRef, m);
                    measurements.Add(m.PsrRef, m);
                }
            }

            foreach (KeyValuePair<long, GeographicalRegionDb> gr in geoRegions)
            {
                bool toBeAdded = false;
                DynamicMeasurement m = new DynamicMeasurement(gr.Key);

                foreach (KeyValuePair<long, DynamicMeasurement> meas in addSubGeoRegions)
                {
                    if (subGeoRegions[meas.Key].GeoRegionID == m.PsrRef)
                    {
                        toBeAdded = true;
                        m.CurrentP += meas.Value.CurrentP;
                        m.CurrentQ += meas.Value.CurrentQ;
                        m.CurrentV += meas.Value.CurrentV;
                        ++cntForVoltage;

                        if (meas.Value.IsAlarm)
                        {
                            m.IsAlarm = true;
                        }
                    }
                }

                if (m.CurrentV > 0)
                {
                    m.CurrentV /= cntForVoltage;
                }

                cntForVoltage = 0;

                if (toBeAdded)
                {
                    measurements.Add(m.PsrRef, m);
                }
            }

            smartCachesForDeleting.Clear();

            foreach (ISmartCacheForCE sc in smartCaches)
            {
                try
                {
                    sc.SendMeasurements(measurements);
                    sc.SendAlarm(delta);
                }
                catch
                {
                    smartCachesForDeleting.Add(sc);
                }
            }

            foreach (ISmartCacheForCE sc in smartCachesForDeleting)
            {
                smartCaches.Remove(sc);
            }

            Logger.LogMessageToFile(string.Format("CE.CalculationEngine.DataFromScada; line: {0}; Finish transport data SCADA-CE-Client", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private DeltaForAlarm CheckAlarms(List<DynamicMeasurement> measurements)
        {
            DeltaForAlarm delta = new DeltaForAlarm();
            Dictionary<long, DynamicMeasurement> gidsInAlarm = new Dictionary<long, DynamicMeasurement>();

            foreach (DynamicMeasurement dm in measurements)
            {
                if (!dm.IsAlarm)
                {
                    if (alarmActiveDB.ContainsKey(dm.PsrRef))
                    {
                        ActiveAlarm alarmA = alarmActiveDB[dm.PsrRef];

                        ResolvedAlarm alarmR = new ResolvedAlarm();
                        alarmR.FromPeriod = alarmA.FromPeriod;
                        alarmR.Id = alarmA.Id;
                        alarmR.Consumer = alarmA.Consumer;
                        alarmR.ToPeriod = dm.TimeStamp;
                        alarmR.TypeVoltage = alarmA.TypeVoltage;

                        dataBaseAdapter.AddResolvedAlarm(alarmR);
                        dataBaseAdapter.DeleteActiveAlarm(alarmA);
                        alarmActiveDB.Remove(dm.PsrRef);
                        delta.DeleteOperations.Add(alarmA.Id);
                    }
                }
                else
                {
                    gidsInAlarm.Add(dm.PsrRef, dm);

                    if (!alarmActiveDB.ContainsKey(dm.PsrRef))
                    {
                        ActiveAlarm a = new ActiveAlarm();
                        a.FromPeriod = dm.TimeStamp;
                        a.Id = dm.PsrRef;
                        a.Voltage = dm.CurrentV;
                        a.TypeVoltage = dm.TypeVoltage;
                        if (amis.ContainsKey(dm.PsrRef))
                        {
                            a.Consumer = amis[dm.PsrRef].Name;
                        }

                        alarmActiveDB[dm.PsrRef] = a;
                        dataBaseAdapter.AddActiveAlarm(a);
                        delta.InsertOperations.Add(a);
                    }
                }
            }

            if (gidsInAlarm.Count > 0)
            {
                Console.WriteLine(ProxyScada.Command(gidsInAlarm));
            }

            return delta;
        }

        public void Subscribe()
        {
            this.smartCaches.Add(OperationContext.Current.GetCallbackChannel<ISmartCacheForCE>());
        }

        public void FillDataBase(List<DynamicMeasurement> measurements)
        {
            timeSeriesDataBaseAdapter.AddMeasurements(measurements);
        }

        public void DoUndoneFill()
        {
            timeSeriesDataBaseAdapter.DoUndoneFill();
        }

        public int GetTotalPageCount()
        {
            listAfterRefresh.Clear();
            listAfterRefresh = dataBaseAdapter.ReadResolvedAlarm();

            return listAfterRefresh.Count();
        }

        public List<ResolvedAlarm> GetResolvedAlarms(int startIndex, int range)
        {
            if (startIndex + range > listAfterRefresh.Count - 1)
            {
                return listAfterRefresh.GetRange(startIndex, listAfterRefresh.Count - startIndex);
            }
            else
            {
                return listAfterRefresh.GetRange(startIndex, range);
            }
        }

        public Tuple<List<Statistics>, Statistics> GetMeasurementsForChartViewByFilter(List<long> gids, Filter filter)
        {
            List<Statistics> result = this.timeSeriesDataBaseAdapter.ReadHourAggregationTableByFilter(gids, filter);

            return null;
        }
    }
}
