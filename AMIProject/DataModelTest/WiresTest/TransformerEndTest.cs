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
    public class TransformerEndTest
    {
        private TransformerEnd transformerEnd;
        private long baseVoltage = 42949682232;
        private List<long> ratioTapChanger = new List<long>() { 42949682233, 42949682234 };
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.transformerEnd = new TransformerEnd();
            this.transformerEnd.BaseVoltage = baseVoltage;
            this.transformerEnd.RatioTapChanger = ratioTapChanger;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new TransformerEnd());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new TransformerEnd(42949682231));
        }

        [Test]
        public void BaseVoltageTest()
        {
            transformerEnd.BaseVoltage = baseVoltage;

            Assert.AreEqual(baseVoltage, transformerEnd.BaseVoltage);
        }

        [Test]
        public void RatioTapChangerTest()
        {
            transformerEnd.RatioTapChanger = ratioTapChanger;

            Assert.AreEqual(ratioTapChanger, transformerEnd.RatioTapChanger);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.transformerEnd;
            bool result = transformerEnd.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = transformerEnd.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.TRANSFORMEREND_BASEVOLT)]
        [TestCase(ModelCode.TRANSFORMEREND_RATIOTAPCHANGER)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = transformerEnd.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = transformerEnd.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.TRANSFORMEREND_BASEVOLT)]
        [TestCase(ModelCode.TRANSFORMEREND_RATIOTAPCHANGER)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => transformerEnd.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => transformerEnd.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.TRANSFORMEREND_BASEVOLT)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.TRANSFORMEREND_BASEVOLT:
                    property.SetValue(baseVoltage);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("TransformerEnd_1");
                    break;
            }
            
            Assert.DoesNotThrow(() => transformerEnd.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => transformerEnd.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, transformerEnd.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        [TestCase(TypeOfReference.Reference)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => transformerEnd.GetReferences(references, refType));
        }

        [Test]
        [TestCase(ModelCode.RATIOTAPCHANGER_TRANSEND, 42949682235)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => transformerEnd.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682236)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<Exception>(() => transformerEnd.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.RATIOTAPCHANGER_TRANSEND, 42949682235)]
        [TestCase(ModelCode.RATIOTAPCHANGER_TRANSEND, 42949682237)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => transformerEnd.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682236)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<ModelException>(() => transformerEnd.RemoveReference(referenceId, globalId));
        }
    }
}
