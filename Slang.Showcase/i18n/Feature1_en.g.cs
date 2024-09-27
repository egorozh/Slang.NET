using Slang;
using Slang.Showcase.MyNamespace;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Slang.Showcase.MyNamespace
{
	partial class Feature1
	{
		protected virtual Feature1 _root { get; } // ignore: unused_field

		public Feature1()
		{
			_root = this;
			Screen = new Feature1ScreenEn(_root);
		}

		// Translations
		public virtual Feature1ScreenEn Screen { get; }

		// Path: Screen
		public class Feature1ScreenEn
		{
			public Feature1ScreenEn(Feature1 root)
			{
				this._root = root;
			}

			protected virtual Feature1 _root { get; } // ignore: unused_field

			// Translations
			public virtual string Locale1 => "Locale 1";
		}

   }
}