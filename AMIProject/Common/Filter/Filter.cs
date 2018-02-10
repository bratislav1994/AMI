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
