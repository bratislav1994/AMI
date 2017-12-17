﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using FTN.Common;
using TC57CIM.IEC61970.Core;
using FTN.ServiceContracts;
using System.ServiceModel;
using System.Threading;
using TC57CIM.IEC61970.Meas;
using FTN.Common.Logger;
using TC57CIM.IEC61970.Wires;

namespace FTN.Services.NetworkModelService
{
    public class NetworkModel
    {
        /// <summary>
        /// Dictionaru which contains all data: Key - DMSType, Value - Container
        /// </summary>
        public Dictionary<DMSType, Container> networkDataModel;
        //private Dictionary<DMSType, Container> copy;

        /// <summary>
        /// ModelResourceDesc class contains metadata of the model
        /// </summary>
        private ModelResourcesDesc resourcesDescs;

        private List<IModelForDuplex> clients;
        private List<IModelForDuplex> clientsForDeleting;
        private Thread updateThreadClient;
        private bool clientsNeedToUpdate = false;
        private object lockObjectClient;
        private object lockObjectScada;

        public ModelResourcesDesc ResourcesDescs
        {
            get
            {
                return resourcesDescs;
            }

            set
            {
                resourcesDescs = value;
            }
        }

        public List<IModelForDuplex> Clients
        {
            get
            {
                return clients;
            }

            set
            {
                clients = value;
            }
        }

        public List<IModelForDuplex> ClientsForDeleting
        {
            get
            {
                return clientsForDeleting;
            }

            set
            {
                clientsForDeleting = value;
            }
        }

        public Thread UpdateThreadClient
        {
            get
            {
                return updateThreadClient;
            }

            set
            {
                updateThreadClient = value;
            }
        }

        public bool ClientsNeedToUpdate
        {
            get
            {
                return clientsNeedToUpdate;
            }

            set
            {
                clientsNeedToUpdate = value;
            }
        }

        public object LockObjectClient
        {
            get
            {
                return lockObjectClient;
            }

            set
            {
                lockObjectClient = value;
            }
        }

        public object LockObjectScada
        {
            get
            {
                return lockObjectScada;
            }

