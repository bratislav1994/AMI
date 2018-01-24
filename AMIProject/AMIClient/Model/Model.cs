﻿using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;
using TC57CIM.IEC61970.Meas;
using System.ComponentModel;
using FTN.Common.Logger;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using AMIClient.Classes;

namespace AMIClient
{
    public class Model : IDisposable, IModelForDuplex, INotifyPropertyChanged
    {
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private List<ResourceDescription> meas = new List<ResourceDescription>();
        private Dictionary<long, int> positions = new Dictionary<long, int>();
        private Dictionary<long, int> positionsAmi = new Dictionary<long, int>();
        private ObservableCollection<TableItem> tableItems = new ObservableCollection<TableItem>();
        private ObservableCollection<TableItem> amiTableItems = new ObservableCollection<TableItem>();
        private ObservableCollection<TableItemForAlarm> tableItemsForAlarm = new ObservableCollection<TableItemForAlarm>();
        private RootElement root;
        private bool firstContact = true;
        private bool firstContactCE = true;
        private bool firstContactSC = true;
        private ISmartCacheDuplexForClient scProxy = null;
        private DuplexChannelFactory<ISmartCacheDuplexForClient> factorySC;
        private INetworkModelGDAContractDuplexClient gdaQueryProxy = null;
        private ICalculationForClient ceQueryProxy = null;
        private object lockObj = new object();
        private DuplexChannelFactory<INetworkModelGDAContractDuplexClient> factory;
        private ChannelFactory<ICalculationForClient> factoryCE;
        private Thread checkNMS;
        private ICollectionView viewTableItems;
        //private ICollectionView viewAmiTableItems;
        private ICollectionView viewTableItemsForAlarm;
        private Dictionary<long, DynamicMeasurement> changesForAmis = new Dictionary<long, DynamicMeasurement>();
        private DateTime timeOfLastUpdate = DateTime.Now;

        public INetworkModelGDAContractDuplexClient GdaQueryProxy
        {
            get
            {
                if (FirstContact)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.GdaQueryProxy; line: {0}; Create channel between Client and NMS", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    factory = new DuplexChannelFactory<INetworkModelGDAContractDuplexClient>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10000/NetworkModelService/GDADuplexClient"));
                    gdaQueryProxy = factory.CreateChannel();
                    FirstContact = false;
                }

                Logger.LogMessageToFile(string.Format("AMIClient.Model.GdaQueryProxy; line: {0}; Channel Client-NMS is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return gdaQueryProxy;
            }

            set
            {
                gdaQueryProxy = value;
            }
        }

        public ICalculationForClient CEQueryProxy
        {
            get
            {
                if (FirstContactCE)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.CEQueryProxy; line: {0}; Create channel between Client and CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    factoryCE = new ChannelFactory<ICalculationForClient>(
                        binding,
                        new EndpointAddress("net.tcp://localhost:10006/CalculationEngine/Client"));
                    ceQueryProxy = factoryCE.CreateChannel();
                    FirstContactCE = false;
                }

                Logger.LogMessageToFile(string.Format("AMIClient.Model.CEQueryProxy; line: {0}; Channel Client-CE is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return ceQueryProxy;
            }

            set
            {
                ceQueryProxy = value;
            }
        }

        public ISmartCacheDuplexForClient ScProxy
        {
            get
            {
                if (firstContactSC)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.SCProxy; line: {0}; Create channel between Client and SC", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    factorySC = new DuplexChannelFactory<ISmartCacheDuplexForClient>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10008/SmartCache/Client"));
                    scProxy = factorySC.CreateChannel();
                    firstContactSC = false;
                }

                Logger.LogMessageToFile(string.Format("AMIClient.Model.SCProxy; line: {0}; Channel Client-SC is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

                return scProxy;
            }

            set
            {
                scProxy = value;
            }
        }

        public ObservableCollection<TableItem> TableItems
        {
            get
            {
                return tableItems;
            }

            set
            {
                tableItems = value;
                RaisePropertyChanged("TableItems");
            }
        }

