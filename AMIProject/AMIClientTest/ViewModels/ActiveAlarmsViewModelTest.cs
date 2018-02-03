using AMIClient;
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
    public class ActiveAlarmsViewModelTest
    {
        private ActiveAlarmsViewModel activeAlarm;
        private Model model;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.activeAlarm = new ActiveAlarmsViewModel();
            this.model = new Model();
            this.activeAlarm.SetModel(model);
            //    Logger.Path = "TestClient.txt";
        }

        [Test]
        public void InstanceTest()
        {
            ActiveAlarmsViewModel alarmA = ActiveAlarmsViewModel.Instance;
            Assert.IsNotNull(alarmA);
        }

        [Test]
        public void ModelTest()
        {
            activeAlarm.Model = model;

            Assert.AreEqual(model, activeAlarm.Model);
        }

        [Test]
        public void ConsumerFilterTest()
        {
            string consumerFilter = "consumer";
            activeAlarm.ConsumerFilter = consumerFilter;

            Assert.AreEqual(consumerFilter, activeAlarm.ConsumerFilter);
        }

        [Test]
        public void StatusFilterTest()
        {
            activeAlarm.ConsumerFilter = string.Empty;
            string status = "ACTIVE";
            activeAlarm.StatusFilter = status;

            Assert.AreEqual(status, activeAlarm.StatusFilter);
        }

        [Test]
        public void TypeVoltageFilterTest()
        {
            activeAlarm.ConsumerFilter = string.Empty;
            activeAlarm.StatusFilter = string.Empty;
            string type = "UNDERVOLTAGE";
            activeAlarm.TypeVoltageFilter = type;

            Assert.AreEqual(type, activeAlarm.TypeVoltageFilter);
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.activeAlarm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.activeAlarm.ConsumerFilter = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("ConsumerFilter", receivedEvents);
        }
    }
}
