using System;
using System.Reflection;
using System.Text;
using System.Threading;

namespace GZipTest
{
    public static class LogResources
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
}