        public ICollectionView ViewTableItems
        {
            get
            {
                return viewTableItems;
            }

            set
            {
                viewTableItems = value;
                RaisePropertyChanged("ViewTableItems");
            }
        }

        public ObservableCollection<TableItem> AmiTableItems
        {
            get
            {
                return amiTableItems;
            }

            set
            {
                amiTableItems = value;
                RaisePropertyChanged("AmiTableItems");
            }
        }

        /*public ICollectionView ViewAmiTableItems
        {
            get
            {
                return viewAmiTableItems;
            }

            set
            {
                viewAmiTableItems = value;
                RaisePropertyChanged("ViewAmiTableItems");
            }
        }*/

        public ObservableCollection<TableItemForAlarm> TableItemsForAlarm
        {
            get
            {
                return tableItemsForAlarm;
            }

            set
            {
                tableItemsForAlarm = value;
                RaisePropertyChanged("TableItemsForAlarm");
            }
        }

        public ICollectionView ViewTableItemsForAlarm
        {
            get
            {
                return viewTableItemsForAlarm;
            }

            set
            {
                viewTableItemsForAlarm = value;
                RaisePropertyChanged("ViewTableItemsForAlarm");
            }
        }

        public bool FirstContact
        {
            get
            {
                return firstContact;
            }

            set
            {
                firstContact = value;
            }
        }

        public bool FirstContactCE
        {
            get
            {
                return firstContactCE;
            }

            set
            {
                firstContactCE = value;
            }
        }
        
        public Model()
        {
            // OBRISATI KASNIJE
            TableItemForAlarm item = new TableItemForAlarm();
            item.Consumer = "Consumer1";
            item.ToPeriod = DateTime.Now;
            item.Status = HelperClasses.Status.ACTIVE;
            item.TypeVoltage = HelperClasses.TypeVoltage.OVERVOLTAGE;
            this.TableItemsForAlarm.Add(item);

            TableItemForAlarm item2 = new TableItemForAlarm();
            item2.Consumer = "Consumer2";
            item2.FromPeriod = new DateTime(2018, 1, 19, 12, 0, 0);
            item2.ToPeriod = DateTime.Now;
            item2.Status = HelperClasses.Status.RESOLVED;
            item2.TypeVoltage = HelperClasses.TypeVoltage.OVERVOLTAGE;
            TableItemsForAlarm.Add(item2);
        }

        public void Start()
        {
            checkNMS = new Thread(() => CheckIfNMSIsAlive());

            Thread t = new Thread(() => ConnectToNMS());
            t.Start();
            
            Thread t2 = new Thread(() => ConnectToCE());
            t2.Start();

            Thread t3 = new Thread(() => ConnectToSC());
            t3.Start();

            t.Join();
            t2.Join();
            t3.Join();
        }

