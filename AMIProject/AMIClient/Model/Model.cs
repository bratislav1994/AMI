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
        DuplexChannelFactory<INetworkModelGDAContractDuplexClient> factory;
        DuplexChannelFactory<ICalculationDuplexClient> factoryCE;

        public INetworkModelGDAContractDuplexClient GdaQueryProxy
        {
            get
            {
                if (firstContact)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.GdaQueryProxy; line: {0}; Create channel between Client and NMS", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    factory = new DuplexChannelFactory<INetworkModelGDAContractDuplexClient>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10000/NetworkModelService/GDADuplexClient"));
                    gdaQueryProxy = factory.CreateChannel();
                    firstContact = false;
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
                if (firstContactCE)
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.CEQueryProxy; line: {0}; Create channel between Client and CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    factoryCE = new DuplexChannelFactory<ICalculationDuplexClient>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10006/CalculationEngine/Client"));
                    ceQueryProxy = factoryCE.CreateChannel();
                    firstContactCE = false;
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

        public Model()
        {
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
                    CEQueryProxy.ConncetClient();
                    factoryCE.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(1);
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToCE; line: {0}; Client is connected to the CE", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToCE; line: {0}; Client faild to connect with CE. CATCH", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    firstContactCE = true;
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
                    factory.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(1);
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToNMS; line: {0}; Client is connected to the NMS", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    Logger.LogMessageToFile(string.Format("AMIClient.Model.ConnectToNMS; line: {0}; Client faild to connect with NMS. CATCH", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    firstContact = true;
                    Thread.Sleep(1000);
                }
            }
        }

        public void SetRoot(RootElement root)
        {
            this.root = root;
        }

        #region GDAQueryService
        
        public void GetAllRegions()
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllRegions; line: {0}; Start the GetAllRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            GeoRegions.Clear();
            GetExtentValues(ModelCode.GEOREGION);
            //return GeoRegions;
        }

        public void GetAllSubRegions()
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubRegions; line: {0}; Start the GetAllSubRegions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            SubGeoRegions.Clear();
            GetExtentValues(ModelCode.SUBGEOREGION);
            //return SubGeoRegions;
        }

        public void GetAllSubstations()
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllSubstations; line: {0}; Start the GetAllSubstations function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            Substations.Clear();
            GetExtentValues(ModelCode.SUBSTATION);
            //return Substations;
        }

        public void GetAllAmis()
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetAllAmis; line: {0}; Start the GetAllAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            Amis.Clear();
            GetExtentValues(ModelCode.ENERGYCONS);
            //return Amis;
        }

        private void GetExtentValues(ModelCode modelCode)
        {
            Substations.Clear();
            Amis.Clear();
            string message = "Getting extent values method started.";
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            int iteratorId = 0;
            List<long> ids = new List<long>();
            

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
                                GeoRegions.Add(gr);
                                break;
                            case ModelCode.SUBGEOREGION:
                                SubGeographicalRegion sgr = new SubGeographicalRegion();
                                sgr.RD2Class(rds[i]);
                                SubGeoRegions.Add(sgr);
                                break;
                            case ModelCode.SUBSTATION:
                                Substation ss = new Substation();
                                ss.RD2Class(rds[i]);
                                Substations.Add(ss);
                                break;
                            case ModelCode.ENERGYCONS:
                                EnergyConsumer ec = new EnergyConsumer();
                                ec.RD2Class(rds[i]);
                                Amis.Add(new EnergyConsumerForTable(ec));
                                positions.Add(ec.GlobalId, Amis.Count - 1);
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
            }
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

        public void GetSomeSubregions(long regionId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Start the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
            associtaion.Type = ModelCode.SUBGEOREGION;
            GetRelatedValues(regionId, properties, associtaion, ModelCode.SUBGEOREGION);

            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubregions; line: {0}; Finish the GetSomeSubregions function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            //return SubGeoRegions;
        }

        public void GetSomeSubstations(long subRegionId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Start the GetSomeSubstation function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBSTATION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.SUBGEOREGION_SUBS;
            associtaion.Type = ModelCode.SUBSTATION;
            GetRelatedValues(subRegionId, properties, associtaion, ModelCode.SUBSTATION);

            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeSubstation; line: {0}; Finish the GetSomeSubstations function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            //return Substations;
        }

        public void GetSomeAmis(long substationId)
        {
            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeAmis; line: {0}; Start the GetSomeAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));

            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.EQCONTAINER_EQUIPMENTS;
            associtaion.Type = ModelCode.ENERGYCONS;
            GetRelatedValues(substationId, properties, associtaion, ModelCode.ENERGYCONS);

            Logger.LogMessageToFile(string.Format("AMIClient.Model.GetSomeAmis; line: {0}; Finish the GetSomeAmis function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            //return Amis;
        }

        private void GetRelatedValues(long source, List<ModelCode> propIds, Association association, ModelCode modelCode)
        {
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
                            SubGeoRegions.Add(sgr);
                        }
                        break;
                    case ModelCode.SUBSTATION:
                        foreach (ResourceDescription rd in results)
                        {
                            Substation ss = new Substation();
                            ss.RD2Class(rd);
                            Substations.Add(ss);
                        }
                        break;
                    case ModelCode.ENERGYCONS:
                        foreach (ResourceDescription rd in results)
                        {
                            EnergyConsumer ec = new EnergyConsumer();
                            ec.RD2Class(rd);
                            Amis.Add(new EnergyConsumerForTable(ec));
                            positions.Add(ec.GlobalId, Amis.Count - 1);
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
            lock (lockObj)
            {
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

        public void ClearPositions()
        {
            lock (lockObj)
            {
                this.positions.Clear();
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
    }
}
