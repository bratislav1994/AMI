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
            string consumerFilter = "consumerFilter";
            alarm.ConsumerFilter = consumerFilter;

            Assert.AreEqual(consumerFilter, alarm.ConsumerFilter);
        }

        [Test]
        public void StatusFilterTest()
        {
            string status = "statusFilter";
            alarm.StatusFilter = status;

            Assert.AreEqual(status, alarm.StatusFilter);
        }

        [Test]
        public void TypeVoltageFilterTest()
        {
            string type = "typeFilter";
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
