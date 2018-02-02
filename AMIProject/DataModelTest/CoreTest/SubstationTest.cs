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
    public class SubstationTest
    {
        private Substation substation;
        private long globalID = 42949682351;
        private long subGeoRegion = 42949682352;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.substation = new Substation();
            this.substation.SubGeoRegion = subGeoRegion;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Substation());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new Substation(globalID));
        }

        [Test]
        public void SubGeoRegionTest()
        {
            substation.SubGeoRegion = subGeoRegion;

            Assert.AreEqual(subGeoRegion, substation.SubGeoRegion);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.substation;
            bool result = substation.Equals(obj);

            Assert.AreEqual(true, result);

            // incorrect
            obj = new Substation() { SubGeoRegion = 100 };
            result = substation.Equals(obj);
            Assert.AreNotEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = substation.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void ToStringTest()
        {
            substation.Mrid = "SUBSTATION_1";
            substation.Name = "Substation_1";
            string region = string.Format(substation.Mrid + "\n" + substation.Name);

            string result = substation.ToString();

            Assert.AreEqual(region, result);
        }

        [Test]
        [TestCase(ModelCode.SUBSTATION_SUBGEOREGION)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = substation.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ENERGYCONS_PMAX)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = substation.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.SUBSTATION_SUBGEOREGION)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => substation.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ENERGYCONS_PMAX)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => substation.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.SUBSTATION_SUBGEOREGION)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.SUBSTATION_SUBGEOREGION:
                    property.SetValue(subGeoRegion);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("Substation_1");
                    break;
            }

            Assert.DoesNotThrow(() => substation.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ENERGYCONS_PMAX, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => substation.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, substation.IsReferenced);
            Substation s = new Substation();
            Assert.AreEqual(false, s.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        [TestCase(TypeOfReference.Reference)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => substation.GetReferences(references, refType));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 42949682356)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => substation.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 42949682356)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => substation.RemoveReference(referenceId, globalId));
        }

        [Test]
        public void GetHashCodeTest()
        {
            Substation s = new Substation() { Name = "sub" };
            int hashCode = s.GetHashCode();
            Substation s2 = new Substation() { Name = "sub" };
            int hashCodeBv = s2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            s = s2;
            Assert.AreEqual(s.GetHashCode(), s2.GetHashCode());
        }

        [Test]
        public void DeepCopyTest()
        {
            Substation s = new Substation() { GlobalId = 1234, Mrid = "123" };
            Assert.AreEqual(s, s.DeepCopy());
        }
    }
}
