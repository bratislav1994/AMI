using FTN.Common;
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
using System.Windows.Media;
using FTN.Common.ClassesForAlarmDB;
using FTN.Common.Filter;
using AMIClient.HelperClasses;
using ToastNotifications.Messages;

namespace AMIClient
{
    public class Model : IDisposable, IModelForDuplex, INotifyPropertyChanged
    {
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private List<ResourceDescription> meas = new List<ResourceDescription>();
        private Dictionary<long, int> positions = new Dictionary<long, int>();
        private Dictionary<long, int> positionsAmi = new Dictionary<long, int>();
        private Dictionary<long, int> positionsAlarm = new Dictionary<long, int>();
        private List<TableItem> tableItems = new List<TableItem>();
        private List<ActiveAlarm> tableItemsForActiveAlarm = new List<ActiveAlarm>();
        private ObservableCollection<ResolvedAlarm> tableItemsForResolvedAlarm = new ObservableCollection<ResolvedAlarm>();
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
        private ICollectionView viewTableItemsForActiveAlarm;
        private ICollectionView viewTableItemsForResolvedAlarm;
        private Dictionary<long, DynamicMeasurement> changesForAmis = new Dictionary<long, DynamicMeasurement>();
        private DateTime timeOfLastUpdate = DateTime.Now;
        private DateTime timeOfLastUpdateAlarm = DateTime.Now;
        private DateTime timeOfLastMeas = DateTime.Now;
        private bool isTest = false;
        private object lockForSmartCache = new object();
        private object lockForNMS = new object();
        private List<IdentifiedObject> geoRegions;
        private List<IdentifiedObject> subGeoRegions;
        private List<IdentifiedObject> substations;

        public INetworkModelGDAContractDuplexClient GdaQueryProxy
        {
            get
            {
                if (FirstContact)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.GdaQueryProxy; line: {0}; Create channel between Client and NMS", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.Transport;
                    binding.ReceiveTimeout = TimeSpan.MaxValue;
                    binding.SendTimeout = TimeSpan.FromHours(1);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                    
                    factory = new DuplexChannelFactory<INetworkModelGDAContractDuplexClient>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://lastamicluster.westus.cloudapp.azure.com:10200/NMSProxy/Client/")
                        /*new EndpointAddress("net.tcp://localhost:10200/NMSProxy/Client/")*/);

                    factory.Credentials.Windows.ClientCredential.UserName = "amiteam";
                    factory.Credentials.Windows.ClientCredential.Password = "dr34mt34m4m1@";
                    factory.Credentials.UserName.UserName = "amiteam";
                    factory.Credentials.UserName.Password = "dr34mt34m4m1@";

                    gdaQueryProxy = factory.CreateChannel();
                    FirstContact = false;
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.GdaQueryProxy; line: {0}; Channel Client-NMS is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                }

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
                    binding.Security.Mode = SecurityMode.Transport;
                    binding.ReceiveTimeout = TimeSpan.MaxValue;
                    binding.SendTimeout = TimeSpan.FromHours(1);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

                    factoryCE = new ChannelFactory<ICalculationForClient>(
                        binding,
                        new EndpointAddress("net.tcp://lastamicluster.westus.cloudapp.azure.com:10100/CEProxy/Client/")
                        /*new EndpointAddress("net.tcp://localhost:10100/CEProxy/Client/")*/);

                    factoryCE.Credentials.Windows.ClientCredential.UserName = "amiteam";
                    factoryCE.Credentials.Windows.ClientCredential.Password = "dr34mt34m4m1@";
                    factoryCE.Credentials.UserName.UserName = "amiteam";
                    factoryCE.Credentials.UserName.Password = "dr34mt34m4m1@";

