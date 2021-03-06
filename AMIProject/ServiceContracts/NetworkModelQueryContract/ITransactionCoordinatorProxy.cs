﻿using FTN.Common;
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
        bool EnlistMeasScada(List<ResourceDescription> measurements);
        

        [OperationContract]
        bool PrepareScada();

        [OperationContract]
        void CommitScada();

        [OperationContract]
        void RollbackScada();
    }
}
