using System.Collections;
using Slang.Generator;

namespace Slang.Gpt;

public static class MapUtils
{
    // /// converts Map<dynamic, dynamic> to Map<String, dynamic> for all children
    // /// forcing all keys to be strings
    // public static Map<String, dynamic> deepCast(Map<dynamic, dynamic> source) {
    //     return source.map((key, value) {
    //         var dynamic castedValue;
    //         if (value is Map) {
    //             castedValue = deepCast(value);
    //         } else if (value is List) {
    //             castedValue = _deepCastList(value);
    //         } else {
    //             castedValue = value;
    //         }
    //         return MapEntry(key.toString(), castedValue);
    //     });
    // }
    //
    // /// Returns the value at the specified path
    // /// or null if the path does not exist.
    // public static dynamic getValueAtPath({
    //     required Map<String, dynamic> map,
    //     required String path,
    // }) {
    //     var pathList = path.split(".");
    //
    //     Map<String, dynamic> currMap = map;
    //
    //     for (int i = 0; i < pathList.length; i++) {
    //         String currKey = pathList[i];
    //         int currKeyBracketIndex = currKey.indexOf("(");
    //         if (currKeyBracketIndex != -1) {
    //             currKey = currKey.substring(0, currKeyBracketIndex);
    //         }
    //
    //         Object? currValue;
    //         for (var entry in currMap.entries) {
    //             var key = entry.key;
    //             var bracketIndex = key.indexOf("(");
    //             if (bracketIndex != -1) {
    //                 if (key.substring(0, bracketIndex) == currKey) {
    //                     currValue = entry.value;
    //                     break;
    //                 }
    //             } else if (key == currKey) {
    //                 currValue = entry.value;
    //                 break;
    //             }
    //         }
    //
    //         if (currValue == null) {
    //             // does not exist
    //             return null;
    //         }
    //
    //         if (i == pathList.length - 1) {
    //             // destination
    //             return currValue;
    //         } else {
    //             if (currValue is! Map<String, dynamic>) {
    //                 // The leaf cannot be updated because "currEntry" is not a map.
    //                 return false;
    //             }
    //             currMap = currValue;
    //         }
    //     }
    //
    //     // This should never be reached.
    //     return null;
    // }
    //
    /// Adds a leaf to the map at the specified path
    public static void addItemToMap(
        Dictionary<string, dynamic> map,
        string destinationPath,
        object item)
    {
        string[] pathList = destinationPath.Split(".");

        // starts with type Map<String, dynamic> but
        // may be a Map<String, dynamic> or List<dynamic> after the 1st iteration
        object curr = map;

        for (int i = 0; i < pathList.Length; i++)
        {
            string subPath = pathList[i];

            int? subPathInt = int.TryParse(subPath, out int subPathInt2) ? subPathInt2 : null;

            string? nextSubPath = i + 1 < pathList.Length ? pathList[i + 1] : null;
            bool nextIsList = nextSubPath != null && int.TryParse(nextSubPath, out int _);

            if (i == pathList.Length - 1)
            {
                // destination
                if (subPathInt.HasValue)
                {
                    if (curr is not IList list)
                    {
                        throw new Exception(
                            "The leaf \"$destinationPath\" cannot be added because the parent of \"$subPathInt\" is not a list.");
                    }

                    bool added = addToList(
                        list: list,
                        index: subPathInt.Value,
                        element: item,
                        overwrite: true
                    );

                    if (!added)
                    {
                        throw new Exception(
                            "The leaf \"$destinationPath\" cannot be added because there are missing indices.");
                    }
                }
                else
                {
                    if (curr is not IDictionary dictionary)
                    {
                        throw new Exception(
                            "The leaf \"$destinationPath\" cannot be added because the parent of \"$subPath\" is not a map.");
                    }

                    dictionary[subPath] = item;
                }
            }
            else
            {
                // make sure that the path to the leaf exists
                if (subPathInt != null)
                {
                    // list mode
                    if (curr is not IList list)
                    {
                        throw new Exception(
                            "The leaf \"$destinationPath\" cannot be added because the parent of \"$subPathInt\" is not a list.");
                    }

                    bool added = addToList(
                        list: list,
                        index: subPathInt.Value,
                        element: nextIsList
                            ? new List<object>()
                            : new Dictionary<string, object>(),
                        overwrite: false
                    );

                    if (!added)
                    {
                        throw new Exception(
                            "The leaf \"$destinationPath\" cannot be added because there are missing indices.");
                    }

                    curr = list[subPathInt.Value];
                }
                else
                {
                    // map mode
                    if (curr is not IDictionary dictionary)
                    {
                        throw new Exception(
                            "The leaf \"$destinationPath\" cannot be added because the parent of \"$subPath\" is not a map.");
                    }

                    if (!dictionary.Contains(subPath))
                    {
                        // path touches first time the tree, make sure the path exists
                        // but do not overwrite,
                        // so previous [addStringToMap] calls get not lost
                        dictionary[subPath] = nextIsList ? new List<object>() : new Dictionary<string, object>();
                    }

                    curr = dictionary[subPath];
                }
            }
        }
    }

