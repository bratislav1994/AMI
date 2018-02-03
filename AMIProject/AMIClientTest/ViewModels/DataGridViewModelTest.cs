using AMIClient;
using AMIClient.ViewModels;
using FTN.Common;
using FTN.ServiceContracts;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class DataGridViewModelTest
    {
        private DataGridViewModel dataGrid;
        private Model model;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.model = new Model();
            this.dataGrid = new DataGridViewModel();
            this.dataGrid.SetModel(model);

            //    Logger.Path = "TestClient.txt";
        }

        [Test]
        public void NameFilterTest()
        {
            string name = "Name";
            dataGrid.NameFilter = name;

            Assert.AreEqual(name, dataGrid.NameFilter);
        }

        [Test]
        public void TypeFilterTest()
        {
            dataGrid.NameFilter = string.Empty;
            string type = "ENERGY_CONSUMER";
            dataGrid.TypeFilter = type;

            Assert.AreEqual(type, dataGrid.TypeFilter);
        }

        [Test]
        public void InstanceTest()
        {
            DataGridViewModel dataGridVM = DataGridViewModel.Instance;
            Assert.IsNotNull(dataGridVM);
        }

        [Test]
        public void ModelTest()
        {
            dataGrid.Model = model;

            Assert.AreEqual(model, dataGrid.Model);
        }

        [Test]
        public void SetModelTest()
        {
            Assert.DoesNotThrow(() => dataGrid.SetModel(model));
        }

        [Test]
        public void ShowAmisActionTest()
        {
            Substation sub = new Substation(1);
            Assert.DoesNotThrow(() => dataGrid.ShowAmisCommand.Execute(sub));
        }

        [Test]
        public void SelectedAMIActionTest()
        {
            EnergyConsumer consumer = new EnergyConsumer(21474836481);

            Assert.DoesNotThrow(() => dataGrid.IndividualAmiChartCommand.Execute(consumer));
        }

        [Test]
        public void SelectedAMIHourActionTest()
        {
            Substation substation = new Substation(17179869185);
            substation.Name = "Substation";

            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subStation = new ResourceDescription();
            ResourceDescription ami = new ResourceDescription();
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 17179869185));
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subStation };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret2);
            mock2.IteratorClose(1);

            NetworkPreviewViewModel network = new NetworkPreviewViewModel();
            network.Model = new Model();
            network.Model.FirstContact = false;
            network.Model.GdaQueryProxy = mock2;
            
            Assert.DoesNotThrow(() => dataGrid.IndividualAmiHourChartCommand.Execute(substation));
        }

        [Test]
        public void SelectedAMIDayActionSubgeoregionTest()
        {
            SubGeographicalRegion subgeoregion = new SubGeographicalRegion(8589934593);
            subgeoregion.Name = "Subgeoregion";

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

            NetworkPreviewViewModel network = new NetworkPreviewViewModel();
            network.Model = new Model();
            network.Model.FirstContact = false;
            network.Model.GdaQueryProxy = mock2;
            Assert.DoesNotThrow(() => dataGrid.IndividualAmiDayChartCommand.Execute(subgeoregion));
        }

        [Test]
        public void SelectedAMIDayActionGeoregionTest()
        {
            GeographicalRegion georegin = new GeographicalRegion(4294967297);
            georegin.Name = "Georegion";

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

            NetworkPreviewViewModel network = new NetworkPreviewViewModel();
            network.Model = new Model();
            network.Model.FirstContact = false;
            network.Model.GdaQueryProxy = mock2;

            Assert.DoesNotThrow(() => dataGrid.IndividualAmiDayChartCommand.Execute(georegin));
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.dataGrid.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.dataGrid.NameFilter = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("NameFilter", receivedEvents);
        }
    }
}
