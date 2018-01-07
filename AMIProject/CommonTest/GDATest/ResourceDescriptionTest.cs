using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FTN.Common.ResourceDescription;

namespace CommonTest.GDATest
{
    [TestFixture]
    public class ResourceDescriptionTest
    {
        private ResourceDescription rd;
        private long id = 183843728945;
        private List<Property> properties = new List<Property>() { new Property(ModelCode.ANALOG_MAXVALUE, new PropertyValue(100)), new Property(ModelCode.ANALOG_MINVALUE, new PropertyValue(50)) };

        private EqualityComparer ec;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.rd = new ResourceDescription();
            this.rd.Id = id;
            this.rd.Properties = properties;

            this.ec = new EqualityComparer();
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new ResourceDescription());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new ResourceDescription(id));
            Assert.DoesNotThrow(() => new ResourceDescription(id, properties));
            Assert.DoesNotThrow(() => new ResourceDescription(rd));
        }

        [Test]
        public void IdTest()
        {
            rd.Id = id;

            Assert.AreEqual(id, rd.Id);
        }

        [Test]
        public void PropertiesTest()
        {
            rd.Properties = properties;

            Assert.AreEqual(properties, rd.Properties);
        }

        [Test]
        public void AddPropertyTest()
        {
            Assert.DoesNotThrow(() => rd.AddProperty(new Property(ModelCode.ANALOG_ALARMHIGH, new PropertyValue(150))));

            Assert.DoesNotThrow(() => rd.AddProperty(new Property(ModelCode.ANALOG_MINVALUE, new PropertyValue(50))));
        }

        #region EqualityComparerTest

        [Test]
        public void EqualsTest()
        {
            ResourceDescription rd1 = null;
            Assert.Throws<NullReferenceException>(() => ec.Equals(rd, rd1));

            ResourceDescription rd2 = null;
            rd1 = rd;
            Assert.Throws<NullReferenceException>(() => ec.Equals(rd1, rd2));

            rd2 = new ResourceDescription();
            rd2.Id = 8476485947364;
            rd2.Properties = new List<Property>() { new Property(ModelCode.BASEVOLTAGE, new PropertyValue(293837288274)), new Property(ModelCode.BASEVOLTAGE_NOMINALVOL, new PropertyValue(100)) };
            
            bool result = ec.Equals(rd, rd2);
            Assert.AreNotEqual(true, result);

            rd2.Id = rd.Id;
            rd.Properties = new List<Property>();
            rd2.Properties = new List<Property>();
            result = ec.Equals(rd, rd2);
            Assert.AreEqual(true, result);

            rd.Properties = new List<Property>() { new Property(ModelCode.ANALOG_MAXVALUE, new PropertyValue(100)), new Property(ModelCode.ANALOG_MINVALUE, new PropertyValue(50)) };
            result = ec.Equals(rd, rd2);
            Assert.AreNotEqual(true, result);

            rd2.Properties = new List<Property>() { new Property(ModelCode.BASEVOLTAGE, new PropertyValue(293837288274)), new Property(ModelCode.BASEVOLTAGE_NOMINALVOL, new PropertyValue(100)) };
            result = ec.Equals(rd, rd2);
            Assert.AreNotEqual(true, result);

            rd2.Properties = rd.Properties;
            result = ec.Equals(rd, rd2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetHashCodeTest()
        {
            string value = null;
            this.rd.Properties.Add(new Property(ModelCode.IDOBJ_NAME, new PropertyValue(value)));

            Assert.DoesNotThrow(() => ec.GetHashCode(rd));
        }

        #endregion
    }
}
