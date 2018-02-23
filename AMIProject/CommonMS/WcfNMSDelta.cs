﻿using FTN.ServiceContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonMS
{
    public class WcfNMSDelta : ServicePartitionClient<WcfCommunicationClient<INetworkModel>>
    {
        public WcfNMSDelta(ICommunicationClientFactory<WcfCommunicationClient<INetworkModel>> communicationClientFactory, Uri serviceUri, ServicePartitionKey partitionKey = null, string listenerName = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, OperationRetrySettings retrySettings = null)
            : base(communicationClientFactory, serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings)
        {
        }
    }
}