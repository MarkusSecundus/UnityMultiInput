using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TextUtils 
{
    public static string MakeString<T>(this IEnumerable<T> self, string separator = ", ", string format=null)
    {
        using var it = self.GetEnumerator();
        if (!it.MoveNext()) return "";
        var ret = new StringBuilder().Append(doFormat(it.Current));
        while (it.MoveNext()) ret.Append(separator).Append(doFormat(it.Current));
        return ret.ToString();

        string doFormat(T item) => format == null ? item.ToString() : ((IFormattable)item).ToString(format, System.Globalization.CultureInfo.CurrentCulture);
    }
}
