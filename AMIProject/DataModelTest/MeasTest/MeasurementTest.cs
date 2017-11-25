using FTN.Common;
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
        private long powerSystemResource = 0;
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
            Assert.DoesNotThrow(() => new Measurement(0));
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

            Assert.Throws<Exception>(() => measurement.GetProperty(property));
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

            Assert.Throws<Exception>(() => measurement.SetProperty(property));
        }
        
        [Test]
        [TestCase(TypeOfReference.Reference)]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => measurement.GetReferences(references, refType));
        }
    }
}
