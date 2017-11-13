using System;
using System.Collections.Concurrent;
using System.Linq;

namespace OpenNos.Core
{
    public static class ConcurrentBagExtensions
    {
        #region Methods

        public static void Clear<T>(this ConcurrentBag<T> queue)
        {
            while (queue.TryTake(out T item))
            {
                // NOTHING
            }
        }

        public static ConcurrentBag<T> Where<T>(this ConcurrentBag<T> queue, Func<T, bool> predicate)
        {
            ConcurrentBag<T> temp = new ConcurrentBag<T>();
            foreach (T line in queue.AsEnumerable().Where(predicate))
            {
                temp.Add(line);
            }
            return temp;
        }

        #endregion
    }
}