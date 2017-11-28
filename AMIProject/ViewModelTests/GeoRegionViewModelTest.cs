/*using AMIClient;
using NSubstitute;
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
    public class GeoRegionViewModelTest
    {
        GeoRegionViewModel grvm;
        

        [OneTimeSetUp]
        public void Setup()
        {
            var model = Substitute.For<IModel>();
            grvm = new GeoRegionViewModel();
        }

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new GeoRegionViewModel());
        }

        [Test]
        public void GeoRegionsPropertyTest()
        {
            ObservableCollection<GeographicalRegion> geoRegions = new ObservableCollection<GeographicalRegion>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            grvm.GeoRegions = geoRegions;

            Assert.AreEqual(2, grvm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)grvm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)grvm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            DelegateCommand getElementsCommand = null;
            grvm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(grvm.GetElementsCommand);
            Assert.IsNotNull(grvm.GetElementsCommand);
        }
    }
}*/
