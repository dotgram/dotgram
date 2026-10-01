using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A <c>parse</c> read from a position gives the rule's first reading (§6.3, §4), in the engine
/// and in the direct reader, where the grammar is compiled knowing that a parse is also read
/// from a position (<see cref="GramCompilerOptions.PositionalFollow"/>).
/// </summary>
/// <remarks>
/// <para>
/// A rule is compiled once for its whole forms, which demand the end of input, and its
/// positional forms, which demand nothing after it. Compiled against the end alone, a decision
/// taken on its strength — a way back to an optional's skip not kept, a repetition's turn
/// taken as final because the end could not begin where it stopped — refuses or shortens a
/// reading the rule has from a position. Each shape below is one such decision, and each is
/// held to the reference interpreter.
/// </para>
/// <para>
/// The option is off by default. Where the default answers wrongly the case is kept beside the
/// right one as a skipped test, so that the day the default changes, or the analysis learns to
/// tell the two kinds of entry apart, the skip is what is left to remove.
/// </para>
/// </remarks>
public sealed class PositionalFollowTests
{
	const string OffWrong = "Without PositionalFollow the rule is compiled against the end of input that only the whole forms demand, and this positional reading is refused or cut short.";

	/// <summary>The shapes, each with the input and position it was found on.</summary>
	static readonly Dictionary<string, (string Grammar, string Input, int At)> Shapes = new()
	{
		// An optional at the end whose every way fails after reading a character: entered on
		// the `a`, the engine kept no way back to its skip.
		["Optional"] = ("Start = (R1 | R1)?\nR1 = 'a' & 'k'\nparse Start\n", "ab", 0),

		// A repetition whose turns share a first character, then a negative lookahead: the
		// second turn fails after its `a`, and the reading must stop after the first turn.
		["Star"] = ("Start = ('a' & 'b' | 'a' & 'c')* & ?!'x'\nparse Start\n", "abacx", 0),

		// The same with a turn that reads one character or two.
		["StarShort"] = ("Start = ('a' | 'a' & 'c')* & ?!'b'\nparse Start\n", "aab", 0),

		// An optional before a negative lookahead: the lookahead fails after the optional's
		// reading, and the skip must still be there to fall back on.
		["OptionalBeforeLook"] = ("Start = ('a' & 'b' | 'a' & 'c')? & ?!'b'\nparse Start\n", "abb", 0),

		// A quoted string with doubled quotes inside: from a position the reading ends at the
		// first closing quote, which over the end of input could not have been the last.
		["Quoted"] = ("""
			Start = '"' & ("\"\"" | [^'"'])* & '"'
			parse Start

			""", "\"a\"\"x", 0),

		// The shape it was first met in, with trivia and ignore-case literals around it.
		["Fragment"] = ("""
			trivia = { ' '* }
			Start = (R2 | ('b' | 'k' | R1 | R1)? | "AB"i | (?! 'B' & R2 & R2?))
			R1 = R2
			R2 = ('a' & (?! 'b'i & "Ka"i))
			parse Start

			""", "aKK1", 0),
	};

	/// <summary>
	/// What each shape reads from its position, which is what the reference interpreter reads
	/// there — said once so that a test that disagrees with the table shows which is wrong.
	/// </summary>
	[Theory]
	[InlineData("Optional", 0)]
	[InlineData("Star", 2)]
	[InlineData("StarShort", 1)]
	[InlineData("OptionalBeforeLook", 0)]
	[InlineData("Quoted", 3)]
	[InlineData("Fragment", 0)]
	public void The_reference_interpreter_reads_the_first_reading(string shape, int end)
	{
		var (grammar, input, at) = Shapes[shape];

		Assert.Equal(end, Reference(grammar, input, at));
	}

	/// <summary>
	/// With the option, both backends answer as the reference interpreter does: on the input
	/// each shape was found on, and from every position of every short input over its letters.
	/// The whole form is held to it as well, since the option must not change what it accepts.
	/// </summary>
	[Theory]
	[InlineData("Optional", false)]
	[InlineData("Optional", true)]
	[InlineData("Star", false)]
	[InlineData("Star", true)]
	[InlineData("StarShort", false)]
	[InlineData("StarShort", true)]
	[InlineData("OptionalBeforeLook", false)]
	[InlineData("OptionalBeforeLook", true)]
	[InlineData("Quoted", false)]
	[InlineData("Quoted", true)]
	[InlineData("Fragment", false)]
	[InlineData("Fragment", true)]
	public void With_the_option_a_reading_from_a_position_is_the_first_reading(string shape, bool direct)
	{
		var (grammar, input, at) = Shapes[shape];
		var graph    = Graph(grammar);
		var start    = graph.Rules.First(static rule => rule.Name == "Start");
		var assembly = Compiled(grammar, direct, positionalFollow: true);

		Assert.Equal(Expected(graph, start, input, at), Answer(assembly, input, at));

		foreach (var text in Inputs(input.Distinct().ToArray(), 4))
		{
			for (var from = 0; from <= text.Length; from++)
				Assert.True(
					Expected(graph, start, text, from) == Answer(assembly, text, from),
					$"'{text}' from {from}: expected {Expected(graph, start, text, from)}, answered {Answer(assembly, text, from)}");

			Assert.True(
				ReferenceInterpreter.Parses(graph, start, text) == EmittedCode.Match(assembly, "Grammar", "TryParseStart", text).IsSuccess,
				$"whole '{text}'");
		}
	}

