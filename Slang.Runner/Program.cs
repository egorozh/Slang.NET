#if RELEASE
using BenchmarkDotNet.Running;
#endif

using Slang.Runner;

#if DEBUG

Console.WriteLine("Start");

var config = Test.GetConfig(args);

I18NBuilder builder = new(config);

await builder.Build();

Console.WriteLine("End");

#else
var _ = BenchmarkRunner.Run<GenerateFiles>();
#endif