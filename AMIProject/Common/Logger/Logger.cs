using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Common.Logger
{
    public class Logger
    {
        private static object lockObject = new object();

        public static void LogMessageToFile(string msg, string path)
        {
            lock (lockObject)
            {
                if (!File.Exists(path))
                {
                    var file = System.IO.File.Create(path);
                    file.Close();
                }
                System.IO.StreamWriter sw = System.IO.File.AppendText(path);
                string logline = string.Format(
                    "{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logline);
                sw.Flush();
                sw.Close();
            }
        }
    }
}
