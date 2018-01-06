using FTN.Common.Logger;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTest.LoggerTest
{
    [TestFixture]
    public class LoggerTest
    {
        private string path = string.Empty;

        [OneTimeSetUp]
        public void SetupTest()
        {
            Logger.Path = path;
        }

        [Test]
        public void PathTest()
        {
            Logger.Path = path;

            Assert.AreEqual(path, Logger.Path);
        }

        [Test]
        public void LogMessageToFileTest()
        {
            Logger.Path = "loggerTest.txt";
            Assert.DoesNotThrow(() => Logger.LogMessageToFile("Testiranje"));
        }
    }
}
