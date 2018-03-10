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
        private static string path = string.Empty;

        public static string Path
        {
            get
            {
                return path;
            }

            set
            {
                if (path != value)
                {
                    path = value;
                }
            }
        }

        public static void LogMessageToFile(string msg)
        {
            lock (lockObject)
            {
                if (!File.Exists(path))
                {
                    var file = System.IO.File.Create(path);
                    file.Close();
                }

                try
                {
                    System.IO.StreamWriter sw = System.IO.File.AppendText(path);
                    string logline = string.Format(
                        "{0:G}: {1}.", System.DateTime.Now, msg);
                    sw.WriteLine(logline);
                    sw.Flush();
                    sw.Close();
                }
                catch (IOException e)
                {

                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
