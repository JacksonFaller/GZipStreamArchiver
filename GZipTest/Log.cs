using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Reflection;

namespace Veeam.Common
{
    public static class Resources
    {
        public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        // Default file path
        public static readonly string FilePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Logs\";
        // DateTime format for log file name
        public static readonly string Format = "dd.MM.yy.HH.mm";

        public static void GetLogHeader(StringBuilder stringBuilder)
        {
            stringBuilder.AppendFormat("Machine Name: {0}{1}", Environment.MachineName, Environment.NewLine);
            stringBuilder.AppendFormat("User Name: {0}{1}", Environment.UserName, Environment.NewLine);
            stringBuilder.AppendFormat("Time: {0} GMT+{1}{2}",
               DateTime.Now, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours, Environment.NewLine);
            stringBuilder.AppendFormat("Current thread ID: {0}{1}", Thread.CurrentThread.ManagedThreadId, Environment.NewLine);
        }
    }
    
    public static class Log
    {
        static ILogger Logger;

        private static object _lock = new object();
        public static void SetLogger(ILogger logger)
        {
            Logger = logger;
        }

        public static void WriteLine (string message, params object[] values)
        {
            lock (_lock)
            {
                Logger.WriteLine(message, values);
            }
        }

        public static void Error(Exception ex)
        {
            lock (_lock)
            {
                Logger.Error(ex);
            }
        }

        public static void Warning(string message, params object[] values)
        {
            lock (_lock)
            {
                Logger.Warning(message, values);
            }
        }
    }
}
