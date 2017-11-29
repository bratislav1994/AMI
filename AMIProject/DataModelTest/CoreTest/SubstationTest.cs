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
        private List<long> voltageLevels = new List<long>() { 42949682353, 42949682354};
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.substation = new Substation();
            this.substation.SubGeoRegion = subGeoRegion;
            this.substation.VoltageLevels = voltageLevels;
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
        public void VoltageLevelsTest()
        {
            substation.VoltageLevels = voltageLevels;

            Assert.AreEqual(voltageLevels, substation.VoltageLevels);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.substation;
            bool result = substation.Equals(obj);

            Assert.AreEqual(true, result);
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
        [TestCase(ModelCode.SUBSTATION_VOLTLEVELS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = substation.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = substation.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.SUBSTATION_SUBGEOREGION)]
        [TestCase(ModelCode.SUBSTATION_VOLTLEVELS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => substation.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => substation.GetProperty(property));
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
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => substation.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, substation.IsReferenced);
            Substation s = new Substation();
            Assert.AreEqual(false, s.IsReferenced);
            s.VoltageLevels = voltageLevels;
            Assert.AreEqual(true, s.IsReferenced);
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
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682355)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => substation.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 42949682356)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<Exception>(() => substation.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682355)]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682357)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => substation.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 42949682356)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<ModelException>(() => substation.RemoveReference(referenceId, globalId));
        }

        [Test]
        public void RD2ClassTest()
        {
            ResourceDescription rd = new ResourceDescription(globalID);

            ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBSTATION);

            for(int i=0; i<properties.Count; i++)
            {
                rd.AddProperty(new Property(properties[i]));
            }

            rd.AddProperty(new Property() { Id = ModelCode.SUBSTATION_SUBGEOREGION, PropertyValue = new PropertyValue() { LongValue = 7890 } });
            //rd.Properties.First().PropertyValue.LongValue = 6378;
            Assert.DoesNotThrow(() => substation.RD2Class(rd));
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
    }
}
