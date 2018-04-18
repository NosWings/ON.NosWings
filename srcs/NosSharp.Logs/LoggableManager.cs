using System;
using System.Collections.Generic;

namespace NosSharp.Logs
{
    public class LoggableManager : IDisposable
    {
        private readonly List<ILoggable> _logs;
        private readonly long _maxLogToFlush;
        private ILogger _logger;

        /// <summary>
        ///     LogManager uses an ILogger to store its logs
        /// </summary>
        public LoggableManager()
        {
            _maxLogToFlush = 1500;
            _logs = new List<ILoggable>();
            _logger = null;
        }

        public void InitializeLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Add logs
        /// </summary>
        /// <param name="loggable"></param>
        public void AddLog(ILoggable loggable)
        {
            if (_logs.Count > _maxLogToFlush)
            {
                Flush();
            }

            _logs.Add(loggable);
        }

        public void AddLog(IEnumerable<ILoggable> logs)
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
            _logger.InsertLogs(_logs);
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

        ~LoggableManager()
        {
            Dispose(false);
        }

        #endregion


        #region Singleton

        private static LoggableManager _instance;

        public static LoggableManager Instance => _instance ?? (_instance = new LoggableManager());

        #endregion
    }
}