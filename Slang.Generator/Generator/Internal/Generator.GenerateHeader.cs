using System.Text;
using Slang.Generator.Generator.Entities;
using Slang.Generator.NodesData;
using static Slang.Generator.Generator.Helper;

namespace Slang.Generator.Generator;

internal static partial class Generator
{
    private static string GenerateHeader(GenerateConfig config, List<I18NData> allLocales)
    {
        var now = DateTime.Now;

        return
            $$"""
              /// Generated file. Do not edit.
              ///
              ///
              /// Locales: {{allLocales.Count}}
              ///
              /// Built on {{now.ToShortDateString()}} at {{now.ToShortTimeString()}} UTC

              using Slang;
              using {{config.Namespace}};
              using System;
              using System.Collections.Generic;
              using System.Globalization;
              using System.Linq;
              using System.Threading;

              namespace {{config.Namespace}}
              {
              	partial class {{config.ClassName}}
              	{
              {{GenerateCultures(config, allLocales)}}
              
                      private readonly static CultureInfo _baseCulture = _{{config.BaseLocale.TwoLetterISOLanguageName}};
              
              		public static IReadOnlyList<CultureInfo> SupportedCultures => _translations.Keys.ToList();
              
              		public static Strings Translations
              		{
              			get
              			{
              				var culture = CultureInfo.CurrentUICulture;
              
              				if (_translations.TryGetValue(culture, out var translation))
              					return translation;
              					
                            var sameCulture = SupportedCultures.FirstOrDefault(c => c.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName);
              
                            if (sameCulture != null)
                                return _translations[sameCulture];
                                
              				return _translations[_baseCulture];
              			}
              		}
              
              		public static void SetCulture(CultureInfo culture, bool uiOnly = false)
              		{
              			if (!uiOnly)
              				Thread.CurrentThread.CurrentCulture = culture;
              
              			Thread.CurrentThread.CurrentUICulture = culture;
              		}
              	}
              }
              """;
    }

    private static string GenerateCultures(GenerateConfig config, List<I18NData> allLocales)
    {
        StringBuilder buffer = new();

        for (int i = 0; i < allLocales.Count; i++)
        {
            var locale = allLocales[i].Locale;

            buffer.AppendLineWithTab(
                $"private readonly static CultureInfo _{locale.TwoLetterISOLanguageName} = new CultureInfo(\"{locale.Name}\");",
                tabCount: 2);
        }

        buffer.AppendLine();

        buffer.AppendLine(
            """
                    private readonly static IReadOnlyDictionary<CultureInfo, Strings> _translations =
                        new Dictionary<CultureInfo, Strings>()
                        {
            """);

        for (int i = 0; i < allLocales.Count; i++)
        {
            var locale = allLocales[i].Locale;

            string className = allLocales[i].BaseLocale
                ? config.ClassName
                : GetClassNameRoot(baseName: config.BaseName, locale: locale);

            buffer.AppendWithTab(
                $"{{_{locale.TwoLetterISOLanguageName}, new {className}() }}", tabCount: 4);

            if (i < allLocales.Count - 1)
                buffer.Append(',');

            buffer.AppendLine();
        }


        buffer.AppendLine("            };");

        return buffer.ToString();
    }
}