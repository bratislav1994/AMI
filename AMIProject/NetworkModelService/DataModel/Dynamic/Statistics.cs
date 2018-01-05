using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.Dynamic
{
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
