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

namespace CalculationEngine
{
    public class Filler
    {
        private static Dictionary<int, float> householdConsumption = new Dictionary<int, float>();
        private static Dictionary<int, float> shoppingCenterConsumption = new Dictionary<int, float>();
        private static Dictionary<int, float> firmConsumption = new Dictionary<int, float>();
        private const int resolution = 60;
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
            InitFirm();
            InitHousehold();
            InitShoppingCenter();

            DateTime beginning = new DateTime(2018, 1, 1);
            beginning = beginning.AddSeconds(1);
            DateTime end = new DateTime(2018, 1, 3);

            NetTcpBinding binding = new NetTcpBinding();
            binding.SendTimeout = TimeSpan.FromDays(1);
            ChannelFactory<INMSForScript> factoryNMS = new ChannelFactory<INMSForScript>(
                binding,
                new EndpointAddress("net.tcp://localhost:10011/NetworkModelService/FillingScript"));
            INMSForScript proxyNMS = factoryNMS.CreateChannel();

            Tuple<Dictionary<long, IdentifiedObject>, List<IdentifiedObject>> consumers = proxyNMS.GetConsumers();

            //while (beginning < end)
            //{
            //    Dictionary<long, DynamicMeasurement> measurements = new Dictionary<long, DynamicMeasurement>();

            //    foreach (Measurement m in consumers.Item2)
            //    {
            //        switch (((EnergyConsumer)consumers.Item1[m.PowerSystemResourceRef]).Type)
            //        {
            //            case ConsumerType.FIRM:
            //                if (!measurements.ContainsKey(m.PowerSystemResourceRef))
            //                {
            //                    measurements.Add(m.PowerSystemResourceRef, new DynamicMeasurement(m.PowerSystemResourceRef, beginning));
            //                }

            //                float valueToSendFirm = 0;

            //                switch (m.UnitSymbol)
            //                {
            //                    case UnitSymbol.P:
            //                        valueToSendFirm = m.MaxRawValue * firmConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentP = valueToSendFirm;
            //                        break;
            //                    case UnitSymbol.Q:
            //                        valueToSendFirm = m.MaxRawValue * firmConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentQ = valueToSendFirm;
            //                        break;
            //                    case UnitSymbol.V:
            //                        valueToSendFirm = m.MaxRawValue * firmConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentV = valueToSendFirm;
            //                        break;
            //                }
            //                break;
            //            case ConsumerType.HOUSEHOLD:
            //                if (!measurements.ContainsKey(m.PowerSystemResourceRef))
            //                {
            //                    measurements.Add(m.PowerSystemResourceRef, new DynamicMeasurement(m.PowerSystemResourceRef, beginning));
            //                }

            //                float valueToSendHouseHold = 0;

            //                switch (m.UnitSymbol)
            //                {
            //                    case UnitSymbol.P:
            //                        valueToSendHouseHold = m.MaxRawValue * householdConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentP = valueToSendHouseHold;
            //                        break;
            //                    case UnitSymbol.Q:
            //                        valueToSendHouseHold = m.MaxRawValue * householdConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentQ = valueToSendHouseHold;
            //                        break;
            //                    case UnitSymbol.V:
            //                        valueToSendHouseHold = m.MaxRawValue * householdConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentV = valueToSendHouseHold;
            //                        break;
            //                }
            //                break;
            //            case ConsumerType.SHOPPING_CENTER:
            //                if (!measurements.ContainsKey(m.PowerSystemResourceRef))
            //                {
            //                    measurements.Add(m.PowerSystemResourceRef, new DynamicMeasurement(m.PowerSystemResourceRef, beginning));
            //                }

            //                float valueToSendShoppingCenter = 0;

            //                switch (m.UnitSymbol)
            //                {
            //                    case UnitSymbol.P:
            //                        valueToSendShoppingCenter = m.MaxRawValue * shoppingCenterConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentP = valueToSendShoppingCenter;
            //                        break;
            //                    case UnitSymbol.Q:
            //                        valueToSendShoppingCenter = m.MaxRawValue * shoppingCenterConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentQ = valueToSendShoppingCenter;
            //                        break;
            //                    case UnitSymbol.V:
            //                        valueToSendShoppingCenter = m.MaxRawValue * shoppingCenterConsumption[beginning.Hour];
            //                        measurements[m.PowerSystemResourceRef].CurrentV = valueToSendShoppingCenter;
            //                        break;
            //                }
            //                break;
            //        }
            //    }

            //    measurementsList.AddRange(measurements.Values.ToList());
            //    beginning = beginning.AddSeconds(resolution);
            //}

