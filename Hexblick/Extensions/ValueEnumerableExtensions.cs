using System;

#pragma warning disable IDE0130

namespace ZLinq;

internal static class ValueEnumerableExtensions
{
    public static TSource? FirstOrNull<TEnumerator, TSource>(
        this ValueEnumerable<TEnumerator, TSource> source,
        Func<TSource, bool> predicate)
        where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
        where TSource : struct
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var enumerator = source.Enumerator;

        if (enumerator.TryGetSpan(out var span))
        {
            foreach (var item in span)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
        }

        while (enumerator.TryGetNext(out var item))
        {
            if (predicate(item))
            {
                return item;
            }
        }

        return null;
    }
}
