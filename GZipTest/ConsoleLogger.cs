using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Veeam.Common
{
    /// <summary>
    /// Log information to the console
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private static StreamWriter _stream; // Log stream
        private StringBuilder _stringBuilder;
        private readonly int capacity = 128;
        public ConsoleLogger()
        {
            _stringBuilder = new StringBuilder(capacity);
            _stream = new StreamWriter(Console.OpenStandardOutput());
            Resources.GetLogHeader(_stringBuilder);
            _stream.WriteLine(_stringBuilder.ToString());
        }

        public void Error(Exception ex)
        {
            _stringBuilder.AppendFormat("{0} ThreadID: {1} Error {2}",
                DateTime.Now, Thread.CurrentThread.ManagedThreadId, ex.Message);
            _stream.WriteLine(_stringBuilder.ToString());
            _stringBuilder.Clear();
        }

        public void WriteLine(string message, params object[] values)
        {
            _stringBuilder.AppendFormat("{0}\t{1}", DateTime.Now.ToString("T"), String.Format(message, values));
            _stream.WriteLine(_stringBuilder.ToString());
            _stringBuilder.Clear();
        }

        public void Warning(string message, params object[] values)
        {
            _stringBuilder.AppendFormat("Warning: {0}", String.Format(message, values));
            _stream.WriteLine(_stringBuilder.ToString());
            _stringBuilder.Clear();
        }
    }
}
