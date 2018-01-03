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

        private float currentP;

        private float currentQ;

        private float currentV;

        private DateTime timeStamp;

        private OperationType operationType;

        public DynamicMeasurement()
        {

        }

        public DynamicMeasurement(long psrRef)
        {
            this.psrRef = psrRef;
            this.timeStamp = DateTime.Now;
            this.currentP = -1;
            this.currentQ = -1;
            this.currentV = -1;
        }

        [DataMember]
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
        public OperationType OperationType
        {
            get
            {
                return operationType;
            }

            set
            {
                operationType = value;
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
    }
}
