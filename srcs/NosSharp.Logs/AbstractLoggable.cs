using System;

namespace NosSharp.Logs
{
    public abstract class AbstractLoggable : ILoggable
    {
        protected AbstractLoggable(string collection)
        {
            Collection = collection;
            Date = DateTime.Now;
        }

        /// <summary>
        /// The date when the loggable has been created
        /// </summary>
        public DateTime Date { get; }


        /// <summary>
        /// The collection where the loggable need to be stored
        /// </summary>
        public string Collection { get; }
    }
}