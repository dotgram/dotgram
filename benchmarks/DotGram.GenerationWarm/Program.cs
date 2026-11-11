using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

// Usage: DotGram.GenerationWarm <generator DotGram.dll> <project .csproj> <runs>
//
// Runs the generator over the project's C# and .gram files the way a build does (a Roslyn generator driver),
// `runs` times in this one process, with the generator's compile cache off so that every run compiles, and
// prints one line per host and run, "host<TAB>name<TAB>run<TAB>ms": the figure the host's own generation
// report carries, which is the one Gate-Generation.ps1 reads in a build. Run 0 is what a fresh process pays;
// the runs after it are what a process pays that has already run the generator. A generator from before the
// reports (2026-09-17) prints no host lines.
//
// The projects the project references (not the generator, not the analyzers) are given to the compilation
// as the assemblies a build of the project has already made, found under the repository's .build.
if (args.Length != 3)
	throw new ArgumentException("Usage: DotGram.GenerationWarm <generator DotGram.dll> <project .csproj> <runs>");

var generatorPath = Path.GetFullPath(args[0]);
var projectFile   = Path.GetFullPath(args[1]);
var runs          = int.Parse(args[2], CultureInfo.InvariantCulture);
var projectDir    = Path.GetDirectoryName(projectFile)!;
var separator     = Path.DirectorySeparatorChar;
var skipped       = new[] { $"{separator}obj{separator}", $"{separator}bin{separator}", $"{separator}.build{separator}" };

var parse = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: ["NET10_0", "NET10_0_OR_GREATER", "NET", "NETCOREAPP", "NET8_0_OR_GREATER", "NET9_0_OR_GREATER"]);

var trees = Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories)
	.Where(path => !skipped.Any(path.Contains))
	.Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parse, path))
	.ToList();

var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
	.Where(path => Path.GetFileName(path).StartsWith("System", StringComparison.Ordinal) || Path.GetFileName(path) is "netstandard.dll" or "mscorlib.dll" or "Microsoft.CSharp.dll")
	.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
	.ToList();

// A reference that is not an analyzer is a project of the build whose assembly the compilation takes.
var repository = Path.GetFullPath(Path.Combine(projectDir, "..", ".."));

foreach (var reference in XDocument.Load(projectFile).Descendants("ProjectReference").Where(static r => r.Attribute("OutputItemType") is null))
{
	var path = Path.GetFullPath(Path.Combine(projectDir, ((string)reference.Attribute("Include")!).Replace('\\', '/')));
	var name = Path.GetFileNameWithoutExtension(path);

	var assembly = new[] { "release_net10.0", "release" }
		.Select(folder => Path.Combine(repository, ".build", "bin", name, folder, name + ".dll"))
		.FirstOrDefault(File.Exists);

	if (assembly is null)
		Console.Error.WriteLine($"warning: no built {name} found for {Path.GetFileName(projectFile)}");
	else
		references.Add(MetadataReference.CreateFromFile(assembly));
}

var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(projectFile), trees, references,
	new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable, allowUnsafe: true));

var texts = Directory.EnumerateFiles(projectDir, "*.gram", SearchOption.AllDirectories)
	.Where(path => !skipped.Any(path.Contains))
	.Select(static path => (AdditionalText)new FileText(path))
	.ToImmutableArray();

var generatorType = AssemblyLoadContext.Default.LoadFromAssemblyPath(generatorPath).GetType("DotGram.Generation.GramGenerator") ?? throw new InvalidOperationException("No GramGenerator in " + generatorPath);
var summary       = new Regex(@"DotGram: (?<host>[^,]+), \d+ normalized rules?, \d+ bytes UTF-8 C#, (?<ms>[\d.]+) ms generation");
var options       = new Options();

for (var run = 0; run < runs; run++)
{
	var generator = (IIncrementalGenerator)Activator.CreateInstance(generatorType)!;
	var driver    = CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], texts, parse, options).RunGenerators(compilation);

	foreach (var source in driver.GetRunResult().Results.SelectMany(static result => result.GeneratedSources))
	{
		if (!source.HintName.EndsWith("DotGramReport.g.cs", StringComparison.Ordinal))
			continue;

		var match = summary.Match(source.SourceText.ToString());

		if (match.Success)
			Console.WriteLine($"host\t{match.Groups["host"].Value}\t{run}\t{match.Groups["ms"].Value}");
	}
}

sealed class FileText(string path) : AdditionalText
{
	public override string Path
	{
		get { return path; }
	}

	public override SourceText GetText(CancellationToken cancellationToken = default)
	{
		return SourceText.From(File.ReadAllText(path));
	}
}

sealed class Options : AnalyzerConfigOptionsProvider
{
	static readonly AnalyzerConfigOptions Global = new Set(new Dictionary<string, string>
	{
		["build_property.DotGramNoCache"] = "true",
		["build_property.DotGramReportGeneration"] = "true",
	});

	static readonly AnalyzerConfigOptions Empty = new Set(new Dictionary<string, string>());

	public override AnalyzerConfigOptions GlobalOptions
	{
		get { return Global; }
	}

	public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
	{
		return Empty;
	}

	public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
	{
		return Empty;
	}

	sealed class Set(Dictionary<string, string> values) : AnalyzerConfigOptions
	{
		public override bool TryGetValue(string key, out string value)
		{
			return values.TryGetValue(key, out value!);
		}
	}
}
