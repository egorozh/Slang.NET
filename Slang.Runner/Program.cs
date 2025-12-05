#if RELEASE
using BenchmarkDotNet.Running;
#endif

using Slang.Runner;

#if DEBUG
using Slang.Generator.Core.Data;


Console.WriteLine("Start");

//var config = Test.GetConfig();

const string sourceFilesDirectory = "../../../../Examples/Slang.Console/i18n";

//Generator builder = new(config, sourceFilesDirectory);

//await builder.Generate();
//
// var config2 = ConfigRepository.Create(
//     inputFileName: "feature1",
//     @namespace: "Slang.Console.MyNamespace",
//     className: "Feature1",
//     baseLocale: "ru-RU");
//
// Generator builder2 = new(config2, sourceFilesDirectory);
//
// await builder2.Generate();
//
// var config3 = ConfigRepository.Create(
//     inputFileName: "feature2",
//     @namespace: "Slang.Console.MyNamespace",
//     className: "Feature2",
//     baseLocale: "ru-RU");
//
// Generator builder3 = new(config3, sourceFilesDirectory);
//
// await builder3.Generate();

var config4 = ConfigRepository.Create(
    inputFileName: "feature3",
    @namespace: "Slang.Console.Features.Feature3",
    className: "Feature3",
    baseLocale: "ru-RU",
    startCharacter: "{{",
    endCharacter: "}}");

Generator builder4 = new(config4, sourceFilesDirectory);

await builder4.Generate();

Console.WriteLine("End");

#else
var _ = BenchmarkRunner.Run<GenerateFilesBenchmark>();
#endif