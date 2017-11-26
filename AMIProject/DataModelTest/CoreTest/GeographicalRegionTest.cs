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
            Assert.DoesNotThrow(() => new GeographicalRegion(42949682331));
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

            Assert.Throws<Exception>(() => geoRegion.GetProperty(property));
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

            Assert.Throws<Exception>(() => geoRegion.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, geoRegion.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => geoRegion.GetReferences(references, refType));
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
            Assert.Throws<Exception>(() => geoRegion.AddReference(referenceId, globalId));
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
            Assert.Throws<ModelException>(() => geoRegion.RemoveReference(referenceId, globalId));
        }
    }
}
