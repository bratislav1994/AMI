using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace DataModelTest.WiresTest
{
    [TestFixture]
    public class PowerTransformerEndTest
    {
        private PowerTransformerEnd powerTransformerEnd;
        private long powerTrans = 42949672142;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.powerTransformerEnd = new PowerTransformerEnd();
            this.powerTransformerEnd.PowerTrans = powerTrans;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new PowerTransformerEnd());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new PowerTransformerEnd(42949672122));
        }

        [Test]
        public void TransformerEndTest()
        {
            powerTransformerEnd.PowerTrans = powerTrans;

            Assert.AreEqual(powerTrans, powerTransformerEnd.PowerTrans);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.powerTransformerEnd;
            bool result = powerTransformerEnd.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = powerTransformerEnd.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.POWERTRANSEND_POWERTRANSF)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = powerTransformerEnd.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = powerTransformerEnd.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.POWERTRANSEND_POWERTRANSF)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => powerTransformerEnd.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => powerTransformerEnd.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.POWERTRANSEND_POWERTRANSF)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.POWERTRANSEND_POWERTRANSF:
                    property.SetValue(powerTrans);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("PowerTransformerEnd_1");
                    break;
            }

            Assert.DoesNotThrow(() => powerTransformerEnd.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => powerTransformerEnd.SetProperty(property));
        }

        [Test]
        [TestCase(TypeOfReference.Reference)]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => powerTransformerEnd.GetReferences(references, refType));
        }

        [Test]
        public void GetHashCodeTest()
        {
            PowerTransformerEnd pt = new PowerTransformerEnd() { Name = "pte" };
            int hashCode = pt.GetHashCode();
            PowerTransformerEnd pt2 = new PowerTransformerEnd() { Name = "pte" };
            int hashCodeBv = pt2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            pt = pt2;
            Assert.AreEqual(pt.GetHashCode(), pt2.GetHashCode());
        }
    }
}
