using AMIClient;
using AMIClient.ViewModels;
using FTN.Common.Logger;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClientTest.ViewModels
{
    [TestFixture]
    public class NetworkPreviewViewModelTest
    {
        private NetworkPreviewViewModel networkPreview;
        private Model model;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.model = new Model();
            this.networkPreview = new NetworkPreviewViewModel();

            Logger.Path = "TestClient.txt";
        }

        [Test]
        public void InstanceTest()
        {
            NetworkPreviewViewModel network = NetworkPreviewViewModel.Instance;
            Assert.IsNotNull(network);
        }

        [Test]
        public void RootElementsTest()
        {
            ObservableCollection<RootElement> root = new ObservableCollection<RootElement>();
            root.Add(new RootElement(model));
            networkPreview.RootElements = root;

            Assert.AreEqual(root, networkPreview.RootElements);
        }

        [Test]
        public void ModelTest()
        {
            networkPreview.Model = model;

            Assert.AreEqual(model, networkPreview.Model);
        }

        [Test]
        public void SetModelTest()
        {
            Assert.DoesNotThrow(() => networkPreview.SetModel(model));
        }

        //[Test]
        //public void RaisePropertyChangedTest()
        //{
        //    string receivedEvents = null;
        //    this.networkPreview.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
        //    {
        //        receivedEvents = e.PropertyName;
        //    };

        //    this.networkPreview.CurrentViewModel = null;
        //    Assert.IsNotNull(receivedEvents);
        //    Assert.AreEqual("CurrentViewModel", receivedEvents);
        //}
    }
}
