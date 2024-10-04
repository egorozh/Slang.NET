using System.Globalization;

namespace Slang.Gpt.Tests;

public class LocalesTests
{
    [Test]
    public void Should_return_exact_locale()
    {
        var locale = new CultureInfo("zh-CN");

        Assert.That(locale.EnglishName, Is.EqualTo("Chinese (China mainland)"));
    }
    
    [Test]
    public void Should_fallback_to_language()
    {
        var locale = new CultureInfo("de-CN");
        
        Assert.That(locale.EnglishName, Is.EqualTo("German (China mainland)"));
    }
}