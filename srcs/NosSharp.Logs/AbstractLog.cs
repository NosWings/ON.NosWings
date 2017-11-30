using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.Logs
{
    public abstract class AbstractLog
    {
        protected AbstractLog()
        {
            Date = DateTime.Now;
        }

        public DateTime Date { get; }

        public abstract string Collection { get; }
    }
}