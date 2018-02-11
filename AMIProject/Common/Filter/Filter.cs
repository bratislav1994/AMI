using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common.Filter
{
    [DataContract]
    public class Filter
    {
        private Season season;
        private List<DayOfWeek> typeOfDay;
        private ConsumerType consumerType;
        private bool seasonHasValue;
        private bool typeOfDayHasValue;
        private bool consumerHasValue;
        private bool specificDayHasValue;
        private int yearTo;
        private int yearFrom;
        private int month;
        private int day;

        public Filter()
        {
            this.typeOfDay = new List<DayOfWeek>();
        }

        [DataMember]
        public Season Season
        {
            get
            {
                return season;
            }

            set
            {
                season = value;
            }
        }

        [DataMember]
        public List<DayOfWeek> TypeOfDay
        {
            get
            {
                return typeOfDay;
            }

            set
            {
                typeOfDay = value;
            }
        }

        [DataMember]
        public ConsumerType ConsumerType
        {
            get
            {
                return consumerType;
            }

            set
            {
                consumerType = value;
            }
        }

        [DataMember]
        public bool SeasonHasValue
        {
            get
            {
                return seasonHasValue;
            }

            set
            {
                seasonHasValue = value;
            }
        }

        [DataMember]
        public bool TypeOfDayHasValue
        {
            get
            {
                return typeOfDayHasValue;
            }

            set
            {
                typeOfDayHasValue = value;
            }
        }

        [DataMember]
        public bool ConsumerHasValue
        {
            get
            {
                return consumerHasValue;
            }

            set
            {
                consumerHasValue = value;
            }
        }

        [DataMember]
        public bool SpecificDayHasValue
        {
            get
            {
                return specificDayHasValue;
            }

            set
            {
                specificDayHasValue = value;
                if (!value)
                {
                    this.Day = -1;
                    this.Month = -1;
                    this.YearFrom = 1;
                    this.YearTo = DateTime.Now.Year;
                }
            }
        }

        [DataMember]
        public int Day
        {
            get
            {
                return day;
            }

            set
            {
                day = value;
            }
        }

        [DataMember]
        public int Month
        {
            get
            {
                return month;
            }

            set
            {
                month = value;
            }
        }
        
        [DataMember]
        public int YearFrom
        {
            get
            {
                return yearFrom;
            }

            set
            {
                yearFrom = value;
            }
        }

        [DataMember]
        public int YearTo
        {
            get
            {
                return yearTo;
            }

            set
            {
                yearTo = value;
            }
        }
    }
}
