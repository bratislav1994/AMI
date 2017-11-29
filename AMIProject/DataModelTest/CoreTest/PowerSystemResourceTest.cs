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
    public class PowerSystemResourceTest
    {
        private PowerSystemResource psr;
        private List<long> measurements = new List<long>() { 42949682111, 42949682112 };
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.psr = new PowerSystemResource();
            this.psr.Measurements = measurements;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new PowerSystemResource());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new PowerSystemResource(42949682110));
        }

        [Test]
        public void MeasurementsTest()
        {
            psr.Measurements = measurements;

            Assert.AreEqual(measurements, psr.Measurements);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.psr;
            bool result = psr.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = psr.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.PSR_MEASUREMENTS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = psr.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = psr.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.PSR_MEASUREMENTS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => psr.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => psr.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME, "PSR_1")]
        public void SetPropertyTestCorrects(ModelCode t, string value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => psr.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => psr.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, psr.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => psr.GetReferences(references, refType));
        }

        [Test]
        [TestCase(ModelCode.MEASUREMENT_PSR, 42949682113)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => psr.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682115)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<Exception>(() => psr.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.MEASUREMENT_PSR, 42949682113)]
        [TestCase(ModelCode.MEASUREMENT_PSR, 42949682114)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => psr.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682115)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<ModelException>(() => psr.RemoveReference(referenceId, globalId));
        }
    }
}
