using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Parsing;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A chain of calls ten thousand long is compiled on a stack of one megabyte, by the compiler
/// on its own and by the generator as Roslyn runs it (<see cref="DeepChains"/>).
/// </summary>
[Collection(typeof(Alone))]
public sealed class DeepChainTests
{
	/// <summary>The whole compile, as the generator runs it with no host around it.</summary>
	[Theory]
	[MemberData(nameof(DeepChains.NamesAndReadings), MemberType = typeof(DeepChains))]
	public void A_long_chain_of_calls_compiles_on_a_small_stack(string shape, bool direct)
	{
		var grammar  = DeepChains.Shapes[shape](DeepChains.Depth);
		var compiled = DeepChains.OnSmallStack(() => GramCompiler.Compile(
			grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance, Direct = direct }));

		Assert.False(
			compiled.HasErrors,
			string.Join("\n", compiled.Diagnostics.Where(static d => d.Severity == GramSeverity.Error).Take(5)));

		Assert.NotEmpty(compiled.Sources);
	}

	/// <summary>
	/// What one rule nests is not walked with a stack of its own: it is bounded where it is read,
	/// and a rule nested past the bound is told so once, on any stack, with nothing else said.
	/// </summary>
	[Fact]
	public void A_rule_nested_past_the_bound_is_refused_once_rather_than_overflowing()
	{
		foreach (var grammar in new[]
		{
			DeepChains.Nested(DeepChains.Depth),

			// A condition's parentheses, inside each other.
			"A = when " + new string('(', DeepChains.Depth) + "'x' is 'x'" + new string(')', DeepChains.Depth) + " & 'x'\nparse A\n",

			// A call's arguments.
			"P(x) = x\nA = " + string.Concat(Enumerable.Repeat("P(", DeepChains.Depth)) + "'x'" +
				new string(')', DeepChains.Depth) + "\nparse A\n",

			// Namespaces.
			string.Concat(Enumerable.Repeat("namespace N {\n", DeepChains.Depth)) + "A = 'x'\n" +
				new string('}', DeepChains.Depth) + "\nB = 'x'\nparse B\n",
		})
		{
			var compiled = DeepChains.OnSmallStack(() => GramCompiler.Compile(grammar));

			var error = Assert.Single(compiled.Diagnostics, static d => d.Severity == GramSeverity.Error);

			Assert.Equal(GramParser.NestedTooDeep, error.Id);
		}

		// And the bound is the bound: sixty-three groups inside each other are a grammar like any
		// other, on the small stack too.
		Assert.False(DeepChains.OnSmallStack(() => GramCompiler.Compile(DeepChains.Nested(63))).HasErrors);
		Assert.True(DeepChains.OnSmallStack(() => GramCompiler.Compile(DeepChains.Nested(64))).HasErrors);
	}

	/// <summary>
	/// A grammar in a <c>[Gram]</c> attribute, through the generator as Roslyn runs it, with the
	/// host's C# resolved by Roslyn and what it wrote compiled with the host.
	/// </summary>
	[Fact]
	public void A_long_chain_of_calls_in_a_host_class_compiles_on_a_small_stack()
	{
		var grammar = DeepChains.Shapes["guard"](DeepChains.Depth);
		var source  = "using DotGram;\n\n[Gram(\"\"\"\n" + grammar + "\"\"\")]\npublic static partial class Deep;\n\n" +
			// The parser has to be there to be called: a run that wrote only the support would pass otherwise.
			"static class Use { static void Call() { Deep.ParseR(\"b\"); } }\n";

		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation  = CSharpCompilation.Create(
			"DotGram.Tests.DeepChain", [CSharpSyntaxTree.ParseText(source, parseOptions, "Deep.cs", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var run = DeepChains.OnSmallStack(() =>
		{
			var driver = CSharpGeneratorDriver
				.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parseOptions)
				.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

			return (Result: driver.GetRunResult(), Output: output);
		});

		Assert.Empty(run.Result.Diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error));
		Assert.Contains(
			run.Result.Results.SelectMany(static r => r.GeneratedSources),
			static s => s.SourceText.ToString().Contains("partial class Deep", StringComparison.Ordinal));

		var errors = DeepChains.OnSmallStack(() =>
			run.Output.GetDiagnostics(TestContext.Current.CancellationToken)
				.Where(static d => d.Severity == DiagnosticSeverity.Error).Take(5).ToArray());

		Assert.Empty(errors);
	}
}
