///////////////////////////////////////////////////////////
//  EnergyConsumer.cs
//  Implementation of the Class EnergyConsumer
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:16:43 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;
using FTN.Common;

namespace TC57CIM.IEC61970.Wires {
	/// <summary>
	/// Generic user of energy - a  point of consumption on the power system model.
	/// </summary>
	public class EnergyConsumer : ConductingEquipment {

        private float pfixed;
        private float qfixed;
        private ConsumerType type;

        public EnergyConsumer()
        {

        }

        public EnergyConsumer(long globalId) : base(globalId)
        {
        }

        public float Pfixed
        {
            get { return pfixed; }
            set { pfixed = value; }
        }

        public float Qfixed
        {
            get { return qfixed; }
            set { qfixed = value; }
        }

        public ConsumerType Type
        {
            get { return type; }
            set { type = value; }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                EnergyConsumer x = (EnergyConsumer)obj;
                return (x.pfixed == this.pfixed && x.qfixed == this.qfixed && x.type == this.type);
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
                case ModelCode.ENERGYCONS_PFIXED:
                case ModelCode.ENERGYCONS_QFIXED:
                case ModelCode.ENERGYCONS_TYPE:
                    return true;

                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.ENERGYCONS_PFIXED:
                    property.SetValue(pfixed);
                    break;
                case ModelCode.ENERGYCONS_QFIXED:
                    property.SetValue(qfixed);
                    break;
                case ModelCode.ENERGYCONS_TYPE:
                    property.SetValue((int)type);
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
                case ModelCode.ENERGYCONS_PFIXED:
                    pfixed = property.AsFloat();
                    break;
                case ModelCode.ENERGYCONS_QFIXED:
                    qfixed = property.AsFloat();
                    break;
                case ModelCode.ENERGYCONS_TYPE:
                    type = (ConsumerType)property.AsInt();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        public void RD2Class(ResourceDescription rd)
        {
            foreach (Property p in rd.Properties)
            {
                if (p.Id == ModelCode.PSR_MEASUREMENTS)
                {
                    foreach (long l in p.PropertyValue.LongValues)
                    {
                        this.AddReference(ModelCode.MEASUREMENT_PSR, l);
                    }
                }
                else
                {
                    SetProperty(p);
                }
            }
        }

        public EnergyConsumer DeepCopy()
        {
            EnergyConsumer consumerCopy = new EnergyConsumer();

            consumerCopy.GlobalId = this.GlobalId;
            consumerCopy.Mrid = this.Mrid;
            consumerCopy.Name = this.Name;
            consumerCopy.BaseVoltage = this.BaseVoltage;
            consumerCopy.EqContainer = this.EqContainer;
            consumerCopy.Measurements = this.Measurements;
            consumerCopy.Pfixed = this.Pfixed;
            consumerCopy.Qfixed = this.Qfixed;
            consumerCopy.Type = this.Type;

            return consumerCopy;
        }

    }//end EnergyConsumer

}//end namespace Wires