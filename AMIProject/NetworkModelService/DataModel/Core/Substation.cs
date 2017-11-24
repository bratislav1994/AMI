///////////////////////////////////////////////////////////
//  Substation.cs
//  Implementation of the Class Substation
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:16:57 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



using TC57CIM.IEC61970.Core;
using FTN.Common;

namespace TC57CIM.IEC61970.Core {
	/// <summary>
	/// A collection of equipment for purposes other than generation or utilization,
	/// through which electric energy in bulk is passed for the purposes of switching
	/// or modifying its characteristics.
	/// </summary>
	public class Substation : EquipmentContainer {

        private long subGeoRegion = 0;
        private List<long> voltageLevels = new List<long>();

        public Substation()
        {

        }

        public Substation(long globalId) : base(globalId)
        {
        }

        public long SubGeoRegion
        {
            get { return subGeoRegion; }
            set { subGeoRegion = value; }
        }

        public List<long> VoltageLevels
        {
            get { return voltageLevels; }
            set { voltageLevels = value; }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Substation x = (Substation)obj;
                return (x.subGeoRegion == this.subGeoRegion &&
                        CompareHelper.CompareLists(x.voltageLevels, this.voltageLevels));
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
                case ModelCode.SUBSTATION_SUBGEOREGION:
                case ModelCode.SUBSTATION_VOLTLEVELS:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.SUBSTATION_SUBGEOREGION:
                    property.SetValue(subGeoRegion);
                    break;

                case ModelCode.SUBSTATION_VOLTLEVELS:
                    property.SetValue(voltageLevels);
                    break;

                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.SUBSTATION_SUBGEOREGION:
                    subGeoRegion = property.AsReference();
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
                return voltageLevels.Count != 0 || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (subGeoRegion != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.SUBSTATION_SUBGEOREGION] = new List<long>();
                references[ModelCode.SUBSTATION_SUBGEOREGION].Add(subGeoRegion);
            }

            if (voltageLevels != null && voltageLevels.Count != 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.SUBSTATION_VOLTLEVELS] = voltageLevels.GetRange(0, voltageLevels.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.VOLTAGELEVEL_SUBSTATION:
                    voltageLevels.Add(globalId);
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
                case ModelCode.VOLTAGELEVEL_SUBSTATION:

                    if (voltageLevels.Contains(globalId))
                    {
                        voltageLevels.Remove(globalId);
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
                if (p.Id == ModelCode.SUBSTATION_VOLTLEVELS)
                {
                    foreach (long l in p.PropertyValue.LongValues)
                    {
                        this.AddReference(ModelCode.VOLTAGELEVEL_SUBSTATION, l);
                    }
                }
                else if (p.Id != ModelCode.PSR_MEASUREMENTS && p.Id != ModelCode.EQCONTAINER_EQUIPMENTS)
                {
                    SetProperty(p);
                }
            }
        }

    }//end Substation

}//end namespace Core