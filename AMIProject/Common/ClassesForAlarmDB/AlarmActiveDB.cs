using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common.ClassesForAlarmDB
{
    [DataContract]
    public class AlarmActiveDB
    {
        private long id;
        private Status status;
        private DateTime fromPeriod;
        private TypeVoltage typeVoltage;
        private float voltage;

        public AlarmActiveDB()
        {
        }

        [DataMember]
        [Key]
        public long Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
   //             RaisePropertyChanged("Id");
            }
        }

        [DataMember]
        public Status Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
     //           RaisePropertyChanged("Status");
            }
        }

        [DataMember]
        public DateTime FromPeriod
        {
            get
            {
                return fromPeriod;
            }

            set
            {
                fromPeriod = value;
   //             RaisePropertyChanged("FromPeriod");
            }
        }

        [DataMember]
        public TypeVoltage TypeVoltage
        {
            get
            {
                return typeVoltage;
            }

            set
            {
                typeVoltage = value;
     //           RaisePropertyChanged("Type");
            }
        }

        [DataMember]
        public float Voltage
        {
            get
            {
                return voltage;
            }

            set
            {
                voltage = value;
            }
        }
    }
}
