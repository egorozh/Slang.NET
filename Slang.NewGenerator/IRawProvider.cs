namespace Slang.Shared;

public interface IRawProvider
{
    bool TryGetString(object? input, out string value);

    bool TryGetArray(object? input, out IEnumerable<object> value);

    Dictionary<string, object?>? GetDictionary(object? value);
}