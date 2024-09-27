namespace Slang.Generator.Domain.Nodes.Utils;

internal interface IRawProvider
{
    bool TryGetString(object? input, out string value);

    bool TryGetArray(object? input, out IEnumerable<object> value);

    Dictionary<string, object?>? GetDictionary(object? value);
}