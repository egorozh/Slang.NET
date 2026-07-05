# Slang.NET

Type-safe i18n for .NET: a Roslyn incremental source generator that turns `*.i18n.json`
translation files into strongly-typed C# classes. Published to NuGet as **Slang.Net**.

## Solution layout

The solution file is `Slang.slnx` (there is no `.sln`; don't recreate one).

Framework (ships in the `Slang.Net` package):
- `Slang/` — runtime library (`net8.0;net9.0`) + packaging project. Produces the `Slang.Net`
  nupkg: `lib/` from this project, generator DLLs bundled under `analyzers/dotnet/cs`.
- `Slang.Generator/` — the incremental generator entry point (`TranslateGenerator`).
- `Slang.Generator.Core/` — generation pipeline (parsing, node tree, code building).
- `Slang.Shared/` — helpers shared by generator and utilities.

The generator chain (`Slang.Generator`, `Slang.Generator.Core`, `Slang.Shared`) **must stay
`netstandard2.0`** — it is loaded by the compiler host, including Visual Studio on .NET
Framework. `Slang.Shared/Netstandard2Polyfills.cs` fills BCL gaps; PolySharp supplies
language-feature attributes. Don't add net-only APIs there.

Supporting projects:
- `Slang.Tests/` — NUnit 4 tests, including integration tests that run the full generation
  pipeline and compile its output with Roslyn.
- `Slang.Runner/` — BenchmarkDotNet benchmarks (see `Benchmarks.md`).
- `Utilities/` — internal tools (CLI tool `slang`, Avalonia desktop app, GPT-assisted
  translation). Not part of the framework package; warnings are not errors here.
- `Examples/` — consumer samples. `Slang.Console` references the in-repo generator via
  `ProjectReference` (analyzer); the other examples consume the published NuGet package.

## Build rules

- Package versions are managed centrally in `Directory.Packages.props` (CPM). No `Version=`
  on `PackageReference` in csproj files.
- `Microsoft.CodeAnalysis.CSharp` version = minimum supported Roslyn for consumers
  (currently 4.8.0 ↔ .NET SDK 8 / VS 17.8). Bump deliberately.
- Root `Directory.Build.props` sets `TargetFrameworks` (plural) as default. Single-target
  projects must override **`TargetFrameworks`**, never `TargetFramework` — the singular
  form desyncs NuGet restore from build and silently drops package build assets.
- `TreatWarningsAsErrors` is on for framework projects (off under `Utilities/`).

## Commands

```bash
dotnet build Slang.slnx                          # full build
dotnet test Slang.Tests/Slang.Tests.csproj       # framework tests
dotnet pack Slang/Slang.csproj -c Release        # produce the Slang.Net nupkg
dotnet run --project Examples/Slang.Console      # end-to-end smoke test of generation
```

After changing the generator, verify the nupkg layout: it must contain `lib/net8.0`,
`lib/net9.0`, and `analyzers/dotnet/cs` with the generator chain + System.Text.Json closure,
and **no** `Microsoft.CodeAnalysis.*` DLLs (the host provides those).

## Releases

CI (`.github/workflows/ci.yml`) builds and tests on PRs. Pushing a `v*` tag (e.g. `v9.8.0`)
triggers `release.yml`, which packs with `-p:ReleaseVersion=<tag>` and pushes to NuGet.
