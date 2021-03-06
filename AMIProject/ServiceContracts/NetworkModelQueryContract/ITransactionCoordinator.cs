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
    public interface ITransactionCoordinator
    {
        [OperationContract]
        string ApplyDelta(Delta delta);

        [OperationContract]
        void Connect(ServiceInfo serviceInfo);

        [OperationContract]
        void Ping();
    }
}
