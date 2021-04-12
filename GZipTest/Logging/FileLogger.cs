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
    /// Log information to the text file
    /// </summary>
    public class FileLogger : ILogger, IDisposable
    {
        private StreamWriter _stream; // Log stream
        private StringBuilder _stringBuilder;
        private readonly int capacity = 128;
        private bool _disposed = false;
        /// <summary>
        /// Open file stream and write header
        /// </summary>
        /// <param name="filePath">path to the log file</param>
        public FileLogger(string filePath)
        {
            if (filePath == null) filePath = LogResources.FilePath;
            Directory.CreateDirectory(filePath);
            _stringBuilder = new StringBuilder(capacity);

            _stringBuilder.AppendFormat("{0}{1}.{2}.log", filePath, LogResources.AssemblyName, DateTime.Now.ToString(LogResources.Format));
            _stream = new StreamWriter(_stringBuilder.ToString(), true);
            _stringBuilder.Clear();
            LogResources.GetLogHeader(_stringBuilder);
            _stream.WriteLine(_stringBuilder.ToString());
            _stringBuilder.Clear();
        }

        public FileLogger() : this(LogResources.FilePath)
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
            _stringBuilder.AppendFormat("{0}\t{1}", DateTime.Now.ToString("G"), String.Format(message, values));
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
