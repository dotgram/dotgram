---
paths:
  - "src/DotGram/**"
  - ".work/genprof/**"
---

# Profiling the generator

What was worked out once and should not be searched for again. The measured subject is
`GramGenerator` over `DotGram.Sql` — the slowest thing the solution builds.

## 1. Measure in the build first

```powershell
dotnet build src/DotGram.Sql/DotGram.Sql.csproj -c Debug --no-dependencies -t:Rebuild `
    -p:ReportAnalyzer=true -bl:$scratch\sql.binlog
dotnet msbuild $scratch\sql.binlog -noconlog "-flp:logfile=$scratch\sql.log;verbosity=diagnostic"
Select-String $scratch\sql.log 'Total generator execution time'
```

One line per target framework. The generator runs once for each, so Visual Studio pays it twice.
Everything above that number in the build is `csc` itself.

## 2. The harness

A console app that runs the generator over `DotGram.Sql` in one process a profiler can start.
It lives in `.work/genprof/` (ignored by git), and if that is gone it is recreated from here.

`genprof.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<OutputType>Exe</OutputType>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
		<IsPackable>false</IsPackable>
	</PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" />
		<ProjectReference Include="..\..\src\DotGram\DotGram.csproj" />
	</ItemGroup>
</Project>
```

`Program.cs`:

```csharp
using System;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

// Usage: genprof [dump-directory|-] [runs]

var repo = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var root = Path.Combine(repo, "src", "DotGram.Sql");
var dump = args.Length > 0 && args[0] != "-" ? args[0] : null;
var runs = args.Length > 1 ? int.Parse(args[1]) : 1;

var parse = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: ["NET10_0", "NET10_0_OR_GREATER", "NET", "NETCOREAPP"]);
var trees = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
	.Where(p => !p.Contains(@"\obj\") && !p.Contains(@"\bin\"))
	.Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), parse, p))
	.ToList();

var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
	.Where(p => Path.GetFileName(p).StartsWith("System") || Path.GetFileName(p) is "netstandard.dll" or "mscorlib.dll")
	.Select(p => MetadataReference.CreateFromFile(p));

