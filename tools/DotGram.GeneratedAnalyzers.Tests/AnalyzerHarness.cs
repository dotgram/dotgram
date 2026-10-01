using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotGram.GeneratedAnalyzers.Tests;

/// <summary>
/// A hand-rolled compilation, the same technique the repository's own profiling harness uses
/// (docs/.claude/rules/profiling.md's genprof): no Microsoft.CodeAnalysis.Testing package, just
/// a real Compilation plus CompilationWithAnalyzers over one source file.
/// </summary>
static class AnalyzerHarness
{
	public static async Task<ImmutableArray<Diagnostic>> RunAsync(string source, string path = "Test.g.cs")
	{
		var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
		var tree = CSharpSyntaxTree.ParseText(source, parseOptions, path: path);

		var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(System.IO.Path.PathSeparator)
			.Select(p => MetadataReference.CreateFromFile(p));

		var compilation = CSharpCompilation.Create(
			"Test",
			[tree],
			refs,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new GeneratedCodeAnalyzer());

		// reportSuppressedDiagnostics: true -- GetAnalyzerDiagnosticsAsync drops a
		// pragma-suppressed diagnostic entirely by default, which makes a test that wants to
		// confirm "the rule still fired, merely suppressed" (as opposed to "the rule never
		// fired at all") impossible without this: with it, a suppressed diagnostic is
		// returned with IsSuppressed set, rather than not returned at all.
		var options = new CompilationWithAnalyzersOptions(
			new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty),
			onAnalyzerException: null,
			concurrentAnalysis: true,
			logAnalyzerExecutionTime: false,
			reportSuppressedDiagnostics: true);

		var withAnalyzers = compilation.WithAnalyzers(analyzers, options);

		var compilationDiagnostics = compilation.GetDiagnostics();
		var errors = compilationDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
		if (errors.Length > 0)
			throw new InvalidOperationException("Test source does not compile: " + string.Join("\n", errors.Select(e => e.ToString())));

		var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();

		return diagnostics;
	}
}
