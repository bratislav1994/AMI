using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using FTN.Common;

namespace FTN.Services.NetworkModelService
{	
	public class ResourceIterator
	{
        private static NetworkModel networkModel = null;
		private List<long> globalDs = new List<long>();
		private Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>(); 	
		
		private int lastReadIndex = 0; // index of the last read resource description
		private int maxReturnNo = 5000;
        
        public static NetworkModel NetworkModel
        {
            get
            {
                return networkModel;
            }

            set { networkModel = value; }
        }

		public ResourceIterator()
		{			
		}

		public ResourceIterator(List<long> globalIDs, Dictionary<DMSType, List<ModelCode>> class2PropertyIDs)
		{
			this.globalDs = globalIDs;
			this.class2PropertyIDs = class2PropertyIDs;
		}
		
		public int ResourcesLeft()
		{
			return globalDs.Count - lastReadIndex;
		}

		public int ResourcesTotal()
		{
			return globalDs.Count;
		}

		public List<ResourceDescription> Next(int n)
		{
			try
			{
				if (n < 0)
				{
					return null;
				}

				if (n > maxReturnNo)
				{
					n = maxReturnNo;
				}

				List<long> resultIDs;

				if (ResourcesLeft() < n)
				{
					resultIDs = globalDs.GetRange(lastReadIndex, globalDs.Count - lastReadIndex);
					lastReadIndex = globalDs.Count;
				}
				else
				{
					resultIDs = globalDs.GetRange(lastReadIndex, n);
					lastReadIndex += n;
				}

				List<ResourceDescription> result = CollectData(resultIDs);

				return result;
			}
			catch (Exception ex)
			{
				string message = string.Format("Failed to get next set of ResourceDescription iterators. {0}", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);                
                throw new Exception(message);
			}
		}
        
		public void Rewind()
		{
			lastReadIndex = 0;
		}

		private List<ResourceDescription> CollectData(List<long> resultIDs)
		{
			try
			{
				List<ResourceDescription> result = new List<ResourceDescription>();

                List<ModelCode> propertyIds = null;
                foreach (long globalId in resultIDs)
                {
                    propertyIds = class2PropertyIDs[(DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId)];
                    result.Add(networkModel.GetValues(globalId, propertyIds));
                }

				return result;
			}
			catch (Exception ex)
			{
                CommonTrace.WriteTrace(CommonTrace.TraceError, "Collecting ResourceDescriptions failed. Exception: " + ex.Message);
				throw;
			}
		}
	}
}
