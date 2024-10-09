# Slang.NET

[![Nuget](https://img.shields.io/nuget/v/Slang.Net?label=Slang.Net)](https://www.nuget.org/packages/Slang.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Type-safe i18n for .NET

## About this library

Slang.NET is a .NET port of the [slang](https://pub.dev/packages/slang) from the Dart/Flutter community with new features (like string format).

You can view how the generated files [general](https://github.com/egorozh/Slang.NET/blob/develop/Slang.Tests/Integration/Resources/_expected_header.output), [en](https://github.com/egorozh/Slang.NET/blob/develop/Slang.Tests/Integration/Resources/_expected_en.output), and [de](https://github.com/egorozh/Slang.NET/blob/develop/Slang.Tests/Integration/Resources/_expected_de.output) look

## Getting Started:

Install the library as a NuGet package:

```powershell
Install-Package dotnet add package Slang.Net
```

### Add JSON files:
> **Important** file must end with ".i18n.json". This is necessary so that the SourceGenerator does not track changes to other AdditionalFiles.

`i18n/strings_en.i18n.json` or `i18n/strings_en-US.i18n.json` or `i18n/strings.i18n.json` (for base culture)

```json
{
  "screen": {
    "locale1": "Locale 1"
  }
}
```

`i18n/strings_ru.i18n.json` or `i18n/strings_ru-RU.i18n.json`

```json
{
  "screen": {
    "locale1": "Локаль 1"
  }
}
```

`slang.json`

```json
{
  "base_culture": "en" // or "en-EN"
}
```

> **Recommendation** It is recommended to specify the country code, such as "en-US," for proper functionality when formatting strings, especially if you will be retrieving the list of cultures from SupportedCultures.

### Include JSON files as AdditionalFiles:

```xml
  <ItemGroup>
    <AdditionalFiles Include="i18n\*.i18n.json" />
    <AdditionalFiles Include="slang.json" />
  </ItemGroup>
```


### Add a partial class:

``` csharp
[Translations(InputFileName = "strings")]
public partial class Strings;
```

### Done! 

```csharp
Strings.SetCulture(new CultureInfo("ru-RU")); 

Console.WriteLine(Strings.Instance.Root.Screen.Locale1); // Локаль 1

Strings.SetCulture(new CultureInfo("en-US"));

Console.WriteLine(Strings.Instance.Root.Screen.Locale1); // Locale 1
```
or 
```xaml
  <MenuItem  Header="{Binding Root.Screen.Locale1, Source={x:Static localization:Strings.Instance}}" />
```

## Features

- [String Interpolation](#string-interpolation)
- [Typed Parameters](#typed-parameters)
- [Comments](#comments)
- [String Format](#string-format)
- [Pluralization](#pluralization)
- [Linked Translations](#main-features)
- [Maps](#main-features)
- [Lists](#main-features)

### String Interpolation

You can specify parameters passed at runtime.. 

```json
"Hello": "Hello {name}"
```

The generated code will look like this:

```csharp
/// In en, this message translates to:
/// **"Hello {name}"**
public virtual string Hello(object name) => $"Hello {name}";
```

### Typed Parameters

### Comments

### String Format

### Pluralization

## Tools

### Translate with GPT

Take advantage of GPT to internationalize your app with context-aware translations.

Download slang-gpt.

Then add the following configuration in your slang.json:

```json
{
  "base_culture": "ru",
  "gpt": {
    "model": "gpt-4o-mini",
    "description": "Showcase for Slang.Net.Gpt"
  }
}
```

Then use slang-gpt:

```bash
<cli-directory>/slang-gpt <csproj-path> --target=ru --api-key=<api-key>
```

See more: [Documentation](https://github.com/egorozh/Slang.NET/tree/develop/Slang.Gpt.Cli)
