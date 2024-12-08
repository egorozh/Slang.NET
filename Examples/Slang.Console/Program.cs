using Slang.Console;
using Slang.Console.Features.Feature1;

foreach (var culture in Feature1.SupportedCultures)
{
    Console.WriteLine($"{culture} translations:\n");

    Strings.SetCulture(culture);

    ShowLocales();

    Console.WriteLine();
}

return;

void ShowLocales()
{
    Console.WriteLine(Strings.Loc.SearchHandler.SearchDirectoryText("qwerty", "My computer"));

    var texts = Strings.Loc.SomeKey;
    
    Console.WriteLine(texts.Apple(6));
    Console.WriteLine(texts.Introduce("Egor", 29));
    object a = texts.NiceList[1]; // "nice"
    object b = texts.NiceList[2][0]; // "first item in nested list"
    object c = texts.NiceList[3].Ok; // "OK!"
    object d = texts.NiceList[4].AMapEntry;
    Console.WriteLine($"{a}, {b}, {c}, {d}");

    string aMap = texts.A["helloWorld"]; // "hello"
    string bMap = texts.B.B0; // "hey"
    string cMap = texts.B.B1["hiThere"];

    Console.WriteLine($"{aMap}, {bMap}, {cMap}");

    Console.WriteLine(Feature1.Instance.Root.Screen.Data(DateOnly.FromDateTime(DateTime.Now)));
    Console.WriteLine(Feature1.Instance.Root.Screen.TestParam(232));
    Console.WriteLine(Feature1.Instance.Root.ProfileRating(4.763413));

    var formattingBloc = Feature1.Instance.Root.Formatting;

    string price = formattingBloc.DecimalExample(12.23123M);
    
    Console.WriteLine(price);
    Console.WriteLine(formattingBloc.LongExample(124214));
    Console.WriteLine(formattingBloc.IntExample(123));
    Console.WriteLine(formattingBloc.TimeSpanExample(TimeSpan.FromMinutes(13)));
    Console.WriteLine(formattingBloc.ObjectExample2("Hello World!"));
    Console.WriteLine(formattingBloc.FloatExample(123.31414f));
}