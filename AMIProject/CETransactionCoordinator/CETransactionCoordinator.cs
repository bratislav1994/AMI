using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using FTN.Common;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using TC57CIM.IEC61970.Core;
using FTN.Services.NetworkModelService.DataModel;
using TC57CIM.IEC61970.Wires;
using CommonMS.Access;
using CommonMS;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using FTN.Services.NetworkModelService.DataModel.Dynamic;

namespace CETransactionCoordinator
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CETransactionCoordinator : StatelessService, ICalculationEngine
    {
        private List<ResourceDescription> meas;
        private Dictionary<long, GeographicalRegionDb> geoRegions;
        private Dictionary<long, SubGeographicalRegionDb> subGeoRegions;
        private Dictionary<long, SubstationDb> substations;
        private Dictionary<long, EnergyConsumerDb> amis;
        private Dictionary<long, BaseVoltageDb> baseVoltages;
        private Dictionary<long, GeographicalRegionDb> geoRegionsTemp;
        private Dictionary<long, SubGeographicalRegionDb> subGeoRegionsTemp;
        private Dictionary<long, SubstationDb> substationsTemp;
        private Dictionary<long, EnergyConsumerDb> amisTemp;
        private Dictionary<long, BaseVoltageDb> baseVoltagesTemp;
        private DB dataBaseAdapter;
        private WcfTransactionCoordinatorCE proxy;
        private WcfCommunicationClientFactory<ITransactionDuplexCE> factory;

        public CETransactionCoordinator(StatelessServiceContext context)
            : base(context)
        {
            dataBaseAdapter = new DB();
            meas = new List<ResourceDescription>();
            geoRegions = new Dictionary<long, GeographicalRegionDb>();
            subGeoRegions = new Dictionary<long, SubGeographicalRegionDb>();
            substations = new Dictionary<long, SubstationDb>();
            amis = new Dictionary<long, EnergyConsumerDb>();
            baseVoltages = new Dictionary<long, BaseVoltageDb>();
            geoRegionsTemp = new Dictionary<long, GeographicalRegionDb>();
            subGeoRegionsTemp = new Dictionary<long, SubGeographicalRegionDb>();
            substationsTemp = new Dictionary<long, SubstationDb>();
            amisTemp = new Dictionary<long, EnergyConsumerDb>();
            baseVoltagesTemp = new Dictionary<long, BaseVoltageDb>();
            geoRegions = dataBaseAdapter.ReadGeoRegions();
            subGeoRegions = dataBaseAdapter.ReadSubGeoRegions();
            substations = dataBaseAdapter.ReadSubstations();
            amis = dataBaseAdapter.ReadConsumers();
            baseVoltages = dataBaseAdapter.ReadBaseVoltages();
        }
        
        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var serviceListener = new ServiceInstanceListener((context) =>
        new WcfCommunicationListener<ICalculationEngine>(
            wcfServiceObject: this,
            serviceContext: context,
            //
            // The name of the endpoint configured in the ServiceManifest under the Endpoints section
            // that identifies the endpoint that the WCF ServiceHost should listen on.
            //
            endpointResourceName: "ServiceEndpoint",

            //
            // Populate the binding information that you want the service to use.
            //
            listenerBinding: WcfUtility.CreateTcpListenerBinding()
        ),
        "CETransactionCoordinatorListener"
    );

            return new ServiceInstanceListener[] { serviceListener };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            factory = new WcfCommunicationClientFactory<ITransactionDuplexCE>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy = new WcfTransactionCoordinatorCE(
                            factory,
                            new Uri("fabric:/TransactionCoordinatorMS/TransactionCoordinator"),
                            new ServicePartitionKey(1),
                            "CEListener");

            proxy.InvokeWithRetry(client => client.Channel.ConnectCE(new ServiceInfo(base.Context.PartitionId.ToString() + "-" + base.Context.ReplicaOrInstanceId, base.Context.ServiceName.ToString(), FTN.Common.ServiceType.STATELESS)));
        }

        public void Commit()
        {
            dataBaseAdapter.AddAddBaseVoltages(baseVoltagesTemp.Values.ToList());
            dataBaseAdapter.AddGeoRegions(geoRegionsTemp.Values.ToList());
            dataBaseAdapter.AddSubGeoRegions(subGeoRegionsTemp.Values.ToList());
            dataBaseAdapter.AddSubstations(substationsTemp.Values.ToList());
            dataBaseAdapter.AddConsumers(amisTemp.Values.ToList());


            foreach (KeyValuePair<long, BaseVoltageDb> kvp in baseVoltagesTemp)
            {
                baseVoltages.Add(kvp.Key, kvp.Value);
            }

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

            this.baseVoltagesTemp.Clear();
            this.amisTemp.Clear();
            this.substationsTemp.Clear();
            this.subGeoRegionsTemp.Clear();
            this.geoRegionsTemp.Clear();
            this.meas.Clear();
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
                    case DMSType.BASEVOLTAGE:
                        BaseVoltage bv = new BaseVoltage();
                        bv.RD2Class(rd);
                        bv.GlobalId = rd.Id;
                        BaseVoltageDb bvCE = new BaseVoltageDb(bv);
                        baseVoltagesTemp.Add(rd.Id, bvCE);
                        break;
                }
            }

            return true;
        }

        public void Rollback()
        {
            this.baseVoltagesTemp.Clear();
            this.amisTemp.Clear();
            this.substationsTemp.Clear();
            this.subGeoRegionsTemp.Clear();
            this.geoRegionsTemp.Clear();
            this.meas.Clear();
        }
    }
}
