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
    public class ActiveAlarm
    {
        private int idDB;
        private long id;
        private string consumer;
        private DateTime fromPeriod;
        private TypeVoltage typeVoltage;
        private float voltage;

        public ActiveAlarm()
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

        [DataMember]
        public string Consumer
        {
            get
            {
                return consumer;
            }

            set
            {
                consumer = value;
            }
        }
    }
}
