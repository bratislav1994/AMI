using AMIClient;
using FTN.Common.Logger;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Wires;

namespace AMIClientTest.ClassesTest
{
    [TestFixture]
    public class TreeClassesTest
    {
        private ObservableCollection<EnergyConsumer> amis = new ObservableCollection<EnergyConsumer>();
        private Model model = new Model();
        private TreeClasses treeClasses;

        [OneTimeSetUp]
        public void Init()
        {
            Logger.Path = "TestClient.txt";
        }

        //[OneTimeSetUp]
        public void SetupTest()
        {
            this.treeClasses = new TreeClasses();
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new TreeClasses());
        }

        [Test]
        public void ConstructorTestWithParameters()
        {
            Assert.DoesNotThrow(() => new TreeClasses(new TreeClasses(), model));
        }

        [Test]
        public void LoadChildrenTest()
        {
            SetupTest();
            this.treeClasses.LoadChildren();
        }

        [Test]
        public void ChildrenTest()
        {
            this.SetupTest();
            Assert.IsNotNull(this.treeClasses.Children);
        }

        [Test]
        public void ParentTest()
        {
            TreeClasses obj = new TreeClasses(new TreeClasses(), model);
            Assert.IsNotNull(obj.Parent);
        }

        [Test]
        public void ModelTest()
        {
            Model temp = this.model;
            Assert.AreEqual(temp, this.model);

            this.treeClasses.Model = this.model;
            Assert.IsNotNull(this.treeClasses.Model);
        }

        [Test]
        public void CallTest()
        {
            this.SetupTest();
            Assert.DoesNotThrow(() => treeClasses.CheckIfSeleacted());
        }
    }
}
