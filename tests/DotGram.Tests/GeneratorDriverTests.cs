using System;
using System.Collections.Immutable;
using System.Linq;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Level 2: the generator driven in memory, so what it produced can be read back as
/// text and inspected as code.
/// </summary>
public sealed class GeneratorDriverTests
{
	[Fact]
	public void Emits_the_marker_attributes_into_every_compilation()
	{
		var source = GetGeneratedSource(RunGenerator(""), "DotGram.Attributes.g.cs");

		Assert.Contains("internal sealed class GramAttribute", source, StringComparison.Ordinal);
		Assert.Contains("internal sealed class GramRuntimeAttribute", source, StringComparison.Ordinal);
	}

	[Fact]
	public void Emits_support_types_internal_by_default()
	{
		var source = GetGeneratedSource(RunGenerator(""), "DotGram.Support.g.cs");

		Assert.Contains("internal enum Outcome", source, StringComparison.Ordinal);
		Assert.Contains("internal readonly struct SourceSpan", source, StringComparison.Ordinal);
		Assert.DoesNotContain("public enum Outcome", source, StringComparison.Ordinal);
	}

	[Fact]
	public void Emits_support_types_public_when_the_assembly_publishes_them()
	{
		var source = GetGeneratedSource(
			RunGenerator("[assembly: DotGram.GramRuntime]"),
			"DotGram.Support.g.cs");

		Assert.Contains("public enum Outcome", source, StringComparison.Ordinal);
		Assert.Contains("public readonly struct SourceSpan", source, StringComparison.Ordinal);
	}

	[Fact]
	public void Generated_sources_compile()
	{
		RunGenerator("", out var output);

		var errors = output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.Empty(errors);
	}

	static GeneratorDriverRunResult RunGenerator(string source) => RunGenerator(source, out _);

	static GeneratorDriverRunResult RunGenerator(string source, out Compilation output)
	{
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.GeneratorDriver",
			[CSharpSyntaxTree.ParseText(source, parseOptions, "GeneratorDriverTest.cs")],
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var driver = CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out output, out _);

		return driver.GetRunResult();
	}

	static string GetGeneratedSource(GeneratorDriverRunResult result, string hintName)
	{
		var sources = result.Results
			.SelectMany(static r => r.GeneratedSources)
			.Where(s => string.Equals(s.HintName, hintName, StringComparison.Ordinal))
			.ToArray();

		Assert.True(
			sources.Length == 1,
			$"Expected exactly one '{hintName}'; got {sources.Length}. Generated: " +
			string.Join(", ", result.Results.SelectMany(static r => r.GeneratedSources).Select(static s => s.HintName)));

		return sources[0].SourceText.ToString();
	}

	static ImmutableArray<MetadataReference> GetMetadataReferences() =>
	[
		.. AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
	];
}
