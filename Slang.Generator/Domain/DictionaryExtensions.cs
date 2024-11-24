namespace Slang.Generator.Domain;

public static class DictionaryExtensions
{
    public static void AddAll<T, TKey>(this Dictionary<T, TKey> dictionary, Dictionary<T, TKey> addedDictionary) where T : notnull
    {
        foreach (var addedKeyValue in addedDictionary)
        {
            if (!dictionary.ContainsKey(addedKeyValue.Key))
                dictionary.Add(addedKeyValue.Key, addedKeyValue.Value);
        }
    }

    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> addedDictionary)
    {
        foreach (var addedKeyValue in addedDictionary)
            set.Add(addedKeyValue);
    }
}