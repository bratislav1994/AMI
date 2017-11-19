using System;
using System.Collections.Generic;
using CIM.Model;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;

namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	/// <summary>
	/// PowerTransformerImporter
	/// </summary>
	public class PowerTransformerImporter
	{
		/// <summary> Singleton </summary>
		private static PowerTransformerImporter ptImporter = null;
		private static object singletoneLock = new object();

		private ConcreteModel concreteModel;
		private Delta delta;
		private ImportHelper importHelper;
		private TransformAndLoadReport report;


		#region Properties
		public static PowerTransformerImporter Instance
		{
			get
			{
				if (ptImporter == null)
				{
					lock (singletoneLock)
					{
						if (ptImporter == null)
						{
							ptImporter = new PowerTransformerImporter();
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
            ImportRegulatingControl();
            ImportReactiveCapabilityCurve();
            ImportCurveData();
            ImportSynMachine();
            ImportFrequencyConverter();
            ImportShuntCompensator();
            ImportStaticVarCompensator();
            ImportControl();
            ImportTerminal();

			LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
		}

        #region Import
        private void ImportRegulatingControl()
        {
            SortedDictionary<string, object> cimRegulControls = concreteModel.GetAllObjectsOfType("FTN.RegulatingControl");
            if (cimRegulControls != null)
            {
                foreach (KeyValuePair<string, object> cimRegulControlPair in cimRegulControls)
                {
                    FTN.RegulatingControl cimRegulControl = cimRegulControlPair.Value as FTN.RegulatingControl;

                    ResourceDescription rd = CreateRegulatingControlResourceDescription(cimRegulControl);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("RegulatingControl ID = ").Append(cimRegulControl.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("RegulatingControl ID = ").Append(cimRegulControl.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateRegulatingControlResourceDescription(FTN.RegulatingControl cimRegulControl)
        {
            ResourceDescription rd = null;
            if (cimRegulControl != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.REGULCONTROL, importHelper.CheckOutIndexForDMSType(DMSType.REGULCONTROL));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimRegulControl.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateRegulatingControlProperties(cimRegulControl, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportReactiveCapabilityCurve()
        {
            SortedDictionary<string, object> cimReactCapabCurves = concreteModel.GetAllObjectsOfType("FTN.ReactiveCapabilityCurve");
            if (cimReactCapabCurves != null)
            {
                foreach (KeyValuePair<string, object> cimReactCapabCurvePair in cimReactCapabCurves)
                {
                    FTN.ReactiveCapabilityCurve cimReactCapabCurve = cimReactCapabCurvePair.Value as FTN.ReactiveCapabilityCurve;

                    ResourceDescription rd = CreateReactCapabCurveResourceDescription(cimReactCapabCurve);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("ReactiveCapabilityCurve ID = ").Append(cimReactCapabCurve.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("ReactiveCapabilityCurve ID = ").Append(cimReactCapabCurve.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateReactCapabCurveResourceDescription(FTN.ReactiveCapabilityCurve cimReactCapabCurve)
        {
            ResourceDescription rd = null;
            if (cimReactCapabCurve != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.REACTCAPABCURVE, importHelper.CheckOutIndexForDMSType(DMSType.REACTCAPABCURVE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimReactCapabCurve.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateReactCapabCurveProperties(cimReactCapabCurve, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportCurveData()
        {
            SortedDictionary<string, object> cimCurveDatas = concreteModel.GetAllObjectsOfType("FTN.CurveData");
            if (cimCurveDatas != null)
            {
                foreach (KeyValuePair<string, object> cimCurveDataPair in cimCurveDatas)
                {
                    FTN.CurveData cimCurveData = cimCurveDataPair.Value as FTN.CurveData;

                    ResourceDescription rd = CreateCurveDataResourceDescription(cimCurveData);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("CurveData ID = ").Append(cimCurveData.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("CurveData ID = ").Append(cimCurveData.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateCurveDataResourceDescription(FTN.CurveData cimCurveData)
        {
            ResourceDescription rd = null;
            if (cimCurveData != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.CURVEDATA, importHelper.CheckOutIndexForDMSType(DMSType.CURVEDATA));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimCurveData.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateCurveDataProperties(cimCurveData, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSynMachine()
        {
            SortedDictionary<string, object> cimSynMachines = concreteModel.GetAllObjectsOfType("FTN.SynchronousMachine");
            if (cimSynMachines != null)
            {
                foreach (KeyValuePair<string, object> cimSynMachinePair in cimSynMachines)
                {
                    FTN.SynchronousMachine cimSynMachine = cimSynMachinePair.Value as FTN.SynchronousMachine;

                    ResourceDescription rd = CreateSynMachineResourceDescription(cimSynMachine);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("SynchronousMachine ID = ").Append(cimSynMachine.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("SynchronousMachine ID = ").Append(cimSynMachine.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateSynMachineResourceDescription(FTN.SynchronousMachine cimSynMachine)
        {
            ResourceDescription rd = null;
            if (cimSynMachine != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SYNMACHINE, importHelper.CheckOutIndexForDMSType(DMSType.SYNMACHINE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimSynMachine.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateSynMachineProperties(cimSynMachine, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportFrequencyConverter()
        {
            SortedDictionary<string, object> cimFreqConverters = concreteModel.GetAllObjectsOfType("FTN.FrequencyConverter");
            if (cimFreqConverters != null)
            {
                foreach (KeyValuePair<string, object> cimFreqConverterPair in cimFreqConverters)
                {
                    FTN.FrequencyConverter cimFreqConverter = cimFreqConverterPair.Value as FTN.FrequencyConverter;

                    ResourceDescription rd = CreateFrequencyConverterResourceDescription(cimFreqConverter);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("FrequencyConverter ID = ").Append(cimFreqConverter.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("FrequencyConverter ID = ").Append(cimFreqConverter.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateFrequencyConverterResourceDescription(FTN.FrequencyConverter cimFreqConverter)
        {
            ResourceDescription rd = null;
            if (cimFreqConverter != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.FREQCONVERTER, importHelper.CheckOutIndexForDMSType(DMSType.FREQCONVERTER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimFreqConverter.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateFreqControlProperties(cimFreqConverter, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportShuntCompensator()
        {
            SortedDictionary<string, object> cimShuntCompensators = concreteModel.GetAllObjectsOfType("FTN.ShuntCompensator");
            if (cimShuntCompensators != null)
            {
                foreach (KeyValuePair<string, object> cimShuntCompensatorPair in cimShuntCompensators)
                {
                    FTN.ShuntCompensator cimShuntCompensator = cimShuntCompensatorPair.Value as FTN.ShuntCompensator;

                    ResourceDescription rd = CreateShuntCompensatorResourceDescription(cimShuntCompensator);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("ShuntCompensator ID = ").Append(cimShuntCompensator.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("ShuntCompensator ID = ").Append(cimShuntCompensator.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateShuntCompensatorResourceDescription(FTN.ShuntCompensator cimShuntCompensator)
        {
            ResourceDescription rd = null;
            if (cimShuntCompensator != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SHUNTCOMPENSATOR, importHelper.CheckOutIndexForDMSType(DMSType.SHUNTCOMPENSATOR));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimShuntCompensator.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateShuntCompensatorProperties(cimShuntCompensator, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportStaticVarCompensator()
        {
            SortedDictionary<string, object> cimStaticVarCompensators = concreteModel.GetAllObjectsOfType("FTN.StaticVarCompensator");
            if (cimStaticVarCompensators != null)
            {
                foreach (KeyValuePair<string, object> cimStaticVarCompensatorPair in cimStaticVarCompensators)
                {
                    FTN.StaticVarCompensator cimStaticVarCompensator = cimStaticVarCompensatorPair.Value as FTN.StaticVarCompensator;

                    ResourceDescription rd = CreateStaticVarCompensatorResourceDescription(cimStaticVarCompensator);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("StaticVarCompensator ID = ").Append(cimStaticVarCompensator.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("StaticVarCompensator ID = ").Append(cimStaticVarCompensator.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateStaticVarCompensatorResourceDescription(FTN.StaticVarCompensator cimStaticVarCompensator)
        {
            ResourceDescription rd = null;
            if (cimStaticVarCompensator != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.STATICVARCOMPENSATOR, importHelper.CheckOutIndexForDMSType(DMSType.STATICVARCOMPENSATOR));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimStaticVarCompensator.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateStaticVarCompensatorProperties(cimStaticVarCompensator, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportControl()
        {
            SortedDictionary<string, object> cimControls = concreteModel.GetAllObjectsOfType("FTN.Control");
            if (cimControls != null)
            {
                foreach (KeyValuePair<string, object> cimControlPair in cimControls)
                {
                    FTN.Control cimControl = cimControlPair.Value as FTN.Control;

                    ResourceDescription rd = CreateControlResourceDescription(cimControl);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("Control ID = ").Append(cimControl.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("Control ID = ").Append(cimControl.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateControlResourceDescription(FTN.Control cimControl)
        {
            ResourceDescription rd = null;
            if (cimControl != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.CONTROL, importHelper.CheckOutIndexForDMSType(DMSType.CONTROL));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimControl.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateControlProperties(cimControl, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportTerminal()
        {
            SortedDictionary<string, object> cimTerminals = concreteModel.GetAllObjectsOfType("FTN.Terminal");
            if (cimTerminals != null)
            {
                foreach (KeyValuePair<string, object> cimTerminalPair in cimTerminals)
                {
                    FTN.Terminal cimTerminal = cimTerminalPair.Value as FTN.Terminal;

                    ResourceDescription rd = CreateTerminalResourceDescription(cimTerminal);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("Terminal ID = ").Append(cimTerminal.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("Terminal ID = ").Append(cimTerminal.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateTerminalResourceDescription(FTN.Terminal cimTerminal)
        {
            ResourceDescription rd = null;
            if (cimTerminal != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.TERMINAL, importHelper.CheckOutIndexForDMSType(DMSType.TERMINAL));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimTerminal.ID, gid);

                ////populate ResourceDescription
                PowerTransformerConverter.PopulateTerminalProperties(cimTerminal, rd, importHelper, report);
            }
            return rd;
        }

		#endregion Import
	}
}

