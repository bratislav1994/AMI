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
        private ModelCode id = ModelCode.ENERGYCONS_PMAX;
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
            Assert.DoesNotThrow(() => new Property(ModelCode.ENERGYCONS_PMAX));
        }

        [Test]
        public void ConstructorWithParameter3Test()
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.ENERGYCONS_PMAX, value));
        }

        [Test]
        public void ConstructorWithParameter42Test()
        {
            Assert.Throws<Exception>(() => new Property(ModelCode.ENERGYCONS_PMAX, false));
        }

        [Test]
        [TestCase(100)]
        public void ConstructorWithParameter5Test(float value)
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.ENERGYCONS_PMAX, value));
        }

        [Test]
        [TestCase(100)]
        public void ConstructorWithParameter6Test(float value)
        {
            Assert.DoesNotThrow(() => new Property(ModelCode.ENERGYCONS_PMAX, value));
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
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
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
            prop1 = new Property(ModelCode.ENERGYCONS_PMAX, value);
            prop2 = new Property(ModelCode.ENERGYCONS_PMAX, value);
            Assert.AreEqual(true, prop1 == prop2);

            value = new PropertyValue(v);
            prop1 = new Property(ModelCode.ENERGYCONS_QMAX, value);
            prop2 = new Property(ModelCode.ENERGYCONS_QMAX, value);
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
            Assert.AreEqual(PropertyType.Float, Property.GetPropertyType(ModelCode.ENERGYCONS_PMAX));
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

        [Test]
        public void ToStringTest()
        {
            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            this.property.PropertyValue = new PropertyValue(1);
            string result = property.ToString();
            Assert.AreEqual("1", result);

            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            this.property.PropertyValue = new PropertyValue(150);
            result = property.PropertyValue.LongValue.ToString();
            Assert.AreEqual("150", result);

            this.property.Id = ModelCode.IDOBJ_GID;
            this.property.PropertyValue = new PropertyValue(2947647382947);
            result = property.ToString();
            Assert.AreEqual("2947647382947", result);

            this.property.Id = ModelCode.MEASUREMENT_PSR;
            this.property.PropertyValue = new PropertyValue(49584662738463);
            result = property.ToString();
            Assert.AreEqual("49584662738463", result);

            this.property.Id = ModelCode.IDOBJ_NAME;
            this.property.PropertyValue = new PropertyValue("IDObj");
            result = property.ToString();
            Assert.AreEqual("IDObj", result);

            this.property.Id = ModelCode.GEOREGION_SUBGEOREGIONS;
            this.property.PropertyValue = new PropertyValue();
            result = property.ToString();
            Assert.AreEqual(null, result);
        }

        [Test]
        public void SetValueTest()
        {
            this.property.Id = ModelCode.IDOBJ_MRID;
            int valueI = 1000;
            Assert.Throws<Exception>(() => property.SetValue(valueI));
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.DoesNotThrow(() => property.SetValue(valueI));
            this.property.Id = ModelCode.IDOBJ_GID;
            Assert.DoesNotThrow(() => property.SetValue(valueI));
            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            Assert.DoesNotThrow(() => property.SetValue(valueI));

            float valueF = 1000;
            Assert.Throws<Exception>(() => property.SetValue(valueF));
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.DoesNotThrow(() => property.SetValue(valueF));

            long valueL = 1000;
            Assert.Throws<Exception>(() => property.SetValue(valueL));
            this.property.Id = ModelCode.IDOBJ_GID;
            Assert.DoesNotThrow(() => property.SetValue(valueL));
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.DoesNotThrow(() => property.SetValue(valueL));
            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            Assert.DoesNotThrow(() => property.SetValue(valueL));
            this.property.Id = ModelCode.MEASUREMENT_PSR;
            Assert.DoesNotThrow(() => property.SetValue(valueL));

            string valueS = "NameID";
            Assert.Throws<Exception>(() => property.SetValue(valueS));
            this.property.Id = ModelCode.IDOBJ_NAME;
            Assert.DoesNotThrow(() => property.SetValue(valueS));

            short valueSh = 1;
            Assert.Throws<Exception>(() => property.SetValue(valueSh));
            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            Assert.DoesNotThrow(() => property.SetValue(valueSh));
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.DoesNotThrow(() => property.SetValue(valueSh));
            this.property.Id = ModelCode.IDOBJ_GID;
            Assert.DoesNotThrow(() => property.SetValue(valueSh));
            
            Assert.Throws<Exception>(() => property.SetValue(true));

            List<long> value = new List<long>() { 837437837463, 5933625738 };
            Assert.Throws<Exception>(() => property.SetValue(value));
            this.property.Id = ModelCode.GEOREGION_SUBGEOREGIONS;
            Assert.DoesNotThrow(() => property.SetValue(value));
        }

        [Test]
        public void AsBoolTest()
        {
            Assert.Throws<Exception>(() => property.AsBool());
        }

        [Test]
        public void AsEnumTest()
        {
            short value = 1;
            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            property.SetValue(value);
            short result = property.AsEnum();
            Assert.AreEqual(value, result);
            
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.Throws<Exception>(() => property.AsEnum());
        }

        [Test]
        public void AsIntTest()
        {
            float value = 100;
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            property.SetValue(value);
            float result = property.AsFloat();
            Assert.AreEqual(value, result);

            int value1 = 1;
            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            property.SetValue(value1);
            int result1 = property.AsInt();
            Assert.AreEqual(value1, result1);

            this.property.Id = ModelCode.MEASUREMENT_PSR;
            Assert.Throws<Exception>(() => property.AsInt());
        }

        [Test]
        public void AsLongTest()
        {
            long value = 100;
            this.property.Id = ModelCode.IDOBJ_GID;
            property.SetValue(value);
            long result = property.AsLong();
            Assert.AreEqual(value, result);

            value = 1;
            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            property.SetValue(value);
            result = property.AsLong();
            Assert.AreEqual(value, result);

            value = 100;
            this.property.Id = ModelCode.MEASUREMENT_RTUADDRESS;
            property.SetValue(value);
            result = property.AsLong();
            Assert.AreEqual(value, result);

            value = 1;
            this.property.Id = ModelCode.MEASUREMENT_PSR;
            property.SetValue(value);
            result = property.AsLong();
            Assert.AreEqual(value, result);

            this.property.Id = ModelCode.IDOBJ_NAME;
            Assert.Throws<Exception>(() => property.AsLong());
        }

        [Test]
        public void AsFloatTest()
        {
            float value = 100;
            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            property.SetValue(value);
            float result = property.AsFloat();
            Assert.AreEqual(value, result);

            this.property.Id = ModelCode.IDOBJ_NAME;
            Assert.Throws<Exception>(() => property.AsFloat());
        }

        [Test]
        public void AsStringTest()
        {
            string value = null;
            this.property.Id = ModelCode.IDOBJ_NAME;
            property.SetValue(value);
            string result = property.AsString();
            Assert.AreEqual(string.Empty, result);

            value = "IDOBJ";
            this.property.Id = ModelCode.IDOBJ_NAME;
            property.SetValue(value);
            result = property.AsString();
            Assert.AreEqual(value, result);

            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.Throws<Exception>(() => property.AsString());
        }

        [Test]
        public void AsReferenceTest()
        {
            long value = 8273627645382;
            this.property.Id = ModelCode.MEASUREMENT_PSR;
            property.SetValue(value);
            long result = property.AsReference();
            Assert.AreEqual(value, result);

            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.Throws<Exception>(() => property.AsReference());
        }

        [Test]
        public void AsReferencesTest()
        {
            List<long> value = new List<long>() { 837437837463, 5933625738 };
            this.property.Id = ModelCode.GEOREGION_SUBGEOREGIONS;
            property.SetValue(value);
            List<long> result = property.AsReferences();
            Assert.AreEqual(value, result);

            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            Assert.Throws<Exception>(() => property.AsReferences());
        }

        [Test]
        public void IsCompatibleWithTest()
        {
            Property newProp = new Property();
            newProp.Id = ModelCode.ENERGYCONS_PMAX;
            newProp.PropertyValue = new PropertyValue(100);
            PropertyType newType = newProp.Type;
            bool result = property.IsCompatibleWith(newType);
            Assert.AreEqual(true, result);

            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            newProp.Id = ModelCode.IDOBJ_GID;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(true, result);
            newProp.Id = ModelCode.MEASUREMENT_DIRECTION;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(true, result);
            newProp.Id = ModelCode.BASEVOLTAGE_CONDEQS;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(false, result);

            this.property.Id = ModelCode.IDOBJ_GID;
            newProp.Id = ModelCode.ENERGYCONS_PMAX;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(true, result);
            newProp.Id = ModelCode.MEASUREMENT_DIRECTION;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(true, result);
            newProp.Id = ModelCode.BASEVOLTAGE_CONDEQS;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(false, result);

            this.property.Id = ModelCode.MEASUREMENT_DIRECTION;
            newProp.Id = ModelCode.IDOBJ_GID;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(true, result);
            newProp.Id = ModelCode.ENERGYCONS_PMAX;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(true, result);
            newProp.Id = ModelCode.BASEVOLTAGE_CONDEQS;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(false, result);

            this.property.Id = ModelCode.BASEVOLTAGE_CONDEQS;
            newProp.Id = ModelCode.IDOBJ_GID;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(false, result);
            newProp.Id = ModelCode.ENERGYCONS_PMAX;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(false, result);
            newProp.Id = ModelCode.MEASUREMENT_DIRECTION;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(false, result);

            this.property.Id = ModelCode.ENERGYCONS_PMAX;
            newProp.Id = ModelCode.BASEVOLTAGE_CONDEQS;
            newType = newProp.Type;
            result = property.IsCompatibleWith(newType);
            Assert.AreEqual(false, result);
        }
    }
}