                    ceQueryProxy = factoryCE.CreateChannel();
                    FirstContactCE = false;
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.CEQueryProxy; line: {0}; Channel Client-CE is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                }

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
                if (FirstContactSC)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.SCProxy; line: {0}; Create channel between Client and SC", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Mode = SecurityMode.Transport;
                    binding.ReceiveTimeout = TimeSpan.MaxValue;
                    binding.SendTimeout = TimeSpan.FromHours(1);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    binding.MaxBufferSize = Int32.MaxValue;
                    binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

                    factorySC = new DuplexChannelFactory<ISmartCacheDuplexForClient>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://lastamicluster.westus.cloudapp.azure.com:10400/SmartCache/Client/")
                        /*new EndpointAddress("net.tcp://localhost:10400/SmartCache/Client/")*/);

                    factorySC.Credentials.Windows.ClientCredential.UserName = "amiteam";
                    factorySC.Credentials.Windows.ClientCredential.Password = "dr34mt34m4m1@";
                    factorySC.Credentials.UserName.UserName = "amiteam";
                    factorySC.Credentials.UserName.Password = "dr34mt34m4m1@";

                    scProxy = factorySC.CreateChannel();
                    FirstContactSC = false;
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.SCProxy; line: {0}; Channel Client-SC is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                }

                return scProxy;
            }

            set
            {
                scProxy = value;
            }
        }

        public List<TableItem> TableItems
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

        public List<ActiveAlarm> TableItemsForActiveAlarm
        {
            get
            {
                return tableItemsForActiveAlarm;
            }

            set
            {
                tableItemsForActiveAlarm = value;
                RaisePropertyChanged("TableItemsForActiveAlarm");
            }
        }

        public ICollectionView ViewTableItemsForActiveAlarm
        {
            get
            {
                return viewTableItemsForActiveAlarm;
            }

            set
            {
                viewTableItemsForActiveAlarm = value;
                RaisePropertyChanged("ViewTableItemsForActiveAlarm");
            }
        }

        public ObservableCollection<ResolvedAlarm> TableItemsForResolvedAlarm
        {
            get
            {
                return tableItemsForResolvedAlarm;
            }

            set
            {
                tableItemsForResolvedAlarm = value;
                RaisePropertyChanged("TableItemsForResolvedAlarm");
            }
        }

        public ICollectionView ViewTableItemsForResolvedAlarm
        {
            get
            {
                return viewTableItemsForResolvedAlarm;
            }

            set
            {
                viewTableItemsForResolvedAlarm = value;
                RaisePropertyChanged("ViewTableItemsForResolvedAlarm");
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

        public bool FirstContactSC
        {
            get
            {
                return firstContactSC;
            }

            set
            {
                firstContactSC = value;
            }
        }

        public bool IsTest
        {
            get
            {
                return isTest;
            }

            set
            {
                isTest = value;
            }
        }

        public Model()
        {
        }

        public void Start()
        {
            geoRegions = new List<IdentifiedObject>();
            subGeoRegions = new List<IdentifiedObject>();
            substations = new List<IdentifiedObject>();

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
                    if (!isTest)
                    {
                        ((IContextChannel)ScProxy).OperationTimeout = TimeSpan.FromHours(1);
                    }

                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToSC; line: {0}; Client is connected to the SC", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch(Exception ex)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToSC; line: {0}; Client faild to connect to SC. CATCH", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    FirstContactSC = true;
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
                    if (!isTest)
                    {
                        ((IContextChannel)CEQueryProxy).OperationTimeout = TimeSpan.FromHours(1);
                    }

                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToCE; line: {0}; Client is connected to the CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch (Exception ex)
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
                    if (!isTest)
                    {
                        ((IContextChannel)GdaQueryProxy).OperationTimeout = TimeSpan.FromMinutes(5);
                    }

                    List<long> grId = new List<long>();
                    List<long> sgrId = new List<long>();


                    lock (lockForNMS)
                    {
                        geoRegions = GetExtentValues(ModelCode.GEOREGION);
                    }

                    foreach (IdentifiedObject io in geoRegions)
                    {
                        grId.Add(io.GlobalId);
                    }

                    subGeoRegions.AddRange(GetSomeSubregions(grId));
                    
                    foreach (IdentifiedObject io in subGeoRegions)
                    {
                        sgrId.Add(io.GlobalId);
                    }

                    substations.AddRange(GetSomeSubstations(sgrId));

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
                    if (App.Current == null)
                    {
                        break;
                    }

                    lock (lockForNMS)
                    {
                        GdaQueryProxy.Ping();
                    }
                }
                catch
                {
                    FirstContact = true;

                    if (IsTest)
                    {
                        break;
                    }

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

        public List<GeographicalRegion> GetAllRegions()
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllRegions; line: {0}; Start the GetAllRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<GeographicalRegion> retVal = new List<GeographicalRegion>();

            lock (lockForNMS)
            {
                List<IdentifiedObject> results = GetExtentValues(ModelCode.GEOREGION);
                
                foreach (GeographicalRegion gr in results)
                {
                    retVal.Add(gr);
                }
            }

            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllRegions; line: {0}; Finish the GetAllRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            return retVal;
        }

        public List<IdentifiedObject> GetAllAmis()
        {
            lock (lockForNMS)
            {
                return GetExtentValues(ModelCode.ENERGYCONS);
            }
        }

        public void GetAllTableItems(bool needGDA)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllTableItems function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<long> gidsInTable = new List<long>();
            ClearTableItems();
            ClearPositions();
            if (geoRegions == null)
            {
                geoRegions = new List<IdentifiedObject>();
            }

            if (needGDA || geoRegions.Count == 0)
            {
                GetTableItemsWithGDA(gidsInTable);
            }
            else
            {
                GetTableItemsWithoutGDA(gidsInTable);
            }

            GetLastMeasurements(gidsInTable);
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Finish the GetAllTableItems function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
        }

        private void GetTableItemsWithoutGDA(List<long> gidsInTable)
        {

            foreach (IdentifiedObject io in geoRegions)
            {
                gidsInTable.Add(io.GlobalId);
                TableItems.Add(new TableItem(io) { Type = HelperClasses.DataGridType.GEOGRAPHICALREGION });
                positions.Add(io.GlobalId, TableItems.Count - 1);
                List<IdentifiedObject> subGeoRegionsLocal = new List<IdentifiedObject>();

                subGeoRegions.ForEach(x =>
                {
                    if (((SubGeographicalRegion)x).GeoRegion == ((GeographicalRegion)io).GlobalId)
                    {
                        subGeoRegionsLocal.Add(x);
                    }
                });

                foreach (IdentifiedObject io1 in subGeoRegionsLocal)
                {
                    gidsInTable.Add(io1.GlobalId);
                    TableItems.Add(new TableItem(io1) { Type = HelperClasses.DataGridType.SUBGEOGRAPHICALREGION });
                    positions.Add(io1.GlobalId, TableItems.Count - 1);
                    List<IdentifiedObject> substationsLocal = new List<IdentifiedObject>();

                    substations.ForEach(x =>
                    {
                        if (((Substation)x).SubGeoRegion == io1.GlobalId)
                        {
                            substationsLocal.Add(x);
                        }
                    });

                    foreach (IdentifiedObject io2 in substationsLocal)
                    {
                        gidsInTable.Add(io2.GlobalId);
                        TableItems.Add(new TableItem(io2) { Type = HelperClasses.DataGridType.SUBSTATION });
                        positions.Add(io2.GlobalId, TableItems.Count - 1);
                    }
                }
            }
        }

        private void GetTableItemsWithGDA(List<long> gidsInTable)
        {
            geoRegions = new List<IdentifiedObject>();
            subGeoRegions = new List<IdentifiedObject>();
            substations = new List<IdentifiedObject>();
            List<IdentifiedObject> subGeoRegionsLocal = new List<IdentifiedObject>();
            List<IdentifiedObject> substationsLocal = new List<IdentifiedObject>();

            lock (lockForNMS)
            {
                geoRegions = GetExtentValues(ModelCode.GEOREGION);
            }

            foreach (IdentifiedObject io in geoRegions)
            {
                gidsInTable.Add(io.GlobalId);
                TableItems.Add(new TableItem(io) { Type = HelperClasses.DataGridType.GEOGRAPHICALREGION });
                positions.Add(io.GlobalId, TableItems.Count - 1);
                subGeoRegionsLocal = GetSomeSubregions(new List<long>() { io.GlobalId });
                subGeoRegions.AddRange(subGeoRegionsLocal);

                foreach (IdentifiedObject io1 in subGeoRegionsLocal)
                {
                    gidsInTable.Add(io1.GlobalId);
                    TableItems.Add(new TableItem(io1) { Type = HelperClasses.DataGridType.SUBGEOGRAPHICALREGION });
                    positions.Add(io1.GlobalId, TableItems.Count - 1);
                    substationsLocal = GetSomeSubstations(new List<long>() { io1.GlobalId });
                    substations.AddRange(substationsLocal);

                    foreach (IdentifiedObject io2 in substationsLocal)
                    {
                        gidsInTable.Add(io2.GlobalId);
                        TableItems.Add(new TableItem(io2) { Type = HelperClasses.DataGridType.SUBSTATION });
                        positions.Add(io2.GlobalId, TableItems.Count - 1);
                    }
                }
            }
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
                int numberOfResources = 500;
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

        public List<IdentifiedObject> GetSomeSubregions(List<long> regionId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Start the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
            associtaion.Type = ModelCode.SUBGEOREGION;
            List<IdentifiedObject> results = new List<IdentifiedObject>();

            lock (lockForNMS)
            {
                results = GetRelatedValues(regionId, properties, associtaion, ModelCode.SUBGEOREGION);
            }

            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Finish the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            return results;
        }

        public List<IdentifiedObject> GetSomeSubstations(List<long> subRegionId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Start the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBSTATION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.SUBGEOREGION_SUBS;
            associtaion.Type = ModelCode.SUBSTATION;
            List<IdentifiedObject> results = new List<IdentifiedObject>();

            lock (lockForNMS)
            {
                results = GetRelatedValues(subRegionId, properties, associtaion, ModelCode.SUBSTATION);
            }
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Finish the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            return results;
        }

        public List<IdentifiedObject> GetSomeAmis(List<long> substationId)
        {
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.EQCONTAINER_EQUIPMENTS;
            associtaion.Type = ModelCode.ENERGYCONS;
            List<IdentifiedObject> results = new List<IdentifiedObject>();

            lock (lockForNMS)
            {
                results = GetRelatedValues(substationId, properties, associtaion, ModelCode.ENERGYCONS);
            }

            return results;
        }

        private List<IdentifiedObject> GetRelatedValues(List<long> source, List<ModelCode> propIds, Association association, ModelCode modelCode)
        {
            List<IdentifiedObject> retVal = new List<IdentifiedObject>();

            try
            {
                int iteratorId = GdaQueryProxy.GetRelatedValues(source, propIds, association);
                int resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

                int numberOfResources = 500;
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
            List<long> gidsInTable = new List<long>();
            List<IdentifiedObject> subGeoRegionsLocal = new List<IdentifiedObject>();
            subGeoRegions.ForEach(x =>
            { if (((SubGeographicalRegion)x).GeoRegion == globalId)
                {
                    subGeoRegionsLocal.Add(x);
                }
            });

            foreach (IdentifiedObject io1 in subGeoRegionsLocal)
            {
                gidsInTable.Add(io1.GlobalId);
                TableItems.Add(new TableItem(io1) { Type = HelperClasses.DataGridType.SUBGEOGRAPHICALREGION });
                positions.Add(io1.GlobalId, TableItems.Count - 1);
                List<IdentifiedObject> substationsLocal = new List<IdentifiedObject>();

                substations.ForEach(x =>
                {
                    if (((Substation)x).SubGeoRegion == io1.GlobalId)
                    {
                        substationsLocal.Add(x);
                    }
                });

                foreach (IdentifiedObject io2 in substationsLocal)
                {
                    gidsInTable.Add(io2.GlobalId);
                    TableItems.Add(new TableItem(io2) { Type = HelperClasses.DataGridType.SUBSTATION });
                    positions.Add(io2.GlobalId, TableItems.Count - 1);
                }
            }

            GetLastMeasurements(gidsInTable);
        }

        public void GetSomeTableItemsForSubGeoRegion(long globalId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            ClearTableItems();
            List<long> gidsInTable = new List<long>();
            ClearPositions();
            List<IdentifiedObject> substationsLocal = new List<IdentifiedObject>();

            substations.ForEach(x =>
            {
                if (((Substation)x).SubGeoRegion == globalId)
                {
                    substationsLocal.Add(x);
                }
            });

            foreach (IdentifiedObject io2 in substationsLocal)
            {
                gidsInTable.Add(io2.GlobalId);
                TableItems.Add(new TableItem(io2) { Type = HelperClasses.DataGridType.SUBSTATION });
                positions.Add(io2.GlobalId, TableItems.Count - 1);
            }

            GetLastMeasurements(gidsInTable);
        }

        #endregion GDAQueryService

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void NewDeltaApplied()
        {
            lock (lockForNMS)
            {
                new Thread(() => ModelUpdate()).Start();
            }
        }

        private void ModelUpdate()
        {
            geoRegions = new List<IdentifiedObject>();
            subGeoRegions = new List<IdentifiedObject>();
            substations = new List<IdentifiedObject>();
            List<IdentifiedObject> subGeoRegionsLocal = new List<IdentifiedObject>();
            List<IdentifiedObject> substationsLocal = new List<IdentifiedObject>();

            lock (lockForNMS)
            {
                geoRegions = GetExtentValues(ModelCode.GEOREGION);
            }

            foreach (IdentifiedObject io in geoRegions)
            {
                subGeoRegionsLocal = GetSomeSubregions(new List<long>() { io.GlobalId });
                subGeoRegions.AddRange(subGeoRegionsLocal);

                foreach (IdentifiedObject io1 in subGeoRegionsLocal)
                {
                    substationsLocal = GetSomeSubstations(new List<long>() { io1.GlobalId });
                    substations.AddRange(substationsLocal);
                }
            }

            lock (this.root.LockObject)
            {
                this.root.NeedsUpdate = true;
            }
        }

        public void SendMeasurements(List<DynamicMeasurement> measurements)
        {
            lock (lockForSmartCache)
            {
                new Thread(() => ReceiveMeasurements(measurements)).Start();
            }
        }

        private void ReceiveMeasurements(List<DynamicMeasurement> measurements)
        {
            UpdateTables(measurements);
            this.timeOfLastUpdate = DateTime.Now;
        }

        private void UpdateTables(List<DynamicMeasurement> measurements)
        {
            lock (lockObj)
            {
                Logger.LogMessageToFile(string.Format("AMIClient.Model.Updatetables; line: {0}; Update started", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

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
                        TableItems[positions[dm.PsrRef]].IsAlarm = dm.IsAlarm;
                    }

                    if ((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(dm.PsrRef) == DMSType.ENERGYCONS)
                    {
                        changesForAmis.Add(dm.PsrRef, dm);
                    }
                }

                Logger.LogMessageToFile(string.Format("AMIClient.Model.Updatetables; line: {0}; Update finished", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                this.timeOfLastMeas = DateTime.Now;
            }
        }

        public void GetLastMeasurements(List<long> gidsInTable)
        {
            lock (lockForSmartCache)
            {
                this.UpdateTables(this.ScProxy.GetLastMeas(gidsInTable));
            }
        }

        public bool NewChangesAvailable(DateTime lastUpdate)
        {
            return DateTime.Compare(lastUpdate, this.timeOfLastUpdate) < 0;
        }

        public bool NewChangesAvailableAlarm(DateTime lastUpdate)
        {
            return DateTime.Compare(lastUpdate, this.timeOfLastUpdateAlarm) < 0;
        }

        public bool NewChangesAvailableMeas(DateTime lastUpdate)
        {
            return DateTime.Compare(lastUpdate, this.timeOfLastMeas) < 0;
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

        public List<ActiveAlarm> GetChangesAlarm()
        {
            return this.TableItemsForActiveAlarm;
        }

        public List<TableItem> GetChangesMeas()
        {
            return this.TableItems;
        }

        public DateTime GetTimeOfTheLastUpdate()
        {
            return this.timeOfLastUpdate;
        }

        public DateTime GetTimeOfTheLastUpdateAlarm()
        {
            return this.timeOfLastUpdateAlarm;
        }

        public DateTime GetTimeOfTheLastUpdateMeas()
        {
            return this.timeOfLastMeas;
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

        public Tuple<List<HourAggregation>, Statistics> GetMeasurementsForChartViewByFilter(List<long> gids, Filter filter)
        {
            return this.CEQueryProxy.GetMeasurementsForChartViewByFilter(gids, filter);
        }

        public void SendAlarm(DeltaForAlarm delta)
        {
            lock (lockForSmartCache)
            {
                Logger.LogMessageToFile(string.Format("AMIClient.Model.UpdateAlarms; line: {0}; Update alarms started", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

                new Thread(() => ReceiveAlarm(delta)).Start();

                Logger.LogMessageToFile(string.Format("AMIClient.Model.UpdateAlarms; line: {0}; Update alarms finished", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                this.timeOfLastUpdateAlarm = DateTime.Now;
            }
        }

        private void ReceiveAlarm(DeltaForAlarm delta)
        {
            foreach (ActiveAlarm alarm in delta.InsertOperations)
            {
                TableItemsForActiveAlarm.Add(new ActiveAlarm()
                {
                    Id = alarm.Id,
                    Consumer = alarm.Consumer,
                    FromPeriod = alarm.FromPeriod,
                    Voltage = alarm.Voltage,
                    TypeVoltage = alarm.TypeVoltage,
                    Georegion = alarm.Georegion
                });

                positionsAlarm.Add(alarm.Id, TableItemsForActiveAlarm.Count - 1);
                if (App.Current != null)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        NotifierClass.Instance.notifier.ShowError(alarm.FromPeriod.ToString() + "\nVoltage type: " + EnumDescription.GetEnumDescription(alarm.TypeVoltage) + "\nConsumer: " +
                                                            alarm.Consumer + "\nVoltage: " + alarm.Voltage.ToString() + "V");
                    });
                }
            }

            foreach (long psrRef in delta.DeleteOperations)
            {
                if (positionsAlarm.ContainsKey(psrRef))
                {
                    int emptyPosition = positionsAlarm[psrRef];
                    List<long> temp = new List<long>();
                    ActiveAlarm alarm = TableItemsForActiveAlarm[emptyPosition];

                    if (App.Current != null)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            NotifierClass.Instance.notifier.ShowInformation("From: " + alarm.FromPeriod.ToString() + "\nConsumer: " + alarm.Consumer + "\nVoltage type: " +
                                                         EnumDescription.GetEnumDescription(alarm.TypeVoltage));
                        });
                    }

                    TableItemsForActiveAlarm.RemoveAt(emptyPosition);
                    positionsAlarm.Remove(psrRef);

                    foreach (long key in positionsAlarm.Keys)
                    {
                        if (positionsAlarm[key] > emptyPosition)
                        {
                            temp.Add(key);
                        }
                    }

                    foreach (long key in temp)
                    {
                        positionsAlarm[key] -= 1;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public bool PingClient()
        {
            if (App.Current == null)
            {
                return false;
            }
            return true;
        }
    }
}
