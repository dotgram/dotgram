using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The fold's residue, committed where coming back to it could never change the parse.
/// </summary>
/// <remarks>
/// <para>
/// A guarded pair over one operand — <c>t: Dec &amp; when @(fits) =&gt; int | t: Dec =&gt;
/// long</c> — folds into a choice of tails that read nothing: past the shared operand the
/// alternatives differ in which factory runs, not in what the text says. Left uncommitted,
/// that choice stayed alive in the arena, and a failed parse came back to it, "reread" the
/// same span the other way, and walked everything after it again toward the same failure —
/// once per literal, which made refusing exponential where accepting was linear:
/// 74/327/1299 us at two, four and six parentheses in <c>ExpressionLanguage</c>, against
/// 26/47/75 for accepting one character more.
/// </para>
/// <para>
/// So the fold wraps such a residue in an atomic group, and these tests hold the three
/// conditions that license it: the tails read nothing, and the choice of factory is not
/// readable back — not through a capture the rule's own guards name, and not through a
/// value a guard elsewhere can materialize.
/// </para>
/// </remarks>
public sealed class CommittedResidueTests
{
	const string Pair =
		"""
		trivia = { (' ')* }
		Digits = ['0'..'9']+
		Start : @string
			= token: Digits & when @(token.Length < 3) => @("int:" + token)
			| token: Digits                            => @("long:" + token)
		""";

	[Fact]
	public void A_guarded_pair_over_one_operand_is_committed()
	{
		var graph = Graph(Pair);

		// The residue prints as an atomic group over the choice of tails: the braces are
		// the commit, and they are the compiler's — an author's construction inside braces
		// is refused before anything folds.
		Assert.Contains("{ (", Rendered(graph, "Start"), StringComparison.Ordinal);
	}

	/// <summary>The committed pair still answers exactly what the uncommitted one did.</summary>
	[Theory]
	[InlineData("12",   "int:12")]
	[InlineData("1234", "long:1234")]
	public void Committing_changes_no_answer(string input, string expected)
	{
		var result = Compile(Pair + "\nparse Start");

		Assert.Empty(result.Diagnostics);
		Assert.Equal(
			expected,
			EmittedCode.Match(
				EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseStart", input).Value);
	}

	/// <summary>
	/// A value a guard elsewhere can materialize is a window onto which factory ran, so a
	/// rule any guard-named capture reaches keeps its residue uncommitted.
	/// </summary>
	[Fact]
	public void A_rule_whose_value_a_guard_can_read_is_not_committed()
	{
		var graph = Graph(
			"""
			trivia = { (' ')* }
			Digits = ['0'..'9']+
			Inner : @string
				= token: Digits & when @(token.Length < 3) => @("int:" + token)
				| token: Digits                            => @("long:" + token)
			Start : @string = v: Inner & when @(v.Length > 2) => @(v)
			""");

		Assert.DoesNotContain("{ (", Rendered(graph, "Inner"), StringComparison.Ordinal);
	}

	/// <summary>Tails that read input are the machinery working, and are left alone.</summary>
	[Fact]
	public void Tails_that_read_are_not_committed()
	{
		var graph = Graph(
			"""
			trivia = { (' ')* }
			Digits = ['0'..'9']+
			Start : @string
				= token: Digits & "px" => @("px:" + token)
				| token: Digits & "em" => @("em:" + token)
			""");

		var rendered = Rendered(graph, "Start");

		// The fold shares the operand — the choice of tails is there — but the tails read
		// text, so no braces stand around them.
		Assert.Contains("\"px\"", rendered, StringComparison.Ordinal);
		Assert.DoesNotContain("{ (", rendered, StringComparison.Ordinal);
	}

	/// <summary>
	/// §4.5 weaves trivia between operands, and a guard is not one: it reads nothing, so no
	/// seam stands before it, and an alternative that ends in a guard ends where its last
	/// reading operand did.
	/// </summary>
	/// <remarks>
	/// What this pins beyond the seam count is the shape the commit rests on: with a seam
	/// woven before the guard, the guarded tail read trivia where its unguarded twin read
	/// nothing, and whether committing past that difference is invisible was a question
	/// about every continuation in the grammar. With no seam there, both tails read
	/// nothing, and the commit needs no such question answered.
	/// </remarks>
	[Fact]
	public void No_trivia_is_woven_before_a_guard()
	{
		var graph = Graph(
			"""
			trivia = { (' ')* }
			Word   = ['a'..'z']+
			Start  = a: Word & when @(a.Length > 1) & b: Word
			""");

		// One seam — between the two words — and none before the guard.
		Assert.Equal(1, Rendered(graph, "Start").Split("trivia").Length - 1);
	}

	static string Rendered(RecognitionGraph graph, string name) =>
		graph.Bodies[graph.Rules.First(rule => rule.Name == name)].ToString();

	static RecognitionGraph Graph(string grammar) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(grammar + "\nparse Start", RoslynCSharpScanner.Instance)).File),
			null,
			RoslynCSharpScanner.Instance);

	static GramCompilation Compile(string grammar) => GramCompiler.Compile(
		grammar,
		new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });
}
