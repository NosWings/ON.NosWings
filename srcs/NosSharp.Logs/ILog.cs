using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.Logs
{
    public interface ILog
    {
        DateTime Date { get; }
        string Collection { get; }
    }
}