#if NETSTANDARD2_0
// Polyfills for BCL APIs missing on netstandard2.0.
// Declared in System.* namespaces so call sites compile unchanged.

namespace System
{
    public static class SlangStringPolyfills
    {
        public static bool StartsWith(this string s, char c) => s.Length > 0 && s[0] == c;

        public static bool EndsWith(this string s, char c) => s.Length > 0 && s[s.Length - 1] == c;
    }
}

namespace System.Collections.Generic
{
    public static class SlangCollectionPolyfills
    {
        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> pair,
            out TKey key,
            out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class SlangLinqPolyfills
    {
        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) => new(source);
    }
}
#endif
