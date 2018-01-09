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

            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(25769803777);
            DMSType type2 = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(42949672961);
            nm.CreateContainer(25769803777);
            nm.CreateContainer(42949672961);
            ResourceDescription rd1 = new ResourceDescription(25769803777);
            ResourceDescription rd2 = new ResourceDescription(42949672961);
            rd2.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, 25769803777));
            rd1.AddProperty(new Property(ModelCode.PSR_MEASUREMENTS, new List<long>() { 42949672961 }));
            nm.FillContainer(rd1);
            nm.FillContainer(rd2);
        }

        [Test]
        public void GerValuesTest()
        {
            Init();
            Assert.AreEqual(25769803777, nm.GetValues(25769803777, new List<ModelCode>() { ModelCode.IDOBJ_GID }).Id);
        }

        [Test]
        public void GlobalIdsTest()
        {
            Init();
            Assert.AreEqual(2, nm.GetGlobalIds().Count);
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
            Assert.Throws<Exception>( () => nm.GetRelatedValues(25769803777, new List<ModelCode>() { ModelCode.IDOBJ_GID }, null));
        }

        [Test]
        public void GetRelatedValuesTest1()
        {
            Init();
            Association associtaion = new Association();
            associtaion.PropertyId = ModelCode.PSR_MEASUREMENTS;
            associtaion.Type = ModelCode.ANALOG;
           Assert.IsNotNull(nm.GetRelatedValues(25769803777, new List<ModelCode>() { ModelCode.IDOBJ_GID }, associtaion));
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
    }
}
