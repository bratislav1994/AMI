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
    public class VoltageLevelTest
    {
        private VoltageLevel voltageLevel;
        private long baseVoltage = 42949672153;
        private long substation = 42949672163;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.voltageLevel = new VoltageLevel();
            this.voltageLevel.BaseVoltage = baseVoltage;
            this.voltageLevel.Substation = substation;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new VoltageLevel());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new VoltageLevel(42949672123));
        }

        [Test]
        public void BaseVoltageTest()
        {
            voltageLevel.BaseVoltage = baseVoltage;

            Assert.AreEqual(baseVoltage, voltageLevel.BaseVoltage);
        }

        [Test]
        public void SubstationTest()
        {
            voltageLevel.Substation = substation;

            Assert.AreEqual(substation, voltageLevel.Substation);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.voltageLevel;
            bool result = voltageLevel.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = voltageLevel.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_BASEVOLTAGE)]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = voltageLevel.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = voltageLevel.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_BASEVOLTAGE)]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => voltageLevel.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => voltageLevel.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_BASEVOLTAGE)]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.VOLTAGELEVEL_BASEVOLTAGE:
                    property.SetValue(baseVoltage);
                    break;
                case ModelCode.VOLTAGELEVEL_SUBSTATION:
                    property.SetValue(substation);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("VoltageLevel_1");
                    break;
            }

            Assert.DoesNotThrow(() => voltageLevel.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => voltageLevel.SetProperty(property));
        }

        [Test]
        [TestCase(TypeOfReference.Reference)]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => voltageLevel.GetReferences(references, refType));
        }
    }
}
