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

			// Rebindings one after another, each wrapping the operand once more.
			"B = 'b'\nC = 'c'\nA = B" + string.Concat(Enumerable.Repeat(" with (B = C)", DeepChains.Depth)) + "\nparse A\n",

			// Namespaces.
			string.Concat(Enumerable.Repeat("namespace N {\n", DeepChains.Depth)) + "A = 'x'\n" +
				new string('}', DeepChains.Depth) + "\nB = 'x'\nparse B\n",
		})
		{
			var compiled = DeepChains.OnSmallStack(() => GramCompiler.Compile(grammar));

			var error = Assert.Single(compiled.Diagnostics, static d => d.Severity == GramSeverity.Error);

			Assert.Equal(GramParser.NestedTooDeep, error.Id);
		}

		// A chain of conditions is a list and not a tree: read, however long, and with nothing to say.
		foreach (var joined in new[] { "and", "or" })
		{
			var chain = "A = when 'x' is 'x'" + string.Concat(Enumerable.Repeat($" {joined} 'x' is 'x'", DeepChains.Depth)) +
				" & 'x'\nparse A\n";

			var compiled = DeepChains.OnSmallStack(() => GramCompiler.Compile(chain));

			Assert.False(
				compiled.HasErrors,
				string.Join("\n", compiled.Diagnostics.Where(static d => d.Severity == GramSeverity.Error).Take(5)));
		}

		// And the bound is the bound: sixty-three groups inside each other are a grammar like any
		// other, on the small stack too.
		Assert.False(DeepChains.OnSmallStack(() => GramCompiler.Compile(DeepChains.Nested(63))).HasErrors);
		Assert.True(DeepChains.OnSmallStack(() => GramCompiler.Compile(DeepChains.Nested(64))).HasErrors);
	}

	/// <summary>
	/// The chains a review found after the shapes above passed, as it wrote them: rules declared
	/// from the top down through forwarding and transparent types, a chain of rebindings, the
	/// readers of a rebinding, guards after a choice that ends twice, and a negative look cut in two.
	/// </summary>
	public static TheoryData<string> Found()
	{
		return new TheoryData<string>("forwarders", "transparent", "rebindings", "readers", "ends twice", "look cut in two");
	}

	static string Descending(int depth, Func<int, string> level, string bottom, string tail)
	{
		var text = new System.Text.StringBuilder();

		for (var i = depth; i >= 1; i--)
			text.Append(level(i)).Append('\n');

		return text.Append(bottom).Append('\n').Append(tail).ToString();
	}

	static string Ascending(int from, int depth, string head, Func<int, string> level, string tail)
	{
		var text = new System.Text.StringBuilder(head).Append('\n');

		for (var i = from; i <= depth; i++)
			text.Append(level(i)).Append('\n');

		return text.Append(tail).ToString();
	}

	[Theory]
	[MemberData(nameof(Found))]
	public void A_chain_a_review_found_compiles_on_a_small_stack(string shape)
	{
		var (grammar, lexical) = shape switch
		{
			"forwarders" => (Descending(
				DeepChains.Depth, static i => $"A{i} : @int = v:A{i - 1} => @(v)", "A0 : @int = 'b' => @(1)",
				$"parse A{DeepChains.Depth}\n"), false),

			"transparent" => (Descending(
				DeepChains.Depth, static i => $"A{i} : A{i - 1} = v:A{i - 1} => @(v)", "A0 : @int = 'b' => @(1)",
				$"parse A{DeepChains.Depth}\n"), false),

			"rebindings" => ("B = 'b'\nC = 'c'\n" + Descending(
				2500, static i => $"A{i} = A{i - 1} with (B = C)", "A0 = B", "parse A2500\n"), false),

			"readers" => (Ascending(
				2, 2500, "B : @int = 'b' => @(1)\nD : @string = 'd' => @(\"d\")\nA1 : B = when @(true) & B",
				static i => $"A{i} : A{i - 1} = when @(true) & A{i - 1}", "parse A2500 with (B = D)\n"), false),

			"ends twice" => (Ascending(
				1, DeepChains.Depth, "A0 = \"a\" | \"ab\"", static i => $"A{i} = A{i - 1} & when @(true)",
				$"Row : @string = 'r' => @(\"r\")\nR : @string[] = A{DeepChains.Depth} & 'b' & Row*\nparse R\n"), false),

			"look cut in two" => (Ascending(
				1, DeepChains.Depth,
				"trivia = { ' '* }\nWord : @string = 'a' => @(\"a\")\nA0 : @string[] = (?!('a' & 'b') & Word)* & when @(true)",
				static i => $"A{i} : A{i - 1} = A{i - 1} & when @(true)",
				$"R : A{DeepChains.Depth} = A{DeepChains.Depth} & 'a' & 'b'\nparse R\n"), true),

			_ => throw new ArgumentOutOfRangeException(nameof(shape)),
		};

		var compiled = DeepChains.OnSmallStack(() => GramCompiler.Compile(
			grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance, Lexical = lexical }));

		Assert.False(
			compiled.HasErrors,
			string.Join("\n", compiled.Diagnostics.Where(static d => d.Severity == GramSeverity.Error).Take(5)));
	}

	/// <summary>
	/// A `when` looked at to tell a condition from a guard, its operand too deep: the look takes
	/// back what it said, and the reading for real says it, once.
	/// </summary>
	[Fact]
	public void A_when_too_deep_is_told_once_after_it_was_looked_at()
	{
		var grammar = "P(x) = x\nA = when P(" + new string('(', 64) + "'x'" + new string(')', 64) + ") is 'x' & 'x'\nparse A\n";

		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);

		Assert.Single(parsed.Diagnostics, static d => d.Id == GramParser.NestedTooDeep);
	}

	/// <summary>Reading past a chain of rebindings too long stops at the next declaration, a publication too.</summary>
	[Fact]
	public void Reading_past_a_rule_too_deep_stops_at_the_publication_after_it()
	{
		var grammar = "B = 'b'\nC = 'c'\nA = B" + string.Concat(Enumerable.Repeat(" with (B = C)", 130)) + "\nparse Missing\n";

		var parsed = GramCompiler.Read(grammar);

		Assert.Single(parsed.Diagnostics, static d => d.Id == GramParser.NestedTooDeep);
		Assert.Contains(parsed.File.Decls, static d => d is Decl.Publish);
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
