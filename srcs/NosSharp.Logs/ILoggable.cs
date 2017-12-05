using System;

namespace NosSharp.Logs
{
    public interface ILoggable
    {
        DateTime Date { get; }
        string Collection { get; }
    }
}