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
    public class PropertyValueTest
    {
        private PropertyValue value;
        List<long> longValues = new List<long>() { 424948523564, 5430538372 };
        List<float> floatValues = new List<float>() { 424948523564, 5430538372 };
        List<string> stringValues = new List<string>() { "IDObj", "Meas" };
        private long longValue = 424948523564;
        private float floatValue = 424948523564;
        private string stringValue = "Meas";

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.value = new PropertyValue();
            this.value.LongValues = longValues;
            this.value.FloatValues = floatValues;
            this.value.StringValues = stringValues;
            this.value.FloatValue = floatValue;
            this.value.FloatValue = floatValue;
            this.value.StringValue = stringValue;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new PropertyValue());
        }

        [Test]
        public void ConstructorWithParameterTest()
        {
            Assert.DoesNotThrow(() => new PropertyValue(value));
            Assert.DoesNotThrow(() => new PropertyValue(longValue));
            Assert.DoesNotThrow(() => new PropertyValue(floatValue));
            Assert.DoesNotThrow(() => new PropertyValue(stringValue));
            Assert.DoesNotThrow(() => new PropertyValue(longValues));
            Assert.DoesNotThrow(() => new PropertyValue(floatValues));
            Assert.DoesNotThrow(() => new PropertyValue(stringValues));
        }

        [Test]
        public void LongValuesTest()
        {
            value.LongValues = longValues;

            Assert.AreEqual(longValues, value.LongValues);
        }

        [Test]
        public void FloatValuesTest()
        {
            value.FloatValues = floatValues;

            Assert.AreEqual(floatValues, value.FloatValues);
        }

        [Test]
        public void StringValuesTest()
        {
            value.StringValues = stringValues;

            Assert.AreEqual(stringValues, value.StringValues);
        }

        [Test]
        public void LongValueTest()
        {
            value.LongValue = longValue;

            Assert.AreEqual(longValue, value.LongValue);
        }

        [Test]
        public void FloatValueTest()
        {
            value.FloatValue = floatValue;

            Assert.AreEqual(floatValue, value.FloatValue);
        }

        [Test]
        public void StringValueTest()
        {
            value.StringValue = stringValue;

            Assert.AreEqual(stringValue, value.StringValue);
        }

        [Test]
        public void TestEquals()
        {
            PropertyValue value1 = null;
            PropertyValue value2 = null;
            Assert.AreEqual(true, value1 == value2);

            value1 = new PropertyValue();
            Assert.AreNotEqual(true, value1 == value2);

            value1 = null;
            value2 = new PropertyValue();
            Assert.AreNotEqual(true, value1 == value2);

            List<long> longValues2 = new List<long>() { 424948523564, 5430538366 };
            value1 = new PropertyValue(longValues);
            value2 = new PropertyValue(longValues2);
            Assert.AreNotEqual(true, value1 == value2);

            List<float> floatValues2 = new List<float>() { 424948523555, 543458366 };
            value1 = new PropertyValue(floatValues);
            value2 = new PropertyValue(floatValues2);
            Assert.AreNotEqual(true, value1 == value2);

            List<string> stringValues2 = new List<string>() { "Analog", "Discrete" };
            value1 = new PropertyValue(stringValues);
            value2 = new PropertyValue(stringValues2);
            Assert.AreNotEqual(true, value1 == value2);

            value1 = new PropertyValue(longValues);
            value2 = new PropertyValue(longValues);
            Assert.AreEqual(true, value1 == value2);
        }

        [Test]
        public void UneaqualTest()
        {
            List<long> longValues2 = new List<long>() { 424948523564, 5430538366 };
            PropertyValue value1 = new PropertyValue(longValues);
            PropertyValue value2 = new PropertyValue(longValues2);
            Assert.AreEqual(true, value1 != value2);
        }

        [Test]
        public void EqualsTest()
        {
            Assert.AreEqual(true, value.Equals(value));
        }

        [Test]
        public void EqualsFaildTest()
        {
            Property prop = new Property();
            Assert.AreNotEqual(true, value.Equals(prop));

            PropertyValue propV = new PropertyValue(424948523511);
            Assert.AreNotEqual(true, value.Equals(propV));
        }

        [Test]
        public void GetHashCodeTest()
        {
            int number = value.LongValues.GetHashCode() + value.FloatValues.GetHashCode() + value.StringValues.GetHashCode();

            Assert.AreEqual(number, value.GetHashCode());
        }
    }
}
