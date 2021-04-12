using System;

namespace GZipTest
{
    public static class Log
    {
        private static ILogger _logger;
        private static readonly object _lock = new object();

        static Log()
        {
            AppDomain.CurrentDomain.ProcessExit += DisposeLogger;
        }
        public static void SetLogger<T>(T logger) where T: ILogger, IDisposable
        {
            _logger = logger;
        }

        public static void WriteLine (string message, params object[] values)
        {
            lock (_lock)
            {
                _logger.WriteLine(message, values);
            }
        }

        public static void Error(Exception ex)
        {
            lock (_lock)
            {
                _logger.Error(ex);
            }
        }

        public static void Warning(string message, params object[] values)
        {
            lock (_lock)
            {
                _logger.Warning(message, values);
            }
        }
        private static void DisposeLogger(object sender, EventArgs e)
        {
            ((IDisposable)_logger).Dispose();
        }
    }
}
