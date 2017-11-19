using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    public class SynchronousMachine : RotatingMachine
    {
        private long reactiveCapabilityCurves = 0;

        public SynchronousMachine(long globalId)
            : base(globalId)
        {
        }

        public long ReactiveCapabilityCurves
        {
            get
            {
                return reactiveCapabilityCurves;
            }

            set
            {
                reactiveCapabilityCurves = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                SynchronousMachine x = (SynchronousMachine)obj;
                return (x.reactiveCapabilityCurves == this.reactiveCapabilityCurves);
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
                case ModelCode.SYNMACHINE_REACTCAPABCURVE:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.SYNMACHINE_REACTCAPABCURVE:
                    property.SetValue(reactiveCapabilityCurves);
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
                case ModelCode.SYNMACHINE_REACTCAPABCURVE:
                    reactiveCapabilityCurves = property.AsReference();
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
            if (reactiveCapabilityCurves != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.SYNMACHINE_REACTCAPABCURVE] = new List<long>();
                references[ModelCode.SYNMACHINE_REACTCAPABCURVE].Add(reactiveCapabilityCurves);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation		
    }
}
