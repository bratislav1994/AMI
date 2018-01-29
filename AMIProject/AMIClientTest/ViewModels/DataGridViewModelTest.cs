using AMIClient;
using AMIClient.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class DataGridViewModelTest
    {
        private DataGridViewModel dataGrid;
        private Model model;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.model = new Model();
            this.dataGrid = new DataGridViewModel();
            this.dataGrid.SetModel(model);

            //    Logger.Path = "TestClient.txt";
        }

        [Test]
        public void NameFilterTest()
        {
            string name = "Name";
            dataGrid.NameFilter = name;

            Assert.AreEqual(name, dataGrid.NameFilter);
        }

        [Test]
        public void TypeFilterTest()
        {
            dataGrid.NameFilter = string.Empty;
            string type = "ENERGY_CONSUMER";
            dataGrid.TypeFilter = type;

            Assert.AreEqual(type, dataGrid.TypeFilter);
        }

        [Test]
        public void InstanceTest()
        {
            DataGridViewModel dataGridVM = DataGridViewModel.Instance;
            Assert.IsNotNull(dataGridVM);
        }

        [Test]
        public void ModelTest()
        {
            dataGrid.Model = model;

            Assert.AreEqual(model, dataGrid.Model);
        }

        [Test]
        public void SetModelTest()
        {
            Assert.DoesNotThrow(() => dataGrid.SetModel(model));
        }

        [Test]
        public void ShowAmisActionTest()
        {
            Substation sub = new Substation(1);
            Assert.DoesNotThrow(() => dataGrid.ShowAmisCommand.Execute(sub));
        }

        [Test]
        public void SelectedAMIActionTest()
        {
            EnergyConsumer consumer = new EnergyConsumer(1);

            Assert.DoesNotThrow(() => dataGrid.IndividualAmiChartCommand.Execute(consumer));
        }

        [Test]
        public void SelectedAMIHourActionTest()
        {
            EnergyConsumer consumer = new EnergyConsumer(1);

            Assert.DoesNotThrow(() => dataGrid.IndividualAmiHourChartCommand.Execute(consumer));
        }

        [Test]
        public void SelectedAMIDayActionTest()
        {
            EnergyConsumer consumer = new EnergyConsumer(1);

            Assert.DoesNotThrow(() => dataGrid.IndividualAmiDayChartCommand.Execute(consumer));
        }

        [Test]
        public void RaisePropertyChangedTest()
        {
            string receivedEvents = null;
            this.dataGrid.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                receivedEvents = e.PropertyName;
            };

            this.dataGrid.NameFilter = null;
            Assert.IsNotNull(receivedEvents);
            Assert.AreEqual("NameFilter", receivedEvents);
        }
    }
}