            set
            {
                lockObjectScada = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Model class.
        /// </summary>
        public NetworkModel()
        {
            LockObjectClient = new object();
            LockObjectScada = new object();
            networkDataModel = new Dictionary<DMSType, Container>();
            ResourcesDescs = new ModelResourcesDesc();
            Clients = new List<IModelForDuplex>();
            ClientsForDeleting = new List<IModelForDuplex>();
            UpdateThreadClient = new Thread(() => InformClients());
            UpdateThreadClient.Start();
            Initialize();
        }

        public void ConnectClient()
        {
            this.Clients.Add(OperationContext.Current.GetCallbackChannel<IModelForDuplex>());
        }
        
        #region Find
        
		public bool EntityExists(long globalId)
		{
			DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

		    if (ContainerExists(type))
		    {
				Container container = GetContainer(type);

				if (container.EntityExists(globalId))
				{
					return true;
				}
		    }

		    return false;
		}

        public IdentifiedObject GetEntity(long globalId)
		{
			if (EntityExists(globalId))
			{
				DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
				IdentifiedObject io = GetContainer(type).GetEntity(globalId);

				return io;
			}
			else
			{
				string message = string.Format("Entity  (GID = 0x{0:x16}) does not exist.", globalId);
				throw new Exception(message);
			}
		}
        
        /// <summary>
        /// Checks if container exists in model.
        /// </summary>
        /// <param name="type">Type of container.</param>
        /// <returns>True if container exists, otherwise FALSE.</returns>
        private bool ContainerExists(DMSType type)
		{
			if (networkDataModel.ContainsKey(type))
			{
				return true;
			}
			
			return false;			
		}

        /// <summary>
        /// Gets container of specified type.
        /// </summary>
        /// <param name="type">Type of container.</param>
        /// <returns>Container for specified local id</returns>
        private Container GetContainer(DMSType type)
		{
			if (ContainerExists(type))
			{
				return networkDataModel[type];
			}
			else
			{
				string message = string.Format("Container does not exist for type {0}.", type);
				throw new Exception(message);
			}
			
		}

        #endregion Find

        #region GDA query

        /// <summary>
        /// Gets resource description for entity requested by globalId.
        /// </summary>
        /// <param name="globalId">Id of the entity</param>
        /// <param name="properties">List of requested properties</param>		
        /// <returns>Resource description of the specified entity</returns>
        public ResourceDescription GetValues(long globalId, List<ModelCode> properties)
		{
			CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting values for GID = 0x{0:x16}.", globalId));

			try
			{
				IdentifiedObject io = GetEntity(globalId);

				ResourceDescription rd = new ResourceDescription(globalId);

				Property property = null;

				// insert specified properties
				foreach (ModelCode propId in properties)
				{
					property = new Property(propId);
					io.GetProperty(property);
					rd.AddProperty(property);
				}

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting values for GID = 0x{0:x16} succedded.", globalId));

				return rd;
			}			
			catch (Exception ex)
			{
				string message = string.Format("Failed to get values for entity with GID = 0x{0:x16}. {1}", globalId, ex.Message);				
				throw new Exception(message);
			}
		}

        public List<long> GetGlobalIds()
        {
            List<long> globalIds = new List<long>();

            foreach (DMSType type in Enum.GetValues(typeof(DMSType)))
            {
                if (ContainerExists(type))
                {
                    Container container = GetContainer(type);
                    List<long> list = container.GetEntitiesGlobalIds();
                    if (list != null)
                    {
                        foreach (long id in list)
                        {
                            globalIds.Add(id);
                        }
                    }
                }
            }

            return globalIds;
        }

		/// <summary>
		/// Gets resource iterator that holds descriptions for all entities of the specified type.
		/// </summary>		
		/// <param name="type">Type of entity that is requested</param>
		/// <param name="properties">List of requested properties</param>		
		/// <returns>Resource iterator for the requested entities</returns>
		public ResourceIterator GetExtentValues(ModelCode entityType, List<ModelCode> properties)
		{			
			CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Getting extent values for entity type = {0} .", entityType);

			try
			{
				List<long> globalIds = new List<long>();
				Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();
		
				DMSType entityDmsType = ModelCodeHelper.GetTypeFromModelCode(entityType);
				
				if (ContainerExists(entityDmsType))
				{
					Container container = GetContainer(entityDmsType);
					globalIds = container.GetEntitiesGlobalIds();					
					class2PropertyIDs.Add(entityDmsType, properties);					
				}

				ResourceIterator ri = new ResourceIterator(globalIds, class2PropertyIDs);

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Getting extent values for entity type = {0} succedded.", entityType);

				return ri;
			}		
			catch (Exception ex)
			{
				string message = string.Format("Failed to get extent values for entity type = {0}. {1}", entityType, ex.Message);				
				throw new Exception(message);
			}			
		}
		
		/// <summary>
		/// Gets resource iterator that holds descriptions for all entities related to specified source.
		/// </summary>
		/// <param name="contextId">Context Id</param>
		/// <param name="properties">List of requested properties</param>
		/// <param name="association">Relation between source and entities that should be returned</param>
		/// <param name="source">Id of entity that is start for association search</param>
		/// <param name="typeOfQuery">Query type choice(global or local)</param>
		/// <returns>Resource iterator for the requested entities</returns>
		public ResourceIterator GetRelatedValues(long source, List<ModelCode> properties, Association association)
		{
			CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting related values for source = 0x{0:x16}.", source));
		
			try
			{
				List<long> relatedGids = ApplyAssocioationOnSource(source, association);
			
				
				Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();

				foreach (long relatedGid in relatedGids)
				{
					DMSType entityDmsType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(relatedGid);

					if (!class2PropertyIDs.ContainsKey(entityDmsType))
					{
						class2PropertyIDs.Add(entityDmsType, properties);
					}
				}

				ResourceIterator ri = new ResourceIterator(relatedGids, class2PropertyIDs);

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, String.Format("Getting related values for source = 0x{0:x16} succeeded.", source));

				return ri;
			}
			catch (Exception ex)
			{
				string message = String.Format("Failed to get related values for source GID = 0x{0:x16}. {1}.", source, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

        #endregion GDA query	

        public Delta ApplyDelta(Delta delta)
		{
            try
            {
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Applying  delta to network model.");

                Dictionary<short, int> typesCounters = GetCounters();
                Dictionary<long, long> globalIdPairs = new Dictionary<long, long>();
                delta.FixNegativeToPositiveIds(ref typesCounters, ref globalIdPairs);
                delta.SortOperations();

                foreach (ResourceDescription rd in delta.InsertOperations)
                {
                    InsertEntity(rd);
                }

                foreach (ResourceDescription rd in delta.UpdateOperations)
                {
                    UpdateEntity(rd);
                }

                foreach (ResourceDescription rd in delta.DeleteOperations)
                {
                    DeleteEntity(rd);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Applying delta to network model failed. {0}.", ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            }

            return delta;
        }

        public void IssueSaveDelta(Delta delta)
        {
            SaveDelta(delta);
        }
        
        /// <summary>
        /// Inserts entity into the network model.
        /// </summary>
        /// <param name="rd">Description of the resource that should be inserted</param>        
        private void InsertEntity(ResourceDescription rd)
        {
            if (rd == null)
            {
                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Insert entity is not done because update operation is empty.");
                return;
            }

            long globalId = rd.Id;

            CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Inserting entity with GID ({0:x16}).", globalId);

            // check if mapping for specified global id already exists			
            if (this.EntityExists(globalId))
            {
                string message = String.Format("Failed to insert entity because entity with specified GID ({0:x16}) already exists in network model.", globalId);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                throw new Exception(message);
            }

            try
            {
                // find type
                DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);

                Container container = null;

                // get container or create container 
                if (ContainerExists(type))
                {
                    container = GetContainer(type);
                }
                else
                {
                    container = new Container();
                    networkDataModel.Add(type, container);
                }

                // create entity and add it to container
                IdentifiedObject io = container.CreateEntity(globalId);

                // apply properties on created entity
                if (rd.Properties != null)
                {
                    foreach (Property property in rd.Properties)
                    {
                        // globalId must not be set as property
                        if (property.Id == ModelCode.IDOBJ_GID)
                        {
                            continue;
                        }

                        if (property.Type == PropertyType.Reference)
                        {
                            // if property is a reference to another entity 
                            long targetGlobalId = property.AsReference();

                            if (targetGlobalId != 0)
                            {

                                if (!EntityExists(targetGlobalId))
                                {
                                    string message = string.Format("Failed to get target entity with GID: 0x{0:X16}. {1}", targetGlobalId);
                                    throw new Exception(message);
                                }

                                // get referenced entity for update
                                IdentifiedObject targetEntity = GetEntity(targetGlobalId);
                                targetEntity.AddReference(property.Id, io.GlobalId);
                            }

                            io.SetProperty(property);
                        }
                        else
                        {
                            io.SetProperty(property);
                        }
                    }
                }

                CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Inserting entity with GID ({0:x16}) successfully finished.", globalId);
            }
            catch (Exception ex)
            {
                string message = String.Format("Failed to insert entity (GID = 0x{0:x16}) into model. {1}", rd.Id, ex.Message);
                CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Updates entity in block model.
        /// </summary>
        /// <param name="rd">Description of the resource that should be updated</param>		
        private void UpdateEntity(ResourceDescription rd)
		{
			if (rd == null || rd.Properties == null && rd.Properties.Count == 0)
			{	
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Update entity is not done because update operation is empty.");			
				return;
			}
			
			try
			{
					long globalId = rd.Id;

					CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Updating entity with GID ({0:x16}).", globalId);

					if (!this.EntityExists(globalId))
					{
						string message = String.Format("Failed to update entity because entity with specified GID ({0:x16}) does not exist in network model.", globalId);
						CommonTrace.WriteTrace(CommonTrace.TraceError, message);
						throw new Exception(message);
					}

					IdentifiedObject io = GetEntity(globalId);					

					// updating properties of entity
					foreach (Property property in rd.Properties)
					{
						if (property.Type == PropertyType.Reference)
						{
                            long oldTargetGlobalId = io.GetProperty(property.Id).AsReference();
                            
                            if (oldTargetGlobalId != 0)
                            {
                                IdentifiedObject oldTargetEntity = GetEntity(oldTargetGlobalId);
                                oldTargetEntity.RemoveReference(property.Id, globalId);
                            }

							// updating reference of entity
							long targetGlobalId = property.AsReference();

							if (targetGlobalId != 0)
							{								
								if (!EntityExists(targetGlobalId))
								{
									string message = string.Format("Failed to get target entity with GID: 0x{0:X16}.", targetGlobalId);
									throw new Exception(message);
								}

                                IdentifiedObject targetEntity = GetEntity(targetGlobalId);
                                targetEntity.AddReference(property.Id, globalId);
							}

							// update value of the property in specified entity
							io.SetProperty(property);
						}
						else
						{
							// update value of the property in specified entity
							io.SetProperty(property);
						}							
					}

					CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Updating entity with GID ({0:x16}) successfully finished.", globalId);
			}						
			catch (Exception ex)
			{
				string message = String.Format("Failed to update entity (GID = 0x{0:x16}) in model. {1} ", rd.Id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		/// <summary>
		/// Deletes resource from the netowrk model.
		/// </summary>
		/// <param name="rd">Description of the resource that should be deleted</param>		
		private void DeleteEntity(ResourceDescription rd)
		{
			if (rd == null)
			{
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, "Delete entity is not done because update operation is empty.");
				return;
			}

			try
			{
				long globalId = rd.Id;

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Deleting entity with GID ({0:x16}).", globalId);

				// check if entity exists
				if (!this.EntityExists(globalId))
				{
					string message = String.Format("Failed to delete entity because entity with specified GID ({0:x16}) does not exist in network model.", globalId);
					CommonTrace.WriteTrace(CommonTrace.TraceError, message);
					throw new Exception(message);
				}

				// get entity to be deleted
				IdentifiedObject io = GetEntity(globalId);

				// check if entity could be deleted (if it is not referenced by any other entity)
				if (io.IsReferenced)
				{
                    Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode,List<long>>();
                    io.GetReferences(references, TypeOfReference.Target);

                    StringBuilder sb = new StringBuilder();
                    
                    foreach(KeyValuePair<ModelCode, List<long>> kvp in references)
                    {
                        foreach (long referenceGlobalId in kvp.Value)
                        {
                            sb.AppendFormat("0x{0:x16}, ", referenceGlobalId);
                        }
                    }

					string message = String.Format("Failed to delete entity (GID = 0x{0:x16}) because it is referenced by entities with GIDs: {1}.", globalId, sb.ToString());
					CommonTrace.WriteTrace(CommonTrace.TraceError, message);
					throw new Exception(message);
				}
							
				// find property ids
				List<ModelCode> propertyIds = ResourcesDescs.GetAllSettablePropertyIdsForEntityId(io.GlobalId);

				// remove references
				Property property = null;
				foreach (ModelCode propertyId in propertyIds)
				{
					PropertyType propertyType = Property.GetPropertyType(propertyId);

					if (propertyType == PropertyType.Reference)
					{
						property = io.GetProperty(propertyId);

						if (propertyType == PropertyType.Reference)
						{
							// get target entity and remove reference to another entity
							long targetGlobalId = property.AsReference();

							if (targetGlobalId != 0)
							{
								// get target entity
								IdentifiedObject targetEntity = GetEntity(targetGlobalId);

								// remove reference to another entity
								targetEntity.RemoveReference(propertyId, globalId);
							}
						}						                        
					}
				}

				// remove entity form netowrk model
				DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
				Container container = GetContainer(type);
				container.RemoveEntity(globalId);

				CommonTrace.WriteTrace(CommonTrace.TraceVerbose, "Deleting entity with GID ({0:x16}) successfully finished.", globalId);
			}			
			catch (Exception ex)
			{
				string message = String.Format("Failed to delete entity (GID = 0x{0:x16}) from model. {1}", rd.Id, ex.Message);
				CommonTrace.WriteTrace(CommonTrace.TraceError, message);
				throw new Exception(message);
			}
		}

		/// <summary>
		/// Returns related gids with source according to the association 
		/// </summary>
		/// <param name="source">source id</param>		
		/// <param name="association">desinition of association</param>
		/// <returns>related gids</returns>
		private List<long> ApplyAssocioationOnSource(long source, Association association)
		{
			List<long> relatedGids = new List<long>();

			if (association == null)
			{
				association = new Association();
			}

			IdentifiedObject io = GetEntity(source);

			if (!io.HasProperty(association.PropertyId))
			{
				throw new Exception(string.Format("Entity with GID = 0x{0:x16} does not contain prperty with Id = {1}.", source, association.PropertyId));
			}

			Property propertyRef = null;
			if (Property.GetPropertyType(association.PropertyId) == PropertyType.Reference)
			{
				propertyRef = io.GetProperty(association.PropertyId);
				long relatedGidFromProperty = propertyRef.AsReference();

				if (relatedGidFromProperty != 0)
				{
					if (association.Type == 0 || (short)ModelCodeHelper.GetTypeFromModelCode(association.Type) == ModelCodeHelper.ExtractTypeFromGlobalId(relatedGidFromProperty))
					{
						relatedGids.Add(relatedGidFromProperty);
					}
				}
			}
			else if (Property.GetPropertyType(association.PropertyId) == PropertyType.ReferenceVector)
			{
				propertyRef = io.GetProperty(association.PropertyId);
				List<long> relatedGidsFromProperty = propertyRef.AsReferences();

				if (relatedGidsFromProperty != null)
				{
					foreach (long relatedGidFromProperty in relatedGidsFromProperty)
					{
						if (association.Type == 0 || (short)ModelCodeHelper.GetTypeFromModelCode(association.Type) == ModelCodeHelper.ExtractTypeFromGlobalId(relatedGidFromProperty))
						{
							relatedGids.Add(relatedGidFromProperty);
						}
					}
				}
			}
			else
			{
				throw new Exception(string.Format("Association propertyId = {0} is not reference or reference vector type.", association.PropertyId));
			}

			return relatedGids;
		}

		/// <summary>
		/// Writes delta to log
		/// </summary>
		/// <param name="delta">delta instance which will be logged</param>
		public static void TraceDelta(Delta delta)
		{
			try
			{
				StringWriter stringWriter = new StringWriter();
				XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
				xmlWriter.Formatting = Formatting.Indented;
				delta.ExportToXml(xmlWriter);
				xmlWriter.Flush();
				CommonTrace.WriteTrace(CommonTrace.TraceInfo, stringWriter.ToString());				
				xmlWriter.Close();
				stringWriter.Close();
			}
			catch (Exception ex)
			{
				CommonTrace.WriteTrace(CommonTrace.TraceError, "Failed to trace delta with ID = {0}. Reason: {1}", delta.Id, ex.Message);
			}
		}

		private void Initialize()
		{
			List<Delta> result = ReadAllDeltas();

			foreach (Delta delta in result)
			{
				try
				{
					foreach (ResourceDescription rd in delta.InsertOperations)
					{
						InsertEntity(rd);
					}

					foreach (ResourceDescription rd in delta.UpdateOperations)
					{
						UpdateEntity(rd);
					}

					foreach (ResourceDescription rd in delta.DeleteOperations)
					{
						DeleteEntity(rd);
					}
				}
				catch(Exception ex)
				{
					CommonTrace.WriteTrace(CommonTrace.TraceError, "Error while applying delta (id = {0}) during service initialization. {1}", delta.Id, ex.Message);
				}
			}		
		}
        
		private void SaveDelta(Delta delta)
		{
			bool fileExisted = false;

			if (File.Exists(Config.Instance.ConnectionString))
			{
				fileExisted = true;
			}

			FileStream fs = new FileStream(Config.Instance.ConnectionString, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			fs.Seek(0, SeekOrigin.Begin);

			BinaryReader br = null;
			int deltaCount = 0;

			if (fileExisted)
			{
				br = new BinaryReader(fs);
				deltaCount = br.ReadInt32();
			}

			BinaryWriter bw = new BinaryWriter(fs);
			fs.Seek(0, SeekOrigin.Begin);

			delta.Id = ++deltaCount;
			byte[] deltaSerialized = delta.Serialize();
			int deltaLength = deltaSerialized.Length;

			bw.Write(deltaCount);
			fs.Seek(0, SeekOrigin.End);
			bw.Write(deltaLength);
			bw.Write(deltaSerialized);

			if (br != null)
			{
				br.Close();
			}

			bw.Close();			
			fs.Close(); 
		}

		private List<Delta> ReadAllDeltas()
		{
			List<Delta> result = new List<Delta>();
            
            if (!File.Exists(Config.Instance.ConnectionString))
            {
                return result;
            }

			FileStream fs = new FileStream(Config.Instance.ConnectionString, FileMode.OpenOrCreate, FileAccess.Read);
			fs.Seek(0, SeekOrigin.Begin);

			if (fs.Position < fs.Length) // if it is not empty stream
			{
				BinaryReader br = new BinaryReader(fs);
				
				int deltaCount = br.ReadInt32();
				int deltaLength = 0;
				byte[] deltaSerialized = null;
				Delta delta = null;

				for (int i = 0; i < deltaCount; i++)
				{
					deltaLength = br.ReadInt32();
					deltaSerialized = new byte[deltaLength];
					br.Read(deltaSerialized, 0, deltaLength);
					delta = Delta.Deserialize(deltaSerialized);
					result.Add(delta);
				}

				br.Close();
			}

			fs.Close();

			return result;
		}
        
		private Dictionary<short, int> GetCounters()
		{
			Dictionary<short, int> typesCounters = new Dictionary<short, int>();

			foreach (DMSType type in Enum.GetValues(typeof(DMSType)))
			{
				typesCounters[(short)type] = 0;

				if (networkDataModel.ContainsKey(type))
				{
					typesCounters[(short)type] = GetContainer(type).Count;
				}
			}

			return typesCounters;
		}

        private void InformClients()
        {
            while (true)
            {
                lock (LockObjectClient)
                {
                    if (ClientsNeedToUpdate)
                    {
                        ClientsForDeleting.Clear();
                        foreach (IModelForDuplex client in Clients)
                        {
                            try
                            {
                                client.NewDeltaApplied();
                            }
                            catch
                            {
                                ClientsForDeleting.Add(client);
                            }
                        }
                        foreach (IModelForDuplex client in ClientsForDeleting)
                        {
                            Clients.Remove(client);
                        }
                        ClientsNeedToUpdate = false;
                    }
                }
                Thread.Sleep(200);
            }
        }

        public void UpdateClients()
        {
            lock(LockObjectClient)
            {
                ClientsNeedToUpdate = true;
            }
        }

        public Dictionary<DMSType, Container> DeepCopy()
        {
            Dictionary<DMSType, Container> retVal = new Dictionary<DMSType, Container>();
            foreach(KeyValuePair<DMSType, Container> kvp in networkDataModel)
            {
                retVal.Add(kvp.Key, new Container());
                foreach(KeyValuePair<long, IdentifiedObject> kvp2 in kvp.Value.Entities)
                {
                    switch(kvp.Key)
                    {
                        case DMSType.ANALOG:
                            Analog analogCopy = ((Analog)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(analogCopy.GlobalId, analogCopy);
                            break;
                        case DMSType.BASEVOLTAGE:
                            BaseVoltage baseVoltageCopy = ((BaseVoltage)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(baseVoltageCopy.GlobalId, baseVoltageCopy);
                            break;
                        case DMSType.DISCRETE:
                            Discrete discreteCopy = ((Discrete)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(discreteCopy.GlobalId, discreteCopy);
                            break;
                        case DMSType.ENERGYCONS:
                            EnergyConsumer ecCopy = ((EnergyConsumer)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(ecCopy.GlobalId, ecCopy);
                            break;
                        case DMSType.GEOREGION:
                            GeographicalRegion geoRegionCopy = ((GeographicalRegion)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(geoRegionCopy.GlobalId, geoRegionCopy);
                            break;
                        case DMSType.POWERTRANSEND:
                            PowerTransformerEnd pteCopy = ((PowerTransformerEnd)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(pteCopy.GlobalId, pteCopy);
                            break;
                        case DMSType.POWERTRANSFORMER:
                            PowerTransformer ptCopy = ((PowerTransformer)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(ptCopy.GlobalId, ptCopy);
                            break;
                        case DMSType.RATIOTAPCHANGER:
                            RatioTapChanger rtpCopy = ((RatioTapChanger)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(rtpCopy.GlobalId, rtpCopy);
                            break;
                        case DMSType.SUBGEOREGION:
                            SubGeographicalRegion sgrCopy = ((SubGeographicalRegion)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(sgrCopy.GlobalId, sgrCopy);
                            break;
                        case DMSType.SUBSTATION:
                            Substation ssCopy = ((Substation)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(ssCopy.GlobalId, ssCopy);
                            break;
                        case DMSType.VOLTAGELEVEL:
                            VoltageLevel vlCopy = ((VoltageLevel)kvp2.Value).DeepCopy();
                            retVal[kvp.Key].Entities.Add(vlCopy.GlobalId, vlCopy);
                            break;
                    }
                }
            }

            return retVal;
        }
    }
}
