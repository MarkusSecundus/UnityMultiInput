using System.Collections.Generic;

public static class CollectionUtils
{
    public static IEnumerable<int> RangeFromToExclusive(int begin, int end, int increment=1)
    {
        for (int t = begin; t < end; t += increment)
            yield return t;
    }
}