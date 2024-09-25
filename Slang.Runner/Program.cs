#if RELEASE
using BenchmarkDotNet.Running;
#endif

using Slang.Runner;

#if DEBUG

Console.WriteLine("Start");

var config = Test.GetConfig();

I18NBuilder builder = new(config);

await builder.Build();

var config2 = Test.GetConfig2();

I18NBuilder builder2 = new(config2);

await builder2.Build();

Console.WriteLine("End");

#else
var _ = BenchmarkRunner.Run<GenerateFilesBenchmark>();
#endif