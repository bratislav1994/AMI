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
    public class CompareHelperTest
    {
        [Test]
        public void CompareListsLongTest()
        {
            bool result;

            List<long> xList1 = null;
            List<long> yList1 = null;
            result = CompareHelper.CompareLists(xList1, yList1);
            Assert.AreEqual(true, result);

            List<long> yList2 = new List<long>() { 42949672811, 42949672812 };
            result = CompareHelper.CompareLists(xList1, yList2);
            Assert.AreNotEqual(true, result);

            List<long> xList2 = new List<long>() { 42949672811, 42949672812 };
            result = CompareHelper.CompareLists(xList2, yList1);
            Assert.AreNotEqual(true, result);

            List<long> xList3 = new List<long>() { 42949672811, 42949672855 };
            result = CompareHelper.CompareLists(xList3, yList2);
            Assert.AreNotEqual(true, result);

            List<long> xList4 = new List<long>() { 42949672811 };
            result = CompareHelper.CompareLists(xList4, yList2);
            Assert.AreNotEqual(true, result);

            result = CompareHelper.CompareLists(xList2, yList2);
            Assert.AreEqual(true, result);

            result = CompareHelper.CompareLists(xList3, yList2, false);
            Assert.AreNotEqual(true, result);
        }

        [Test]
        public void CompareListsIntTest()
        {
            bool result;

            List<int> xList1 = null;
            List<int> yList1 = null;
            result = CompareHelper.CompareLists(xList1, yList1);
            Assert.AreEqual(true, result);

            List<int> yList2 = new List<int>() { 42949, 42949 };
            result = CompareHelper.CompareLists(xList1, yList2);
            Assert.AreNotEqual(true, result);

            List<int> xList2 = new List<int>() { 42949, 42949 };
            result = CompareHelper.CompareLists(xList2, yList1);
            Assert.AreNotEqual(true, result);

            List<int> xList3 = new List<int>() { 42949, 42955 };
            result = CompareHelper.CompareLists(xList3, yList2);
            Assert.AreNotEqual(true, result);

            List<int> xList4 = new List<int>() { 42949 };
            result = CompareHelper.CompareLists(xList4, yList2);
            Assert.AreNotEqual(true, result);

            result = CompareHelper.CompareLists(xList2, yList2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CompareListsBoolTest()
        {
            bool result;

            List<bool> xList1 = null;
            List<bool> yList1 = null;
            result = CompareHelper.CompareLists(xList1, yList1);
            Assert.AreEqual(true, result);

            List<bool> yList2 = new List<bool>() { true, false };
            result = CompareHelper.CompareLists(xList1, yList2);
            Assert.AreNotEqual(true, result);

            List<bool> xList2 = new List<bool>() { true, false };
            result = CompareHelper.CompareLists(xList2, yList1);
            Assert.AreNotEqual(true, result);

            List<bool> xList3 = new List<bool>() { true, true };
            result = CompareHelper.CompareLists(xList3, yList2);
            Assert.AreNotEqual(true, result);

            List<bool> xList4 = new List<bool>() { true };
            result = CompareHelper.CompareLists(xList4, yList2);
            Assert.AreNotEqual(true, result);

            result = CompareHelper.CompareLists(xList2, yList2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CompareListsShortTest()
        {
            bool result;

            List<short> xList1 = null;
            List<short> yList1 = null;
            result = CompareHelper.CompareLists(xList1, yList1);
            Assert.AreEqual(true, result);

            List<short> yList2 = new List<short>() { 42, 49 };
            result = CompareHelper.CompareLists(xList1, yList2);
            Assert.AreNotEqual(true, result);

            List<short> xList2 = new List<short>() { 42, 49 };
            result = CompareHelper.CompareLists(xList2, yList1);
            Assert.AreNotEqual(true, result);

            List<short> xList3 = new List<short>() { 42, 55 };
            result = CompareHelper.CompareLists(xList3, yList2);
            Assert.AreNotEqual(true, result);

            List<short> xList4 = new List<short>() { 42 };
            result = CompareHelper.CompareLists(xList4, yList2);
            Assert.AreNotEqual(true, result);

            result = CompareHelper.CompareLists(xList2, yList2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CompareListsDMSTypeTest()
        {
            bool result;

            List<DMSType> xList1 = null;
            List<DMSType> yList1 = null;
            result = CompareHelper.CompareLists(xList1, yList1);
            Assert.AreEqual(true, result);

            List<DMSType> yList2 = new List<DMSType>() { DMSType.ANALOG, DMSType.DISCRETE };
            result = CompareHelper.CompareLists(xList1, yList2);
            Assert.AreNotEqual(true, result);

            List<DMSType> xList2 = new List<DMSType>() { DMSType.ANALOG, DMSType.DISCRETE };
            result = CompareHelper.CompareLists(xList2, yList1);
            Assert.AreNotEqual(true, result);

            List<DMSType> xList3 = new List<DMSType>() { DMSType.ANALOG, DMSType.GEOREGION };
            result = CompareHelper.CompareLists(xList3, yList2);
            Assert.AreNotEqual(true, result);

            List<DMSType> xList4 = new List<DMSType>() { DMSType.ANALOG };
            result = CompareHelper.CompareLists(xList4, yList2);
            Assert.AreNotEqual(true, result);

            result = CompareHelper.CompareLists(xList2, yList2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CompareListsFloatTest()
        {
            bool result;

            List<float> xList1 = null;
            List<float> yList1 = null;
            result = CompareHelper.CompareLists(xList1, yList1);
            Assert.AreEqual(true, result);

            List<float> yList2 = new List<float>() { 42949, 42949 };
            result = CompareHelper.CompareLists(xList1, yList2);
            Assert.AreNotEqual(true, result);

            List<float> xList2 = new List<float>() { 42949, 42949 };
            result = CompareHelper.CompareLists(xList2, yList1);
            Assert.AreNotEqual(true, result);

            List<float> xList3 = new List<float>() { 42949, 42955 };
            result = CompareHelper.CompareLists(xList3, yList2);
            Assert.AreNotEqual(true, result);

            List<float> xList4 = new List<float>() { 42949 };
            result = CompareHelper.CompareLists(xList4, yList2);
            Assert.AreNotEqual(true, result);

            result = CompareHelper.CompareLists(xList2, yList2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CompareListsStringTest()
        {
            bool result;

            List<string> xList1 = null;
            List<string> yList1 = null;
            result = CompareHelper.CompareLists(xList1, yList1);
            Assert.AreEqual(true, result);

            List<string> yList2 = new List<string>() { "analog", "discrete" };
            result = CompareHelper.CompareLists(xList1, yList2);
            Assert.AreNotEqual(true, result);

            List<string> xList2 = new List<string>() { "analog", "discrete" };
            result = CompareHelper.CompareLists(xList2, yList1);
            Assert.AreNotEqual(true, result);

            List<string> xList3 = new List<string>() { "analog", "georegion" };
            result = CompareHelper.CompareLists(xList3, yList2);
            Assert.AreNotEqual(true, result);

            List<string> xList4 = new List<string>() { "analog" };
            result = CompareHelper.CompareLists(xList4, yList2);
            Assert.AreNotEqual(true, result);

            result = CompareHelper.CompareLists(xList2, yList2);
            Assert.AreEqual(true, result);
        }
    }
}
