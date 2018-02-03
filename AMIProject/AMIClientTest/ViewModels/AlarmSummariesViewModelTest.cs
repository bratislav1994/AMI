using AMIClient;
using AMIClient.Classes;
using AMIClient.ViewModels;
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
    public class AlarmSummariesViewModelTest
    {
        private AlarmSummariesViewModel alarm;
        private Model model;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.alarm = new AlarmSummariesViewModel();
            this.model = new Model();
            this.alarm.SetModel(model);
            //    Logger.Path = "TestClient.txt";
        }

        [Test]
        public void SetModelTest()
        {
            Assert.DoesNotThrow(() => alarm.SetModel(model));
        }

        [Test]
        public void InstanceTest()
        {
            AlarmSummariesViewModel alarmS = AlarmSummariesViewModel.Instance;
            Assert.IsNotNull(alarmS);
        }

        [Test]
        public void ModelTest()
        {
            alarm.Model = model;

            Assert.AreEqual(model, alarm.Model);
        }

        [Test]
        public void ActiveAlarmsVMTest()
        {
            ActiveAlarmsViewModel activeA = new ActiveAlarmsViewModel();
            alarm.ActiveAlarmsVM = activeA;

            Assert.AreEqual(activeA, alarm.ActiveAlarmsVM);
        }

        [Test]
        public void ResolvedAlarmsVMTest()
        {
            ResolvedAlarmsViewModel resolvedA = new ResolvedAlarmsViewModel();
            alarm.ResolvedAlarmsVM = resolvedA;

            Assert.AreEqual(resolvedA, alarm.ResolvedAlarmsVM);
        }

        [Test]
        public void AlarmViewModelsTest()
        {
            BindingList<AlarmViewModel> alarmsVM = new BindingList<AlarmViewModel>();
            alarm.AlarmViewModels = alarmsVM;

            Assert.AreEqual(alarmsVM, alarm.AlarmViewModels);
        }

        [Test]
        public void SelectedTabTest()
        {
            AlarmViewModel select = new AlarmViewModel();
            alarm.SelectedTab = select;

            Assert.AreEqual(select, alarm.SelectedTab);
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.alarm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.alarm.ActiveAlarmsVM = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("ActiveAlarmsVM", receivedEvents);
        }
    }
}
