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
    public class EquipmentTest
    {
        private Equipment equipment;
        private long eqContainer = 42949672174;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.equipment = new Equipment();
            this.equipment.EqContainer = eqContainer;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Equipment());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new Equipment(42949672124));
        }

        [Test]
        public void EqContainerTest()
        {
            equipment.EqContainer = eqContainer;

            Assert.AreEqual(eqContainer, equipment.EqContainer);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.equipment;
            bool result = equipment.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = equipment.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.EQUIPMENT_EQCONTAINER)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = equipment.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = equipment.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.EQUIPMENT_EQCONTAINER)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => equipment.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => equipment.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.EQUIPMENT_EQCONTAINER)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.EQUIPMENT_EQCONTAINER:
                    property.SetValue(eqContainer);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("Equipment_1");
                    break;
            }

            Assert.DoesNotThrow(() => equipment.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => equipment.SetProperty(property));
        }

        [Test]
        [TestCase(TypeOfReference.Reference)]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => equipment.GetReferences(references, refType));
        }
    }
}
