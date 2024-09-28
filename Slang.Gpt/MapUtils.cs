namespace Slang.Gpt;

public static class MapUtils
{
    /// <summary>
    /// Removes all keys from [target] that also exist in [other].
    /// </summary>
    public static Dictionary<string, object> Subtract(
        Dictionary<string, object?> target,
        Dictionary<string, object?> other
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
                        other: other[otherKey] as Dictionary<string, object?>
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
}