var compilation = CSharpCompilation.Create("DotGram.Sql", trees, refs,
	new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

var texts = Directory.EnumerateFiles(root, "*.gram", SearchOption.AllDirectories)
	.Where(p => !p.Contains(@"\obj\"))
	.Select(p => (AdditionalText)new FileText(p))
	.ToImmutableArray();

for (var run = 0; run < runs; run++)
{
	GeneratorDriver driver = CSharpGeneratorDriver.Create(
		[new DotGram.Generation.GramGenerator().AsSourceGenerator()],
		texts, parse, driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

	var watch = Stopwatch.StartNew();

	driver = driver.RunGenerators(compilation);

	var result = driver.GetRunResult();

	Console.WriteLine($"run {run}: {watch.Elapsed.TotalSeconds:F2}s");

	foreach (var generator in result.Results)
	{
		foreach (var (name, steps) in generator.TrackedSteps)
			if (name == DotGram.Generation.GramGenerator.CompiledStage)
				foreach (var step in steps)
					Console.WriteLine($"  {name,-10} {step.ElapsedTime.TotalSeconds,8:F2}s  {Describe(step)}");

		foreach (var source in generator.GeneratedSources)
		{
			Console.WriteLine($"  out {source.HintName} {source.SourceText.Length:N0}");

			if (dump is not null && run == 0)
			{
				Directory.CreateDirectory(dump);
				File.WriteAllText(Path.Combine(dump, source.HintName), source.SourceText.ToString());
			}
		}

		foreach (var diagnostic in generator.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Take(5))
			Console.WriteLine($"  ERR {diagnostic}");
	}
}

static string Describe(IncrementalGeneratorRunStep step)
{
	var text = step.Outputs.FirstOrDefault().Value?.ToString() ?? "";

	return text.Length > 90 ? text[..90] : text;
}

sealed class FileText(string path) : AdditionalText
{
	public override string Path => path;

	public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(File.ReadAllText(path));
}
```

```powershell
dotnet build .work/genprof -c Release
dotnet .work\genprof\bin\Release\net10.0\genprof.dll $scratch\after
```

It prints the whole run and the `Compiled` stage per host: which grammar the time goes to. The
other stages (`Asked`, `Answered`) are fractions of a second.

## 3. A change to speed must not change output

Before touching anything, take a baseline from the commit being changed, in a worktree of its
own so the working tree stays as it is:

```powershell
git worktree add --detach $scratch\base-wt HEAD
# a copy of .work/genprof whose ProjectReference points at $scratch\base-wt\src\DotGram\DotGram.csproj
dotnet $scratch\genprof-base\bin\Release\net10.0\genprof.dll $scratch\baseline
```

After each change, dump again and compare every file by hash. "same" for all of them is the
bar; a `DIFF` is a behaviour change, not a speed-up. Remove the worktree when done
(`git worktree remove`).

A baseline goes stale when a rebase brings grammar changes: re-take it rather than read a `DIFF`
the grammar caused. Wall-clock comparisons need a quiet machine — run baseline and candidate
alternately, never beside a test run.

## 4. dotTrace — where the time goes

Installed as a dotnet tool, which is how it is run:

```powershell
dotnet tool install -g JetBrains.dotTrace.GlobalTools

dottrace start --profiling-type=Sampling --time-measurement=ThreadTime `
    --save-to=$scratch\dt\gen.dtp --overwrite --no-check-for-updates `
    P:\...\.work\genprof\bin\Release\net10.0\genprof.exe $scratch\dump
```

- Give it the **apphost** `genprof.exe` by full path. `dotnet genprof.dll` fails: it does not
  look `dotnet` up on `PATH`.
- No argument to the profiled program may begin with `-`: `dottrace` takes it for its own option.
  Pass a dump directory rather than `-`.
- `ThreadTime` counts CPU time of each thread, so a snapshot taken while something else runs is
  still comparable with another one.

Read the snapshot with **Reporter**, which ships with the installed dotTrace:

```powershell
# pattern file: <Patterns><Pattern>.*</Pattern></Patterns>
& "$env:LOCALAPPDATA\JetBrains\Installations\dotTrace262\Reporter.exe" report $scratch\dt\gen.dtp `
    --pattern=$scratch\dt\all.xml --save-to=$scratch\dt\all.xml.report --overwrite --no-check-for-updates

[xml]$x = Get-Content $scratch\dt\all.xml.report
$x.Report.Function |
    ForEach-Object { [pscustomobject]@{ Own = [int]$_.OwnTime; Total = [int]$_.TotalTime; Inst = [int]$_.Instances; FQN = $_.FQN } } |
    Sort-Object Own -Descending | Select-Object -First 30
```

The report is flat: own and total milliseconds per function, and how many call-tree nodes it
has. There are no callers in it. Sorting by `Total` over `DotGram.*` gives the stages; sorting by
`Own` gives the hot loops. Two snapshots are compared by loading both reports and printing the
same functions side by side. The call tree itself is in the snapshot, for the UI.

**Do not use `dotnet-trace`'s thread-time profile to choose what to fix.** It reported
`Buffer.Memmove` at 20%, `RuntimeHelpers.GetHashCode` at 14% and `Monitor.Enter_Slowpath` at 9%,
and dotTrace shows none of those.

## 5. dotMemory — allocations and GC

Not a dotnet tool. The console profiler is the NuGet package
`JetBrains.dotMemory.Console.windows-x64`, unpacked into the scratchpad. The `dotMemory.exe` of
the installed dotMemory prints nothing, and the one under Rider fails to load
`JetBrains.Platform.Core`.

```powershell
Invoke-WebRequest https://www.nuget.org/api/v2/package/JetBrains.dotMemory.Console.windows-x64/2026.2.1 -OutFile $scratch\dotmemory.zip
Expand-Archive $scratch\dotmemory.zip $scratch\dotmemory

& $scratch\dotmemory\tools\dotMemory.exe start --trigger-timer=40s --trigger-max-snapshots=1 `
    --save-to-dir=$scratch\dm --no-check-for-updates `
    P:\...\.work\genprof\bin\Release\net10.0\genprof.exe $scratch\dump
```

Allocations are sampled unless `--collect-alloc` is given. The result is a workspace (`.dmw`,
close to a gigabyte) that only the dotMemory UI reads — there is no command-line report, so the
analysis is the user's to open.

## 6. Tests

`dotnet test tests/DotGram.Tests/DotGram.Tests.csproj` reports "Zero tests ran". Build the project,
then run the assembly with its own xUnit runner:

```powershell
dotnet build tests/DotGram.Tests/DotGram.Tests.csproj -c Debug
dotnet tests\DotGram.Tests\bin\Debug\net10.0\DotGram.Tests.dll
```
