using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using FTN.ServiceContracts;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using FTN.Services.NetworkModelService.DataModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using CommonMS;
using FTN.Common.ClassesForAlarmDB;
using CommonMS.Access;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Dynamic;

namespace CEScada
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CEScada : StatefulService, ICEScada, ICalculationEngineDuplexSmartCache
    {
        IReliableDictionary<string, ServiceInfo> proxies;
        IReliableDictionary<string, ServiceInfo> caches;
        private WcfCEProxy proxy;
        private WcfSmartCacheCE smartCache;
        private WcfCommunicationClientFactory<IScadaForCECommand> factory;
        private Dictionary<long, GeographicalRegionDb> geoRegions;
        private Dictionary<long, SubGeographicalRegionDb> subGeoRegions;
        private Dictionary<long, SubstationDb> substations;
        private Dictionary<long, EnergyConsumerDb> amis;
        private Dictionary<long, BaseVoltageDb> baseVoltages;
        private Dictionary<long, ActiveAlarm> alarmActiveDB;

        public CEScada(StatefulServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            Binding listenerBindingScada = WcfUtility.CreateTcpClientBinding();
            listenerBindingScada.ReceiveTimeout = TimeSpan.MaxValue;

            var scadaListener = new ServiceReplicaListener((context) =>
        new WcfCommunicationListener<ICEScada>(
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
            listenerBinding: listenerBindingScada
        ),
        "ScadaListener"
    );
            Binding listenerBindingSC = WcfUtility.CreateTcpClientBinding();
            listenerBindingSC.ReceiveTimeout = TimeSpan.MaxValue;

            var SCListener = new ServiceReplicaListener((context) =>
        new WcfCommunicationListener<ICalculationEngineDuplexSmartCache>(
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
            listenerBinding: listenerBindingSC
        ),
        "SmartCacheListener"
    );

            return new ServiceReplicaListener[] { scadaListener, SCListener };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            proxies = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceInfo>>("ProxiesForCEScada");
            caches = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ServiceInfo>>("SmartCaches");
            geoRegions = new Dictionary<long, GeographicalRegionDb>();
            subGeoRegions = new Dictionary<long, SubGeographicalRegionDb>();
            substations = new Dictionary<long, SubstationDb>();
            amis = new Dictionary<long, EnergyConsumerDb>();
            alarmActiveDB = new Dictionary<long, ActiveAlarm>();
            geoRegions = DB.Instance.ReadGeoRegions();
            subGeoRegions = DB.Instance.ReadSubGeoRegions();
            substations = DB.Instance.ReadSubstations();
            amis = DB.Instance.ReadConsumers();
            baseVoltages = DB.Instance.ReadBaseVoltages();
            alarmActiveDB = DB.Instance.ReadActiveAlarm();
        }
        public async void Connect(ServiceInfo serviceInfo)
        {
            while (proxies == null)
            {
                Thread.Sleep(1000);
            }

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await proxies.TryGetValueAsync(tx, serviceInfo.TraceID);

                if (!result.HasValue)
                {
                    await proxies.AddAsync(tx, serviceInfo.TraceID, serviceInfo);
                }

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }

            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            factory = new WcfCommunicationClientFactory<IScadaForCECommand>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: serviceInfo.TraceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
            proxy = new WcfCEProxy(
                            factory,
                            new Uri(serviceInfo.ServiceName),
                            ServicePartitionKey.Singleton,
                            "CEScadaListener");
        }

        public async void DataFromScada(Dictionary<long, DynamicMeasurement> measurements)
        {
            if(DB.Instance.GeoregionsNeedToRefresh(geoRegions.Count))
            {
                geoRegions.Clear();
                geoRegions = DB.Instance.ReadGeoRegions();
            }

            if (DB.Instance.SubgeoregionsNeedToRefresh(subGeoRegions.Count))
            {
                subGeoRegions.Clear();
                subGeoRegions = DB.Instance.ReadSubGeoRegions();
            }

            if (DB.Instance.SubstationsNeedToRefresh(substations.Count))
            {
                substations.Clear();
                substations = DB.Instance.ReadSubstations();
            }

            if (DB.Instance.ConsumersNeedToRefresh(amis.Count))
            {
                amis.Clear();
                amis = DB.Instance.ReadConsumers();
            }

            if (DB.Instance.BaseVoltagesNeedToRefresh(baseVoltages.Count))
            {
                baseVoltages.Clear();
                baseVoltages = DB.Instance.ReadBaseVoltages();
            }

            int cntForVoltage = 0;
            Dictionary<long, DynamicMeasurement> addSubstations = new Dictionary<long, DynamicMeasurement>();

            AddMeasurements(measurements.Values.ToList());
            DeltaForAlarm delta = CheckAlarms(measurements.Values.ToList());

            foreach (KeyValuePair<long, DynamicMeasurement> kvp in measurements)
            {
                kvp.Value.CurrentV = kvp.Value.CurrentV / baseVoltages[amis[kvp.Key].BaseVoltageID].NominalVoltage;
            }

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

            new Thread(() => SendToClient(measurements, delta)).Start();
        }

        private void SendToClient(Dictionary<long, DynamicMeasurement> measurements, DeltaForAlarm delta)
        {
            try
            {
                if (smartCache != null)
                {
                    smartCache.InvokeWithRetry(client => client.Channel.SendMeasurements(measurements));
                    smartCache.InvokeWithRetry(client => client.Channel.SendAlarm(delta));
                }
            }
            catch
            {
                smartCache = null;
            }
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
                        alarmR.Georegion = alarmA.Georegion;
                        alarmR.ToPeriod = dm.TimeStamp;
                        alarmR.TypeVoltage = alarmA.TypeVoltage;

                        DB.Instance.AddResolvedAlarm(alarmR);
                        DB.Instance.DeleteActiveAlarm(alarmA);
                        alarmActiveDB.Remove(dm.PsrRef);
                        delta.DeleteOperations.Add(alarmA.Id);
                    }
                }
                else
                {
                    if (dm.CurrentV != 0)
                    {
                        gidsInAlarm.Add(dm.PsrRef, dm);
                    }

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
                        a.Georegion = DB.Instance.ReadGeoregionNameByPSRRef(dm.PsrRef);
                        DB.Instance.AddActiveAlarm(a);
                        delta.InsertOperations.Add(a);
                    }
                }
            }

            new Thread(() => SendCommand(gidsInAlarm)).Start();

            return delta;
        }

        private void SendCommand(Dictionary<long, DynamicMeasurement> gidsInAlarm)
        {
            if (gidsInAlarm.Count > 0)
            {
                try
                {
                    if (proxy != null)
                    {
                        Console.WriteLine(proxy.InvokeWithRetry(client => client.Channel.Command(gidsInAlarm)));
                    }
                }
                catch
                {
                    proxy = null;
                }
            }
        }

        private bool AddMeasurements(List<DynamicMeasurement> measurements)
        {
            using (var access = new AccessTSDB())
            {
                foreach (DynamicMeasurement m in measurements)
                {
                    access.Collect.Add(m);
                }

                int i = access.SaveChanges();

                if (i > 0)
                {
                }
                else
                {
                    return false;
                }

                return true;
            }
        }

        public async void Subscribe(ServiceInfo serviceInfo)
        {
            while (caches == null)
            {
                Thread.Sleep(1000);
            }

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await caches.TryGetValueAsync(tx, serviceInfo.TraceID);

                if (!result.HasValue)
                {
                    await caches.AddAsync(tx, serviceInfo.TraceID, serviceInfo);
                }

                // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                // discarded, and nothing is saved to the secondary replicas.
                await tx.CommitAsync();
            }

            // Create binding
            Binding binding = WcfUtility.CreateTcpClientBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            // Create a partition resolver
            IServicePartitionResolver partitionResolver = ServicePartitionResolver.GetDefault();
            // create a  WcfCommunicationClientFactory object.
            var factory = new WcfCommunicationClientFactory<ISmartCacheForCE>
                (clientBinding: binding, servicePartitionResolver: partitionResolver, traceId: serviceInfo.TraceID);

            //
            // Create a client for communicating with the ICalculator service that has been created with the
            // Singleton partition scheme.
            //
           smartCache = new WcfSmartCacheCE(
                            factory,
                            new Uri(serviceInfo.ServiceName),
                            new ServicePartitionKey(1),
                            "CEListener");
        }
    }
}
