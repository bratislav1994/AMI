﻿using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Meas;

namespace DataModelTest.MeasTest
{
    [TestFixture]
    public class MeasurementTest
    {
        private Measurement measurement;
        private UnitSymbol unitSymbol = UnitSymbol.Q;
        private Direction signalDirection = Direction.READ;
        private long powerSystemResource = 4294967281;
        public Property property = new Property();


        [OneTimeSetUp]
        public void SetupTest()
        {
            this.measurement = new Measurement();
            this.measurement.UnitSymbol = unitSymbol;
            this.measurement.SignalDirection = signalDirection;
            this.measurement.PowerSystemResourceRef = powerSystemResource;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Measurement());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new Measurement(42949672961));
        }

        [Test]
        public void UnitSymbolTest()
        {
            measurement.UnitSymbol = unitSymbol;

            Assert.AreEqual(unitSymbol, measurement.UnitSymbol);
        }

        [Test]
        public void SignalDirectionTest()
        {
            measurement.SignalDirection = signalDirection;

            Assert.AreEqual(signalDirection, measurement.SignalDirection);
        }

        [Test]
        public void PowerSystemResourceRefTest()
        {
            measurement.PowerSystemResourceRef = powerSystemResource;

            Assert.AreEqual(powerSystemResource, measurement.PowerSystemResourceRef);
        }
        
        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.measurement;
            bool result = measurement.Equals(obj);

            Assert.AreEqual(true, result);

            // incorrect
            obj = new Measurement() { UnitSymbol = UnitSymbol.P };
            result = measurement.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new Measurement() { SignalDirection = Direction.READ };
            result = measurement.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new Measurement() { PowerSystemResourceRef = 1 };
            result = measurement.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new Measurement() { RtuAddress = 1 };
            result = measurement.Equals(obj);
            Assert.AreNotEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = measurement.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.MEASUREMENT_UNITSYMBOL)]
        [TestCase(ModelCode.MEASUREMENT_DIRECTION)]
        [TestCase(ModelCode.MEASUREMENT_PSR)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = measurement.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = measurement.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.MEASUREMENT_UNITSYMBOL)]
        [TestCase(ModelCode.MEASUREMENT_DIRECTION)]
        [TestCase(ModelCode.MEASUREMENT_PSR)]
        [TestCase(ModelCode.MEASUREMENT_RTUADDRESS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => measurement.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => measurement.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.MEASUREMENT_UNITSYMBOL)]
        [TestCase(ModelCode.MEASUREMENT_DIRECTION)]
        [TestCase(ModelCode.MEASUREMENT_PSR)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.MEASUREMENT_UNITSYMBOL:
                    property.SetValue((int)unitSymbol);
                    break;
                case ModelCode.MEASUREMENT_DIRECTION:
                    property.SetValue((int)signalDirection);
                    break;
                case ModelCode.MEASUREMENT_PSR:
                    property.SetValue(powerSystemResource);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("Measurement_1");
                    break;
            }

            Assert.DoesNotThrow(() => measurement.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => measurement.SetProperty(property));
        }
        
        [Test]
        [TestCase(TypeOfReference.Reference)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => measurement.GetReferences(references, refType));
            Assert.DoesNotThrow(() => measurement.GetReferences(references, TypeOfReference.Target));

            Measurement meas = new Measurement() { PowerSystemResourceRef = 0 };
            Assert.DoesNotThrow(() => measurement.GetReferences(references, refType));
            meas.PowerSystemResourceRef = 1;
            Assert.DoesNotThrow(() => measurement.GetReferences(references, refType));
            meas.PowerSystemResourceRef = powerSystemResource;
            Assert.DoesNotThrow(() => measurement.GetReferences(references, TypeOfReference.Reference));
        }

        [Test]
        public void GetHashCodeTest()
        {
            Measurement m = new Measurement() { Name = "meas" };
            int hashCode = m.GetHashCode();
            Measurement m2 = new Measurement() { Name = "meas" };
            int hashCodeBv = m2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            m = m2;
            Assert.AreEqual(m.GetHashCode(), m2.GetHashCode());
        }

        [Test]
        public void GetIdDbTest()
        {
            measurement.IdDB = 1;
            Assert.AreEqual(measurement.IdDB, 1);
        }

        [Test]
        public void GetRtuAddressTest()
        {
            measurement.RtuAddress = 1;
            Assert.AreEqual(measurement.RtuAddress, 1);
        }
    }
}
