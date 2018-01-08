using FTN.Common;
using FTN.Common.Logger;
using FTN.Services.NetworkModelService;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkModelServiceTest
{
    [TestFixture]
    public class ResourceIteratorTest
    {
        private ResourceIterator ri;

        [OneTimeSetUp]
        public void Init()
        {
            Logger.Path = "TestNMS.txt";
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(25769803777);
            DMSType type2 = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(42949672961);
            Dictionary<FTN.Common.DMSType, List<FTN.Common.ModelCode>> a = new Dictionary<FTN.Common.DMSType, List<FTN.Common.ModelCode>>();
            a.Add(type, new List<ModelCode>());
            a.Add(type2, new List<ModelCode>());
            ResourceIterator.NetworkModel = new NetworkModel();
            ResourceIterator.NetworkModel.Dispose();
            ResourceIterator.NetworkModel.CreateContainer(25769803777);
            ResourceIterator.NetworkModel.CreateContainer(42949672961);
            ResourceIterator.NetworkModel.FillContainer(new FTN.Common.ResourceDescription(25769803777));
            ResourceIterator.NetworkModel.FillContainer(new FTN.Common.ResourceDescription(42949672961));
            ri = new ResourceIterator(new List<long>() { 25769803777, 42949672961 }, a);
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new ResourceIterator());
            Assert.DoesNotThrow(() => new ResourceIterator(new List<long>(), new Dictionary<FTN.Common.DMSType, List<FTN.Common.ModelCode>>()));
        }

        [Test]
        public void ResourcesLeftTest()
        {
            Init();
            Assert.AreEqual(2, ri.ResourcesLeft());
        }

        [Test]
        public void ResourcesTotalTest()
        {
            Init();
            Assert.AreEqual(2, ri.ResourcesTotal());
        }

        [Test]
        public void RewindTest()
        {
            Assert.DoesNotThrow(() => ri.Rewind());
        }

        [Test]
        public void NextIsNull()
        {
            Init();
            Assert.IsNull(ri.Next(-1));
        }

        [Test]
        public void NextTest()
        {
            Init();
            Assert.AreEqual(2, ri.Next(5001).Count);
        }

        [Test]
        public void Next2()
        {
            Init();
            Assert.AreEqual(1, ri.Next(1).Count);
        }

        [Test]
        public void Next3()
        {
            DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(25769803777);
            DMSType type2 = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(42949672961);
            Dictionary<FTN.Common.DMSType, List<FTN.Common.ModelCode>> a = new Dictionary<FTN.Common.DMSType, List<FTN.Common.ModelCode>>();
            a.Add(type, new List<ModelCode>());
            a.Add(type2, new List<ModelCode>());
            ri = new ResourceIterator(new List<long>() { 25769803777, 42949672961 }, a);
            ResourceIterator.NetworkModel = new NetworkModel();
            ResourceIterator.NetworkModel.Dispose();

            Assert.Throws<Exception>(() => ri.Next(1));
        }
    }
}
