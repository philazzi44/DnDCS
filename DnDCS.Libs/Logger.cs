using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DnDCS.Libs
{
    // TODO: Change for log4net at some point
    public static class Logger
    {
        public enum LogMode
        {
            Full = -1,
            Error = 0,
            Warning = 1,
            Info = 2,
            Debug = 3,
        }

        public static LogMode Mode { get; set; }
        public static string FileSuffix { get; set; }

        static Logger()
        {
            Mode = LogMode.Full;
            FileSuffix = string.Empty;
        }

        private static string LogFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FileSuffix))
                    return string.Format("Log{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
                else
                    return string.Format("Log{0}-{1}.log", FileSuffix, DateTime.Now.ToString("yyyy-MM-dd"));
            }
        }

        private static string LogDateTime
        {
            get { return DateTime.Now.ToString("s"); }
        }

        public static void LogError(string message, Exception e = null)
        {
            if ((int)Mode > (int)LogMode.Error)
                return;

            try
            {
                using (var stream = new StreamWriter(LogFileName, true))
                {
                    stream.WriteLine(string.Format("ERROR @ {0}", LogDateTime));
                    stream.WriteLine(string.Format("Message: {0}", message));
                    if (e != null)
                        stream.WriteLine(string.Format("Exception: {0}", e));
                    stream.WriteLine("----");
                }
            }
            catch
            {

            }
        }

        public static void LogWarning(string message, Exception e = null)
        {
            if ((int)Mode > (int)LogMode.Warning)
                return;

            try
            {
                using (var stream = new StreamWriter(LogFileName, true))
                {
                    stream.WriteLine(string.Format("WARNING @ {0}", LogDateTime));
                    stream.WriteLine(string.Format("Message: {0}", message));
                    if (e != null)
                        stream.WriteLine(string.Format("Exception: {0}", e));
                    stream.WriteLine("----");
                }
            }
            catch
            {

            }
        }

        public static void LogInfo(string message, Exception e = null)
        {
            if ((int)Mode > (int)LogMode.Info)
                return;

            try
            {
                using (var stream = new StreamWriter(LogFileName, true))
                {
                    stream.WriteLine(string.Format("INFO @ {0}", LogDateTime));
                    stream.WriteLine(string.Format("Message: {0}", message));
                    if (e != null)
                        stream.WriteLine(string.Format("Exception: {0}", e));
                    stream.WriteLine("----");
                }
            }
            catch
            {

            }
        }

        public static void LogDebug(string message, Exception e = null)
        {
            if ((int)Mode > (int)LogMode.Debug)
                return;

            try
            {
                using (var stream = new StreamWriter(LogFileName, true))
                {
                    stream.WriteLine(string.Format("DEBUG @ {0}", LogDateTime));
                    stream.WriteLine(string.Format("Message: {0}", message));
                    if (e != null)
                        stream.WriteLine(string.Format("Exception: {0}", e));
                    stream.WriteLine("----");
                }
            }
            catch
            {

            }
        }
    }
}