using FTN.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel
{
    [DataContract]
    public class DynamicMeasurement
    {
        private int idDB;
        private long psrRef;
        private EnergyConsumerDb psr;
        private float currentP;
        private float currentQ;
        private float currentV;
        private DateTime timeStamp;
        private bool isAlarm;
        private TypeVoltage typeVoltage;

        public DynamicMeasurement()
        {

        }

        public DynamicMeasurement(long psr)
        {
            this.PsrRef = psr;
            this.isAlarm = false;
            this.currentP = 0;
            this.currentQ = 0;
            this.currentV = 0;
            this.typeVoltage = TypeVoltage.INBOUNDS;
        }

        public DynamicMeasurement(long psrRef, DateTime timeStamp)
        {
            this.psrRef = psrRef;
            this.timeStamp = timeStamp;
            this.currentP = -1;
            this.currentQ = -1;
            this.currentV = -1;
            this.typeVoltage = TypeVoltage.INBOUNDS;
        }

        [DataMember]
        [ForeignKey("Psr")]
        public long PsrRef
        {
            get
            {
                return psrRef;
            }

            set
            {
                psrRef = value;
            }
        }

        [DataMember]
        public float CurrentP
        {
            get
            {
                return currentP;
            }

            set
            {
                currentP = value;
            }
        }

        [DataMember]
        public float CurrentQ
        {
            get
            {
                return currentQ;
            }

            set
            {
                currentQ = value;
            }
        }

        [DataMember]
        public float CurrentV
        {
            get
            {
                return currentV;
            }

            set
            {
                currentV = value;
            }
        }

        [DataMember]
        public DateTime TimeStamp
        {
            get
            {
                return timeStamp;
            }

            set
            {
                timeStamp = value;
            }
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
        
        public EnergyConsumerDb Psr
        {
            get
            {
                return psr;
            }

            set
            {
                psr = value;
            }
        }

        [DataMember]
        public bool IsAlarm
        {
            get
            {
                return isAlarm;
            }
            set
            {
                isAlarm = value;
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
