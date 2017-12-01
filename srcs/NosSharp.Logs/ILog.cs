using System;

namespace NosSharp.Logs
{
    public interface ILog
    {
        DateTime Date { get; }
        string Collection { get; }
    }
}