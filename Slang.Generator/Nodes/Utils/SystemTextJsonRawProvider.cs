using System.Collections;
using System.Text.Json;

namespace Slang.Generator.Nodes.Utils;

internal class SystemTextJsonRawProvider : IRawProvider
{
    public bool TryGetString(object? input, out string value)
    {
        if (input is string or int or JsonElement {ValueKind: JsonValueKind.String})
        {
            value = input.ToString()!;
            return true;
        }

        value = null!;

        return false;
    }

    public bool TryGetArray(object? input, out IEnumerable<object> value)
    {
        if (input is IList or JsonElement {ValueKind: JsonValueKind.Array})
        {
            if (input is IList list)
                value = list.Cast<object>();
            else
                value = ((JsonElement) input).EnumerateArray().Cast<object>();

            return true;
        }

        value = null!;

        return false;
    }

    public Dictionary<string, object?>? GetDictionary(object? value)
    {
        Dictionary<string, object?> dict;

        switch (value)
        {
            case Dictionary<string, object?> dictionary:
                dict = dictionary;
                break;
            case JsonElement {ValueKind: JsonValueKind.Object} jsonElement:
                dict = [];

                foreach (var property in jsonElement.EnumerateObject())
                    dict.Add(property.Name, property.Value);

                break;
            default:
                return null;
        }

        return dict;
    }
}