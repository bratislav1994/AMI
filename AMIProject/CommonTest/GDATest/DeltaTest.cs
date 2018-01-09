using FTN.Common;
using FTN.Services.NetworkModelService;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CommonTest.GDATest
{
    [TestFixture]
    public class DeltaTest
    {
        private Delta delta;
        private long id = 42949672811;
        private List<ResourceDescription> insertOps = new List<ResourceDescription>() { new ResourceDescription(847362548924), new ResourceDescription(2234424354423) };
        private List<ResourceDescription> deleteOps = new List<ResourceDescription>() { new ResourceDescription(847362548924), new ResourceDescription(2234424354423) };
        private List<ResourceDescription> updateOps = new List<ResourceDescription>() { new ResourceDescription(847362548924), new ResourceDescription(2234424354423) };
        private bool positiveIdsAllowed = false;

        private static ModelResourcesDesc resDesc = null;

        byte[] resultSerialize = new byte[0];

        [OneTimeSetUp]
        public void SetupTest()
        {
            this.delta = new Delta();
            this.delta.Id = id;
            this.delta.DeleteOperations = deleteOps;
            this.delta.InsertOperations = insertOps;
            this.delta.UpdateOperations = updateOps;
            this.delta.PositiveIdsAllowed = positiveIdsAllowed;
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new Delta());
        }

        [Test]
        public void IdTest()
        {
            delta.Id = id;

            Assert.AreEqual(id, delta.Id);
        }

        [Test]
        public void ResDescNullTest()
        {
            Delta.ResourceDescs = resDesc;
            
            Assert.AreNotEqual(resDesc, Delta.ResourceDescs);
        }

        [Test]
        public void ResDescTest()
        {
            resDesc = new ModelResourcesDesc();
            Delta.ResourceDescs = resDesc;
            
            Assert.AreEqual(resDesc, Delta.ResourceDescs);
        }

        [Test]
        public void InsertOperationsTest()
        {
            delta.InsertOperations = insertOps;

            Assert.AreEqual(insertOps, delta.InsertOperations);
        }

        [Test]
        public void UpdateOperationsTest()
        {
            delta.UpdateOperations = updateOps;

            Assert.AreEqual(updateOps, delta.UpdateOperations);
        }

        [Test]
        public void DeleteOperationsTest()
        {
            delta.DeleteOperations = deleteOps;

            Assert.AreEqual(deleteOps, delta.DeleteOperations);
        }

        [Test]
        public void PositiveIdsAllowedTest()
        {
            delta.PositiveIdsAllowed = positiveIdsAllowed;

            Assert.AreEqual(positiveIdsAllowed, delta.PositiveIdsAllowed);
        }

        [Test]
        public void NumberOfOperationsTest()
        {
            long number = delta.InsertOperations.Count() + delta.UpdateOperations.Count() + delta.DeleteOperations.Count();

            Assert.AreEqual(number, delta.NumberOfOperations);
        }

        [Test]
        [TestCase(DeltaOpType.Insert, true)]
        [TestCase(DeltaOpType.Update, true)]
        [TestCase(DeltaOpType.Delete, true)]
        [TestCase(DeltaOpType.Insert, false)]
        public void AddDeltaOperationTest(DeltaOpType type, bool addAtEnd)
        {
            ResourceDescription rd = new ResourceDescription(42949682361);

            Assert.DoesNotThrow(() => delta.AddDeltaOperation(type, rd, addAtEnd));
        }

        //[Test]
        //public void FixNegativeToPositiveIdsTest()
        //{
        //    Dictionary<long, long> globalIdPairs = null;

        //    Dictionary<short, int> typesCounters = new Dictionary<short, int>();
        //    foreach (DMSType type in Enum.GetValues(typeof(DMSType)))
        //    {
        //        typesCounters[(short)type] = 0;

        //        int i = 2;
        //        if (i == 5)
        //        {
        //            i = 2;
        //        }
        //        typesCounters[(short)type] = i;
        //        i++;
        //    }

        //    Assert.DoesNotThrow(() => delta.FixNegativeToPositiveIds(ref typesCounters, ref globalIdPairs));
        //}

        [Test]
        public void SortOperationsTest()
        {
            Assert.Throws<ModelException>(() => delta.SortOperations());
        }

        [Test]
        public void SerializeTest()
        {
            resultSerialize = delta.Serialize();
            int deltaLength = resultSerialize.Length;

            Assert.NotZero(deltaLength);
            //.AreEqual(deltaLength, result.Length);
        }

        [Test]
        public void DeserializeTest()
        {
            Delta result = Delta.Deserialize(resultSerialize);
            Assert.IsNotNull(result);

            resultSerialize = null;
            result = Delta.Deserialize(resultSerialize);
            Assert.IsNotNull(result);

            resultSerialize = delta.Serialize();
            result = Delta.Deserialize(resultSerialize);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ExportToXmlTest()
        {
            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.Formatting = Formatting.Indented;

            Assert.DoesNotThrow(() => delta.ExportToXml(xmlWriter));

            xmlWriter.Flush();
            xmlWriter.Close();
            stringWriter.Close();
        }

        [Test]
        public void TestClearDeltaOperations()
        {
            Assert.DoesNotThrow(() => delta.ClearDeltaOperations());
        }

        [Test]
        public void ChangeEntityIdInGlobalIdTest()
        {
            long oldId = 42949682361;
            long result = delta.ChangeEntityIdInGlobalId(oldId, 4294756);

            Assert.AreNotEqual(result, oldId);
        }
    }
}
