using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTest.TraceTest
{
    [TestFixture]
    public class CommonTraceTest
    {
        [Test]
        public void TraceErrorTest()
        {
            Assert.AreEqual(false, CommonTrace.TraceError);
        }

        [Test]
        public void TraceInfoTest()
        {
            Assert.AreEqual(false, CommonTrace.TraceInfo);
        }

        [Test]
        public void TraceVerboseTest()
        {
            Assert.AreEqual(false, CommonTrace.TraceVerbose);
        }

        [Test]
        public void TraceWarningTest()
        {
            Assert.AreEqual(false, CommonTrace.TraceWarning);
        }

        [Test]
        public void TraceLevelTest()
        {
            Assert.AreEqual(TraceLevel.Off, CommonTrace.TraceLevel);
        }

        [Test]
        public void WriteTraceTest()
        {
            long globalId = 2836456284652;
            long refGlobalId = 12948372673;

            string message = "Error - Test";
            Assert.DoesNotThrow(() => CommonTrace.WriteTrace(CommonTrace.TraceError, message));

            message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
            Assert.DoesNotThrow(() => CommonTrace.WriteTrace(CommonTrace.TraceInfo, message));

            //message = "Test - Updating entity with GID ({ 0:x16}) {0}";
            //Assert.DoesNotThrow(() => CommonTrace.WriteTrace(CommonTrace.TraceVerbose, message, globalId));

            //message = "Test - Updating entity with GID ({ 0:x16})";
            //Assert.DoesNotThrow(() => CommonTrace.WriteTrace(CommonTrace.TraceInfo, message, globalId));

            message = "Test - Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}";
            Assert.DoesNotThrow(() => CommonTrace.WriteTrace(CommonTrace.TraceWarning, message, globalId, refGlobalId)); 
        }
    }
}
