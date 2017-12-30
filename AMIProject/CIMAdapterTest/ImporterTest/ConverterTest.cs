using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter;
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

            AMIProfile.Measurement m = new AMIProfile.Measurement() { ID = "M_1", UnitSymbol = AMIProfile.UnitSymbol.P, RtuAddress = 10, SignalDirection = AMIProfile.Direction.READ };
            AMIProfile.PowerSystemResource psr = new AMIProfile.PowerSystemResource() { ID = "psr_1", MRID = "psr_1" };
            m.PowerSystemResource = psr;
            impHelp.DefineIDMapping(m.ID, 1234567);
            impHelp.DefineIDMapping(m.PowerSystemResource.ID, 12345678);
            Converter.PopulateMeasurementProperties(m, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 4);

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

            AMIProfile.Analog a = new AMIProfile.Analog() { ID = "M_1", MaxValue = 15, MinValue = -15, NormalValue = 0, AlarmLow = -5, AlarmHigh = 10 };
            impHelp.DefineIDMapping(a.ID, 1234567);
            Converter.PopulateAnalogProperties(a, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 5);

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

        [Test]
        public void PopulateConnectivityNodeContainerPropertiesTest()
        {
            Init();
            Converter.PopulateConnectivityNodeContainerProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateConnectivityNodeContainerProperties(new AMIProfile.ConnectivityNodeContainer(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.ConnectivityNodeContainer cn = new AMIProfile.ConnectivityNodeContainer() { ID = "CN_1", MRID = "CN_1", Name = "CN" };
            impHelp.DefineIDMapping(cn.ID, 1234567);
            Converter.PopulateConnectivityNodeContainerProperties(cn, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);
        }

        [Test]
        public void PopulateGeographicalRegionPropertiesTest()
        {
            Init();
            Converter.PopulateGeographicalRegionProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateGeographicalRegionProperties(new AMIProfile.GeographicalRegion(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.GeographicalRegion geo = new AMIProfile.GeographicalRegion() { ID = "geo_1", MRID = "geo_1", Name = "Vojvodina" };
            impHelp.DefineIDMapping(geo.ID, 1234567);
            Converter.PopulateGeographicalRegionProperties(geo, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);
        }

        [Test]
        public void PopulateSubGeographicalRegionPropertiesTest()
        {
            Init();
            Converter.PopulateSubGeographicalRegionProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateSubGeographicalRegionProperties(new AMIProfile.SubGeographicalRegion(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.SubGeographicalRegion sgr = new AMIProfile.SubGeographicalRegion() { ID = "sub_geo1", MRID = "sub_geo1" };
            AMIProfile.GeographicalRegion gr = new AMIProfile.GeographicalRegion() { ID = "geo1" };
            sgr.Region = gr;
            impHelp.DefineIDMapping(sgr.ID, 1234567);
            impHelp.DefineIDMapping(sgr.Region.ID, 12345678);
            Converter.PopulateSubGeographicalRegionProperties(sgr, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);

            Converter.PopulateSubGeographicalRegionProperties(sgr, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateEquipmentContainerPropertiesTest()
        {
            Init();
            Converter.PopulateEquipmentContainerProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateEquipmentContainerProperties(new AMIProfile.EquipmentContainer(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.EquipmentContainer eqc = new AMIProfile.EquipmentContainer() { ID = "eqC1", MRID = "eqC1" };
            impHelp.DefineIDMapping(eqc.ID, 1234567);
            Converter.PopulateEquipmentContainerProperties(eqc, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 1);
        }

        [Test]
        public void PopulateSubstationPropertiesTest()
        {
            Init();
            Converter.PopulateSubstationProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateSubstationProperties(new AMIProfile.Substation(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.Substation sub = new AMIProfile.Substation() { ID = "sub1", MRID = "sub1", Name = "Trafo 1" };
            AMIProfile.SubGeographicalRegion gr = new AMIProfile.SubGeographicalRegion() { ID = "geo1" };
            sub.Region = gr;
            impHelp.DefineIDMapping(sub.ID, 1234567);
            impHelp.DefineIDMapping(sub.Region.ID, 12345678);
            Converter.PopulateSubstationProperties(sub, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 3);

            Converter.PopulateSubstationProperties(sub, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateTapChangerPropertiesTest()
        {
            Init();
            Converter.PopulateTapChangerProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateTapChangerProperties(new AMIProfile.TapChanger(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.TapChanger tapC = new AMIProfile.TapChanger() { ID = "sub1", MRID = "sub1", HighStep = 4, LowStep = 1, NeutralStep = 0, NormalStep = 1 };
            impHelp.DefineIDMapping(tapC.ID, 1234567);
            Converter.PopulateTapChangerProperties(tapC, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 5);

            Converter.PopulateTapChangerProperties(tapC, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateRatioTapChangerPropertiesTest()
        {
            Init();
            Converter.PopulateRatioTapChangerProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateRatioTapChangerProperties(new AMIProfile.RatioTapChanger(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.RatioTapChanger ratio = new AMIProfile.RatioTapChanger() { ID = "ratio1", MRID = "ratio1"};
            AMIProfile.TransformerEnd trEnd = new AMIProfile.TransformerEnd() { ID = "trEnd_1" };
            ratio.TransformerEnd = trEnd;
            impHelp.DefineIDMapping(ratio.ID, 1234567);
            impHelp.DefineIDMapping(ratio.TransformerEnd.ID, 12345678);
            Converter.PopulateRatioTapChangerProperties(ratio, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);

            Converter.PopulateRatioTapChangerProperties(ratio, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulateVoltageLevelPropertiesTest()
        {
            Init();
            Converter.PopulateVoltageLevelProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulateVoltageLevelProperties(new AMIProfile.VoltageLevel(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.VoltageLevel volLevel = new AMIProfile.VoltageLevel() { ID = "volLevel1", MRID = "volLevel1" };
            AMIProfile.BaseVoltage baseVol = new AMIProfile.BaseVoltage() { ID = "baseVol_1" };
            AMIProfile.Substation sub = new AMIProfile.Substation() { ID = "sub_1" };
            volLevel.BaseVoltage = baseVol;
            volLevel.Substation = sub;
            impHelp.DefineIDMapping(volLevel.ID, 1234567);
            impHelp.DefineIDMapping(volLevel.BaseVoltage.ID, 12345678);
            impHelp.DefineIDMapping(volLevel.Substation.ID, 34267);
            Converter.PopulateVoltageLevelProperties(volLevel, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 3);

            Converter.PopulateVoltageLevelProperties(volLevel, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        public void PopulatePowerTransformerEndPropertiesTest()
        {
            Init();
            Converter.PopulatePowerTransformerEndProperties(null, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            Converter.PopulatePowerTransformerEndProperties(new AMIProfile.PowerTransformerEnd(), resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 0);

            AMIProfile.PowerTransformerEnd powerEnd = new AMIProfile.PowerTransformerEnd() { ID = "powerTransEnd_1", MRID = "powerTransEnd_1" };
            AMIProfile.PowerTransformer powerTrans = new AMIProfile.PowerTransformer() { ID = "powerTrans_1" };
            powerEnd.PowerTransformer = powerTrans;
            impHelp.DefineIDMapping(powerEnd.ID, 1234567);
            impHelp.DefineIDMapping(powerEnd.PowerTransformer.ID, 12345678);
            Converter.PopulatePowerTransformerEndProperties(powerEnd, resDesc, impHelp, report);
            Assert.AreEqual(resDesc.Properties.Count, 2);

            Converter.PopulatePowerTransformerEndProperties(powerEnd, resDesc, new ImportHelper(), report); // gid < 0
        }

        [Test]
        [TestCase(AMIProfile.UnitSymbol.P)]
        [TestCase(AMIProfile.UnitSymbol.Q)]
        [TestCase(AMIProfile.UnitSymbol.V)]
        [TestCase(3)]
        public void GetDMSUnitSymbolTest(AMIProfile.UnitSymbol unit)
        {
            FTN.Common.UnitSymbol ret;
            switch (unit)
            {
                case AMIProfile.UnitSymbol.P:
                    ret = Converter.GetDMSUnitSymbol(unit);
                    Assert.AreEqual(FTN.Common.UnitSymbol.P, ret);
                    break;
                case AMIProfile.UnitSymbol.Q:
                    ret = Converter.GetDMSUnitSymbol(unit);
                    Assert.AreEqual(FTN.Common.UnitSymbol.Q, ret);
                    break;
                case AMIProfile.UnitSymbol.V:
                    ret = Converter.GetDMSUnitSymbol(unit);
                    Assert.AreEqual(FTN.Common.UnitSymbol.V, ret);
                    break;

                default:
                    ret = Converter.GetDMSUnitSymbol(unit);
                    Assert.AreEqual(FTN.Common.UnitSymbol.V, ret);
                    break;
            }
        }

        [Test]
        [TestCase(AMIProfile.Direction.WRITE)]
        [TestCase(AMIProfile.Direction.READWRITE)]
        [TestCase(AMIProfile.Direction.READ)]
        [TestCase(3)]
        public void GetDMSDirectionTest(AMIProfile.Direction direction)
        {
            FTN.Common.Direction ret;
            switch (direction)
            {
                case AMIProfile.Direction.WRITE:
                    ret = Converter.GetDMSDirection(direction);
                    Assert.AreEqual(FTN.Common.Direction.WRITE, ret);
                    break;
                case AMIProfile.Direction.READWRITE:
                    ret = Converter.GetDMSDirection(direction);
                    Assert.AreEqual(FTN.Common.Direction.READWRITE, ret);
                    break;
                case AMIProfile.Direction.READ:
                    ret = Converter.GetDMSDirection(direction);
                    Assert.AreEqual(FTN.Common.Direction.READ, ret);
                    break;

                default:
                    ret = Converter.GetDMSDirection(direction);
                    Assert.AreEqual(FTN.Common.Direction.READ, ret);
                    break;
            }
        }
    }
}
