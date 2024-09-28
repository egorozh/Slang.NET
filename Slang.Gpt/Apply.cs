using System.Collections;
using Slang.Generator;

namespace Slang.Gpt;

public static class Apply
{
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

            if (actualValue is IDictionary)
            {
                actualValue = ApplyMapRecursive(
                    path: currPath,
                    baseMap: baseMap[key] as Dictionary<string, object?> ??
                             throw new Exception($"In the base translations, \"{key}\" is not a map."),
                    newMap: newEntry as Dictionary<string, object?> ?? [],
                    oldMap: oldMap[key] as Dictionary<string, object?> ?? [],
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

            object? newEntry = newMap[key];
            object? actualValue = newEntry ?? oldMap[key];

            if (actualValue is IDictionary)
            {
                actualValue = ApplyMapRecursive(
                    baseMap: [],
                    newMap: newEntry as Dictionary<string, object?> ?? [],
                    oldMap: oldMap[key] as Dictionary<string, object?> ?? [],
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

            if (actualValue is IDictionary)
            {
                actualValue = ApplyMapRecursive(
                    baseMap: [],
                    newMap: entry.Value as Dictionary<string, object?> ?? [],
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
        if (value is IDictionary)
            return;

        Console.WriteLine($"    -> Set [{path}]: \"{value}\"");
    }
}