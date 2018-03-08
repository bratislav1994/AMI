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
using FTN.Common.Logger;
using TC57CIM.IEC61970.Wires;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GenericDataAccess : INMSForScript
    {
        //INetworkModelGDAContract,
        private static Dictionary<int, ResourceIterator> resourceItMap = new Dictionary<int, ResourceIterator>();
        private static int resourceItId = 0;
        private NetworkModel nm = null;
        private NetworkModel nmCopy = null;
        public Delta delta;

        public GenericDataAccess()
        {

        }
        
        public NetworkModel NetworkModel
        {
            get
            {
                return nm;
            }

            set
            {
                nm = value;
                nmCopy = nm;
            }
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
            nmCopy.LockObjectClient = nm.LockObjectClient;
            nmCopy.LockObjectScada = nm.LockObjectScada;
            nmCopy.ResourcesDescs = nm.ResourcesDescs;

            Dictionary<DMSType, Container> copy = nm.DeepCopy();
            nmCopy.networkDataModel = copy;

            return nmCopy.ApplyDelta(delta);
        }

        public void Commit()
        {
            nmCopy.IsTest = nm.IsTest;
            nm = nmCopy;
            ResourceIterator.NetworkModel = nm;
            //nm.UpdateClients();
            //nm.IssueSaveDelta(delta);
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
                return (retVal);
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
                return (retVal);
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

                return (retVal);
            }
            catch (Exception ex)
            {
                string message = string.Format("Getting extent values for ModelCode = {0} failed. ", entityType, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                throw new Exception(message);
            }
        }

        public int GetRelatedValues(List<long> source, List<ModelCode> propIds, Association association)
        {
            try
            {
                ResourceIterator ri = nm.GetRelatedValues(source, propIds, association);
                int retVal = AddIterator(ri);

                return (retVal);
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

                return (retVal);
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

                return (retVal);
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

                return (resourcesLeft);
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
            return (RemoveIterator(id));
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

        public void ClearResourceMap()
        {
            resourceItMap.Clear();
        }

        public List<IdentifiedObject> GetConsumers()
        {
            return nm.GetConsumers();
        }

        public Dictionary<long, IdentifiedObject> GetVoltages()
        {
            return nm.GetVoltages();
        }
    }
}
