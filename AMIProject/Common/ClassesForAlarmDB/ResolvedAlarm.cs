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
    public class ResolvedAlarm
    {
        private int idDB;
        private long id;
        private Status status;
        private DateTime fromPeriod;
        private DateTime toPeriod;
        private TypeVoltage typeVoltage;

        public ResolvedAlarm()
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
            }
        }
    }
}
