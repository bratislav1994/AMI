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

        #region summer

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

        #region winter

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
    }
}
