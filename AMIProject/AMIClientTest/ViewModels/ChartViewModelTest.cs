using AMIClient;
using AMIClient.ViewModels;
using FTN.Common;
using FTN.Common.Logger;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class ChartViewModelTest
    {
        private ChartViewModel chartVM;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.chartVM = new ChartViewModel();
            Logger.Path = "TestClient.txt";
        }

        [Test]
        public void InstanceTest()
        {
            ChartViewModel chart = ChartViewModel.Instance;
            Assert.IsNotNull(chart);
        }

        [Test]
        public void DataHistoryPTest()
        {
            List<KeyValuePair<DateTime, float>> tempP = new List<KeyValuePair<DateTime, float>>();
            tempP.Add(new KeyValuePair<DateTime, float>(DateTime.Now, 55));
            chartVM.DataHistoryP = tempP;

            Assert.AreEqual(1, chartVM.DataHistoryP.Count);
            Assert.AreEqual(55, chartVM.DataHistoryP[0].Value);
        }

        [Test]
        public void DataHistoryQTest()
        {
            List<KeyValuePair<DateTime, float>> tempQ = new List<KeyValuePair<DateTime, float>>();
            tempQ.Add(new KeyValuePair<DateTime, float>(DateTime.Now, 55));
            chartVM.DataHistoryQ = tempQ;

            Assert.AreEqual(1, chartVM.DataHistoryQ.Count);
            Assert.AreEqual(55, chartVM.DataHistoryQ[0].Value);
        }

        [Test]
        public void DataHistoryVTest()
        {
            List<KeyValuePair<DateTime, float>> tempV = new List<KeyValuePair<DateTime, float>>();
            tempV.Add(new KeyValuePair<DateTime, float>(DateTime.Now, 55));
            chartVM.DataHistoryV = tempV;

            Assert.AreEqual(1, chartVM.DataHistoryV.Count);
            Assert.AreEqual(55, chartVM.DataHistoryV[0].Value);
        }

        [Test]
        public void FromPeriodTest()
        {
            string dt = DateTime.Now.ToString();
            chartVM.FromPeriod = dt;

            Assert.AreEqual(dt, chartVM.FromPeriod);
        }

        [Test]
        public void ModelTest()
        {
            Model m = new Model();
            chartVM.Model = m;

            Assert.AreEqual(m, chartVM.Model);
        }

        [Test]
        public void DateTimePickTest()
        {
            chartVM.DateTimePick = Visibility.Visible;

            Assert.AreEqual(Visibility.Visible, chartVM.DateTimePick);
        }

        [Test]
        public void DatePickTest()
        {
            chartVM.DatePick = Visibility.Visible;

            Assert.AreEqual(Visibility.Visible, chartVM.DatePick);
        }

        [Test]
        public void ResolutionTest()
        {
            chartVM.Resolution = ResolutionType.MINUTE;
            Assert.AreEqual(Visibility.Visible, chartVM.DateTimePick);

            chartVM.Resolution = FTN.Common.ResolutionType.DAY;
            Assert.AreEqual(Visibility.Visible, chartVM.DatePick);
            Assert.AreEqual(ResolutionType.DAY, chartVM.Resolution);
        }

        [Test]
        public void AmiGidsTest()
        {
            chartVM.AmiGids = new List<long>() { 1 };
            Assert.AreEqual(1, chartVM.AmiGids.Count);
        }

        [Test]
        public void StatisticsTest()
        {
            chartVM.Statistics = null;
            Assert.IsNull(chartVM.Statistics);
        }

        [Test]
        public void CanShowDataExecuteTest()
        {
            Assert.IsFalse(chartVM.ShowDataCommand.CanExecute());
            chartVM.FromPeriod = DateTime.Now.ToString();
            Assert.IsTrue(chartVM.ShowDataCommand.CanExecute());
            chartVM.FromPeriod = "fsda";
            Assert.IsFalse(chartVM.ShowDataCommand.CanExecute());
        }

        [Test]
        public void SetGidsTest()
        {
            Assert.DoesNotThrow(() => chartVM.SetGids(new List<long>() { 2, 3 }));
        }

        [Test]
        public void ShowCommandActionTest()
        {
            chartVM.Model = new Model();
            chartVM.Model.FirstContactCE = false;
            chartVM.FromPeriod = DateTime.Now.ToString();
            chartVM.Resolution = ResolutionType.MINUTE;
            chartVM.AmiGids = new List<long>() { 1 };
            ICalculationForClient mock = Substitute.For<ICalculationForClient>();
            Tuple<List<Statistics>, Statistics> measForChart = null;
            mock.GetMeasurementsForChartView(chartVM.AmiGids, DateTime.Parse(chartVM.FromPeriod), chartVM.Resolution).Returns(measForChart);
            this.chartVM.Model.CEQueryProxy = mock;

            Assert.DoesNotThrow(() => this.chartVM.ShowDataCommand.Execute());

            chartVM.Resolution = ResolutionType.HOUR;
            Assert.DoesNotThrow(() => this.chartVM.ShowDataCommand.Execute());

            chartVM.Resolution = ResolutionType.DAY;
            Assert.DoesNotThrow(() => this.chartVM.ShowDataCommand.Execute());

            List<Statistics> s1 = new List<Statistics>() { new Statistics() { TimeStamp = DateTime.Now, AvgP = 22 } };
            Statistics s2 = new Statistics();
            Tuple<List<Statistics>, Statistics> measForChart2 = new Tuple<List<Statistics>, Statistics>(s1, s2);
            ICalculationForClient mock2 = Substitute.For<ICalculationForClient>();
            mock2.GetMeasurementsForChartView(chartVM.AmiGids, DateTime.Parse(chartVM.FromPeriod), chartVM.Resolution).ReturnsForAnyArgs(measForChart2);
            this.chartVM.Model.CEQueryProxy = mock2;
            Assert.DoesNotThrow(() => this.chartVM.ShowDataCommand.Execute());
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.chartVM.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };
            
            this.chartVM.DataHistoryP = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("DataHistoryP", receivedEvents);
        }
    }
}
