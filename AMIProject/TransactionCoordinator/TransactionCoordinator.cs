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
    public class TransactionCoordinator : ITransactionCoordinator, ITransactionDuplexNMS, ITransactionDuplexScada, ITransactionDuplexCE
    {
        private List<IScada> scadas;
        private List<IScada> scadasForDeleting;
        private INetworkModel proxyNMS;
        private ICalculationEngine proxyCE;
        private static TransactionCoordinator instance;

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

        public void ConnectCE()
        {
            this.proxyCE = OperationContext.Current.GetCallbackChannel<ICalculationEngine>();
        }

        public bool ApplyDelta(Delta delta)
        {
            Logger.LogMessageToFile(string.Format("TranscactionCoordinator.TranscactionCoordinator.ApplyDelta; line: {0}; Coordinator sends data to NMS and SCADA", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
            List<ResourceDescription> dataForScada = new List<ResourceDescription>();
            List<ResourceDescription> dataForCE = new List<ResourceDescription>();
            Delta newDelta = null;
            bool CalculationEnginePrepareSuccess = false;

            try
            {
                proxyNMS.EnlistDelta(delta);
                newDelta = proxyNMS.Prepare();
            }
            catch
            {
                proxyNMS = null;
                return false;
            }

            if (newDelta != null)
            {
                foreach (ResourceDescription rd in newDelta.InsertOperations)
                {
                    DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(rd.Id);
                    if (type == DMSType.ANALOG || type == DMSType.ENERGYCONS)
                    {
                        dataForScada.Add(rd);
                    }
                    if (type == DMSType.ENERGYCONS || type == DMSType.GEOREGION || type == DMSType.SUBGEOREGION || 
                            type == DMSType.SUBSTATION)
                    {
                        dataForCE.Add(rd);
                    }
                }

                foreach (IScada scada in scadas)
                {
                    try
                    {
                        scada.EnlistMeas(dataForScada);
                    }
                    catch
                    {
                        scadasForDeleting.Add(scada);
                    }
                }

                if (scadasForDeleting.Count > 0)
                {
                    scadasForDeleting.ForEach(s => scadas.Remove(s));
                    return false;
                }

                try
                {
                    proxyCE.EnlistMeas(dataForCE);
                    CalculationEnginePrepareSuccess = proxyCE.Prepare();
                }
                catch
                {
                    proxyCE = null;
                    return false;
                }

                List<bool> list = new List<bool>(scadas.Count);
                foreach (IScada scada in scadas)
                {
                    try
                    {
                        list.Add(scada.Prepare());
                    }
                    catch
                    {
                        scadasForDeleting.Add(scada);
                    }
                }

                if (scadasForDeleting.Count > 0)
                {
                    scadasForDeleting.ForEach(s => scadas.Remove(s));
                    return false;
                }

                if (list.All(x => x == true) && CalculationEnginePrepareSuccess)
                {
                    foreach (IScada scada in scadas)
                    {
                        try
                        {
                            scada.Commit();
                        }
                        catch
                        {
                            scadasForDeleting.Add(scada);
                        }
                    }

                    if (scadasForDeleting.Count > 0)
                    {
                        scadasForDeleting.ForEach(s => scadas.Remove(s));
                        return false;
                    }

                    try
                    {
                        proxyNMS.Commit();
                    }
                    catch
                    {
                        proxyNMS = null;
                        return false;
                    }

                    try
                    {
                        proxyCE.Commit();
                    }
                    catch
                    {
                        proxyCE = null;
                        return false;
                    }
                    
                    Logger.LogMessageToFile(string.Format("TranscactionCoordinator.TranscactionCoordinator.ApplyDelta; line: {0}; Data is successfully sent", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    return true;
                }
                else
                {
                    try
                    {
                        proxyNMS.Rollback();
                    }
                    catch
                    {
                        proxyNMS = null;
                    }

                    try
                    {
                        proxyCE.Rollback();
                    }
                    catch
                    {
                        proxyCE = null;
                    }

                    foreach (IScada scada in scadas)
                    {
                        try
                        {
                            scada.Rollback();
                        }
                        catch
                        {
                            scadasForDeleting.Add(scada);
                        }
                    }

                    if (scadasForDeleting.Count > 0)
                    {
                        scadasForDeleting.ForEach(s => scadas.Remove(s));
                    }

                    Logger.LogMessageToFile(string.Format("TranscactionCoordinator.TranscactionCoordinator.ApplyDelta; line: {0}; Data failed to send successfully", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    return false;
                }
            }
            else
            {
                try
                {
                    proxyNMS.Rollback();
                }
                catch
                {
                    proxyNMS = null;
                }
                
                return false;
            }
        }
    }
}
