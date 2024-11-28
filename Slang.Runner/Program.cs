#if RELEASE
using BenchmarkDotNet.Running;
#endif

using Slang.Generator.Core.Data;
using Slang.Runner;

#if DEBUG

Console.WriteLine("Start");

var config = Test.GetConfig();

const string sourceFilesDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Examples/Slang.Console/i18n";

Generator builder = new(config, sourceFilesDirectory);

await builder.Generate();

var config2 = ConfigRepository.Create(
    inputFileName: "feature1",
    @namespace: "Slang.Console.MyNamespace",
    className: "Feature1",
    baseLocale: "ru-RU");

Generator builder2 = new(config2, sourceFilesDirectory);

await builder2.Generate();

Console.WriteLine("End");

#else
var _ = BenchmarkRunner.Run<GenerateFilesBenchmark>();
#endif