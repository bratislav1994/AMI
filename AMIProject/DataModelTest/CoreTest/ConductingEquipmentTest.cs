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
    public class ConductingEquipmentTest
    {
        private ConductingEquipment conductingEquipment;
        private long baseVoltage = 42949672186;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.conductingEquipment = new ConductingEquipment();
            this.conductingEquipment.BaseVoltage = baseVoltage;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new ConductingEquipment());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new ConductingEquipment(42949672126));
        }

        [Test]
        public void BaseVoltageTest()
        {
            conductingEquipment.BaseVoltage = baseVoltage;

            Assert.AreEqual(baseVoltage, conductingEquipment.BaseVoltage);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.conductingEquipment;
            bool result = conductingEquipment.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = conductingEquipment.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = conductingEquipment.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = conductingEquipment.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => conductingEquipment.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => conductingEquipment.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.CONDEQ_BASEVOLTAGE:
                    property.SetValue(baseVoltage);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("CondEquipment_1");
                    break;
            }

            Assert.DoesNotThrow(() => conductingEquipment.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => conductingEquipment.SetProperty(property));
        }

        [Test]
        [TestCase(TypeOfReference.Reference)]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => conductingEquipment.GetReferences(references, refType));
        }
    }
}
