using AMIClient;
using NUnit.Framework;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [OneTimeSetUp]
        public void Setup()
        {
            svm = new SubstationViewModel();
        }

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new SubstationViewModel());
        }

        [Test]
        public void SubGeoRegionPropertyTest()
        {
            SubGeographicalRegion sgr = new SubGeographicalRegion(918273645);
            svm.SubGeoRegion = sgr;

            Assert.AreEqual(918273645, ((SubGeographicalRegion)svm.SubGeoRegion).GlobalId);
        }

        [Test]
        public void GeoRegionPropertyTest()
        {
            GeographicalRegion gr = new GeographicalRegion(918273645);
            svm.GeoRegion = gr;

            Assert.AreEqual(918273645, ((GeographicalRegion)svm.GeoRegion).GlobalId);
        }

        [Test]
        public void GeoRegionsPropertyTest()
        {
            ObservableCollection<object> geoRegions = new ObservableCollection<object>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            svm.GeoRegions = geoRegions;

            Assert.AreEqual(2, svm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)svm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)svm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubGeoRegionsPropertyTest()
        {
            ObservableCollection<object> subGeoRegions = new ObservableCollection<object>() { new SubGeographicalRegion(918273645), new SubGeographicalRegion(192837465) };
            svm.SubGeoRegions = subGeoRegions;

            Assert.AreEqual(2, svm.SubGeoRegions.Count);
            Assert.AreEqual(918273645, ((SubGeographicalRegion)svm.SubGeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((SubGeographicalRegion)svm.SubGeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubstationsPropertyTest()
        {
            ObservableCollection<Substation> substations = new ObservableCollection<Substation>() { new Substation(918273645), new Substation(192837465) };
            svm.Substations = substations;

            Assert.AreEqual(2, svm.Substations.Count);
            Assert.AreEqual(918273645, ((Substation)svm.Substations[0]).GlobalId);
            Assert.AreEqual(192837465, ((Substation)svm.Substations[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            DelegateCommand getElementsCommand = null;
            svm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(svm.GetElementsCommand);
            Assert.IsNotNull(svm.GetElementsCommand);
        }
    }
}
