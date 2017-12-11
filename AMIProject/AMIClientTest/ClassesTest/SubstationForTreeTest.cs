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
    public class SubstationForTreeTest
    {
        private SubstationForTree substationForTree;
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private IModel model;
        private Substation substation;
        private SubGeoRegionForTree parent;

        public void SetupTest()
        {
            amis = new ObservableCollection<EnergyConsumer>();
            substation = new Substation() { Name = "sub", GlobalId = 1234 };
            parent = new SubGeoRegionForTree();
            IModel im = Substitute.For<IModel>();
            im.GetAllRegions().Returns(new ObservableCollection<GeographicalRegion>());
            model = im;
            //root.Model = im;
            newChange = DateTime.Now;
            this.substationForTree = new SubstationForTree();
            this.substationForTree.Substation = substation;
        }

        [Test]
        public void ConstructorTest()
        {
            SetupTest();
            Assert.DoesNotThrow(() =>
                substationForTree = new SubstationForTree(substation, parent, model, ref amis, ref newChange)
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
            this.substationForTree = new SubstationForTree(substation, parent, model, ref amis, ref newChange);
            this.substationForTree.IsSelected = false;
            Assert.AreEqual(substationForTree.IsSelected, false);
            IModel mock = Substitute.For<IModel>();
            mock.GetSomeAmis(1).ReturnsForAnyArgs(new ObservableCollection<EnergyConsumer>());
            this.substationForTree.Model = mock;
            this.substationForTree.IsSelected = true;
            Assert.AreEqual(this.substationForTree.IsSelected, true);
        }

        [Test]
        public void IsExpandedTest()
        {
            this.SetupTest();

            IModel mock = Substitute.For<IModel>();
            mock.GetSomeSubstations(this.substation.GlobalId).ReturnsForAnyArgs(new ObservableCollection<Substation>()
            {
                new Substation() { GlobalId = 123}
            });
            mock.GetSomeSubregions(this.substation.GlobalId).ReturnsForAnyArgs(new ObservableCollection<SubGeographicalRegion>()
            {
                new SubGeographicalRegion() { GlobalId = 123}
            });

            this.substationForTree = new SubstationForTree(substation, parent, model, ref amis, ref newChange);
            this.substationForTree.Substation = new Substation() { GlobalId = 13 };

            this.substationForTree.Model = mock;
            ((SubGeoRegionForTree)this.substationForTree.Parent).SubGeoRegion = new SubGeographicalRegion() { GlobalId = 1453 };

            // sad se mora mokovati model u subGeoRegion
            IModel mock2 = Substitute.For<IModel>();
            mock2.GetSomeSubstations(43).ReturnsForAnyArgs(new ObservableCollection<Substation>());
            ((SubGeoRegionForTree)this.substationForTree.Parent).Model = mock2;

            this.substationForTree.IsExpanded = true;
            Assert.AreEqual(this.substationForTree.IsExpanded, true);
        }

        [Test]
        public void CheckIfSeleactedTest()
        {
            SetupTest();
            this.substationForTree = new SubstationForTree(substation, parent, model, ref amis, ref newChange);

            IModel mock = Substitute.For<IModel>();

            //mock.GetSomeSubstations(this.subGeoRegion.GlobalId).ReturnsForAnyArgs(new ObservableCollection<Substation>()
            //{
            //    new Substation()
            //});
            mock.GetSomeAmis(this.substation.GlobalId).ReturnsForAnyArgs(new ObservableCollection<EnergyConsumer>()
            {
                new EnergyConsumer()
            });

            this.substationForTree.Model = mock;
            this.substationForTree.Substation = substation;

            this.substationForTree.IsSelected = false;
            this.substationForTree.IsSelected = true;
            Assert.DoesNotThrow(() => this.substationForTree.CheckIfSeleacted());
        }
    }
}
