﻿using AMIClient;
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
        private ObservableCollection<RootElement> rootElements;
        private ObservableCollection<EnergyConsumer> amis = new ObservableCollection<EnergyConsumer>();
        private DateTime newChange;
        private IModel model;
        private TreeClasses treeClasses;
        private TreeClasses parent;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.treeClasses = new TreeClasses();
            IModel im = Substitute.For<IModel>();
            model = im;
            //rootElements = new ObservableCollection<RootElement>();
            //rootElements.Add(new RootElement(model, ref amis, ref newChange));
            //this.model.SetRoot(rootElements[0]);
            //newChange = DateTime.Now;
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
        public void ChildrenTest()
        {
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
            IModel temp = this.model;
            Assert.AreEqual(temp, this.model);

            this.treeClasses.Model = this.model;
            Assert.IsNotNull(this.treeClasses.Model);
        }

        [Test]
        public void CallTest()
        {
            Assert.DoesNotThrow(() => treeClasses.CheckIfSeleacted());
        }
    }
}
