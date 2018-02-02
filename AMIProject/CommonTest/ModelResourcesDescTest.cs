using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTest
{
    [TestFixture]
    public class ModelResourcesDescTest
    {
        private ResourcePropertiesDesc resPropDesc;
        private ModelCode resourceId;
        private string resourceName;
        //private Dictionary<ModelCode, string> propertyIds = new Dictionary<ModelCode, string>(new ModelCodeComparer());
        IEnumerable<ModelCode> propertyIds;

        private ModelResourcesDesc modelResDesc;
        //private Dictionary<long, ResourcePropertiesDesc> resourceDescs;
        private Dictionary<DMSType, ModelCode> type2modelCode = new Dictionary<DMSType, ModelCode>(new DMSTypeComparer());
        private List<ModelCode> typeIdsInInsertOrder = new List<ModelCode>();
        private List<ModelCode> nonAbstractClassIds = new List<ModelCode>();
        private HashSet<ModelCode> notSettablePropertyIds = new HashSet<ModelCode>(new ModelCodeComparer());
        private Dictionary<ModelCode, bool> notAccessiblePropertyIds = new Dictionary<ModelCode, bool>(new ModelCodeComparer());
        private HashSet<ModelCode> allModelCodes = new HashSet<ModelCode>(new ModelCodeComparer());
        private HashSet<DMSType> allDMSTypes = new HashSet<DMSType>(new DMSTypeComparer());

        private DMSTypeComparer dmsTypeComparer;
        private ModelCodeComparer mcComparer;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.modelResDesc = new ModelResourcesDesc();
            nonAbstractClassIds = this.modelResDesc.NonAbstractClassIds;
            notSettablePropertyIds = this.modelResDesc.NotSettablePropertyIds;

            this.resPropDesc = new ResourcePropertiesDesc(ModelCode.ANALOG, "Analog");

            this.dmsTypeComparer = new DMSTypeComparer();
            this.mcComparer = new ModelCodeComparer();
        }

        #region ResourcePropertiesDesc

        [Test]
        public void ResourceIdTest()
        {
            resourceId = this.resPropDesc.ResourceId;

            Assert.AreEqual(resPropDesc.ResourceId, resourceId);
        }

        [Test]
        public void ResourceNameTest()
        {
            resourceName = this.resPropDesc.ResourceName;

            Assert.AreEqual(resourceName, resPropDesc.ResourceName);
        }

        [Test]
        public void PropertyIdsTest()
        {
            propertyIds = this.resPropDesc.PropertyIds;

            Assert.AreEqual(propertyIds, resPropDesc.PropertyIds);
        }

        [Test]
        public void AddPropertyIdTest()
        {
            Assert.DoesNotThrow(() => resPropDesc.AddPropertyId(resourceId));
            Assert.DoesNotThrow(() => resPropDesc.AddPropertyId(resourceId, resourceName));
        }

        [Test]
        public void PropertyExistsTest()
        {
            bool result = resPropDesc.PropertyExists(resourceId);

            Assert.AreEqual(true, result);
        }

        #endregion

        #region ModelResourcesDesc

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new ModelResourcesDesc());
        }

        [Test]
        public void TypeIdsInInsertOrderTest()
        {
            typeIdsInInsertOrder = this.modelResDesc.TypeIdsInInsertOrder;

            Assert.AreEqual(typeIdsInInsertOrder, modelResDesc.TypeIdsInInsertOrder);
        }

        [Test]
        public void NonAbstractClassIdsTest()
        {
            this.modelResDesc.NonAbstractClassIds = nonAbstractClassIds;

            Assert.AreEqual(nonAbstractClassIds, modelResDesc.NonAbstractClassIds);
        }

        [Test]
        public void NotSettablePropertyIdsTest()
        {
            this.modelResDesc.NotSettablePropertyIds = notSettablePropertyIds;

            Assert.AreEqual(notSettablePropertyIds, modelResDesc.NotSettablePropertyIds);
        }

        [Test]
        public void GetTypeFromModelCodeTest()
        {
            DMSType result = ModelResourcesDesc.GetTypeFromModelCode(ModelCode.ANALOG);

            Assert.AreEqual(DMSType.ANALOG, result);
        }

        [Test]
        public void AddResourceDescTest()
        {
            Assert.DoesNotThrow(() => modelResDesc.AddResourceDesc(ModelCode.DISCRETE));
        }

        [Test]
        public void GetResourcePropertiesDescTest()
        {
            ResourcePropertiesDesc rpd = modelResDesc.AddResourceDesc(ModelCode.DISCRETE);
            Assert.AreEqual(rpd, modelResDesc.GetResourcePropertiesDesc(ModelCode.DISCRETE));

            //Assert.Throws<Exception>(() => modelResDesc.GetResourcePropertiesDesc(ModelCode.DISCRETE_NORMALVALUE));
        }

        [Test]
        public void GetResourceAncestorsTest()
        {
            List<ModelCode> ancestorIds = null;

            Assert.DoesNotThrow(() => modelResDesc.GetResourceAncestors(ModelCode.IDOBJ, ref ancestorIds));
            Assert.DoesNotThrow(() => modelResDesc.GetResourceAncestors(ModelCode.ANALOG, ref ancestorIds));
        }

        [Test]
        public void GetClassPropertyIdsTest()
        {
            List<ModelCode> result = modelResDesc.GetClassPropertyIds(ModelCode.BASEVOLTAGE);

            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void GetAllPropertyIdsTest()
        {
            List<ModelCode> result = modelResDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);

            Assert.AreEqual(5, result.Count);
        }

        [Test]
        public void GetAllSettablePropertyIdsTest()
        {
            List<ModelCode> result = modelResDesc.GetAllSettablePropertyIds(ModelCode.BASEVOLTAGE, false);
            Assert.AreEqual(1, result.Count);

            result = modelResDesc.GetAllSettablePropertyIds(ModelCode.IDOBJ, true);
            Assert.AreEqual(2, result.Count);

            result = modelResDesc.GetAllSettablePropertyIds(DMSType.ANALOG);
            Assert.AreEqual(14, result.Count);
        }

        //[Test]
        //public void GetAllSettablePropertyIdsForEntityIdTest()
        //{
        //    List<ModelCode> result = modelResDesc.GetAllSettablePropertyIdsForEntityId(128746374934);
        //    Assert.AreEqual(2, result.Count);
        //}

        [Test]
        public void GetModelCodeFromTypeTest()
        {
            ModelCode result = modelResDesc.GetModelCodeFromType(DMSType.ANALOG);
            Assert.AreEqual(ModelCode.ANALOG, result);

            Assert.Throws<Exception>(() => modelResDesc.GetModelCodeFromType(DMSType.MASK_TYPE));
        }

        #endregion

        #region DMSTypeComparerTest

        [Test]
        public void EqualsTest()
        {
            bool result = dmsTypeComparer.Equals(DMSType.ANALOG, DMSType.ANALOG);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.DoesNotThrow(() => dmsTypeComparer.GetHashCode(DMSType.ANALOG));
        }

        #endregion

        #region ModelCodeComparer

        [Test]
        public void ConstructorComparerTest()
        {
            Assert.DoesNotThrow(() => new ModelCodeComparer());
        }

        [Test]
        public void EqualsComparerTest()
        {
            bool result = mcComparer.Equals(ModelCode.ANALOG, ModelCode.ANALOG);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetHashCodeComparerTest()
        {
            Assert.DoesNotThrow(() => mcComparer.GetHashCode(ModelCode.ANALOG));
        }

        #endregion
    }
}
