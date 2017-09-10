using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veeam.Common
{
    public interface ILogger
    {
        void Error(Exception ex); 
        void WriteLine(string message, params object[] values);
        void Warning(string message, params object[] values); 
    }
}
