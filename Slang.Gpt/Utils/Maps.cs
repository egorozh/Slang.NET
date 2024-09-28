using Slang.Generator.Domain.Nodes.Utils;

namespace Slang.Gpt.Utils;

public static class Maps
{
    private const string IgnoreGpt = "ignoreGpt";

    /// <summary>
    /// Remove all entries from [map] that have the "ignoreGpt" modifier.
    /// This method removes the entries in-place.
    /// </summary>
    public static void RemoveIgnoreGpt(Dictionary<string, object> map)
    {
        List<string> keysToRemove = [];

        foreach (var entry in map)
        {
            if (NodeUtils.ParseModifiers(entry.Key).Modifiers.ContainsKey(IgnoreGpt))
            {
                keysToRemove.Add(entry.Key);
            }
            else if (entry.Value is Dictionary<string, object> dictionary)
            {
                RemoveIgnoreGpt(dictionary);
            }
        }

        foreach (string key in keysToRemove)
            map.Remove(key);
    }

    /// Remove all entries from [map] that are comments.
    /// This method removes the entries in-place.
    /// A new map is returned containing the comments.
    public static Dictionary<string, object> extractComments(
        Dictionary<string, object> map,
        bool remove
    )
    {
        Dictionary<string, object> comments = [];

        List<string> keysToRemove = [];

        foreach (var entry in map)
        {
            if (entry.Key.StartsWith('@'))
            {
                comments[entry.Key] = entry.Value;

                if (remove)
                    keysToRemove.Add(entry.Key);
            }
            else if (entry.Value is Dictionary<string, object> dictionary)
            {
                var childComments = extractComments(map: dictionary, remove: remove);

                if (childComments.Count > 0)
                    comments[entry.Key] = childComments;

                if (remove && dictionary.Count == 0)
                    keysToRemove.Add(entry.Key);
            }
        }

        if (remove)
        {
            foreach (string key in keysToRemove)
                map.Remove(key);
        }

        return comments;
    }
}