﻿// See https://aka.ms/new-console-template for more information

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
    Console.WriteLine(Strings.Loc.SearchHandler.SearchDirectoryText("qwerty", "My computer"));
    Console.WriteLine(Strings.Loc.SomeKey.Apple(6));
    Console.WriteLine(Strings.Loc.SomeKey.Introduce("Egor", 29));
    object a = Strings.Loc.SomeKey.NiceList[1]; // "nice"
    object b = Strings.Loc.SomeKey.NiceList[2][0]; // "first item in nested list"
    object c = Strings.Loc.SomeKey.NiceList[3].Ok; // "OK!"
    object d = Strings.Loc.SomeKey.NiceList[4].AMapEntry;
    Console.WriteLine($"{a}, {b}, {c}, {d}");

    string aMap = Strings.Loc.SomeKey.A["helloWorld"]; // "hello"
    string bMap = Strings.Loc.SomeKey.B.B0; // "hey"
    string cMap = Strings.Loc.SomeKey.B.B1["hiThere"];

    Console.WriteLine($"{aMap}, {bMap}, {cMap}");

    Console.WriteLine(Feature1.Instance.Root.Screen.Data(DateOnly.FromDateTime(DateTime.Now)));
    Console.WriteLine(Feature1.Instance.Root.Screen.TestParam(232));
    Console.WriteLine(Feature1.Instance.Root.ProfileRating(4.763413));
}