using System;
using System.Collections.Concurrent;
using System.Linq;

namespace OpenNos.Core.Extensions
{
    public static class ConcurrentBagExtensions
    {
        #region Methods

        public static void Clear<T>(this ConcurrentBag<T> queue)
        {
            while (queue.TryTake(out T _))
            {
                // NOTHING
            }
        }

        public static ConcurrentBag<T> Where<T>(this ConcurrentBag<T> queue, Func<T, bool> predicate)
        {
            return new ConcurrentBag<T>(queue.ToList().Where(predicate));
        }

        #endregion
    }
}