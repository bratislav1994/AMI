using FTN.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTN.Common;
using System.ServiceModel;
using FTN.Services.NetworkModelService;
using FTN.Common.Logger;

namespace TransactionCoordinator
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TransactionCoordinator : ITransactionCoordinator, ITransactionDuplexNMS, ITransactionDuplexScada
    {
        bool firstTimeNMS = true;
        private List<IScada> scadas;
        private List<IScada> scadasForDeleting;
        private INetworkModel proxyNMS;
        private static TransactionCoordinator instance;
        private Delta delta;

        public TransactionCoordinator()
        {
            scadas = new List<IScada>();
            scadasForDeleting = new List<IScada>();
        }

        public static TransactionCoordinator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TransactionCoordinator();
                }

                return instance;
            }
        }
        
        public void ConnectScada()
        {
            this.scadas.Add(OperationContext.Current.GetCallbackChannel<IScada>());
        }

        public void ConnectNMS()
        {
            this.proxyNMS = OperationContext.Current.GetCallbackChannel<INetworkModel>();
        }

        public bool ApplyDelta(Delta delta)
        {
            Logger.LogMessageToFile(string.Format("TranscactionCoordinator.TranscactionCoordinator.ApplyDelta; line: {0}; Coordinator sends data to NMS and SCADA", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ResourceDescription> measurements = new List<ResourceDescription>();

            foreach (ResourceDescription rd in delta.InsertOperations)
            {
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);
                if (type == DMSType.ANALOG)
                {
                    measurements.Add(rd);
                }
            }

            foreach (IScada scada in scadas)
            {
                scada.EnlistMeas(measurements);
            }

            proxyNMS.EnlistDelta(delta);

            List<bool> list = new List<bool>(scadas.Count);
            foreach (IScada scada in scadas)
            {
                list.Add(scada.Prepare());
            }

            if (list.All(x => x == true) && proxyNMS.Prepare())
            {
                foreach (IScada scada in scadas)
                {
                    scada.Commit();
                }

                proxyNMS.Commit();
                Logger.LogMessageToFile(string.Format("TranscactionCoordinator.TranscactionCoordinator.ApplyDelta; line: {0}; Data is successfully sent", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return true;
            }
            else
            {
                foreach (IScada scada in scadas)
                {
                    scada.Rollback();
                }

                proxyNMS.Rollback();
                Logger.LogMessageToFile(string.Format("TranscactionCoordinator.TranscactionCoordinator.ApplyDelta; line: {0}; Data failed to send successfully", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return false;
            }
        }
    }
}
