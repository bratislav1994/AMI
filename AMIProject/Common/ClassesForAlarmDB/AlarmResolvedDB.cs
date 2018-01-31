using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common.ClassesForAlarmDB
{
    [DataContract]
    public class AlarmResolvedDB
    {
        private int idDB;
        private long id;
        private Status status;
        private DateTime fromPeriod;
        private DateTime toPeriod;
        private TypeVoltage typeVoltage;
        private float voltage;

        public AlarmResolvedDB()
        {
        }

        [IgnoreDataMember]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDB
        {
            get
            {
                return idDB;
            }

            set
            {
                idDB = value;
            }
        }

        [DataMember]
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
        public DateTime ToPeriod
        {
            get
            {
                return toPeriod;
            }

            set
            {
                toPeriod = value;
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
