///////////////////////////////////////////////////////////
//  PowerTransformerEnd.cs
//  Implementation of the Class PowerTransformerEnd
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:17:06 PM
//  Original author: Batinic
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



using TC57CIM.IEC61970.Wires;
using FTN.Common;
using TC57CIM.IEC61970.Core;

namespace TC57CIM.IEC61970.Wires {
	/// <summary>
	/// A PowerTransformerEnd is associated with each Terminal of a PowerTransformer.
	/// The impdedance values r, r0, x, and x0 of a PowerTransformerEnd represents a
	/// star equivalent as follows
	/// 1) for a two Terminal PowerTransformer the high voltage PowerTransformerEnd has
	/// non zero values on r, r0, x, and x0 while the low voltage PowerTransformerEnd
	/// has zero values for r, r0, x, and x0.
	/// 2) for a three Terminal PowerTransformer the three PowerTransformerEnds
	/// represents a star equivalent with each leg in the star represented by r, r0, x,
	/// and x0 values.
	/// 3) for a PowerTransformer with more than three Terminals the
	/// PowerTransformerEnd impedance values cannot be used. Instead use the
	/// TransformerMeshImpedance or split the transformer into multiple
	/// PowerTransformers.
	/// </summary>
	public class PowerTransformerEnd : TransformerEnd {

        private long powerTrans = 0;
        
        public PowerTransformerEnd()
        {
        }
        

        public PowerTransformerEnd(long globalId) : base(globalId)
        {
        }

        public long PowerTrans
        {
            get { return powerTrans; }
            set { powerTrans = value; }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                PowerTransformerEnd x = (PowerTransformerEnd)obj;
                return (x.powerTrans == this.powerTrans);
            }
            else
            {
                return false;
            }
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                case ModelCode.POWERTRANSEND_POWERTRANSF:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.POWERTRANSEND_POWERTRANSF:
                    property.SetValue(powerTrans);
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
                case ModelCode.POWERTRANSEND_POWERTRANSF:
                    powerTrans = property.AsReference();
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
            if (powerTrans != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.POWERTRANSEND_POWERTRANSF] = new List<long>();
                references[ModelCode.POWERTRANSEND_POWERTRANSF].Add(powerTrans);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation

    }//end PowerTransformerEnd

}//end namespace Wires