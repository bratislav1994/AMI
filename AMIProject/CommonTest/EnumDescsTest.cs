using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTest
{
    [TestFixture]
    public class EnumDescsTest
    {
        private EnumDescs enumDesc = new EnumDescs();
        private Dictionary<ModelCode, Type> property2enumType = new Dictionary<ModelCode, Type>();

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new EnumDescs());
        }

        [Test]
        public void GetEnumTypeForPropertyIdTest()
        {
            Type result = enumDesc.GetEnumTypeForPropertyId(ModelCode.MEASUREMENT_DIRECTION);
            Type expected = typeof(Direction);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetStringFromEnumTest()
        {
            string result = enumDesc.GetStringFromEnum(ModelCode.MEASUREMENT_DIRECTION, 1);
            Assert.AreEqual("READWRITE", result);

            result = enumDesc.GetStringFromEnum(ModelCode.MEASUREMENT_DIRECTION, 4);
            Assert.AreEqual("4", result);
        }
    }
}
