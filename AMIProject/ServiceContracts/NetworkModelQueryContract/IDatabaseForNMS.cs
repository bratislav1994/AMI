using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    [ServiceContract]
    public interface IDatabaseForNMS
    {
        [OperationContract]
        int Connect();

        [OperationContract]
        bool SaveDelta(Delta delta);

        [OperationContract]
        List<Delta> ReadDelta();

        [OperationContract]
        void Ping();
    }
}
