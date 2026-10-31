using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The analysis of repetitions that can read one run of text in two ways: what it tells, with
/// what witness, and what it is silent about and why.
/// </summary>
/// <remarks>
/// Asked of the analysis directly, over the graph the compiler would build, and not through a
/// diagnostic: which grammars it tells is held here first, its words later.
/// </remarks>
public sealed class ExponentialRepetitionTests
{
	[Theory]
	[InlineData("R = (('a' | 'a' & 'a')+)* & 'b'", "'a' 'a'")]
	[InlineData("R = ('x'+)* & 'b'",                "'x' 'x'")]
	[InlineData("R = ('x'+)+ & 'b'",                "'x' 'x'")]
	[InlineData("R = (\"xy\" | \"xy\")* & 'b'",     "'x' 'y'")]
	[InlineData("R = ('a' | 'a' & 'a')+ & 'b'",     "'a' 'a'")]

	// Through a call: what a rule reads is read where it is called.
	[InlineData("X = 'x'+\nR = X* & 'b'", "'x' 'x'")]

	// A bounded repetition inside is written out turn by turn, and still cuts two ways.
	[InlineData("R = ('x'{1,2})* & 'b'", "'x' 'x'")]

	// A choice of two that can both read nothing is two ways to read nothing, between two letters.
	[InlineData("R = ('a' & ('b'? | 'c'?) & 'd')* & ';'", "'a' 'd'")]
	public void A_repetition_that_cuts_two_ways_is_told(string rules, string witness)
	{
		var told = Assert.Single(Analyse(rules + "\nparse R\n").Found);

		Assert.Equal("R", told.Rule.Name);
		Assert.Equal(witness, told.Witness);
	}

	/// <summary>
	/// Folding white space optional on both sides of a repeated word: the space between two
	/// words is the end of one turn or the start of the next.
	/// </summary>
	[Fact]
	public void Optional_space_on_both_sides_of_a_repeated_word_is_told()
	{
		var told = Assert.Single(Analyse(
			"Cfws = ' '+\n" +
			"W = { ['a'..'z']+ }\n" +
			"R = (Cfws? & W & Cfws?)+ & ';'\n" +
			"parse R\n").Found);

		Assert.Equal("R", told.Rule.Name);
		Assert.Equal("W ' ' W", told.Witness);
	}

	/// <summary>A literal run of characters repeated inside a repetition: the URI template's shape before it was braced.</summary>
	[Fact]
	public void The_unbraced_template_literal_run_is_told()
	{
		var told = Assert.Single(Analyse(
			"LiteralChar = ['a'..'z' | '0'..'9']\n" +
			"Pct = '%' & ['0'..'9'] & ['0'..'9']\n" +
			"Literals = (LiteralChar | Pct)+\n" +
			"Part = Literals | '{' & ['a'..'z']+ & '}'\n" +
			"R = Part* & eof\n" +
			"parse R\n").Found);

		Assert.Equal("R", told.Rule.Name);
	}

	/// <summary>Only the innermost of a nest is told: the one around it is ambiguous because of it.</summary>
	[Fact]
	public void The_innermost_of_a_nest_is_told()
	{
		var told = Assert.Single(Analyse("R = (('a' | 'a' & 'a')+)* & 'b'\nparse R\n").Found);

		Assert.Equal("('a' | \"aa\")+", told.Loop.ToString());
	}

	[Theory]

	// Braces commit what a turn, or the whole run, read.
	[InlineData("R = { ('x'+)* } & 'b'")]
	[InlineData("R = ({ 'x'+ })* & 'b'")]
	[InlineData("R = { ('a' | 'a' & 'a')+ } & 'b'")]

	// Alternatives that begin differently.
	[InlineData("R = (',' & ['a'..'z'] | ';' & ['0'..'9'])* & eof")]
	[InlineData("R = ['a'..'z'] & (',' & ['a'..'z']+)* & eof")]

	// Two repetitions one after the other cut a run in as many ways as it is long, which is
	// polynomial: another diagnostic's business.
	[InlineData("R = 'a'* & 'a'* & 'b'")]

	// A run pinned at the end of each turn.
	[InlineData("R = ('_'* & ['0'..'9'])* & eof")]

	// Alternatives of one character each are one class by the time the graph is built.
	[InlineData("R = ('x' | 'x')* & 'b'")]
	[InlineData("R = (['a'..'z'] | ['a'..'f'])* & ';'")]
	public void A_repetition_that_cuts_one_way_is_not_told(string rules)
	{
		Assert.Empty(Analyse(rules + "\nparse R\n").Found);
	}

	/// <summary>
	/// A branch that can never be finished reads its beginning a second way, but never gets to
	/// the next turn by it, so a run is still cut one way.
	/// </summary>
	[Fact]
	public void A_branch_that_cannot_be_finished_is_not_a_second_reading()
	{
		Assert.Empty(Analyse("R = ('x' & 'y' | 'x' & 'y' & ['b'..'a'])* & 'b'\nparse R\n").Found);
	}

