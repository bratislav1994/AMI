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
    public class PowerTransformerTest
    {
        private PowerTransformer powerTransformer;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.powerTransformer = new PowerTransformer();
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new PowerTransformer());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new PowerTransformer(42949682121));
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.powerTransformer;
            bool result = powerTransformer.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = powerTransformer.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = powerTransformer.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => powerTransformer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME, "PowerTransformer_1")]
        public void SetPropertyTestCorrects(ModelCode t, string value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => powerTransformer.SetProperty(property));
        }

        [Test]
        public void GetHashCodeTest()
        {
            PowerTransformer pt = new PowerTransformer() { Name = "pt" };
            int hashCode = pt.GetHashCode();
            PowerTransformer pt2 = new PowerTransformer() { Name = "pt" };
            int hashCodeBv = pt2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            pt = pt2;
            Assert.AreEqual(pt.GetHashCode(), pt2.GetHashCode());
        }

        [Test]
        public void DeepCopyTest()
        {
            PowerTransformer pte = new PowerTransformer() { GlobalId = 1234, Mrid = "123" };
            Assert.AreEqual(pte, pte.DeepCopy());
        }
    }
}
