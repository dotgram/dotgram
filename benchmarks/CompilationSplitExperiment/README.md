# Compilation file-splitting experiment

A standalone experiment over the generated legacy FIX parser in DotGram.Examples.
It is not part of the solution and does not modify production generator output.
Run from the repository root using the .NET 10 SDK. It references that SDK's Roslyn.

```powershell
dotnet build examples/DotGram.Examples -c Release -p:NuGetAudit=false
dotnet build benchmarks/CompilationSplitExperiment -c Release -p:NuGetAudit=false
$probe = 'benchmarks/CompilationSplitExperiment/bin/Release/net10.0/CompilationSplitExperiment.dll'
dotnet $probe prepare
dotnet $probe original 1
dotnet $probe split 1
dotnet $probe split 2
dotnet $probe original 2
```

Preparation requires a fresh `.work/file-split` directory. Preserve/move previous
results before preparing another baseline. It generates all example sources once,
then moves only direct method members of Fix44Grammar into partial declarations,
packing about two million characters per file unless a single method is larger.
Fields, constructors and nested types remain together in their original order.
Every moved method's complete text is checked by SHA-256, including trivia.

Each timed command is a fresh process. It loads the prepared text before the timer,
parses files in parallel, then emits Debug code with embedded PDB information.
References come from the runtime platform assemblies and the built Finance library.
These are controlled compiler comparisons, not full MSBuild or Visual Studio timings.
Generator time and preparation/splitting costs are excluded. PeakWorkingSet64 covers
the entire process; allocatedBytes covers the timed parse/compilation phase.

Do not run builds, tests or another benchmark during the timed commands. Each command
writes its variant DLL and prints one JSON result. A failing preparation or compilation
reports an error and returns a nonzero exit code. Preparation intentionally rejects
unexpected class attributes or a base list rather than silently copying their effects.

Validation used an isolated copy of the Finance test output directory, replacing only
DotGram.Examples.dll with each experimental DLL, then running all 3,808 tests. Normal
project outputs were not replaced. See the [report](../../docs/design/file-split-2026-09-16.md).
