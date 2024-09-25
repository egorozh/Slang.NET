using System.Collections;

namespace Slang.Generator.Nodes.Utils;

internal interface IRawProvider
{
    bool TryGetString(object? input, out string value);

    bool TryGetArray(object? input, out IList value);

    Dictionary<string, object?>? GetDictionary(object? value);
}