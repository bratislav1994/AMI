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

                if (cimEnergyConsumer.PMaxHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONS_PMAX, cimEnergyConsumer.PMax));
                }

                if (cimEnergyConsumer.QMaxHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONS_QMAX, cimEnergyConsumer.QMax));
                }

                if (cimEnergyConsumer.ConsumerTypeHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONS_TYPE, (int)cimEnergyConsumer.ConsumerType));
                }

                if (cimEnergyConsumer.ValidRangePercentHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONS_VALIDRANGEPERCENT, cimEnergyConsumer.ValidRangePercent));
                }

                if (cimEnergyConsumer.InvalidRangePercentHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.ENERGYCONS_INVALIDRANGEPERCENT, cimEnergyConsumer.InvalidRangePercent));
                }
            }
        }

        public static void PopulatePowerTransformerProperties(PowerTransformer cimPowerTrans, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
        {
            if ((cimPowerTrans != null) && (rd != null))
            {
                Converter.PopulateConductingEquipmentProperties(cimPowerTrans, rd, importHelper, report);

                if (cimPowerTrans.ValidRangePercentHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.POWERTRANSFORMER_VALIDRANGEPERCENT, cimPowerTrans.ValidRangePercent));
                }

                if (cimPowerTrans.InvalidRangePercentHasValue)
                {
                    rd.AddProperty(new Property(ModelCode.POWERTRANSFORMER_INVALIDRANGEPERCENT, cimPowerTrans.InvalidRangePercent));
                }
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
        
        #endregion Populate ResourceDescription

        #region Enums convert

        public static FTN.Common.ConsumerType GetDMSConsumerType(AMIProfile.ConsumerType type)
        {
            switch (type)
            {
                case AMIProfile.ConsumerType.HOUSEHOLD:
                    return FTN.Common.ConsumerType.HOUSEHOLD;
                case AMIProfile.ConsumerType.SHOPPING_CENTER:
                    return FTN.Common.ConsumerType.SHOPPING_CENTER;
                case AMIProfile.ConsumerType.FIRM:
                    return FTN.Common.ConsumerType.FIRM;

                default: return FTN.Common.ConsumerType.HOUSEHOLD;
            }
        }

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
