using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public interface ILogger
    {
        /// <summary>
        /// Log exception information to the log
        /// </summary>
        /// <param name="ex">exception</param>
        void Error(Exception ex); 

        /// <summary>
        /// Write formated message to the log
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="values">objects to format</param>
        void WriteLine(string format, params object[] values);

        /// <summary>
        /// Write warning message and current DateTime to the log
        /// </summary>
        /// <param name="message">format message</param>
        /// <param name="values">objects to format</param>
        void Warning(string message, params object[] values); 
    }
}
