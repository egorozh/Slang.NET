#if RELEASE
using BenchmarkDotNet.Running;
#endif

using Slang.Runner;

#if DEBUG

await Task.Delay(10000);

Console.WriteLine("Start");

var config = Test.GetConfig(args);

I18NBuilder builder = new(config);

await builder.Build();

Console.WriteLine("End");
Console.ReadLine();

#else
var _ = BenchmarkRunner.Run<GenerateFiles>();
#endif