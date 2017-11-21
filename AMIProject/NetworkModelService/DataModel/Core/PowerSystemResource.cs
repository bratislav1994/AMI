///////////////////////////////////////////////////////////
//  PowerSystemResource.cs
//  Implementation of the Class PowerSystemResource
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:16:49 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Meas;
using FTN.Common;

namespace TC57CIM.IEC61970.Core {
	/// <summary>
	/// A power system resource can be an item of equipment such as a switch, an
	/// equipment container containing many individual items of equipment such as a
	/// substation, or an organisational entity such as sub-control area. Power system
	/// resources can have measurements associated.
	/// </summary>
	public class PowerSystemResource : IdentifiedObject {

        private List<long> measurements = new List<long>();

        public PowerSystemResource(long globalId)
            : base(globalId)
        {
        }

        public List<long> Measurements
        {
            get { return measurements; }

            set { measurements = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                PowerSystemResource x = (PowerSystemResource)obj;
                return (CompareHelper.CompareLists(x.measurements,this.measurements,true));
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

        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.PSR_MEASUREMENTS:
                    return true;
              
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.PSR_MEASUREMENTS:
                    property.SetValue(measurements);
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
                return (measurements.Count > 0) || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (measurements != null && measurements.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.PSR_MEASUREMENTS] = measurements.GetRange(0, measurements.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.PSR_MEASUREMENTS:
                    measurements.Add(globalId);
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
                case ModelCode.PSR_MEASUREMENTS:

                    if (measurements.Contains(globalId))
                    {
                        measurements.Remove(globalId);
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

    }//end PowerSystemResource

}//end namespace Core