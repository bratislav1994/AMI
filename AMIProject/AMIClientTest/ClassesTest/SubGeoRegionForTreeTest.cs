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
    public class SubGeoRegionForTreeTest
    {
        private SubGeoRegionForTree subGeoRegionForTree;
        private RootElement root;
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private Dictionary<long, TreeClasses> allTreeEl = new Dictionary<long, TreeClasses>();
        private IModel model;
        private GeoRegionForTree parent;
        private SubGeographicalRegion subGeoRegion;

        public void SetupTest()
        {
            amis = new ObservableCollection<EnergyConsumer>();
            subGeoRegion = new SubGeographicalRegion() { Name = "geo", GlobalId = 1234 };
            parent = new GeoRegionForTree();
            subGeoRegionForTree = new SubGeoRegionForTree();
            subGeoRegionForTree.SubGeoRegion = subGeoRegion;
            subGeoRegionForTree.Amis = amis;
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
                subGeoRegionForTree = new SubGeoRegionForTree(subGeoRegion, parent, model, ref amis, ref newChange, ref allTreeEl)
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
            SetupTest();

            IModel mock = Substitute.For<IModel>();

            mock.GetSomeSubstations(this.subGeoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<Substation>()
            {
                new Substation()
            });
            mock.GetSomeAmis(this.subGeoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<EnergyConsumer>()
            {
                new EnergyConsumer()
            });

            this.subGeoRegionForTree.Model = mock;
            this.subGeoRegionForTree.SubGeoRegion = subGeoRegion;

            this.subGeoRegionForTree.IsSelected = false;
            this.subGeoRegionForTree.IsSelected = true;
            Assert.DoesNotThrow(() => this.subGeoRegionForTree.CheckIfSeleacted());
        }

        [Test]
        public void LoadChildrenTest()
        {
            SetupTest();
            IModel mock = Substitute.For<IModel>();
            mock.GetSomeSubstations(this.subGeoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<Substation>()
            {
                new Substation() { GlobalId = 123}
            });

            subGeoRegionForTree = new SubGeoRegionForTree(subGeoRegion, parent, model, ref amis, ref newChange, ref allTreeEl);

            this.subGeoRegionForTree.Model = mock;
            this.subGeoRegionForTree.SubGeoRegion = subGeoRegion;

            Assert.DoesNotThrow(() => this.subGeoRegionForTree.LoadChildren());
        }

        [Test]
        public void IsExpandedTest()
        {
            this.SetupTest();

            IModel mock = Substitute.For<IModel>();
            mock.GetSomeSubstations(this.subGeoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<Substation>()
            {
                new Substation() { GlobalId = 123}
            });
            mock.GetSomeSubregions(this.subGeoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<SubGeographicalRegion>()
            {
                new SubGeographicalRegion() { GlobalId = 123}
            });
            
            subGeoRegionForTree = new SubGeoRegionForTree(subGeoRegion, parent, model, ref amis, ref newChange, ref allTreeEl);
            subGeoRegionForTree.SubGeoRegion = new SubGeographicalRegion() { GlobalId = 13 };

            this.subGeoRegionForTree.Model = mock;
            ((GeoRegionForTree)this.subGeoRegionForTree.Parent).GeoRegion = new GeographicalRegion() {GlobalId = 1453 };

            // sad se mora mokovati model u geoRegion
            IModel mock2 = Substitute.For<IModel>();
            mock2.GetSomeSubregions(43).ReturnsForAnyArgs(new ObservableCollection<SubGeographicalRegion>());
            ((GeoRegionForTree)this.subGeoRegionForTree.Parent).Model = mock2;

            this.subGeoRegionForTree.IsExpanded = true;
            Assert.AreEqual(subGeoRegionForTree.IsExpanded, true);
        }
    }
}
