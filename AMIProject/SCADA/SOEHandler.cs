using Automatak.DNP3.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
    public class SOEHandler : PrintingSOEHandler
    {
        public SOEHandler()
            :base()
        { }

        public void Process(HeaderInfo info, IEnumerable<IndexedValue<Analog>> values)
        {
            List<IndexedValue<Analog>> analogs = values.ToList();
            foreach(IndexedValue<Analog> analog in analogs)
            {
                if(analog.Value.Value != 0)
                {
                    Console.WriteLine("[" + analog.Index + "]" + "=" + analog.Value.Value + "\n");
                }
                else
                {
                    break;
                }
            }
        }
    }
}
