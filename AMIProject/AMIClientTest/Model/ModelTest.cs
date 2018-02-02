using AMIClient;
using FTN.ServiceContracts;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using NSubstitute.ExceptionExtensions;
using FTN.Common.Logger;
using FTN.Services.NetworkModelService.DataModel;
using TC57CIM.IEC61970.Core;
using FTN.Common;

namespace AMIClientTest.ModelTest
{
    [TestFixture]
    public class ModelTest
    {
        private Model model;
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

        [OneTimeSetUp]
        public void SetupTest()
        {
            Logger.Path = "TestClient.txt";
            this.model = new Model();
        }

        [Test]
        public void TableItemsTest()
        {
            this.model.TableItems = new System.Collections.ObjectModel.ObservableCollection<TableItem>();
            Assert.IsNotNull(this.model.TableItems);
        }

        [Test]
        public void ViewTableItemsTest()
        {
            this.model.TableItems = new System.Collections.ObjectModel.ObservableCollection<TableItem>();
            this.model.ViewTableItems = new CollectionViewSource { Source = this.model.TableItems }.View;
            this.model.ViewTableItems = CollectionViewSource.GetDefaultView(this.model.TableItems);
            Assert.IsNotNull(this.model.ViewTableItems);
        }
        
        //[Test]
        //public void ViewTableItemsForAlarmTest()
        //{
        //    this.model.TableItemsForAlarm = new System.Collections.ObjectModel.ObservableCollection<AMIClient.Classes.TableItemForAlarm>();
        //    this.model.ViewTableItemsForAlarm = new CollectionViewSource { Source = this.model.TableItemsForAlarm }.View;
        //    this.model.ViewTableItemsForAlarm = CollectionViewSource.GetDefaultView(this.model.TableItemsForAlarm);
        //    Assert.IsNotNull(this.model.ViewTableItemsForAlarm);
        //}

        [Test]
        public void StartTest()
        {
            ISmartCacheDuplexForClient mock = Substitute.For<ISmartCacheDuplexForClient>();
            mock.Subscribe();
            ICalculationForClient mock2 = Substitute.For<ICalculationForClient>();
            mock2.ConnectClient();
            INetworkModelGDAContractDuplexClient mock3 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            mock3.ConnectClient();
            mock3.When(fake => fake.Ping()).Do(call => { throw new Exception(); });

            this.model.CEQueryProxy = mock2;
            this.model.GdaQueryProxy = mock3;
            this.model.ScProxy = mock;
            this.model.FirstContact = false;
            this.model.FirstContactCE = false;
            this.model.FirstContactSC = false;
            //Action action = () => mock4.Ping();
            //action.ShouldThrow<Exception>(); // Pass
            this.model.IsTest = true;
            Assert.DoesNotThrow(() => this.model.Start());
        }

        [Test]
        public void SetRootTest()
        {
            RootElement root = new RootElement();
            Assert.DoesNotThrow(() => this.model.SetRoot(root));
        }

        [Test]
        public void GetTimeOfTheLastUpdateTest()
        {
            Assert.AreNotSame(DateTime.Now, this.model.GetTimeOfTheLastUpdate());
        }

        [Test]
        public void GetSomeAmisTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subStation = new ResourceDescription();
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

            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
            ISmartCacheDuplexForClient mock3 = Substitute.For<ISmartCacheDuplexForClient>();
            List<DynamicMeasurement> retMeas = new List<DynamicMeasurement>();
            mock3.GetLastMeas().ReturnsForAnyArgs(retMeas);
            model.ScProxy = mock3;
            model.FirstContactSC = false;

            Assert.AreEqual(1, this.model.GetSomeAmis(4322).Count);
        }

        [Test]
        public void GetSomeTableItemsForSubGeoRegionTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription subStation = new ResourceDescription();
            subStation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { subStation };
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

            this.model.GetSomeTableItemsForSubGeoRegion(4322);

            // setup ends

