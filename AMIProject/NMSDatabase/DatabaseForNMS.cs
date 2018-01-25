using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using FTN.Common;
using NMSDatabase.Access;

namespace NMSDatabase
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class DatabaseForNMS : IDatabaseForNMS
    {
        private FunctionDB dataBaseAdapter = new FunctionDB();

        public DatabaseForNMS()
        {

        }

        public int Connect()
        {
            return 1;
        }

        public List<Delta> ReadDelta()
        {
            return dataBaseAdapter.ReadDelta();
        }

        public bool SaveDelta(Delta delta)
        {
            return dataBaseAdapter.AddDelta(delta);
        }

        public int GetDeltaId()
        {
            return dataBaseAdapter.GetDeltaId();
        }

        public void Ping()
        {
            return;
        }
    }
}
