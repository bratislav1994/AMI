using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace CommonTest.GDATest
{
    [TestFixture]
    public class PropertyTest
    {
        private Property property;
        private ModelCode id = ModelCode.ANALOG_NORMALVALUE;
        private PropertyValue value = new PropertyValue(100);

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.property = new Property();
            this.property.Id = id;
            this.property.PropertyValue = value;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Property());
        }

        [Test]
        public void ConstructorWithParameter1Test()
        {
            Assert.DoesNotThrow(() => new Property(property));
        }

        [Test]
        public void ConstructorWithParameter2Test()
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.ANALOG_NORMALVALUE));
        }

        [Test]
        public void ConstructorWithParameter3Test()
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.ANALOG_NORMALVALUE, value));
        }

        [Test]
        public void ConstructorWithParameter42Test()
        {
            Assert.Throws<Exception>(() => new Property(ModelCode.ANALOG_NORMALVALUE, false));
        }

        [Test]
        [TestCase(100)]
        public void ConstructorWithParameter5Test(int value)
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.ANALOG_ALARMLOW, value));
        }

        [Test]
        [TestCase(100)]
        public void ConstructorWithParameter6Test(float value)
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.ANALOG_NORMALVALUE, value));
        }

        [Test]
        public void ConstructorWithParameter7Test()
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.SUBGEOREGION_GEOREG, 42949667788));
        }

        [Test]
        public void ConstructorWithParameter8Test()
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.IDOBJ_NAME, "IDObj"));
        }

        [Test]
        public void IdTest()
        {
            property.Id = id;

            Assert.AreEqual(id, property.Id);
        }

        [Test]
        public void ValueTest()
        {
            property.PropertyValue = value;

            Assert.AreEqual(value, property.PropertyValue);
        }

        [Test]
        public void TypeTest()
        {
            Assert.AreEqual(PropertyType.Float, property.Type);
        }

        [Test]
        [TestCase(1000)]
        public void TestEquals(float v)
        {
            Property prop1 = null;
            Property prop2 = null;
            Assert.AreEqual(true, prop1 == prop2);

            prop2 = new Property();
            Assert.AreNotEqual(true, prop1 == prop2);

            prop1 = new Property();
            prop2 = null;
            Assert.AreNotEqual(true, prop1 == prop2);

            PropertyValue value = new PropertyValue(1000);
            prop1 = new Property(ModelCode.ANALOG_ALARMHIGH, value);
            prop2 = new Property(ModelCode.ANALOG_ALARMHIGH, value);
            Assert.AreEqual(true, prop1 == prop2);

            value = new PropertyValue(v);
            prop1 = new Property(ModelCode.ANALOG_NORMALVALUE, value);
            prop2 = new Property(ModelCode.ANALOG_NORMALVALUE, value);
            Assert.AreEqual(true, prop1 == prop2);

            List<long> gid = new List<long>() { 48562846284, 485628462845 };
            value = new PropertyValue(gid);
            prop1 = new Property(ModelCode.BASEVOLTAGE_CONDEQS, value);
            prop2 = new Property(ModelCode.BASEVOLTAGE_CONDEQS, value);
            Assert.AreEqual(true, prop1 == prop2);

            value = new PropertyValue(48562846285);
            prop1 = new Property(ModelCode.IDOBJ_GID, value);
            prop2 = new Property(ModelCode.IDOBJ_GID, value);
            Assert.AreEqual(true, prop1 == prop2);

            value = new PropertyValue("IDobj");
            prop1 = new Property(ModelCode.IDOBJ_NAME, value);
            prop2 = new Property(ModelCode.IDOBJ_NAME, value);
            Assert.AreEqual(true, prop1 == prop2);

            value = new PropertyValue(48562846285);
            prop1 = new Property(ModelCode.MEASUREMENT_PSR, value);
            prop2 = new Property(ModelCode.MEASUREMENT_PSR, value);
            Assert.AreEqual(true, prop1 == prop2);

            value = new PropertyValue(1);
            prop1 = new Property(ModelCode.MEASUREMENT_DIRECTION, value);
            prop2 = new Property(ModelCode.MEASUREMENT_DIRECTION, value);
            Assert.AreEqual(true, prop1 == prop2);
        }

        [Test]
        public void GetPropertyType()
        {
            Assert.AreEqual(PropertyType.Float, Property.GetPropertyType(ModelCode.ANALOG_MAXVALUE));
        }

        [Test]
        public void UnequalTest()
        {
            Property prop1 = null;
            Property prop2 = new Property();
            Assert.AreEqual(true, prop1 != prop2);
        }

        [Test]
        public void EqualTest()
        {
            Assert.AreEqual(true, property.Equals(property));
        }
    }
}
