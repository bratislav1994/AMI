using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.Dynamic
{
    [DataContract]
    public class Statistics
    {
        private float maxP;
        private float minP;
        private float maxQ;
        private float minQ;
        private float maxV;
        private float minV;
        private float avgP;
        private float avgQ;
        private float avgV;
        private float integralP;
        private float integralQ;
        private float integralV;

        public Statistics() { }

        [DataMember]
        public float MaxP
        {
            get
            {
                return maxP;
            }

            set
            {
                maxP = value;
            }
        }

        [DataMember]
        public float MinP
        {
            get
            {
                return minP;
            }

            set
            {
                minP = value;
            }
        }

        [DataMember]
        public float MaxQ
        {
            get
            {
                return maxQ;
            }

            set
            {
                maxQ = value;
            }
        }

        [DataMember]
        public float MinQ
        {
            get
            {
                return minQ;
            }

            set
            {
                minQ = value;
            }
        }

        [DataMember]
        public float MaxV
        {
            get
            {
                return maxV;
            }

            set
            {
                maxV = value;
            }
        }

        [DataMember]
        public float MinV
        {
            get
            {
                return minV;
            }

            set
            {
                minV = value;
            }
        }

        [DataMember]
        public float AvgP
        {
            get
            {
                return avgP;
            }

            set
            {
                avgP = value;
            }
        }

        [DataMember]
        public float AvgQ
        {
            get
            {
                return avgQ;
            }

            set
            {
                avgQ = value;
            }
        }

        [DataMember]
        public float AvgV
        {
            get
            {
                return avgV;
            }

            set
            {
                avgV = value;
            }
        }

        [DataMember]
        public float IntegralP
        {
            get
            {
                return integralP;
            }

            set
            {
                integralP = value;
            }
        }

        [DataMember]
        public float IntegralQ
        {
            get
            {
                return integralQ;
            }

            set
            {
                integralQ = value;
            }
        }

        [DataMember]
        public float IntegralV
        {
            get
            {
                return integralV;
            }

            set
            {
                integralV = value;
            }
        }
    }
}
