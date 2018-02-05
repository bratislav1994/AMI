///////////////////////////////////////////////////////////
//  IdentifiedObject.cs
//  Implementation of the Class IdentifiedObject
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:16:47 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FTN.Common;
using System.Runtime.Serialization;
using TC57CIM.IEC61970.Meas;

namespace TC57CIM.IEC61970.Core
{

    public enum TypeOfReference : short
    {
        Reference = 1,
        Target = 2,
        Both = 3,
    }

    /// <summary>
    /// This is a root class to provide common identification for all classes needing
    /// identification and naming attributes.
    /// </summary>
    [DataContract]
    [Serializable]
    [KnownType(typeof(Measurement))]
    [KnownType(typeof(PowerSystemResource))]
    [KnownType(typeof(BaseVoltage))]
    public class IdentifiedObject
    {

        private static ModelResourcesDesc resourcesDescs = new ModelResourcesDesc();
        private long globalId;

        /// <summary>
        /// Master resource identifier issued by a model authority. The mRID is globally
        /// unique within an exchange context.
        /// Global uniqeness is easily achived by using a UUID for the mRID. It is strongly
        /// recommended to do this.
        /// For CIMXML data files in RDF syntax, the mRID is mapped to rdf:ID or rdf:about
        /// attributes that identify CIM object elements.
        /// </summary>
        private string mRID;
        /// <summary>
        /// The name is any free human readable and possibly non unique text naming the
        /// object.
        /// </summary>
        private string name;

        public IdentifiedObject()
        {

        }

        /// <summary>
        /// Initializes a new instance of the IdentifiedObject class.
        /// </summary>		
        /// <param name="globalId">Global id of the entity.</param>
        public IdentifiedObject(long globalId)
        {
            this.globalId = globalId;
        }

        ~IdentifiedObject()
        {

        }

        /// <summary>
        /// Gets or sets global id of the entity (identified object).
        /// </summary>	
        [DataMember]
        public long GlobalId
        {
            get
            {
                return globalId;
            }

            set
            {
                globalId = value;
            }
        }

        /// <summary>
        /// Gets or sets mrid of the entity (identified object).
        /// </summary>			
        [DataMember]
        public string Mrid
        {
            get { return mRID; }
            set { mRID = value; }
        }

        [DataMember]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public static bool operator ==(IdentifiedObject x, IdentifiedObject y)
        {
            if (Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null))
            {
                return true;
            }
            else if ((Object.ReferenceEquals(x, null) && !Object.ReferenceEquals(y, null)) ||
                (!Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null)))
            {
                return false;
            }
            else
            {
                return x.Equals(y);
            }
        }

        public static bool operator !=(IdentifiedObject x, IdentifiedObject y)
        {
            return !(x == y);
        }

        public override bool Equals(object x)
        {
            if (Object.ReferenceEquals(x, null))
            {
                return false;
            }
            else
            {
                if (!(x is IdentifiedObject))
                    return false;
                IdentifiedObject io = (IdentifiedObject)x;
                return ((io.GlobalId == this.GlobalId) && (io.mRID == this.mRID));
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IAccess implementation

        public virtual bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.IDOBJ_GID:
                case ModelCode.IDOBJ_MRID:
                case ModelCode.IDOBJ_NAME:
                    return true;

                default:
                    return false;
            }
        }

        public virtual void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.IDOBJ_GID:
                    property.SetValue(globalId);
                    break;
                case ModelCode.IDOBJ_MRID:
                    property.SetValue(mRID);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue(Name);
                    break;
                default:
                    string message = string.Format("Unknown property id = {0} for entity (GID = 0x{1:x16}).", property.Id.ToString(), this.GlobalId);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    break;
                    //throw new Exception(message);
            }
        }

        public virtual void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.IDOBJ_GID:
                    globalId = property.AsLong();
                    break;
                case ModelCode.IDOBJ_MRID:
                    mRID = property.AsString();
                    break;
                case ModelCode.IDOBJ_NAME:
                    Name = property.AsString();
                    break;
                default:
                    string message = string.Format("Unknown property id = {0} for entity (GID = 0x{1:x16}).", property.Id.ToString(), this.GlobalId);
                    CommonTrace.WriteTrace(CommonTrace.TraceError, message);
                    break;
                    //throw new Exception(message);
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        public virtual bool IsReferenced
        {
            get
            {
                return false;
            }
        }

        public virtual void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            return;
        }

        public virtual void AddReference(ModelCode referenceId, long globalId)
        {
            string message = string.Format("Can not add reference {0} to entity (GID = 0x{1:x16}).", referenceId, this.GlobalId);
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            //throw new Exception(message);
        }

        public virtual void RemoveReference(ModelCode referenceId, long globalId)
        {
            string message = string.Format("Can not remove reference {0} from entity (GID = 0x{1:x16}).", referenceId, this.GlobalId);
            CommonTrace.WriteTrace(CommonTrace.TraceError, message);
            //throw new ModelException(message);
        }

        #endregion IReference implementation

        #region utility methods

        public void GetReferences(Dictionary<ModelCode, List<long>> references)
        {
            GetReferences(references, TypeOfReference.Target | TypeOfReference.Reference);
        }

        //public ResourceDescription GetAsResourceDescription(bool onlySettableAttributes)
        //{
        //    ResourceDescription rd = new ResourceDescription(globalId);
        //    List<ModelCode> props = new List<ModelCode>();

        //    if (onlySettableAttributes == true)
        //    {
        //        props = resourcesDescs.GetAllSettablePropertyIdsForEntityId(globalId);
        //    }
        //    else
        //    {
        //        props = resourcesDescs.GetAllPropertyIdsForEntityId(globalId);
        //    }

        //    return rd;
        //}

        //public ResourceDescription GetAsResourceDescription(List<ModelCode> propIds)
        //{
        //    ResourceDescription rd = new ResourceDescription(globalId);

        //    for (int i = 0; i < propIds.Count; i++)
        //    {
        //        rd.AddProperty(GetProperty(propIds[i]));
        //    }

        //    return rd;
        //}

        public virtual Property GetProperty(ModelCode propId)
        {
            Property property = new Property(propId);
            GetProperty(property);
            return property;
        }

        //public void GetDifferentProperties(IdentifiedObject compared, out List<Property> valuesInOriginal, out List<Property> valuesInCompared)
        //{
        //    valuesInCompared = new List<Property>();
        //    valuesInOriginal = new List<Property>();

        //    ResourceDescription rd = this.GetAsResourceDescription(false);

        //    if (compared != null)
        //    {
        //        ResourceDescription rdCompared = compared.GetAsResourceDescription(false);

        //        for (int i = 0; i < rd.Properties.Count; i++)
        //        {
        //            if (rd.Properties[i] != rdCompared.Properties[i])
        //            {
        //                valuesInOriginal.Add(rd.Properties[i]);
        //                valuesInCompared.Add(rdCompared.Properties[i]);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < rd.Properties.Count; i++)
        //        {
        //            valuesInOriginal.Add(rd.Properties[i]);
        //        }
        //    }
        //}

        #endregion utility methods

    }//end IdentifiedObject

}//end namespace Core