using AMIClient;
using AMIClient.ViewModels;
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

        //[OneTimeSetUp]
        //public void SetupTest()
        //{
        //    this.model = new Model();
        //    this.master = new MasterViewModel();

        //    //    Logger.Path = "TestClient.txt";
        //}

        //[Test]
        //public void XmlvmTest()
        //{
        //    AddCimXmlViewModel cimXml = new AddCimXmlViewModel();
        //    master.Xmlvm = cimXml;

        //    Assert.AreEqual(cimXml, master.Xmlvm);
        //}

        //[Test]
        //public void TvmTest()
        //{
        //    NetworkPreviewViewModel tvm = new NetworkPreviewViewModel();
        //    master.Tvm = tvm;

        //    Assert.AreEqual(tvm, master.Tvm);
        //}

        //[Test]
        //public void ChartVMTest()
        //{
        //    ChartViewModel chart = new ChartViewModel();
        //    master.ChartVM = chart;

        //    Assert.AreEqual(chart, master.ChartVM);
        //}

        //[Test]
        //public void AlarmVMTest()
        //{
        //    AlarmSummariesViewModel alarmVM = new AlarmSummariesViewModel();
        //    master.AlarmVM = alarmVM;

        //    Assert.AreEqual(alarmVM, master.AlarmVM);
        //}

        //[Test]
        //public void ModelTest()
        //{
        //    Model model = new Model();
        //    master.Model = model;

        //    Assert.AreEqual(model, master.Model);
        //}

        //[Test]
        //public void CurrentViewModelTest()
        //{
        //    object currentView = new object();
        //    master.CurrentViewModel = currentView;

        //    Assert.AreEqual(currentView, master.CurrentViewModel);
        //}

        //[Test]
        //public void NetworkPreviewActionTest()
        //{
        //    Assert.DoesNotThrow(() => master.NetworkPreviewCommand.Execute());
        //}

        //[Test]
        //public void AddCimXmlActionTest()
        //{
        //    Assert.DoesNotThrow(() => master.AddCimXmlCommand.Execute());
        //}

        //[Test]
        //public void ChartActionTest()
        //{
        //    Assert.DoesNotThrow(() => master.ChartCommand.Execute());
        //}

        //[Test]
        //public void AlarmSummariesActionTest()
        //{
        //    Assert.DoesNotThrow(() => master.AlarmSummariesCommand.Execute());
        //}

        //[Test]
        //public void DgVMTest()
        //{
        //    DataGridViewModel dataGrid = new DataGridViewModel();
        //    master.DgVM = dataGrid;

        //    Assert.AreEqual(dataGrid, master.DgVM);
        //}

        //[Test]
        //public void RaisePropertyChangedTest()
        //{
        //    string receivedEvents = null;
        //    this.master.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
        //    {
        //        receivedEvents = e.PropertyName;
        //    };

        //    this.master.CurrentViewModel = null;
        //    Assert.IsNotNull(receivedEvents);
        //    Assert.AreEqual("CurrentViewModel", receivedEvents);
        //}
    }
}
