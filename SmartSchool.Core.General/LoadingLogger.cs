using System;
using System.IO;
using System.Text;

namespace SmartSchool.Core.General
{
    public static class LoadingLogger
    {
        private static string LogPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "loading_log.txt");

        // Removed static constructor to avoid resetting log

        public static void Log(string message)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(LogPath))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " - " + message);
                }
            }
            catch { }
        }

        public static void Start(string processName)
        {
            Log("Start: " + processName);
        }

        public static void End(string processName)
        {
            Log("End: " + processName);
        }
    }
}
