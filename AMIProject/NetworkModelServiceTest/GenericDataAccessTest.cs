using FTN.Common.Logger;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService;

namespace NetworkModelServiceTest
{
    [TestFixture]
    public class GenericDataAccessTest
    {
        private GenericDataAccess gda;

        [OneTimeSetUp]
        public void Init()
        {
            Logger.Path = "TestNMS.txt";
            var ProxyCoordinator = Substitute.For<ITransactionDuplexNMS>();
            gda = new GenericDataAccess() { ProxyCoordinator = ProxyCoordinator, FirstTimeCoordinator = false };
            gda.ProxyCoordinator.ConnectNMS();
        }

        [Test]
        public void ConstructorTest()
        {
            var ProxyCoordinator = Substitute.For<ITransactionDuplexNMS>();
            GenericDataAccess gda = new GenericDataAccess() { ProxyCoordinator = ProxyCoordinator, FirstTimeCoordinator = false };
            gda.ProxyCoordinator.ConnectNMS();
            Assert.DoesNotThrow(() => gda.Run());
        }

        [Test]
        public void NetworkModelTest()
        {
            Init();
            gda.NetworkModel = new NetworkModel();
            gda.NetworkModel.Dispose();
            Assert.IsNotNull(gda.NetworkModel);
        }

        [Test]
        public void FirstTimeCoordinatorTest()
        {
            Init();
            gda.FirstTimeCoordinator = true;
            Assert.AreEqual(true, gda.FirstTimeCoordinator);
        }

        //[Test]
        //public void ConnectClientTest()
        //{
        //    Init();
        //    var networkModelProxy = Substitute.For<INetworkModelForTest>();
        //    networkModelProxy.ConnectClient();
        //    gda.SetModel(networkModelProxy);
        //    Assert.DoesNotThrow(() => gda.ConnectClient());
        //}

        [Test]
        public void EnlistDeltaTest()
        {
            Init();
            Assert.DoesNotThrow(() => gda.EnlistDelta(new FTN.Common.Delta()));
        }

        [Test]
        public void PrepareTest()
        {
            Init();
            gda.NetworkModel = new NetworkModel();
            gda.NetworkModel.Dispose();
            gda.EnlistDelta(new FTN.Common.Delta());
            Assert.DoesNotThrow(() => gda.Prepare());
        }

        [Test]
        public void CommitTest()
        {
            Init();
            gda.NetworkModel = new NetworkModel();
            gda.NetworkModel.Dispose();
            Assert.Throws<NullReferenceException>(() => gda.Commit());
        }

        [Test]
        public void RollbackTest()
        {
            Init();
            Assert.DoesNotThrow(() => gda.Rollback());
        }

        [Test]
        public void PingTest()
        {
            Init();
            Assert.DoesNotThrow(() => gda.Ping());
        }
    }
}
