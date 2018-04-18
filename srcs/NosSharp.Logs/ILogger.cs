using System.Collections.Generic;

namespace NosSharp.Logs
{
    public interface ILogger
    {
        void InsertLog(ILoggable log);

        void InsertLogs(IEnumerable<ILoggable> logs);

        void DeleteAll(string loggableCategory);
    }
}