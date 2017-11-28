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

namespace ViewModelTests
{
    [TestFixture]
    public class AMIsViewModelTest
    {
        AMIsViewModel avm;

        [OneTimeSetUp]
        public void Setup()
        {
            avm = new AMIsViewModel();
        }

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new AMIsViewModel());
        }

        [Test]
        public void SubstationPropertyTest()
        {
            Substation ss = new Substation(918273645);
            avm.Substation = ss;

            Assert.AreEqual(918273645, ((Substation)avm.Substation).GlobalId);
        }

        [Test]
        public void SubGeoRegionPropertyTest()
        {
            SubGeographicalRegion sgr = new SubGeographicalRegion(918273645);
            avm.SubGeoRegion = sgr;

            Assert.AreEqual(918273645, ((SubGeographicalRegion)avm.SubGeoRegion).GlobalId);
        }

        [Test]
        public void GeoRegionPropertyTest()
        {
            GeographicalRegion gr = new GeographicalRegion(918273645);
            avm.GeoRegion = gr;

            Assert.AreEqual(918273645, ((GeographicalRegion)avm.GeoRegion).GlobalId);
        }

        [Test]
        public void AmisPropertyTest()
        {
            ObservableCollection<EnergyConsumer> amis = new ObservableCollection<EnergyConsumer>() { new EnergyConsumer(918273645), new EnergyConsumer(192837465)};
            avm.Amis = amis;

            Assert.AreEqual(2, avm.Amis.Count);
            Assert.AreEqual(918273645, avm.Amis[0].GlobalId);
            Assert.AreEqual(192837465, avm.Amis[1].GlobalId);
        }

        [Test]
        public void GeoRegionsPropertyTest()
        {
            ObservableCollection<object> geoRegions = new ObservableCollection<object>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            avm.GeoRegions = geoRegions;

            Assert.AreEqual(2, avm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)avm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)avm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubGeoRegionsPropertyTest()
        {
            ObservableCollection<object> subGeoRegions = new ObservableCollection<object>() { new SubGeographicalRegion(918273645), new SubGeographicalRegion(192837465) };
            avm.SubGeoRegions = subGeoRegions;

            Assert.AreEqual(2, avm.SubGeoRegions.Count);
            Assert.AreEqual(918273645, ((SubGeographicalRegion)avm.SubGeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((SubGeographicalRegion)avm.SubGeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubstationsPropertyTest()
        {
            ObservableCollection<object> substations = new ObservableCollection<object>() { new Substation(918273645), new Substation(192837465) };
            avm.Substations = substations;

            Assert.AreEqual(2, avm.Substations.Count);
            Assert.AreEqual(918273645, ((Substation)avm.Substations[0]).GlobalId);
            Assert.AreEqual(192837465, ((Substation)avm.Substations[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            DelegateCommand getElementsCommand = null;
            avm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(avm.GetElementsCommand);
            Assert.IsNotNull(avm.GetElementsCommand);
        }
    }
}
