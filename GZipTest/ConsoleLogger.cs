using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    /// <summary>
    /// Log information to the console
    /// </summary>
    public class ConsoleLogger : ILogger, IDisposable
    {
        private static StreamWriter _stream; // Log stream
        private StringBuilder _stringBuilder;
        private readonly int capacity = 128;
        private bool _disposed = false;
        /// <summary>
        /// Open console stream and write header
        /// </summary>
        /// <param name="autoFlush">true to force stream to flush its buffer after every call to write; 
        /// otherwise, false(default)</param>
        public ConsoleLogger(bool autoFlush)
        {
            _stringBuilder = new StringBuilder(capacity);
            _stream = new StreamWriter(Console.OpenStandardOutput());
            _stream.AutoFlush = autoFlush;
            Resources.GetLogHeader(_stringBuilder);
            _stream.WriteLine(_stringBuilder.ToString());
            _stringBuilder.Clear();
        }

        public ConsoleLogger():this(false)
        {
        }
        public void Error(Exception ex)
        {
            _stringBuilder.AppendFormat("{0} ThreadID: {1}; Error: {2}",
                DateTime.Now, Thread.CurrentThread.ManagedThreadId, ex.GetType().FullName);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Free any other managed objects here.
                _stream.Dispose();
            }
            // Free any unmanaged objects here.
            _disposed = true;
        }
    }
}
