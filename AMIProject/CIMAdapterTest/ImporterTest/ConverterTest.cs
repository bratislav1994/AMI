using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Importer;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;

namespace CIMAdapterTest.ImporterTest
{
    [TestFixture]
    public class ConverterTest
    {
        private ResourceDescription resDesc = null;

        [OneTimeSetUp]
        public void SetupTest()
        {
            resDesc = new ResourceDescription();
        }

        [Test]
        public void PopulateIdentifiedObjectPropertiesTest()
        {
            Converter.PopulateIdentifiedObjectProperties(null, resDesc);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateIdentifiedObjectProperties(new AMIProfile.IdentifiedObject(), resDesc);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateIdentifiedObjectProperties(new AMIProfile.IdentifiedObject() { MRID = "142", Name = "Name" }, resDesc);
            Assert.AreEqual(resDesc.Properties.Count, 2);
            Assert.AreEqual(resDesc.Properties[0].PropertyValue.StringValue, "142");
            Assert.AreEqual(resDesc.Properties[1].PropertyValue.StringValue, "Name");
        }
    }
}
