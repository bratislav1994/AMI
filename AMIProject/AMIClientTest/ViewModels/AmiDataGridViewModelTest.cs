using AMIClient;
using AMIClient.ViewModels;
using FTN.Common;
using FTN.Common.Logger;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class AmiDataGridViewModelTest
    {
        private AmiDataGridViewModel adgvm;
        private Model model = new Model();

        [OneTimeSetUp]
        public void SetupTest()
        {
            Logger.Path = "TestClient.txt";
        }

        [Test]
        public void NameFilterTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(model);
            this.adgvm.NameFilter = "testNameFilter";
            Assert.AreEqual(this.adgvm.NameFilter, "testNameFilter");
        }

        [Test]
        public void TypeFilterTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(model);
            this.adgvm.TypeFilter = "testTypeFilter";
            Assert.AreEqual(this.adgvm.TypeFilter, "testTypeFilter");
        }

        [Test]
        public void ModelTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(model);
            Assert.IsNotNull(this.adgvm.Model);
        }

        [Test]
        public void ParentGidGeoRegionTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(new Model());
            int countForResourcesLeft1 = 0;
            int countForResourcesLeft2 = 0;
            int countForResourcesLeft3 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subGeoRegion = new ResourceDescription();
            ResourceDescription subStation = new ResourceDescription();
            ResourceDescription ami = new ResourceDescription();
            subGeoRegion.AddProperty(new Property(ModelCode.IDOBJ_GID, 4321));
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            ami.AddProperty(new Property(ModelCode.IDOBJ_NAME, "ami"));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subGeoRegion };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { subStation };
            List<ResourceDescription> ret3 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(2).Returns(x => (countForResourcesLeft2++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(3).Returns(x => (countForResourcesLeft3++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret1);
            mock2.IteratorNext(10, 2).Returns(ret2);
            mock2.IteratorNext(10, 3).Returns(ret3);

            mock2.IteratorClose(1);
            mock2.IteratorClose(2);
            mock2.IteratorClose(3);

            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
            ISmartCacheDuplexForClient mock3 = Substitute.For<ISmartCacheDuplexForClient>();
            List<DynamicMeasurement> retMeas = new List<DynamicMeasurement>();
            mock3.GetLastMeas().ReturnsForAnyArgs(retMeas);
            model.ScProxy = mock3;
            model.FirstContactSC = false;
            this.adgvm.Model = (model);

            this.adgvm.ParentType = DMSType.GEOREGION;
            Assert.AreEqual(DMSType.GEOREGION, this.adgvm.ParentType);
            this.adgvm.ParentGid = 4320;

            Assert.AreEqual(1, this.adgvm.AmiTableItems.Count);
        }

        [Test]
        public void ParentGidSubGeoRegionTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(model);
            int countForResourcesLeft1 = 0;
            int countForResourcesLeft2 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subGeoRegion = new ResourceDescription();
            ResourceDescription subStation = new ResourceDescription();
            ResourceDescription ami = new ResourceDescription();
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            ami.AddProperty(new Property(ModelCode.IDOBJ_NAME, "ami"));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subStation };
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(2).Returns(x => (countForResourcesLeft2++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret1);
            mock2.IteratorNext(10, 2).Returns(ret2);

            mock2.IteratorClose(1);
            mock2.IteratorClose(2);

            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
            ISmartCacheDuplexForClient mock3 = Substitute.For<ISmartCacheDuplexForClient>();
            List<DynamicMeasurement> retMeas = new List<DynamicMeasurement>();
            mock3.GetLastMeas().ReturnsForAnyArgs(retMeas);
            model.ScProxy = mock3;
            model.FirstContactSC = false;
            this.adgvm.Model = (model);

            this.adgvm.ParentType = DMSType.SUBGEOREGION;
            Assert.AreEqual(DMSType.SUBGEOREGION, this.adgvm.ParentType);
            this.adgvm.ParentGid = 4320;

            Assert.AreEqual(1, this.adgvm.AmiTableItems.Count);
        }

        [Test]
        public void ParentGidSubStationTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(model);
            this.adgvm.NameFilter = "ami";
            this.adgvm.TypeFilter = "ene";
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription ami = new ResourceDescription();
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 25769803779));
            ami.AddProperty(new Property(ModelCode.IDOBJ_NAME, "ami"));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret1);

            mock2.IteratorClose(1);

            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
            ISmartCacheDuplexForClient mock3 = Substitute.For<ISmartCacheDuplexForClient>();
            List<DynamicMeasurement> retMeas = new List<DynamicMeasurement>();
            mock3.GetLastMeas().ReturnsForAnyArgs(retMeas);
            model.ScProxy = mock3;
            model.FirstContactSC = false;
            this.adgvm.Model = (model);
            List<DynamicMeasurement> list = new List<DynamicMeasurement>() { new DynamicMeasurement(25769803779) { CurrentP = 10, CurrentQ = 10, CurrentV = 10 } };
            this.adgvm.Model.SendMeasurements(list);

            this.adgvm.ParentType = DMSType.SUBSTATION;
            Assert.AreEqual(DMSType.SUBSTATION, this.adgvm.ParentType);
            this.adgvm.ParentGid = 4320;

            Assert.AreEqual(1, this.adgvm.AmiTableItems.Count);
            Assert.IsNotNull(this.adgvm.AmiTableItems);
        }

        [Test]
        public void CheckForUpdatesTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(model);
            this.adgvm.NameFilter = "ene";
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription ami = new ResourceDescription();
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 25769803779));
            ami.AddProperty(new Property(ModelCode.IDOBJ_NAME, "ami"));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();
            mock2.GetRelatedValues(1234, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);

            mock2.IteratorNext(10, 1).Returns(ret1);

            mock2.IteratorClose(1);

            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
            ISmartCacheDuplexForClient mock3 = Substitute.For<ISmartCacheDuplexForClient>();
            List<DynamicMeasurement> retMeas = new List<DynamicMeasurement>();
            mock3.GetLastMeas().ReturnsForAnyArgs(retMeas);
            model.ScProxy = mock3;
            model.FirstContactSC = false;
            this.adgvm.Model = (model);
            Thread.Sleep(1000);
            

            this.adgvm.ParentType = DMSType.SUBSTATION;
            Assert.AreEqual(DMSType.SUBSTATION, this.adgvm.ParentType);
            this.adgvm.ParentGid = 4320;

            List<DynamicMeasurement> list = new List<DynamicMeasurement>() { new DynamicMeasurement(25769803779) { CurrentP = 10, CurrentQ = 10, CurrentV = 10 } };
            this.adgvm.Model.SendMeasurements(list);
            Thread.Sleep(1000);
            Assert.AreEqual(1, this.adgvm.AmiTableItems.Count);
            Assert.IsNotNull(this.adgvm.AmiTableItems);
            Assert.DoesNotThrow(() => this.adgvm.Dispose());
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.adgvm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.adgvm.AmiTableItems = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("AmiTableItems", receivedEvents);
        }

        [Test]
        public void CommandTest()
        {
            this.adgvm = new AmiDataGridViewModel();
            this.adgvm.SetModel(model);

            Assert.DoesNotThrow(() => this.adgvm.IndividualAmiChartCommand.Execute(new IdentifiedObject() { GlobalId = 1234, Name = "name"}));
            Assert.DoesNotThrow(() => this.adgvm.IndividualAmiDayChartCommand.Execute(new IdentifiedObject() { GlobalId = 1234, Name = "name" }));
            Assert.DoesNotThrow(() => this.adgvm.IndividualAmiHourChartCommand.Execute(new IdentifiedObject() { GlobalId = 1234, Name = "name" }));
        }
    }
}
