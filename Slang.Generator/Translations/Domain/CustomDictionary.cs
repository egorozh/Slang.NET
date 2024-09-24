using System.Collections;

namespace Slang.Generator.Translations.Domain;

public class CustomDictionary<TKey, TValue>(Dictionary<TKey, TValue?> translations)
    : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
    where TValue : class
{
    private readonly Dictionary<TKey, TValue?> _dictionary = new(translations);

    public TValue? this[TKey key]
    {
        get
        {
            if (!_dictionary.ContainsKey(key))
                return null;

            return _dictionary[key];
        }
        set => _dictionary[key] = value;
    }

    public Dictionary<TKey, TValue?>.KeyCollection Keys => _dictionary.Keys;

    public void Add(TKey key, TValue item)
    {
        _dictionary.Add(key, item);
    }

    // // Дополнительные методы и свойства при необходимости
    // public void Add(TKey key, TValue value)
    // {
    //     _dictionary.Add(key, value);
    // }
    //
    // public bool Remove(TKey key)
    // {
    //     return _dictionary.Remove(key);
    // }
    //
    // public int Count => _dictionary.Count;
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
}