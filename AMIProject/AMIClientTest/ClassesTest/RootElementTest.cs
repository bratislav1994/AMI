using AMIClient;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClientTest.ClassesTest
{
    [TestFixture]
    public class RootElementTest
    {
        private RootElement root;
        private ObservableCollection<EnergyConsumer> amis;
        private DateTime newChange;
        private Dictionary<long, TreeClasses> allTreeElements;
        private IModel model;

        //[OneTimeSetUp]
        public void SetupTest()
        {
            root = new RootElement();
            IModel im = Substitute.For<IModel>();
            ObservableCollection<GeographicalRegion> ret = new ObservableCollection<GeographicalRegion>();
            im.GetAllRegions().Returns(ret);
            model = im;
            newChange = DateTime.Now;
        }

        [Test]
        public void ConstructorTest()
        {
            SetupTest();
            Assert.DoesNotThrow(() => { var r = new RootElement(model, ref amis, ref newChange); r.Dispose(); });
        }

        [Test]
        public void LoadChildrenTest()
        {
            SetupTest();
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
