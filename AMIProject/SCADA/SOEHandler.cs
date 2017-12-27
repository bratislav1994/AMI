using Automatak.DNP3.Interface;
using FTN.Common;
using FTN.Common.Logger;
using FTN.Services.NetworkModelService.DataModel;
using System;
using System.Collections.Generic;
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
        public List<DynamicMeasurement> resourcesToSend;
        private object lockObject;
        private bool hasNewMeas = false;

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

        public SOEHandler(List<MeasurementForScada> measurements, List<DynamicMeasurement> resourcesToSend, ref object lockObject)
        {
            this.measurements = measurements;
            this.resourcesToSend = resourcesToSend;
            this.lockObject = lockObject;
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
                    Dictionary<long, DynamicMeasurement> localDic = new Dictionary<long, DynamicMeasurement>(this.measurements.Count/3);
                    
                    foreach (IndexedValue<Automatak.DNP3.Interface.Analog> analog in analogs)
                    {
                        if (analog.Value.Value != 0)
                        {
                            TC57CIM.IEC61970.Meas.Analog a = new TC57CIM.IEC61970.Meas.Analog();
                            a.RD2Class(GetResDesc(analog.Index));
                            
                            if (localDic.ContainsKey(a.PowerSystemResourceRef))
                            {
                                switch ((analog.Index % 1000) % 3)
                                {
                                    case 0:
                                        localDic[a.PowerSystemResourceRef].CurrentP = (float)analog.Value.Value;
                                        break;
                                    case 1:
                                        localDic[a.PowerSystemResourceRef].CurrentQ = (float)analog.Value.Value;
                                        break;
                                    case 2:
                                        localDic[a.PowerSystemResourceRef].CurrentV = (float)analog.Value.Value;
                                        break;
                                }
                            }
                            else
                            {
                                localDic.Add(a.PowerSystemResourceRef, new DynamicMeasurement(a.PowerSystemResourceRef));

                                switch ((analog.Index % 1000) % 3)
                                {
                                    case 0:
                                        localDic[a.PowerSystemResourceRef].CurrentP = (float)analog.Value.Value;
                                        break;
                                    case 1:
                                        localDic[a.PowerSystemResourceRef].CurrentQ = (float)analog.Value.Value;
                                        break;
                                    case 2:
                                        localDic[a.PowerSystemResourceRef].CurrentV = (float)analog.Value.Value;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    foreach (KeyValuePair<long, DynamicMeasurement> kvp in localDic)
                    {
                        resourcesToSend.Add(kvp.Value);
                    }

                    if (localDic.Count > 0)
                    {
                        HasNewMeas = true;
                    }
                   
                    Logger.LogMessageToFile(string.Format("SCADA.SOEHandler.Process; line: {0}; Finish the Process function", (new System.Diagnostics.StackFrame(0, true)).GetFileLineNumber()));
                }
            }
        }

        private ResourceDescription GetResDesc(int index)
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
