using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common.Logger
{
    public class Logger
    {
        private static string fileName;
        
        public static string FileName { get; set; }
        
        static Logger()
        {
            fileName = "~..\\..\\..\\..\\..\\..\\CommonFiles\\Logging\\logs.txt";
        }

        public static void LogMessageToFile(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(fileName);
            try
            {
                string logline = string.Format(
                    "{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logline);
            }
            finally
            {
                sw.Close();
            }
        }
    }
}
