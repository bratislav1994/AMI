///////////////////////////////////////////////////////////
//  Measurement.cs
//  Implementation of the Class Measurement
//  Generated by Enterprise Architect
//  Created on:      20-Nov-2017 7:16:58 PM
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



using FTN.Common;
using TC57CIM.IEC61970.Core;
namespace TC57CIM.IEC61970.Meas {
	/// <summary>
	/// A Measurement represents any measured, calculated or non-measured non-
	/// calculated quantity. Any piece of equipment may contain Measurements, e.g. a
	/// substation may have temperature measurements and door open indications, a
	/// transformer may have oil temperature and tank pressure measurements, a bay may
	/// contain a number of power flow measurements and a Breaker may contain a switch
	/// status measurement.
	/// The PSR - Measurement association is intended to capture this use of
	/// Measurement and is included in the naming hierarchy based on EquipmentContainer.
	/// The naming hierarchy typically has Measurements as leafs, e.g. Substation-
	/// VoltageLevel-Bay-Switch-Measurement.
	/// Some Measurements represent quantities related to a particular sensor location
	/// in the network, e.g. a voltage transformer (PT) at a busbar or a current
	/// transformer (CT) at the bar between a breaker and an isolator. The sensing
	/// position is not captured in the PSR - Measurement association. Instead it is
	/// captured by the Measurement - Terminal association that is used to define the
	/// sensing location in the network topology. The location is defined by the
	/// connection of the Terminal to ConductingEquipment.
	/// If both a Terminal and PSR are associated, and the PSR is of type
	/// ConductingEquipment, the associated Terminal should belong to that
	/// ConductingEquipment instance.
	/// When the sensor location is needed both Measurement-PSR and Measurement-
	/// Terminal are used. The Measurement-Terminal association is never used alone.
	/// </summary>
	public class Measurement : IdentifiedObject {

		/// <summary>
		/// The unit of measure of the measured quantity.
		/// </summary>
		public UnitSymbol unitSymbol;
        public Direction direction;

        private long powerSystemResource = 0;

		public Measurement(long globalId)
            : base(globalId)
        { 
		}

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Measurement x = (Measurement)obj;
                return (x.unitSymbol == this.unitSymbol && x.direction == this.direction && x.powerSystemResource == this.powerSystemResource);
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
                case ModelCode.MEASUREMENT_UNITSYMBOL:
                case ModelCode.MEASUREMENT_DIRECTION:
                case ModelCode.MEASUREMENT_PSR:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.MEASUREMENT_UNITSYMBOL:
                    property.SetValue((int)unitSymbol);
                    break;
                case ModelCode.MEASUREMENT_DIRECTION:
                    property.SetValue((int)direction);
                    break;
                case ModelCode.MEASUREMENT_PSR:
                    property.SetValue(powerSystemResource);
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
                case ModelCode.MEASUREMENT_UNITSYMBOL:
                    unitSymbol = (UnitSymbol)property.AsInt();
                    break;
                case ModelCode.MEASUREMENT_DIRECTION:
                    direction = (Direction)property.AsInt();
                    break;
                case ModelCode.MEASUREMENT_PSR:
                    powerSystemResource = property.AsReference();
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
            if (powerSystemResource != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.MEASUREMENT_PSR] = new List<long>();
                references[ModelCode.MEASUREMENT_PSR].Add(powerSystemResource);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation		

    }//end Measurement

}//end namespace Meas