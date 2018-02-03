using AMIClient;
using AMIClient.ViewModels;
using FTN.Common.Logger;
using FTN.ServiceContracts;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class MasterViewModelTest
    {
        private MasterViewModel master;
        private Model model;

        [OneTimeSetUp]
        public void SetupTest()
        {
            Logger.Path = "TestClient.txt";

            ISmartCacheDuplexForClient mock = Substitute.For<ISmartCacheDuplexForClient>();
            mock.Subscribe();
            ICalculationForClient mock2 = Substitute.For<ICalculationForClient>();
            mock2.ConnectClient();
            INetworkModelGDAContractDuplexClient mock3 = Substitute.For<INetworkModelGDAContractDuplexClient>();
            mock3.ConnectClient();
            mock3.When(fake => fake.Ping()).Do(call => { throw new Exception(); });

            this.model = new Model();
            this.model.CEQueryProxy = mock2;
            this.model.GdaQueryProxy = mock3;
            this.model.ScProxy = mock;
            this.model.FirstContact = false;
            this.model.FirstContactCE = false;
            this.model.FirstContactSC = false;

            this.model.IsTest = true;
            this.model.Start();

            //this.master.Model = model;
            this.master = new MasterViewModel();
        }

        [Test]
        public void XmlvmTest()
        {
            AddCimXmlViewModel cimXml = new AddCimXmlViewModel();
            master.Xmlvm = cimXml;

            Assert.AreEqual(cimXml, master.Xmlvm);
        }

        [Test]
        public void TvmTest()
        {
            NetworkPreviewViewModel tvm = new NetworkPreviewViewModel();
            master.Tvm = tvm;

            Assert.AreEqual(tvm, master.Tvm);
        }

        [Test]
        public void ChartVMTest()
        {
            ChartViewModel chart = new ChartViewModel();
            master.ChartVM = chart;

            Assert.AreEqual(chart, master.ChartVM);
        }

        [Test]
        public void AlarmVMTest()
        {
            AlarmSummariesViewModel alarmVM = new AlarmSummariesViewModel();
            master.AlarmVM = alarmVM;

            Assert.AreEqual(alarmVM, master.AlarmVM);
        }

        [Test]
        public void ModelTest()
        {
            master.Model = model;

            Assert.AreEqual(model, master.Model);
        }

        [Test]
        public void CurrentViewModelTest()
        {
            object currentView = new object();
            master.CurrentViewModel = currentView;

            Assert.AreEqual(currentView, master.CurrentViewModel);
        }

        [Test]
        public void NetworkPreviewActionTest()
        {
            Assert.DoesNotThrow(() => master.NetworkPreviewCommand.Execute());
        }

        [Test]
        public void AddCimXmlActionTest()
        {
            Assert.DoesNotThrow(() => master.AddCimXmlCommand.Execute());
        }

        [Test]
        public void ChartActionTest()
        {
            Assert.DoesNotThrow(() => master.ChartCommand.Execute());
        }

        [Test]
        public void AlarmSummariesActionTest()
        {
            Assert.DoesNotThrow(() => master.AlarmSummariesCommand.Execute());
        }

        [Test]
        public void DgVMTest()
        {
            DataGridViewModel dataGrid = new DataGridViewModel();
            master.DgVM = dataGrid;

            Assert.AreEqual(dataGrid, master.DgVM);
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.master.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.master.CurrentViewModel = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("CurrentViewModel", receivedEvents);
        }
    }
}
