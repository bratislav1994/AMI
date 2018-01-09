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
using FTN.Common;

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

            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(25769803779);
            DMSType type2 = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(42949672961);
            gda.NetworkModel = new NetworkModel();
            gda.NetworkModel.Dispose();
            gda.NetworkModel.CreateContainer(25769803779);
            gda.NetworkModel.CreateContainer(42949672961);
            ResourceDescription rd1 = new ResourceDescription(25769803779);
            ResourceDescription rd2 = new ResourceDescription(42949672961);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803779));
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_RTUADDRESS, 10));
            gda.NetworkModel.FillContainer(rd1);
            gda.NetworkModel.FillContainer(rd2);
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

        [Test]
        public void EnlistDeltaTest()
        {
            Init();
            Assert.DoesNotThrow(() => gda.EnlistDelta(new FTN.Common.Delta()));
        }

        [Test]
        public void PrepareFailValidationTest()
        {
            Init();
            gda.NetworkModel = new NetworkModel();
            gda.NetworkModel.Dispose();
            Delta delta = new Delta();
            ResourceDescription rd1 = new ResourceDescription(42949672961);
            rd1.AddProperty(new Property(ModelCode.IDOBJ_MRID, "test"));
            ResourceDescription rd2 = new ResourceDescription(42949672957);
            rd2.AddProperty(new Property(ModelCode.IDOBJ_MRID, "test"));
            delta.InsertOperations.Add(rd1);
            delta.InsertOperations.Add(rd2);
            gda.EnlistDelta(delta);
            Assert.IsNull(gda.Prepare());
        }

        [Test]
        public void PrepareTest()
        {
            Init();
            gda.NetworkModel = new NetworkModel();
            gda.NetworkModel.Dispose();
            Delta delta = new Delta();
            ResourceDescription rd1 = new ResourceDescription(42949672959);
            rd1.AddProperty(new Property(ModelCode.IDOBJ_MRID, "test"));
            delta.InsertOperations.Add(rd1);
            gda.EnlistDelta(delta);
            Assert.DoesNotThrow(() => gda.Prepare());
            gda.NetworkModel.IsTest = true;
            Assert.DoesNotThrow(() => gda.Commit());

            Delta delta2 = new Delta();
            ResourceDescription rd2 = new ResourceDescription(42949672957);
            rd2.AddProperty(new Property(ModelCode.IDOBJ_MRID, "test"));
            delta2.InsertOperations.Add(rd2);
            gda.EnlistDelta(delta2);
            Assert.IsNull(gda.Prepare());
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

        [Test]
        public void GetValuesTest()
        {
            Init();
            Assert.IsNotNull(gda.GetValues(25769803779, new List<ModelCode>() { ModelCode.IDOBJ_GID }));
        }

        [Test]
        public void GetValuesFailTest()
        {
            Init();
            gda.NetworkModel = null;
            Assert.Throws<Exception>(() => gda.GetValues(25769803779, new List<ModelCode>() { ModelCode.IDOBJ_GID }));
        }

        [Test]
        public void GlobalIdsTest()
        {
            Init();
            Assert.AreEqual(2, gda.GetGlobalIds().Count);
        }

        [Test]
        public void GlobalIdsFailTest()
        {
            Init();
            gda.NetworkModel = null;
            Assert.Throws<Exception>(() => gda.GetGlobalIds());
        }

        [Test]
        public void GetExtentValuesTest()
        {
            Init();
            Assert.IsNotNull(gda.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID }));
        }

        [Test]
        public void GetExtentValuesFailTest()
        {
            Init();
            gda.NetworkModel = null;
            Assert.Throws<Exception>(() => gda.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID }));
        }

        [Test]
        public void GetRelatedValuesTest1()
        {
            Init();
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.PSR_MEASUREMENTS;
            associtaion.Type = ModelCode.ANALOG;
            Assert.IsNotNull(gda.GetRelatedValues(25769803779, new List<ModelCode>() { ModelCode.IDOBJ_GID }, associtaion));
        }

        [Test]
        public void GetRelatedValuesFailTest()
        {
            Init();
            gda.NetworkModel = null;
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.PSR_MEASUREMENTS;
            associtaion.Type = ModelCode.ANALOG;
            Assert.Throws<Exception>(() => gda.GetRelatedValues(25769803779, new List<ModelCode>() { ModelCode.IDOBJ_GID }, associtaion));
        }

        [Test]
        public void IteratorNextTest()
        {
            Init();
            ResourceIterator.NetworkModel = gda.NetworkModel;
            int id = gda.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID });
            Assert.IsNotNull(gda.IteratorNext(1, id));
        }

        [Test]
        public void IteratorCloseTest()
        {
            Init();
            ResourceIterator.NetworkModel = gda.NetworkModel;
            int id = gda.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID });
            Assert.IsTrue(gda.IteratorClose(id));
        }

        [Test]
        public void IteratorNextFailTest()
        {
            Init();
            ResourceIterator.NetworkModel = null;
            Assert.Throws<Exception>(() => gda.IteratorNext(1, 1));
        }

        [Test]
        public void ResourcesLeftTest()
        {
            Init();
            ResourceIterator.NetworkModel = gda.NetworkModel;
            int id = gda.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID });
            Assert.AreEqual(1, gda.IteratorResourcesLeft(id));
        }

        [Test]
        public void ResourcesLeftFailTest()
        {
            Init();
            ResourceIterator.NetworkModel = null;
            gda.ClearResourceMap();
            Assert.Throws<Exception>(() => gda.IteratorResourcesLeft(1));
        }

        [Test]
        public void ResourcesTotalTest()
        {
            Init();
            ResourceIterator.NetworkModel = gda.NetworkModel;
            int id = gda.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID });
            Assert.AreEqual(1, gda.IteratorResourcesTotal(id));
        }

        [Test]
        public void ResourcesTotalFailTest()
        {
            Init();
            ResourceIterator.NetworkModel = null;
            gda.ClearResourceMap();
            Assert.Throws<Exception>(() => gda.IteratorResourcesTotal(1));
        }

        [Test]
        public void IteratorRewindTest()
        {
            Init();
            ResourceIterator.NetworkModel = gda.NetworkModel;
            int id = gda.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID });
            Assert.IsTrue(gda.IteratorRewind(id));
        }

        [Test]
        public void IteratorRewindFailTest()
        {
            Init();
            ResourceIterator.NetworkModel = null;
            gda.ClearResourceMap();
            Assert.Throws<Exception>(() => gda.IteratorRewind(1));
        }
    }
}
