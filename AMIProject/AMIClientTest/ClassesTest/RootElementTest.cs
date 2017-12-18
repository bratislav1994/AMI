using AMIClient;
using FTN.Common;
using FTN.ServiceContracts;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;
using static System.Net.Mime.MediaTypeNames;
using static System.Object;

namespace AMIClientTest.ClassesTest
{
    [TestFixture]
    public class RootElementTest
    {
        private RootElement root;
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private Dictionary<long, TreeClasses> allTreeEl = new Dictionary<long, TreeClasses>();
        private Model model;

        //[OneTimeSetUp]
        public void SetupTest()
        {
            root = new RootElement();
            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            List<ModelCode> properties = new List<ModelCode>();
            List<ResourceDescription> ret = new List<ResourceDescription>();
            mock2.GetExtentValues(ModelCode.ANALOG, properties).Returns(2);
            mock2.IteratorResourcesLeft(0).Returns(0);
            mock2.IteratorClose(2);
            root.Model = new Model();
            root.Model.GdaQueryProxy = mock2;
            model = new Model();
            model.GdaQueryProxy = mock2;
            newChange = DateTime.Now;
            root.Model.GeoRegions = new ObservableCollection<GeographicalRegion>()
            {
                new GeographicalRegion() { GlobalId = 1234 }
            };
        }

        [Test]
        public void ConstructorTest()
        {
            SetupTest();
            Assert.DoesNotThrow(() => { var r = new RootElement(model); r.Dispose(); });
        }
        
        [Test]
        public void NeedsUpdateTest()
        {
            this.SetupTest();
            root.NeedsUpdate = true;
            Assert.AreEqual(root.NeedsUpdate, true);
        }

        [Test]
        public void LockObjectTest()
        {
            this.SetupTest();
            root.LockObject = new object();
            Assert.IsNotNull(root.LockObject);
        }

        [Test]
        public void IsSelectedTest()
        {
            this.SetupTest();
            root.IsSelected = false;
            Assert.AreEqual(root.IsSelected, false);
            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            List<ModelCode> properties = new List<ModelCode>();
            List<ResourceDescription> ret = new List<ResourceDescription>();
            mock2.GetExtentValues(ModelCode.ANALOG, properties).Returns(2);
            mock2.IteratorResourcesLeft(0).Returns(0);
            mock2.IteratorClose(2);
            root.Model = new Model();
            root.Model.GdaQueryProxy = mock2;
            root.IsSelected = true;
            Assert.AreEqual(root.IsSelected, true);
        }

        [Test]
        public void NameTest()
        {
            this.SetupTest();
            Assert.AreEqual(root.Name, "All");
        }

        [Test]
        public void IsExpandedTest()
        {
            this.SetupTest();
            this.root.IsExpanded = true;
            Assert.AreEqual(root.IsExpanded, true);
        }

        [Test]
        public void LoadChildrenTest()
        {
            SetupTest();
            root = new RootElement();
            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            List<ModelCode> properties = new List<ModelCode>();
            List<ResourceDescription> ret = new List<ResourceDescription>();
            mock2.GetExtentValues(ModelCode.GEOREGION, properties).ReturnsForAnyArgs(2);
            mock2.IteratorResourcesLeft(0).Returns(1);
            mock2.IteratorNext(2, 2).Returns(new List<ResourceDescription>() { new ResourceDescription() });
            mock2.IteratorClose(2);
            root.Model = new Model();
            root.Model.GdaQueryProxy = mock2;
            model = new Model();
            model.GdaQueryProxy = mock2;
            newChange = DateTime.Now;

            this.root.LoadChildren();
        }

        [Test]
        public void AllTreeElementsTest()
        {
            SetupTest();
            this.root.AllTreeElements = new Dictionary<long, TreeClasses>();
            Assert.IsNotNull(this.root.AllTreeElements);
        }

        //[Test]
        //public void CheckForUpdatesTest()
        //{
        //    //Dispatcher a = Dispatcher.CurrentDispatcher;
        //    SetupTest();
        //    IModel im = Substitute.For<IModel>();
        //    im.GetAllRegions().Returns(new ObservableCollection<GeographicalRegion>());
        //    im.GetAllAmis().Returns(new ObservableCollection<EnergyConsumer>());
        //    root.Model = im;
        //    model = im;
        //    newChange = DateTime.Now;

        //    Assert.DoesNotThrow(() => { root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true, Model = im, amis = new ObservableCollection<EnergyConsumer>() }; root.Dispose(); });
            
        //    Assert.DoesNotThrow(() => { root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true, Model = im, amis = new ObservableCollection<EnergyConsumer>(), IsSelected = true }; root.Dispose(); });
        //    TreeClasses tc = new TreeClasses();
        //    allTreeEl.Add(234, tc);
            
        //    Assert.DoesNotThrow(() => { root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true, Model = im, amis = new ObservableCollection<EnergyConsumer>(), IsSelected = false, AllTreeElements = allTreeEl }; root.Dispose(); });
        //    //Assert.DoesNotThrow(() => root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true});
        //}

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.root.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            IModel im = Substitute.For<IModel>();
            this.root.Children.Add(new GeoRegionForTree());
            this.root.IsExpanded = true;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("IsExpanded", receivedEvents);
        }
    }
}
