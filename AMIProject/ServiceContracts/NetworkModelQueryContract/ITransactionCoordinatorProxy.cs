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
    public interface ITransactionCoordinatorProxy
    {
        [OperationContract]

        void EnlistDeltaNMS(Delta delta);

        [OperationContract]
        void EnlistMeasScada(List<ResourceDescription> measurements);

        [OperationContract]
        void EnlistDeltaCE(List<ResourceDescription> data);

        [OperationContract]
        Delta PrepareNMS();

        [OperationContract]
        bool PrepareCE();

        [OperationContract]
        bool PrepareScada();

        [OperationContract]
        void CommitNMS();

        [OperationContract]
        void CommitCE();

        [OperationContract]
        void CommitScada();

        [OperationContract]
        void RollbackNMS();

        [OperationContract]
        void RollbackCE();

        [OperationContract]
        void RollbackScada();
    }
}
