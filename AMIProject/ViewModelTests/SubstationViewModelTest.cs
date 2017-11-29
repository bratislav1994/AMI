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
using TC57CIM.IEC61970.Wires;

namespace ViewModelTests
{
    [TestFixture]
    public class SubstationViewModelTest
    {
        SubstationViewModel svm;

        public void Init()
        {
            var model = Substitute.For<IModel>();
            ObservableCollection<GeographicalRegion> dummyList = new ObservableCollection<GeographicalRegion>() { new GeographicalRegion(123), new GeographicalRegion(456) };
            ObservableCollection<SubGeographicalRegion> dummyList2_1 = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(123), new SubGeographicalRegion(456) };
            ObservableCollection<SubGeographicalRegion> dummyList2_2 = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(456) };
            ObservableCollection<Substation> dummyList3_1 = new ObservableCollection<Substation>() { new Substation(123), new Substation(456) };
            ObservableCollection<Substation> dummyList3_2 = new ObservableCollection<Substation>() { new Substation(456) };
            ObservableCollection<Substation> dummyList3_3 = new ObservableCollection<Substation>() { new Substation(123) };

            model.GetAllRegions().Returns(dummyList);
            model.GetAllSubRegions().Returns(dummyList2_1);
            model.GetAllSubstations().Returns(dummyList3_1);
            model.GetSomeSubstations(123).Returns(dummyList3_2);
            model.GetSomeSubstations(456).Returns(dummyList3_3);
            model.GetSomeSubregions(123).Returns(dummyList2_2);
            model.GetSomeSubregions(918273645).Returns(dummyList2_2);

            svm = new SubstationViewModel(model);
        }

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new SubstationViewModel(Substitute.For<IModel>()));
        }

        [Test]
        public void SubGeoRegionPropertyTest()
        {
            Init();

            SubGeographicalRegion sgr = new SubGeographicalRegion(918273645);
            svm.SubGeoRegion = sgr;

            Assert.AreEqual(918273645, ((SubGeographicalRegion)svm.SubGeoRegion).GlobalId);
        }

        [Test]
        public void GeoRegionPropertyTest()
        {
            Init();

            GeographicalRegion gr = new GeographicalRegion(918273645);
            svm.GeoRegion = gr;

            Assert.AreEqual(918273645, ((GeographicalRegion)svm.GeoRegion).GlobalId);
        }

        [Test]
        public void GeoRegionsPropertyTest()
        {
            Init();

            ObservableCollection<object> geoRegions = new ObservableCollection<object>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            svm.GeoRegions = geoRegions;

            Assert.AreEqual(2, svm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)svm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)svm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubGeoRegionsPropertyTest()
        {
            Init();

            ObservableCollection<object> subGeoRegions = new ObservableCollection<object>() { new SubGeographicalRegion(918273645), new SubGeographicalRegion(192837465) };
            svm.SubGeoRegions = subGeoRegions;

            Assert.AreEqual(2, svm.SubGeoRegions.Count);
            Assert.AreEqual(918273645, ((SubGeographicalRegion)svm.SubGeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((SubGeographicalRegion)svm.SubGeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubstationsPropertyTest()
        {
            Init();

            ObservableCollection<Substation> substations = new ObservableCollection<Substation>() { new Substation(918273645), new Substation(192837465) };
            svm.Substations = substations;

            Assert.AreEqual(2, svm.Substations.Count);
            Assert.AreEqual(918273645, ((Substation)svm.Substations[0]).GlobalId);
            Assert.AreEqual(192837465, ((Substation)svm.Substations[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            Init();

            DelegateCommand getElementsCommand = null;
            svm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(svm.GetElementsCommand);
            Assert.IsNotNull(svm.GetElementsCommand);
        }

        [Test]
        public void GetElementsCommandActionTest()
        {
            Init();

            svm.GeoRegion = "All";
            svm.SubGeoRegion = new SubGeographicalRegion(123);
            svm.GetElementsCommand.Execute();

            Assert.AreEqual(1, svm.Substations.Count);
            Assert.AreEqual(456, svm.Substations[0].GlobalId);

            svm.SubGeoRegion = "All";
            svm.GeoRegion = new GeographicalRegion(123);
            svm.GetElementsCommand.Execute();

            Assert.AreEqual(1, svm.Substations.Count);
            Assert.AreEqual(123, svm.Substations[0].GlobalId);

            svm.GeoRegion = "All";
            svm.GetElementsCommand.Execute();

            Assert.AreEqual(2, svm.Substations.Count);
            Assert.AreEqual(123, svm.Substations[0].GlobalId);
            Assert.AreEqual(456, svm.Substations[1].GlobalId);
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;

            this.svm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.svm.GeoRegions = new ObservableCollection<object>();
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("GeoRegions", receivedEvents);
        }
    }
}
