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
    public class GeographicalRegionTest
    {
        private GeographicalRegion geoRegion;
        private long globalID = 42949682331;
        private List<long> subGeoRegions = new List<long>() { 42949682332, 42949682333 };
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.geoRegion = new GeographicalRegion();
            this.geoRegion.SubGeoRegions = subGeoRegions;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new GeographicalRegion());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new GeographicalRegion(globalID));
        }

        [Test]
        public void SubGeoRegionsTest()
        {
            geoRegion.SubGeoRegions = subGeoRegions;

            Assert.AreEqual(subGeoRegions, geoRegion.SubGeoRegions);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.geoRegion;
            bool result = geoRegion.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = geoRegion.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void ToStringTest()
        {
            geoRegion.Mrid = "VOJVODINA";
            geoRegion.Name = "Vojvodina";
            string region = string.Format(geoRegion.Mrid + "\n" + geoRegion.Name);

            string result = geoRegion.ToString();

            Assert.AreEqual(region, result);
        }

        [Test]
        [TestCase(ModelCode.GEOREGION_SUBGEOREGIONS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = geoRegion.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = geoRegion.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.GEOREGION_SUBGEOREGIONS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => geoRegion.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => geoRegion.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME, "Vojvodina")]
        public void SetPropertyTestCorrects(ModelCode t, string value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => geoRegion.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => geoRegion.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, geoRegion.IsReferenced);
            GeographicalRegion gr = new GeographicalRegion();
            Assert.AreEqual(false, gr.IsReferenced);
            gr.SubGeoRegions = subGeoRegions;
            Assert.AreEqual(true, gr.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => geoRegion.GetReferences(references, refType));
            Assert.DoesNotThrow(() => geoRegion.GetReferences(references, TypeOfReference.Both));

            GeographicalRegion gr = new GeographicalRegion() { SubGeoRegions = null };
            Assert.DoesNotThrow(() => gr.GetReferences(references, refType));
            gr.SubGeoRegions = new List<long>();
            Assert.DoesNotThrow(() => gr.GetReferences(references, refType));
            gr.SubGeoRegions = subGeoRegions;
            Assert.DoesNotThrow(() => gr.GetReferences(references, TypeOfReference.Reference));
        }

        [Test]
        [TestCase(ModelCode.SUBGEOREGION_GEOREG, 42949682334)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => geoRegion.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682335)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => geoRegion.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.SUBGEOREGION_GEOREG, 42949682334)]
        [TestCase(ModelCode.SUBGEOREGION_GEOREG, 42949682336)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => geoRegion.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682335)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => geoRegion.RemoveReference(referenceId, globalId));
        }

        [Test]
        public void RD2ClassTest()
        {
            ResourceDescription rd = new ResourceDescription(globalID);

            ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.GEOREGION);

            for (int i = 0; i < properties.Count; i++)
            {
                rd.AddProperty(new Property(properties[i]));
            }

            rd.Properties.First().PropertyValue.LongValue = 4245;
            Assert.DoesNotThrow(() => geoRegion.RD2Class(rd));
        }

        [Test]
        public void GetHashCodeTest()
        {
            GeographicalRegion gr = new GeographicalRegion() { Name = "geo" };
            int hashCode = gr.GetHashCode();
            GeographicalRegion gr2 = new GeographicalRegion() { Name = "geo" };
            int hashCodeBv = gr2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            gr = gr2;
            Assert.AreEqual(gr.GetHashCode(), gr2.GetHashCode());
        }

        [Test]
        public void DeepCopyTest()
        {
            GeographicalRegion gr = new GeographicalRegion() { GlobalId = 1234, Mrid = "123" };
            Assert.AreEqual(gr, gr.DeepCopy());
        }
    }
}
