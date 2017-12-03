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

        [OneTimeSetUp]
        public void SetupTest()
        {
            root = new RootElement();
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
