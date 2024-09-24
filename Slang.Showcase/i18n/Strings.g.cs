/// Generated file. Do not edit.
///
///
/// Locales: 2
/// Strings: 284 (142 per locale)///
/// Built on 2024-09-24 at 20:28 UTC


using Slang;
using Slang.Showcase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;


namespace Slang.Showcase
{
	partial class Strings
	{
		private readonly static CultureInfo _en = new CultureInfo("en");
		private readonly static CultureInfo _ru = new CultureInfo("ru");

        private readonly static IReadOnlyDictionary<CultureInfo, Strings> _translations =
            new Dictionary<CultureInfo, Strings>()
            {
				{_en, new Strings() },
				{_ru, new StringsRu() }
            };


        private readonly static CultureInfo _baseCulture = _en;

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