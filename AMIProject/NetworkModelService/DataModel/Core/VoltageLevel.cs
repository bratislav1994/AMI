///////////////////////////////////////////////////////////
//  VoltageLevel.cs
//  Implementation of the Class VoltageLevel
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:17:12 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FTN.Common;

namespace TC57CIM.IEC61970.Core {
	/// <summary>
	/// A collection of equipment at one common system voltage forming a switchgear.
	/// The equipment typically consist of breakers, busbars, instrumentation, control,
	/// regulation and protection devices as well as assemblies of all these.
	/// </summary>
	public class VoltageLevel : EquipmentContainer {
        
		private long baseVoltage;
        private long substation;

        public VoltageLevel()
        {

        }

		public VoltageLevel(long globalId) : base(globalId)
        {
        }

        public long BaseVoltage
        {
            get
            {
                return baseVoltage;
            }

            set
            {
                baseVoltage = value;
            }
        }

        public long Substation
        {
            get
            {
                return substation;
            }

            set
            {
                substation = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                VoltageLevel x = (VoltageLevel)obj;
                return (x.baseVoltage == this.baseVoltage && x.substation == this.substation);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                case ModelCode.VOLTAGELEVEL_BASEVOLTAGE:
                case ModelCode.VOLTAGELEVEL_SUBSTATION:
                    return true;

                default:
                    return base.HasProperty(t);

            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.VOLTAGELEVEL_BASEVOLTAGE:
                    property.SetValue(baseVoltage);
                    break;
                case ModelCode.VOLTAGELEVEL_SUBSTATION:
                    property.SetValue(substation);
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
                case ModelCode.VOLTAGELEVEL_BASEVOLTAGE:
                    baseVoltage = property.AsReference();
                    break;
                case ModelCode.VOLTAGELEVEL_SUBSTATION:
                    substation = property.AsReference();
                    break;

                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (baseVoltage != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.VOLTAGELEVEL_BASEVOLTAGE] = new List<long>();
                references[ModelCode.VOLTAGELEVEL_BASEVOLTAGE].Add(baseVoltage);
            }

            if (substation != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.VOLTAGELEVEL_SUBSTATION] = new List<long>();
                references[ModelCode.VOLTAGELEVEL_SUBSTATION].Add(substation);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation	

    }//end VoltageLevel

}//end namespace Core