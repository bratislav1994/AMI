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
    public class UpdateResultTest
    {
        private UpdateResult upResult;
        private Dictionary<long, long> globalIdPairs = new Dictionary<long, long>() { { 424948523555, 543458366 } };
        private string message = string.Empty;
        private ResultType result = ResultType.Succeeded;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.upResult = new UpdateResult();
            this.upResult.GlobalIdPairs = globalIdPairs;
            this.upResult.Message = message;
            this.upResult.Result = result;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new UpdateResult());
        }

        [Test]
        public void GlobalIdPairsTest()
        {
            upResult.GlobalIdPairs = globalIdPairs;

            Assert.AreEqual(globalIdPairs, upResult.GlobalIdPairs);
        }

        [Test]
        public void MessageTest()
        {
            upResult.Message = message;

            Assert.AreEqual(message, upResult.Message);
        }

        [Test]
        public void ResultTest()
        {
            upResult.Result = result;

            Assert.AreEqual(result, upResult.Result);
        }

        [Test]
        public void ToStringTest()
        {
            string results = string.Empty;
            Assert.DoesNotThrow(() => results = upResult.ToString());

            Assert.IsNotEmpty(results);
        }
    }
}
