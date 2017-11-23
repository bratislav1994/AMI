using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClient.ClassesForTable
{
    public class GeoRegionForTable
    {
        private string mRID;
        private string name;

        public GeoRegionForTable()
        {

        }

        public string MRID
        {
            get
            {
                return mRID;
            }

            set
            {
                mRID = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
    }
}
