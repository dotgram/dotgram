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
	/// Split (<see cref="GramCompilerOptions.PositionalFollowSplit"/>), every shape is read as the
	/// option alone reads it: the proofs that learned to tell a positional stop from anything
	/// were taught only where that changes no answer, and these are the shapes where it would.
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
	public void Split_a_reading_from_a_position_is_the_first_reading(string shape, bool direct)
	{
		var (grammar, input, at) = Shapes[shape];
		var graph    = Graph(grammar);
		var start    = graph.Rules.First(static rule => rule.Name == "Start");
		var assembly = Compiled(grammar, direct, positionalFollow: true, split: true);

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
	/// Bodies a repetition or an optional at the end of a published rule is written with: turns
	/// that share a first character and fail after it, turns of one length or two, a doubled
	/// quote, one character, a turn that ends in an optional.
	/// </summary>
	static readonly string[] TailBodies =
	[
		"('a' & 'b' | 'a' & 'c')",
		"('a' | 'a' & 'c')",
		"(\"\\\"\\\"\" | [^ '\"'])",
		"['a'..'b']",
		"('a' & 'b'?)",
	];

	/// <summary>
	/// What stands between that repetition and the end of the rule: nothing, a look of either sign,
	/// a refusal of one class or of a rule that is one, the end of input, something that cannot
	/// fail, and things that can refuse while reading nothing.
	/// </summary>
	static readonly string[] Tails =
	[
		"",
		" & ?!'c'",
		" & ?='a'",
		" & ?!'a'",
		" & ?!['a'..'c']",
		" & Halt",
		" & 'c'?",
		" & 'c'* & ?!'a'",
		" & ('c' | ?!'b')",
		" & (?!'b' & 'c')?",
		" & eof",
		" & '\"'",
		" & Sub",
	];

	/// <summary>Every body under every repetition before every tail, each its own published rule.</summary>
	static string TailShapes(out int count)
	{
		var text  = new System.Text.StringBuilder("Halt = ?!['a'..'c']\nSub = ('a' | 'c')?\n");
		var index = 0;

		foreach (var body in TailBodies)
			foreach (var repeat in new[] { "?", "*", "+", "{0,2}" })
				foreach (var tail in Tails)
				{
					text.Append($"S{index} = {body}{repeat}{tail}\nparse S{index}\n");
					index++;
				}

		count = index;

		return text.ToString();
	}

	/// <summary>
	/// Split, every generated tail shape reads from every position of every short input what the
	/// reference interpreter reads there, in both backends, and the whole form accepts what the
	/// interpreter accepts.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Split_every_tail_shape_reads_the_first_reading_from_every_position(bool direct)
	{
		var grammar  = TailShapes(out var count);
		var graph    = Graph(grammar);
		var assembly = Compiled(grammar, direct, positionalFollow: true, split: true);
		var wrong    = new List<string>();
		var flat     = 0;

		for (var index = 0; index < count; index++)
		{
			var start = graph.Rules.First(rule => rule.Name == $"S{index}");

			// A publication on the flat path is offered no form that reads from a position (§6.3).
			if (!assembly.GetType("Grammar")!.GetMethods().Any(one =>
				one.Name == $"TryParseS{index}" && one.GetParameters() is { Length: 3 } taken && taken[1].ParameterType.IsByRef))
			{
				flat++;

				continue;
			}

			foreach (var text in Inputs(['a', 'b', 'c', '"'], 4))
			{
				for (var from = 0; from <= text.Length; from++)
				{
					var expected = Expected(graph, start, text, from);
					var answer   = EmittedCode.Answered(assembly, "Grammar", $"TryParseS{index}", text, from);

					if (expected != (answer.Read, answer.At))
						wrong.Add($"S{index} '{text}' from {from}: expected {expected}, answered {(answer.Read, answer.At)}");
				}

				if (ReferenceInterpreter.Parses(graph, start, text) != EmittedCode.Match(assembly, "Grammar", $"TryParseS{index}", text).IsSuccess)
					wrong.Add($"S{index} whole '{text}'");
			}
		}

		Assert.True(flat < count / 2, $"{flat} of {count} shapes have no positional form.");
		Assert.True(wrong.Count == 0, $"{wrong.Count} cells differ, the first: " + string.Join("\n", wrong.Take(20)) + "\n" + grammar);
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
			positionalFollow: positionalFollow,
			positionalSplit: true);

		var split = LexicalSplit.Of(graph);

		Assert.NotNull(split);
		Assert.Equal(positionalFollow, split.Syntax.PositionalFollow);

		// The split means nothing without the option, and is carried with it.
		Assert.Equal(positionalFollow, split.Syntax.PositionalSplit);
	}

	/// <summary>
	/// The proofs that may read a continuation's view (<c>Continuation.Taught</c>): each asks only
	/// whether what follows can fail after a construct succeeded and then succeed earlier. Whether
	/// what follows can <em>begin</em> somewhere — the settled optional's entry, a run past its stop
	/// character (<c>NeverGivesBackPast</c>), the replay of a carrier — is never one of them.
	/// </summary>
	static readonly string[] Taught = ["NeverGivesBack", "Possessive", "LiteralRun", "LiteralGroup", "SettledText"];

	/// <summary>
	/// Every read of the view is an argument of a call to one of <see cref="Taught"/>, itself or its
	/// <c>.Plain</c>, and nothing but the continuation itself names the view: a new reader of it, one
	/// handed through a variable or to another call beside a listed one, or a continuation rebuilt
	/// without it, fails here until it is classified.
	/// </summary>
	/// <remarks>
	/// Read as C# rather than as text, so that what is checked is where the value goes. Which of a
	/// listed proof's calls may read it is not something a name can say — the settled optional's
	/// entry asks <c>NeverGivesBack</c> too, of the halves — and that is held by the shapes above.
	/// </remarks>
	[Fact]
	public void Only_the_listed_proofs_read_the_view()
	{
		var source = System.IO.Path.Combine(ReaderCoverageTests.Root(AppContext.BaseDirectory), "src", "DotGram");
		var found  = new List<string>();
		var reads  = 0;

		foreach (var file in System.IO.Directory.GetFiles(source, "*.cs", System.IO.SearchOption.AllDirectories)
			.Where(one => !one.Contains(System.IO.Path.DirectorySeparatorChar + "obj" + System.IO.Path.DirectorySeparatorChar)))
		{
			var name = System.IO.Path.GetFileName(file);

			if (name == "FollowSets.cs")
				continue;

			var root = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(System.IO.File.ReadAllText(file), cancellationToken: TestContext.Current.CancellationToken).GetRoot(TestContext.Current.CancellationToken);

			// Documentation comments are trivia and not walked: a <see cref> to the view is not a use of it.
			foreach (var identifier in root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax>())
			{
				var text = identifier.Identifier.ValueText;

				if (text == "View")
					found.Add($"{name}:{Line(identifier)}: names Continuation.View");

				if (text != "Taught")
					continue;

				reads++;

				if (Feeds(identifier) is not { } callee || !Taught.Contains(callee))
					found.Add($"{name}:{Line(identifier)}: `{identifier.Parent}` reads the view for {Feeds(identifier) ?? "no call"}");
			}
		}

		Assert.True(reads >= 10, $"Only {reads} reads of the view were found under {source}: has it been renamed?");
		Assert.True(found.Count == 0, string.Join("\n", found));

		static int Line(Microsoft.CodeAnalysis.SyntaxNode node)
		{
			return node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
		}

		// The method a read of the view is handed to, where it is one: `x.Taught` or `x.Taught.Plain`
		// written as an argument of a call; null where it is anything else.
		static string? Feeds(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax taught)
		{
			if (taught.Parent is not Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax access || access.Name != taught)
				return null;

			Microsoft.CodeAnalysis.SyntaxNode value = access;

			if (value.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax plain &&
				plain.Expression == value && plain.Name.Identifier.ValueText == "Plain")
				value = plain;

			if (value.Parent is not Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax argument ||
				argument.Parent?.Parent is not Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax call)
				return null;

			return call.Expression switch
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax called => called.Identifier.ValueText,
				Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax called => called.Name.Identifier.ValueText,
				_ => null,
			};
		}
	}

	/// <summary>
	/// Tails a selector stands in: a captured repetition, optional or choice of literals before a
	/// <c>switch</c> one of whose cases reads nothing. A selector picks its case and does not try the
	/// others, so it can refuse at one place and read nothing at another.
	/// </summary>
	static readonly string[] SelectorTails =
	[
		"D = ['0'..'9']\nStart = (y: D)* & switch @(y) { case \"1\": none default: 'z' }\nparse Start\n",
		"D = ['0'..'9']\nStart = y: D* & switch @(y) { case \"1\": none default: 'z' }\nparse Start\n",
		"D = ['0'..'9']\nStart = (y: D)? & switch @(y) { case \"1\": none default: 'z' }\nparse Start\n",
		"D = ['0'..'9']\nStart = (y: D)+ & switch @(y) { case \"1\": none default: 'z' }\nparse Start\n",
		"D = ['0'..'9']\nStart = (y: D){0,2} & switch @(y) { case \"12\": none default: 'z' }\nparse Start\n",
		"D = ['0'..'9']\nStart = (y: D)* & switch @(y) { case \"12\": 'z' default: none }\nparse Start\n",
		"Start = (y: (\"12\" | \"1\")) & switch @(y) { case \"1\": none default: 'z' }\nparse Start\n",
		"Start = (y: (\"12\" | \"1\"))? & switch @(y) { case \"12\": 'z' default: none }\nparse Start\n",
	];

	public static TheoryData<int, bool> SelectorCases()
	{
		var cases = new TheoryData<int, bool>();

		for (var index = 0; index < SelectorTails.Length; index++)
		{
			cases.Add(index, false);
			cases.Add(index, true);
		}

		return cases;
	}

	/// <summary>
	/// Split, a selector between a repetition and the end reads as the option alone reads it — held to
	/// the option and not to the reference interpreter, which does not run C#.
	/// </summary>
	[Theory]
	[MemberData(nameof(SelectorCases))]
	public void Split_a_selector_at_the_end_reads_as_the_option_alone(int index, bool direct)
	{
		var grammar = SelectorTails[index];
		var follow  = Compiled(grammar, direct, positionalFollow: true);
		var split   = Compiled(grammar, direct, positionalFollow: true, split: true);
		var wrong   = new List<string>();

		foreach (var text in Inputs(['1', '2', 'z', 'x'], 4))
		{
			for (var from = 0; from <= text.Length; from++)
			{
				var expected = EmittedCode.Answered(follow, "Grammar", "TryParseStart", text, from);
				var answer   = EmittedCode.Answered(split, "Grammar", "TryParseStart", text, from);

				if (expected.Read != answer.Read || expected.At != answer.At)
					wrong.Add($"'{text}' from {from}: follow {(expected.Read, expected.At)}, split {(answer.Read, answer.At)}");
			}

			if (EmittedCode.Match(follow, "Grammar", "TryParseStart", text).IsSuccess != EmittedCode.Match(split, "Grammar", "TryParseStart", text).IsSuccess)
				wrong.Add($"whole '{text}'");
		}

		Assert.True(wrong.Count == 0, $"{wrong.Count} cells differ:\n" + string.Join("\n", wrong.Take(15)));
	}

	/// <summary>The option is off unless asked for.</summary>
	[Fact]
	public void The_option_is_off_by_default()
	{
		Assert.False(new GramCompilerOptions().PositionalFollow);
		Assert.False(new GramCompilerOptions().PositionalFollowSplit);
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

	static Assembly Compiled(string grammar, bool direct, bool positionalFollow, bool split = false)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName        = "Grammar",
				Direct           = direct,
				PositionalFollow = positionalFollow,
				PositionalFollowSplit = split,
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
