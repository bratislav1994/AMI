using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using FTN.Common;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    public class Curve : IdentifiedObject
    {
        private CurveStyle curveStyle;
        private UnitMultiplier xMultiplier;
        private UnitSymbol xUnit;
        private UnitMultiplier y1Multiplier;
        private UnitSymbol y1Unit;
        private UnitMultiplier y2Multiplier;
        private UnitSymbol y2Unit;
        private UnitMultiplier y3Multiplier;
        private UnitSymbol y3Unit;
        private List<long> curveDatas = new List<long>();

        public Curve(long globalId) : base(globalId)
        {
        }

        public List<long> CurveDatas
        {
            get
            {
                return curveDatas;
            }

            set
            {
                curveDatas = value;
            }
        }

        public CurveStyle CurvStyle
        {
            get
            {
                return curveStyle;
            }

            set
            {
                curveStyle = value;
            }
        }

        public UnitMultiplier XMultiplier
        {
            get
            {
                return xMultiplier;
            }

            set
            {
                xMultiplier = value;
            }
        }

        public UnitSymbol XUnit
        {
            get
            {
                return xUnit;
            }

            set
            {
                xUnit = value;
            }
        }

        public UnitMultiplier Y1Multiplier
        {
            get
            {
                return y1Multiplier;
            }

            set
            {
                y1Multiplier = value;
            }
        }

        public UnitSymbol Y1Unit
        {
            get
            {
                return y1Unit;
            }

            set
            {
                y1Unit = value;
            }
        }

        public UnitMultiplier Y2Multiplier
        {
            get
            {
                return y2Multiplier;
            }

            set
            {
                y2Multiplier = value;
            }
        }

        public UnitSymbol Y2Unit
        {
            get
            {
                return y2Unit;
            }

            set
            {
                y2Unit = value;
            }
        }

        public UnitMultiplier Y3Multiplier
        {
            get
            {
                return y3Multiplier;
            }

            set
            {
                y3Multiplier = value;
            }
        }

        public UnitSymbol Y3Unit
        {
            get
            {
                return y3Unit;
            }

            set
            {
                y3Unit = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Curve x = (Curve)obj;
                return (x.curveStyle == this.curveStyle && x.xMultiplier == this.xMultiplier && x.xUnit == this.xUnit &&
                    x.y1Multiplier == this.y1Multiplier && x.y1Unit == this.y1Unit && x.y2Multiplier == this.y2Multiplier && x.y2Unit == this.y2Unit &&
                    x.y3Multiplier == this.y3Multiplier && x.y3Unit == this.y3Unit && CompareHelper.CompareLists(x.curveDatas, this.curveDatas, true));
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
                case ModelCode.CURVE_CURVEDATAS:
                case ModelCode.CURVE_CURVESTYLE:
                case ModelCode.CURVE_XMYLTIPLIER:
                case ModelCode.CURVE_XUNIT:
                case ModelCode.CURVE_Y1MULTIPLIER:
                case ModelCode.CURVE_Y1UNIT:
                case ModelCode.CURVE_Y2MULTIPLIER:
                case ModelCode.CURVE_Y2UNIT:
                case ModelCode.CURVE_Y3MULTIPLIER:
                case ModelCode.CURVE_Y3UNIT:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.CURVE_CURVEDATAS:
                    prop.SetValue(curveDatas);
                    break;
                case ModelCode.CURVE_CURVESTYLE:
                    prop.SetValue((short)curveStyle);
                    break;
                case ModelCode.CURVE_XMYLTIPLIER:
                    prop.SetValue((short)xMultiplier);
                    break;
                case ModelCode.CURVE_XUNIT:
                    prop.SetValue((short)xUnit);
                    break;
                case ModelCode.CURVE_Y1MULTIPLIER:
                    prop.SetValue((short)y1Multiplier);
                    break;
                case ModelCode.CURVE_Y1UNIT:
                    prop.SetValue((short)y1Unit);
                    break;
                case ModelCode.CURVE_Y2MULTIPLIER:
                    prop.SetValue((short)y2Multiplier);
                    break;
                case ModelCode.CURVE_Y2UNIT:
                    prop.SetValue((short)y2Unit);
                    break;
                case ModelCode.CURVE_Y3MULTIPLIER:
                    prop.SetValue((short)y3Multiplier);
                    break;
                case ModelCode.CURVE_Y3UNIT:
                    prop.SetValue((short)y3Unit);
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
                case ModelCode.CURVE_CURVESTYLE:
                    curveStyle = (CurveStyle)property.AsEnum();
                    break;
                case ModelCode.CURVE_XMYLTIPLIER:
                    xMultiplier = (UnitMultiplier)property.AsEnum();
                    break;
                case ModelCode.CURVE_XUNIT:
                    xUnit = (UnitSymbol)property.AsEnum();
                    break;
                case ModelCode.CURVE_Y1MULTIPLIER:
                    y1Multiplier = (UnitMultiplier)property.AsEnum();
                    break;
                case ModelCode.CURVE_Y1UNIT:
                    y1Unit = (UnitSymbol)property.AsEnum();
                    break;
                case ModelCode.CURVE_Y2MULTIPLIER:
                    y2Multiplier = (UnitMultiplier)property.AsEnum();
                    break;
                case ModelCode.CURVE_Y2UNIT:
                    y2Unit = (UnitSymbol)property.AsEnum();
                    break;
                case ModelCode.CURVE_Y3MULTIPLIER:
                    y3Multiplier = (UnitMultiplier)property.AsEnum();
                    break;
                case ModelCode.CURVE_Y3UNIT:
                    y3Unit = (UnitSymbol)property.AsEnum();
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
                return curveDatas.Count > 0 || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (curveDatas != null && curveDatas.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.CURVE_CURVEDATAS] = curveDatas.GetRange(0, curveDatas.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.CURVEDATA_CURVE:
                    curveDatas.Add(globalId);
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
                case ModelCode.CURVEDATA_CURVE:

                    if (curveDatas.Contains(globalId))
                    {
                        curveDatas.Remove(globalId);
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
