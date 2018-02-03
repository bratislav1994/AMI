using Automatak.DNP3.Interface;
using FTN.Common;
using FTN.Common.Logger;
using FTN.Services.NetworkModelService.DataModel;
using FTN.Services.NetworkModelService.DataModel.Dynamic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;

namespace SCADA
{
    public class SOEHandler : ISOEHandler
    {
        private bool canExecute = false;
        private List<MeasurementForScada> measurements;
        public Dictionary<long, DynamicMeasurement> resourcesToSend;
        private object lockObject;
        private bool hasNewMeas = false;
        private Dictionary<long, DataForScada> data;

        public bool HasNewMeas
        {
            get
            {
                return hasNewMeas;
            }

            set
            {
                hasNewMeas = value;
            }
        }

        public SOEHandler(List<MeasurementForScada> measurements, Dictionary<long, DynamicMeasurement> resourcesToSend, object lockObject, List<DataForScada> data)
        {
            this.measurements = measurements;
            this.resourcesToSend = resourcesToSend;
            this.lockObject = lockObject;
            this.data = new Dictionary<long, DataForScada>();
            data.ForEach(x => { if (x is EnergyConsumerForScada) { this.data.Add(((EnergyConsumerForScada)x).GlobalId, x); }
                                else if (x is PowerTransformerForScada) { this.data.Add(((PowerTransformerForScada)x).GlobalId, x); }
                                else if (x is BaseVoltageForScada) { this.data.Add(((BaseVoltageForScada)x).GlobalId, x); }
                                else if (x is SubstationForScada) { this.data.Add(((SubstationForScada)x).GlobalId, x); }
            });
        }