	/// <summary>
	/// A nest whose outer turns can never be finished: the outer is trimmed to nothing, and the
	/// inner one, whose own turns can, is told by itself. A refusal of it is exponential all the
	/// same, since the backtracking tries every reading before it learns the turn cannot end.
	/// </summary>
	[Fact]
	public void The_inner_repetition_of_a_dead_turn_is_told_by_itself()
	{
		var told = Assert.Single(Analyse("R = ((\"xy\" | \"xy\")+ & ['b'..'a'] | 'z')* & eof\nparse R\n").Found);

		Assert.Equal("(\"xy\" | \"xy\")+", told.Loop.ToString());
	}

	/// <summary>The trivia woven between operands is GRAM4051's: it reads nothing here.</summary>
	[Fact]
	public void A_trivia_seam_is_not_told()
	{
		var result = Analyse(
			"trivia = ' '*\n" +
			"W = ['a'..'z']+\n" +
			"R = (W & ',')+ & eof\n" +
			"parse R\n");

		Assert.Empty(result.Found);
	}

	const string Split =
		"trivia = { ' '* }\n" +
		"namespace Token { trivia = none\n Id = ['a'..'z']+ }\n";

	/// <summary>Over kinds a rule marked <c>?</c> gives back what it read, and is asked.</summary>
	[Fact]
	public void A_rule_that_gives_back_is_told_over_kinds()
	{
		var told = Assert.Single(Analyse(
			Split +
			"R? = (Token.Id | Token.Id & Token.Id)+ & ';'\n" +
			"Start = R & eof\n" +
			"parse Start\n",
			lexical: true).Found);

		Assert.Equal("R", told.Rule.Name);
	}

	/// <summary>Over kinds a rule not marked <c>?</c> answers once: its repetitions are as if braced.</summary>
	[Fact]
	public void A_rule_that_answers_once_is_not_told_over_kinds()
	{
		Assert.Empty(Analyse(
			Split +
			"R = (Token.Id | Token.Id & Token.Id)+ & ';'\n" +
			"Start = R & eof\n" +
			"parse Start\n",
			lexical: true).Found);
	}

	/// <summary>
	/// The same shape in a recursive rule over kinds, which a reader remembers the failure of at
	/// a token: it answers once anyway, and nothing is told.
	/// </summary>
	[Fact]
	public void A_memoised_recursive_rule_over_kinds_is_not_told()
	{
		Assert.Empty(Analyse(
			Split +
			"R = (Token.Id | Token.Id & Token.Id)+ & (';' | '(' & R & ')')\n" +
			"Start = R & eof\n" +
			"parse Start\n",
			lexical: true).Found);
	}

	/// <summary>The lexer's terminals are read by its automaton and give nothing back.</summary>
	[Fact]
	public void A_lexer_terminal_is_not_told()
	{
		Assert.Empty(Analyse(
			"trivia = { ' '* }\n" +
			"namespace Token { trivia = none\n Id = (['a'..'z']+)+ }\n" +
			"Start = Token.Id+ & eof\n" +
			"parse Start\n",
			lexical: true).Found);
	}

	/// <summary>
	/// A miss, pinned: a call to a recursive rule is one opaque letter, which overlaps only
	/// itself, so a turn that reads the same text through it and through a character is not
	/// seen. Not told is not proved safe.
	/// </summary>
	[Fact]
	public void A_recursive_call_reading_the_same_text_is_a_miss()
	{
		var result = Analyse(
			"X = 'a' | '(' & X & ')'\n" +
			"R = (X | 'a')+ & 'b'\n" +
			"parse R\n");

		Assert.Empty(result.Found);
		Assert.True(result.Opaque > 0);
	}

	/// <summary>A call past the inlining budget is opaque and counted, and the repetition is still asked.</summary>
	[Fact]
	public void A_call_past_the_inlining_budget_is_opaque()
	{
		var big = "Big = " + string.Join(" | ", Enumerable.Range(0, 300).Select(static i => $"\"k{i}\"")) + "\n";

		var result = Analyse(big + "R = (Big | ',')* & eof\nparse R\n");

		Assert.Empty(result.Found);
		Assert.True(result.Opaque > 0);
	}

	/// <summary>The work done grows with the grammar, and is counted for a test to hold it to.</summary>
	[Fact]
	public void The_work_is_counted()
	{
		var graph = Graph("R = ('a' | 'a' & 'a')+ & 'b'\nparse R\n", lexical: false, out var lexical);

		ExponentialRepetition.Check(graph, lexical, out var steps);

		Assert.True(steps > 0);
	}

	internal static ExponentialRepetition.Result Analyse(string grammar, bool lexical = false)
	{
		var graph = Graph(grammar, lexical, out var split);

		return ExponentialRepetition.Check(graph, split);
	}

	/// <summary>The graph the compiler builds, and the grammar cut in two where that was asked for and done.</summary>
	internal static RecognitionGraph Graph(string grammar, bool lexical, out LexicalSplit? split)
	{
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);

		Assert.DoesNotContain(parsed.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var model = GrammarBinder.Bind(parsed.File, null, grammar.Length);
		var graph = GrammarNormalizer.Normalize(model, scanner: RoslynCSharpScanner.Instance);

		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		split = lexical ? LexicalSplit.Of(graph) : null;

		if (lexical)
			Assert.NotNull(split);

		return graph;
	}
}