    /// Adds an element to the list
    /// Adding must be in the correct order, so if the list is too small, then it won"t be added
    /// Returns true, if the element was added
    public static bool addToList(
        IList list,
        int index,
        object element,
        bool overwrite)
    {
        if (index <= list.Count)
        {
            if (index == list.Count)
                list.Add(element);
            else if (overwrite)
                list[index] = element;

            return true;
        }

        return false;
    }
    //
    // /// Updates an existing entry at [path].
    // /// Modifiers are ignored and should be not included in the [path].
    // ///
    // /// The [update] function uses the key and value of the entry
    // /// and returns the result to update the entry.
    // ///
    // /// It updates the entry in place.
    // /// Returns true, if the entry was updated.
    // public static bool updateEntry({
    //     required Map<String, dynamic> map,
    //     required String path,
    //         required MapEntry<String, dynamic> Function(String key, Object path) update,
    // }) {
    //     var pathList = path.split(".");
    //
    //     Map<String, dynamic> currMap = map;
    //
    //     for (int i = 0; i < pathList.length; i++) {
    //         var subPath = pathList[i];
    //         var entryList = currMap.entries.toList();
    //         var entryIndex = entryList.indexWhere((entry) {
    //             var key = entry.key;
    //             if (key.contains("(")) {
    //                 return key.substring(0, key.indexOf("(")) == subPath;
    //             }
    //             return key == subPath;
    //         });
    //         if (entryIndex == -1) {
    //             // The leaf cannot be updated because it does not exist.
    //             return false;
    //         }
    //         var MapEntry<String, dynamic> currEntry = entryList[entryIndex];
    //
    //         if (i == pathList.length - 1) {
    //             // destination
    //             var updated = update(currEntry.key, currEntry.value);
    //
    //             if (currEntry.key == updated.key) {
    //                 // key did not change
    //                 currMap[currEntry.key] = updated.value;
    //             } else {
    //                 // key changed, we need to reconstruct the map to keep the order
    //                 currMap.clear();
    //                 currMap.addEntries(entryList.take(entryIndex));
    //                 currMap[updated.key] = updated.value;
    //                 currMap.addEntries(entryList.skip(entryIndex + 1));
    //             }
    //
    //             return true;
    //         } else {
    //             if (currEntry.value is! Map<String, dynamic>) {
    //                 // The leaf cannot be updated because "subPath" is not a map.
    //                 return false;
    //             }
    //             currMap = currEntry.value;
    //         }
    //     }
    //
    //     // This should never be reached.
    //     return false;
    // }
    //
    // /// Deletes an existing entry at [path].
    // /// Returns true, if the entry was deleted.
    // static bool deleteEntry({
    //     required Map<String, dynamic> map,
    //     required String path,
    // }) {
    //     var pathList = path.split(".");
    //
    //     Map<String, dynamic> currMap = map;
    //
    //     for (int i = 0; i < pathList.length; i++) {
    //         String currKey = pathList[i];
    //         int currKeyBracketIndex = currKey.indexOf("(");
    //         if (currKeyBracketIndex != -1) {
    //             currKey = currKey.substring(0, currKeyBracketIndex);
    //         }
    //
    //         MapEntry<String, dynamic>? currEntry;
    //         for (var entry in currMap.entries) {
    //             var key = entry.key;
    //             var bracketIndex = key.indexOf("(");
    //             if (bracketIndex != -1) {
    //                 if (key.substring(0, bracketIndex) == currKey) {
    //                     currEntry = entry;
    //                     break;
    //                 }
    //             } else if (key == currKey) {
    //                 currEntry = entry;
    //                 break;
    //             }
    //         }
    //
    //         if (currEntry == null) {
    //             // The leaf cannot be deleted because it does not exist.
    //             return false;
    //         }
    //
    //         if (i == pathList.length - 1) {
    //             // destination
    //             currMap.remove(currEntry.key);
    //             return true;
    //         } else {
    //             if (currEntry.value is! Map<String, dynamic>) {
    //                 // The leaf cannot be updated because "currEntry" is not a map.
    //                 return false;
    //             }
    //             currMap = currEntry.value;
    //         }
    //     }
    //
    //     // This should never be reached.
    //     return false;
    // }

    /// <summary>
    /// Removes all keys from [target] that also exist in [other].
    /// </summary>
    public static Dictionary<string, object> Subtract(
        Dictionary<string, object> target,
        Dictionary<string, object> other
    )
    {
        Dictionary<string, object> resultMap = [];

        foreach (var entry in target)
        {
            string? keyWithoutModifier = entry.Key.WithoutModifiers();

            if (entry.Value is not Dictionary<string, object> dictionary)
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
                    var subtracted = Subtract(
                        target: dictionary,
                        other: other[otherKey] as Dictionary<string, object>
                    );
                    if (subtracted.Count > 0)
                    {
                        resultMap[entry.Key] = subtracted;
                    }
                }
            }
        }

        return resultMap;
    }

    // /// Returns a list of all keys in the map.
    // /// A key is a path to a leaf (e.g. "a.b.c")
    // static List<String> getFlatMap(Map<String, dynamic> map) {
    //     var resultMap = <String>[];
    //     for (var entry in map.entries) {
    //         if (entry.value is Map) {
    //             // recursive
    //             var subMap = getFlatMap(entry.value);
    //             for (var subEntry in subMap) {
    //                 resultMap.add("${entry.key}.$subEntry");
    //             }
    //         } else {
    //             resultMap.add(entry.key);
    //         }
    //     }
    //     return resultMap;
    // }
    //
    // /// Removes all entries that are empty maps recursively.
    // /// They are removed from the map in place.
    // static void clearEmptyMaps(Map map) {
    //     for (var key in [...map.keys]) {
    //         var value = map[key];
    //         if (value is Map) {
    //             clearEmptyMaps(value);
    //             if (value.isEmpty) {
    //                 map.remove(key);
    //             }
    //         }
    //     }
    // }
}