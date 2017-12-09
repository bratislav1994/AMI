using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ServiceContracts
{
    [ServiceContract]
    public interface IBaseTransaction
    {
        [OperationContract]
        bool Prepare();

        [OperationContract]
        void Commit();

        [OperationContract]
        void Rollback();
    }
}
