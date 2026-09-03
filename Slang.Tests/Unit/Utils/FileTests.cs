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

    [Test]
    public void GetTranslationWithScript()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1_zh-Hant.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.Locale, Is.EqualTo(new CultureInfo("zh-Hant")));
    }

    [Test]
    public void GetTranslationWithScriptAndCountry()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1_zh-Hant-TW.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.Locale, Is.EqualTo(new CultureInfo("zh-Hant-TW")));
    }

    [Test]
    public void GetTranslationsDifferingOnlyByScript()
    {
        var cyrillic = FilesRepository.GetTranslationFile(
            new CultureInfo("ru-RU"), "strings_sr-Cyrl-RS.i18n.json", contentFactory: () => Task.FromResult(""));

        var latin = FilesRepository.GetTranslationFile(
            new CultureInfo("ru-RU"), "strings_sr-Latn-RS.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.Multiple(() =>
        {
            Assert.That(cyrillic!.Value.Locale, Is.EqualTo(new CultureInfo("sr-Cyrl-RS")));
            Assert.That(latin!.Value.Locale, Is.EqualTo(new CultureInfo("sr-Latn-RS")));
            Assert.That(latin.Value.Locale, Is.Not.EqualTo(cyrillic.Value.Locale));
        });
    }

    [Test]
    public void ScriptIsNormalizedToTitleCase()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "strings_uz-latn.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That($"{file!.Value.Locale}", Is.EqualTo("uz-Latn"));
    }

    [Test]
    public void FileNameIsKept()
    {
        var file = FilesRepository.GetTranslationFile(
            new CultureInfo("en"), "feature1_en-US.i18n.json", contentFactory: () => Task.FromResult(""));

        Assert.That(file!.Value.FileName, Is.EqualTo("feature1_en-US.i18n.json"));
    }
}
