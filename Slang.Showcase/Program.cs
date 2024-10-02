// See https://aka.ms/new-console-template for more information

using System;
using System.Globalization;
using Slang.Showcase;
using Slang.Showcase.MyNamespace;

Console.WriteLine("Hello, World!");

Strings.SetCulture(new CultureInfo("ru-RU"));

ShowLocales();

Strings.SetCulture(new CultureInfo("en-US"));

ShowLocales();

void ShowLocales()
{
    Console.WriteLine(Strings.Instance.Root.SearchHandler.SearchDirectoryText("qwerty", "My computer"));
    Console.WriteLine(Strings.Instance.Root.SomeKey.Apple(6));
    Console.WriteLine(Strings.Instance.Root.SomeKey.Introduce("Egor", 29));
    object a = Strings.Instance.Root.SomeKey.NiceList[1]; // "nice"
    object b = Strings.Instance.Root.SomeKey.NiceList[2][0]; // "first item in nested list"
    object c = Strings.Instance.Root.SomeKey.NiceList[3].Ok; // "OK!"
    object d = Strings.Instance.Root.SomeKey.NiceList[4].AMapEntry;
    Console.WriteLine($"{a}, {b}, {c}, {d}");
    
    string aMap = Strings.Instance.Root.SomeKey.A["helloWorld"]; // "hello"
    string bMap = Strings.Instance.Root.SomeKey.B.B0; // "hey"
    string cMap = Strings.Instance.Root.SomeKey.B.B1["hiThere"];

    Console.WriteLine($"{aMap}, {bMap}, {cMap}");

    Console.WriteLine(Feature1.Instance.Root.Screen.Locale1);
}
