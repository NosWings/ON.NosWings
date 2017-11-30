using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace NosSharp.Logs
{
    class LogManager : IDisposable
    {
        private readonly NosSharpLogger _logger;
        private readonly List<AbstractLog> _logs;
        private readonly long _maxLogToFlush;

        /// <summary>
        /// LogManager instanciate a NosSharpLogger client
        /// </summary>
        public LogManager()
        {
            _maxLogToFlush = 1500;
            _logs = new List<AbstractLog>();
            _logger = new NosSharpLogger("NosSharp.Logs");
        }


        #region Singleton

        private LogManager _instance;

        public LogManager Instance
        {
            get { return _instance ?? (_instance = new LogManager()); }
        }

        #endregion

        public void AddLog(AbstractLog log)
        {
            if (_logs.Count > _maxLogToFlush)
            {
                Flush();
            }

            _logs.Add(log);
        }

        public void AddLog(IEnumerable<AbstractLog> logs)
        {
            if (_logs.Count > _maxLogToFlush)
            {
                Flush();
            }

            _logs.AddRange(logs);
        }

        /// <summary>
        /// Flush the stored logs through
        /// </summary>
        private void Flush()
        {
            foreach (IGrouping<string, AbstractLog> abstractLogs in _logs.GroupBy(s => s.Collection))
            {
                _logger.InsertLogs(abstractLogs.Select(g => g.ToBsonDocument()), abstractLogs.Key);
            }

            _logs.Clear();
        }

        public void Dispose()
        {
            Flush();
        }
    }
}