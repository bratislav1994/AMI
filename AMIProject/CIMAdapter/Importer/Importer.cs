using System;
using System.Collections.Generic;
using CIM.Model;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using AMIProfile;

namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	/// <summary>
	/// PowerTransformerImporter
	/// </summary>
	public class Importer
	{
		/// <summary> Singleton </summary>
		private static Importer ptImporter = null;
		private static object singletoneLock = new object();

		private ConcreteModel concreteModel;
		private Delta delta;
		private ImportHelper importHelper;
		private TransformAndLoadReport report;


		#region Properties
		public static Importer Instance
		{
			get
			{
				if (ptImporter == null)
				{
					lock (singletoneLock)
					{
						if (ptImporter == null)
						{
							ptImporter = new Importer();
							ptImporter.Reset();
						}
					}
				}
				return ptImporter;
			}
		}

		public Delta NMSDelta
		{
			get 
			{
				return delta;
			}
		}
		#endregion Properties


		public void Reset()
		{
			concreteModel = null;
			delta = new Delta();
			importHelper = new ImportHelper();
			report = null;
		}

		public TransformAndLoadReport CreateNMSDelta(ConcreteModel cimConcreteModel)
		{
			LogManager.Log("Importing PowerTransformer Elements...", LogLevel.Info);
			report = new TransformAndLoadReport();
			concreteModel = cimConcreteModel;
			delta.ClearDeltaOperations();

			if ((concreteModel != null) && (concreteModel.ModelMap != null))
			{
				try
				{
					// convert into DMS elements
					ConvertModelAndPopulateDelta();
				}
				catch (Exception ex)
				{
					string message = string.Format("{0} - ERROR in data import - {1}", DateTime.Now, ex.Message);
					LogManager.Log(message);
					report.Report.AppendLine(ex.Message);
					report.Success = false;
				}
			}
			LogManager.Log("Importing PowerTransformer Elements - END.", LogLevel.Info);
			return report;
		}

		/// <summary>
		/// Method performs conversion of network elements from CIM based concrete model into DMS model.
		/// </summary>
		private void ConvertModelAndPopulateDelta()
		{
			LogManager.Log("Loading elements and creating delta...", LogLevel.Info);

            //// import all concrete model types (DMSType enum)
            ImportBaseVoltage();
            ImportGeographicalRegion();
            ImportSubGeographicalRegion();
            ImportSubstation();
            ImportVoltageLevel();
            ImportPowerTransformer();
            ImportPowerTransformerEnd();
            ImportRatioTapChanger();
            ImportEnergyConsumer();
            ImportAnalogMeasurement();
            ImportDiscreteMeasurement();

            LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
		}

        #region Import
        private void ImportBaseVoltage()
        {
            SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("AMIProfile.BaseVoltage");
            if (cimBaseVoltages != null)
            {
                foreach (KeyValuePair<string, object> cimBaseVoltagesPair in cimBaseVoltages)
                {
                    BaseVoltage cimBaseVoltage = cimBaseVoltagesPair.Value as BaseVoltage;

                    ResourceDescription rd = CreateBaseVoltageResourceDescription(cimBaseVoltage);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("BaseVoltage ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("BaseVoltage ID = ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateBaseVoltageResourceDescription(BaseVoltage cimBaseVoltage)
        {
            ResourceDescription rd = null;
            if (cimBaseVoltage != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.BASEVOLTAGE, importHelper.CheckOutIndexForDMSType(DMSType.BASEVOLTAGE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimBaseVoltage.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateBaseVoltageProperties(cimBaseVoltage, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportGeographicalRegion()
        {
            SortedDictionary<string, object> cimGeographicalRegions = concreteModel.GetAllObjectsOfType("AMIProfile.GeographicalRegion");
            if (cimGeographicalRegions != null)
            {
                foreach (KeyValuePair<string, object> cimGeographicalRegionPair in cimGeographicalRegions)
                {
                    GeographicalRegion cimGeographicalRegion = cimGeographicalRegionPair.Value as GeographicalRegion;

                    ResourceDescription rd = CreateGeographicalRegionResourceDescription(cimGeographicalRegion);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("GeographicalRegion ID = ").Append(cimGeographicalRegion.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("GeographicalRegion ID = ").Append(cimGeographicalRegion.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateGeographicalRegionResourceDescription(GeographicalRegion cimGeographicalRegion)
        {
            ResourceDescription rd = null;
            if (cimGeographicalRegion != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.GEOREGION, importHelper.CheckOutIndexForDMSType(DMSType.GEOREGION));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimGeographicalRegion.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateGeographicalRegionProperties(cimGeographicalRegion, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSubGeographicalRegion()
        {
            SortedDictionary<string, object> cimSubGeographicalRegions = concreteModel.GetAllObjectsOfType("AMIProfile.SubGeographicalRegion");
            if (cimSubGeographicalRegions != null)
            {
                foreach (KeyValuePair<string, object> cimSubGeographicalRegionPair in cimSubGeographicalRegions)
                {
                    SubGeographicalRegion cimSubGeographicalRegion = cimSubGeographicalRegionPair.Value as SubGeographicalRegion;

                    ResourceDescription rd = CreateSubGeographicalRegionResourceDescription(cimSubGeographicalRegion);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("SubGeographicalRegion ID = ").Append(cimSubGeographicalRegion.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("SubGeographicalRegion ID = ").Append(cimSubGeographicalRegion.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateSubGeographicalRegionResourceDescription(SubGeographicalRegion cimSubGeographicalRegion)
        {
            ResourceDescription rd = null;
            if (cimSubGeographicalRegion != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SUBGEOREGION, importHelper.CheckOutIndexForDMSType(DMSType.SUBGEOREGION));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimSubGeographicalRegion.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateSubGeographicalRegionProperties(cimSubGeographicalRegion, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSubstation()
        {
            SortedDictionary<string, object> cimSubstations = concreteModel.GetAllObjectsOfType("AMIProfile.Substation");
            if (cimSubstations != null)
            {
                foreach (KeyValuePair<string, object> cimSubstationPair in cimSubstations)
                {
                    Substation cimSubstation = cimSubstationPair.Value as Substation;

                    ResourceDescription rd = CreateSubstationResourceDescription(cimSubstation);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("Substation ID = ").Append(cimSubstation.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("Substation ID = ").Append(cimSubstation.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateSubstationResourceDescription(Substation cimSubstation)
        {
            ResourceDescription rd = null;
            if (cimSubstation != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SUBSTATION, importHelper.CheckOutIndexForDMSType(DMSType.SUBSTATION));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimSubstation.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateSubstationProperties(cimSubstation, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportVoltageLevel()
        {
            SortedDictionary<string, object> cimVoltageLevels = concreteModel.GetAllObjectsOfType("AMIProfile.VoltageLevel");
            if (cimVoltageLevels != null)
            {
                foreach (KeyValuePair<string, object> cimVoltageLevelPair in cimVoltageLevels)
                {
                    VoltageLevel cimVoltageLevel = cimVoltageLevelPair.Value as VoltageLevel;

                    ResourceDescription rd = CreateVoltageLevelResourceDescription(cimVoltageLevel);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("VoltageLevel ID = ").Append(cimVoltageLevel.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("VoltageLevel ID = ").Append(cimVoltageLevel.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateVoltageLevelResourceDescription(VoltageLevel cimVoltageLevel)
        {
            ResourceDescription rd = null;
            if (cimVoltageLevel != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.VOLTAGELEVEL, importHelper.CheckOutIndexForDMSType(DMSType.VOLTAGELEVEL));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimVoltageLevel.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateVoltageLevelProperties(cimVoltageLevel, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportPowerTransformer()
        {
            SortedDictionary<string, object> cimPowerTransformers = concreteModel.GetAllObjectsOfType("AMIProfile.PowerTransformer");
            if (cimPowerTransformers != null)
            {
                foreach (KeyValuePair<string, object> cimPowerTransformerPair in cimPowerTransformers)
                {
                    PowerTransformer cimPowerTransformer = cimPowerTransformerPair.Value as PowerTransformer;

                    ResourceDescription rd = CreatePowerTransformerResourceDescription(cimPowerTransformer);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("PowerTransformer ID = ").Append(cimPowerTransformer.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("PowerTransformer ID = ").Append(cimPowerTransformer.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreatePowerTransformerResourceDescription(PowerTransformer cimPowerTransformer)
        {
            ResourceDescription rd = null;
            if (cimPowerTransformer != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.POWERTRANSFORMER, importHelper.CheckOutIndexForDMSType(DMSType.POWERTRANSFORMER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimPowerTransformer.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulatePowerTransformerProperties(cimPowerTransformer, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportPowerTransformerEnd()
        {
            SortedDictionary<string, object> cimPowerTransformerEnds = concreteModel.GetAllObjectsOfType("AMIProfile.PowerTransformerEnd");
            if (cimPowerTransformerEnds != null)
            {
                foreach (KeyValuePair<string, object> cimPowerTransformerEndPair in cimPowerTransformerEnds)
                {
                    PowerTransformerEnd cimPowerTransformerEnd = cimPowerTransformerEndPair.Value as PowerTransformerEnd;

                    ResourceDescription rd = CreatePowerTransformerEndResourceDescription(cimPowerTransformerEnd);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("PowerTransformerEnd ID = ").Append(cimPowerTransformerEnd.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("PowerTransformerEnd ID = ").Append(cimPowerTransformerEnd.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreatePowerTransformerEndResourceDescription(PowerTransformerEnd cimPowerTransformerEnd)
        {
            ResourceDescription rd = null;
            if (cimPowerTransformerEnd != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.POWERTRANSEND, importHelper.CheckOutIndexForDMSType(DMSType.POWERTRANSEND));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimPowerTransformerEnd.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulatePowerTransformerEndProperties(cimPowerTransformerEnd, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportRatioTapChanger()
        {
            SortedDictionary<string, object> cimRatioTapChangers = concreteModel.GetAllObjectsOfType("AMIProfile.RatioTapChanger");
            if (cimRatioTapChangers != null)
            {
                foreach (KeyValuePair<string, object> cimRatioTapChangerPair in cimRatioTapChangers)
                {
                    RatioTapChanger cimRatioTapChanger = cimRatioTapChangerPair.Value as RatioTapChanger;

                    ResourceDescription rd = CreateRatioTapChangerResourceDescription(cimRatioTapChanger);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("RatioTapChanger ID = ").Append(cimRatioTapChanger.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("RatioTapChanger ID = ").Append(cimRatioTapChanger.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateRatioTapChangerResourceDescription(RatioTapChanger cimRatioTapChanger)
        {
            ResourceDescription rd = null;
            if (cimRatioTapChanger != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.RATIOTAPCHANGER, importHelper.CheckOutIndexForDMSType(DMSType.RATIOTAPCHANGER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimRatioTapChanger.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateRatioTapChangerProperties(cimRatioTapChanger, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportEnergyConsumer()
        {
            SortedDictionary<string, object> cimEnergyConsumers = concreteModel.GetAllObjectsOfType("AMIProfile.EnergyConsumer");
            if (cimEnergyConsumers != null)
            {
                foreach (KeyValuePair<string, object> cimEnergyConsumerPair in cimEnergyConsumers)
                {
                    EnergyConsumer cimEnergyConsumer = cimEnergyConsumerPair.Value as EnergyConsumer;

                    ResourceDescription rd = CreateEnergyConsumerResourceDescription(cimEnergyConsumer);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("EnergyConsumer ID = ").Append(cimEnergyConsumer.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("EnergyConsumer ID = ").Append(cimEnergyConsumer.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateEnergyConsumerResourceDescription(EnergyConsumer cimEnergyConsumer)
        {
            ResourceDescription rd = null;
            if (cimEnergyConsumer != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENERGYCONS, importHelper.CheckOutIndexForDMSType(DMSType.ENERGYCONS));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimEnergyConsumer.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateEnergyConsumerProperties(cimEnergyConsumer, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportAnalogMeasurement()
        {
            SortedDictionary<string, object> cimAnalogs = concreteModel.GetAllObjectsOfType("AMIProfile.Analog");
            if (cimAnalogs != null)
            {
                foreach (KeyValuePair<string, object> cimAnalogPair in cimAnalogs)
                {
                    Analog cimAnalog = cimAnalogPair.Value as Analog;

                    ResourceDescription rd = CreateAnalogMeasurementResourceDescription(cimAnalog);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("Analog ID = ").Append(cimAnalog.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("Analog ID = ").Append(cimAnalog.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateAnalogMeasurementResourceDescription(Analog cimAnalog)
        {
            ResourceDescription rd = null;
            if (cimAnalog != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ANALOG, importHelper.CheckOutIndexForDMSType(DMSType.ANALOG));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimAnalog.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateAnalogProperties(cimAnalog, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportDiscreteMeasurement()
        {
            SortedDictionary<string, object> cimDiscretes = concreteModel.GetAllObjectsOfType("AMIProfile.Discrete");
            if (cimDiscretes != null)
            {
                foreach (KeyValuePair<string, object> cimDiscretePair in cimDiscretes)
                {
                    Discrete cimDiscrete = cimDiscretePair.Value as Discrete;

                    ResourceDescription rd = CreateDiscreteMeasurementResourceDescription(cimDiscrete);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("Discrete ID = ").Append(cimDiscrete.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("Discrete ID = ").Append(cimDiscrete.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateDiscreteMeasurementResourceDescription(Discrete cimDiscrete)
        {
            ResourceDescription rd = null;
            if (cimDiscrete != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.DISCRETE, importHelper.CheckOutIndexForDMSType(DMSType.DISCRETE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimDiscrete.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateDiscreteProperties(cimDiscrete, rd, importHelper, report);
            }
            return rd;
        }

        #endregion Import
    }
}

