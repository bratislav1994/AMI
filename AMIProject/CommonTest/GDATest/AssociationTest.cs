using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTest.GDATest
{
    [TestFixture]
    public class AssociationTest
    {
        private Association association;
        private bool inverse = true;
        private ModelCode propertyId = ModelCode.GEOREGION_SUBGEOREGIONS;
        private ModelCode type = ModelCode.SUBGEOREGION;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.association = new Association();
            this.association.Inverse = inverse;
            this.association.PropertyId = propertyId;
            this.association.Type = type;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Association());
        }

        [Test]
        public void ConstructorWithParameter1Test()
        {
            Assert.DoesNotThrow(() => new Association(ModelCode.GEOREGION_SUBGEOREGIONS));
        }

        [Test]
        public void ConstructorWithParameter2Test()
        {
            Assert.DoesNotThrow(() => new Association(ModelCode.GEOREGION_SUBGEOREGIONS, true));
        }

        [Test]
        public void ConstructorWithParameter3Test()
        {
            Assert.DoesNotThrow(() => new Association(ModelCode.GEOREGION_SUBGEOREGIONS, ModelCode.SUBGEOREGION));
        }

        [Test]
        public void ConstructorWithParameter4Test()
        {
            Assert.DoesNotThrow(() => new Association(ModelCode.BASEVOLTAGE_NOMINALVOL, ModelCode.BASEVOLTAGE, true));
        }

        //[Test]
        //public void ConstructorWithParameter5Test()
        //{
        //    Assert.DoesNotThrow(() => new Association(ModelCode.BASEVOLTAGE));
        //}

        //[Test]
        //public void ConstructorWithParameterTest()
        //{
        //    Assert.DoesNotThrow(() => new Association(ModelCode.BASEVOLTAGE));
        //}

        [Test]
        public void InverseTest()
        {
            association.Inverse = inverse;

            Assert.AreEqual(inverse, association.Inverse);
        }

        [Test]
        public void PropertyIdTest()
        {
            association.PropertyId = propertyId;

            Assert.AreEqual(propertyId, association.PropertyId);
        }

        [Test]
        public void TypeTest()
        {
            association.Type = type;

            Assert.AreEqual(type, association.Type);
        }
    }
}
