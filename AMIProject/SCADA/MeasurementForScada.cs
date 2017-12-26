using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
    public class MeasurementForScada
    {
        private int index;
        private ResourceDescription measurement;

        public MeasurementForScada()
        {

        }

        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
            }
        }

        public ResourceDescription Measurement
        {
            get
            {
                return measurement;
            }

            set
            {
                measurement = value;
            }
        }
    }
}