	/// <summary>
	/// Without the option: the cells the default answers rightly run, and those it answers
	/// wrongly — refused, or cut short — are kept, skipped, for the day it answers them too.
	/// </summary>
	[Theory]
	[InlineData("Optional", true)]
	[InlineData("StarShort", true)]
	[InlineData("Quoted", false)]
	[InlineData("Fragment", true)]
	[InlineData("Optional", false, Skip = OffWrong)]
	[InlineData("Star", false, Skip = OffWrong)]
	[InlineData("Star", true, Skip = OffWrong)]
	[InlineData("StarShort", false, Skip = OffWrong)]
	[InlineData("OptionalBeforeLook", false, Skip = OffWrong)]
	[InlineData("OptionalBeforeLook", true, Skip = OffWrong)]
	[InlineData("Quoted", true, Skip = OffWrong)]
	[InlineData("Fragment", false, Skip = OffWrong)]
	public void Without_the_option_a_reading_from_a_position_is_the_first_reading(string shape, bool direct)
	{
		var (grammar, input, at) = Shapes[shape];
		var graph    = Graph(grammar);
		var start    = graph.Rules.First(static rule => rule.Name == "Start");
		var assembly = Compiled(grammar, direct, positionalFollow: false);

		Assert.Equal(Expected(graph, start, input, at), Answer(assembly, input, at));
	}

	/// <summary>
	/// The smallest shape again, by its own numbers: from 0 in <c>ab</c> nothing is read and the
	/// reading succeeds, in <c>akb</c> the <c>ak</c> is read, and the whole form still refuses
	/// <c>ab</c>.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void An_optional_at_the_end_of_a_parse_keeps_its_skip_from_a_position(bool direct)
	{
		var assembly = Compiled(Shapes["Optional"].Grammar, direct, positionalFollow: true);

		Assert.Equal((true, 0), Answer(assembly, "ab", 0));
		Assert.Equal((true, 2), Answer(assembly, "akb", 0));
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "ab").IsSuccess);
	}

	/// <summary>
	/// The forms that answer with a match, from a position and in a window, give the same
	/// readings as the form that moves the position: where the value began and how long it is.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void The_match_forms_from_a_position_and_in_a_window_keep_the_skip(bool direct)
	{
		var assembly = Compiled(Shapes["Optional"].Grammar, direct, positionalFollow: true);

		Assert.Equal((true, (string?)null, 0L, 0L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "ab", 0));
		Assert.Equal((true, (string?)null, 1L, 0L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "xab", 1));
		Assert.Equal((true, (string?)null, 0L, 2L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "akb", 0));

		// A window that ends inside `ak` leaves the optional nothing to read but its skip.
		Assert.Equal((true, (string?)null, 0L, 0L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "akb", 0, 1));
		Assert.Equal((true, (string?)null, 0L, 2L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "akb", 0, 2));
		Assert.Equal((true, (string?)null, 1L, 0L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "xab", 1, 2));

		Assert.Equal((true, 0), Answer(assembly, "akb", 0, 1));
		Assert.Equal((true, 3), Answer(assembly, "xakb", 1, 2));
	}

	/// <summary>
	/// A grammar cut into tokens carries the option onto its syntactic half, which is the graph
	/// the parser is compiled from.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void The_lexical_split_carries_the_option(bool positionalFollow)
	{
		var graph = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(
				"trivia = { ' '* }\nStart = \"ab\" & (\"c\" | \"d\")?\nparse Start\n", RoslynCSharpScanner.Instance)).File),
			positionalFollow: positionalFollow);

		var split = LexicalSplit.Of(graph);

		Assert.NotNull(split);
		Assert.Equal(positionalFollow, split.Syntax.PositionalFollow);
	}

	/// <summary>The option is off unless asked for.</summary>
	[Fact]
	public void The_option_is_off_by_default()
	{
		Assert.False(new GramCompilerOptions().PositionalFollow);
		Assert.False(Graph(Shapes["Optional"].Grammar).PositionalFollow);
	}

	static RecognitionGraph Graph(string grammar)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));
	}

	static int Reference(string grammar, string input, int at)
	{
		var graph = Graph(grammar);

		return ReferenceInterpreter.Reads(graph, graph.Rules.First(static rule => rule.Name == "Start"), input, at);
	}

	static (bool Read, int At) Expected(RecognitionGraph graph, RuleSymbol start, string input, int at)
	{
		var end = ReferenceInterpreter.Reads(graph, start, input, at);

		return end >= 0 ? (true, end) : (false, at);
	}

	static Assembly Compiled(string grammar, bool direct, bool positionalFollow)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName        = "Grammar",
				Direct           = direct,
				PositionalFollow = positionalFollow,
				CSharpScanner    = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text);
	}

	static (bool Read, int At) Answer(Assembly assembly, string input, int at, int? length = null)
	{
		var answer = EmittedCode.Answered(assembly, "Grammar", "TryParseStart", input, at, length);

		return (answer.Read, answer.At);
	}

	/// <summary>Every string over <paramref name="letters"/> no longer than <paramref name="longest"/>.</summary>
	static IEnumerable<string> Inputs(char[] letters, int longest)
	{
		var level = new List<string> { "" };

		for (var length = 0; length <= longest; length++)
		{
			foreach (var text in level)
				yield return text;

			level = [.. level.SelectMany(text => letters.Select(letter => text + letter))];
		}
	}
}
