///////////////////////////////////////////////////////////
//  SubGeographicalRegion.cs
//  Implementation of the Class SubGeographicalRegion
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:16:55 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;
using FTN.Common;

namespace TC57CIM.IEC61970.Core {
	/// <summary>
	/// A subset of a geographical region of a power system network model.
	/// </summary>
	public class SubGeographicalRegion : IdentifiedObject {
        
        private long geoRegion = 0;
        private List<long> substations = new List<long>();

        public SubGeographicalRegion()
        {

        }

        public SubGeographicalRegion(long globalId) : base(globalId)
        {
        }
        
        public long GeoRegion
        {
            get { return geoRegion; }
            set { geoRegion = value; }
        }

        public List<long> Substations
        {
            get { return substations; }
            set { substations = value; }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(base.Equals(obj))
            {
                SubGeographicalRegion x = (SubGeographicalRegion)obj;
                return (x.geoRegion == this.geoRegion &&
                        CompareHelper.CompareLists(x.substations, this.substations));
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return this.Mrid + "\n" + this.Name;
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                case ModelCode.SUBGEOREGION_GEOREG:
                case ModelCode.SUBGEOREGION_SUBS:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.SUBGEOREGION_GEOREG:
                    property.SetValue(geoRegion);
                    break;

                case ModelCode.SUBGEOREGION_SUBS:
                    property.SetValue(substations);
                    break;

                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch(property.Id)
            {
                case ModelCode.SUBGEOREGION_GEOREG:
                    geoRegion = property.AsReference();
                    break;

                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        public override bool IsReferenced
        {
            get
            {
                return substations.Count != 0 || base.IsReferenced;
            }
        }
       
        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if(geoRegion != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.SUBGEOREGION_GEOREG] = new List<long>();
                references[ModelCode.SUBGEOREGION_GEOREG].Add(geoRegion);
            }

            if(substations != null && substations.Count != 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.SUBGEOREGION_SUBS] = substations.GetRange(0, substations.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.SUBSTATION_SUBGEOREGION:
                    substations.Add(globalId);
                    break;

                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.SUBSTATION_SUBGEOREGION:

                    if(substations.Contains(globalId))
                    {
                        substations.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }

                    break;

                default:
                    base.RemoveReference(referenceId, globalId);
                    break;
            }
        }

        #endregion IReference implementation

        public void RD2Class(ResourceDescription rd)
        {
            foreach (Property p in rd.Properties)
            {
                if (p.Id == ModelCode.SUBGEOREGION_SUBS)
                {
                    foreach (long l in p.PropertyValue.LongValues)
                    {
                        this.AddReference(ModelCode.SUBSTATION_SUBGEOREGION, l);
                    }
                }
                else
                {
                    SetProperty(p);
                }
            }
        }

        public SubGeographicalRegion DeepCopy()
        {
            SubGeographicalRegion subGeoRegionCopy = new SubGeographicalRegion();

            subGeoRegionCopy.GlobalId = this.GlobalId;
            subGeoRegionCopy.Mrid = this.Mrid;
            subGeoRegionCopy.Name = this.Name;
            subGeoRegionCopy.GeoRegion = this.GeoRegion;
            subGeoRegionCopy.Substations = this.Substations;

            return subGeoRegionCopy;
        }

    }//end SubGeographicalRegion

}//end namespace Core