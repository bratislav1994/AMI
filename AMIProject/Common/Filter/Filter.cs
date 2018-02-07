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
        private TypeOfDay typeOfDay;
        private ConsumerType consumerType;
        private bool seasonHasValue;
        private bool typeOfDayHasValue;
        private bool consumerHasValue;

        public Filter()
        {

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
        public TypeOfDay TypeOfDay
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
    }
}
