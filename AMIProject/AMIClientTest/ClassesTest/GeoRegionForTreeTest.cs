using AMIClient;
using FTN.Common;
using FTN.Common.Logger;
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
    public class GeoRegionForTreeTest
    {
        private GeoRegionForTree geoRegionForTree;
        private RootElement root;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private Dictionary<long, TreeClasses> allTreeEl = new Dictionary<long, TreeClasses>();
        private Model model;
        private TreeClasses parent;
        private GeographicalRegion geoRegion;

        [OneTimeSetUp]
        public void Init()
        {
            Logger.Path = "TestClient.txt";
        }

        public void SetupTest()
        {
            geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new TreeClasses();
            geoRegionForTree = new GeoRegionForTree();
            geoRegionForTree.GeoRegion = geoRegion;
            root = new RootElement();


            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            List<ModelCode> propertiesGeoregion = modelResourcesDesc.GetAllPropertyIds(ModelCode.GEOREGION);
            ResourceDescription rd1 = new ResourceDescription();
            ResourceDescription rd2 = new ResourceDescription();
            rd1.AddProperty(new Property(ModelCode.IDOBJ_GID, 4321));
            rd2.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            List<ResourceDescription> ret = new List<ResourceDescription>() { rd1, rd2 };
            List<ModelCode> propertiesSubregion = modelResourcesDesc.GetAllPropertyIds(ModelCode.SUBGEOREGION);
            Association associtaion = new Association();
            /*associtaion.PropertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
            associtaion.Type = ModelCode.SUBGEOREGION;*/
            mock2.GetExtentValues(ModelCode.GEOREGION, propertiesGeoregion).Returns(2);
            mock2.GetRelatedValues(1234, propertiesSubregion, associtaion).ReturnsForAnyArgs(2);
            //mock2.IteratorResourcesLeft(2).Returns(x => (count++ < 1) ? 1 : 0);
            mock2.IteratorNext(10, 2).Returns(ret);
            mock2.IteratorClose(2);
            geoRegionForTree.Model = new Model();
            geoRegionForTree.Model.FirstContact = false;
            geoRegionForTree.Model.GdaQueryProxy = mock2;
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
        }

        [Test]
        public void ConstructorTest()
        {
            SetupTest();

            Assert.DoesNotThrow(() =>
                geoRegionForTree = new GeoRegionForTree(parent, geoRegion, model, ref allTreeEl)
            );
        }

        [Test]
        public void NameTest()
        {
            geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 1234 };
            geoRegionForTree = new GeoRegionForTree();
            geoRegionForTree.GeoRegion = geoRegion;
            Assert.AreEqual(geoRegionForTree.Name, "geo");
        }

        [Test]
        public void CheckIfSeleactedTest()
        {
            int countForResourcesLeft1 = 0;
            int countForResourcesLeft2 = 0;
            int countForResourcesLeft3 = 0;
            int countForResourcesLeft4 = 0;
            int countForResourcesLeft5 = 0;
            int countForRelatedValues = 0;

            geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new TreeClasses();
            geoRegionForTree = new GeoRegionForTree();
            geoRegionForTree.GeoRegion = geoRegion;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subGeoregion1 = new ResourceDescription();
            ResourceDescription subStation = new ResourceDescription();
            ResourceDescription ami = new ResourceDescription();
            subGeoregion1.AddProperty(new Property(ModelCode.IDOBJ_GID, 4321));
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            List<ResourceDescription> ret = new List<ResourceDescription>() { subGeoregion1 };
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subStation };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(2).Returns(x => (countForResourcesLeft2++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(3).Returns(x => (countForResourcesLeft3++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(4).Returns(x => (countForResourcesLeft4++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(5).Returns(x => (countForResourcesLeft5++ < 1) ? 1 : 0);
            mock2.IteratorNext(10, 1).Returns(ret);
            mock2.IteratorNext(10, 2).Returns(ret1);
            mock2.IteratorNext(10, 3).Returns(ret2);
            mock2.IteratorNext(10, 4).Returns(ret1);
            mock2.IteratorNext(10, 5).Returns(ret2);
            mock2.IteratorClose(1);
            mock2.IteratorClose(2);
            mock2.IteratorClose(3);
            mock2.IteratorClose(4);
            mock2.IteratorClose(5);
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;

            this.geoRegionForTree.Model = model;
            this.geoRegionForTree.GeoRegion = geoRegion;

            this.geoRegionForTree.IsSelected = false;
            this.geoRegionForTree.IsSelected = true;
            Assert.DoesNotThrow(() => this.geoRegionForTree.CheckIfSeleacted());
        }

        [Test]
        public void LoadChildrenTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new TreeClasses();
            geoRegionForTree = new GeoRegionForTree();
            geoRegionForTree.GeoRegion = geoRegion;
            geoRegionForTree.AllTreeElements = allTreeEl;

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

            this.geoRegionForTree.Model = model;
            this.geoRegionForTree.GeoRegion = geoRegion;

            Assert.DoesNotThrow(() => this.geoRegionForTree.LoadChildren());
        }

        [Test]
        public void IsExpandedTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new TreeClasses();
            geoRegionForTree = new GeoRegionForTree();
            geoRegionForTree.GeoRegion = geoRegion;
            geoRegionForTree.AllTreeElements = allTreeEl;

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

            this.geoRegionForTree.Model = model;
            this.geoRegionForTree.GeoRegion = geoRegion;

            this.geoRegionForTree.IsExpanded = true;
            Assert.AreEqual(geoRegionForTree.IsExpanded, true);
        }
    }
}
