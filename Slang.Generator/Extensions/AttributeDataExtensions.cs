using Microsoft.CodeAnalysis;

namespace Slang.Generator.Extensions;

internal static class AttributeDataExtensions
{
    public static T? GetNamedArgument<T>(this AttributeData attributeData, string name, T? fallback = default)
    {
        if (attributeData.TryGetNamedArgument(name, out T? value))
            return value;

        return fallback;
    }


    private static bool TryGetNamedArgument<T>(this AttributeData attributeData, string name, out T? value)
    {
        foreach (KeyValuePair<string, TypedConstant> properties in attributeData.NamedArguments)
        {
            if (properties.Key == name)
            {
                value = (T?)properties.Value.Value;

                return true;
            }
        }

        value = default;

        return false;
    }
}
