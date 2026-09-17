using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

static class Production
{
	public static void Run(string name, string projectName)
	{
		var repo = Directory.GetCurrentDirectory();
		var work = Path.Combine(repo, ".work/source-parts");
		Directory.CreateDirectory(work);
		var project = Path.Combine(repo, projectName);
		var assembly = Path.GetFileName(project);
		var parse = new CSharpParseOptions(LanguageVersion.Preview,
			preprocessorSymbols: ["DEBUG", "TRACE", "NET10_0", "NET10_0_OR_GREATER", "NET", "NETCOREAPP"]);
		var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
			.Where(p => Path.GetFileName(p).StartsWith("System") || Path.GetFileName(p) is "netstandard.dll" or "mscorlib.dll" or "Microsoft.CSharp.dll")
			.Append(Path.Combine(repo, "src/DotGram.Finance/bin/Release/net10.0/DotGram.Finance.dll"))
			.Select(p => MetadataReference.CreateFromFile(p));
		var files = Directory.EnumerateFiles(project, "*.cs", SearchOption.AllDirectories)
			.Where(p => !p.Contains("\\obj\\") && !p.Contains("\\bin\\")).ToList();
		files.Add(Path.Combine(project, $"obj/Release/net10.0/{assembly}.GlobalUsings.g.cs"));
		files.Add(Path.Combine(project, $"obj/Release/net10.0/{assembly}.AssemblyInfo.cs"));
		var trees = files.Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), parse, p, Encoding.UTF8));
		var compilation = CSharpCompilation.Create(assembly, trees, references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug,
				concurrentBuild: true, deterministic: true, nullableContextOptions: NullableContextOptions.Enable));
		var texts = Directory.EnumerateFiles(project, "*.gram", SearchOption.AllDirectories)
			.Where(p => !p.Contains("\\obj\\") && !p.Contains("\\bin\\"))
			.Select(p => (AdditionalText)new Input(p)).ToImmutableArray();
		GeneratorDriver driver = CSharpGeneratorDriver.Create([new GramGenerator().AsSourceGenerator()], texts, parse);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		var allocated = GC.GetTotalAllocatedBytes(true);
		var watch = Stopwatch.StartNew();
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
		var generationMs = watch.Elapsed.TotalMilliseconds;
		using (var stream = File.Create(Path.Combine(work, name + ".dll")))
		{
			var result = output.Emit(stream, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.Embedded));
			if (!result.Success)
				throw new Exception(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Take(15)));
		}
		watch.Stop();
		var results = driver.GetRunResult();
		Console.WriteLine(JsonSerializer.Serialize(new
		{
			name, assembly, generationMs, totalMs = watch.Elapsed.TotalMilliseconds,
			allocatedBytes = GC.GetTotalAllocatedBytes(true) - allocated,
			peakWorkingSet = Process.GetCurrentProcess().PeakWorkingSet64,
			sourceCount = results.GeneratedTrees.Length,
			sourceCharacters = results.GeneratedTrees.Sum(t => (long)t.Length),
			generatorSHA256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(GramGenerator).Assembly.Location))),
		}));
		var dump = Path.Combine(work, name + "-sources");
		Directory.CreateDirectory(dump);
		foreach (var source in results.GeneratedTrees)
			File.WriteAllText(Path.Combine(dump, Path.GetFileName(source.FilePath)), source.ToString());
	}

	sealed class Input(string path) : AdditionalText
	{
		public override string Path => path;
		public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(File.ReadAllText(path), Encoding.UTF8);
	}
}
