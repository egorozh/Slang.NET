using Slang.Shared;

namespace Slang.Gpt.Domain.Utils;

public static class MapUtils
{
    private static readonly IRawProvider RawProvider = new SystemTextJsonRawProvider();

    /// <summary>
    /// Removes all keys from [target] that also exist in [other].
    /// </summary>
    public static Dictionary<string, object?> Subtract(
        Dictionary<string, object?> target,
        Dictionary<string, object?> other
    )
    {
        Dictionary<string, object> resultMap = [];

        foreach (var entry in target)
        {
            string? keyWithoutModifier = entry.Key.WithoutModifiers();

            var dictionary = RawProvider.GetDictionary(entry.Value);

            if (dictionary is null)
            {
                // Add the entry if the key does not exist in the other map.
                if (other.Keys.FirstOrDefault(k => k.WithoutModifiers() == keyWithoutModifier) == null)
                {
                    resultMap[entry.Key] = entry.Value;
                }
            }
            else
            {
                // Recursively subtract the map.
                string? otherKey = other.Keys.FirstOrDefault(k => k.WithoutModifiers() == keyWithoutModifier);

                if (otherKey == null)
                {
                    resultMap[entry.Key] = entry.Value;
                }
                else
                {
                    var otherDictionary = RawProvider.GetDictionary(other[otherKey]);

                    var subtracted = Subtract(
                        target: dictionary,
                        other: otherDictionary
                    );

                    if (subtracted.Count > 0)
                        resultMap[entry.Key] = subtracted;
                }
            }
        }

        return resultMap;
    }

    /// <summary>
    /// Adds entries from [newMap] to [oldMap] while respecting the order specified
    /// in [baseMap].
    ///
    /// Modifiers of [baseMap] are applied.
    ///
    /// Entries in [oldMap] get removed when they get replaced and have the "OUTDATED" modifier.
    ///
    /// The returned map is a new instance (i.e. no side effects for the given maps)
    /// </summary>
    public static Dictionary<string, object?> ApplyMapRecursive(
        Dictionary<string, object?> baseMap,
        Dictionary<string, object?> newMap,
        Dictionary<string, object?> oldMap,
        bool verbose,
        string? path = null
    )
    {
        Dictionary<string, object?> resultMap = [];

        List<string> resultKeys = []; // keys without modifiers

        // Keys that have been applied.
        // They do not have modifiers in their path.
        HashSet<string> appliedKeys = [];

        // [newMap] but without modifiers
        newMap = newMap
            .Select(kv => new KeyValuePair<string, object?>(kv.Key.WithoutModifiers(), kv.Value))
            .ToDictionary();

        // Add keys according to the order in base map.
        // Prefer new map over old map.
        foreach (string key in baseMap.Keys)
        {
            string keyWithoutModifiers = key.WithoutModifiers();

            object? newEntry = null;

            if (newMap.TryGetValue(keyWithoutModifiers, out object? value))
                newEntry = value;

            object? actualValue = newEntry ?? oldMap.GetValueOrDefault(key);

            if (actualValue == null)
                continue;

            string currPath = path == null ? key : $"{path}.{key}";

            var actualDictionary = RawProvider.GetDictionary(actualValue);

            if (actualDictionary is not null)
            {
                var baseMapDictionary = RawProvider.GetDictionary(baseMap[key]);
                var newMapDictionary = RawProvider.GetDictionary(newEntry);
                var oldMapDictionary = oldMap.ContainsKey(key) ? RawProvider.GetDictionary(oldMap[key]) : null;

                actualValue = ApplyMapRecursive(
                    path: currPath,
                    baseMap: baseMapDictionary ??
                             throw new Exception($"In the base translations, \"{key}\" is not a map."),
                    newMap: newMapDictionary ?? [],
                    oldMap: oldMapDictionary ?? [],
                    verbose: verbose
                );
            }

            if (newEntry != null)
            {
                string[] split = key.Split("(");
                appliedKeys.Add(split.First());

                if (verbose)
                    PrintAdding(currPath, actualValue);
            }

            resultMap[key] = actualValue;
            resultKeys.Add(keyWithoutModifiers);
        }

        // Add keys from old map that are unknown in base locale.
        // It may contain the OUTDATED modifier.
        foreach (string key in oldMap.Keys)
        {
            string keyWithoutModifiers = key.WithoutModifiers();

            if (resultKeys.Contains(keyWithoutModifiers))
                continue;

            string currPath = path == null ? key : $"{path}.{key}";

            object? newEntry = newMap.ContainsKey(key) ? newMap[key] : null;
            object? actualValue = newEntry ?? oldMap[key];

            var actualDictionary = RawProvider.GetDictionary(actualValue);

            if (actualDictionary is not null)
            {
                var newMapDictionary = RawProvider.GetDictionary(newEntry);
                var oldMapDictionary = RawProvider.GetDictionary(oldMap[key]);

                actualValue = ApplyMapRecursive(
                    baseMap: [],
                    newMap: newMapDictionary ?? [],
                    oldMap: oldMapDictionary ?? [],
                    verbose,
                    path: currPath);
            }

            if (verbose && newEntry != null)
            {
                PrintAdding(currPath, actualValue);
            }

            resultMap[key] = actualValue;
            resultKeys.Add(keyWithoutModifiers);
        }

        // Add remaining new keys that are not in base locale and not in old map.
        foreach (var entry in newMap)
        {
            string keyWithoutModifiers = entry.Key.WithoutModifiers();

            if (resultKeys.Contains(keyWithoutModifiers))
                continue;

            string? currPath = path == null ? entry.Key : $"{path}.{entry.Key}";

            object? actualValue = entry.Value;

            var actualDictionary = RawProvider.GetDictionary(actualValue);

            if (actualDictionary is not null)
            {
                var newMapDictionary = RawProvider.GetDictionary(entry.Value);

                actualValue = ApplyMapRecursive(
                    baseMap: [],
                    newMap: newMapDictionary ?? [],
                    oldMap: [],
                    verbose: verbose,
                    path: currPath
                );
            }

            if (verbose)
            {
                PrintAdding(currPath, actualValue);
            }

            resultMap[entry.Key] = actualValue;
        }

        return resultMap;
    }


    private static void PrintAdding(string path, object? value)
    {
        var dictionary = RawProvider.GetDictionary(value);

        if (dictionary is not null)
            return;

        Console.WriteLine($"    -> Set [{path}]: \"{value}\"");
    }
}