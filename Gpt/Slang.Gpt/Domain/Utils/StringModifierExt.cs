namespace Slang.Gpt.Domain.Utils;

public static class StringModifierExt
{
    /// Returns the key without modifiers.
    public static string WithoutModifiers(this string key)
    {
        int index = key.IndexOf('(');

        if (index == -1)
            return key;

        return key[..index];
    }
}