using Automatak.DNP3.Interface;
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
        Dictionary<int, TC57CIM.IEC61970.Meas.Measurement> measurements;

        public SOEHandler(ref Dictionary<int, TC57CIM.IEC61970.Meas.Measurement> measurements)
        {
            this.measurements = measurements;
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

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Analog>> values)
        {
            if (canExecute)
            {
                List<IndexedValue<Analog>> analogs = new List<IndexedValue<Analog>>();
                analogs.AddRange(values.ToList());
                foreach (IndexedValue<Analog> analog in analogs)
                {
                    if (analog.Value.Value != 0)
                    {
                        Console.WriteLine("[" + analog.Index + "]" + "=" + analog.Value.Value);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public void Start()
        {
            this.canExecute = true;
        }
    }
}
