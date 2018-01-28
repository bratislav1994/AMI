using AMIClient;
using AMIClient.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string type = "Type";
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

        //[Test]
        //public void ShowAmisActionTest()
        //{
        //    Assert.DoesNotThrow(() => dataGrid.ShowAmisCommand.Execute());
        //}


    }
}
