using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using AMIClient;
using TC57CIM.IEC61970.Core;
using System.Collections.ObjectModel;
using TC57CIM.IEC61970.Wires;
using Prism.Commands;
using NSubstitute;
using System.ComponentModel;

namespace ViewModelTests
{
    [TestFixture]
    public class AMIsViewModelTest
    {
        AMIsViewModel avm;

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new AMIsViewModel(Substitute.For<IModel>()));
        }

        private void Init()
        {
            var model = Substitute.For<IModel>();
            ObservableCollection<GeographicalRegion> dummyList = new ObservableCollection<GeographicalRegion>() { new GeographicalRegion(123), new GeographicalRegion(456) };
            ObservableCollection<SubGeographicalRegion> dummyList2_1 = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(123), new SubGeographicalRegion(456) };
            ObservableCollection<SubGeographicalRegion> dummyList2_2 = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(456) };
            ObservableCollection<Substation> dummyList3_1 = new ObservableCollection<Substation>() { new Substation(123), new Substation(456) };
            ObservableCollection<Substation> dummyList3_2 = new ObservableCollection<Substation>() { new Substation(456) };
            ObservableCollection<Substation> dummyList3_3 = new ObservableCollection<Substation>() { new Substation(123) };
            ObservableCollection<EnergyConsumer> dummyList4_1 = new ObservableCollection<EnergyConsumer>() { new EnergyConsumer(123), new EnergyConsumer(456) };
            ObservableCollection<EnergyConsumer> dummyList4_2 = new ObservableCollection<EnergyConsumer>() { new EnergyConsumer(456) };
            ObservableCollection<EnergyConsumer> dummyList4_3 = new ObservableCollection<EnergyConsumer>() { new EnergyConsumer(123) };

            model.GetAllRegions().Returns(dummyList);
            model.GetAllSubRegions().Returns(dummyList2_1);
            model.GetAllSubstations().Returns(dummyList3_1);
            model.GetSomeSubstations(123).Returns(dummyList3_2);
            model.GetSomeSubstations(456).Returns(dummyList3_3);
            model.GetSomeSubregions(123).Returns(dummyList2_2);
            model.GetSomeSubregions(918273645).Returns(dummyList2_2);
            model.GetAllAmis().Returns(dummyList4_1);
            model.GetSomeSubstations(918273645).Returns(dummyList3_3);
            model.GetSomeAmis(123).Returns(dummyList4_2);
            model.GetSomeAmis(456).Returns(dummyList4_3);

            avm = new AMIsViewModel(model);
        }

        [Test]
        public void SubstationPropertyTest()
        {
            Init();

            Substation ss = new Substation(918273645);
            avm.Substation = ss;

            Assert.AreEqual(918273645, ((Substation)avm.Substation).GlobalId);
        }

        [Test]
        public void SubGeoRegionPropertyTest()
        {
            Init();

            SubGeographicalRegion sgr = new SubGeographicalRegion(918273645);
            avm.SubGeoRegion = sgr;

            Assert.AreEqual(918273645, ((SubGeographicalRegion)avm.SubGeoRegion).GlobalId);
        }

        [Test]
        public void GeoRegionPropertyTest()
        {
            Init();

            GeographicalRegion gr = new GeographicalRegion(918273645);
            avm.GeoRegion = gr;

            Assert.AreEqual(918273645, ((GeographicalRegion)avm.GeoRegion).GlobalId);
        }

        [Test]
        public void AmisPropertyTest()
        {
            Init();

            ObservableCollection<EnergyConsumer> amis = new ObservableCollection<EnergyConsumer>() { new EnergyConsumer(918273645), new EnergyConsumer(192837465)};
            avm.Amis = amis;

            Assert.AreEqual(2, avm.Amis.Count);
            Assert.AreEqual(918273645, avm.Amis[0].GlobalId);
            Assert.AreEqual(192837465, avm.Amis[1].GlobalId);
        }

        [Test]
        public void GeoRegionsPropertyTest()
        {
            Init();

            ObservableCollection<object> geoRegions = new ObservableCollection<object>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            avm.GeoRegions = geoRegions;

            Assert.AreEqual(2, avm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)avm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)avm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubGeoRegionsPropertyTest()
        {
            Init();

            ObservableCollection<object> subGeoRegions = new ObservableCollection<object>() { new SubGeographicalRegion(918273645), new SubGeographicalRegion(192837465) };
            avm.SubGeoRegions = subGeoRegions;

            Assert.AreEqual(2, avm.SubGeoRegions.Count);
            Assert.AreEqual(918273645, ((SubGeographicalRegion)avm.SubGeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((SubGeographicalRegion)avm.SubGeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubstationsPropertyTest()
        {
            Init();

            ObservableCollection<object> substations = new ObservableCollection<object>() { new Substation(918273645), new Substation(192837465) };
            avm.Substations = substations;

            Assert.AreEqual(2, avm.Substations.Count);
            Assert.AreEqual(918273645, ((Substation)avm.Substations[0]).GlobalId);
            Assert.AreEqual(192837465, ((Substation)avm.Substations[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            Init();

            DelegateCommand getElementsCommand = null;
            avm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(avm.GetElementsCommand);
            Assert.IsNotNull(avm.GetElementsCommand);
        }

        [Test]
        public void GetElementsCommandActionTest()
        {
            Init();

            avm.Substation = new Substation(123);
            avm.GetElementsCommand.Execute();

            Assert.AreEqual(1, avm.Amis.Count);
            Assert.AreEqual(456, avm.Amis[0].GlobalId);

            avm.Substation = "All";
            avm.SubGeoRegion = new SubGeographicalRegion(123);

            avm.GetElementsCommand.Execute();

            Assert.AreEqual(1, avm.Amis.Count);
            Assert.AreEqual(123, avm.Amis[0].GlobalId);

            avm.SubGeoRegion = "All";
            avm.GeoRegion = new GeographicalRegion(123);

            avm.GetElementsCommand.Execute();

            Assert.AreEqual(1, avm.Amis.Count);
            Assert.AreEqual(456, avm.Amis[0].GlobalId);

            avm.GeoRegion = "All";

            avm.GetElementsCommand.Execute();

            Assert.AreEqual(2, avm.Amis.Count);
            Assert.AreEqual(123, avm.Amis[0].GlobalId);
            Assert.AreEqual(456, avm.Amis[1].GlobalId);
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;

            this.avm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.avm.GeoRegions = new ObservableCollection<object>();
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("GeoRegions", receivedEvents);
        }

        [Test]
        public void SubGeoRegionIsNull()
        {
            Init();
            this.avm.SubGeoRegion = null;
            Assert.IsNull(this.avm.SubGeoRegion);
        }
    }
}