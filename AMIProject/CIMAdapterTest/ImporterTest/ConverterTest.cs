using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using FTN.ESI.SIMES.CIM.CIMAdapter.Importer;
using NSubstitute;
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
        private ImportHelper impHelp = null;
        private TransformAndLoadReport report = null;

        [OneTimeSetUp]
        public void SetupTest()
        {
            resDesc = new ResourceDescription();
            impHelp = new ImportHelper();
            report = new TransformAndLoadReport();
        }

        private void Init()
        {
            resDesc = new ResourceDescription();
            impHelp = new ImportHelper();
            report = new TransformAndLoadReport();
        }

        [Test]
        public void PopulateIdentifiedObjectPropertiesTest()
        {
            Init();
            Converter.PopulateIdentifiedObjectProperties(null, resDesc);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateIdentifiedObjectProperties(new AMIProfile.IdentifiedObject(), resDesc);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateIdentifiedObjectProperties(new AMIProfile.IdentifiedObject() { MRID = "142", Name = "Name" }, resDesc);
            Assert.AreEqual(resDesc.Properties.Count, 2);
            Assert.AreEqual(resDesc.Properties[0].PropertyValue.StringValue, "142");
            Assert.AreEqual(resDesc.Properties[1].PropertyValue.StringValue, "Name");
        }

        [Test]
        public void PopulateConductingEquipmentPropertiesTest()
        {
            Init();
            Converter.PopulateConductingEquipmentProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateConductingEquipmentProperties(new AMIProfile.ConductingEquipment(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.BaseVoltage bv = new AMIProfile.BaseVoltage() { ID = "BV_1" };
            AMIProfile.ConductingEquipment condEq = new AMIProfile.ConductingEquipment() { ID = "ce_1", MRID = "ce_1" };
            condEq.BaseVoltage = bv;
            //ImportHelper i = Substitute.For<ImportHelper>();
            impHelp.DefineIDMapping(condEq.ID, 1234567);
            impHelp.DefineIDMapping(condEq.BaseVoltage.ID, 12345678);
            Converter.PopulateConductingEquipmentProperties(condEq, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);
            Assert.AreEqual(resDesc.Properties[0].PropertyValue.StringValue, "ce_1");

            Converter.PopulateConductingEquipmentProperties(condEq, resDesc, new ImportHelper(), report); // gid < 0
            //Assert.AreEqual(resDesc.Properties[1].PropertyValue.StringValue, "BV_1");
        }

        [Test]
        public void PopulateEquipmentPropertiesTest()
        {
            Init();
            Converter.PopulateEquipmentProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateEquipmentProperties(new AMIProfile.Equipment(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.EquipmentContainer ec = new AMIProfile.EquipmentContainer() { ID = "EC_1" };
            AMIProfile.Equipment eq = new AMIProfile.Equipment() { ID = "ce_1", MRID = "ce_1" };
            eq.EquipmentContainer = ec;
            impHelp.DefineIDMapping(eq.ID, 1234567);
            impHelp.DefineIDMapping(eq.EquipmentContainer.ID, 12345678);
            Converter.PopulateEquipmentProperties(eq, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);
            Assert.AreEqual(resDesc.Properties[0].PropertyValue.StringValue, "ce_1");

            Converter.PopulateEquipmentProperties(eq, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulatePowerSystemResourcePropertiesTest()
        {
            Init();
            Converter.PopulatePowerSystemResourceProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulatePowerSystemResourceProperties(new AMIProfile.Equipment(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.PowerSystemResource psr = new AMIProfile.PowerSystemResource() { ID = "EC_1", Name = "PSR_1" };
            Converter.PopulatePowerSystemResourceProperties(psr, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 1);
        }

        [Test]
        public void PopulateEnergyConsumerPropertiesTest()
        {
            Init();
            Converter.PopulateEnergyConsumerProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateEnergyConsumerProperties(new AMIProfile.EnergyConsumer(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.EnergyConsumer ec = new AMIProfile.EnergyConsumer() { ID = "EC_1", Pfixed = 15, Qfixed = 15 };
            Converter.PopulateEnergyConsumerProperties(ec, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);
        }

        [Test]
        public void PopulatePowerTransformerPropertiesTest()
        {
            Init();
            Converter.PopulatePowerTransformerProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulatePowerTransformerProperties(new AMIProfile.PowerTransformer() { Name = "PT_1" }, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 1);
        }

        [Test]
        public void PopulateMeasurementPropertiesTest()
        {
            Init();
            Converter.PopulateMeasurementProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateMeasurementProperties(new AMIProfile.Measurement(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.Measurement m = new AMIProfile.Measurement() { ID = "M_1" };
            AMIProfile.PowerSystemResource psr = new AMIProfile.PowerSystemResource() { ID = "psr_1", MRID = "psr_1" };
            m.PowerSystemResource = psr;
            impHelp.DefineIDMapping(m.ID, 1234567);
            impHelp.DefineIDMapping(m.PowerSystemResource.ID, 12345678);
            Converter.PopulateMeasurementProperties(m, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 1);

            Converter.PopulateMeasurementProperties(m, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateDiscretePropertiesTest()
        {
            Init();
            Converter.PopulateDiscreteProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateDiscreteProperties(new AMIProfile.Discrete(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.Discrete d = new AMIProfile.Discrete() { ID = "M_1", MaxValue = 15, MinValue = -15, NormalValue = 0 };
            impHelp.DefineIDMapping(d.ID, 1234567);
            Converter.PopulateDiscreteProperties(d, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 3);

            Converter.PopulateDiscreteProperties(d, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateAnalogPropertiesTest()
        {
            Init();
            Converter.PopulateAnalogProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateAnalogProperties(new AMIProfile.Analog(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.Analog a = new AMIProfile.Analog() { ID = "M_1", MaxValue = 15, MinValue = -15, NormalValue = 0 };
            impHelp.DefineIDMapping(a.ID, 1234567);
            Converter.PopulateAnalogProperties(a, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 3);

            Converter.PopulateAnalogProperties(a, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateBaseVoltagePropertiesTest()
        {
            Init();
            Converter.PopulateBaseVoltageProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateBaseVoltageProperties(new AMIProfile.BaseVoltage(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.BaseVoltage bv = new AMIProfile.BaseVoltage() { ID = "M_1", NominalVoltage = 100};
            impHelp.DefineIDMapping(bv.ID, 1234567);
            Converter.PopulateBaseVoltageProperties(bv, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 1);

            Converter.PopulateBaseVoltageProperties(bv, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateTransformerEndPropertiesTest()
        {
            Init();
            Converter.PopulateTransformerEndProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateTransformerEndProperties(new AMIProfile.TransformerEnd(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.TransformerEnd te = new AMIProfile.TransformerEnd() { ID = "TE_1", MRID = "bv_1" };
            AMIProfile.BaseVoltage bv = new AMIProfile.BaseVoltage() { ID = "bv_1" };
            te.BaseVoltage = bv;
            impHelp.DefineIDMapping(te.ID, 1234567);
            impHelp.DefineIDMapping(te.BaseVoltage.ID, 12345678);
            Converter.PopulateTransformerEndProperties(te, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);

            Converter.PopulateTransformerEndProperties(te, resDesc, new ImportHelper(), report); // gid < 0
        }
    }
}
