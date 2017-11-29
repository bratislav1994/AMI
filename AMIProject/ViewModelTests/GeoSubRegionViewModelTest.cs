using AMIClient;
using NSubstitute;
using NUnit.Framework;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace ViewModelTests
{
    [TestFixture]
    public class GeoSubRegionViewModelTest
    {
        GeoSubRegionViewModel gdrvm;

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new GeoSubRegionViewModel(Substitute.For<IModel>()));
        }

        public void Init()
        {
            var model = Substitute.For<IModel>();
            ObservableCollection<GeographicalRegion> dummyList = new ObservableCollection<GeographicalRegion>() { new GeographicalRegion(123), new GeographicalRegion(456) };
            ObservableCollection<SubGeographicalRegion> dummyList2_1 = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(123), new SubGeographicalRegion(456) };
            ObservableCollection<SubGeographicalRegion> dummyList2_2 = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(456) };
            model.GetAllRegions().Returns(dummyList);
            model.GetAllSubRegions().Returns(dummyList2_1);
            model.GetSomeSubregions(123).Returns(dummyList2_2);
            gdrvm = new GeoSubRegionViewModel(model);
        }

        [Test]
        public void GeoRegionPropertyTest()
        {
            Init();

            GeographicalRegion gr = new GeographicalRegion(918273645);
            string dummy = "All";
            gdrvm.GeoRegion = gr;

            Assert.AreEqual(918273645, ((GeographicalRegion)gdrvm.GeoRegion).GlobalId);

            gdrvm.GeoRegion = dummy;
            Assert.AreEqual("All", gdrvm.GeoRegion);
        }

        [Test]
        public void GeoRegionsPropertyTest()
        {
            Init();

            ObservableCollection<object> geoRegions = new ObservableCollection<object>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            gdrvm.GeoRegions = geoRegions;

            Assert.AreEqual(2, gdrvm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)gdrvm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)gdrvm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubGeoRegionsPropertyTest()
        {
            Init();

            ObservableCollection<SubGeographicalRegion> subGeoRegions = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(918273645), new SubGeographicalRegion(192837465) };
            gdrvm.SubRegions = subGeoRegions;

            Assert.AreEqual(2, gdrvm.SubRegions.Count);
            Assert.AreEqual(918273645, ((SubGeographicalRegion)gdrvm.SubRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((SubGeographicalRegion)gdrvm.SubRegions[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            Init();

            DelegateCommand getElementsCommand = null;
            gdrvm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(gdrvm.GetElementsCommand);
            Assert.IsNotNull(gdrvm.GetElementsCommand);
        }

        [Test]
        public void GetElementsCommandActionTest()
        {
            Init();

            gdrvm.GeoRegion = "All";
            gdrvm.GetElementsCommand.Execute();

            Assert.AreEqual(2, gdrvm.SubRegions.Count);
            Assert.AreEqual(123, gdrvm.SubRegions[0].GlobalId);
            Assert.AreEqual(456, gdrvm.SubRegions[1].GlobalId);

            gdrvm.GeoRegion = new GeographicalRegion(123);
            gdrvm.GetElementsCommand.Execute();

            Assert.AreEqual(1, gdrvm.SubRegions.Count);
            Assert.AreEqual(456, gdrvm.SubRegions[0].GlobalId);

        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;

            this.gdrvm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.gdrvm.GeoRegions = new ObservableCollection<object>();
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("GeoRegions", receivedEvents);
        }
    }
}