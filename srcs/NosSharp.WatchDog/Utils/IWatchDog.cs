using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
