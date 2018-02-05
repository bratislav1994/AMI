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
            DateTime beginning = new DateTime(2017, 7, 7);
            beginning = beginning.AddSeconds(1);
            DateTime end = new DateTime(2017, 7, 9);

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

                            Tuple<float, Season> tuplePF = GetPQFirm(beginning, rnd, ec.PMax);

                            valueToSendFirmP = tuplePF.Item1;

                            if (valueToSendFirmP < 0)
                            {
                                valueToSendFirmP = 0;
                            }

                            measurements[ec.GlobalId].CurrentP = valueToSendFirmP;
                            measurements[ec.GlobalId].Season = tuplePF.Item2;
                            measurements[ec.GlobalId].Type = ec.Type;

                            Tuple<float, Season> tupleQF = GetPQFirm(beginning, rnd, ec.QMax);

                            valueToSendFirmQ = tupleQF.Item1;

                            if (valueToSendFirmQ < 0)
                            {
                                valueToSendFirmQ = 0;
                            }

                            measurements[ec.GlobalId].CurrentQ = valueToSendFirmQ;
                            float bvF = ((BaseVoltage)voltages[ec.BaseVoltage]).NominalVoltage;

                            if ((valueToSendFirmP / ec.PMax) < 0.1)
                            {
                                valueToSendFirmV = (float)(bvF + bvF * voltageLoss * 100);
                            }
                            else
                            {
                                valueToSendFirmV = (float)(bvF - bvF * voltageLoss * ((valueToSendFirmP / ec.PMax) * 100));
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

                            Tuple<float, Season> tuplePHH = GetPQHouseHold(beginning, rnd, ec.PMax);

                            valueToSendHHP = tuplePHH.Item1;

                            if (valueToSendHHP < 0)
                            {
                                valueToSendHHP = 0;
                            }

                            measurements[ec.GlobalId].CurrentP = valueToSendHHP;
                            measurements[ec.GlobalId].Season = tuplePHH.Item2;
                            measurements[ec.GlobalId].Type = ec.Type;

                            Tuple<float, Season> tupleQHH = GetPQHouseHold(beginning, rnd, ec.QMax);

                            valueToSendHHQ = tupleQHH.Item1;

                            if (valueToSendHHQ < 0)
                            {
                                valueToSendHHQ = 0;
                            }

                            measurements[ec.GlobalId].CurrentQ = valueToSendHHQ;
                            float bvHH = ((BaseVoltage)voltages[ec.BaseVoltage]).NominalVoltage;

                            if ((valueToSendHHP / ec.PMax) < 0.1)
                            {
                                valueToSendHHV = (float)(bvHH + bvHH * voltageLoss * 100);
                            }
                            else
                            {
                                valueToSendHHV = (float)(bvHH - bvHH * voltageLoss * ((valueToSendHHP / ec.PMax) * 100));
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

                            Tuple<float, Season> tuplePSC = GetPQShoppingCenter(beginning, rnd, ec.PMax);
                            valueToSendSCP = tuplePSC.Item1;

                            if (valueToSendSCP < 0)
                            {
                                valueToSendSCP = 0;
                            }

                            measurements[ec.GlobalId].CurrentP = valueToSendSCP;
                            measurements[ec.GlobalId].Season = tuplePSC.Item2;
                            measurements[ec.GlobalId].Type = ec.Type;

                            Tuple<float, Season> tupleQSC = GetPQShoppingCenter(beginning, rnd, ec.QMax);
                            valueToSendSCQ = tupleQSC.Item1;

                            if (valueToSendSCQ < 0)
                            {
                                valueToSendSCQ = 0;
                            }

                            measurements[ec.GlobalId].CurrentQ = valueToSendSCQ;
                            float bvSC = ((BaseVoltage)voltages[ec.BaseVoltage]).NominalVoltage;

                            if ((valueToSendSCP / ec.PMax) < 0.1)
                            {
                                valueToSendSCV = (float)(bvSC + bvSC * voltageLoss * 100);
                            }
                            else
                            {
                                valueToSendSCV = (float)(bvSC - bvSC * voltageLoss * ((valueToSendSCP / ec.PMax) * 100));
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

        #region getting Ps and Qs

        private static Tuple<float, Season> GetPQFirm(DateTime beginning, Random rnd, float maxPQ)
        {
            float valueToSendFirmPQ;
            Season season;

            if (beginning.DayOfWeek == DayOfWeek.Saturday || beginning.DayOfWeek == DayOfWeek.Sunday)
            {
                if (beginning.Month > DailyConsumption.SummerBeggining.Month && beginning.Month < DailyConsumption.SummerEnding.Month) // letoooo
                {
                    if (beginning.Month == DailyConsumption.WorkersDay.Month && beginning.Day == DailyConsumption.WorkersDay.Day)
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionHolidaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                    else
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWeekendSummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                }
                else if (beginning.Month < DailyConsumption.WinterEnding.Month || beginning.Month > DailyConsumption.WinterBeggining.Month)
                {
                    if (beginning.Month == DailyConsumption.Christmas.Month && beginning.Day == DailyConsumption.Christmas.Day)
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionHolidayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                    else
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWeekendWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                }
                else if ((beginning.Month == DailyConsumption.SummerBeggining.Month && beginning.Day >= DailyConsumption.SummerBeggining.Day) ||
                            (beginning.Month == DailyConsumption.SummerEnding.Month && beginning.Day <= DailyConsumption.SummerEnding.Day)) //opet letooo
                {
                    valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWeekendSummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.SUMMER;
                }
                else
                {
                    valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWeekendWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.WINTER;
                }
            }
            else
            {
                if (beginning.Month > DailyConsumption.SummerBeggining.Month && beginning.Month < DailyConsumption.SummerEnding.Month) // letoooo
                {
                    if (beginning.Month == DailyConsumption.WorkersDay.Month && beginning.Day == DailyConsumption.WorkersDay.Day)
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionHolidaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                    else
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWorkDaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                }
                else if (beginning.Month < DailyConsumption.WinterEnding.Month || beginning.Month > DailyConsumption.WinterBeggining.Month)
                {
                    if (beginning.Month == DailyConsumption.Christmas.Month && beginning.Day == DailyConsumption.Christmas.Day)
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionHolidayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                    else
                    {
                        valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWorkDayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                }
                else if ((beginning.Month == DailyConsumption.SummerBeggining.Month && beginning.Day >= DailyConsumption.SummerBeggining.Day) ||
                            (beginning.Month == DailyConsumption.SummerEnding.Month && beginning.Day <= DailyConsumption.SummerEnding.Day)) //opet letooo
                {
                    valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWorkDaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.SUMMER;
                }
                else
                {
                    valueToSendFirmPQ = maxPQ * DailyConsumption.FirmConsumptionWorkDayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.WINTER;
                }
            }

            return new Tuple<float, Season>(valueToSendFirmPQ, season);
        }

        private static Tuple<float, Season> GetPQHouseHold(DateTime beginning, Random rnd, float maxPQ)
        {
            float valueToSendHouseHoldPQ;
            Season season;

            if (beginning.DayOfWeek == DayOfWeek.Saturday || beginning.DayOfWeek == DayOfWeek.Sunday)
            {
                if (beginning.Month > DailyConsumption.SummerBeggining.Month && beginning.Month < DailyConsumption.SummerEnding.Month) // letoooo
                {
                    if (beginning.Month == DailyConsumption.WorkersDay.Month && beginning.Day == DailyConsumption.WorkersDay.Day)
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionHolidaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                    else
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWeekendSummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                }
                else if (beginning.Month < DailyConsumption.WinterEnding.Month || beginning.Month > DailyConsumption.WinterBeggining.Month)
                {
                    if (beginning.Month == DailyConsumption.Christmas.Month && beginning.Day == DailyConsumption.Christmas.Day)
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionHolidayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                    else
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWeekendWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                }
                else if ((beginning.Month == DailyConsumption.SummerBeggining.Month && beginning.Day >= DailyConsumption.SummerBeggining.Day) ||
                            (beginning.Month == DailyConsumption.SummerEnding.Month && beginning.Day <= DailyConsumption.SummerEnding.Day)) //opet letooo
                {
                    valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWeekendSummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.SUMMER;
                }
                else
                {
                    valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWeekendWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.WINTER;
                }
            }
            else
            {
                if (beginning.Month > DailyConsumption.SummerBeggining.Month && beginning.Month < DailyConsumption.SummerEnding.Month) // letoooo
                {
                    if (beginning.Month == DailyConsumption.WorkersDay.Month && beginning.Day == DailyConsumption.WorkersDay.Day)
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionHolidaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                    else
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWorkDaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                }
                else if (beginning.Month < DailyConsumption.WinterEnding.Month || beginning.Month > DailyConsumption.WinterBeggining.Month)
                {
                    if (beginning.Month == DailyConsumption.Christmas.Month && beginning.Day == DailyConsumption.Christmas.Day)
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionHolidayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                    else
                    {
                        valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWorkDayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                }
                else if ((beginning.Month == DailyConsumption.SummerBeggining.Month && beginning.Day >= DailyConsumption.SummerBeggining.Day) ||
                            (beginning.Month == DailyConsumption.SummerEnding.Month && beginning.Day <= DailyConsumption.SummerEnding.Day)) //opet letooo
                {
                    valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWorkDaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.SUMMER;
                }
                else
                {
                    valueToSendHouseHoldPQ = maxPQ * DailyConsumption.HouseholdConsumptionWorkDayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.WINTER;
                }
            }

            return new Tuple<float, Season>(valueToSendHouseHoldPQ, season);
        }

        private static Tuple<float, Season> GetPQShoppingCenter(DateTime beginning, Random rnd, float maxPQ)
        {
            float valueToSendShocppingCenterPQ;
            Season season;

            if (beginning.DayOfWeek == DayOfWeek.Saturday || beginning.DayOfWeek == DayOfWeek.Sunday)
            {
                if (beginning.Month > DailyConsumption.SummerBeggining.Month && beginning.Month < DailyConsumption.SummerEnding.Month) // letoooo
                {
                    if (beginning.Month == DailyConsumption.WorkersDay.Month && beginning.Day == DailyConsumption.WorkersDay.Day)
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionHolidaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                    else
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWeekendSummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                }
                else if (beginning.Month < DailyConsumption.WinterEnding.Month || beginning.Month > DailyConsumption.WinterBeggining.Month)
                {
                    if (beginning.Month == DailyConsumption.Christmas.Month && beginning.Day == DailyConsumption.Christmas.Day)
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionHolidayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                    else
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWeekendWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                }
                else if ((beginning.Month == DailyConsumption.SummerBeggining.Month && beginning.Day >= DailyConsumption.SummerBeggining.Day) ||
                            (beginning.Month == DailyConsumption.SummerEnding.Month && beginning.Day <= DailyConsumption.SummerEnding.Day)) //opet letooo
                {
                    valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWeekendSummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.SUMMER;
                }
                else
                {
                    valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWeekendWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.WINTER;
                }
            }
            else
            {
                if (beginning.Month > DailyConsumption.SummerBeggining.Month && beginning.Month < DailyConsumption.SummerEnding.Month) // letoooo
                {
                    if (beginning.Month == DailyConsumption.WorkersDay.Month && beginning.Day == DailyConsumption.WorkersDay.Day)
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionHolidaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                    else
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWorkDaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.SUMMER;
                    }
                }
                else if (beginning.Month < DailyConsumption.WinterEnding.Month || beginning.Month > DailyConsumption.WinterBeggining.Month)
                {
                    if (beginning.Month == DailyConsumption.Christmas.Month && beginning.Day == DailyConsumption.Christmas.Day)
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionHolidayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                    else
                    {
                        valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWorkDayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                        season = Season.WINTER;
                    }
                }
                else if ((beginning.Month == DailyConsumption.SummerBeggining.Month && beginning.Day >= DailyConsumption.SummerBeggining.Day) ||
                            (beginning.Month == DailyConsumption.SummerEnding.Month && beginning.Day <= DailyConsumption.SummerEnding.Day)) //opet letooo
                {
                    valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWorkDaySummer[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.SUMMER;
                }
                else
                {
                    valueToSendShocppingCenterPQ = maxPQ * DailyConsumption.ShoppingCenterConsumptionWorkDayWinter[beginning.Hour % 24] + rnd.Next(-5, 5);
                    season = Season.WINTER;
                }
            }

            return new Tuple<float, Season>(valueToSendShocppingCenterPQ, season);
        }

        #endregion getting Ps and Qs
    }
}