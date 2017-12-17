using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Security.Principal;
using System.Threading;
using FTN.Common;
using System.Collections;
using FTN.ServiceContracts;
using TC57CIM.IEC61970.Meas;
using TC57CIM.IEC61970.Core;

namespace FTN.Services.NetworkModelService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class GenericDataAccess : INetworkModelGDAContractDuplexClient, INetworkModel
	{
        //INetworkModelGDAContract,
        private static Dictionary<int, ResourceIterator> resourceItMap = new Dictionary<int, ResourceIterator>();
		private static int resourceItId = 0;
		private NetworkModel nm = null;
        private NetworkModel nmCopy = null;
        private ITransactionDuplexNMS proxyCoordinator;
        private bool firstTimeCoordinator = true;
        private Delta delta;

        public GenericDataAccess()
		{
            while (true)
            {
                try
                {
                    //Logger.LogMessageToFile(string.Format("NMS.NetworkModel; line: {0}; NMS try to connect with Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    this.ProxyCoordinator.ConnectNMS();
                    //Logger.LogMessageToFile(string.Format("NMS.NetworkModel; line: {0}; NMS is connected to the Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    break;
                }
                catch
                {
                    //Logger.LogMessageToFile(string.Format("NMS.NetworkModel; line: {0}; NMS faild to connect with Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    Thread.Sleep(1000);
                }
            }
        }

        public ITransactionDuplexNMS ProxyCoordinator
        {
            get
            {
                if (firstTimeCoordinator)
                {
                    //Logger.LogMessageToFile(string.Format("NMS.NetworkModel.ProxyCoordinator; line: {0}; Create channel between NMS and Coordinator", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.SendTimeout = TimeSpan.FromSeconds(3);
                    DuplexChannelFactory<ITransactionDuplexNMS> factory = new DuplexChannelFactory<ITransactionDuplexNMS>(
                    new InstanceContext(this),
                        binding,
                        new EndpointAddress("net.tcp://localhost:10003/TransactionCoordinator/NMS"));
                    proxyCoordinator = factory.CreateChannel();
                    firstTimeCoordinator = false;
                }
                //Logger.LogMessageToFile(string.Format("NMS.NetworkModel.ProxyCoordinator; line: {0}; Channel NMS-Coordinator is created", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                return proxyCoordinator;
            }

            set
            {
                proxyCoordinator = value;
            }
        }

        public NetworkModel NetworkModel
		{
			set
			{
				nm = value;
                nmCopy = nm;
			}
		}

        public void ConnectClient()
        {
            nm.ConnectClient();
        }

        public void EnlistDelta(Delta delta)
        {
            this.delta = delta;
        }

        public Delta Prepare()
        {
            if (!DeltaValidation())
            {
                return null;
            }

            nmCopy = new NetworkModel();
            nmCopy.Clients = nm.Clients;
            nmCopy.ClientsForDeleting = nm.ClientsForDeleting;
            nmCopy.ClientsNeedToUpdate = nm.ClientsNeedToUpdate;
            nmCopy.LockObjectClient = nm.LockObjectClient;
            nmCopy.LockObjectScada = nm.LockObjectScada;
            nmCopy.ResourcesDescs = nm.ResourcesDescs;
            nmCopy.UpdateThreadClient = nm.UpdateThreadClient;
            
            Dictionary<DMSType, Container> copy = nm.DeepCopy();
            nmCopy.networkDataModel = copy;
            
            return nmCopy.ApplyDelta(delta);
        }

        public void Commit()
        {
            nm = nmCopy;
            ResourceIterator.NetworkModel = nm;
            nm.UpdateClients();
            nm.IssueSaveDelta(delta);
        }

        public void Rollback()
        {
            nmCopy = nm;
        }

        private bool DeltaValidation()
        {
            List<string> mRIDs = new List<string>();

            foreach (ResourceDescription resDesc in delta.InsertOperations)
            {
                mRIDs.Add(resDesc.Properties.Where(x => x.Id.Equals(ModelCode.IDOBJ_MRID)).FirstOrDefault().PropertyValue.StringValue);
            }

            foreach (string mrid in mRIDs)
            {
                if (mRIDs.Where(x => x.Equals(mrid)).Count() != 1)
                {
                    return false;
                }
            }
            
            foreach (KeyValuePair<DMSType, Container> kvp in nm.networkDataModel)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvp2 in kvp.Value.Entities)
                {
                    if (mRIDs.Where(x => x.Equals(kvp2.Value.Mrid)).Count() != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			try
			{				
				ResourceDescription retVal = nm.GetValues(resourceId, propIds);				
				return retVal;
			}			
			catch (Exception ex)
			{
				string message = string.Format("Getting values for resource with ID = 0x{0:x16} failed. {1}", resourceId, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}	
		}

        public List<long> GetGlobalIds()
        {
            try
            {
                List<long> retVal = nm.GetGlobalIds();
                return retVal;
            }
            catch (Exception ex)
            {
                string message = string.Format(ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                throw new Exception(message);
            }
        }

        public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			try
			{
				ResourceIterator ri = nm.GetExtentValues(entityType, propIds);
				int retVal = AddIterator(ri);

				return retVal;
			}
			catch (Exception ex)
			{
				string message = string.Format("Getting extent values for ModelCode = {0} failed. ", entityType, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			try
			{
				ResourceIterator ri = nm.GetRelatedValues(source, propIds, association);
				int retVal = AddIterator(ri);

				return retVal;
			}
			catch (Exception ex)
			{
				string message = string.Format("Getting related values for resource with ID = 0x{0:x16} failed. {1}", source, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}					

		public List<ResourceDescription> IteratorNext(int n, int id)
		{			
			try
			{								
				List<ResourceDescription> retVal = GetIterator(id).Next(n);				

				return retVal;
			}			
			catch (Exception ex)
			{
				string message = string.Format("IteratorNext failed. Iterator ID = {0}. Resources to fetch count = {1}. {2} ", id, n, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}		
		}

		public bool IteratorRewind(int id)
		{			
			try
			{								
				GetIterator(id).Rewind();
				
				return true;
			}						
			catch (Exception ex)
			{
				string message = string.Format("IteratorRewind failed. Iterator ID = {0}. {1}", id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}				
		}

		public int IteratorResourcesTotal(int id)
		{			
			try
			{				
				int retVal = GetIterator(id).ResourcesTotal();
				return retVal;
			}			
			catch (Exception ex)
			{
				string message = string.Format("IteratorResourcesTotal failed. Iterator ID = {0}. {1}", id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}				
		}

		public int IteratorResourcesLeft(int id)
		{			
			try
			{								
				int resourcesLeft = GetIterator(id).ResourcesLeft();

				return resourcesLeft;
			}						
			catch (Exception ex)
			{
				string message = string.Format("IteratorResourcesLeft failed. Iterator ID = {0}. {1}", id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}				
		}

		public bool IteratorClose(int id)
		{			
			try
			{				
				bool retVal = RemoveIterator(id);				

				return retVal;
			}			
			catch (Exception ex)
			{
				string message = string.Format("IteratorClose failed. Iterator ID = {0}. {1}", id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}			
		}

		private int AddIterator(ResourceIterator iterator)
		{
			lock (resourceItMap)
			{
				int iteratorId = ++resourceItId;
				resourceItMap.Add(iteratorId, iterator);
				return iteratorId;
			}
		}

		private ResourceIterator GetIterator(int iteratorId)
		{
			lock (resourceItMap)
			{
				if (resourceItMap.ContainsKey(iteratorId))
				{
					return resourceItMap[iteratorId];
				}
				else
				{
					throw new Exception(string.Format("Iterator with given ID = {0} doesn't exist.", iteratorId));					
				}
			}
		}

		private bool RemoveIterator(int iteratorId)
		{
			lock (resourceItMap)
			{
				return resourceItMap.Remove(iteratorId);
			}
		}

        public void Ping()
        {
            return ;
        }
    }
}
