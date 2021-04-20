using LogManager.Core.Entities;
using LogManager.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogManager.Core.Extensions
{
    public static class CustomLinq
    {
        private static IComparer<Ip> IpAddressComparer = new IpAddressComparer();
        private static IComparer<Ip> IpAddressComparerReverse = new IpAddressComparerReverse();

        public static IEnumerable<T> LogEntitiesOrderBy<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> selector)
        {
            if (true)
            {
                var ips = collection.Select(x => x as Ip).ToArray();
                Array.Sort(ips, IpAddressComparer);
                return ips as IEnumerable<T>;
            }

            return collection.OrderBy(selector);
        }

        public static IEnumerable<T> LogEntitiesOrderByDescending<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> selector)
        {
            if (typeof(T) == typeof(byte[]))
            {
                var ips = collection.Select(x => x as Ip).ToArray();
                Array.Sort(ips, IpAddressComparerReverse);
                return ips as IEnumerable<T>;
            }

            return collection.OrderByDescending(selector);
        }
    }
}
