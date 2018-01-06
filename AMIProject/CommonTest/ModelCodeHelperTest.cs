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
    public class ModelCodeHelperTest
    {
        long globalId = 187273492876;

        [Test]
        public void ExtractTypeFromGlobalIdTest()
        {
            short result = ModelCodeHelper.ExtractTypeFromGlobalId(globalId);
            Assert.AreNotEqual(result, globalId);
        }

        [Test]
        public void ExtractEntityIdFromGlobalIdTest()
        {
            Assert.DoesNotThrow(() => ModelCodeHelper.ExtractEntityIdFromGlobalId(globalId));
        }

        [Test]
        public void CreateGlobalIdTest()
        {
            Assert.DoesNotThrow(() => ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ANALOG, 19283));
        }

        [Test]
        public void GetTypeFromModelCodeTest()
        {
            DMSType result = ModelCodeHelper.GetTypeFromModelCode(ModelCode.ANALOG);
            Assert.AreEqual(DMSType.ANALOG, result);
        }
    }
}
