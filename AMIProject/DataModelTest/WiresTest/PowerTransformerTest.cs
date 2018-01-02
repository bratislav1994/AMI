﻿using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace DataModelTest.WiresTest
{
    [TestFixture]
    public class PowerTransformerTest
    {
        private PowerTransformer powerTransformer;
        private List<long> powerTransEnds = new List<long>() { 42949682122, 42949682123 };
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.powerTransformer = new PowerTransformer();
            this.powerTransformer.PowerTransEnds = powerTransEnds;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new PowerTransformer());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new PowerTransformer(42949682121));
        }

        [Test]
        public void PowerTransEndsTest()
        {
            powerTransformer.PowerTransEnds = powerTransEnds;

            Assert.AreEqual(powerTransEnds, powerTransformer.PowerTransEnds);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.powerTransformer;
            bool result = powerTransformer.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            object obj = null;
            bool result = powerTransformer.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.POWERTRANSFORMER_POWTRANSENDS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = powerTransformer.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = powerTransformer.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.POWERTRANSFORMER_POWTRANSENDS)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => powerTransformer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => powerTransformer.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_NAME, "PowerTransformer_1")]
        public void SetPropertyTestCorrects(ModelCode t, string value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => powerTransformer.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.ANALOG_MAXVALUE, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.DoesNotThrow(() => powerTransformer.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(true, powerTransformer.IsReferenced);
            PowerTransformer pt = new PowerTransformer();
            Assert.AreEqual(false, pt.IsReferenced);
            pt.PowerTransEnds = powerTransEnds;
            Assert.AreEqual(true, pt.IsReferenced);
        }

        [Test]
        [TestCase(TypeOfReference.Target)]
        public void GetReferencesTest(TypeOfReference refType)
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => powerTransformer.GetReferences(references, refType));
            Assert.DoesNotThrow(() => powerTransformer.GetReferences(references, TypeOfReference.Both));

            BaseVoltage bv = new BaseVoltage() { ConductingEquipments = null };
            Assert.DoesNotThrow(() => powerTransformer.GetReferences(references, refType));
            bv.TransformerEnds = new List<long>();
            Assert.DoesNotThrow(() => powerTransformer.GetReferences(references, refType));
            bv.TransformerEnds = powerTransEnds;
            Assert.DoesNotThrow(() => powerTransformer.GetReferences(references, TypeOfReference.Reference));
        }

        [Test]
        [TestCase(ModelCode.POWERTRANSEND_POWERTRANSF, 42949682124)]
        public void AddReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => powerTransformer.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682125)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => powerTransformer.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.POWERTRANSEND_POWERTRANSF, 42949682124)]
        [TestCase(ModelCode.POWERTRANSEND_POWERTRANSF, 42949682126)]
        public void RemoveReferenceTestCorrect(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => powerTransformer.RemoveReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.VOLTAGELEVEL_SUBSTATION, 42949682125)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.DoesNotThrow(() => powerTransformer.RemoveReference(referenceId, globalId));
        }

        [Test]
        public void GetHashCodeTest()
        {
            PowerTransformer pt = new PowerTransformer() { Name = "pt" };
            int hashCode = pt.GetHashCode();
            PowerTransformer pt2 = new PowerTransformer() { Name = "pt" };
            int hashCodeBv = pt2.GetHashCode();
            Assert.AreNotEqual(hashCode, hashCodeBv);
            pt = pt2;
            Assert.AreEqual(pt.GetHashCode(), pt2.GetHashCode());
        }

        [Test]
        public void DeepCopyTest()
        {
            PowerTransformerEnd pte = new PowerTransformerEnd() { GlobalId = 1234, Mrid = "123" };
            Assert.AreEqual(pte, pte.DeepCopy());
        }
    }
}