        private void ConnectToSC()
        {
            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToSC; line: {0}; Client try to connect to SC", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    ScProxy.Subscribe();
                    ((IContextChannel)ScProxy).OperationTimeout = TimeSpan.FromMinutes(5);
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToSC; line: {0}; Client is connected to the SC", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToSC; line: {0}; Client faild to connect to SC. CATCH", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    firstContactSC = true;
                    Thread.Sleep(1000);
                }
            }
        }

        private void ConnectToCE()
        {
            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToCE; line: {0}; Client try to connect with CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    CEQueryProxy.ConnectClient();
                    //factoryCE.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(1);
                    ((IContextChannel)CEQueryProxy).OperationTimeout = TimeSpan.FromMinutes(5);
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToCE; line: {0}; Client is connected to the CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToCE; line: {0}; Client faild to connect with CE. CATCH", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    FirstContactCE = true;
                    Thread.Sleep(1000);
                }
            }
        }

        private void ConnectToNMS()
        {
            while (true)
            {
                try
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToNMS; line: {0}; Client try to connect with NMS", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    GdaQueryProxy.ConnectClient();
                    //factory.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(1);
                    ((IContextChannel)GdaQueryProxy).OperationTimeout = TimeSpan.FromMinutes(5);
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToNMS; line: {0}; Client is connected to the NMS", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    checkNMS.Start();

                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToNMS; line: {0}; Client faild to connect with NMS. CATCH", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    FirstContact = true;
                    Thread.Sleep(1000);
                }
            }
        }

        private void CheckIfNMSIsAlive()
        {
            while (true)
            {
                try
                {
                    GdaQueryProxy.Ping();
                }
                catch
                {
                    FirstContact = true;
                    new Thread(() => ConnectToNMS()).Start();
                    break;
                }

                Thread.Sleep(3000);
            }
        }

        public void SetRoot(RootElement root)
        {
            this.root = root;
        }

        #region GDAQueryService

        public List<GeographicalRegion> GetAllRegions(bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllRegions; line: {0}; Start the GetAllRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<IdentifiedObject> results = GetExtentValues(ModelCode.GEOREGION);
            List<GeographicalRegion> retVal = new List<GeographicalRegion>();

            foreach (GeographicalRegion gr in results)
            {
                retVal.Add(gr);
            }

            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllRegions; line: {0}; Start the GetAllRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            return retVal;
        }

        public List<IdentifiedObject> GetAllAmis()
        {
            return GetExtentValues(ModelCode.ENERGYCONS);
        }

        public void GetAllTableItems(bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<IdentifiedObject> geoRegions = GetExtentValues(ModelCode.GEOREGION);

            ClearTableItems();
            ClearPositions();

            foreach (IdentifiedObject io in geoRegions)
            {
                TableItems.Add(new TableItem(io) { Type = HelperClasses.DataGridType.GEOGRAPHICALREGION });
                positions.Add(io.GlobalId, TableItems.Count - 1);
                List<IdentifiedObject> subGeoRegions = GetSomeSubregions(io.GlobalId, true);

                foreach (IdentifiedObject io1 in subGeoRegions)
                {
                    TableItems.Add(new TableItem(io1) { Type = HelperClasses.DataGridType.SUBGEOGRAPHICALREGION });
                    positions.Add(io1.GlobalId, TableItems.Count - 1);
                    List<IdentifiedObject> substations = GetSomeSubstations(io1.GlobalId, true);

                    foreach (IdentifiedObject io2 in substations)
                    {
                        TableItems.Add(new TableItem(io2) { Type = HelperClasses.DataGridType.SUBSTATION });
                        positions.Add(io2.GlobalId, TableItems.Count - 1);
                    }
                }
            }

            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private List<IdentifiedObject> GetExtentValues(ModelCode modelCode)
        {
            string message = "Getting extent values method started.";
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            int iteratorId = 0;
            List<long> ids = new List<long>();
            List<IdentifiedObject> retVal = new List<IdentifiedObject>();

            try
            {
                int numberOfResources = 2;
                int resourcesLeft = 0;

                List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(modelCode);

                iteratorId = GdaQueryProxy.GetExtentValues(modelCode, properties);
                resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);

                    for (int i = 0; i < rds.Count; i++)
                    {
                        switch (modelCode)
                        {
                            case ModelCode.GEOREGION:
                                GeographicalRegion gr = new GeographicalRegion();
                                gr.RD2Class(rds[i]);
                                retVal.Add(gr);
                                break;
                            case ModelCode.SUBGEOREGION:
                                SubGeographicalRegion sgr = new SubGeographicalRegion();
                                sgr.RD2Class(rds[i]);
                                retVal.Add(sgr);
                                break;
                            case ModelCode.SUBSTATION:
                                Substation ss = new Substation();
                                ss.RD2Class(rds[i]);
                                retVal.Add(ss);
                                break;
                            case ModelCode.ENERGYCONS:
                                EnergyConsumer ec = new EnergyConsumer();
                                ec.RD2Class(rds[i]);
                                retVal.Add(ec);
                                break;

                            default:
                                break;
                        }

                        ids.Add(rds[i].Id);
                    }

                    resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
                }

                GdaQueryProxy.IteratorClose(iteratorId);

                message = "Getting extent values method successfully finished.";
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }
            catch (Exception e)
            {
                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCode, e.Message);
                MessageBox.Show(e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);

                if (!FirstContact)
                {
                    FirstContact = true;
                    new Thread(() => ConnectToNMS()).Start();
                }
            }

            return retVal;
        }

        public List<IdentifiedObject> GetSomeSubregions(long regionId, bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Start the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
            associtaion.Type = ModelCode.SUBGEOREGION;
            List<IdentifiedObject> results = GetRelatedValues(regionId, properties, associtaion, ModelCode.SUBGEOREGION);
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Finish the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            return results;
        }

        public List<IdentifiedObject> GetSomeSubstations(long subRegionId, bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Start the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBSTATION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.SUBGEOREGION_SUBS;
            associtaion.Type = ModelCode.SUBSTATION;
            GetRelatedValues(subRegionId, properties, associtaion, ModelCode.SUBSTATION);
            List<IdentifiedObject> results = GetRelatedValues(subRegionId, properties, associtaion, ModelCode.SUBSTATION);
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Start the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            return results;
        }

        public List<IdentifiedObject> GetSomeAmis(long substationId)
        {
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.EQCONTAINER_EQUIPMENTS;
            associtaion.Type = ModelCode.ENERGYCONS;
            List<IdentifiedObject> results = GetRelatedValues(substationId, properties, associtaion, ModelCode.ENERGYCONS);

            return results;
        }

        //public void GetSomeTableItems(long substationId, bool returnValue)
        //{
        //    Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeAmis; line: {0}; Start the GetSomeAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        //    List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
        //    Association associtaion = new Association();
        //    associtaion.PropertyId = ModelCode.EQCONTAINER_EQUIPMENTS;
        //    associtaion.Type = ModelCode.ENERGYCONS;
        //    List<IdentifiedObject> results = GetRelatedValues(substationId, properties, associtaion, ModelCode.ENERGYCONS);

        //    foreach (EnergyConsumer ec in results)
        //    {
        //        AmiTableItems.Add(new TableItem(ec) { Type = HelperClasses.DataGridType.ENERGY_CONSUMER });
        //        positionsAmi.Add(ec.GlobalId, TableItems.Count - 1);
        //    }

        //    Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeAmis; line: {0}; Finish the GetSomeAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        //}

        private List<IdentifiedObject> GetRelatedValues(long source, List<ModelCode> propIds, Association association, ModelCode modelCode)
        {
            List<IdentifiedObject> retVal = new List<IdentifiedObject>();
            try
            {
                int iteratorId = GdaQueryProxy.GetRelatedValues(source, propIds, association);
                int resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

                int numberOfResources = 10;
                List<ResourceDescription> results = new List<ResourceDescription>();

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                    results.AddRange(rds);
                    resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
                }

                switch (modelCode)
                {
                    case ModelCode.SUBGEOREGION:
                        foreach (ResourceDescription rd in results)
                        {
                            SubGeographicalRegion sgr = new SubGeographicalRegion();
                            sgr.RD2Class(rd);
                            retVal.Add(sgr);
                        }
                        break;
                    case ModelCode.SUBSTATION:
                        foreach (ResourceDescription rd in results)
                        {
                            Substation ss = new Substation();
                            ss.RD2Class(rd);
                            retVal.Add(ss);
                        }
                        break;
                    case ModelCode.ENERGYCONS:
                        foreach (ResourceDescription rd in results)
                        {
                            EnergyConsumer ec = new EnergyConsumer();
                            ec.RD2Class(rd);
                            retVal.Add(ec);
                        }
                        break;

                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                string message = string.Format("Getting related values method failed for {0}.\n\t{1}", modelCode, e.Message);
                MessageBox.Show(e.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);

                if (!FirstContact)
                {
                    FirstContact = true;
                    new Thread(() => ConnectToNMS()).Start();
                }
            }
            return retVal;
        }

        public void GetSomeTableItemsForGeoRegion(long globalId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            ClearTableItems();
            ClearPositions();
            List<IdentifiedObject> subGeoRegions = GetSomeSubregions(globalId, true);

            foreach (IdentifiedObject io1 in subGeoRegions)
            {
                TableItems.Add(new TableItem(io1) { Type = HelperClasses.DataGridType.SUBGEOGRAPHICALREGION });
                positions.Add(io1.GlobalId, TableItems.Count - 1);
                List<IdentifiedObject> substations = GetSomeSubstations(io1.GlobalId, true);

                foreach (IdentifiedObject io2 in substations)
                {
                    TableItems.Add(new TableItem(io2) { Type = HelperClasses.DataGridType.SUBSTATION });
                    positions.Add(io2.GlobalId, TableItems.Count - 1);
                }
            }
        }

        public void GetSomeTableItemsForSubGeoRegion(long globalId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            ClearTableItems();
            ClearPositions();
            List<IdentifiedObject> substations = GetSomeSubstations(globalId, true);

            foreach (IdentifiedObject io2 in substations)
            {
                TableItems.Add(new TableItem(io2) { Type = HelperClasses.DataGridType.SUBSTATION });
                positions.Add(io2.GlobalId, TableItems.Count - 1);
            }
        }

        #endregion GDAQueryService

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void NewDeltaApplied()
        {
            lock (this.root.LockObject)
            {
                this.root.NeedsUpdate = true;
            }
        }

        public void SendMeasurements(List<DynamicMeasurement> measurements)
        {
            UpdateTables(measurements);
            this.timeOfLastUpdate = DateTime.Now;
        }

        private void UpdateTables(List<DynamicMeasurement> measurements)
        {
            lock (lockObj)
            {
                if (TableItems.Count == 0)
                {
                    positions.Clear();
                }

                changesForAmis.Clear();

                foreach (DynamicMeasurement dm in measurements)
                {
                    if (positions.ContainsKey(dm.PsrRef))
                    {
                        TableItems[positions[dm.PsrRef]].CurrentP = dm.CurrentP != -1 ? dm.CurrentP : TableItems[positions[dm.PsrRef]].CurrentP;
                        TableItems[positions[dm.PsrRef]].CurrentQ = dm.CurrentQ != -1 ? dm.CurrentQ : TableItems[positions[dm.PsrRef]].CurrentQ;
                        TableItems[positions[dm.PsrRef]].CurrentV = dm.CurrentV != -1 ? dm.CurrentV : TableItems[positions[dm.PsrRef]].CurrentV;
                    }

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(dm.PsrRef) == DMSType.ENERGYCONS)
                    {
                        changesForAmis.Add(dm.PsrRef, dm);
                    }
                }
            }
        }

        public void GetLastMeasurements()
        {
            this.UpdateTables(this.ScProxy.GetLastMeas());
        }

        public bool NewChangesAvailable(DateTime lastUpdate)
        {
            return DateTime.Compare(lastUpdate, this.timeOfLastUpdate) < 0;
        }

        public Dictionary<long, DynamicMeasurement> GetChanges(List<long> gids)
        {
            Dictionary<long, DynamicMeasurement> retVal = new Dictionary<long, DynamicMeasurement>();

            lock (lockObj)
            {
                foreach (long gid in gids)
                {
                    if (changesForAmis.ContainsKey(gid))
                    {
                        retVal.Add(gid, changesForAmis[gid]);
                    }
                }
            }

            return retVal;
        }

        public DateTime GetTimeOfTheLastUpdate()
        {
            return this.timeOfLastUpdate;
        }

        public void ClearTableItems()
        {
            lock (lockObj)
            {
                this.TableItems.Clear();
            }
        }

        public void ClearPositions()
        {
            lock (lockObj)
            {
                this.positions.Clear();
            }
        }

        public Tuple<List<Statistics>, Statistics> GetMeasForChart(List<long> gids, DateTime from, ResolutionType resolution)
        {
            return this.CEQueryProxy.GetMeasurementsForChartView(gids, from, resolution);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
