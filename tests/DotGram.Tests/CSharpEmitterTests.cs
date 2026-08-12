using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The emitter checked by compiling what it wrote and running it.
/// </summary>
/// <remarks>
/// Asserting on the text would only say the generator is consistent with itself.
/// Compiling it says it is valid C#, and running it says the grammar recognizes what
/// it claims to — which is the only claim worth making at this stage.
/// </remarks>
public sealed class CSharpEmitterTests
{
	static string Emit(string grammar, string ruleName = "Start")
	{
		var model = GrammarBinder.Bind(
			GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File);

		var graph = GrammarNormalizer.Normalize(model);
		var rule  = graph.Rules.Single(r => r.Name == ruleName);

		return CSharpEmitter.Emit(graph, "Grammar", [new Publication(rule, "Parse")]);
	}

	/// <summary>Compiles the emitted source and calls its TryParse.</summary>
	static (bool Matched, string Value) Run(string grammar, string input, string ruleName = "Start")
	{
		var source      = Emit(grammar, ruleName);
		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Emitted",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ }}\n{source}")],
			AppDomain.CurrentDomain.GetAssemblies()
				.Where(static assembly => !assembly.IsDynamic && assembly.Location.Length > 0)
				.Select(static assembly => MetadataReference.CreateFromFile(assembly.Location)),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using var stream = new System.IO.MemoryStream();

		var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(
			result.Success,
			"Emitted source did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)) +
			"\n\n" + source);

		var type      = System.Reflection.Assembly.Load(stream.ToArray()).GetType("Grammar")!;
		var arguments = new object?[] { input, null, null, null };
		var matched   = (bool)type.GetMethod("TryParse")!.Invoke(null, arguments)!;

		return (matched, (string)arguments[1]!);
	}

	[Theory]
	[InlineData("abc",  true)]
	[InlineData("abd",  false)]
	[InlineData("ab",   false)]
	[InlineData("abcd", false)]     // parse requires the whole input
	public void Literals(string input, bool expected) =>
		Assert.Equal(expected, Run("""Start = 'a' & 'b' & 'c'""", input).Matched);

	[Theory]
	[InlineData("42",    true)]
	[InlineData("0",     true)]
	[InlineData("",      false)]
	[InlineData("4x",    false)]
	public void Element_sets_and_repetition(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = ['0'..'9']+", input).Matched);

	[Theory]
	[InlineData("cat", true)]
	[InlineData("dog", true)]
	[InlineData("cow", false)]
	public void Ordered_choice(string input, bool expected) =>
		Assert.Equal(expected, Run("""Start = "cat" | "dog" """, input).Matched);

	[Fact]
	public void Ordered_choice_backtracks_across_a_shared_prefix()
	{
		// The case a commit point would have broken: both alternatives begin with the
		// same rule and diverge only after it (docs/syntax.md §10).
		var grammar = """
			Start = Call | Index
			Call  = Name & '(' & ')'
			Index = Name & '[' & ']'
			Name  = ['a'..'z']+
			""";

		Assert.True(Run(grammar, "foo()").Matched);
		Assert.True(Run(grammar, "foo[]").Matched);
		Assert.False(Run(grammar, "foo{}").Matched);
	}

	[Theory]
	[InlineData("ab",   true)]
	[InlineData("b",    true)]
	[InlineData("aab",  false)]
	public void Optional(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a'? & 'b'", input).Matched);

	[Theory]
	[InlineData("aaa", true)]
	[InlineData("aa",  false)]
	[InlineData("aaaa", false)]
	public void Counted_repetition(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a'{3}", input).Matched);

	[Theory]
	[InlineData("ab", true)]
	[InlineData("ax", false)]
	public void Lookahead_consumes_nothing(string input, bool expected) =>
		Assert.Equal(expected, Run("Start = 'a' & ?='b' & 'b'", input).Matched);

	[Fact]
	public void The_value_is_the_matched_text() =>
		Assert.Equal("hello", Run("Start = ['a'..'z']+", "hello").Value);

	[Fact]
	public void Rules_call_each_other()
	{
		var grammar = """
			Start  = Digits & '-' & Digits
			Digits = ['0'..'9']+
			""";

		Assert.True (Run(grammar, "12-34").Matched);
		Assert.False(Run(grammar, "12-").Matched);
	}
}
