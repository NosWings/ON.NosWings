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
            return new ConcurrentBag<T>(queue.AsEnumerable().Where(predicate));
        }

        #endregion
    }
}