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
    public class ResolvedAlarmsViewModelTest
    {
        private ResolvedAlarmsViewModel resolvedAlarm;
        private Model model;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.resolvedAlarm = new ResolvedAlarmsViewModel();
            this.model = new Model();
            this.resolvedAlarm.SetModel(model);
            //    Logger.Path = "TestClient.txt";
        }

        [Test]
        public void InstanceTest()
        {
            ResolvedAlarmsViewModel alarmA = ResolvedAlarmsViewModel.Instance;
            Assert.IsNotNull(alarmA);
        }

        [Test]
        public void ModelTest()
        {
            resolvedAlarm.Model = model;

            Assert.AreEqual(model, resolvedAlarm.Model);
        }

        [Test]
        public void ConsumerFilterTest()
        {
            string consumerFilter = "consumer";
            resolvedAlarm.ConsumerFilter = consumerFilter;

            Assert.AreEqual(consumerFilter, resolvedAlarm.ConsumerFilter);
        }

        [Test]
        public void StatusFilterTest()
        {
            resolvedAlarm.ConsumerFilter = string.Empty;
            string status = "ACTIVE";
            resolvedAlarm.StatusFilter = status;

            Assert.AreEqual(status, resolvedAlarm.StatusFilter);
        }

        [Test]
        public void TypeVoltageFilterTest()
        {
            resolvedAlarm.ConsumerFilter = string.Empty;
            resolvedAlarm.StatusFilter = string.Empty;
            string type = "UNDERVOLTAGE";
            resolvedAlarm.TypeVoltageFilter = type;

            Assert.AreEqual(type, resolvedAlarm.TypeVoltageFilter);
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.resolvedAlarm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.resolvedAlarm.ConsumerFilter = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("ConsumerFilter", receivedEvents);
        }
    }
}
