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
            while (!queue.IsEmpty)
            {
                queue.TryTake(out T _);
            }
        }

        public static void RemoveWhere<T>(this ConcurrentBag<T> queue, Func<T, bool> predicate, out ConcurrentBag<T> queueReturned)
        {
            queueReturned = new ConcurrentBag<T>(queue.Where(predicate));
        }

        private static Func<T, bool> Not<T>(this Func<T, bool> predicate)
        {
            return value => !predicate(value);
        }

        #endregion
    }
}