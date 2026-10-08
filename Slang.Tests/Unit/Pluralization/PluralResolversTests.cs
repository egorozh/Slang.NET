namespace Slang.Tests.Unit.Pluralization;

public class PluralResolversTests
{
    private static string Resolve(string language, int n) =>
        PluralResolvers.Cardinal(language)(n, one: "one", two: "two", few: "few", other: "other");

    [TestCase("sr")]
    [TestCase("hr")]
    [TestCase("bs")]
    public void South_Slavic(string language)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Resolve(language, 1), Is.EqualTo("one"));
            Assert.That(Resolve(language, 21), Is.EqualTo("one"));
            Assert.That(Resolve(language, 2), Is.EqualTo("few"));
            Assert.That(Resolve(language, 24), Is.EqualTo("few"));
            Assert.That(Resolve(language, 5), Is.EqualTo("other"));
            Assert.That(Resolve(language, 11), Is.EqualTo("other"));
            Assert.That(Resolve(language, 12), Is.EqualTo("other"));
            Assert.That(Resolve(language, 0), Is.EqualTo("other"));
        });
    }

    [Test]
    public void Slovenian_Has_A_Dual()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Resolve("sl", 1), Is.EqualTo("one"));
            Assert.That(Resolve("sl", 101), Is.EqualTo("one"));
            Assert.That(Resolve("sl", 2), Is.EqualTo("two"));
            Assert.That(Resolve("sl", 102), Is.EqualTo("two"));
            Assert.That(Resolve("sl", 3), Is.EqualTo("few"));
            Assert.That(Resolve("sl", 4), Is.EqualTo("few"));
            Assert.That(Resolve("sl", 5), Is.EqualTo("other"));
            Assert.That(Resolve("sl", 11), Is.EqualTo("other"));
        });
    }
}
