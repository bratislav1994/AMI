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
    public class BaseVoltageTest
    {
        private BaseVoltage baseVoltage;
        private float nominalVoltage = 15000;
        private List<long> conductingEquipments = new List<long>() { 42949672811, 42949672812 };
        private List<long> transformerEnds = new List<long>() { 42949672821, 42949672822 };
        private List<long> voltageLevels = new List<long>() { 42949672831, 42949672832 };
        public Property property = new Property();


        [OneTimeSetUp]
        public void SetupTest()
        {
            this.baseVoltage = new BaseVoltage();
            this.baseVoltage.NominalVoltage = nominalVoltage;
            this.baseVoltage.ConductingEquipments = conductingEquipments;
            this.baseVoltage.TransformerEnds = transformerEnds;
            this.baseVoltage.VoltageLevels = voltageLevels;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new BaseVoltage());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new BaseVoltage(42949672711));
        }

        [Test]
        public void NominalVoltageTest()
        {
            baseVoltage.NominalVoltage = nominalVoltage;

            Assert.AreEqual(nominalVoltage, baseVoltage.NominalVoltage);
        }

        [Test]
        public void ConductingEquipmentsTest()
        {
            baseVoltage.ConductingEquipments = conductingEquipments;

            Assert.AreEqual(conductingEquipments, baseVoltage.ConductingEquipments);
        }

        [Test]
        public void TransformerEndsTest()
        {
            baseVoltage.TransformerEnds = transformerEnds;

            Assert.AreEqual(transformerEnds, baseVoltage.TransformerEnds);
        }

        [Test]
        public void voltageLevelsTest()
        {
            baseVoltage.VoltageLevels = voltageLevels;

            Assert.AreEqual(voltageLevels, baseVoltage.VoltageLevels);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.baseVoltage;
            bool result = baseVoltage.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = baseVoltage.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        [TestCase(ModelCode.BASEVOLTAGE_CONDEQS)]
        [TestCase(ModelCode.BASEVOLTAGE_TRANSENDS)]
        [TestCase(ModelCode.BASEVOLTAGE_VOLTLEVELS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = baseVoltage.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = baseVoltage.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        [TestCase(ModelCode.BASEVOLTAGE_CONDEQS)]
        [TestCase(ModelCode.BASEVOLTAGE_TRANSENDS)]
        [TestCase(ModelCode.BASEVOLTAGE_VOLTLEVELS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => baseVoltage.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => baseVoltage.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.BASEVOLTAGE_NOMINALVOL:
                    property.SetValue(nominalVoltage);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("BaseVoltage_1");
                    break;
            }

            Assert.DoesNotThrow(() => baseVoltage.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => baseVoltage.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, baseVoltage.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => baseVoltage.GetReferences(references, refType));
        }

        [Test]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE, 42949672813)]
        [TestCase(ModelCode.TRANSFORMEREND_BASEVOLT, 42949672823)]
        [TestCase(ModelCode.VOLTAGELEVEL_BASEVOLTAGE, 42949672833)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => baseVoltage.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949672843)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<Exception>(() => baseVoltage.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE, 42949672813)]
        [TestCase(ModelCode.TRANSFORMEREND_BASEVOLT, 42949672823)]
        [TestCase(ModelCode.VOLTAGELEVEL_BASEVOLTAGE, 42949672833)]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE, 42949672814)]
        [TestCase(ModelCode.TRANSFORMEREND_BASEVOLT, 42949672824)]
        [TestCase(ModelCode.VOLTAGELEVEL_BASEVOLTAGE, 42949672834)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => baseVoltage.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949672843)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<ModelException>(() => baseVoltage.RemoveReference(referenceId, globalId));
        }
    }
}
