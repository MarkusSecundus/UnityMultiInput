using System;
using System.Collections.Generic;


namespace MarkusSecundus.Utils
{
    internal static class CollectionUtils
    {
        public static IEnumerable<int> RangeFromToExclusive(int begin, int end, int increment = 1)
        {
            for (int t = begin; t < end; t += increment)
                yield return t;
        }

        public static (T Value, int Index)? FirstIndexed<T>(this IEnumerable<T> self, Func<T, bool> predicate)
        {
            int t = 0;
            foreach (var item in self)
            {
                if (predicate(item)) return (item, t);
                ++t;
            }
            return null;
        }
    }
}