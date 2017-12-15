using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel
{
    [DataContract]
    public class DynamicMeasurement
    {
        private long psrRef;
        private float currentP;
        private float currentQ;
        private float currentV;

        public DynamicMeasurement(long psrRef)
        {
            this.psrRef = psrRef;
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
    }
}
