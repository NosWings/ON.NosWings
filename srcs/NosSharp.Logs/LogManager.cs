using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MongoDB.Bson;

namespace NosSharp.Logs
{
    public class LogManager : IDisposable
    {
        private readonly NosSharpLogger _logger;
        private readonly List<ILog> _logs;
        private readonly long _maxLogToFlush;

        /// <summary>
        ///     LogManager instanciate a NosSharpLogger client
        /// </summary>
        public LogManager()
        {
            _maxLogToFlush = 1500;
            _logs = new List<ILog>();
            _logger = new NosSharpLogger("mongodb://localhost:27017", "NosSharp.Logs");
        }

        /// <summary>
        /// Add logs
        /// </summary>
        /// <param name="log"></param>
        public void AddLog(ILog log)
        {
            if (_logs.Count > _maxLogToFlush)
            {
                Flush();
            }

            _logs.Add(log);
        }

        public void AddLog(IEnumerable<ILog> logs)
        {
            if (_logs.Count > _maxLogToFlush)
            {
                Flush();
            }

            _logs.AddRange(logs);
        }

        /// <summary>
        ///     Flush the stored logs through
        /// </summary>
        private void Flush()
        {
            foreach (IGrouping<string, ILog> abstractLogs in _logs.GroupBy(s => s.Collection))
            {
                _logger.InsertLogs(abstractLogs.Select(g => g.ToBsonDocument()), abstractLogs.Key);
            }

            _logs.Clear();
        }

        #region Disposable Pattern
        // Flag: Has Dispose already been called?
        private bool _disposed;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // NOTHING ATM
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
        }

        ~LogManager()
        {
            Dispose(false);
        }
        #endregion


        #region Singleton

        private LogManager _instance;

        public LogManager Instance
        {
            get { return _instance ?? (_instance = new LogManager()); }
        }

        #endregion
    }
}