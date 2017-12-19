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
    public class SubstationForTreeTest
    {
        private SubstationForTree substationForTree;
        private ObservableCollection<EnergyConsumer> amis;
        private Model model;
        private Substation substation;
        private SubGeoRegionForTree parent;
        private GeoRegionForTree parent2;
        private Dictionary<long, TreeClasses> allTreeEl = new Dictionary<long, TreeClasses>();

        [OneTimeSetUp]
        public void Init()
        {
            Logger.Path = "TestClient.txt";
        }

        public void SetupTest()
        {
            amis = new ObservableCollection<EnergyConsumer>();
            substation = new Substation() { Name = "sub", GlobalId = 1234 };
            parent = new SubGeoRegionForTree();
            
            //root.Model = im;
            this.substationForTree = new SubstationForTree();
            this.substationForTree.Substation = substation;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            List<ModelCode> properties = new List<ModelCode>();
            List<ResourceDescription> ret = new List<ResourceDescription>();
            mock2.GetExtentValues(ModelCode.ANALOG, properties).ReturnsForAnyArgs(2);
            mock2.IteratorResourcesLeft(0).Returns(0);
            mock2.IteratorClose(2);
            substationForTree.Model = new Model();
            substationForTree.Model.FirstContact = false;
            substationForTree.Model.GdaQueryProxy = mock2;
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
        }

        [Test]
        public void ConstructorTest()
        {
            SetupTest();
            Assert.DoesNotThrow(() =>
                substationForTree = new SubstationForTree(substation, parent, model)
            );

            this.substationForTree = new SubstationForTree();
        }

        [Test]
        public void NameTest()
        {
            this.SetupTest();
            Assert.AreEqual(substationForTree.Name, "sub");
        }

        [Test]
        public void IsSelectedTest()
        {
            this.SetupTest();
            this.substationForTree = new SubstationForTree(substation, parent, model);
            this.substationForTree.IsSelected = false;
            Assert.AreEqual(substationForTree.IsSelected, false);
            IModel mock = Substitute.For<IModel>();
            mock.GetSomeAmis(1).ReturnsForAnyArgs(new ObservableCollection<EnergyConsumer>());
            this.substationForTree.IsSelected = true;
            Assert.AreEqual(this.substationForTree.IsSelected, true);
        }

        [Test]
        public void IsExpandedTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            substation = new Substation() { Name = "sub", GlobalId = 1234 };
            parent = new SubGeoRegionForTree();
            SetForParentIsExpanded();
            substationForTree = new SubstationForTree(substation, parent, model);
            substationForTree.Substation = substation;

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

            this.substationForTree.Model = model;
            this.substationForTree.Substation = substation;

            this.substationForTree.IsExpanded = true;
            Assert.AreEqual(substationForTree.IsExpanded, true);
        }

        private void SetForParentIsExpanded()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            SubGeographicalRegion subGeoRegion = new SubGeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new SubGeoRegionForTree();
            SetForParentIsExpanded2();
            Model m = parent2.Model;
            parent = new SubGeoRegionForTree(new SubGeographicalRegion() { GlobalId = 1235 }, parent2, m, ref allTreeEl);
            //SubGeoRegionForTree subGeoRegionForTree = new SubGeoRegionForTree(subGeoRegion, parent2, model, ref allTreeEl);
            //subGeoRegionForTree.SubGeoRegion = subGeoRegion;
            //subGeoRegionForTree.AllTreeElements = allTreeEl;

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

            this.parent.Model = model;
            this.parent.SubGeoRegion = subGeoRegion;
        }

        private void SetForParentIsExpanded2()
        {
            Model model = new Model();
            GeographicalRegion geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 12345 };
            parent2 = new GeoRegionForTree();
            parent2.GeoRegion = geoRegion;
            parent2.AllTreeElements = allTreeEl;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subGeoregion1 = new ResourceDescription();
            subGeoregion1.AddProperty(new Property(ModelCode.IDOBJ_GID, 43211));
            List<ResourceDescription> ret = new List<ResourceDescription>() { subGeoregion1 };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(12345, properties, associtaion).ReturnsForAnyArgs(0);
            mock2.IteratorResourcesLeft(1).Returns(0);
            mock2.IteratorNext(10, 1).Returns(ret);
            mock2.IteratorClose(1);
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;

            this.parent2.Model = model;
            this.parent2.GeoRegion = geoRegion;
        }

        [Test]
        public void CheckIfSeleactedTest()
        {
            //SetupTest();
            //this.substationForTree = new SubstationForTree(substation, parent, model);

            //this.substationForTree.Substation = substation;

            //this.substationForTree.IsSelected = false;
            //this.substationForTree.IsSelected = true;
            //Assert.DoesNotThrow(() => this.substationForTree.CheckIfSeleacted());

            int countForResourcesLeft1 = 0;
            int countForResourcesLeft2 = 0;
            int countForResourcesLeft3 = 0;
            int countForRelatedValues = 0;

            substation = new Substation() { Name = "sub", GlobalId = 1234 };
            parent = new SubGeoRegionForTree();
            substationForTree = new SubstationForTree();
            substationForTree.Substation = substation;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription ami = new ResourceDescription();
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(0);
            mock2.IteratorResourcesLeft(3).Returns(0);
            mock2.IteratorNext(10, 3).Returns(ret2);
            mock2.IteratorClose(3);
            model = new Model();
            model.FirstContact = false;
            model.GdaQueryProxy = mock2;

            this.substationForTree.Model = model;
            this.substationForTree.Substation = substation;

            this.substationForTree.IsSelected = false;
            this.substationForTree.IsSelected = true;
            Assert.DoesNotThrow(() => this.substationForTree.CheckIfSeleacted());
        }
    }
}
