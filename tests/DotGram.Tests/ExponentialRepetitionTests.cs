using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

	// One character read by either of two rules is two readings where it is not the whole turn.
	[InlineData("X = 'x'\nY = 'x'\nR = ((X | Y) & 'z')* & 'b'", "'x' 'z'")]

	// Trivia the author joined with `~` is no seam, and is read as any call is.
	[InlineData("trivia = 'a'*\nR = (trivia ~ 'a')* ~ 'b'", "'a' 'a'")]

	// Braces that never succeed never commit: a count the input sets can fail after any reading.
	[InlineData("R = $q:{ 'q'+ } ~ ':' ~ { ('a' | 'a' & 'a'){#q,} } ~ 'b'", "'a' 'a'")]

	// What follows a repetition inside braces or a look can fail, and asks for another reading.
	[InlineData("R = { (('a' | 'a' & 'a')+ & 'c')* } & 'b'", "'a' 'a'")]
	[InlineData("R = (?=(('a' | 'a' & 'a')+ & 'c')) & 'a'+ & 'b'", "'a' 'a'")]

	// A line end read whole or as two: the shape of four examples' blank lines.
	[InlineData("R = ([' ']* & eol)* & '!'", "'\\r' '\\n'")]
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

	/// <summary>
	/// An address's words without the braces the library puts on them: folding white space and
	/// comments optional on both sides of a word that a phrase repeats.
	/// </summary>
	[Fact]
	public void The_unbraced_address_word_is_told()
	{
		var told = Assert.Single(Analyse(Address(braced: false)).Found);

		Assert.Equal("ObsPhrase", told.Rule.Name);
		Assert.Equal("AtomText {…} AtomText", told.Witness);
	}

	/// <summary>The same words as the library writes them, sealed: nothing is told.</summary>
	[Fact]
	public void The_braced_address_word_is_not_told()
	{
		Assert.Empty(Analyse(Address(braced: true)).Found);
	}

	/// <summary>A copy of the address phrase, its word braced as the library braces it or not.</summary>
	internal static string Address(bool braced)
	{
		var word = braced ? "{ Cfws? & (AtomText | QuotedBody) & Cfws? }" : "Cfws? & (AtomText | QuotedBody) & Cfws?";

		return
			"Fws        = [' ' | '\\t']+\n" +
			"Comment    = '(' & { [^ ')']* } & ')'\n" +
			"Cfws       = { (Fws? & Comment)+ & Fws? } | Fws\n" +
			"AtomText   = { ['a'..'z' | 'A'..'Z' | '0'..'9' | '!' | '#']+ }\n" +
			"QuotedBody = '\"' & { [^ '\"']* } & '\"'\n" +
			$"WordText   = {word}\n" +
			"ObsPhrase  = WordText & (WordText | { '.' & Cfws? })*\n" +
			"R          = ObsPhrase & '<' & ['a'..'z']+ & '>'\n" +
			"parse R\n";
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

	// A look in front of a class takes its characters out: the library's quoted run.
	[InlineData("R = ('\\'' & ?!'\\'' & any)* & eof")]
	[InlineData("R = (\"''\" & (?!'\\'' & any)*)* & eof")]

	// A look that ends a way lets nothing it refuses follow on that way.
	[InlineData("R = ('\"' & ['a'..'z']* & ('\"' | eof))* & ';'")]
	[InlineData("R = (['a'..'z']+ & ':' & [^ '\\n']* & ('\\n' | ?=eof))* & eof")]

	// A braced run read to its end leaves nothing of its class to what follows: a comment to the
	// end of the line, written either way, ends a turn that cannot go on.
	[InlineData("C = ';' & { [^ '\\n']* }\nT = ' '* & C | C\nR = ('[' & ['a'..'z'] & ']' & T?)* & eof")]

	// A repetition that recovers commits every turn it finishes.
	[InlineData("R = ('x' & 'y' | 'x' & 'y')* recover ';' & eof")]

	// Nothing after a repetition inside braces or a look can fail, so its first reading stands,
	// through the repetition around it.
	[InlineData("R = { (('a' | 'a' & 'a')+)* } & 'b'")]
	[InlineData("R = (?=(('a' | 'a' & 'a')+)) & 'a'+ & 'b'")]

	// A rule that cannot fail, called twice after the run: neither call can ask for another reading.
	[InlineData("Tail = 'b'?\nPair = Tail & Tail\nR = { ('a' | 'a' & 'a')+ & Pair } & 'c'")]

	// A braced call that takes the whole run leaves nothing it begins with to a second call.
	[InlineData("X = { 'a'+ }\nR = (X | X & X)+ & 'b'")]

	// A turn of one character, whichever alternative reads it, is one test in the parser: the
	// emitter leaves nothing to come back to, and the analysis asks it the same question.
	[InlineData("X = 'x'\nY = 'x'\nR = (X | Y)* & 'b'")]
	[InlineData("X = 'x'\nY = ['x'..'y']\nR = (X | Y)+ & 'b'")]
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

	/// <summary>
	/// A false report, pinned: a look of more than one character is taken as reading nothing, so
	/// the second reading it removes is still seen. The warning there is wrong, and is silenced.
	/// </summary>
	[Fact]
	public void A_look_of_more_than_one_character_is_a_false_report()
	{
		var told = Assert.Single(Analyse("R = (?!\"aa\" & 'a' | \"aa\")+ & 'b'\nparse R\n").Found);

		Assert.Equal("'a' 'a'", told.Witness);
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

	/// <summary>
	/// The work of building a repetition's automaton is bounded while it is built, not after, and
	/// counted: a nest of written-out counts is given up once it has more letters than a repetition
	/// may, and a run of looks whose ways to read nothing multiply, two at every look, once its
	/// entries are past the budget — each turn of it is read in two to the sixteen ways, and it is
	/// not told: past the budget is not proof.
	/// </summary>
	[Theory]
	[InlineData("R = ((((((('a'){8}){8}){8}){8}){8}){8})* & 'b'\nparse R\n", 1_000)]
	[InlineData(null, 1 << 16)]
	public void The_work_of_building_a_repetition_is_bounded_and_counted(string? grammar, int least)
	{
		grammar ??= "R = ('x' & " +
			string.Join(" & ", Enumerable.Range(0, 16).Select(static i => $"(?!'{(char)('a' + i)}' | ?!'{(char)('A' + i)}')")) +
			" & 'y')* & 'z'\nparse R\n";

		var graph = Graph(grammar, lexical: false, out var split);
		var watch = Stopwatch.StartNew();
		var bytes = GC.GetAllocatedBytesForCurrentThread();

		var result = ExponentialRepetition.Check(graph, split, out var steps);

		bytes = GC.GetAllocatedBytesForCurrentThread() - bytes;

		Assert.Equal(1, result.Skipped);
		Assert.Empty(result.Found);
		Assert.True(bytes < 16 << 20, $"{bytes} bytes allocated");
		Assert.True(watch.Elapsed.TotalSeconds < 2, $"{watch.Elapsed.TotalMilliseconds:F0} ms");
		Assert.True(steps >= least, $"{steps} steps: the work done is counted");
		TestContext.Current.SendDiagnosticMessage($"{steps} steps, {bytes} bytes, {watch.Elapsed.TotalMilliseconds:F1} ms");
	}

	/// <summary>The work done grows with the grammar, and is counted for a test to hold it to.</summary>
	[Fact]
	public void The_work_is_counted()
	{
		var graph = Graph("R = ('a' | 'a' & 'a')+ & 'b'\nparse R\n", lexical: false, out var lexical);

		ExponentialRepetition.Check(graph, lexical, out var steps);

		Assert.True(steps > 0);
	}

	/// <summary>
	/// The warning: at the rule, as the author named it, with the repetition and the shortest text
	/// it reads two ways, and once for a repetition however many nest around it.
	/// </summary>
	[Fact]
	public void The_warning_is_said_at_the_rule_once_a_repetition()
	{
		const string grammar = "S = 'x'\nRun = (('a' | 'a' & 'a')+)* & 'b'\nparse Run\n";

		var told = Assert.Single(Compiled(grammar), static one => one.Id == GramCompiler.ExponentialRepetitions);

		Assert.Equal(GramSeverity.Warning, told.Severity);
		Assert.Equal("Run = (('a' | 'a' & 'a')+)* & 'b'", grammar.Substring(told.Position, told.Length));
		Assert.Equal(
			"The repetition '('a' | \"aa\")+' in 'Run' can read 'a' 'a' in two ways, so a refused input is read again " +
			"for every way of cutting it: exponentially many times (docs/syntax.md §3.3).",
			told.Message);
	}

	/// <summary>Two repetitions written alike in one rule are two repetitions, and two warnings.</summary>
	[Fact]
	public void Repetitions_written_alike_are_told_each()
	{
		const string grammar = "R = ('a' | 'a' & 'a')+ & ',' & ('a' | 'a' & 'a')+ & ';'\nparse R\n";

		var result = Analyse(grammar);

		Assert.Equal(2, result.Loops);
		Assert.Equal(2, result.Found.Count);
		Assert.Equal(2, Compiled(grammar).Count(static one => one.Id == GramCompiler.ExponentialRepetitions));
	}

	/// <summary>
	/// Grammars of deep and wide call graphs, each asked on a thread with the smallest stack a
	/// process starts with, a megabyte (<see cref="DeepCallGraphs"/>), at depths past every limit
	/// the walks have: the chains are three hundred calls, past the sixty-four a call is written out
	/// to; the slow suite asks them eight thousand deep.
	/// </summary>
	[Theory]
	[InlineData("look over a diamond", 1, 0, 0)]
	[InlineData("look over a diamond, then a run read two ways", 1, 1, 0)]
	[InlineData("chain of guards", 1, 0, 0)]
	[InlineData("chain of guards, then a run read two ways", 2, 1, 0)]
	[InlineData("chain of aliases repeated", 1, 0, 0)]
	[InlineData("diamond of aliases repeated", 1, 0, 0)]
	[InlineData("braces before a diamond under aliases", 1, 1, 0)]
	public void A_deep_or_wide_call_graph_is_asked_in_bounded_work(string shape, int loops, int told, int skipped)
	{
		DeepCallGraphs.Ask(shape, 300, loops, told, skipped);
	}

	internal static void OnThread(Action run, int stack)
	{
		DeepCallGraphs.OnThread(run, stack);
	}

	/// <summary>
	/// A clone keeps the numbers its rule's repetitions are written with, though a rebinding makes
	/// one of them free there: each repetition told is told once, with its own witness.
	/// </summary>
	[Fact]
	public void A_clone_keeps_the_occurrences_of_its_rule()
	{
		var found = Analyse(
			"X = 'x'\nE = none\nY = 'b' | \"bb\"\nOne = 'b'\nZ = 'c'\nTwo = 'c' | \"cc\"\n" +
			"Q = { 'a'+ & X } & Y+ & ',' & Z+ & '!'\nparse Q\nparse Q with (X = E, Y = One, Z = Two) as Q2\n").Found;

		Assert.Equal(["'b' 'b'", "'c' 'c'"], found.Select(static one => one.Witness).Order(StringComparer.Ordinal));
	}

	/// <summary>A witness is a text the repetition can read: as many turns as it must take.</summary>
	[Theory]
	[InlineData("R = (\"xy\" | \"xy\"){2,} & 'b'", "'x' 'y' 'x' 'y'")]
	[InlineData("R = (\"xy\" | \"xy\"){3,} & 'b'", "'x' 'y' 'x' 'y' 'x' 'y'")]
	[InlineData("R = (\"xy\" | \"xy\"){1,} & 'b'", "'x' 'y'")]
	public void A_witness_takes_the_turns_the_repetition_must(string grammar, string witness)
	{
		Assert.Equal(witness, Assert.Single(Analyse(grammar + "\nparse R\n").Found).Witness);
	}

	/// <summary>
	/// A run of looks over a chain of aliases is charged for every alias it looks through and every
	/// operand it passes over.
	/// </summary>
	[Fact]
	public void Looks_through_aliases_are_charged()
	{
		var grammar = new StringBuilder("A0 = 'a'\n");

		for (var i = 1; i <= 400; i++)
			grammar.Append($"A{i} = A{i - 1}\n");

		grammar.Append("R = (").Append(string.Concat(Enumerable.Repeat("?=A400 & ", 200))).Append("'a')* & 'b'\nparse R\n");

		var graph  = Graph(grammar.ToString(), lexical: false, out var split);
		var result = ExponentialRepetition.Check(graph, split, out var steps);

		Assert.Equal(1, result.Loops);
		Assert.True(steps >= 200 * 400 || result.Skipped == 1, $"{steps} steps, {result.Skipped} skipped");
	}

	/// <summary>
	/// Calls are written out sixty-four deep and no deeper: past that a call is one opaque step,
	/// and a run it reads two ways is not seen. Pinned on both sides of the depth.
	/// </summary>
	[Theory]
	[InlineData(60, 1)]
	[InlineData(65, 0)]
	public void Calls_past_the_depth_are_one_step(int depth, int told)
	{
		var grammar = new StringBuilder("A0 = 'a' | 'a' & 'a'\n");

		for (var i = 1; i <= depth; i++)
			grammar.Append($"A{i} = when @(true) & A{i - 1}\n");

		grammar.Append($"R = A{depth}+ & 'b'\nparse R\n");

		Assert.Equal(told, Analyse(grammar.ToString()).Found.Count);
	}

	/// <summary>
	/// A repetition in a rule lifted out of a marked one is an occurrence of its own; a clone of
	/// the rule it stands in is not.
	/// </summary>
	[Theory]
	[InlineData("R = ((('a' | \"aa\")+ & ',')* recover ',' & (('x' | \"xx\")+ & ';')* recover ';' & '\\n')* recover '\\n'\nparse R\n", 2)]
	[InlineData(
		"X = 'x'\nY = 'y'\nQ = ((('a' | \"aa\")+ & X)* recover ',' & '\\n')* recover '\\n'\nparse Q\nparse Q with (X = Y) as Q2\n", 1)]
	public void Each_occurrence_is_told_once(string grammar, int told)
	{
		var graph = Graph(grammar, lexical: false, out var split);
		var found = ExponentialRepetition.Check(graph, split).Found;

		Assert.True(
			told == found.Count,
			string.Join("; ", found.Select(static one => $"{one.Rule.Name}: {one.Loop}")) + " | " +
			string.Join("; ", graph.Rules.Where(one => graph.Bodies.ContainsKey(one)).Select(one =>
				$"{one.Name}{(graph.Lifted.TryGetValue(one, out var lift) ? $" <- {lift.Parent.Name}/{lift.Origin.Name}" : "")}")));
	}

	/// <summary>Two repetitions of one rule are two warnings.</summary>
	[Fact]
	public void Each_repetition_is_said_once()
	{
		var told = Compiled("Run = ('x'+)* & ',' & ('y'+)+ & 'b'\nparse Run\n")
			.Where(static one => one.Id == GramCompiler.ExponentialRepetitions)
			.Select(static one => one.Message)
			.ToArray();

		Assert.Equal(2, told.Length);
		Assert.Contains("'x' 'x'", told[0], StringComparison.Ordinal);
		Assert.Contains("'y' 'y'", told[1], StringComparison.Ordinal);
	}

	/// <summary>
	/// A repetition of an included grammar is that grammar's to be told about; a repetition of the
	/// author's own over a rule it includes is the author's.
	/// </summary>
	[Fact]
	public void A_repetition_of_an_included_grammar_is_not_said_where_it_is_included()
	{
		const string own      = "using L;\nRun = (Word | Word & Word)+ & ';' & Runs\nparse Run\n";
		const string included = "Word = ['a'..'z']\nRuns = ('x'+)* & 'b'\n";

		var (joined, map) = GrammarSplice.Join(new GrammarSplice.Part(own, null, null), [new GrammarSplice.Part(included, "L", null)]);

		var told = GramCompiler.Compile(joined, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
			Own           = own.Length,
			Cuts          = GrammarSplice.Cuts(joined, map.Segments.Skip(1).Select(static one => (one.Start, one.Length))),
		}).Diagnostics.Where(static one => one.Id == GramCompiler.ExponentialRepetitions).ToArray();

		Assert.Contains("in 'Run'", Assert.Single(told).Message, StringComparison.Ordinal);
	}

	/// <summary>Over kinds the warning is said too, in a rule that gives back.</summary>
	[Fact]
	public void The_warning_is_said_over_kinds()
	{
		var told = GramCompiler.Compile(
			Split + "R? = (Token.Id | Token.Id & Token.Id)+ & ';'\nStart = R & eof\nparse Start\n",
			new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance, Lexical = true }).Diagnostics;

		Assert.Contains("in 'R'", Assert.Single(told, static one => one.Id == GramCompiler.ExponentialRepetitions).Message, StringComparison.Ordinal);
	}

	static IReadOnlyList<GramDiagnostic> Compiled(string grammar)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;
	}

	/// <summary>
	/// Every grammar in the repository, asked: what is told, where, with what witness, and how
	/// much of each grammar the analysis could not see.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A report and a guard: it writes <c>ExponentialRepetitions.txt</c> beside the assembly and
	/// to the output, and holds what is told to <see cref="Triaged"/>. A hit is either a grammar
	/// that a refused input can drive exponentially, which is a defect of that grammar to mend, or
	/// one that is told and cannot be, judged by driving it and listed there with why.
	/// </para>
	/// <para>
	/// The grammars are the files and every <c>[Gram]</c> attribute, however its text is written,
	/// each joined with what it includes and read the way it is compiled: over kinds where some
	/// attribute or snapshot rendering asks for <c>Lexical</c>, over characters where some use
	/// does not.
	/// </para>
	/// </remarks>
	[Fact]
	public void Corpus()
	{
		var root   = SourceRoot.Directory;
		var report = new StringBuilder();
		var hits   = 0;
		var read   = 0;
		var found  = new List<string>();

		// Each grammar and way of reading it on its own, in parallel, and the report put together in
		// the order they were found: what each says does not depend on the others.
		var jobs  = Grammars(root).SelectMany(static grammar => grammar.Modes.Select(lexical => (Grammar: grammar, Lexical: lexical))).ToArray();
		var parts = new (string Report, int Hits, bool Read, List<string> Found)[jobs.Length];

		// The longest first, so that the largest grammar is not the last one begun.
		var order = Enumerable.Range(0, jobs.Length).OrderByDescending(at => jobs[at].Grammar.Text.Length).ToArray();

		System.Threading.Tasks.Parallel.ForEach(
			System.Collections.Concurrent.Partitioner.Create(order, System.Collections.Concurrent.EnumerablePartitionerOptions.NoBuffering),
			at =>
		{
			var (grammar, lexical) = jobs[at];
			var (name, text)       = (grammar.Name, grammar.Text);
			var part               = new StringBuilder();
			var told               = new List<string>();

			RecognitionGraph graph;
			LexicalSplit?    split;
			long             built;

			var watch = Stopwatch.StartNew();

			try
			{
				graph = Built(grammar, lexical, out split);
				built = watch.ElapsedTicks;
			}
			catch (Exception error)
			{
				part.AppendLine($"{name} [{(lexical ? "kinds" : "chars")}]: not read ({error.GetType().Name}: {error.Message.Split('\n')[0]})");
				parts[at] = (part.ToString(), 0, false, told);

				return;
			}

			watch.Restart();

			var result   = ExponentialRepetition.Check(graph, split, out var steps);
			var checking = watch.ElapsedTicks;

			var mode = (lexical ? split is null ? "kinds asked, read over chars" : "kinds" : "chars") +
				(grammar.Included.Count == 0 ? "" : ", with " + string.Join(", ", grammar.Included.Select(static one => one.Name)));

			part.AppendLine(string.Create(
				System.Globalization.CultureInfo.InvariantCulture,
				$"{name} [{mode}]: {result.Loops} loops, {result.Skipped} past the budget, {result.Opaque} opaque letters, " +
				$"{result.Found.Count} told; {steps} steps, {Milliseconds(checking):F1} ms against {Milliseconds(built):F1} ms to build the graph"));

			foreach (var one in result.Found)
			{
				var line = one.Rule.Declaration is { } declaration
					? 1 + text.Take(Math.Min(declaration.At.Position, text.Length)).Count(static c => c == '\n')
					: 0;

				part.AppendLine($"  HIT {one.Rule.Name} (line {line}): {Shortened(one.Loop.ToString())} reads {one.Witness} in two ways");
				told.Add($"{name.Split(':')[0]} [{mode}] {one.Rule.Name}: {one.Loop}");
			}

			parts[at] = (part.ToString(), result.Found.Count, true, told);
		});

		foreach (var part in parts)
		{
			report.Append(part.Report);
			hits += part.Hits;
			read += part.Read ? 1 : 0;
			found.AddRange(part.Found);
		}

		report.Insert(0, $"{read} grammars read, {hits} hits.\n");

		var written = report.ToString();

		File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "ExponentialRepetitions.txt"), written);
		TestContext.Current.SendDiagnosticMessage(written);
		Console.WriteLine(written);

		Assert.True(read > 300, $"Only {read} grammars were read; the walk over the repository is broken.");
		// Each repetition told, as often as it is told: a second one in a rule already listed is a
		// new hit, and so is the same one read a second way.
		Assert.Equal(Triaged.Select(static one => one.Repetition).Order(StringComparer.Ordinal), found.Order(StringComparer.Ordinal));
	}

	/// <summary>The repetitions of the repository that are told, each with why it is not mended.</summary>
	static readonly (string Repetition, string Why)[] Triaged =
	[
		// Every argument is a value where nothing else reads it, `Value = any*`: every list is
		// accepted, and nothing is refused after the two readings of `-o`. The snapshot beside it
		// was written from this text.
		("Segmented.gram [chars] Args: items: Arg*", "accepts every input"),

		// The lookahead in front of the decimal refuses what `Long` would read, which a look of
		// more than one character cannot show the analysis; the example silences it, with why.
		("MetricsLineExample.cs [chars] Line: (trivia & item0: Reading)+", "a look of more than one character"),
	];

	static double Milliseconds(long ticks)
	{
		return ticks * 1000.0 / Stopwatch.Frequency;
	}

	static string Shortened(string text)
	{
		const int most = 100;

		return text.Length <= most ? text : text.Substring(0, most) + " …";
	}

	/// <summary>The graph a grammar compiles to, with the grammars it includes spliced on; the library itself is read as it is, with nothing spliced onto it.</summary>
	static RecognitionGraph Built(CorpusGrammar grammar, bool lexical, out LexicalSplit? split)
	{
		var text = grammar.Text;

		DotGram.Grammar.Parsing.ParseResult parsed;

		if (grammar.Name == "Std.gram")
			parsed = DotGram.Grammar.Parsing.GramParser.Parse(DotGram.Grammar.Parsing.GramLexer.Tokenize(text, RoslynCSharpScanner.Instance));
		else if (grammar.Included.Count == 0)
			parsed = GramCompiler.Read(text, RoslynCSharpScanner.Instance);
		else
		{
			// Joined as the generator joins them: the author's text first and untouched, each
			// included one wrapped in a namespace of its name.
			var (joined, map) = GrammarSplice.Join(
				new GrammarSplice.Part(text, null, null),
				[.. grammar.Included.Select(static one => new GrammarSplice.Part(one.Text, one.Name, null))]);

			parsed = GramCompiler.Read(
				joined, RoslynCSharpScanner.Instance, text.Length,
				GrammarSplice.Cuts(joined, map.Segments.Skip(1).Select(static one => (one.Start, one.Length))));

			text = joined;
		}

		var model = GrammarBinder.Bind(parsed.File, null, grammar.Text.Length);
		var graph = GrammarNormalizer.Normalize(model, scanner: RoslynCSharpScanner.Instance);

		split = lexical ? LexicalSplit.Of(graph) : null;

		return graph;
	}

	/// <summary>A grammar of the repository: what the report calls it, its own text, what it includes, and the ways it is read.</summary>
	/// <param name="Included">Every grammar it is built on, each once, with the namespace it is reached under.</param>
	/// <param name="Modes">Over characters (false), over kinds (true), or both.</param>
	sealed record CorpusGrammar(string Name, string Text, IReadOnlyList<(string Name, string Text)> Included, bool[] Modes);

	/// <summary>A class that carries a <c>[Gram]</c>, as its source writes it.</summary>
	/// <param name="Where">Where it was written: the file, and the line of the attribute.</param>
	/// <param name="Scope">The file, and the string it was written in where it is a test's source: where a name it includes is looked for first.</param>
	/// <param name="Text">The grammar written into the attribute, or null where it names a file.</param>
	/// <param name="File">The file it names, or null.</param>
	/// <param name="Bases">The classes it derives from and the classes it includes, each with the <c>As</c> it is included under.</param>
	sealed record Host(
		string Where, string Scope, string Directory, string Class, string? Text, string? File, bool Lexical, bool Primary,
		string? IncludedAs, IReadOnlyList<(string Class, string? As)> Bases);

	/// <summary>
	/// Every grammar in the repository, with what it includes and the ways it is read: over
	/// characters, over kinds, or both.
	/// </summary>
	/// <remarks>
	/// The sources are read as C#, so that every way of writing the attribute's text is read the
	/// way the compiler reads it — a raw string, a quoted one with its escapes, a verbatim one —
	/// and a test's source written into a string of its own is read again as C#, its grammars
	/// with it. A class's grammar is joined with what it is built on, by <c>[GramInclude]</c> or
	/// by its base, found by class name: in the same string or file first, then anywhere.
	/// </remarks>
	static IEnumerable<CorpusGrammar> Grammars(string root)
	{
		var hosts = new List<Host>();

		// Each file read on its own, in parallel, and its hosts put together in the order of the files.
		var sources = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
			.Where(path => !Scratch(root, path))
			.OrderBy(static path => path, StringComparer.Ordinal)
			.ToArray();
		var found   = new List<Host>[sources.Length];

		System.Threading.Tasks.Parallel.For(0, sources.Length, at =>
		{
			var path   = sources[at];
			var source = File.ReadAllText(path);
			var mine   = new List<Host>();

			if (source.Contains("Gram", StringComparison.Ordinal))
				Hosts(CSharpSyntaxTree.ParseText(source).GetRoot(), Path.GetFileName(path), Path.GetDirectoryName(path)!, 0, path, mine, 0);

			found[at] = mine;
		});

		foreach (var mine in found)
			hosts.AddRange(mine);

		var files = new Dictionary<string, List<string>>(StringComparer.Ordinal);

		foreach (var path in Directory.EnumerateFiles(root, "*.gram", SearchOption.AllDirectories))
		{
			if (Scratch(root, path))
				continue;

			var name = Path.GetFileName(path);

			if (!files.TryGetValue(name, out var paths))
				files[name] = paths = [];

			paths.Add(path);
		}

		// What a host's grammar is: the text written into it, or the file it names, beside it where there is one.
		string? TextOf(Host host)
		{
			if (host.Text is not null)
				return host.Text;

			if (host.File is null || !files.TryGetValue(Path.GetFileName(host.File), out var paths))
				return null;

			var near = paths.FirstOrDefault(one => string.Equals(Path.GetDirectoryName(one), host.Directory, StringComparison.Ordinal));

			return File.ReadAllText(near ?? paths[0]);
		}

		// The host a name it includes means: in the same string or file first, then anywhere.
		Host? Named(string name, Host from)
		{
			var all = hosts.Where(one => one.Class == name && one.Primary).ToList();

			return all.FirstOrDefault(one => one.Scope == from.Scope) ??
				all.FirstOrDefault(one => one.Where.Split(':')[0] == from.Where.Split(':')[0]) ??
				(all.Count == 1 ? all[0] : null);
		}

		// Everything a host is built on, each grammar once however many paths lead to it.
		List<(string Name, string Text)> Included(Host host)
		{
			var included = new List<(string, string)>();
			var seen     = new HashSet<Host> { host };
			var pending  = new Queue<(Host Host, string? As)>();

			foreach (var (name, called) in host.Bases)
				if (Named(name, host) is { } found)
					pending.Enqueue((found, called));

			while (pending.Count > 0)
			{
				var (one, called) = pending.Dequeue();

				if (!seen.Add(one) || TextOf(one) is not { } text)
					continue;

				included.Add((called ?? one.IncludedAs ?? one.Class, text));

				foreach (var (name, further) in one.Bases)
					if (Named(name, one) is { } found)
						pending.Enqueue((found, further));
			}

			return included;
		}

		// A file's modes by the attributes that name it: those beside it, where there are any, since
		// that is its host; a test that writes the name in a string of its own is not.
		foreach (var (name, paths) in files.OrderBy(static one => one.Key, StringComparer.Ordinal))
		{
			foreach (var path in paths)
			{
				var text   = File.ReadAllText(path);
				var naming = hosts.Where(one => one.File is { } file && Path.GetFileName(file) == name).ToList();
				var beside = naming.Where(one => string.Equals(one.Directory, Path.GetDirectoryName(path), StringComparison.Ordinal)).ToList();
				var owners = beside.Count > 0 ? beside : naming;
				var modes  = new HashSet<bool>(owners.Select(static one => one.Lexical));

				if (Path.GetFileName(Path.GetDirectoryName(path)) == "Snapshots")
					foreach (var (options, _) in SnapshotRenderings.Of(Path.GetFileNameWithoutExtension(path), text, EmittedCode.References))
						modes.Add(options.Lexical);

				if (modes.Count == 0)
					modes.Add(false);

				var host = beside.FirstOrDefault(static one => one.Primary) ?? beside.FirstOrDefault();

				yield return new CorpusGrammar(name, text, host is null ? [] : Included(host), [.. modes.OrderBy(static one => one)]);
			}
		}

		foreach (var host in hosts)
			if (host.Text is { } text)
				yield return new CorpusGrammar(host.Where, text, Included(host), [host.Lexical]);
	}

	/// <summary>Every class in a tree that carries a <c>[Gram]</c>, and those of any source written into a string in it.</summary>
	static void Hosts(SyntaxNode root, string file, string directory, int lines, string scope, List<Host> hosts, int depth)
	{
		foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
		{
			var attributes = type.AttributeLists.SelectMany(static list => list.Attributes).ToList();
			var bases      = new List<(string, string?)>();

			foreach (var above in type.BaseList?.Types ?? default)
				bases.Add((Simple(above.Type), null));

			foreach (var include in attributes.Where(static one => Is(one, "GramInclude")))
				if (include.ArgumentList?.Arguments.FirstOrDefault()?.Expression is TypeOfExpressionSyntax named)
					bases.Add((Simple(named.Type), Argument(include, "As")));

			foreach (var gram in attributes.Where(static one => Is(one, "Gram")))
			{
				if (gram.ArgumentList?.Arguments.FirstOrDefault() is not { NameEquals: null, NameColon: null } first ||
					Constant(first.Expression) is not { } value)
					continue;

				var line  = lines + 1 + gram.GetLocation().GetLineSpan().StartLinePosition.Line;
				var named = value.EndsWith(".gram", StringComparison.Ordinal) && !value.Contains('\n');

				hosts.Add(new Host(
					$"{file}:{line}", scope, directory, type.Identifier.ValueText,
					named ? null : value, named ? value : null,
					Argument(gram, "Lexical") == "true", Argument(gram, "Suffix") is null,
					Argument(gram, "IncludedAs"), bases));
			}
		}

		// A test's source written into a string, read again as C#; in an interpolated one, what is
		// put in is left out, so that an option a test varies reads as its default.
		if (depth < 3)
			foreach (var node in root.DescendantNodes())
			{
				var written = node switch
				{
					LiteralExpressionSyntax { Token.Value: string text } => text,
					InterpolatedStringExpressionSyntax interpolated => string.Concat(interpolated.Contents.Select(static part =>
						part is InterpolatedStringTextSyntax text ? text.TextToken.ValueText : "")),
					_ => null,
				};

				if (written is null || !written.Contains("Gram(", StringComparison.Ordinal) || !written.Contains("class ", StringComparison.Ordinal))
					continue;

				// A raw string of many lines begins on the line after its quotes.
				var at = lines + node.GetLocation().GetLineSpan().StartLinePosition.Line +
					(node.ToString().TrimStart('$').StartsWith("\"\"\"", StringComparison.Ordinal) ? 1 : 0);

				Hosts(CSharpSyntaxTree.ParseText(written).GetRoot(), file, directory, at, $"{scope}:{at}", hosts, depth + 1);
			}

		// A string as the compiler reads it, where it is a constant: a literal, or literals joined with `+`.
		static string? Constant(ExpressionSyntax expression)
		{
			return expression switch
			{
				LiteralExpressionSyntax { Token.Value: string text } => text,
				ParenthesizedExpressionSyntax inner => Constant(inner.Expression),
				BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } joined =>
					Constant(joined.Left) is { } left && Constant(joined.Right) is { } right ? left + right : null,
				_ => null,
			};
		}

		static bool Is(AttributeSyntax attribute, string name)
		{
			var simple = Simple(attribute.Name);

			return simple == name || simple == name + "Attribute";
		}

		static string Simple(TypeSyntax type)
		{
			return type switch
			{
				QualifiedNameSyntax qualified => Simple(qualified.Right),
				AliasQualifiedNameSyntax aliased => Simple(aliased.Name),
				GenericNameSyntax generic => generic.Identifier.ValueText,
				SimpleNameSyntax simple => simple.Identifier.ValueText,
				_ => type.ToString(),
			};
		}

		// A named argument's value as written: a string's value, or the expression's text.
		static string? Argument(AttributeSyntax attribute, string name)
		{
			foreach (var argument in attribute.ArgumentList?.Arguments ?? default)
				if (argument.NameEquals?.Name.Identifier.ValueText == name)
					return argument.Expression is LiteralExpressionSyntax { Token.Value: string text } ? text : argument.Expression.ToString();

			return null;
		}
	}

	/// <summary>Build output and scratch, judged below the root as <c>ExponentialShapeProbe</c> judges it.</summary>
	static bool Scratch(string root, string path)
	{
		var below = path.Substring(root.Length);

		return
			below.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}.work{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}.build{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
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
