using AMIClient;
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
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private Dictionary<long, TreeClasses> allTreeEl = new Dictionary<long, TreeClasses>();
        private IModel model;
        private TreeClasses parent;
        private GeographicalRegion geoRegion;

        public void SetupTest()
        {
            amis = new ObservableCollection<EnergyConsumer>();
            geoRegion = new GeographicalRegion() { Name = "geo", GlobalId = 1234};
            parent = new TreeClasses();
            geoRegionForTree = new GeoRegionForTree();
            geoRegionForTree.GeoRegion = geoRegion;
            geoRegionForTree.Amis = amis;
            root = new RootElement();
            IModel im = Substitute.For<IModel>();
            im.GetAllRegions().Returns(new ObservableCollection<GeographicalRegion>());
            model = im;
            root.Model = im;
            newChange = DateTime.Now;
        }

        [Test]
        public void ConstructorTest()
        {
            SetupTest();
            Assert.DoesNotThrow(() => 
                geoRegionForTree = new GeoRegionForTree(parent, geoRegion, model, ref amis, ref newChange, ref allTreeEl)
            );
        }

        [Test]
        public void NameTest()
        {
            this.SetupTest();
            Assert.AreEqual(geoRegionForTree.Name, "geo");
        }

        [Test]
        public void CheckIfSeleactedTest()
        {
            SetupTest();

            IModel mock = Substitute.For<IModel>();
            mock.GetSomeSubregions(this.geoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<SubGeographicalRegion>()
            {
                new SubGeographicalRegion()
            });
            mock.GetSomeSubstations(this.geoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<Substation>()
            {
                new Substation()
            });
            mock.GetSomeAmis(this.geoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<EnergyConsumer>()
            {
                new EnergyConsumer()
            });

            this.geoRegionForTree.Model = mock;
            this.geoRegionForTree.GeoRegion = geoRegion;

            this.geoRegionForTree.IsSelected = false;
            this.geoRegionForTree.IsSelected = true;
            Assert.DoesNotThrow(() => this.geoRegionForTree.CheckIfSeleacted());
        }

        [Test]
        public void LoadChildrenTest()
        {
            SetupTest();
            IModel mock = Substitute.For<IModel>();
            mock.GetSomeSubregions(this.geoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<SubGeographicalRegion>()
            {
                new SubGeographicalRegion() { GlobalId = 123}
            });

            geoRegionForTree = new GeoRegionForTree(parent, geoRegion, model, ref amis, ref newChange, ref allTreeEl);

            this.geoRegionForTree.Model = mock;
            this.geoRegionForTree.GeoRegion = geoRegion;

            Assert.DoesNotThrow(() => this.geoRegionForTree.LoadChildren());
        }

        [Test]
        public void IsExpandedTest()
        {
            this.SetupTest();

            IModel mock = Substitute.For<IModel>();
            mock.GetSomeSubregions(this.geoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<SubGeographicalRegion>()
            {
                new SubGeographicalRegion() { GlobalId = 123}
            });

            geoRegionForTree = new GeoRegionForTree(parent, geoRegion, model, ref amis, ref newChange, ref allTreeEl);

            this.geoRegionForTree.Model = mock;
            this.geoRegionForTree.GeoRegion = geoRegion;

            this.geoRegionForTree.IsExpanded = true;
            Assert.AreEqual(geoRegionForTree.IsExpanded, true);
        }
    }
}
