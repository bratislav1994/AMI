using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;


namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    public class RegulatingControl : PowerSystemResource
    {
        private bool discrete = false;
        private RegulatingControlModelKind mode;
        private PhaseCode monitoredPhase;
        private float targetRange;
        private float targetValue;
        private List<long> regulatingCondEq = new List<long>();

        public RegulatingControl(long globalId)
            : base(globalId)
        {
        }

        public List<long> RegulatingCondEq
        {
            get { return regulatingCondEq; }
            set { regulatingCondEq = value; }
        }

        public bool Discrete
        {
            get { return discrete; }
            set { discrete = value; }
        }

        public RegulatingControlModelKind Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public PhaseCode MonitoredPhase
        {
            get { return monitoredPhase; }
            set { monitoredPhase = value; }
        }

        public float TargetRange
        {
            get { return targetRange; }
            set { targetRange = value; }
        }

        public float TargetValue
        {
            get { return targetValue; }
            set { targetValue = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                RegulatingControl x = (RegulatingControl)obj;
                return (x.discrete == this.discrete && x.mode == this.mode && x.monitoredPhase == this.monitoredPhase &&
                    x.targetRange == this.targetRange && x.targetValue == this.targetValue &&
                    CompareHelper.CompareLists(x.regulatingCondEq, this.regulatingCondEq, true));
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
                case ModelCode.REGULCONTROL_DISCRETE:
                case ModelCode.REGULCONTROL_MODE:
                case ModelCode.REGULCONTROL_MONITPHASE:
                case ModelCode.REGULCONTROL_REGULCONDEQS:
                case ModelCode.REGULCONTROL_TARGETRANGE:
                case ModelCode.REGULCONTROL_TARGETVALUE:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.REGULCONTROL_DISCRETE:
                    prop.SetValue(discrete);
                    break;
                case ModelCode.REGULCONTROL_MODE:
                    prop.SetValue((short)mode);
                    break;
                case ModelCode.REGULCONTROL_MONITPHASE:
                    prop.SetValue((short)monitoredPhase);
                    break;
                case ModelCode.REGULCONTROL_REGULCONDEQS:
                    prop.SetValue(regulatingCondEq);
                    break;
                case ModelCode.REGULCONTROL_TARGETRANGE:
                    prop.SetValue(targetRange);
                    break;
                case ModelCode.REGULCONTROL_TARGETVALUE:
                    prop.SetValue(targetValue);
                    break;

                default:
                    base.GetProperty(prop);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.REGULCONTROL_DISCRETE:
                    discrete = property.AsBool();
                    break;
                case ModelCode.REGULCONTROL_MODE:
                    mode = (RegulatingControlModelKind)property.AsEnum();
                    break;
                case ModelCode.REGULCONTROL_MONITPHASE:
                    monitoredPhase = (PhaseCode)property.AsEnum();
                    break;
                case ModelCode.REGULCONTROL_TARGETRANGE:
                    targetRange = property.AsFloat();
                    break;
                case ModelCode.REGULCONTROL_TARGETVALUE:
                    targetValue = property.AsFloat();
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
                return (regulatingCondEq.Count > 0) || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (regulatingCondEq != null && regulatingCondEq.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.REGULCONTROL_REGULCONDEQS] = regulatingCondEq.GetRange(0, regulatingCondEq.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.REGULCONDEQ_REGULCONTROL:
                    regulatingCondEq.Add(globalId);
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
                case ModelCode.REGULCONDEQ_REGULCONTROL:

                    if (regulatingCondEq.Contains(globalId))
                    {
                        regulatingCondEq.Remove(globalId);
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
    }
}