            TimeSeriesDbAdapter.AddMeasurements(measurementsList);
        }

        static void InitFirm()
        {
            firmConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumption.Add(i, 0);
            }

            firmConsumption[0] = (float)5 / 100;
            firmConsumption[1] = (float)5 / 100;
            firmConsumption[2] = (float)5 / 100;
            firmConsumption[3] = (float)5 / 100;
            firmConsumption[4] = (float)5 / 100;
            firmConsumption[5] = (float)5 / 100;
            firmConsumption[6] = (float)35 / 100;
            firmConsumption[7] = (float)45 / 100;
            firmConsumption[8] = (float)75 / 100;
            firmConsumption[9] = (float)80 / 100;
            firmConsumption[10] = (float)80 / 100;
            firmConsumption[11] = (float)80 / 100;
            firmConsumption[12] = (float)80 / 100;
            firmConsumption[13] = (float)80 / 100;
            firmConsumption[14] = (float)80 / 100;
            firmConsumption[15] = (float)80 / 100;
            firmConsumption[16] = (float)80 / 100;
            firmConsumption[17] = (float)62 / 100;
            firmConsumption[18] = (float)55 / 100;
            firmConsumption[19] = (float)40 / 100;
            firmConsumption[20] = (float)10 / 100;
            firmConsumption[21] = (float)10 / 100;
            firmConsumption[22] = (float)10 / 100;
            firmConsumption[23] = (float)10 / 100;
        }

        static void InitHousehold()
        {
            householdConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumption.Add(i, 0);
            }

            householdConsumption[0] = (float)5 / 100;
            householdConsumption[1] = (float)5 / 100;
            householdConsumption[2] = (float)5 / 100;
            householdConsumption[3] = (float)5 / 100;
            householdConsumption[4] = (float)5 / 100;
            householdConsumption[5] = (float)7 / 100;
            householdConsumption[6] = (float)14 / 100;
            householdConsumption[7] = (float)25 / 100;
            householdConsumption[8] = (float)17 / 100;
            householdConsumption[9] = (float)12 / 100;
            householdConsumption[10] = (float)12 / 100;
            householdConsumption[11] = (float)12 / 100;
            householdConsumption[12] = (float)12 / 100;
            householdConsumption[13] = (float)11 / 100;
            householdConsumption[14] = (float)14 / 100;
            householdConsumption[15] = (float)15 / 100;
            householdConsumption[16] = (float)45 / 100;
            householdConsumption[17] = (float)55 / 100;
            householdConsumption[18] = (float)75 / 100;
            householdConsumption[19] = (float)60 / 100;
            householdConsumption[20] = (float)50 / 100;
            householdConsumption[21] = (float)52 / 100;
            householdConsumption[22] = (float)33 / 100;
            householdConsumption[23] = (float)10 / 100;
        }

        static void InitShoppingCenter()
        {
            shoppingCenterConsumption = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumption.Add(i, 0);
            }

            shoppingCenterConsumption[0] = (float)5 / 100;
            shoppingCenterConsumption[1] = (float)5 / 100;
            shoppingCenterConsumption[2] = (float)5 / 100;
            shoppingCenterConsumption[3] = (float)5 / 100;
            shoppingCenterConsumption[4] = (float)5 / 100;
            shoppingCenterConsumption[5] = (float)5 / 100;
            shoppingCenterConsumption[6] = (float)35 / 100;
            shoppingCenterConsumption[7] = (float)45 / 100;
            shoppingCenterConsumption[8] = (float)75 / 100;
            shoppingCenterConsumption[9] = (float)80 / 100;
            shoppingCenterConsumption[10] = (float)80 / 100;
            shoppingCenterConsumption[11] = (float)80 / 100;
            shoppingCenterConsumption[12] = (float)80 / 100;
            shoppingCenterConsumption[13] = (float)80 / 100;
            shoppingCenterConsumption[14] = (float)80 / 100;
            shoppingCenterConsumption[15] = (float)80 / 100;
            shoppingCenterConsumption[16] = (float)80 / 100;
            shoppingCenterConsumption[17] = (float)80 / 100;
            shoppingCenterConsumption[18] = (float)80 / 100;
            shoppingCenterConsumption[19] = (float)80 / 100;
            shoppingCenterConsumption[20] = (float)80 / 100;
            shoppingCenterConsumption[21] = (float)80 / 100;
            shoppingCenterConsumption[22] = (float)55 / 100;
            shoppingCenterConsumption[23] = (float)10 / 100;
        }
    }
}