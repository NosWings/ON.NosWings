using System;
using System.Diagnostics;

namespace NosSharp.WatchDog.Utils
{
    interface IWatchDog
    {
        void RegisterNewProcess(Process process);

        void RemoveProcessById(long id);
        void Restart(long id);

        void Restart(Type type);
    }
}
