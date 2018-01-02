using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Wires;

namespace DataModelTest.WiresTest
{
    [TestFixture]
    public class TapChangerTest
    {
        private TapChanger tapChanger;
        private int highStep = 15;
        private int lowStep = -15;
        private int neutralStep = 0;
        private int normalStep = 0;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.tapChanger = new TapChanger();
            this.tapChanger.HighStep = highStep;
            this.tapChanger.LowStep = lowStep;
            this.tapChanger.NeutralStep = neutralStep;
            this.tapChanger.NormalStep = normalStep;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new TapChanger());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new TapChanger(42949672111));
        }

        [Test]
        public void HighStepTest()
        {
            tapChanger.HighStep = highStep;

            Assert.AreEqual(highStep, tapChanger.HighStep);
        }

        [Test]
        public void LowStepTest()
        {
            tapChanger.LowStep = lowStep;

            Assert.AreEqual(lowStep, tapChanger.LowStep);
        }

        [Test]
        public void NeutralStepTest()
        {
            tapChanger.NeutralStep = neutralStep;

            Assert.AreEqual(neutralStep, tapChanger.NeutralStep);
        }

        [Test]
        public void NormalStepTest()
        {
            tapChanger.NormalStep = normalStep;

            Assert.AreEqual(normalStep, tapChanger.NormalStep);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.tapChanger;
            bool result = tapChanger.Equals(obj);

            Assert.AreEqual(true, result);

            // incorrect
            obj = new TapChanger() { HighStep = 1 };
            result = tapChanger.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new TapChanger() { LowStep = 1 };
            result = tapChanger.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new TapChanger() { NeutralStep = 1 };
            result = tapChanger.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new TapChanger() { NormalStep = 1 };
            result = tapChanger.Equals(obj);
            Assert.AreNotEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = tapChanger.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.TAPCHANGER_HIGHSTEP)]
        [TestCase(ModelCode.TAPCHANGER_LOWSTEP)]
        [TestCase(ModelCode.TAPCHANGER_NEUTRALSTEP)]
        [TestCase(ModelCode.TAPCHANGER_NORMALSTEP)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = tapChanger.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = tapChanger.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.TAPCHANGER_HIGHSTEP)]
        [TestCase(ModelCode.TAPCHANGER_LOWSTEP)]
        [TestCase(ModelCode.TAPCHANGER_NEUTRALSTEP)]
        [TestCase(ModelCode.TAPCHANGER_NORMALSTEP)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => tapChanger.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => tapChanger.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.TAPCHANGER_HIGHSTEP)]
        [TestCase(ModelCode.TAPCHANGER_LOWSTEP)]
        [TestCase(ModelCode.TAPCHANGER_NEUTRALSTEP)]
        [TestCase(ModelCode.TAPCHANGER_NORMALSTEP)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.TAPCHANGER_HIGHSTEP:
                    property.SetValue(highStep);
                    break;
                case ModelCode.TAPCHANGER_LOWSTEP:
                    property.SetValue(lowStep);
                    break;
                case ModelCode.TAPCHANGER_NEUTRALSTEP:
                    property.SetValue(neutralStep);
                    break;
                case ModelCode.TAPCHANGER_NORMALSTEP:
                    property.SetValue(normalStep);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("TapChanger_1");
                    break;
            }

            Assert.DoesNotThrow(() => tapChanger.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => tapChanger.SetProperty(property));
        }
    }
}
