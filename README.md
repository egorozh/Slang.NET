# Slang.NET

[![Nuget](https://img.shields.io/nuget/v/Slang.Net?label=Slang.Net)](https://www.nuget.org/packages/Slang.Net)

Type-safe i18n for .NET

This is a port of the [slang for dart](https://pub.dev/packages/slang)

## Getting Started:

Install the library as a NuGet package:

```powershell
Install-Package dotnet add package Slang.Net
```

### Add json files:

i18n/strings_en.i18n.json

```json
{
  "screen": {
    "locale1": "Locale 1"
  }
}
```

i18n/strings_ru.i18n.json

```json
{
  "screen": {
    "locale1": "Локаль 1"
  }
}
```
### Add partial class:

``` csharp
[Translations(
    BaseLocale = "en",
    InputFileName = "strings",
    InputDirectory = "i18n",
    InputFilePattern = "*.i18n.json")]
public partial class Strings;
```

### Done! 

```csharp
Strings.SetCulture(new CultureInfo("ru-RU")); 

Console.WriteLine(Strings.Translations.Screen.Locale1); // Локаль 1

Strings.SetCulture(new CultureInfo("en-US"));

Console.WriteLine(Strings.Translations.Screen.Locale1); // Locale 1
```

## Features

- String Interpolation
- Pluralization
- Linked Translations
- Maps
- Lists
- Typed Parameters
- Comments

## Roadmap

- Gpt translator CLI
- String Formatting (double, dates and etc)
- Performance improvements
