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
    public class DiscreteTest
    {
        private Discrete discrete;
        private int maxValue = 100;
        private int minValue = 7;
        private int normalValue = 50;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.discrete = new Discrete();
            this.discrete.MaxValue = maxValue;
            this.discrete.MinValue = minValue;
            this.discrete.NormalValue = normalValue;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Discrete());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new Discrete(0));
        }

        [Test]
        public void MaxValueTest()
        {
            discrete.MaxValue = maxValue;

            Assert.AreEqual(maxValue, discrete.MaxValue);
        }

        [Test]
        public void MinValueTest()
        {
            discrete.MinValue = minValue;

            Assert.AreEqual(minValue, discrete.MinValue);
        }

        [Test]
        public void NormalValueTest()
        {
            discrete.NormalValue = normalValue;

            Assert.AreEqual(normalValue, discrete.NormalValue);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.discrete;
            bool result = discrete.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.DISCRETE_MAXVALUE)]
        [TestCase(ModelCode.DISCRETE_MINVALUE)]
        [TestCase(ModelCode.DISCRETE_NORMALVALUE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = discrete.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = discrete.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.DISCRETE_MAXVALUE)]
        [TestCase(ModelCode.DISCRETE_MINVALUE)]
        [TestCase(ModelCode.DISCRETE_NORMALVALUE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => discrete.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => discrete.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.DISCRETE_MAXVALUE)]
        [TestCase(ModelCode.DISCRETE_MINVALUE)]
        [TestCase(ModelCode.DISCRETE_NORMALVALUE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.DISCRETE_MAXVALUE:
                    property.SetValue(maxValue);
                    break;
                case ModelCode.DISCRETE_MINVALUE:
                    property.SetValue(minValue);
                    break;
                case ModelCode.DISCRETE_NORMALVALUE:
                    property.SetValue(normalValue);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("Discrete_1");
                    break;
            }

            Assert.DoesNotThrow(() => discrete.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => discrete.SetProperty(property));
        }
    }
}
