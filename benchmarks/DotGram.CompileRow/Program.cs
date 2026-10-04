using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

// Usage: DotGram.CompileRow <DotGram.Sql project directory> <floor|latest> <runs> [directory for the generated sources]
//
// "latest" runs the generator at the newest C# this compiler knows and compiles everything there.
// "floor" runs it at C# 8 (the version the consumer's parse options say, which is what the emitted
// code is written for) and compiles what it wrote, with the hand-written sources, at the newest:
// one compilation cannot mix language versions, and the hand-written code needs more than C# 8.
// So the row is the cost of the floor's form of the code, not a check that it is C# 8; building
// DotGram.Compatibility is that check.
// One line comes out: the best of the runs by wall time, in milliseconds, and the sizes.
if (args.Length < 3)
	throw new ArgumentException("Usage: DotGram.CompileRow <DotGram.Sql directory> <floor|latest> <runs> [generated sources directory]");

var project = Path.GetFullPath(args[0]);
var floor   = args[1] == "floor";
var runs    = int.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
var keep    = args.Length > 3 ? args[3] : null;

var newest  = LanguageVersion.Preview;
var version = floor ? LanguageVersion.CSharp8 : newest;
var defines = "TRACE;RELEASE;NET;NET10_0;NETCOREAPP;NET5_0_OR_GREATER;NET6_0_OR_GREATER;NET7_0_OR_GREATER;NET8_0_OR_GREATER;NET9_0_OR_GREATER;NET10_0_OR_GREATER".Split(';');

var handOptions = new CSharpParseOptions(newest, DocumentationMode.Diagnose, preprocessorSymbols: defines);
var genOptions  = new CSharpParseOptions(version, DocumentationMode.Diagnose, preprocessorSymbols: defines);

var reference = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "packs", "Microsoft.NETCore.App.Ref"))
	.OrderBy(static directory => directory, StringComparer.Ordinal).Last();

var references = Directory.GetFiles(Path.Combine(reference, "ref", "net10.0"), "*.dll")
	.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
	.ToList();

const string Usings = "global using System;\nglobal using System.Collections.Generic;\nglobal using System.IO;\nglobal using System.Linq;\nglobal using System.Net.Http;\nglobal using System.Threading;\nglobal using System.Threading.Tasks;\n";

var hand = Directory.GetFiles(project, "*.cs", SearchOption.AllDirectories)
	.Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
		!path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
	.OrderBy(static path => path, StringComparer.Ordinal)
	.Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), handOptions, path, Encoding.UTF8))
	.Append(CSharpSyntaxTree.ParseText(Usings, handOptions, "GlobalUsings.g.cs", Encoding.UTF8))
	.ToList();

var grams = Directory.GetFiles(project, "*.gram", SearchOption.AllDirectories)
	.OrderBy(static path => path, StringComparer.Ordinal)
	.Select(static path => (AdditionalText)new GramFile(path))
	.ToList();

var compilationOptions = new CSharpCompilationOptions(
	OutputKind.DynamicallyLinkedLibrary,
	optimizationLevel: OptimizationLevel.Release,
	nullableContextOptions: NullableContextOptions.Enable,
	deterministic: true,
	specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic>
	{
		["CS1591"] = ReportDiagnostic.Suppress,
		["CS1573"] = ReportDiagnostic.Suppress,
	});

var process = Process.GetCurrentProcess();

Run();

var best = (Wall: double.MaxValue, Cpu: 0.0, Generate: 0.0, Parse: 0.0, Bind: 0.0, Emit: 0.0, Dll: 0L, Strings: 0, Source: 0L);

for (var run = 0; run < runs; run++)
{
	var now = Run();

	if (now.Wall < best.Wall)
		best = now;
}

Console.WriteLine(
	$"{(floor ? "C# 8 floor" : "latest")}: wall {best.Wall:F0} ms, cpu {best.Cpu:F0} ms " +
	$"(generate {best.Generate:F0}, parse {best.Parse:F0}, bind {best.Bind:F0}, emit {best.Emit:F0}); " +
	$"dll {best.Dll} bytes, #US heap {best.Strings} bytes, generated source {best.Source} bytes");

return;

(double Wall, double Cpu, double Generate, double Parse, double Bind, double Emit, long Dll, int Strings, long Source) Run()
{
	GC.Collect();
	GC.WaitForPendingFinalizers();
	GC.Collect();
	process.Refresh();

	var cpu0  = process.TotalProcessorTime;
	var clock = Stopwatch.StartNew();

	// The generator reads the language version from the parse options the driver is given.
	// Its trees are read at the version it is told, or the driver cannot add what it writes.
	var seed = CSharpCompilation.Create(
		"DotGram.Sql",
		hand.Select(tree => tree.WithRootAndOptions(tree.GetRoot(), genOptions)),
		references,
		compilationOptions);
	var run  = CSharpGeneratorDriver
		.Create([new GramGenerator().AsSourceGenerator()], additionalTexts: grams, parseOptions: genOptions)
		.RunGenerators(seed);

	var result    = run.GetRunResult();
	var generated = result.Results.SelectMany(static one => one.GeneratedSources).ToList();
	var generate  = clock.Elapsed.TotalMilliseconds;

	if (keep is not null)
	{
		Directory.CreateDirectory(keep);

		foreach (var source in generated)
			File.WriteAllText(Path.Combine(keep, source.HintName), source.SourceText.ToString(), new UTF8Encoding(false));
	}

	var trees = hand
		.Concat(generated
			.Where(static source => !source.HintName.Contains("DotGramReport", StringComparison.Ordinal))
			.Select(source => CSharpSyntaxTree.ParseText(source.SourceText.ToString(), handOptions, source.HintName, Encoding.UTF8)))
		.ToList();

	var parse = clock.Elapsed.TotalMilliseconds - generate;

	var compilation = CSharpCompilation.Create("DotGram.Sql", trees, references, compilationOptions);
	var diagnostics = compilation.GetDiagnostics();
	var bound       = clock.Elapsed.TotalMilliseconds;

	using var image = new MemoryStream();
	using var xml   = new MemoryStream();

	var emitted = compilation.Emit(image, xmlDocumentationStream: xml, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.Embedded));
	var wall    = clock.Elapsed.TotalMilliseconds;

	process.Refresh();

	var errors = diagnostics.Concat(emitted.Diagnostics).Where(static one => one.Severity == DiagnosticSeverity.Error).Take(5).ToList();

	foreach (var error in errors)
		Console.WriteLine("ERROR " + error);

	if (errors.Count > 0 || !emitted.Success)
		throw new InvalidOperationException("The compilation failed.");

	image.Position = 0;

	int strings;

	using (var pe = new PEReader(image, PEStreamOptions.LeaveOpen))
		strings = pe.GetMetadataReader().GetHeapSize(HeapIndex.UserString);

	return (wall, (process.TotalProcessorTime - cpu0).TotalMilliseconds, generate, parse, bound - generate - parse, wall - bound, image.Length, strings,
		generated.Where(static source => !source.HintName.Contains("DotGramReport", StringComparison.Ordinal)).Sum(static source => (long)Encoding.UTF8.GetByteCount(source.SourceText.ToString())));
}

sealed class GramFile(string path) : AdditionalText
{
	public override string Path { get; } = path;

	public override Microsoft.CodeAnalysis.Text.SourceText? GetText(CancellationToken cancellationToken = default)
	{
		return Microsoft.CodeAnalysis.Text.SourceText.From(File.ReadAllText(Path), Encoding.UTF8);
	}
}
