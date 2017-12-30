namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
    using FTN.Common;
    using AMIProfile;

    /// <summary>
    /// PowerTransformerConverter has methods for populating
    /// ResourceDescription objects using PowerTransformerCIMProfile_Labs objects.
    /// </summary>
    public static class Converter
    {
        #region Populate ResourceDescription
        public static void PopulateIdentifiedObjectProperties(IdentifiedObject cimIdentifiedObject, ResourceDescription rd)
        {
            if ((cimIdentifiedObject != null) && (rd != null))
            {
                if (cimIdentifiedObject.MRIDHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_MRID, cimIdentifiedObject.MRID));
                }

                if (cimIdentifiedObject.NameHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.IDOBJ_NAME, cimIdentifiedObject.Name));
                }
            }
        }

        public static void PopulateConductingEquipmentProperties(ConductingEquipment cimConductingEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimConductingEquipment != null) && (rd != null))
            {
                Converter.PopulateEquipmentProperties(cimConductingEquipment, rd, importHelper, report);

                if (cimConductingEquipment.BaseVoltageHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimConductingEquipment.BaseVoltage.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimConductingEquipment.GetType().ToString()).Append(" rdfID = \"").Append(cimConductingEquipment.ID);
                        report.Report.Append("\" - Failed to set reference to BaseVoltage: rdfID \"").Append(cimConductingEquipment.BaseVoltage.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.CONDEQ_BASEVOLTAGE, gid));
                }
            }
        }

        public static void PopulateEquipmentProperties(Equipment cimEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEquipment != null) && (rd != null))
            {
                Converter.PopulatePowerSystemResourceProperties(cimEquipment, rd, importHelper, report);

                if (cimEquipment.EquipmentContainerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimEquipment.EquipmentContainer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimEquipment.GetType().ToString()).Append(" rdfID = \"").Append(cimEquipment.ID);
                        report.Report.Append("\" - Failed to set reference to EquipmentContainer: rdfID \"").Append(cimEquipment.EquipmentContainer.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.EQUIPMENT_EQCONTAINER, gid));
                }
            }
        }

        public static void PopulatePowerSystemResourceProperties(PowerSystemResource cimPowerSystemResource, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimPowerSystemResource != null) && (rd != null))
            {
                Converter.PopulateIdentifiedObjectProperties(cimPowerSystemResource, rd);
            }
        }

        public static void PopulateEnergyConsumerProperties(EnergyConsumer cimEnergyConsumer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEnergyConsumer != null) && (rd != null))
            {
                Converter.PopulateConductingEquipmentProperties(cimEnergyConsumer, rd, importHelper, report);

                if (cimEnergyConsumer.PfixedHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONS_PFIXED, cimEnergyConsumer.Pfixed));
                }

                if (cimEnergyConsumer.QfixedHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONS_QFIXED, cimEnergyConsumer.Qfixed));
                }
            }
        }

        public static void PopulatePowerTransformerProperties(PowerTransformer cimPowerTrans, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimPowerTrans != null) && (rd != null))
            {
                Converter.PopulateConductingEquipmentProperties(cimPowerTrans, rd, importHelper, report);
            }
        }

        public static void PopulateMeasurementProperties(Measurement cimMeas, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimMeas != null) && (rd != null))
            {
                Converter.PopulateIdentifiedObjectProperties(cimMeas, rd);

                if (cimMeas.UnitSymbolHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_UNITSYMBOL, (int)cimMeas.UnitSymbol));
                }
                if (cimMeas.SignalDirectionHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_DIRECTION, (int)cimMeas.SignalDirection));
                }
                if (cimMeas.RtuAddressHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_RTUADDRESS, (int)cimMeas.RtuAddress));
                }
                if (cimMeas.MinRawValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_MINRAWVAL, (int)cimMeas.MinRawValue));
                }
                if (cimMeas.MaxRawValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_MAXRAWVAL, (int)cimMeas.MaxRawValue));
                }
                if (cimMeas.NormalRawValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_NORMALRAWVAL, (int)cimMeas.NormalRawValue));
                }

                if (cimMeas.PowerSystemResourceHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimMeas.PowerSystemResource.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimMeas.GetType().ToString()).Append(" rdfID = \"").Append(cimMeas.ID);
                        report.Report.Append("\" - Failed to set reference to PowerSystemResource: rdfID \"").Append(cimMeas.PowerSystemResource.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.MEASUREMENT_PSR, gid));
                }
            }
        }

        public static void PopulateDiscreteProperties(Discrete cimDiscrete, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimDiscrete != null) && (rd != null))
            {
                Converter.PopulateMeasurementProperties(cimDiscrete, rd, importHelper, report);

                if (cimDiscrete.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MAXVALUE, cimDiscrete.MaxValue));
                }

                if (cimDiscrete.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_MINVALUE, cimDiscrete.MinValue));
                }

                if (cimDiscrete.NormalValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.DISCRETE_NORMALVALUE, cimDiscrete.NormalValue));
                }
            }
        }

        public static void PopulateAnalogProperties(Analog cimAnalog, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimAnalog != null) && (rd != null))
            {
                Converter.PopulateMeasurementProperties(cimAnalog, rd, importHelper, report);

                if (cimAnalog.MaxValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MAXVALUE, cimAnalog.MaxValue));
                }

                if (cimAnalog.MinValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_MINVALUE, cimAnalog.MinValue));
                }

                if (cimAnalog.NormalValueHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_NORMALVALUE, cimAnalog.NormalValue));
                }

                if (cimAnalog.AlarmHighHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_ALARMHIGH, cimAnalog.AlarmHigh));
                }
                if (cimAnalog.AlarmLowHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ANALOG_ALARMLOW, cimAnalog.AlarmLow));
                }
            }
        }

        public static void PopulateBaseVoltageProperties(BaseVoltage cimBaseVol, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimBaseVol != null) && (rd != null))
            {
                Converter.PopulateIdentifiedObjectProperties(cimBaseVol, rd);

                if (cimBaseVol.NominalVoltageHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.BASEVOLTAGE_NOMINALVOL, cimBaseVol.NominalVoltage));
                }
            }
        }

        public static void PopulateTransformerEndProperties(TransformerEnd cimTransEnd, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimTransEnd != null) && (rd != null))
            {
                Converter.PopulateIdentifiedObjectProperties(cimTransEnd, rd);

                if (cimTransEnd.BaseVoltageHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimTransEnd.BaseVoltage.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimTransEnd.GetType().ToString()).Append(" rdfID = \"").Append(cimTransEnd.ID);
                        report.Report.Append("\" - Failed to set reference to BaseVoltage: rdfID \"").Append(cimTransEnd.BaseVoltage.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.TRANSFORMEREND_BASEVOLT, gid));
                }
            }
        }

        public static void PopulateConnectivityNodeContainerProperties(ConnectivityNodeContainer cimCNC, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimCNC != null) && (rd != null))
            {
                Converter.PopulatePowerSystemResourceProperties(cimCNC, rd, importHelper, report);
            }
        }

        public static void PopulateGeographicalRegionProperties(GeographicalRegion cimGeoRegion, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimGeoRegion != null) && (rd != null))
            {
                Converter.PopulateIdentifiedObjectProperties(cimGeoRegion, rd);
            }
        }

        public static void PopulateSubGeographicalRegionProperties(SubGeographicalRegion cimSubRegion, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimSubRegion != null) && (rd != null))
            {
                Converter.PopulateIdentifiedObjectProperties(cimSubRegion, rd);

                if (cimSubRegion.RegionHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimSubRegion.Region.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimSubRegion.GetType().ToString()).Append(" rdfID = \"").Append(cimSubRegion.ID);
                        report.Report.Append("\" - Failed to set reference to Region: rdfID \"").Append(cimSubRegion.Region.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.SUBGEOREGION_GEOREG, gid));
                }
            }
        }

        public static void PopulateEquipmentContainerProperties(EquipmentContainer cimEqContainer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimEqContainer != null) && (rd != null))
            {
                Converter.PopulateConnectivityNodeContainerProperties(cimEqContainer, rd, importHelper, report);
            }
        }

        public static void PopulateSubstationProperties(Substation cimSubstation, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimSubstation != null) && (rd != null))
            {
                Converter.PopulateEquipmentContainerProperties(cimSubstation, rd, importHelper, report);

                if (cimSubstation.RegionHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimSubstation.Region.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimSubstation.GetType().ToString()).Append(" rdfID = \"").Append(cimSubstation.ID);
                        report.Report.Append("\" - Failed to set reference to Region: rdfID \"").Append(cimSubstation.Region.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.SUBSTATION_SUBGEOREGION, gid));
                }
            }
        }

        public static void PopulateTapChangerProperties(TapChanger cimTapChanger, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimTapChanger != null) && (rd != null))
            {
                Converter.PopulatePowerSystemResourceProperties(cimTapChanger, rd, importHelper, report);

                if (cimTapChanger.HighStepHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.TAPCHANGER_HIGHSTEP, cimTapChanger.HighStep));
                }

                if (cimTapChanger.LowStepHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.TAPCHANGER_LOWSTEP, cimTapChanger.LowStep));
                }

                if (cimTapChanger.NeutralStepHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.TAPCHANGER_NEUTRALSTEP, cimTapChanger.NeutralStep));
                }

                if (cimTapChanger.NormalStepHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.TAPCHANGER_NORMALSTEP, cimTapChanger.NormalStep));
                }
            }
        }

        public static void PopulateRatioTapChangerProperties(RatioTapChanger cimRation, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimRation != null) && (rd != null))
            {
                Converter.PopulateTapChangerProperties(cimRation, rd, importHelper, report);

                if (cimRation.TransformerEndHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimRation.TransformerEnd.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimRation.GetType().ToString()).Append(" rdfID = \"").Append(cimRation.ID);
                        report.Report.Append("\" - Failed to set reference to TransformerEnd: rdfID \"").Append(cimRation.TransformerEnd.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.RATIOTAPCHANGER_TRANSEND, gid));
                }
            }
        }

        public static void PopulateVoltageLevelProperties(VoltageLevel cimVolLevel, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimVolLevel != null) && (rd != null))
            {
                Converter.PopulateEquipmentContainerProperties(cimVolLevel, rd, importHelper, report);

                if (cimVolLevel.BaseVoltageHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimVolLevel.BaseVoltage.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimVolLevel.GetType().ToString()).Append(" rdfID = \"").Append(cimVolLevel.ID);
                        report.Report.Append("\" - Failed to set reference to BaseVoltage: rdfID \"").Append(cimVolLevel.BaseVoltage.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.VOLTAGELEVEL_BASEVOLTAGE, gid));
                }

                if (cimVolLevel.SubstationHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimVolLevel.Substation.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimVolLevel.GetType().ToString()).Append(" rdfID = \"").Append(cimVolLevel.ID);
                        report.Report.Append("\" - Failed to set reference to Substation: rdfID \"").Append(cimVolLevel.Substation.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.VOLTAGELEVEL_SUBSTATION, gid));
                }
            }
        }

        public static void PopulatePowerTransformerEndProperties(PowerTransformerEnd cimPowerTransEnd, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimPowerTransEnd != null) && (rd != null))
            {
                Converter.PopulateTransformerEndProperties(cimPowerTransEnd, rd, importHelper, report);

                if (cimPowerTransEnd.PowerTransformerHasValue)
                {
                    long gid = importHelper.GetMappedGID(cimPowerTransEnd.PowerTransformer.ID);
                    if (gid < 0)
                    {
                        report.Report.Append("WARNING: Convert ").Append(cimPowerTransEnd.GetType().ToString()).Append(" rdfID = \"").Append(cimPowerTransEnd.ID);
                        report.Report.Append("\" - Failed to set reference to PowerTransformer: rdfID \"").Append(cimPowerTransEnd.PowerTransformer.ID).AppendLine(" \" is not mapped to GID!");
                    }

                    rd.AddProperty(new Property(ModelCode.POWERTRANSEND_POWERTRANSF, gid));
                }
            }
        }

        #endregion Populate ResourceDescription

        #region Enums convert

        public static FTN.Common.UnitSymbol GetDMSUnitSymbol(AMIProfile.UnitSymbol unit)
        {
            switch (unit)
            {
                case AMIProfile.UnitSymbol.P:
                    return FTN.Common.UnitSymbol.P;
                case AMIProfile.UnitSymbol.Q:
                    return FTN.Common.UnitSymbol.Q;
                case AMIProfile.UnitSymbol.V:
                    return FTN.Common.UnitSymbol.V;

                default: return FTN.Common.UnitSymbol.V;
            }
        }

        public static FTN.Common.Direction GetDMSDirection(AMIProfile.Direction unit)
        {
            switch (unit)
            {
                case AMIProfile.Direction.WRITE:
                    return FTN.Common.Direction.WRITE;
                case AMIProfile.Direction.READWRITE:
                    return FTN.Common.Direction.READWRITE;
                case AMIProfile.Direction.READ:
                    return FTN.Common.Direction.READ;

                default: return FTN.Common.Direction.READ;
            }
        }

        #endregion Enums convert
    }
}
