using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LogDic = System.Collections.Generic.Dictionary<NosSharp.WatchDog.Utils.WatchDog.LogType, string>;

namespace NosSharp.WatchDog.Utils
{
    public class WatchDog : IWatchDog
    {
        internal enum LogType
        {
            Message,
            StackTrace
        };


        private readonly Dictionary<long, Process> _processes;
        private readonly Dictionary<long, List<LogDic>> _logs;

        public WatchDog()
        {
            _processes = new Dictionary<long, Process>();
            _logs = new Dictionary<long, List<LogDic>>();
        }

        public void RegisterNewProcess(Process process)
        {
            _processes.Add(process.Id, process);
        }

        public void RemoveProcessById(long id)
        {
            if (_processes.ContainsKey(id))
            {
                _processes.Remove(id);
            }
        }

        public void AddCrashLog(long id, Exception e)
        {
            LogDic log = new LogDic { { LogType.Message, e.Message }, { LogType.StackTrace, e.StackTrace } };

            if (!_logs.ContainsKey(id))
            {
                _logs.Add(id, new List<LogDic>());
            }
            _logs[id].Add(log);
        }

        public void Restart(long id)
        {
            if (!_processes.TryGetValue(id, out Process proc))
            {
                return;
            }
            proc?.Kill();
            proc?.Start();
        }

        public void Restart(Type type)
        {
            Process proc = _processes.Values.FirstOrDefault(s => s.GetType() == type);
            proc?.Kill();
            proc?.Start();
        }

        public void UpdateStatus()
        {
            foreach (Process processesValue in _processes.Values)
            {
                if (processesValue.HasExited)
                {
                    _processes.Remove(processesValue.Id);
                }
            }
        }
    }
}