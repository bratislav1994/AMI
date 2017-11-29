using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;

namespace DataModelTest.MeasTest
{
    [TestFixture]
    public class AnalogTest
    {
        private Analog analog;
        private float maxValue = 100;
        private float minValue = 7;
        private float normalValue = 50;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.analog = new Analog();
            this.analog.MaxValue = maxValue;
            this.analog.MinValue = minValue;
            this.analog.NormalValue = normalValue;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Analog());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new Analog(4294967212));
        }

        [Test]
        public void MaxValueTest()
        {
            analog.MaxValue = maxValue;

            Assert.AreEqual(maxValue, analog.MaxValue);
        }

        [Test]
        public void MinValueTest()
        {
            analog.MinValue = minValue;

            Assert.AreEqual(minValue, analog.MinValue);
        }

        [Test]
        public void NormalValueTest()
        {
            analog.NormalValue = normalValue;

            Assert.AreEqual(normalValue, analog.NormalValue);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.analog;
            bool result = analog.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = analog.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        [TestCase(ModelCode.ANALOG_MINVALUE)]
        [TestCase(ModelCode.ANALOG_NORMALVALUE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = analog.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = analog.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        [TestCase(ModelCode.ANALOG_MINVALUE)]
        [TestCase(ModelCode.ANALOG_NORMALVALUE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => analog.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => analog.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        [TestCase(ModelCode.ANALOG_MINVALUE)]
        [TestCase(ModelCode.ANALOG_NORMALVALUE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.ANALOG_MAXVALUE:
                    property.SetValue(maxValue);
                    break;
                case ModelCode.ANALOG_MINVALUE:
                    property.SetValue(minValue);
                    break;
                case ModelCode.ANALOG_NORMALVALUE:
                    property.SetValue(normalValue);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("Analog_1");
                    break;
            }

            Assert.DoesNotThrow(() => analog.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => analog.SetProperty(property));
        }
    }
}
