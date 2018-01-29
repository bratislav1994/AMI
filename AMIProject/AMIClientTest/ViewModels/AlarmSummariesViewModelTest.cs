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
        public void ConsumerFilterTest()
        {
            string consumerFilter = "consumer";
            alarm.ConsumerFilter = consumerFilter;

            Assert.AreEqual(consumerFilter, alarm.ConsumerFilter);
        }

        [Test]
        public void StatusFilterTest()
        {
            alarm.ConsumerFilter = string.Empty;
            string status = "ACTIVE";
            alarm.StatusFilter = status;

            Assert.AreEqual(status, alarm.StatusFilter);
        }

        [Test]
        public void TypeVoltageFilterTest()
        {
            alarm.ConsumerFilter = string.Empty;
            alarm.StatusFilter = string.Empty;
            string type = "UNDERVOLTAGE";
            alarm.TypeVoltageFilter = type;

            Assert.AreEqual(type, alarm.TypeVoltageFilter);
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.alarm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.alarm.StatusFilter = string.Empty;
            Assert.IsNotEmpty(receivedEvents);
            Assert.AreEqual("StatusFilter", receivedEvents);
        }
    }
}
