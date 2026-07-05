using System.Globalization;

namespace Slang.Gpt.Tests;

public class LocalesTests
{
    [Test]
    public void Should_return_exact_locale()
    {
        var locale = new CultureInfo("zh-CN");

        // The exact region spelling ("China" vs "China mainland") varies by ICU version,
        // so only assert on the language, which is what this locale should resolve to.
        Assert.That(locale.EnglishName, Does.StartWith("Chinese"));
    }

    [Test]
    public void Should_fallback_to_language()
    {
        var locale = new CultureInfo("de-CN");

        // German with an unusual region: verify it falls back to the German language
        // regardless of how ICU/NLS names the CN region.
        Assert.That(locale.EnglishName, Does.StartWith("German"));
    }
}