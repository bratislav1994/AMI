using Automatak.DNP3.Interface;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
    public class SOEHandler : ISOEHandler
    {
        bool canExecute = false;
        Dictionary<int, ResourceDescription> measurements;
        List<ResourceDescription> resourcesToSend;
        object lockObject;

        public SOEHandler(ref Dictionary<int, ResourceDescription> measurements, ref List<ResourceDescription> resourcesToSend, ref object lockObject)
        {
            this.measurements = measurements;
            this.resourcesToSend = resourcesToSend;
            this.lockObject = lockObject;
        }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Analog>> values)
        {
            if (canExecute)
            {
                lock (lockObject)
                {
                    List<IndexedValue<Analog>> analogs = new List<IndexedValue<Analog>>();
                    analogs.AddRange(values.ToList());
                    foreach (IndexedValue<Analog> analog in analogs)
                    {
                        if (analog.Value.Value != 0)
                        {
                            ResourceDescription newRD = new ResourceDescription();
                            newRD.Id = measurements[analog.Index].Id;
                            foreach (Property p in measurements[analog.Index].Properties)
                            {
                                if (p.Id == ModelCode.ANALOG_NORMALVALUE)
                                {
                                    Property newP = new Property(p.Id, (float)analog.Value.Value);
                                    newRD.AddProperty(newP);
                                }
                                else
                                {
                                    newRD.AddProperty(p);
                                }
                            }
                            resourcesToSend.Add(newRD);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
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
