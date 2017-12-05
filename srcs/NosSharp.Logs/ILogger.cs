using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.Logs
{
    public interface ILogger
    {
        void InsertLog(ILoggable log);

        void InsertLogs(IEnumerable<ILoggable> logs);

        void DeleteAll(string loggableCategory);
    }
}