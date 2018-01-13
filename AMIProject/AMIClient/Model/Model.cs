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

namespace AMIClient
{
    public class Model : IDisposable, IModelForDuplex, INotifyPropertyChanged
    {
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private List<ResourceDescription> meas = new List<ResourceDescription>();
        private ObservableCollection<GeographicalRegion> geoRegions = new ObservableCollection<GeographicalRegion>();
        private ObservableCollection<SubGeographicalRegion> subGeoRegions = new ObservableCollection<SubGeographicalRegion>();
        private ObservableCollection<Substation> substations = new ObservableCollection<Substation>();
        private Dictionary<long, int> positions = new Dictionary<long, int>();
        private ObservableCollection<EnergyConsumerForTable> amis = new ObservableCollection<EnergyConsumerForTable>();
        private RootElement root;
        private bool firstContact = true;
        private bool firstContactCE = true;
        private INetworkModelGDAContractDuplexClient gdaQueryProxy = null;
        private ICalculationDuplexClient ceQueryProxy = null;
        private object lockObj = new object();
        private DuplexChannelFactory<INetworkModelGDAContractDuplexClient> factory;
        private DuplexChannelFactory<ICalculationDuplexClient> factoryCE;
        private Thread checkNMS;

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

        public ICalculationDuplexClient CEQueryProxy
        {
            get
            {
                if (FirstContactCE)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.CEQueryProxy; line: {0}; Create channel between Client and CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    factoryCE = new DuplexChannelFactory<ICalculationDuplexClient>(
                    new InstanceContext(this),
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

        public ObservableCollection<GeographicalRegion> GeoRegions
        {
            get
            {
                return geoRegions;
            }

            set
            {
                geoRegions = value;
            }
        }

        public ObservableCollection<SubGeographicalRegion> SubGeoRegions
        {
            get
            {
                return subGeoRegions;
            }

            set
            {
                subGeoRegions = value;
            }
        }

        public ObservableCollection<Substation> Substations
        {
            get
            {
                return substations;
            }

            set
            {
                substations = value;
            }
        }

        public ObservableCollection<EnergyConsumerForTable> Amis
        {
            get
            {
                return amis;
            }

            set
            {
                amis = value;
                RaisePropertyChanged("Amis");
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
            
        }

        public void Start()
        {
            checkNMS = new Thread(() => CheckIfNMSIsAlive());

            Thread t = new Thread(() => ConnectToNMS());
            t.Start();


            Thread t2 = new Thread(() => ConnectToCE());
            t2.Start();
            t.Join();
            t2.Join();
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
                    ((IContextChannel)CEQueryProxy).OperationTimeout = TimeSpan.FromMinutes(1);
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
                    ((IContextChannel)GdaQueryProxy).OperationTimeout = TimeSpan.FromMinutes(1);
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
            if (!returnValue)
            {
                GeoRegions.Clear();
                foreach (GeographicalRegion gr in results)
                {
                    GeoRegions.Add(gr);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllRegions; line: {0}; Start the GetAllRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return null;
            }
            else
            {
                List<GeographicalRegion> retVal = new List<GeographicalRegion>();
                foreach (GeographicalRegion gr in results)
                {
                    retVal.Add(gr);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllRegions; line: {0}; Start the GetAllRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return retVal;
            }
        }

        public List<SubGeographicalRegion> GetAllSubRegions(bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubRegions; line: {0}; Start the GetAllSubRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<IdentifiedObject> results = GetExtentValues(ModelCode.SUBGEOREGION);

            if (!returnValue)
            {
                SubGeoRegions.Clear();
                foreach (SubGeographicalRegion sgr in results)
                {
                    SubGeoRegions.Add(sgr);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubRegions; line: {0}; Start the GetAllSubRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return null;
            }
            else
            {
                List<SubGeographicalRegion> retVal = new List<SubGeographicalRegion>();
                foreach (SubGeographicalRegion sgr in results)
                {
                    retVal.Add(sgr);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubRegions; line: {0}; Start the GetAllSubRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return retVal;
            }
        }

        public List<Substation> GetAllSubstations(bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubstations; line: {0}; Start the GetAllSubstations function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            Substations.Clear();
            List<IdentifiedObject> results = GetExtentValues(ModelCode.SUBSTATION);
            if (!returnValue)
            {
                Substations.Clear();
                foreach (Substation ss in results)
                {
                    Substations.Add(ss);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubstations; line: {0}; Start the GetAllSubstations function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return null;
            }
            else
            {
                List<Substation> retVal = new List<Substation>();
                foreach (Substation ss in results)
                {
                    retVal.Add(ss);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubstations; line: {0}; Start the GetAllSubstations function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return retVal;
            }
        }

        public List<EnergyConsumerForTable> GetAllAmis(bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            this.ClearPositions();
            List<IdentifiedObject> results = GetExtentValues(ModelCode.ENERGYCONS);

            if (!returnValue)
            {
                ClearAmis();
                ClearPositions();

                foreach (EnergyConsumer ec in results)
                {
                    Amis.Add(new EnergyConsumerForTable(ec));
                    positions.Add(ec.GlobalId, Amis.Count - 1);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return null;
            }
            else
            {
                List<EnergyConsumerForTable> retVal = new List<EnergyConsumerForTable>();
                foreach (EnergyConsumer ec in results)
                {
                    retVal.Add(new EnergyConsumerForTable(ec));
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return retVal;
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
                int numberOfResources = 2;
                int resourcesLeft = 0;

                List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(modelCode);

                iteratorId = GdaQueryProxy.GetExtentValues(modelCode, properties);
                resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
                
                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                    //ret.AddRange(rds);
                    

                    for (int i = 0; i < rds.Count; i++)
                    {
                        switch(modelCode)
                        {
                            case ModelCode.GEOREGION:
                                GeographicalRegion gr = new GeographicalRegion();
                                gr.RD2Class(rds[i]);
                                //GeoRegions.Add(gr);
                                retVal.Add(gr);
                                break;
                            case ModelCode.SUBGEOREGION:
                                SubGeographicalRegion sgr = new SubGeographicalRegion();
                                sgr.RD2Class(rds[i]);
                                //SubGeoRegions.Add(sgr);
                                retVal.Add(sgr);
                                break;
                            case ModelCode.SUBSTATION:
                                Substation ss = new Substation();
                                ss.RD2Class(rds[i]);
                                //Substations.Add(ss);
                                retVal.Add(ss);
                                break;
                            case ModelCode.ENERGYCONS:
                                EnergyConsumer ec = new EnergyConsumer();
                                ec.RD2Class(rds[i]);
                                retVal.Add(ec);
                                //Amis.Add(new EnergyConsumerForTable(ec));
                                //positions.Add(ec.GlobalId, Amis.Count - 1);
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

        public ModelCode GetModelCodeFromDmsType(DMSType type)
        {
            switch (type)
            {
                case DMSType.GEOREGION:
                    return ModelCode.GEOREGION;
                case DMSType.SUBGEOREGION:
                    return ModelCode.SUBGEOREGION;
                case DMSType.BASEVOLTAGE:
                    return ModelCode.BASEVOLTAGE;
                case DMSType.SUBSTATION:
                    return ModelCode.SUBSTATION;
                case DMSType.VOLTAGELEVEL:
                    return ModelCode.VOLTAGELEVEL;
                case DMSType.ENERGYCONS:
                    return ModelCode.ENERGYCONS;
                case DMSType.POWERTRANSFORMER:
                    return ModelCode.POWERTRANSFORMER;
                case DMSType.POWERTRANSEND:
                    return ModelCode.POWERTRANSEND;
                case DMSType.RATIOTAPCHANGER:
                    return ModelCode.RATIOTAPCHANGER;
                case DMSType.ANALOG:
                    return ModelCode.ANALOG;
                case DMSType.DISCRETE:
                    return ModelCode.DISCRETE;

                default:
                    return 0;
            }
        }

        public List<SubGeographicalRegion> GetSomeSubregions(long regionId, bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Start the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
            associtaion.Type = ModelCode.SUBGEOREGION;
            List<IdentifiedObject> results = GetRelatedValues(regionId, properties, associtaion, ModelCode.SUBGEOREGION);

            if (!returnValue)
            {
                foreach (SubGeographicalRegion sgr in results)
                {
                    SubGeoRegions.Add(sgr);
                }

                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Finish the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return null;
            }
            else
            {
                List<SubGeographicalRegion> retVal = new List<SubGeographicalRegion>();

                foreach (SubGeographicalRegion sgr in results)
                {
                    retVal.Add(sgr);
                }

                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Finish the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return retVal;
            }
        }

        public List<Substation> GetSomeSubstations(long subRegionId, bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Start the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBSTATION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.SUBGEOREGION_SUBS;
            associtaion.Type = ModelCode.SUBSTATION;
            GetRelatedValues(subRegionId, properties, associtaion, ModelCode.SUBSTATION);
            List<IdentifiedObject> results = GetRelatedValues(subRegionId, properties, associtaion, ModelCode.SUBSTATION);

            if (!returnValue)
            {
                foreach (Substation ss in results)
                {
                    Substations.Add(ss);
                }

                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Start the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return null;
            }
            else
            {
                List<Substation> retVal = new List<Substation>();
                foreach (Substation ss in results)
                {
                    retVal.Add(ss);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Start the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return retVal;
            }
        }

        public List<EnergyConsumerForTable> GetSomeAmis(long substationId, bool returnValue)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeAmis; line: {0}; Start the GetSomeAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.EQCONTAINER_EQUIPMENTS;
            associtaion.Type = ModelCode.ENERGYCONS;
            List<IdentifiedObject> results = GetRelatedValues(substationId, properties, associtaion, ModelCode.ENERGYCONS);

            if (!returnValue)
            {
                foreach (EnergyConsumer ec in results)
                {
                    Amis.Add(new EnergyConsumerForTable(ec));
                    positions.Add(ec.GlobalId, Amis.Count - 1);
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeAmis; line: {0}; Finish the GetSomeAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return null;
            }
            else
            {
                List<EnergyConsumerForTable> retVal = new List<EnergyConsumerForTable>();
                foreach(EnergyConsumer ec in results)
                {
                    retVal.Add(new EnergyConsumerForTable(ec));
                }
                Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeAmis; line: {0}; Finish the GetSomeAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return retVal;
            }
        }

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
                            //SubGeoRegions.Add(sgr);
                            retVal.Add(sgr);
                        }
                        break;
                    case ModelCode.SUBSTATION:
                        foreach (ResourceDescription rd in results)
                        {
                            Substation ss = new Substation();
                            ss.RD2Class(rd);
                            //Substations.Add(ss);
                            retVal.Add(ss);
                        }
                        break;
                    case ModelCode.ENERGYCONS:
                        foreach (ResourceDescription rd in results)
                        {
                            EnergyConsumer ec = new EnergyConsumer();
                            ec.RD2Class(rd);
                            retVal.Add(ec);
                            //Amis.Add(new EnergyConsumerForTable(ec));
                            //positions.Add(ec.GlobalId, Amis.Count - 1);
                        }
                        break;

                    default:
                        break;
                }
               
            }
            catch(Exception e)
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
            lock (lockObj)
            {
                if (Amis.Count == 0)
                {
                    positions.Clear();
                }

                foreach (DynamicMeasurement dm in measurements)
                {
                    if (positions.ContainsKey(dm.PsrRef))
                    {
                        Amis[positions[dm.PsrRef]].CurrentP = dm.CurrentP != -1 ? dm.CurrentP : Amis[positions[dm.PsrRef]].CurrentP;
                        Amis[positions[dm.PsrRef]].CurrentQ = dm.CurrentQ != -1 ? dm.CurrentQ : Amis[positions[dm.PsrRef]].CurrentQ;
                        Amis[positions[dm.PsrRef]].CurrentV = dm.CurrentV != -1 ? dm.CurrentV : Amis[positions[dm.PsrRef]].CurrentV;
                    }
                }
            }
        }

        public void ClearAmis()
        {
            lock (lockObj)
            {
                this.Amis.Clear();
            }
        }

        public void ClearPositions()
        {
            lock (lockObj)
            {
                this.positions.Clear();
            }
        }

        public Tuple<List<DynamicMeasurement>, Statistics> GetMeasForChart(List<long> gids, DateTime from, DateTime to)
        {
            return this.CEQueryProxy.GetMeasurementsForChartView(gids, from, to);
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
