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
        public Property property = new Property();


        [OneTimeSetUp]
        public void SetupTest()
        {
            this.baseVoltage = new BaseVoltage();
            this.baseVoltage.NominalVoltage = nominalVoltage;
            this.baseVoltage.ConductingEquipments = conductingEquipments;
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
        public void EqualsTestCorrect()
        {
            object obj = this.baseVoltage;
            bool result = baseVoltage.Equals(obj);

            Assert.AreEqual(true, result);

            // incorrect
            obj = new BaseVoltage() { NominalVoltage = 100 };
            result = baseVoltage.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new BaseVoltage() { ConductingEquipments = new List<long>() };
            result = baseVoltage.Equals(obj);
            Assert.AreNotEqual(true, result);
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
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = baseVoltage.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ENERGYCONS_PMAX)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = baseVoltage.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        [TestCase(ModelCode.BASEVOLTAGE_CONDEQS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => baseVoltage.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ENERGYCONS_PMAX)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => baseVoltage.GetProperty(property));
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
        [TestCase(ModelCode.ENERGYCONS_PMAX, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => baseVoltage.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, baseVoltage.IsReferenced);
            BaseVoltage bv = new BaseVoltage();
            Assert.AreEqual(false, bv.IsReferenced);
            bv.ConductingEquipments = conductingEquipments;
            Assert.AreEqual(true, bv.IsReferenced);
            bv = new BaseVoltage();
            Assert.AreEqual(true, bv.IsReferenced);
            bv = new BaseVoltage();
            Assert.AreEqual(true, bv.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => baseVoltage.GetReferences(references, refType));
            Assert.DoesNotThrow(() => baseVoltage.GetReferences(references, TypeOfReference.Both));

            BaseVoltage bv = new BaseVoltage() { ConductingEquipments = null};
            Assert.DoesNotThrow(() => baseVoltage.GetReferences(references, refType));
            bv.ConductingEquipments = new List<long>();
            Assert.DoesNotThrow(() => baseVoltage.GetReferences(references, refType));
            bv.ConductingEquipments = conductingEquipments;
            Assert.DoesNotThrow(() => baseVoltage.GetReferences(references, refType));
            Assert.DoesNotThrow(() => baseVoltage.GetReferences(references, TypeOfReference.Reference));
        }

        [Test]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE, 42949672813)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => baseVoltage.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE, 42949672813)]
        [TestCase(ModelCode.CONDEQ_BASEVOLTAGE, 42949672814)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => baseVoltage.RemoveReference(referenceId, globalId));
        }

        [Test]
        public void GetHashCodeTest()
        {
            BaseVoltage bv = new BaseVoltage() { NominalVoltage = 100 };
            int hashCode = bv.GetHashCode();
            BaseVoltage bv2 = new BaseVoltage() { NominalVoltage = 100 };
            int hashCodeBv = bv2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            bv = bv2;
            Assert.AreEqual(bv.GetHashCode(), bv2.GetHashCode());
        }

        [Test]
        public void DeepCopyTest()
        {
            BaseVoltage bv = new BaseVoltage() { GlobalId = 1234, Mrid = "123"};
            Assert.AreEqual(bv, bv.DeepCopy());
        }
    }
}
