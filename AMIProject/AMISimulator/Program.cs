using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automatak.DNP3.Interface;
using Automatak.DNP3.Adapter;

namespace AMISimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            AMISimulator simulator = new AMISimulator();
            simulator.SendPointValues();
        }
    }
}
