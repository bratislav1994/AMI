using AMIClient;
using AMIClient.ViewModels;
using FTN.Common;
using FTN.Common.Logger;
using FTN.ServiceContracts;
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

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class NetworkPreviewViewModelTest
    {
        private NetworkPreviewViewModel networkPreview;
        private Model model;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.model = new Model();
            this.networkPreview = new NetworkPreviewViewModel();

            Logger.Path = "TestClient.txt";
        }

        [Test]
        public void InstanceTest()
        {
            NetworkPreviewViewModel network = NetworkPreviewViewModel.Instance;
            Assert.IsNotNull(network);
        }

        [Test]
        public void RootElementsTest()
        {
            ObservableCollection<RootElement> root = new ObservableCollection<RootElement>();
            root.Add(new RootElement(model));
            networkPreview.RootElements = root;

            Assert.AreEqual(root, networkPreview.RootElements);
        }

        [Test]
        public void ModelTest()
        {
            networkPreview.Model = model;

            Assert.AreEqual(model, networkPreview.Model);
        }

        [Test]
        public void SetModelTest()
        {
            Assert.DoesNotThrow(() => networkPreview.SetModel(model));
        }

        //[Test]
        //public void RaisePropertyChangedTest()
        //{
        //    string receivedEvents = null;
        //    this.networkPreview.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
        //    {
        //        receivedEvents = e.PropertyName;
        //    };

        //    this.networkPreview.CurrentViewModel = null;
        //    Assert.IsNotNull(receivedEvents);
        //    Assert.AreEqual("CurrentViewModel", receivedEvents);
        //}

        [Test]
        public void SelectedAMIActionTest()
        {
            EnergyConsumer consumer = new EnergyConsumer(1);
            consumer.Name = "ConsumerTest";

            ResolutionType type = ResolutionType.MINUTE;

            Assert.DoesNotThrow(() => networkPreview.SelectedAMIAction(consumer, type));
        }

        [Test]
        public void ChartViewForSubstationTest()
        {
            ResolutionType type = ResolutionType.MINUTE;
            string header = "Substation";

            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subStation = new ResourceDescription(1);
            ResourceDescription ami = new ResourceDescription();
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subStation };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret2);

            mock2.IteratorClose(1);

            networkPreview.Model = new Model();
            networkPreview.Model.FirstContact = false;
            networkPreview.Model.GdaQueryProxy = mock2;

            Assert.DoesNotThrow(() => networkPreview.ChartViewForSubstation(type, subStation.Id, header));
        }

        [Test]
        public void ChartViewForSubGeoRegionTest()
        {
            ResolutionType type = ResolutionType.MINUTE;
            string header = "SubGeoRegion";

            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            int countForResourcesLeft2 = 0;
            int countForRelatedValues2 = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subGeoRegion = new ResourceDescription(1);
            ResourceDescription subStation = new ResourceDescription();
            subGeoRegion.AddProperty(new Property(ModelCode.IDOBJ_GID, 4332));
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4333));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subGeoRegion };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { subStation };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1235, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret2);

            mock2.IteratorClose(1);

            ResourceDescription amis = new ResourceDescription();
            amis.AddProperty(new Property(ModelCode.IDOBJ_GID, 4334));
            List<ResourceDescription> ret3 = new List<ResourceDescription>() { amis };

            Association associtaion1 = new Association();
            List<ModelCode> properties1 = new List<ModelCode>();
            mock2.GetRelatedValues(4333, properties1, associtaion1).ReturnsForAnyArgs(x => (++countForRelatedValues2));
            mock2.IteratorResourcesLeft(2).Returns(x => (countForResourcesLeft2++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 2).Returns(ret3);

            mock2.IteratorClose(2);

            networkPreview.Model = new Model();
            networkPreview.Model.FirstContact = false;
            networkPreview.Model.GdaQueryProxy = mock2;

            Assert.DoesNotThrow(() => networkPreview.ChartViewForSubGeoRegion(type, subGeoRegion.Id, header));
        }

        [Test]
        public void ChartViewForGeoRegionTest()
        {
            ResolutionType type = ResolutionType.MINUTE;
            string header = "GeoRegion";

            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            int countForResourcesLeft2 = 0;
            int countForRelatedValues2 = 0;

            int countForResourcesLeft3 = 0;
            int countForRelatedValues3 = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription GeoRegion = new ResourceDescription(1);
            ResourceDescription subRegion = new ResourceDescription();
            GeoRegion.AddProperty(new Property(ModelCode.IDOBJ_GID, 4332));
            subRegion.AddProperty(new Property(ModelCode.IDOBJ_GID, 4333));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { GeoRegion };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { subRegion };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1235, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret2);

            mock2.IteratorClose(1);

            ResourceDescription subStation = new ResourceDescription();
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4334));
            List<ResourceDescription> ret3 = new List<ResourceDescription>() { subStation };

            Association associtaion1 = new Association();
            List<ModelCode> properties1 = new List<ModelCode>();
            mock2.GetRelatedValues(4333, properties1, associtaion1).ReturnsForAnyArgs(x => (++countForRelatedValues2));
            mock2.IteratorResourcesLeft(2).Returns(x => (countForResourcesLeft2++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 2).Returns(ret3);

            mock2.IteratorClose(2);

            ResourceDescription amis = new ResourceDescription();
            amis.AddProperty(new Property(ModelCode.IDOBJ_GID, 4335));
            List<ResourceDescription> ret4 = new List<ResourceDescription>() { amis };

            Association associtaion2 = new Association();
            List<ModelCode> properties2 = new List<ModelCode>();
            mock2.GetRelatedValues(4333, properties2, associtaion2).ReturnsForAnyArgs(x => (++countForRelatedValues3));
            mock2.IteratorResourcesLeft(3).Returns(x => (countForResourcesLeft3++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 3).Returns(ret4);

            mock2.IteratorClose(3);

            networkPreview.Model = new Model();
            networkPreview.Model.FirstContact = false;
            networkPreview.Model.GdaQueryProxy = mock2;

            Assert.DoesNotThrow(() => networkPreview.ChartViewForGeoRegion(type, GeoRegion.Id, header));
        }
    }
}