            List<DynamicMeasurement> meas = new List<DynamicMeasurement>() { new DynamicMeasurement(4322) { CurrentP = 1, CurrentQ = 2, CurrentV = 3 },
                                                                             new DynamicMeasurement(25769803779) { CurrentP = 1, CurrentQ = 2, CurrentV = 3 } };
            List<DynamicMeasurement> meas2 = new List<DynamicMeasurement>() { new DynamicMeasurement(4322) { CurrentP = -1, CurrentQ = -1, CurrentV = -1 },
                                                                             new DynamicMeasurement(25769803779) { CurrentP = -1, CurrentQ = -1, CurrentV = -1 } };
            Assert.DoesNotThrow(() => this.model.SendMeasurements(meas));
            Assert.DoesNotThrow(() => this.model.SendMeasurements(meas2));
        }

        [Test]
        public void NewChangesAvailableTest()
        {
            Assert.IsTrue(this.model.NewChangesAvailable(DateTime.MinValue));
            Assert.IsFalse(this.model.NewChangesAvailable(DateTime.Now));
        }

        [Test]
        public void NewDeltaAppliedTest()
        {
            this.model.SetRoot(new RootElement());
            Assert.DoesNotThrow(() => this.model.NewDeltaApplied());
        }

        [Test]
        public void GetChangesTest()
        {
            List<DynamicMeasurement> meas = new List<DynamicMeasurement>() { new DynamicMeasurement(25769803779) { CurrentP = 1, CurrentQ = 2, CurrentV = 3 } };
            this.model.SendMeasurements(meas);

            Assert.AreEqual(1, this.model.GetChanges(new List<long>() { 25769803779 }).Count);
        }

        [Test]
        public void GetAllAmisTest()
        {
            int countForResourcesLeft1 = 0;
            int countForRelatedValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription ami = new ResourceDescription();
            ami.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { ami };
            Association associtaion = new Association();
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
            mock2.GetExtentValues(ModelCode.ENERGYCONS, properties).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);

            mock2.IteratorNext(2, 1).Returns(ret1);

            mock2.IteratorClose(1);

            model.FirstContact = false;
            model.GdaQueryProxy = mock2;
            ISmartCacheDuplexForClient mock3 = Substitute.For<ISmartCacheDuplexForClient>();
            List<DynamicMeasurement> retMeas = new List<DynamicMeasurement>();
            mock3.GetLastMeas().ReturnsForAnyArgs(retMeas);
            model.ScProxy = mock3;
            model.FirstContactSC = false;

            Assert.AreEqual(1, this.model.GetAllAmis().Count);
        }

        [Test]
        public void GetAllTableItemsTest()
        {
            int countForResourcesLeft1 = 0;
            int countForResourcesLeft2 = 0;
            int countForResourcesLeft3 = 0;
            int countForRelatedValues = 1;
            int countForExtentValues = 0;

            INetworkModelGDAContractDuplexClient mock2 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            ResourceDescription geoRegion = new ResourceDescription();
            ResourceDescription subGeoRegion = new ResourceDescription();
            ResourceDescription substation = new ResourceDescription();
            geoRegion.AddProperty(new Property(ModelCode.IDOBJ_GID, 4322));
            List<ResourceDescription> ret1 = new List<ResourceDescription>() { geoRegion };
            subGeoRegion.AddProperty(new Property(ModelCode.IDOBJ_GID, 4323));
            List<ResourceDescription> ret2 = new List<ResourceDescription>() { subGeoRegion };
            substation.AddProperty(new Property(ModelCode.IDOBJ_GID, 4324));
            List<ResourceDescription> ret3 = new List<ResourceDescription>() { substation };
            Association associtaion = new Association();
            List<ModelCode> properties = new List<ModelCode>();// modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONS);
            mock2.GetExtentValues(ModelCode.ENERGYCONS, properties).ReturnsForAnyArgs(x => (++countForExtentValues));

            mock2.GetRelatedValues(4322, properties, associtaion).ReturnsForAnyArgs(x => (++countForRelatedValues));
            mock2.IteratorResourcesLeft(1).Returns(x => (countForResourcesLeft1++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(2).Returns(x => (countForResourcesLeft2++ < 1) ? 1 : 0);
            mock2.IteratorResourcesLeft(3).Returns(x => (countForResourcesLeft3++ < 1) ? 1 : 0);

            mock2.IteratorNext(2, 1).Returns(ret1);
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

            Assert.DoesNotThrow(() => this.model.GetAllTableItems());
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.model.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.model.TableItems = new System.Collections.ObjectModel.ObservableCollection<TableItem>();
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("TableItems", receivedEvents);
        }
    }
}
