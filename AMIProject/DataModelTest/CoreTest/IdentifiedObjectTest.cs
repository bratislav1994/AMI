using FTN.Common;
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
    public class IdentifiedObjectTest
    {
        private IdentifiedObject idObject;
        private long globalId = 42949667788;
        private string mRID = "IDOBJECT";
        private string name = "IdObject";
        public Property property = new Property();

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.idObject = new IdentifiedObject();
            this.idObject.GlobalId = globalId;
            this.idObject.Mrid = mRID;
            this.idObject.Name = name;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new IdentifiedObject());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new IdentifiedObject(globalId));
        }

        [Test]
        public void GlobalIdTest()
        {
            idObject.GlobalId = globalId;

            Assert.AreEqual(globalId, idObject.GlobalId);
        }

        [Test]
        public void MrIDTest()
        {
            idObject.Mrid = mRID;

            Assert.AreEqual(mRID, idObject.Mrid);
        }

        [Test]
        public void NameTest()
        {
            idObject.Name = name;

            Assert.AreEqual(name, idObject.Name);
        }

        [Test]
        public void EqualsTestCorrect()
        {
            object obj = this.idObject;
            bool result = idObject.Equals(obj);

            Assert.AreEqual(true, result);
        }

        [Test]
        public void EqualsTestFalse()
        {
            Direction psr = new Direction();
            object obj = psr;
            bool result = idObject.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void EqualsTestNull()
        {
            IdentifiedObject idObjectNull = null;
            object obj = idObjectNull;
            bool result = idObject.Equals(obj);

            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestDifferent()
        {
            Assert.AreEqual(true, this.idObject != new IdentifiedObject());
        }

        [Test]
        public void TestEquals()
        {
            IdentifiedObject io = null;
            IdentifiedObject io2 = null;
            //io = this.idObject;
            Assert.AreEqual(true, io == io2);
            io2 = new IdentifiedObject();
            Assert.AreEqual(false, io == io2);
            io = new IdentifiedObject();
            io2 = null;
            Assert.AreEqual(false, io == io2);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_GID)]
        [TestCase(ModelCode.IDOBJ_MRID)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void HasPropertyTestTrue(ModelCode t)
        {
            bool result = idObject.HasProperty(t);

            Assert.AreEqual(true, result);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void HasPropertyTestFalse(ModelCode t)
        {
            bool result = idObject.HasProperty(t);

            Assert.AreEqual(false, result);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_GID)]
        [TestCase(ModelCode.IDOBJ_MRID)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void GetPropertyTestCorrect(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.DoesNotThrow(() => idObject.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL)]
        public void GetPropertyTestFalse(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            Assert.Throws<Exception>(() => idObject.GetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_GID)]
        [TestCase(ModelCode.IDOBJ_MRID)]
        [TestCase(ModelCode.IDOBJ_NAME)]
        public void SetPropertyTestCorrects(ModelCode t)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();

            switch (property.Id)
            {
                case ModelCode.IDOBJ_GID:
                    property.SetValue(globalId);
                    break;
                case ModelCode.IDOBJ_MRID:
                    property.SetValue(mRID);
                    break;
                case ModelCode.IDOBJ_NAME:
                    property.SetValue(name);
                    break;
            }

            Assert.DoesNotThrow(() => idObject.SetProperty(property));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 15000)]
        public void SetPropertyTestFalse(ModelCode t, float value)
        {
            property.Id = t;
            property.PropertyValue = new PropertyValue();
            property.SetValue(value);

            Assert.Throws<Exception>(() => idObject.SetProperty(property));
        }

        [Test]
        public void IsReferencedTest()
        {
            Assert.AreEqual(false, idObject.IsReferenced);
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 42949682356)]
        public void AddReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<Exception>(() => idObject.AddReference(referenceId, globalId));
        }

        [Test]
        [TestCase(ModelCode.BASEVOLTAGE_NOMINALVOL, 42949682356)]
        public void RemoveReferenceTestFalse(ModelCode referenceId, long globalId)
        {
            Assert.Throws<ModelException>(() => idObject.RemoveReference(referenceId, globalId));
        }

        [Test]
        public void GetReferencesTest()
        {
            Dictionary<ModelCode, List<long>> references = new Dictionary<ModelCode, List<long>>();

            Assert.DoesNotThrow(() => idObject.GetReferences(references));
        }

        //[Test]
        //[TestCase(false)]
        //public void GetAsResourceDescriptionTestFalse(bool onlySettableAttributes)
        //{
        //    ResourceDescription rd = new ResourceDescription(globalId);
        //    rd.AddProperty(new Property(ModelCode.RATIOTAPCHANGER_TRANSEND));
        //    rd.AddProperty(new Property(ModelCode.IDOBJ_GID));
        //    rd.AddProperty(new Property(ModelCode.IDOBJ_MRID));
        //    rd.AddProperty(new Property(ModelCode.IDOBJ_NAME));
        //    rd.AddProperty(new Property(ModelCode.PSR_MEASUREMENTS));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_HIGHSTEP));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_LOWSTEP));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_NEUTRALSTEP));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_NORMALSTEP));

        //    ResourceDescription result = idObject.GetAsResourceDescription(onlySettableAttributes);
        //    Assert.AreEqual(rd, result);
        //}

        //[Test]
        //[TestCase(true)]
        //public void GetAsResourceDescriptionTestTrue(bool onlySettableAttributes)
        //{
        //    ResourceDescription rd = new ResourceDescription(globalId);
        //    rd.AddProperty(new Property(ModelCode.RATIOTAPCHANGER_TRANSEND));
        //    rd.AddProperty(new Property(ModelCode.IDOBJ_MRID));
        //    rd.AddProperty(new Property(ModelCode.IDOBJ_NAME));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_HIGHSTEP));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_LOWSTEP));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_NEUTRALSTEP));
        //    rd.AddProperty(new Property(ModelCode.TAPCHANGER_NORMALSTEP));

        //    ResourceDescription result = idObject.GetAsResourceDescription(onlySettableAttributes);
        //    Assert.AreEqual(rd, result);
        //}

        [Test]
        public void GetAsResourceDescriptionTest()
        {
            ResourceDescription rd = new ResourceDescription(globalId);
            rd.AddProperty(new Property(ModelCode.IDOBJ_GID, globalId));
            rd.AddProperty(new Property(ModelCode.IDOBJ_MRID, mRID));
            rd.AddProperty(new Property(ModelCode.IDOBJ_NAME, name));

            List<ModelCode> propIds = new List<ModelCode>() { ModelCode.IDOBJ_GID, ModelCode.IDOBJ_MRID, ModelCode.IDOBJ_NAME };

            ResourceDescription result = idObject.GetAsResourceDescription(propIds);
            Assert.AreEqual(rd.Properties.Count, result.Properties.Count);
        }

        [Test]
        [TestCase(ModelCode.IDOBJ_MRID)]
        public void GetProperty(ModelCode property)
        {
            Property prop = new Property(property, mRID);

            Assert.AreEqual(prop, idObject.GetProperty(property));
        }

        //[Test]
        //public void GetDifferentProperties()
        //{
        //    List<Property> valuesInCompared1 = new List<Property>();
        //    List<Property> valuesInCompared2 = new List<Property>();
        //    List<Property> valuesInOriginal1 = new List<Property>();
        //    List<Property> valuesInOriginal2 = new List<Property>();

        //    long glId = 6321456432;
        //    IdentifiedObject idObjCompared = new IdentifiedObject(glId);
        //    idObjCompared.GlobalId = glId;
        //    idObjCompared.Mrid = "IO1";
        //    idObjCompared.Name = "idObj1";

        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_GID, globalId));
        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_MRID, mRID));
        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_NAME, name));

        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_GID, glId));
        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_MRID, "IO1"));
        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_NAME, "idObj1"));

        //    idObject.GetDifferentProperties(idObjCompared, out valuesInOriginal1, out valuesInCompared1);
        //    Assert.AreEqual(valuesInCompared1, valuesInCompared2);
        //    Assert.AreEqual(valuesInOriginal1, valuesInOriginal2);
        //}

        //[Test]
        //public void GetDifferentPropertiesNull()
        //{
        //    IdentifiedObject idObjNull = null;
        //    List<Property> valuesInCompared1 = new List<Property>();
        //    List<Property> valuesInCompared2 = new List<Property>();
        //    List<Property> valuesInOriginal1 = new List<Property>();
        //    List<Property> valuesInOriginal2 = new List<Property>();
        //    valuesInOriginal2.Add(new Property(ModelCode.RATIOTAPCHANGER_TRANSEND));
        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_GID));
        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_MRID));
        //    valuesInOriginal2.Add(new Property(ModelCode.IDOBJ_NAME));
        //    valuesInOriginal2.Add(new Property(ModelCode.PSR_MEASUREMENTS));
        //    valuesInOriginal2.Add(new Property(ModelCode.TAPCHANGER_HIGHSTEP));
        //    valuesInOriginal2.Add(new Property(ModelCode.TAPCHANGER_LOWSTEP));
        //    valuesInOriginal2.Add(new Property(ModelCode.TAPCHANGER_NEUTRALSTEP));
        //    valuesInOriginal2.Add(new Property(ModelCode.TAPCHANGER_NORMALSTEP));

        //    idObject.GetDifferentProperties(idObjNull, out valuesInOriginal1, out valuesInCompared1);
        //    Assert.AreEqual(valuesInCompared1, valuesInCompared2);
        //    Assert.AreEqual(valuesInOriginal1, valuesInOriginal2);
        //}
    }
}
