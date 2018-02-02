using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common.ClassesForAlarmDB
{
    public class DeltaForAlarm
    {
        private List<ActiveAlarm> insertOperations;
        private List<long> deleteOperations;

        public DeltaForAlarm()
        {
            insertOperations = new List<ActiveAlarm>();
            deleteOperations = new List<long>();
        }

        public List<ActiveAlarm> InsertOperations
        {
            get
            {
                return insertOperations;
            }

            set
            {
                insertOperations = value;
            }
        }

        public List<long> DeleteOperations
        {
            get
            {
                return deleteOperations;
            }

            set
            {
                deleteOperations = value;
            }
        }
    }
}
