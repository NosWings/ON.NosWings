using System;

namespace NosSharp.Logs
{
    public abstract class AbstractLog : ILog
    {
        protected AbstractLog(string collection)
        {
            Collection = collection;
            Date = DateTime.Now;
        }

        /// <summary>
        /// The date when the log has been created
        /// </summary>
        public DateTime Date { get; }


        /// <summary>
        /// The collection where the log need to be stored
        /// </summary>
        public string Collection { get; }
    }
}