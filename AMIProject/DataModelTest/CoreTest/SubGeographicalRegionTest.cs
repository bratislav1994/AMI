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
    public class SubGeographicalRegionTest
    {
        private SubGeographicalRegion subGeoRegion;
        private long globalID = 42949682341;
        private long geoRegion = 42949682342;
        private List<long> substations = new List<long>() { 42949682343, 42949682344 };
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.subGeoRegion = new SubGeographicalRegion();
            this.subGeoRegion.GeoRegion = geoRegion;
            this.subGeoRegion.Substations = substations;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new SubGeographicalRegion());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new SubGeographicalRegion(globalID));
        }

        [Test]
        public void GeoRegionTest()
        {
            subGeoRegion.GeoRegion = geoRegion;

            Assert.AreEqual(geoRegion, subGeoRegion.GeoRegion);
        }

        [Test]
        public void SubstationsTest()
        {
            subGeoRegion.Substations = substations;

            Assert.AreEqual(substations, subGeoRegion.Substations);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.subGeoRegion;
            bool result = subGeoRegion.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = subGeoRegion.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void ToStringTest()
        {
            subGeoRegion.Mrid = "SREM";
            subGeoRegion.Name = "Srem";
            string region = string.Format(subGeoRegion.Mrid + "\n" + subGeoRegion.Name);

            string result = subGeoRegion.ToString();

            Assert.AreEqual(region, result);
        }

        [Test]
        [TestCase(ModelCode.SUBGEOREGION_GEOREG)]
        [TestCase(ModelCode.SUBGEOREGION_SUBS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = subGeoRegion.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = subGeoRegion.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.SUBGEOREGION_GEOREG)]
        [TestCase(ModelCode.SUBGEOREGION_SUBS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => subGeoRegion.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => subGeoRegion.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.SUBGEOREGION_GEOREG)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.SUBGEOREGION_GEOREG:
                    property.SetValue(geoRegion);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("Srem");
                    break;
            }

            Assert.DoesNotThrow(() => subGeoRegion.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => subGeoRegion.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, subGeoRegion.IsReferenced);
            SubGeographicalRegion gr = new SubGeographicalRegion();
            Assert.AreEqual(false, gr.IsReferenced);
            gr.Substations = substations;
            Assert.AreEqual(true, gr.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        [TestCase(TypeOfReference.Reference)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => subGeoRegion.GetReferences(references, refType));
        }

        [Test]
        [TestCase(ModelCode.SUBSTATION_SUBGEOREGION, 42949682345)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => subGeoRegion.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682346)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<Exception>(() => subGeoRegion.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.SUBSTATION_SUBGEOREGION, 42949682345)]
        [TestCase(ModelCode.SUBSTATION_SUBGEOREGION, 42949682347)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => subGeoRegion.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682346)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<ModelException>(() => subGeoRegion.RemoveReference(referenceId, globalId));
        }

        [Test]
        public void RD2ClassTest()
        {
            ResourceDescription rd = new ResourceDescription(globalID);

            ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION_SUBS);

            for (int i = 0; i < properties.Count; i++)
            {
                rd.AddProperty(new Property(properties[i]));
            }

            //rd.AddProperty(new Property() { Id = ModelCode.SUBSTATION_VOLTLEVELS, PropertyValue = new PropertyValue() { LongValue = 324 } });
            rd.Properties.First().PropertyValue.LongValue = 424;
            Assert.DoesNotThrow(() => subGeoRegion.RD2Class(rd));
        }

        [Test]
        public void GetHashCodeTest()
        {
            SubGeographicalRegion gr = new SubGeographicalRegion() { Name = "geo" };
            int hashCode = gr.GetHashCode();
            SubGeographicalRegion gr2 = new SubGeographicalRegion() { Name = "geo" };
            int hashCodeBv = gr2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            gr = gr2;
            Assert.AreEqual(gr.GetHashCode(), gr2.GetHashCode());
        }
    }
}
