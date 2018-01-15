using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automatak.DNP3.Interface;
using Automatak.DNP3.Adapter;
using System.ServiceModel;
using FTN.ServiceContracts;

namespace AMISimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "RTU";
            AMISimulator simulator = new AMISimulator();
            simulator.SendPointValues();
        }
    }
}
