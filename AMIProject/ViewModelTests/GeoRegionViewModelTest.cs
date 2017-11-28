using AMIClient;
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
        
        public void Init()
        {
            var model = Substitute.For<IModel>();
            ObservableCollection<GeographicalRegion> dummy = new ObservableCollection<GeographicalRegion>() { new GeographicalRegion(123), new GeographicalRegion(456) };
            model.GetAllRegions().Returns(dummy);
            grvm = new GeoRegionViewModel(model);
        }

        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new GeoRegionViewModel(Substitute.For<IModel>()));
        }

        [Test]
        public void GeoRegionsPropertyTest()
        {
            Init();

            ObservableCollection<GeographicalRegion> geoRegions = new ObservableCollection<GeographicalRegion>() { new GeographicalRegion(918273645), new GeographicalRegion(192837465) };
            grvm.GeoRegions = geoRegions;

            Assert.AreEqual(2, grvm.GeoRegions.Count);
            Assert.AreEqual(918273645, ((GeographicalRegion)grvm.GeoRegions[0]).GlobalId);
            Assert.AreEqual(192837465, ((GeographicalRegion)grvm.GeoRegions[1]).GlobalId);
        }

        [Test]
        public void GetElementsCommandTest()
        {
            Init();

            DelegateCommand getElementsCommand = null;
            grvm.GetElementsCommand = getElementsCommand;

            Assert.IsNotNull(grvm.GetElementsCommand);
            Assert.IsNotNull(grvm.GetElementsCommand);
        }

        [Test]
        public void GetElementsCommandActionTest()
        {
            Init();

            grvm.GetElementsCommand.Execute();
            Assert.AreEqual(2, grvm.GeoRegions.Count);
            Assert.AreEqual(123, grvm.GeoRegions[0].GlobalId);
            Assert.AreEqual(456, grvm.GeoRegions[1].GlobalId);
        }


    }
}
