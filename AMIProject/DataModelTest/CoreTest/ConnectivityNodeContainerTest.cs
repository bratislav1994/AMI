using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace DataModelTest.CoreTest
{
    [TestFixture]
    public class ConnectivityNodeContainerTest
    {
        private ConnectivityNodeContainer conNodeContainer;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.conNodeContainer = new ConnectivityNodeContainer();
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new ConnectivityNodeContainer());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new ConnectivityNodeContainer(42949672125));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = conNodeContainer.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = conNodeContainer.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => conNodeContainer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => conNodeContainer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME, "ConNodeContainer_1")]
        public void SetPropertyTestCorrects(ModelCode t, string value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => conNodeContainer.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => conNodeContainer.SetProperty(property));
        }

        [Test]
        public void GetHashCodeTest()
        {
            ConnectivityNodeContainer cnc = new ConnectivityNodeContainer() { Name = "cnc" };
            int hashCode = cnc.GetHashCode();
            ConnectivityNodeContainer cnc2 = new ConnectivityNodeContainer() { Name = "cnc" };
            int hashCodeBv = cnc2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            cnc = cnc2;
            Assert.AreEqual(cnc.GetHashCode(), cnc.GetHashCode());
        }
    }
}
