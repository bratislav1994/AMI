using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
    public class ClassForSerialization
    {
        private int key;
        private ResourceDescription rd;

        public ClassForSerialization()
        {

        }

        public ClassForSerialization(int key, ResourceDescription rd)
        {
            this.key = key;
            this.rd = rd;
        }

        public int Key
        {
            get
            {
                return key;
            }

            set
            {
                key = value;
            }
        }

        public ResourceDescription Rd
        {
            get
            {
                return rd;
            }

            set
            {
                rd = value;
            }
        }
    }
}
