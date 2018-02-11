using CalculationEngine.Access;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Meas;
using TC57CIM.IEC61970.Wires;
using DailyConsumptions;

namespace CalculationEngine
{
    public class Filler
    {
        private const int resolution = 15;
        private DB dbAdapter;
        private TSDB timeSeriesDbAdapter;
        public List<DynamicMeasurement> measurementsList = new List<DynamicMeasurement>();

        public Filler()
        {
            
        }

        public DB DbAdapter
        {
            get
            {
                return this.dbAdapter;
            }

            set
            {
                this.dbAdapter = value;
            }
        }

        public TSDB TimeSeriesDbAdapter
        {
            get
            {
                return this.timeSeriesDbAdapter;
            }

            set
            {
                this.timeSeriesDbAdapter = value;
            }
        }

        public void Fill()
        {
            DateTime beginning = new DateTime(2017, 9, 14);
            beginning = beginning.AddSeconds(1);
            DateTime end = new DateTime(2017, 9, 28);

            NetTcpBinding binding = new NetTcpBinding();
            binding.SendTimeout = TimeSpan.FromDays(1);
            ChannelFactory<INMSForScript> factoryNMS = new ChannelFactory<INMSForScript>(
                binding,
                new EndpointAddress("net.tcp://localhost:10011/NetworkModelService/FillingScript"));
            INMSForScript proxyNMS = factoryNMS.CreateChannel();

            List<IdentifiedObject> consumers = proxyNMS.GetConsumers();
            Dictionary<long, IdentifiedObject> voltages = proxyNMS.GetVoltages();
            Random rnd = new Random();
            double voltageLoss = 0.0007;

            while (beginning < end)
            {
                Dictionary<long, DynamicMeasurement> measurements = new Dictionary<long, DynamicMeasurement>();
                
                foreach (EnergyConsumer ec in consumers)
                {
                    switch (ec.Type)
                    {
                        case ConsumerType.FIRM:
                            if (!measurements.ContainsKey(ec.GlobalId))
                            {
                                measurements.Add(ec.GlobalId, new DynamicMeasurement(ec.GlobalId, beginning));
                            }

                            float valueToSendFirmP = 0;
                            float valueToSendFirmQ = 0;
                            float valueToSendFirmV = 0;

                            valueToSendFirmP = DailyConsumption.GetPQFirm(beginning, rnd, ec.PMax) / 1000;

                            if (valueToSendFirmP < 0)
                            {
                                valueToSendFirmP = 0;
                            }

                            measurements[ec.GlobalId].CurrentP = valueToSendFirmP;
                            measurements[ec.GlobalId].Season = DailyConsumption.GetSeason(beginning);
                            measurements[ec.GlobalId].Type = ec.Type;
                            valueToSendFirmQ = DailyConsumption.GetPQFirm(beginning, rnd, ec.QMax) / 1000;

                            if (valueToSendFirmQ < 0)
                            {
                                valueToSendFirmQ = 0;
                            }

                            measurements[ec.GlobalId].CurrentQ = valueToSendFirmQ;
                            float bvF = ((BaseVoltage)voltages[ec.BaseVoltage]).NominalVoltage;

                            if ((valueToSendFirmP / (ec.PMax / 1000)) < 0.1)
                            {
                                valueToSendFirmV = (float)(bvF + bvF * voltageLoss * 100);
                            }
                            else
                            {
                                valueToSendFirmV = (float)(bvF - bvF * voltageLoss * ((valueToSendFirmP / (ec.PMax / 1000)) * 100));
                            }

                            measurements[ec.GlobalId].CurrentV = valueToSendFirmV;

                            break;
                        case ConsumerType.HOUSEHOLD:
                            if (!measurements.ContainsKey(ec.GlobalId))
                            {
                                measurements.Add(ec.GlobalId, new DynamicMeasurement(ec.GlobalId, beginning));
                            }

                            float valueToSendHHP = 0;
                            float valueToSendHHQ = 0;
                            float valueToSendHHV = 0;

                            valueToSendHHP = DailyConsumption.GetPQHouseHold(beginning, rnd, ec.PMax) / 1000;

                            if (valueToSendHHP < 0)
                            {
                                valueToSendHHP = 0;
                            }

                            measurements[ec.GlobalId].CurrentP = valueToSendHHP;
                            measurements[ec.GlobalId].Season = DailyConsumption.GetSeason(beginning);
                            measurements[ec.GlobalId].Type = ec.Type;

                            valueToSendHHQ = DailyConsumption.GetPQHouseHold(beginning, rnd, ec.QMax) / 1000;

                            if (valueToSendHHQ < 0)
                            {
                                valueToSendHHQ = 0;
                            }

                            measurements[ec.GlobalId].CurrentQ = valueToSendHHQ;
                            float bvHH = ((BaseVoltage)voltages[ec.BaseVoltage]).NominalVoltage;

                            if ((valueToSendHHP / (ec.PMax / 1000)) < 0.1)
                            {
                                valueToSendHHV = (float)(bvHH + bvHH * voltageLoss * 100);
                            }
                            else
                            {
                                valueToSendHHV = (float)(bvHH - bvHH * voltageLoss * ((valueToSendHHP / (ec.PMax / 1000)) * 100));
                            }

                            measurements[ec.GlobalId].CurrentV = valueToSendHHV;

                            break;
                        case ConsumerType.SHOPPING_CENTER:
                            if (!measurements.ContainsKey(ec.GlobalId))
                            {
                                measurements.Add(ec.GlobalId, new DynamicMeasurement(ec.GlobalId, beginning));
                            }

                            float valueToSendSCP = 0;
                            float valueToSendSCQ = 0;
                            float valueToSendSCV = 0;
                            valueToSendSCP = DailyConsumption.GetPQShoppingCenter(beginning, rnd, ec.PMax) / 1000;

                            if (valueToSendSCP < 0)
                            {
                                valueToSendSCP = 0;
                            }

                            measurements[ec.GlobalId].CurrentP = valueToSendSCP;
                            measurements[ec.GlobalId].Season = DailyConsumption.GetSeason(beginning);
                            measurements[ec.GlobalId].Type = ec.Type;
                            valueToSendSCQ = DailyConsumption.GetPQShoppingCenter(beginning, rnd, ec.QMax) / 1000;

                            if (valueToSendSCQ < 0)
                            {
                                valueToSendSCQ = 0;
                            }

                            measurements[ec.GlobalId].CurrentQ = valueToSendSCQ;
                            float bvSC = ((BaseVoltage)voltages[ec.BaseVoltage]).NominalVoltage;

                            if ((valueToSendSCP / (ec.PMax / 1000)) < 0.1)
                            {
                                valueToSendSCV = (float)(bvSC + bvSC * voltageLoss * 100);
                            }
                            else
                            {
                                valueToSendSCV = (float)(bvSC - bvSC * voltageLoss * ((valueToSendSCP / (ec.PMax / 1000)) * 100));
                            }

                            measurements[ec.GlobalId].CurrentV = valueToSendSCV;

                            break;
                    }
                }

                measurementsList.AddRange(measurements.Values.ToList());
                beginning = beginning.AddMinutes(resolution);
            }

            TimeSeriesDbAdapter.AddMeasurements(measurementsList);
        }
    }
}