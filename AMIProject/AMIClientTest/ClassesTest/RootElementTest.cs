using AMIClient;
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
        private IModel model;

        //[OneTimeSetUp]
        public void SetupTest()
        {
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
            Assert.DoesNotThrow(() => { var r = new RootElement(model, ref amis, ref newChange); r.Dispose(); });
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
            IModel mock = Substitute.For<IModel>();
            mock.GetAllAmis().Returns(new ObservableCollection<EnergyConsumer>());
            root.Model = mock;
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
            IModel mock = Substitute.For<IModel>();
            mock.GetAllRegions().Returns(
                new ObservableCollection<GeographicalRegion>()
                {
                    new GeographicalRegion()
                    {
                        GlobalId = 43290,
                        Mrid = "43290",
                        Name = "Vojvodina"
                    }
                });

            this.root.Model = mock;
            this.root.LoadChildren();
        }

        [Test]
        public void AllTreeElementsTest()
        {
            SetupTest();
            this.root.AllTreeElements = new Dictionary<long, TreeClasses>();
            Assert.IsNotNull(this.root.AllTreeElements);
        }

        [Test]
        public void CheckForUpdatesTest()
        {
            //Dispatcher a = Dispatcher.CurrentDispatcher;
            SetupTest();
            IModel im = Substitute.For<IModel>();
            im.GetAllRegions().Returns(new ObservableCollection<GeographicalRegion>());
            im.GetAllAmis().Returns(new ObservableCollection<EnergyConsumer>());
            root.Model = im;
            model = im;
            newChange = DateTime.Now;

            Assert.DoesNotThrow(() => { root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true, Model = im, amis = new ObservableCollection<EnergyConsumer>() }; root.Dispose(); });
            
            Assert.DoesNotThrow(() => { root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true, Model = im, amis = new ObservableCollection<EnergyConsumer>(), IsSelected = true }; root.Dispose(); });
            TreeClasses tc = new TreeClasses();
            allTreeEl.Add(234, tc);
            
            Assert.DoesNotThrow(() => { root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true, Model = im, amis = new ObservableCollection<EnergyConsumer>(), IsSelected = false, AllTreeElements = allTreeEl }; root.Dispose(); });
            //Assert.DoesNotThrow(() => root = new RootElement(model, ref amis, ref newChange) { NeedsUpdate = true});
        }

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