        public void UpdateData(List<DataForScada> data)
        {
            data.ForEach(x => {
                if (x is EnergyConsumerForScada) { this.data.Add(((EnergyConsumerForScada)x).GlobalId, x); }
                else if (x is PowerTransformerForScada) { this.data.Add(((PowerTransformerForScada)x).GlobalId, x); }
                else if (x is BaseVoltageForScada) { this.data.Add(((BaseVoltageForScada)x).GlobalId, x); }
                else if (x is SubstationForScada) { this.data.Add(((SubstationForScada)x).GlobalId, x); }
            });
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Automatak.DNP3.Interface.Analog>> values)
        {
            if (canExecute)
            {
                lock (lockObject)
                {
                    Logger.LogMessageToFile(string.Format("SCADA.SOEHandler.Process; line: {0}; Start the Process function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                    List<IndexedValue<Automatak.DNP3.Interface.Analog>> analogs = new List<IndexedValue<Automatak.DNP3.Interface.Analog>>();
                    analogs.AddRange(values.ToList());
                    Dictionary<long, DynamicMeasurement> localDic = new Dictionary<long, DynamicMeasurement>(this.measurements.Count / 3);
                    DateTime timeStamp = DateTime.Now;
                    Console.WriteLine("Number of points: " + analogs.Count);
                    int cnt = 0;

                    foreach (IndexedValue<Automatak.DNP3.Interface.Analog> analog in analogs)
                    {
                        if (analog.Value.Value != 0)
                        {
                            TC57CIM.IEC61970.Meas.Analog a = (TC57CIM.IEC61970.Meas.Analog)GetMeasurement(analog.Index);

                            if (a != null)
                            {
                                if (localDic.ContainsKey(a.PowerSystemResourceRef))
                                {
                                    switch (analog.Index % 3)
                                    {
                                        case 0:
                                            localDic[a.PowerSystemResourceRef].CurrentP = this.Crunching(analog);
                                            break;
                                        case 1:
                                            localDic[a.PowerSystemResourceRef].CurrentQ = this.Crunching(analog);
                                            break;
                                        case 2:
                                            localDic[a.PowerSystemResourceRef].CurrentV = this.Crunching(analog);
                                            break;
                                    }
                                }
                                else
                                {
                                    localDic.Add(a.PowerSystemResourceRef, new DynamicMeasurement(a.PowerSystemResourceRef, timeStamp));
                                    cnt++;

                                    switch (analog.Index % 3)
                                    {
                                        case 0:
                                            localDic[a.PowerSystemResourceRef].CurrentP = this.Crunching(analog);
                                            break;
                                        case 1:
                                            localDic[a.PowerSystemResourceRef].CurrentQ = this.Crunching(analog);
                                            break;
                                        case 2:
                                            localDic[a.PowerSystemResourceRef].CurrentV = this.Crunching(analog);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    foreach (KeyValuePair<long, DynamicMeasurement> kvp in resourcesToSend)
                    {
                        kvp.Value.TimeStamp = timeStamp;
                    }

                    foreach (KeyValuePair<long, DynamicMeasurement> kvp in localDic)
                    {
                        if (resourcesToSend.ContainsKey(kvp.Key))
                        {
                            if (localDic[kvp.Key].CurrentP != -1)
                            {
                                resourcesToSend[kvp.Key].CurrentP = localDic[kvp.Key].CurrentP;
                            }
                            if (localDic[kvp.Key].CurrentQ != -1)
                            {
                                resourcesToSend[kvp.Key].CurrentQ = localDic[kvp.Key].CurrentQ;
                            }
                            if (localDic[kvp.Key].CurrentV != -1)
                            {
                                resourcesToSend[kvp.Key].CurrentV = localDic[kvp.Key].CurrentV;
                            }
                        }
                        else
                        {
                            resourcesToSend.Add(kvp.Key, kvp.Value);
                        }
                    }

                    foreach(KeyValuePair<long, DynamicMeasurement> kvp in resourcesToSend)
                    {
                        this.SetAlarmStateForMeasurement(kvp.Value);
                    }

                    Console.WriteLine("Number of measurements: " + resourcesToSend.Count + " " + cnt);

                    if (this.resourcesToSend.Count > 0)
                    {
                        HasNewMeas = true;
                    }

                    Logger.LogMessageToFile(string.Format("SCADA.SOEHandler.Process; line: {0}; Finish the Process function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                }
            }
        }

        private float Crunching(IndexedValue<Automatak.DNP3.Interface.Analog> a)
        {
            //    TC57CIM.IEC61970.Meas.Analog analog = (TC57CIM.IEC61970.Meas.Analog)GetMeasurement(a.Index);

            //    float step = (analog.MaxValue - analog.MinValue) / (analog.MaxRawValue - analog.MinRawValue);
            //    double steps = a.Value.Value - analog.MinRawValue;
            //    float retVal = analog.MinValue + step * (float)steps;

            //    return retVal;
            return (float)a.Value.Value;
        }

        private void SetAlarmStateForMeasurement(DynamicMeasurement measurement)
        {
            if (resourcesToSend.ContainsKey(measurement.PsrRef))
            {
                if (resourcesToSend[measurement.PsrRef].IsAlarm)
                {
                    float nominalVoltage = ((BaseVoltageForScada)data[((EnergyConsumerForScada)data[measurement.PsrRef]).BaseVoltageId]).NominalVoltage;
                    float validRange = ((EnergyConsumerForScada)data[measurement.PsrRef]).ValidRangePercent;
                    float normalValueWithLossNegative = nominalVoltage - validRange * nominalVoltage;
                    float normalValueWithLossPositive = nominalVoltage + validRange * nominalVoltage;

                    if (measurement.CurrentV >= normalValueWithLossNegative && measurement.CurrentV <= normalValueWithLossPositive)
                    {
                        measurement.IsAlarm = false;
                        measurement.TypeVoltage = TypeVoltage.INBOUNDS;
                    }
                    else if (measurement.CurrentV > normalValueWithLossPositive)
                    {
                        measurement.IsAlarm = true;
                        measurement.TypeVoltage = TypeVoltage.OVERVOLTAGE;
                    }
                    else if (measurement.CurrentV < normalValueWithLossNegative)
                    {
                        measurement.IsAlarm = true;
                        measurement.TypeVoltage = TypeVoltage.UNDERVOLTAGE;
                    }
                }
                else
                {
                    float nominalVoltage = ((BaseVoltageForScada)data[((EnergyConsumerForScada)data[measurement.PsrRef]).BaseVoltageId]).NominalVoltage;
                    float invalidRange = ((EnergyConsumerForScada)data[measurement.PsrRef]).InvalidRangePercent;
                    float normalValueWithLossNegative = nominalVoltage - invalidRange * nominalVoltage;
                    float normalValueWithLossPositive = nominalVoltage + invalidRange * nominalVoltage;

                    if (measurement.CurrentV < normalValueWithLossNegative)
                    {
                        measurement.IsAlarm = true;
                        measurement.TypeVoltage = TypeVoltage.UNDERVOLTAGE;
                    }
                    else if (measurement.CurrentV > normalValueWithLossPositive)
                    {
                        measurement.IsAlarm = true;
                        measurement.TypeVoltage = TypeVoltage.OVERVOLTAGE;
                    }
                    else
                    {
                        measurement.IsAlarm = false;
                        measurement.TypeVoltage = TypeVoltage.INBOUNDS;
                    }
                }
            }
        }

        private Measurement GetMeasurement(int index)
        {
            foreach (MeasurementForScada meas in measurements)
            {
                if (meas.Index == index)
                {
                    return meas.Measurement;
                }
            }

            return null;
        }

        public void End()
        {
            this.canExecute = false;
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<DoubleBitBinary>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<FrozenCounter>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<AnalogOutputStatus>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<TimeAndInterval>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<AnalogCommandEvent>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<SecurityStat>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<BinaryCommandEvent>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<OctetString>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<BinaryOutputStatus>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Counter>> values)
        {
            throw new NotImplementedException();
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Binary>> values)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            this.canExecute = true;
        }
    }
}
