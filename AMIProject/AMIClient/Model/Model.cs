using FTN.Common;
using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class Model : IDisposable, IModel
    {
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private ObservableCollection<GeographicalRegion> geoRegions = new ObservableCollection<GeographicalRegion>();
        private ObservableCollection<SubGeographicalRegion> subGeoRegions = new ObservableCollection<SubGeographicalRegion>();
        private ObservableCollection<Substation> substations = new ObservableCollection<Substation>();
        private ObservableCollection<EnergyConsumer> amis = new ObservableCollection<EnergyConsumer>();

        private NetworkModelGDAProxy gdaQueryProxy = null;

        private static Model instance;

        public static Model Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Model();
                }
                return instance;
            }
        }

        private NetworkModelGDAProxy GdaQueryProxy
        {
            get
            {
                if (gdaQueryProxy != null)
                {
                    gdaQueryProxy.Abort();
                    gdaQueryProxy = null;
                }

                gdaQueryProxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
                gdaQueryProxy.Open();

                return gdaQueryProxy;
            }
        }

        public Model()
        {

        }

        #region GDAQueryService
        
        public ObservableCollection<GeographicalRegion> GetAllRegions()
        {
            geoRegions.Clear();
            GetExtentValues(ModelCode.GEOREGION);
            return geoRegions;
        }

        public ObservableCollection<SubGeographicalRegion> GetAllSubRegions()
        {
            subGeoRegions.Clear();
            GetExtentValues(ModelCode.SUBGEOREGION);
            return subGeoRegions;
        }

        public ObservableCollection<Substation> GetAllSubstations()
        {
            substations.Clear();
            GetExtentValues(ModelCode.SUBSTATION);
            return substations;
        }

        public ObservableCollection<EnergyConsumer> GetAllAmis()
        {
            amis.Clear();
            GetExtentValues(ModelCode.ENERGYCONS);
            return amis;
        }

        private void GetExtentValues(ModelCode modelCode)
        {
            substations.Clear();
            amis.Clear();
            string message = "Getting extent values method started.";
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);

            XmlTextWriter xmlWriter = null;
            int iteratorId = 0;
            List<long> ids = new List<long>();
            

            try
            {
                int numberOfResources = 2;
                int resourcesLeft = 0;

                List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(modelCode);

                iteratorId = GdaQueryProxy.GetExtentValues(modelCode, properties);
                resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);


                xmlWriter = new XmlTextWriter(Config.Instance.ResultDirecotry + "\\GetExtentValues_Results.xml", Encoding.Unicode);
                xmlWriter.Formatting = Formatting.Indented;
                
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
                                geoRegions.Add(gr);
                                break;
                            case ModelCode.SUBGEOREGION:
                                SubGeographicalRegion sgr = new SubGeographicalRegion();
                                sgr.RD2Class(rds[i]);
                                subGeoRegions.Add(sgr);
                                break;
                            case ModelCode.SUBSTATION:
                                Substation ss = new Substation();
                                ss.RD2Class(rds[i]);
                                substations.Add(ss);
                                break;
                            case ModelCode.ENERGYCONS:
                                EnergyConsumer ec = new EnergyConsumer();
                                ec.RD2Class(rds[i]);
                                amis.Add(ec);
                                break;

                            default:
                                break;
                        }
                        
                        ids.Add(rds[i].Id);               
                        rds[i].ExportToXml(xmlWriter);
                        xmlWriter.Flush();
                    }

                    resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
                }

                GdaQueryProxy.IteratorClose(iteratorId);

                message = "Getting extent values method successfully finished.";
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);

            }
            catch (Exception e)
            {
                message = string.Format("Getting extent values method failed for {0}.\n\t{1}", modelCode, e.Message);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }
            finally
            {
                if (xmlWriter != null)
                {
                    xmlWriter.Close();
                }
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

        public ObservableCollection<SubGeographicalRegion> GetSomeSubregions(long regionId)
        {
            subGeoRegions.Clear();

            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
            associtaion.Type = ModelCode.SUBGEOREGION;
            GetRelatedValues(regionId, properties, associtaion, ModelCode.SUBGEOREGION);

            return subGeoRegions;
        }

        public ObservableCollection<Substation> GetSomeSubstations(long subRegionId)
        {
            substations.Clear();
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBSTATION);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.SUBGEOREGION_SUBS;
            associtaion.Type = ModelCode.SUBSTATION;
            GetRelatedValues(subRegionId, properties, associtaion, ModelCode.SUBSTATION);

            return substations;
        }

        public ObservableCollection<EnergyConsumer> GetSomeAmis(long substationId)
        {
            amis.Clear();
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.EQCONTAINER_EQUIPMENTS;
            associtaion.Type = ModelCode.ENERGYCONS;
            GetRelatedValues(substationId, properties, associtaion, ModelCode.ENERGYCONS);

            return amis;
        }

        private void GetRelatedValues(long source, List<ModelCode> propIds, Association association, ModelCode modelCode)
        {
            int iteratorId = GdaQueryProxy.GetRelatedValues(source, propIds, association);
            int resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);

            int numberOfResources = 10;
            List<ResourceDescription> results = new List<ResourceDescription>();

            while(resourcesLeft > 0)
            {
                List<ResourceDescription> rds = GdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                results.AddRange(rds);
                resourcesLeft = GdaQueryProxy.IteratorResourcesLeft(iteratorId);
            }

            switch(modelCode)
            {
                case ModelCode.SUBGEOREGION:
                    foreach(ResourceDescription rd in results)
                    {
                        SubGeographicalRegion sgr = new SubGeographicalRegion();
                        sgr.RD2Class(rd);
                        subGeoRegions.Add(sgr);
                    }
                    break;
                case ModelCode.SUBSTATION:
                    foreach(ResourceDescription rd in results)
                    {
                        Substation ss = new Substation();
                        ss.RD2Class(rd);
                        substations.Add(ss);
                    }
                    break;
                case ModelCode.ENERGYCONS:
                    foreach (ResourceDescription rd in results)
                    {
                        EnergyConsumer ec = new EnergyConsumer();
                        ec.RD2Class(rd);
                        amis.Add(ec);
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion GDAQueryService

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
