﻿using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Wires;

namespace DataModelTest.WiresTest
{
    [TestFixture]
    public class EnergyConsumerTest
    {
        private EnergyConsumer consumer;
        private long globalID = 42949682361;
        private float pfixed = 0;
        private float qfixed = 0;
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.consumer = new EnergyConsumer();
            this.consumer.PMax = pfixed;
            this.consumer.QMax = qfixed;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new EnergyConsumer());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new EnergyConsumer(globalID));
        }

        [Test]
        public void PMaxTest()
        {
            consumer.PMax = pfixed;

            Assert.AreEqual(pfixed, consumer.PMax);
        }

        [Test]
        public void QMaxTest()
        {
            consumer.QMax = qfixed;

            Assert.AreEqual(qfixed, consumer.QMax);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.consumer;
            bool result = consumer.Equals(obj);

            Assert.AreEqual(true, result);

            // incorrect
            obj = new EnergyConsumer() { PMax = 1 };
            result = consumer.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new EnergyConsumer() { QMax = 1 };
            result = consumer.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new EnergyConsumer() { ValidRangePercent = 1 };
            result = consumer.Equals(obj);
            Assert.AreNotEqual(true, result);

            obj = new EnergyConsumer() { InvalidRangePercent = 1 };
            result = consumer.Equals(obj);
            Assert.AreNotEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = consumer.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void ToStringTest()
        {
            consumer.Mrid = "CONSUMER_1";
            consumer.Name = "Consumer_1";
            string region = string.Format(consumer.Mrid + "\n" + consumer.Name);

            string result = consumer.ToString();

            Assert.AreEqual(region, result);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = consumer.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = consumer.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => consumer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => consumer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.IDOBJ_NAME:
                    property.SetValue("Consumer_1");
                    break;
            }

            Assert.DoesNotThrow(() => consumer.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => consumer.SetProperty(property));
        }

        [Test]
        public void RD2ClassTest()
        {
            ResourceDescription rd = new ResourceDescription(globalID);

            ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
            List<ModelCode> properties = modelResourcesDesc.GetAllPropertyIds(ModelCode.PSR_MEASUREMENTS);

            for (int i = 0; i < properties.Count; i++)
            {
                rd.AddProperty(new Property(properties[i]));
            }

            rd.Properties.First().PropertyValue.LongValue = 1246;
            Assert.DoesNotThrow(() => consumer.RD2Class(rd));
        }

        [Test]
        public void GetHashCodeTest()
        {
            EnergyConsumer ec = new EnergyConsumer() { Name = "ec" };
            int hashCode = ec.GetHashCode();
            EnergyConsumer ec2 = new EnergyConsumer() { Name = "ec" };
            int hashCodeBv = ec2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            ec = ec2;
            Assert.AreEqual(ec.GetHashCode(), ec2.GetHashCode());
        }

        [Test]
        public void DeepCopyTest()
        {
            EnergyConsumer ec = new EnergyConsumer() { GlobalId = 1234, Mrid = "123" };
            Assert.AreEqual(ec, ec.DeepCopy());
        }
    }
}
