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

namespace ViewModelTests
{
    [TestFixture]
    public class GeoSubRegionViewModelTest
    {
        GeoSubRegionViewModel gdrvm;

        [OneTimeSetUp]
        public void Setup()
        {
            gdrvm = new GeoSubRegionViewModel();
        }

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new GeoSubRegionViewModel());
        }

        [Test]
        public void GeoRegionPropertyTest()
        {
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
            ObservableCollection<object> geoRegions = new ObservableCollection<object>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            gdrvm.GeoRegions = geoRegions;

            Assert.AreEqual(2, gdrvm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)gdrvm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)gdrvm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void SubGeoRegionsPropertyTest()
        {
            ObservableCollection<SubGeographicalRegion> subGeoRegions = new ObservableCollection<SubGeographicalRegion>() { new SubGeographicalRegion(918273645), new SubGeographicalRegion(192837465) };
            gdrvm.SubRegions = subGeoRegions;

            Assert.AreEqual(2, gdrvm.SubRegions.Count);
            Assert.AreEqual(918273645, ((SubGeographicalRegion)gdrvm.SubRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((SubGeographicalRegion)gdrvm.SubRegions[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            DelegateCommand getElementsCommand = null;
            gdrvm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(gdrvm.GetElementsCommand);
            Assert.IsNotNull(gdrvm.GetElementsCommand);
        }
    }
}
