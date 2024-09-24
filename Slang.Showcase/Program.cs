// See https://aka.ms/new-console-template for more information

using System;
using System.Globalization;
using Slang.Showcase;

Console.WriteLine("Hello, World!");

Strings.SetCulture(new CultureInfo("ru-RU"));

ShowLocales();

Strings.SetCulture(new CultureInfo("en-US"));

ShowLocales();

void ShowLocales()
{
    Console.WriteLine(Strings.Translations.SearchHandler.SearchDirectoryText("qwerty", "My computer"));
    Console.WriteLine(Strings.Translations.SomeKey.Apple(6));
    Console.WriteLine(Strings.Translations.SomeKey.Introduce("Egor", 29));
    object a = Strings.Translations.SomeKey.NiceList[1]; // "nice"
    object b = Strings.Translations.SomeKey.NiceList[2][0]; // "first item in nested list"
    object c = Strings.Translations.SomeKey.NiceList[3].Ok; // "OK!"
    object d = Strings.Translations.SomeKey.NiceList[4].AMapEntry;
    Console.WriteLine($"{a}, {b}, {c}, {d}");
    
    string aMap = Strings.Translations.SomeKey.A["helloWorld"]; // "hello"
    string bMap = Strings.Translations.SomeKey.B.B0; // "hey"
    string cMap = Strings.Translations.SomeKey.B.B1["hiThere"];

    Console.WriteLine($"{aMap}, {bMap}, {cMap}");
}
