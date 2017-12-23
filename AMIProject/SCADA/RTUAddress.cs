using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
    public class RTUAddress
    {
        private bool isConnected = false;
        private int cnt = 0;

        public RTUAddress()
        {

        }

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                isConnected = value;
            }
        }

        public int Cnt
        {
            get
            {
                return cnt;
            }

            set
            {
                cnt = value;
            }
        }
    }
}
