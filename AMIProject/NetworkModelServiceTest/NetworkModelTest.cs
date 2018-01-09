using FTN.Common;
using FTN.Common.Logger;
using FTN.Services.NetworkModelService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkModelServiceTest
{
    [TestFixture]
    public class NetworkModelTest
    {
        private NetworkModel nm;

        [OneTimeSetUp]
        public void Init()
        {
            Logger.Path = "TestNMS.txt";
            nm = new NetworkModel();
            nm.Dispose();

            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(25769803779);
            DMSType type2 = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(42949672961);
            nm.CreateContainer(25769803778);
            nm.CreateContainer(42949672961);
            ResourceDescription rd1 = new ResourceDescription(25769803779);
            ResourceDescription rd2 = new ResourceDescription(42949672961);
            ResourceDescription rd3 = new ResourceDescription(25769803776);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803779));
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_RTUADDRESS, 10));
            nm.FillContainer(rd1);
            nm.FillContainer(rd2);
            nm.FillContainer(rd3);
        }

        [Test]
        public void GetValuesTest()
        {
            Init();
            Assert.AreEqual(25769803779, nm.GetValues(25769803779, new List<ModelCode>() { ModelCode.IDOBJ_GID }).Id);
        }

        [Test]
        public void GlobalIdsTest()
        {
            Init();
            Assert.AreEqual(3, nm.GetGlobalIds().Count);
        }

        [Test]
        public void GetExtentValuesTest()
        {
            Init();
            Assert.IsNotNull(nm.GetExtentValues(ModelCode.ANALOG, new List<ModelCode>() { ModelCode.IDOBJ_GID }));
        }

        [Test]
        public void GetRelatedValuesTest()
        {
            Init();
            Assert.Throws<Exception>(() => nm.GetRelatedValues(25769803779, new List<ModelCode>() { ModelCode.IDOBJ_GID }, null));
        }

        [Test]
        public void GetRelatedValuesTest1()
        {
            Init();
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.PSR_MEASUREMENTS;
            associtaion.Type = ModelCode.ANALOG;
            Assert.IsNotNull(nm.GetRelatedValues(25769803779, new List<ModelCode>() { ModelCode.IDOBJ_GID }, associtaion));
        }

        [Test]
        public void GetRelatedValuesTest2()
        {
            Init();
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.MEASUREMENT_PSR;
            associtaion.Type = ModelCode.ENERGYCONS;
            Assert.IsNotNull(nm.GetRelatedValues(42949672961, new List<ModelCode>() { ModelCode.IDOBJ_GID }, associtaion));
        }

        [Test]
        public void ApplyDeltaTestFail1()
        {
            Init();
            Delta delta = new Delta();
            delta.InsertOperations.Add(null);
            Assert.IsNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaFail2()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd1 = new ResourceDescription(30064771071);
            delta.InsertOperations.Add(rd1);
            Assert.IsNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaInsertTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd1 = new ResourceDescription(47244640255);
            rd1.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803779));
            rd1.AddProperty(new Property(ModelCode.IDOBJ_GID, 534));
            rd1.AddProperty(new Property(ModelCode.MEASUREMENT_RTUADDRESS, 10));
            delta.InsertOperations.Add(rd1);
            Assert.IsNotNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaInsertTest2()
        {
            Logger.Path = "TestNMS.txt";
            nm = new NetworkModel();
            nm.Dispose();

            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(25769803779);
            nm.CreateContainer(25769803779);
            ResourceDescription rd1 = new ResourceDescription(25769803779);
            nm.FillContainer(rd1);

            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(47244640255);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803779));
            rd2.AddProperty(new Property(ModelCode.IDOBJ_GID, 534));
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_RTUADDRESS, 10));
            delta.InsertOperations.Add(rd2);
            Assert.IsNotNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaInsertFailTest()
        {
            Logger.Path = "TestNMS.txt";
            nm = new NetworkModel();
            nm.Dispose();

            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(25769803779);
            nm.CreateContainer(25769803779);
            ResourceDescription rd1 = new ResourceDescription(25769803779);
            nm.FillContainer(rd1);

            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(47244640255);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803777)); // fail
            rd2.AddProperty(new Property(ModelCode.IDOBJ_GID, 534));
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_RTUADDRESS, 10));
            delta.InsertOperations.Add(rd2);
            Assert.IsNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaDeleteNotExistsTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(25769803777);
            delta.DeleteOperations.Add(rd2);
            Assert.IsNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaDeleteExistsTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(25769803779);
            delta.DeleteOperations.Add(rd2);
            Assert.IsNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaDeleteTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(42949672961);
            delta.DeleteOperations.Add(rd2);
            Assert.IsNotNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaUpdateNotExistsTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(25769803777);
            delta.UpdateOperations.Add(rd2);
            Assert.IsNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaUpdateExistsTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(42949672961);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_RTUADDRESS, 11));
            delta.UpdateOperations.Add(rd2);
            Assert.IsNotNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaUpdateTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(42949672961);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803776));
            delta.UpdateOperations.Add(rd2);
            Assert.IsNotNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void ApplyDeltaUpdateTargetNotExistsTest()
        {
            Init();
            Delta delta = new Delta();
            ResourceDescription rd2 = new ResourceDescription(42949672961);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803777));
            delta.UpdateOperations.Add(rd2);
            Assert.IsNull(nm.ApplyDelta(delta));
        }

        [Test]
        public void DeepCopyTest()
        {
            List<ResourceDescription> rds = new List<ResourceDescription>(10);
            List<long> gids = new List<long>() { 17179869183, 8589934591, 12884901887, 21474836479, 25769803775, 34359738367, 38654705663,
                                                 42949672959, 30064771071, 47244640255 }; // without discrete

            for (int i = 0; i < 10; i++)
            {
                rds.Add(new ResourceDescription(gids[i]));
            }

            Delta delta = new Delta();
            delta.InsertOperations = rds;
            NetworkModel nm = new NetworkModel();
            nm.Dispose();

            nm.ApplyDelta(delta);
            NetworkModel nmCopy = new NetworkModel();
            nmCopy.networkDataModel = nm.DeepCopy();
            Assert.AreEqual(nm.networkDataModel.Count, nmCopy.networkDataModel.Count);
        }
    }
}
