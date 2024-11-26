using System.Globalization;
using Slang.Generator.Core.Data;

namespace Slang.Tests.Unit.Utils;

public class FileTests
{
    [Test]
    public void GetBaseTranslation()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.Locale, Is.EqualTo(new CultureInfo("en")));
    }


    [Test]
    public void GetEnTranslationWithoutCountry()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1_en.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.Locale, Is.EqualTo(new CultureInfo("en")));
    }

    [Test]
    public void GetRuTranslationWithoutCountry()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1_ru.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.Locale, Is.EqualTo(new CultureInfo("ru")));
    }

    [Test]
    public void GetEnTranslationWithCountry()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1_en-US.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.Locale, Is.EqualTo(new CultureInfo("en-US")));
    }

    [Test]
    public void GetRuTranslationWithCountry()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1_ru-RU.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.Locale, Is.EqualTo(new CultureInfo("ru-RU")));
    }
}