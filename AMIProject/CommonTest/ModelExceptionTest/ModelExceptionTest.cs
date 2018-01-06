using FTN.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTest.ModelExceptionTest
{
    [TestFixture]
    public class ModelExceptionTest
    {
        private ModelException exception;
        private ErrorCode code = ErrorCode.Unknown;
        private long globalId = 4324566432214;

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.exception = new ModelException();
            this.exception.GlobalID = globalId;
            this.exception.Code = code;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new ModelException());
        }

        [Test]
        public void ConstructorWithParameter1Test()
        {
            Assert.DoesNotThrow(() => new ModelException("Error"));
        }

        [Test]
        public void ConstructorWithParameter2Test()
        {
            Assert.DoesNotThrow(() => new ModelException(ErrorCode.EntityNotFound, "Entity not found"));
        }

        [Test]
        public void ConstructorWithParameter3Test()
        {
            Assert.DoesNotThrow(() => new ModelException(ErrorCode.EntityNotFound, globalId, "Entity not found"));
        }

        [Test]
        public void ConstructorWithParameter4Test()
        {
            long localId = 83846382937;
            Assert.DoesNotThrow(() => new ModelException(ErrorCode.EntityNotFound, globalId, localId, "Entity not found"));
        }

        [Test]
        public void GlobalIDTest()
        {
            exception.GlobalID = globalId;

            Assert.AreEqual(globalId, exception.GlobalID);
        }

        [Test]
        public void CodeTest()
        {
            exception.Code = code;

            Assert.AreEqual(code, exception.Code);
        }
    }
}
