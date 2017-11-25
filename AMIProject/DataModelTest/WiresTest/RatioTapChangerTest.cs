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
    public class RatioTapChangerTest
    {
        private RatioTapChanger ratioTapChanger;
        private long transformerEnd = 0;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.ratioTapChanger = new RatioTapChanger();
            this.ratioTapChanger.TransformerEnd = transformerEnd;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new RatioTapChanger());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new RatioTapChanger(0));
        }

        [Test]
        public void TransformerEndTest()
        {
            ratioTapChanger.TransformerEnd = transformerEnd;

            Assert.AreEqual(transformerEnd, ratioTapChanger.TransformerEnd);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.ratioTapChanger;
            bool result = ratioTapChanger.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.RATIOTAPCHANGER_TRANSEND)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = ratioTapChanger.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = ratioTapChanger.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.RATIOTAPCHANGER_TRANSEND)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => ratioTapChanger.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => ratioTapChanger.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.RATIOTAPCHANGER_TRANSEND)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.RATIOTAPCHANGER_TRANSEND:
                    property.SetValue(transformerEnd);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("RatioTapChanger_1");
                    break;
            }

            Assert.DoesNotThrow(() => ratioTapChanger.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => ratioTapChanger.SetProperty(property));
        }

        [Test]
        [TestCase(TypeOfReference.Reference)]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => ratioTapChanger.GetReferences(references, refType));
        }
    }
}
