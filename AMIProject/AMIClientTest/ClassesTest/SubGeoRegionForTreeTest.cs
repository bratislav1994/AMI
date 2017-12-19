using AMIClient;
using FTN.Common;
using FTN.ServiceContracts;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClientTest.ClassesTest
{
    [TestFixture]
    public class SubGeoRegionForTreeTest
    {
        private SubGeoRegionForTree subGeoRegionForTree;
        private RootElement root;
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private Dictionary<long, TreeClasses> allTreeEl = new Dictionary<long, TreeClasses>();
        private Model model;
        private GeoRegionForTree parent = new GeoRegionForTree();
        private SubGeographicalRegion subGeoRegion;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

        public void SetupTest()
        {
            amis = new ObservableCollection<EnergyConsumer>();
            subGeoRegion = new SubGeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new GeoRegionForTree();
            subGeoRegionForTree = new SubGeoRegionForTree();
            subGeoRegionForTree.SubGeoRegion = subGeoRegion;
            root = new RootElement();

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            List<ModelCode> propertiesGeoregion = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            ResourceDescription rd1 = new ResourceDescription();
            rd1.AddProperty(new Property(ModelCode.IDOBJ_GID, 4321));
            List<ResourceDescription> ret = new List<ResourceDescription>() { rd1 };
            List<ModelCode> propertiesSubregion = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            Association associtaion = new Association();
            /*associtaion.PropertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
            associtaion.Type = ModelCode.SUBGEOREGION;*/
            mock2.GetExtentValues(ModelCode.GEOREGION, propertiesGeoregion).Returns(2);
            mock2.GetRelatedValues(1234, propertiesSubregion, associtaion).ReturnsForAnyArgs(2);
            //mock2.IteratorResourcesLeft(2).Returns(x => (count++ < 1) ? 1 : 0);
            mock2.IteratorNext(10, 2).Returns(ret);
            mock2.IteratorClose(2);
            subGeoRegionForTree.Model = new Model();
            subGeoRegionForTree.Model.FirstContact = false;
            subGeoRegionForTree.Model.GdaQueryProxy = mock2;
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
        }

        [Test]
        public void ConstructorTest()
        {
            SetupTest();
            Assert.DoesNotThrow(() =>
                subGeoRegionForTree = new SubGeoRegionForTree(subGeoRegion, parent, model, ref allTreeEl)
            );
        }

        [Test]
        public void NameTest()
        {
            this.SetupTest();
            Assert.AreEqual(subGeoRegionForTree.Name, "geo");
        }

        [Test]
        public void CheckIfSeleactedTest()
        {
            int countForResourcesLeft1 = 0;
            int countForResourcesLeft2 = 0;
            int countForResourcesLeft3 = 0;
            int countForResourcesLeft4= 0;
            int countForRelatedValues = 0;

            subGeoRegion = new SubGeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new GeoRegionForTree();
            subGeoRegionForTree = new SubGeoRegionForTree();
            subGeoRegionForTree.SubGeoRegion = subGeoRegion;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subStation = new ResourceDescription();
            ResourceDescription ami = new ResourceDescription();
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subStation };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft2++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(2).Returns(x => (countForResourcesLeft3++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(3).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(4).Returns(x => (countForResourcesLeft4++ < 1) ? 1 : 0);
            mock2.IteratorNext(10, 1).Returns(ret1);
            mock2.IteratorNext(10, 2).Returns(ret2);
            mock2.IteratorNext(10, 3).Returns(ret1);
            mock2.IteratorNext(10, 4).Returns(ret2);
            mock2.IteratorClose(1);
            mock2.IteratorClose(2);
            mock2.IteratorClose(3);
            mock2.IteratorClose(4);
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;

            this.subGeoRegionForTree.Model = model;
            this.subGeoRegionForTree.SubGeoRegion = subGeoRegion;

            this.subGeoRegionForTree.IsSelected = false;
            this.subGeoRegionForTree.IsSelected = true;
            Assert.DoesNotThrow(() => this.subGeoRegionForTree.CheckIfSeleacted());
        }

        [Test]
        public void LoadChildrenTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            subGeoRegion = new SubGeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new GeoRegionForTree();
            subGeoRegionForTree = new SubGeoRegionForTree();
            subGeoRegionForTree.SubGeoRegion = subGeoRegion;
            subGeoRegionForTree.AllTreeElements = allTreeEl;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription substation1 = new ResourceDescription();
            substation1.AddProperty(new Property(ModelCode.IDOBJ_GID, 4321));
            List<ResourceDescription> ret = new List<ResourceDescription>() { substation1 };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorNext(10, 1).Returns(ret);
            mock2.IteratorClose(1);
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;

            this.subGeoRegionForTree.Model = model;
            this.subGeoRegionForTree.SubGeoRegion = subGeoRegion;

            Assert.DoesNotThrow(() => this.subGeoRegionForTree.LoadChildren());
        }

        [Test]
        public void IsExpandedTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            subGeoRegion = new SubGeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new GeoRegionForTree();
            SetForParentIsExpanded();
            subGeoRegionForTree = new SubGeoRegionForTree(subGeoRegion, parent, model, ref allTreeEl);
            subGeoRegionForTree.SubGeoRegion = subGeoRegion;
            subGeoRegionForTree.AllTreeElements = allTreeEl;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription substation1 = new ResourceDescription();
            substation1.AddProperty(new Property(ModelCode.IDOBJ_GID, 4321));
            List<ResourceDescription> ret = new List<ResourceDescription>() { substation1 };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorNext(10, 1).Returns(ret);
            mock2.IteratorClose(1);
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;

            this.subGeoRegionForTree.Model = model;
            this.subGeoRegionForTree.SubGeoRegion = subGeoRegion;

            this.subGeoRegionForTree.IsExpanded = true;
            Assert.AreEqual(subGeoRegionForTree.IsExpanded, true);
        }

        private void SetForParentIsExpanded()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            GeographicalRegion geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new GeoRegionForTree();
            parent.GeoRegion = geoRegion;
            parent.AllTreeElements = allTreeEl;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subGeoregion1 = new ResourceDescription();
            subGeoregion1.AddProperty(new Property(ModelCode.IDOBJ_GID, 4321));
            List<ResourceDescription> ret = new List<ResourceDescription>() { subGeoregion1 };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorNext(10, 1).Returns(ret);
            mock2.IteratorClose(1);
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;

            this.parent.Model = model;
            this.parent.GeoRegion = geoRegion;

            //this.parent.IsExpanded = true;
        }

        [Test]
        public void GetAllThreeElementsTest()
        {
            this.subGeoRegionForTree = new SubGeoRegionForTree();
            this.subGeoRegionForTree.AllTreeElements = new Dictionary<long, TreeClasses>();

            Assert.IsNotNull(this.subGeoRegionForTree.AllTreeElements);
        }
    }
}
