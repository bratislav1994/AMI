using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyConsumptions
{
    public static class DailyConsumption
    {
        private static Dictionary<int, float> householdConsumptionWorkDaySummer;
        private static Dictionary<int, float> shoppingCenterConsumptionWorkDaySummer;
        private static Dictionary<int, float> firmConsumptionWorkDaySummer;
        private static Dictionary<int, float> householdConsumptionWeekendSummer;
        private static Dictionary<int, float> shoppingCenterConsumptionWeekendSummer;
        private static Dictionary<int, float> firmConsumptionWeekendSummer;
        private static Dictionary<int, float> householdConsumptionWorkDayWinter;
        private static Dictionary<int, float> shoppingCenterConsumptionWorkDayWinter;
        private static Dictionary<int, float> firmConsumptionWorkDayWinter;
        private static Dictionary<int, float> householdConsumptionWeekendWinter;
        private static Dictionary<int, float> shoppingCenterConsumptionWeekendWinter;
        private static Dictionary<int, float> firmConsumptionWeekendWinter;
        private static Dictionary<int, float> householdConsumptionHolidaySummer;
        private static Dictionary<int, float> shoppingCenterConsumptionHolidaySummer;
        private static Dictionary<int, float> firmConsumptionHolidaySummer;
        private static Dictionary<int, float> householdConsumptionHolidayWinter;
        private static Dictionary<int, float> shoppingCenterConsumptionHolidayWinter;
        private static Dictionary<int, float> firmConsumptionHolidayWinter;
        private static Dictionary<int, float> householdConsumptionAtypicalDay;
        private static Dictionary<int, float> shoppingCenterConsumptionAtypicalDay;
        private static Dictionary<int, float> firmConsumptionAtypicalDay;
        private static DateTime christmas = new DateTime(1900, 1, 7);
        private static DateTime workersDay = new DateTime(1900, 5, 1);

        #region properties

        public static Dictionary<int, float> HouseholdConsumptionWorkDaySummer
        {
            get
            {
                return householdConsumptionWorkDaySummer == null ? InitHouseholdWorkDaySummer() : householdConsumptionWorkDaySummer;
            }
        }

        public static Dictionary<int, float> ShoppingCenterConsumptionWorkDaySummer
        {
            get
            {
                return shoppingCenterConsumptionWorkDaySummer == null ? InitShoppingCenterWorkDaySummer() : shoppingCenterConsumptionWorkDaySummer;
            }
        }

        public static Dictionary<int, float> FirmConsumptionWorkDaySummer
        {
            get
            {
                return firmConsumptionWorkDaySummer == null ? InitFirmWorkDaySummer() : firmConsumptionWorkDaySummer;
            }
        }

        public static Dictionary<int, float> HouseholdConsumptionWeekendSummer
        {
            get
            {
                return householdConsumptionWeekendSummer == null ? InitHouseholdWeekendSummer() : householdConsumptionWeekendSummer;
            }
        }

        public static Dictionary<int, float> ShoppingCenterConsumptionWeekendSummer
        {
            get
            {
                return shoppingCenterConsumptionWeekendSummer == null ? InitShoppingCenterWeekendSummer() : shoppingCenterConsumptionWeekendSummer;
            }
        }

        public static Dictionary<int, float> FirmConsumptionWeekendSummer
        {
            get
            {
                return firmConsumptionWeekendSummer == null ? InitFirmWeekendSummer() : firmConsumptionWeekendSummer;
            }
        }

        public static Dictionary<int, float> HouseholdConsumptionWorkDayWinter
        {
            get
            {
                return householdConsumptionWorkDayWinter == null ? InitHouseholdWorkDayWinter() : householdConsumptionWorkDayWinter;
            }
        }

        public static Dictionary<int, float> ShoppingCenterConsumptionWorkDayWinter
        {
            get
            {
                return shoppingCenterConsumptionWorkDayWinter == null ? InitShoppingCenterWorkDayWinter() : shoppingCenterConsumptionWorkDayWinter;
            }
        }

        public static Dictionary<int, float> FirmConsumptionWorkDayWinter
        {
            get
            {
                return firmConsumptionWorkDayWinter == null ? InitFirmWorkDayWinter() : firmConsumptionWorkDayWinter;
            }
        }

        public static Dictionary<int, float> HouseholdConsumptionWeekendWinter
        {
            get
            {
                return householdConsumptionWeekendWinter == null ? InitHouseholdWeekendWinter() : householdConsumptionWeekendWinter;
            }
        }

        public static Dictionary<int, float> ShoppingCenterConsumptionWeekendWinter
        {
            get
            {
                return shoppingCenterConsumptionWeekendWinter == null ? InitShoppingCenterWeekendWinter() : shoppingCenterConsumptionWeekendWinter;
            }
        }

        public static Dictionary<int, float> FirmConsumptionWeekendWinter
        {
            get
            {
                return firmConsumptionWeekendWinter == null ? InitFirmWeekendWinter() : firmConsumptionWeekendWinter;
            }
        }

        public static Dictionary<int, float> HouseholdConsumptionHolidaySummer
        {
            get
            {
                return householdConsumptionHolidaySummer == null ? InitHouseholdHolidaySummer() : householdConsumptionHolidaySummer;
            }
        }

        public static Dictionary<int, float> ShoppingCenterConsumptionHolidaySummer
        {
            get
            {
                return shoppingCenterConsumptionHolidaySummer == null ? InitShoppingCenterHolidaySummer() : shoppingCenterConsumptionHolidaySummer;
            }
        }

        public static Dictionary<int, float> FirmConsumptionHolidaySummer
        {
            get
            {
                return firmConsumptionHolidaySummer == null ? InitFirmHolidaySummer() : firmConsumptionHolidaySummer;
            }
        }

        public static Dictionary<int, float> HouseholdConsumptionHolidayWinter
        {
            get
            {
                return householdConsumptionHolidayWinter == null ? InitFirmHolidayWinter() : householdConsumptionHolidayWinter;
            }
        }

        public static Dictionary<int, float> ShoppingCenterConsumptionHolidayWinter
        {
            get
            {
                return shoppingCenterConsumptionHolidayWinter == null ? InitShoppingCenterHolidayWinter() : shoppingCenterConsumptionHolidayWinter;
            }
        }

        public static Dictionary<int, float> FirmConsumptionHolidayWinter
        {
            get
            {
                return firmConsumptionHolidayWinter == null ? InitFirmHolidayWinter() : firmConsumptionHolidayWinter;
            }
        }
        
        public static Dictionary<int, float> HouseholdConsumptionAtypicalDay
        {
            get
            {
                return householdConsumptionAtypicalDay == null ? InitHouseholdAtypicalDay() : householdConsumptionAtypicalDay;
            }
        }

        public static Dictionary<int, float> ShoppingCenterConsumptionAtypicalDay
        {
            get
            {
                return shoppingCenterConsumptionAtypicalDay == null ? InitShoppingCenterAtypicalDay() : shoppingCenterConsumptionAtypicalDay;
            }
        }

        public static Dictionary<int, float> FirmConsumptionAtypicalDay
        {
            get
            {
                return firmConsumptionAtypicalDay == null ? InitFirmAtypicalDay() : firmConsumptionAtypicalDay;
            }
        }

        public static DateTime Christmas
        {
            get
            {
                return christmas;
            }
        }

        public static DateTime WorkersDay
        {
            get
            {
                return workersDay;
            }
        }

        #endregion

        #region workday and weekend - summer

        private static Dictionary<int, float> InitHouseholdWorkDaySummer()
        {
            householdConsumptionWorkDaySummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumptionWorkDaySummer.Add(i, 0);
            }

            householdConsumptionWorkDaySummer[0] = (float)5 / 100;
            householdConsumptionWorkDaySummer[1] = (float)5 / 100;
            householdConsumptionWorkDaySummer[2] = (float)5 / 100;
            householdConsumptionWorkDaySummer[3] = (float)5 / 100;
            householdConsumptionWorkDaySummer[4] = (float)5 / 100;
            householdConsumptionWorkDaySummer[5] = (float)7 / 100;
            householdConsumptionWorkDaySummer[6] = (float)14 / 100;
            householdConsumptionWorkDaySummer[7] = (float)20 / 100;
            householdConsumptionWorkDaySummer[8] = (float)30 / 100;
            householdConsumptionWorkDaySummer[9] = (float)38 / 100;
            householdConsumptionWorkDaySummer[10] = (float)20 / 100;
            householdConsumptionWorkDaySummer[11] = (float)30 / 100;
            householdConsumptionWorkDaySummer[12] = (float)40 / 100;
            householdConsumptionWorkDaySummer[13] = (float)37 / 100;
            householdConsumptionWorkDaySummer[14] = (float)37 / 100;
            householdConsumptionWorkDaySummer[15] = (float)38 / 100;
            householdConsumptionWorkDaySummer[16] = (float)40 / 100;
            householdConsumptionWorkDaySummer[17] = (float)80 / 100;
            householdConsumptionWorkDaySummer[18] = (float)100 / 100;
            householdConsumptionWorkDaySummer[19] = (float)70 / 100;
            householdConsumptionWorkDaySummer[20] = (float)50 / 100;
            householdConsumptionWorkDaySummer[21] = (float)52 / 100;
            householdConsumptionWorkDaySummer[22] = (float)33 / 100;
            householdConsumptionWorkDaySummer[23] = (float)10 / 100;

            return householdConsumptionWorkDaySummer;
        }

        private static Dictionary<int, float> InitFirmWorkDaySummer()
        {
            firmConsumptionWorkDaySummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumptionWorkDaySummer.Add(i, 0);
            }

            firmConsumptionWorkDaySummer[0] = (float)11 / 100;
            firmConsumptionWorkDaySummer[1] = (float)11 / 100;
            firmConsumptionWorkDaySummer[2] = (float)11 / 100;
            firmConsumptionWorkDaySummer[3] = (float)11 / 100;
            firmConsumptionWorkDaySummer[4] = (float)11 / 100;
            firmConsumptionWorkDaySummer[5] = (float)11 / 100;
            firmConsumptionWorkDaySummer[6] = (float)15 / 100;
            firmConsumptionWorkDaySummer[7] = (float)30 / 100;
            firmConsumptionWorkDaySummer[8] = (float)75 / 100;
            firmConsumptionWorkDaySummer[9] = (float)100 / 100;
            firmConsumptionWorkDaySummer[10] = (float)80 / 100;
            firmConsumptionWorkDaySummer[11] = (float)85 / 100;
            firmConsumptionWorkDaySummer[12] = (float)90 / 100;
            firmConsumptionWorkDaySummer[13] = (float)90 / 100;
            firmConsumptionWorkDaySummer[14] = (float)90 / 100;
            firmConsumptionWorkDaySummer[15] = (float)90 / 100;
            firmConsumptionWorkDaySummer[16] = (float)85 / 100;
            firmConsumptionWorkDaySummer[17] = (float)65 / 100;
            firmConsumptionWorkDaySummer[18] = (float)55 / 100;
            firmConsumptionWorkDaySummer[19] = (float)40 / 100;
            firmConsumptionWorkDaySummer[20] = (float)11 / 100;
            firmConsumptionWorkDaySummer[21] = (float)11 / 100;
            firmConsumptionWorkDaySummer[22] = (float)11 / 100;
            firmConsumptionWorkDaySummer[23] = (float)11 / 100;

            return firmConsumptionWorkDaySummer;
        }

        private static Dictionary<int, float> InitShoppingCenterWorkDaySummer()
        {
            shoppingCenterConsumptionWorkDaySummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumptionWorkDaySummer.Add(i, 0);
            }

            shoppingCenterConsumptionWorkDaySummer[0] = (float)11 / 100;
            shoppingCenterConsumptionWorkDaySummer[1] = (float)11 / 100;
            shoppingCenterConsumptionWorkDaySummer[2] = (float)11 / 100;
            shoppingCenterConsumptionWorkDaySummer[3] = (float)11 / 100;
            shoppingCenterConsumptionWorkDaySummer[4] = (float)11 / 100;
            shoppingCenterConsumptionWorkDaySummer[5] = (float)11 / 100;
            shoppingCenterConsumptionWorkDaySummer[6] = (float)11 / 100;
            shoppingCenterConsumptionWorkDaySummer[7] = (float)45 / 100;
            shoppingCenterConsumptionWorkDaySummer[8] = (float)75 / 100;
            shoppingCenterConsumptionWorkDaySummer[9] = (float)100 / 100;
            shoppingCenterConsumptionWorkDaySummer[10] = (float)80 / 100;
            shoppingCenterConsumptionWorkDaySummer[11] = (float)80 / 100;
            shoppingCenterConsumptionWorkDaySummer[12] = (float)90 / 100;
            shoppingCenterConsumptionWorkDaySummer[13] = (float)90 / 100;
            shoppingCenterConsumptionWorkDaySummer[14] = (float)90 / 100;
            shoppingCenterConsumptionWorkDaySummer[15] = (float)90 / 100;
            shoppingCenterConsumptionWorkDaySummer[16] = (float)90 / 100;
            shoppingCenterConsumptionWorkDaySummer[17] = (float)85 / 100;
            shoppingCenterConsumptionWorkDaySummer[18] = (float)85 / 100;
            shoppingCenterConsumptionWorkDaySummer[19] = (float)80 / 100;
            shoppingCenterConsumptionWorkDaySummer[20] = (float)80 / 100;
            shoppingCenterConsumptionWorkDaySummer[21] = (float)80 / 100;
            shoppingCenterConsumptionWorkDaySummer[22] = (float)55 / 100;
            shoppingCenterConsumptionWorkDaySummer[23] = (float)10 / 100;

            return shoppingCenterConsumptionWorkDaySummer;
        }

        private static Dictionary<int, float> InitHouseholdWeekendSummer()
        {
            householdConsumptionWeekendSummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumptionWeekendSummer.Add(i, 0);
            }

            householdConsumptionWeekendSummer[0] = (float)15 / 100;
            householdConsumptionWeekendSummer[1] = (float)9 / 100;
            householdConsumptionWeekendSummer[2] = (float)9 / 100;
            householdConsumptionWeekendSummer[3] = (float)9 / 100;
            householdConsumptionWeekendSummer[4] = (float)9 / 100;
            householdConsumptionWeekendSummer[5] = (float)9 / 100;
            householdConsumptionWeekendSummer[6] = (float)11 / 100;
            householdConsumptionWeekendSummer[7] = (float)12 / 100;
            householdConsumptionWeekendSummer[8] = (float)16 / 100;
            householdConsumptionWeekendSummer[9] = (float)35 / 100;
            householdConsumptionWeekendSummer[10] = (float)45 / 100;
            householdConsumptionWeekendSummer[11] = (float)65 / 100;
            householdConsumptionWeekendSummer[12] = (float)100 / 100;
            householdConsumptionWeekendSummer[13] = (float)65 / 100;
            householdConsumptionWeekendSummer[14] = (float)65 / 100;
            householdConsumptionWeekendSummer[15] = (float)60 / 100;
            householdConsumptionWeekendSummer[16] = (float)30 / 100;
            householdConsumptionWeekendSummer[17] = (float)30 / 100;
            householdConsumptionWeekendSummer[18] = (float)30 / 100;
            householdConsumptionWeekendSummer[19] = (float)45 / 100;
            householdConsumptionWeekendSummer[20] = (float)50 / 100;
            householdConsumptionWeekendSummer[21] = (float)47 / 100;
            householdConsumptionWeekendSummer[22] = (float)33 / 100;
            householdConsumptionWeekendSummer[23] = (float)20 / 100;

            return householdConsumptionWeekendSummer;
        }

        private static Dictionary<int, float> InitFirmWeekendSummer()
        {
            firmConsumptionWeekendSummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumptionWeekendSummer.Add(i, 0);
            }

            firmConsumptionWeekendSummer[0] = (float)11 / 100;
            firmConsumptionWeekendSummer[1] = (float)11 / 100;
            firmConsumptionWeekendSummer[2] = (float)11 / 100;
            firmConsumptionWeekendSummer[3] = (float)11 / 100;
            firmConsumptionWeekendSummer[4] = (float)11 / 100;
            firmConsumptionWeekendSummer[5] = (float)11 / 100;
            firmConsumptionWeekendSummer[6] = (float)11 / 100;
            firmConsumptionWeekendSummer[7] = (float)11 / 100;
            firmConsumptionWeekendSummer[8] = (float)11 / 100;
            firmConsumptionWeekendSummer[9] = (float)11 / 100;
            firmConsumptionWeekendSummer[10] = (float)11 / 100;
            firmConsumptionWeekendSummer[11] = (float)25 / 100;
            firmConsumptionWeekendSummer[12] = (float)35 / 100;
            firmConsumptionWeekendSummer[13] = (float)35 / 100;
            firmConsumptionWeekendSummer[14] = (float)35 / 100;
            firmConsumptionWeekendSummer[15] = (float)35 / 100;
            firmConsumptionWeekendSummer[16] = (float)35 / 100;
            firmConsumptionWeekendSummer[17] = (float)25 / 100;
            firmConsumptionWeekendSummer[18] = (float)25 / 100;
            firmConsumptionWeekendSummer[19] = (float)15 / 100;
            firmConsumptionWeekendSummer[20] = (float)11 / 100;
            firmConsumptionWeekendSummer[21] = (float)11 / 100;
            firmConsumptionWeekendSummer[22] = (float)11 / 100;
            firmConsumptionWeekendSummer[23] = (float)11 / 100;

            return firmConsumptionWeekendSummer;
        }

        private static Dictionary<int, float> InitShoppingCenterWeekendSummer()
        {
            shoppingCenterConsumptionWeekendSummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumptionWeekendSummer.Add(i, 0);
            }

            shoppingCenterConsumptionWeekendSummer[0] = (float)11 / 100;
            shoppingCenterConsumptionWeekendSummer[1] = (float)11 / 100;
            shoppingCenterConsumptionWeekendSummer[2] = (float)11 / 100;
            shoppingCenterConsumptionWeekendSummer[3] = (float)11 / 100;
            shoppingCenterConsumptionWeekendSummer[4] = (float)11 / 100;
            shoppingCenterConsumptionWeekendSummer[5] = (float)11 / 100;
            shoppingCenterConsumptionWeekendSummer[6] = (float)11 / 100;
            shoppingCenterConsumptionWeekendSummer[7] = (float)45 / 100;
            shoppingCenterConsumptionWeekendSummer[8] = (float)75 / 100;
            shoppingCenterConsumptionWeekendSummer[9] = (float)100 / 100;
            shoppingCenterConsumptionWeekendSummer[10] = (float)80 / 100;
            shoppingCenterConsumptionWeekendSummer[11] = (float)80 / 100;
            shoppingCenterConsumptionWeekendSummer[12] = (float)90 / 100;
            shoppingCenterConsumptionWeekendSummer[13] = (float)90 / 100;
            shoppingCenterConsumptionWeekendSummer[14] = (float)90 / 100;
            shoppingCenterConsumptionWeekendSummer[15] = (float)90 / 100;
            shoppingCenterConsumptionWeekendSummer[16] = (float)90 / 100;
            shoppingCenterConsumptionWeekendSummer[17] = (float)85 / 100;
            shoppingCenterConsumptionWeekendSummer[18] = (float)85 / 100;
            shoppingCenterConsumptionWeekendSummer[19] = (float)80 / 100;
            shoppingCenterConsumptionWeekendSummer[20] = (float)80 / 100;
            shoppingCenterConsumptionWeekendSummer[21] = (float)55 / 100;
            shoppingCenterConsumptionWeekendSummer[22] = (float)40 / 100;
            shoppingCenterConsumptionWeekendSummer[23] = (float)10 / 100;

            return shoppingCenterConsumptionWeekendSummer;
        }

        #endregion

        #region workday and weekend - winter

        private static Dictionary<int, float> InitHouseholdWorkDayWinter()
        {
            householdConsumptionWorkDayWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumptionWorkDayWinter.Add(i, 0);
            }

            householdConsumptionWorkDayWinter[0] = (float)12 / 100;
            householdConsumptionWorkDayWinter[1] = (float)12 / 100;
            householdConsumptionWorkDayWinter[2] = (float)12 / 100;
            householdConsumptionWorkDayWinter[3] = (float)12 / 100;
            householdConsumptionWorkDayWinter[4] = (float)12 / 100;
            householdConsumptionWorkDayWinter[5] = (float)12 / 100;
            householdConsumptionWorkDayWinter[6] = (float)22 / 100;
            householdConsumptionWorkDayWinter[7] = (float)30 / 100;
            householdConsumptionWorkDayWinter[8] = (float)30 / 100;
            householdConsumptionWorkDayWinter[9] = (float)35 / 100;
            householdConsumptionWorkDayWinter[10] = (float)20 / 100;
            householdConsumptionWorkDayWinter[11] = (float)17 / 100;
            householdConsumptionWorkDayWinter[12] = (float)15 / 100;
            householdConsumptionWorkDayWinter[13] = (float)15 / 100;
            householdConsumptionWorkDayWinter[14] = (float)16 / 100;
            householdConsumptionWorkDayWinter[15] = (float)25 / 100;
            householdConsumptionWorkDayWinter[16] = (float)40 / 100;
            householdConsumptionWorkDayWinter[17] = (float)80 / 100;
            householdConsumptionWorkDayWinter[18] = (float)100 / 100;
            householdConsumptionWorkDayWinter[19] = (float)70 / 100;
            householdConsumptionWorkDayWinter[20] = (float)60 / 100;
            householdConsumptionWorkDayWinter[21] = (float)62 / 100;
            householdConsumptionWorkDayWinter[22] = (float)65 / 100;
            householdConsumptionWorkDayWinter[23] = (float)22 / 100;

            return householdConsumptionWorkDayWinter;
        }

        private static Dictionary<int, float> InitFirmWorkDayWinter()
        {
            firmConsumptionWorkDayWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumptionWorkDayWinter.Add(i, 0);
            }

            firmConsumptionWorkDayWinter[0] = (float)11 / 100;
            firmConsumptionWorkDayWinter[1] = (float)11 / 100;
            firmConsumptionWorkDayWinter[2] = (float)11 / 100;
            firmConsumptionWorkDayWinter[3] = (float)11 / 100;
            firmConsumptionWorkDayWinter[4] = (float)11 / 100;
            firmConsumptionWorkDayWinter[5] = (float)11 / 100;
            firmConsumptionWorkDayWinter[6] = (float)15 / 100;
            firmConsumptionWorkDayWinter[7] = (float)40 / 100;
            firmConsumptionWorkDayWinter[8] = (float)75 / 100;
            firmConsumptionWorkDayWinter[9] = (float)100 / 100;
            firmConsumptionWorkDayWinter[10] = (float)80 / 100;
            firmConsumptionWorkDayWinter[11] = (float)85 / 100;
            firmConsumptionWorkDayWinter[12] = (float)80 / 100;
            firmConsumptionWorkDayWinter[13] = (float)80 / 100;
            firmConsumptionWorkDayWinter[14] = (float)80 / 100;
            firmConsumptionWorkDayWinter[15] = (float)80 / 100;
            firmConsumptionWorkDayWinter[16] = (float)75 / 100;
            firmConsumptionWorkDayWinter[17] = (float)55 / 100;
            firmConsumptionWorkDayWinter[18] = (float)55 / 100;
            firmConsumptionWorkDayWinter[19] = (float)40 / 100;
            firmConsumptionWorkDayWinter[20] = (float)11 / 100;
            firmConsumptionWorkDayWinter[21] = (float)11 / 100;
            firmConsumptionWorkDayWinter[22] = (float)11 / 100;
            firmConsumptionWorkDayWinter[23] = (float)11 / 100;

            return firmConsumptionWorkDayWinter;
        }

        private static Dictionary<int, float> InitShoppingCenterWorkDayWinter()
        {
            shoppingCenterConsumptionWorkDayWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumptionWorkDayWinter.Add(i, 0);
            }

            shoppingCenterConsumptionWorkDayWinter[0] = (float)11 / 100;
            shoppingCenterConsumptionWorkDayWinter[1] = (float)11 / 100;
            shoppingCenterConsumptionWorkDayWinter[2] = (float)11 / 100;
            shoppingCenterConsumptionWorkDayWinter[3] = (float)11 / 100;
            shoppingCenterConsumptionWorkDayWinter[4] = (float)11 / 100;
            shoppingCenterConsumptionWorkDayWinter[5] = (float)11 / 100;
            shoppingCenterConsumptionWorkDayWinter[6] = (float)11 / 100;
            shoppingCenterConsumptionWorkDayWinter[7] = (float)45 / 100;
            shoppingCenterConsumptionWorkDayWinter[8] = (float)75 / 100;
            shoppingCenterConsumptionWorkDayWinter[9] = (float)100 / 100;
            shoppingCenterConsumptionWorkDayWinter[10] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[11] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[12] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[13] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[14] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[15] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[16] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[17] = (float)75 / 100;
            shoppingCenterConsumptionWorkDayWinter[18] = (float)75 / 100;
            shoppingCenterConsumptionWorkDayWinter[19] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[20] = (float)80 / 100;
            shoppingCenterConsumptionWorkDayWinter[21] = (float)85 / 100;
            shoppingCenterConsumptionWorkDayWinter[22] = (float)65 / 100;
            shoppingCenterConsumptionWorkDayWinter[23] = (float)11 / 100;

            return shoppingCenterConsumptionWorkDayWinter;
        }

        private static Dictionary<int, float> InitHouseholdWeekendWinter()
        {
            householdConsumptionWeekendWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumptionWeekendWinter.Add(i, 0);
            }

            householdConsumptionWeekendWinter[0] = (float)15 / 100;
            householdConsumptionWeekendWinter[1] = (float)9 / 100;
            householdConsumptionWeekendWinter[2] = (float)9 / 100;
            householdConsumptionWeekendWinter[3] = (float)9 / 100;
            householdConsumptionWeekendWinter[4] = (float)9 / 100;
            householdConsumptionWeekendWinter[5] = (float)9 / 100;
            householdConsumptionWeekendWinter[6] = (float)15 / 100;
            householdConsumptionWeekendWinter[7] = (float)17 / 100;
            householdConsumptionWeekendWinter[8] = (float)22 / 100;
            householdConsumptionWeekendWinter[9] = (float)35 / 100;
            householdConsumptionWeekendWinter[10] = (float)45 / 100;
            householdConsumptionWeekendWinter[11] = (float)65 / 100;
            householdConsumptionWeekendWinter[12] = (float)100 / 100;
            householdConsumptionWeekendWinter[13] = (float)75 / 100;
            householdConsumptionWeekendWinter[14] = (float)60 / 100;
            householdConsumptionWeekendWinter[15] = (float)55 / 100;
            householdConsumptionWeekendWinter[16] = (float)25 / 100;
            householdConsumptionWeekendWinter[17] = (float)25 / 100;
            householdConsumptionWeekendWinter[18] = (float)25 / 100;
            householdConsumptionWeekendWinter[19] = (float)45 / 100;
            householdConsumptionWeekendWinter[20] = (float)60 / 100;
            householdConsumptionWeekendWinter[21] = (float)67 / 100;
            householdConsumptionWeekendWinter[22] = (float)43 / 100;
            householdConsumptionWeekendWinter[23] = (float)30 / 100;

            return householdConsumptionWeekendWinter;
        }

        private static Dictionary<int, float> InitFirmWeekendWinter()
        {
            firmConsumptionWeekendWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumptionWeekendWinter.Add(i, 0);
            }

            firmConsumptionWeekendWinter[0] = (float)11 / 100;
            firmConsumptionWeekendWinter[1] = (float)11 / 100;
            firmConsumptionWeekendWinter[2] = (float)11 / 100;
            firmConsumptionWeekendWinter[3] = (float)11 / 100;
            firmConsumptionWeekendWinter[4] = (float)11 / 100;
            firmConsumptionWeekendWinter[5] = (float)11 / 100;
            firmConsumptionWeekendWinter[6] = (float)11 / 100;
            firmConsumptionWeekendWinter[7] = (float)11 / 100;
            firmConsumptionWeekendWinter[8] = (float)11 / 100;
            firmConsumptionWeekendWinter[9] = (float)11 / 100;
            firmConsumptionWeekendWinter[10] = (float)11 / 100;
            firmConsumptionWeekendWinter[11] = (float)11 / 100;
            firmConsumptionWeekendWinter[12] = (float)11 / 100;
            firmConsumptionWeekendWinter[13] = (float)11 / 100;
            firmConsumptionWeekendWinter[14] = (float)11 / 100;
            firmConsumptionWeekendWinter[15] = (float)11 / 100;
            firmConsumptionWeekendWinter[16] = (float)11 / 100;
            firmConsumptionWeekendWinter[17] = (float)11 / 100;
            firmConsumptionWeekendWinter[18] = (float)11 / 100;
            firmConsumptionWeekendWinter[19] = (float)11 / 100;
            firmConsumptionWeekendWinter[20] = (float)11 / 100;
            firmConsumptionWeekendWinter[21] = (float)11 / 100;
            firmConsumptionWeekendWinter[22] = (float)11 / 100;
            firmConsumptionWeekendWinter[23] = (float)11 / 100;

            return firmConsumptionWeekendWinter;
        }

        private static Dictionary<int, float> InitShoppingCenterWeekendWinter()
        {
            shoppingCenterConsumptionWeekendWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumptionWeekendWinter.Add(i, 0);
            }

            shoppingCenterConsumptionWeekendWinter[0] = (float)11 / 100;
            shoppingCenterConsumptionWeekendWinter[1] = (float)11 / 100;
            shoppingCenterConsumptionWeekendWinter[2] = (float)11 / 100;
            shoppingCenterConsumptionWeekendWinter[3] = (float)11 / 100;
            shoppingCenterConsumptionWeekendWinter[4] = (float)11 / 100;
            shoppingCenterConsumptionWeekendWinter[5] = (float)11 / 100;
            shoppingCenterConsumptionWeekendWinter[6] = (float)11 / 100;
            shoppingCenterConsumptionWeekendWinter[7] = (float)45 / 100;
            shoppingCenterConsumptionWeekendWinter[8] = (float)75 / 100;
            shoppingCenterConsumptionWeekendWinter[9] = (float)100 / 100;
            shoppingCenterConsumptionWeekendWinter[10] = (float)80 / 100;
            shoppingCenterConsumptionWeekendWinter[11] = (float)80 / 100;
            shoppingCenterConsumptionWeekendWinter[12] = (float)75 / 100;
            shoppingCenterConsumptionWeekendWinter[13] = (float)75 / 100;
            shoppingCenterConsumptionWeekendWinter[14] = (float)75 / 100;
            shoppingCenterConsumptionWeekendWinter[15] = (float)75 / 100;
            shoppingCenterConsumptionWeekendWinter[16] = (float)75 / 100;
            shoppingCenterConsumptionWeekendWinter[17] = (float)80 / 100;
            shoppingCenterConsumptionWeekendWinter[18] = (float)85 / 100;
            shoppingCenterConsumptionWeekendWinter[19] = (float)80 / 100;
            shoppingCenterConsumptionWeekendWinter[20] = (float)80 / 100;
            shoppingCenterConsumptionWeekendWinter[21] = (float)65 / 100;
            shoppingCenterConsumptionWeekendWinter[22] = (float)40 / 100;
            shoppingCenterConsumptionWeekendWinter[23] = (float)10 / 100;

            return shoppingCenterConsumptionWeekendWinter;
        }

        #endregion

        #region holiday - summer and winter

        private static Dictionary<int, float> InitHouseholdHolidaySummer()
        {
            householdConsumptionHolidaySummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumptionHolidaySummer.Add(i, 0);
            }

            householdConsumptionHolidaySummer[0] = (float)4 / 100;
            householdConsumptionHolidaySummer[1] = (float)4 / 100;
            householdConsumptionHolidaySummer[2] = (float)4 / 100;
            householdConsumptionHolidaySummer[3] = (float)4 / 100;
            householdConsumptionHolidaySummer[4] = (float)4 / 100;
            householdConsumptionHolidaySummer[5] = (float)4 / 100;
            householdConsumptionHolidaySummer[6] = (float)4 / 100;
            householdConsumptionHolidaySummer[7] = (float)5 / 100;
            householdConsumptionHolidaySummer[8] = (float)7 / 100;
            householdConsumptionHolidaySummer[9] = (float)12 / 100;
            householdConsumptionHolidaySummer[10] = (float)15 / 100;
            householdConsumptionHolidaySummer[11] = (float)15 / 100;
            householdConsumptionHolidaySummer[12] = (float)15 / 100;
            householdConsumptionHolidaySummer[13] = (float)15 / 100;
            householdConsumptionHolidaySummer[14] = (float)13 / 100;
            householdConsumptionHolidaySummer[15] = (float)12 / 100;
            householdConsumptionHolidaySummer[16] = (float)15 / 100;
            householdConsumptionHolidaySummer[17] = (float)15 / 100;
            householdConsumptionHolidaySummer[18] = (float)21 / 100;
            householdConsumptionHolidaySummer[19] = (float)25 / 100;
            householdConsumptionHolidaySummer[20] = (float)29 / 100;
            householdConsumptionHolidaySummer[21] = (float)25 / 100;
            householdConsumptionHolidaySummer[22] = (float)20 / 100;
            householdConsumptionHolidaySummer[23] = (float)10 / 100;

            return householdConsumptionHolidaySummer;
        }

        private static Dictionary<int, float> InitFirmHolidaySummer()
        {
            firmConsumptionHolidaySummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumptionHolidaySummer.Add(i, 0);
            }

            firmConsumptionHolidaySummer[0] = (float)11 / 100;
            firmConsumptionHolidaySummer[1] = (float)11 / 100;
            firmConsumptionHolidaySummer[2] = (float)11 / 100;
            firmConsumptionHolidaySummer[3] = (float)11 / 100;
            firmConsumptionHolidaySummer[4] = (float)11 / 100;
            firmConsumptionHolidaySummer[5] = (float)11 / 100;
            firmConsumptionHolidaySummer[6] = (float)11 / 100;
            firmConsumptionHolidaySummer[7] = (float)11 / 100;
            firmConsumptionHolidaySummer[8] = (float)11 / 100;
            firmConsumptionHolidaySummer[9] = (float)11 / 100;
            firmConsumptionHolidaySummer[10] = (float)11 / 100;
            firmConsumptionHolidaySummer[11] = (float)11 / 100;
            firmConsumptionHolidaySummer[12] = (float)11 / 100;
            firmConsumptionHolidaySummer[13] = (float)11 / 100;
            firmConsumptionHolidaySummer[14] = (float)11 / 100;
            firmConsumptionHolidaySummer[15] = (float)11 / 100;
            firmConsumptionHolidaySummer[16] = (float)11 / 100;
            firmConsumptionHolidaySummer[17] = (float)11 / 100;
            firmConsumptionHolidaySummer[18] = (float)11 / 100;
            firmConsumptionHolidaySummer[19] = (float)11 / 100;
            firmConsumptionHolidaySummer[20] = (float)11 / 100;
            firmConsumptionHolidaySummer[21] = (float)11 / 100;
            firmConsumptionHolidaySummer[22] = (float)11 / 100;
            firmConsumptionHolidaySummer[23] = (float)11 / 100;

            return firmConsumptionHolidaySummer;
        }

        private static Dictionary<int, float> InitShoppingCenterHolidaySummer()
        {
            shoppingCenterConsumptionHolidaySummer = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumptionHolidaySummer.Add(i, 0);
            }

            shoppingCenterConsumptionHolidaySummer[0] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[1] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[2] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[3] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[4] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[5] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[6] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[7] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[8] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[9] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[10] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[11] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[12] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[13] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[14] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[15] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[16] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[17] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[18] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[19] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[20] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[21] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[22] = (float)5 / 100;
            shoppingCenterConsumptionHolidaySummer[23] = (float)5 / 100;

            return shoppingCenterConsumptionWorkDaySummer;
        }

        private static Dictionary<int, float> InitHouseholdHolidayWinter()
        {
            householdConsumptionHolidayWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumptionHolidayWinter.Add(i, 0);
            }

            householdConsumptionHolidayWinter[0] = (float)15 / 100;
            householdConsumptionHolidayWinter[1] = (float)9 / 100;
            householdConsumptionHolidayWinter[2] = (float)9 / 100;
            householdConsumptionHolidayWinter[3] = (float)9 / 100;
            householdConsumptionHolidayWinter[4] = (float)9 / 100;
            householdConsumptionHolidayWinter[5] = (float)9 / 100;
            householdConsumptionHolidayWinter[6] = (float)11 / 100;
            householdConsumptionHolidayWinter[7] = (float)15 / 100;
            householdConsumptionHolidayWinter[8] = (float)19 / 100;
            householdConsumptionHolidayWinter[9] = (float)40 / 100;
            householdConsumptionHolidayWinter[10] = (float)50 / 100;
            householdConsumptionHolidayWinter[11] = (float)65 / 100;
            householdConsumptionHolidayWinter[12] = (float)100 / 100;
            householdConsumptionHolidayWinter[13] = (float)70 / 100;
            householdConsumptionHolidayWinter[14] = (float)70 / 100;
            householdConsumptionHolidayWinter[15] = (float)67 / 100;
            householdConsumptionHolidayWinter[16] = (float)50 / 100;
            householdConsumptionHolidayWinter[17] = (float)42 / 100;
            householdConsumptionHolidayWinter[18] = (float)38 / 100;
            householdConsumptionHolidayWinter[19] = (float)45 / 100;
            householdConsumptionHolidayWinter[20] = (float)50 / 100;
            householdConsumptionHolidayWinter[21] = (float)47 / 100;
            householdConsumptionHolidayWinter[22] = (float)33 / 100;
            householdConsumptionHolidayWinter[23] = (float)20 / 100;

            return householdConsumptionHolidayWinter;
        }

        private static Dictionary<int, float> InitFirmHolidayWinter()
        {
            firmConsumptionHolidayWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumptionHolidayWinter.Add(i, 0);
            }

            firmConsumptionHolidayWinter[0] = (float)11 / 100;
            firmConsumptionHolidayWinter[1] = (float)11 / 100;
            firmConsumptionHolidayWinter[2] = (float)11 / 100;
            firmConsumptionHolidayWinter[3] = (float)11 / 100;
            firmConsumptionHolidayWinter[4] = (float)11 / 100;
            firmConsumptionHolidayWinter[5] = (float)11 / 100;
            firmConsumptionHolidayWinter[6] = (float)11 / 100;
            firmConsumptionHolidayWinter[7] = (float)11 / 100;
            firmConsumptionHolidayWinter[8] = (float)11 / 100;
            firmConsumptionHolidayWinter[9] = (float)11 / 100;
            firmConsumptionHolidayWinter[10] = (float)11 / 100;
            firmConsumptionHolidayWinter[11] = (float)11 / 100;
            firmConsumptionHolidayWinter[12] = (float)11 / 100;
            firmConsumptionHolidayWinter[13] = (float)11 / 100;
            firmConsumptionHolidayWinter[14] = (float)11 / 100;
            firmConsumptionHolidayWinter[15] = (float)11 / 100;
            firmConsumptionHolidayWinter[16] = (float)11 / 100;
            firmConsumptionHolidayWinter[17] = (float)11 / 100;
            firmConsumptionHolidayWinter[18] = (float)11 / 100;
            firmConsumptionHolidayWinter[19] = (float)11 / 100;
            firmConsumptionHolidayWinter[20] = (float)11 / 100;
            firmConsumptionHolidayWinter[21] = (float)11 / 100;
            firmConsumptionHolidayWinter[22] = (float)11 / 100;
            firmConsumptionHolidayWinter[23] = (float)11 / 100;

            return firmConsumptionHolidayWinter;
        }

        private static Dictionary<int, float> InitShoppingCenterHolidayWinter()
        {
            shoppingCenterConsumptionHolidayWinter = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumptionHolidayWinter.Add(i, 0);
            }

            shoppingCenterConsumptionHolidayWinter[0] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[1] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[2] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[3] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[4] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[5] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[6] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[7] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[8] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[9] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[10] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[11] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[12] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[13] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[14] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[15] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[16] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[17] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[18] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[19] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[20] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[21] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[22] = (float)11 / 100;
            shoppingCenterConsumptionHolidayWinter[23] = (float)11 / 100;

            return shoppingCenterConsumptionHolidayWinter;
        }

        #endregion

        #region atypical day

        private static Dictionary<int, float> InitHouseholdAtypicalDay()
        {
            householdConsumptionAtypicalDay = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                householdConsumptionAtypicalDay.Add(i, 0);
            }

            householdConsumptionAtypicalDay[0] = (float)5 / 100;
            householdConsumptionAtypicalDay[1] = (float)5 / 100;
            householdConsumptionAtypicalDay[2] = (float)5 / 100;
            householdConsumptionAtypicalDay[3] = (float)5 / 100;
            householdConsumptionAtypicalDay[4] = (float)5 / 100;
            householdConsumptionAtypicalDay[5] = (float)7 / 100;
            householdConsumptionAtypicalDay[6] = (float)14 / 100;
            householdConsumptionAtypicalDay[7] = (float)20 / 100;
            householdConsumptionAtypicalDay[8] = (float)30 / 100;
            householdConsumptionAtypicalDay[9] = (float)38 / 100;
            householdConsumptionAtypicalDay[10] = (float)20 / 100;
            householdConsumptionAtypicalDay[11] = (float)30 / 100;
            householdConsumptionAtypicalDay[12] = (float)40 / 100;
            householdConsumptionAtypicalDay[13] = (float)37 / 100;
            householdConsumptionAtypicalDay[14] = (float)37 / 100;
            householdConsumptionAtypicalDay[15] = (float)38 / 100;
            householdConsumptionAtypicalDay[16] = (float)40 / 100;
            householdConsumptionAtypicalDay[17] = (float)80 / 100;
            householdConsumptionAtypicalDay[18] = (float)100 / 100;
            householdConsumptionAtypicalDay[19] = (float)70 / 100;
            householdConsumptionAtypicalDay[20] = (float)50 / 100;
            householdConsumptionAtypicalDay[21] = (float)52 / 100;
            householdConsumptionAtypicalDay[22] = (float)33 / 100;
            householdConsumptionAtypicalDay[23] = (float)10 / 100;

            return householdConsumptionAtypicalDay;
        }

        private static Dictionary<int, float> InitFirmAtypicalDay()
        {
            firmConsumptionAtypicalDay = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                firmConsumptionAtypicalDay.Add(i, 0);
            }

            firmConsumptionAtypicalDay[0] = (float)11 / 100;
            firmConsumptionAtypicalDay[1] = (float)11 / 100;
            firmConsumptionAtypicalDay[2] = (float)11 / 100;
            firmConsumptionAtypicalDay[3] = (float)11 / 100;
            firmConsumptionAtypicalDay[4] = (float)11 / 100;
            firmConsumptionAtypicalDay[5] = (float)11 / 100;
            firmConsumptionAtypicalDay[6] = (float)15 / 100;
            firmConsumptionAtypicalDay[7] = (float)30 / 100;
            firmConsumptionAtypicalDay[8] = (float)75 / 100;
            firmConsumptionAtypicalDay[9] = (float)100 / 100;
            firmConsumptionAtypicalDay[10] = (float)80 / 100;
            firmConsumptionAtypicalDay[11] = (float)85 / 100;
            firmConsumptionAtypicalDay[12] = (float)90 / 100;
            firmConsumptionAtypicalDay[13] = (float)90 / 100;
            firmConsumptionAtypicalDay[14] = (float)90 / 100;
            firmConsumptionAtypicalDay[15] = (float)90 / 100;
            firmConsumptionAtypicalDay[16] = (float)85 / 100;
            firmConsumptionAtypicalDay[17] = (float)65 / 100;
            firmConsumptionAtypicalDay[18] = (float)55 / 100;
            firmConsumptionAtypicalDay[19] = (float)40 / 100;
            firmConsumptionAtypicalDay[20] = (float)11 / 100;
            firmConsumptionAtypicalDay[21] = (float)11 / 100;
            firmConsumptionAtypicalDay[22] = (float)11 / 100;
            firmConsumptionAtypicalDay[23] = (float)11 / 100;

            return firmConsumptionAtypicalDay;
        }

        private static Dictionary<int, float> InitShoppingCenterAtypicalDay()
        {
            shoppingCenterConsumptionAtypicalDay = new Dictionary<int, float>();

            for (int i = 0; i < 24; i++)
            {
                shoppingCenterConsumptionAtypicalDay.Add(i, 0);
            }

            shoppingCenterConsumptionAtypicalDay[0] = (float)11 / 100;
            shoppingCenterConsumptionAtypicalDay[1] = (float)11 / 100;
            shoppingCenterConsumptionAtypicalDay[2] = (float)11 / 100;
            shoppingCenterConsumptionAtypicalDay[3] = (float)11 / 100;
            shoppingCenterConsumptionAtypicalDay[4] = (float)11 / 100;
            shoppingCenterConsumptionAtypicalDay[5] = (float)11 / 100;
            shoppingCenterConsumptionAtypicalDay[6] = (float)11 / 100;
            shoppingCenterConsumptionAtypicalDay[7] = (float)45 / 100;
            shoppingCenterConsumptionAtypicalDay[8] = (float)75 / 100;
            shoppingCenterConsumptionAtypicalDay[9] = (float)100 / 100;
            shoppingCenterConsumptionAtypicalDay[10] = (float)80 / 100;
            shoppingCenterConsumptionAtypicalDay[11] = (float)80 / 100;
            shoppingCenterConsumptionAtypicalDay[12] = (float)90 / 100;
            shoppingCenterConsumptionAtypicalDay[13] = (float)90 / 100;
            shoppingCenterConsumptionAtypicalDay[14] = (float)90 / 100;
            shoppingCenterConsumptionAtypicalDay[15] = (float)90 / 100;
            shoppingCenterConsumptionAtypicalDay[16] = (float)90 / 100;
            shoppingCenterConsumptionAtypicalDay[17] = (float)85 / 100;
            shoppingCenterConsumptionAtypicalDay[18] = (float)85 / 100;
            shoppingCenterConsumptionAtypicalDay[19] = (float)80 / 100;
            shoppingCenterConsumptionAtypicalDay[20] = (float)80 / 100;
            shoppingCenterConsumptionAtypicalDay[21] = (float)80 / 100;
            shoppingCenterConsumptionAtypicalDay[22] = (float)55 / 100;
            shoppingCenterConsumptionAtypicalDay[23] = (float)10 / 100;

            return shoppingCenterConsumptionAtypicalDay;
        }

        #endregion
    }
}
