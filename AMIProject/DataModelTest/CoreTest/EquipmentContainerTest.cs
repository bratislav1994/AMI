﻿using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace DataModelTest.CoreTest
{
    [TestFixture]
    public class EquipmentContainerTest
    {
        private EquipmentContainer eqContainer;
        private List<long> equipments = new List<long>() { 42949672911, 42949672912 };
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.eqContainer = new EquipmentContainer();
            this.eqContainer.Equipments = equipments;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new EquipmentContainer());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new EquipmentContainer(42949672901));
        }

        [Test]
        public void EquipmentsTest()
        {
            eqContainer.Equipments = equipments;

            Assert.AreEqual(equipments, eqContainer.Equipments);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.eqContainer;
            bool result = eqContainer.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.EQCONTAINER_EQUIPMENTS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = eqContainer.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = eqContainer.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.EQCONTAINER_EQUIPMENTS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => eqContainer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => eqContainer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME, "EqContainer_1")]
        public void SetPropertyTestCorrects(ModelCode t, string value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);
            
            Assert.DoesNotThrow(() => eqContainer.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => eqContainer.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, eqContainer.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => eqContainer.GetReferences(references, refType));
        }

        [Test]
        [TestCase(ModelCode.EQUIPMENT_EQCONTAINER, 42949672913)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => eqContainer.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949672843)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<Exception>(() => eqContainer.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.EQUIPMENT_EQCONTAINER, 42949672913)]
        [TestCase(ModelCode.EQUIPMENT_EQCONTAINER, 42949672914)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => eqContainer.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949672843)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<ModelException>(() => eqContainer.RemoveReference(referenceId, globalId));
        }
    }
}