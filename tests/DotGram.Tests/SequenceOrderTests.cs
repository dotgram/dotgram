using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// §4.1 case 2 says every operand that fits joins the sequence in order. A repetition whose
/// turn reads more than one of them is where that was lost.
/// </summary>
/// <remarks>
/// <para>
/// Each operand of a sequence result was given a capture of its own, and a capture under a
/// repetition collects an array — so <c>(C | S)+</c> handed its construction every <c>C</c>
/// and then every <c>S</c>, and <c>csc</c> came back as <c>c, c, s</c>. The same for
/// <c>(A &amp; B)*</c>, whose turns came back as all the <c>A</c>s and then all the <c>B</c>s.
/// </para>
/// <para>
/// The value oracle is the input itself: every element rule reads one letter of its own and
/// nothing else reads a letter, so whatever the grammar accepts, its value is the letters of
/// the input in the order they stand. What it accepts must not move at all: the same rule
/// without its sequence type — a recognizer, untouched by anything the sequence rewrite does —
/// is asked through the generator, and over characters through
/// <see cref="ReferenceInterpreter"/> as well.
/// </para>
/// </remarks>
public sealed class SequenceOrderTests
{
	const string Types = """
		public abstract class Item { public abstract char Tag { get; } }
		public sealed class ItemA : Item { public override char Tag { get { return 'a'; } } }
		public sealed class ItemB : Item { public override char Tag { get { return 'b'; } } }
		public sealed class ItemC : Item { public override char Tag { get { return 'c'; } } }
		""";

	const string Elements = """
		A : @ItemA = 'a' => @(new ItemA())
		B : @ItemB = 'b' => @(new ItemB())
		C : @ItemC = 'c' => @(new ItemC())
		""";

	/// <summary>Bodies for <c>Rows : @Item[]</c>, each mixing element types inside a repetition.</summary>
	public static TheoryData<string> Shapes()
	{
		return new TheoryData<string>
		{
			"(A | B)+",
			"(A | B | C)*",
			"(A | B | ';')*",
			"(A | ';' | B)*",
			"(A & B)*",
			"(A & B? & ';')*",
			"(A* & B)*",
			"((A | B) & ';')*",
			"A & (B | C)* & A",
			"(A & (B | C)*)+",
			"(A | B & C)*",
			"(A{2} | B)*",
			"((A | B)+ & ';')*",
			"(A | B & ';')*",
			"(';'? & (A | B))*",
			"{A | B}*",
			"(A? & B)*",
			"(A | B)* & ';' & (C | A)*",
			"((A | B) & (C | ';'))*",
			"(A | B)+ & C*",
			"((A & B)* & C)*",
		};
	}

	/// <summary>The report this exists for, built against a real compilation.</summary>
	/// <remarks>
	/// Through the generator rather than <see cref="GramCompiler"/>, so that whether
	/// <c>Circle</c> is a <c>Shape</c> is the host's answer and not a permissive one.
	/// </remarks>
	[Theory]
	[InlineData("Tape",      false)]
	[InlineData("Immediate", false)]
	[InlineData("Tape",      true)]
	[InlineData("Immediate", true)]
	public void Choices_of_different_types_keep_their_order(string carrier, bool lexical)
	{
		var source = $$""""
			using DotGram;

			namespace OrderProbe
			{
				public abstract class Shape
				{
				}

				public sealed class Circle : Shape
				{
				}

				public sealed class Square : Shape
				{
				}

				[Gram("""
					{{(lexical ? "trivia = { [' ']* }" : "")}}
					Shapes : @Shape[] = (C | S)+ & eof
					C : @Circle = 'c' => @(new Circle())
					S : @Square = 's' => @(new Square())
					Pairs : @Shape[] = (C & S)* & eof
					parse Shapes
					parse Pairs
					""", Lexical = {{(lexical ? "true" : "false")}}, Carrier = GramCarrier.{{carrier}})]
				public static partial class P
				{
				}
			}
			"""";

		// A sequence gathered from several types is one the immediate carrier refuses, which
		// leaves it on the tape and says so: asked of both carriers, the order is the same.
		var host = GeneratorDriverTests.Build(source, permittedWarning: "GRAM5007").GetType("OrderProbe.P")!;

		Assert.Equal("CircleCircleSquareCircle", Names(host, "ParseShapes", "ccsc"));
		Assert.Equal("CircleSquareCircleSquare", Names(host, "ParsePairs", "cscs"));

		static string Names(Type host, string method, string input)
		{
			var parse  = host.GetMethod(method, BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;
			var shapes = (Array)parse.Invoke(null, [input])!;

			return string.Concat(shapes.Cast<object>().Select(static shape => shape.GetType().Name));
		}
	}

	/// <summary>
	/// Every shape, on every rendering and every way of handing the input over: the order the
	/// input itself has, and the acceptance the rule has without a sequence type.
	/// </summary>
	[Theory]
	[MemberData(nameof(Shapes))]
	public void Repeated_operands_join_in_the_order_they_were_read(string shape)
	{
		var grammar = $"""
			Rows : @Item[] = {shape}
			{Elements}
			parse Rows stream bytes
			""";

		// The same rule as a recognizer: no sequence, so nothing the sequence rewrite does.
		var plain = $"""
			Rows = {shape}
			{Elements}
			parse Rows
			""";
		var graph = Normalized(plain);
		var rows  = graph.Rules.First(static rule => rule.Name == "Rows");

		foreach (var carrier in new[] { "engine", "tape", "immediate" })
		{
			var host      = Compile(grammar, carrier);
			var recognize = Compile(plain, carrier);

			foreach (var input in Inputs(""))
			{
				var whole    = ReferenceInterpreter.Parses(graph, rows, input);
				var expected = whole ? Letters(input) : null;
				var context  = $"shape {shape}, {carrier}, input \"{input}\"";

				Assert.True(whole == Accepts(recognize, input), context + ": the recognizer and the semantics disagree");

				Same(expected, Text(host, input), context + ", text");
				Same(expected, Streamed(host, input, chars: true), context + ", chars");
				Same(expected, Streamed(host, input, chars: false), context + ", bytes");

				// Begun at the start and not held to the end: the first reading is the one, and
				// its value is the letters of what it read.
				var end = ReferenceInterpreter.Reads(graph, rows, input, 0);

				Same(end < 0 ? null : Letters(input.Substring(0, end)), Positioned(host, input), context + ", positioned");
			}
		}
	}

	/// <summary>
	/// The same shapes over tokens, in a rule marked <c>?</c> — where a later failure in the rule
	/// may reopen a choice of its own, and nothing the rewrite does may take that away.
	/// </summary>
	/// <remarks>
	/// Each element is one token and the separator another, so the tokens commit nothing a
	/// character reading would have reopened, and the semantics of §11 are the oracle here too.
	/// A shape is also read with a trailing <c>';' &amp; eof</c>, which is what gives a later
	/// failure something to send the parse back for.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Shapes))]
	public void Over_tokens_a_rule_that_gives_back_still_does(string shape)
	{
		foreach (var tail in new[] { "", " & ';' & eof" })
		{
			var grammar = $$"""
				trivia = { ' '* }
				Rows? : @Item[] = ({{shape}}){{tail}}
				{{Elements}}
				parse Rows
				""";
			var graph = Normalized($"""
				Rows = ({shape}){tail}
				{Elements}
				""");
			var rows  = graph.Rules.First(static rule => rule.Name == "Rows");

			foreach (var carrier in new[] { "engine", "tape", "immediate" })
			{
				var host = Compile(grammar, carrier, lexical: true);

				foreach (var input in Inputs(tail.Length == 0 ? "" : ";"))
				{
					var expected = ReferenceInterpreter.Parses(graph, rows, input) ? Letters(input) : null;

					Same(expected, Text(host, input), $"shape {shape}{tail}, {carrier}, over tokens, input \"{input}\"");
				}
			}
		}
	}

	/// <summary>
	/// A later failure reopens the first turn over tokens, in a rule marked <c>?</c>: the turn's
	/// own choice is still the rule's to revisit.
	/// </summary>
	[Theory]
	[InlineData("engine")]
	[InlineData("tape")]
	[InlineData("immediate")]
	public void A_later_failure_reopens_a_turn_over_tokens(string carrier)
	{
		var same = Compile("""
			trivia = { ' '* }
			Rows? : @int[] = (A | A & 'b')+ & 'c' & eof
			A : @int = 'a' => @(1)
			parse Rows
			""", carrier, lexical: true, types: false);

		Assert.Equal([1], Values(same, "abc"));
		Assert.Equal([1, 1], Values(same, "aabc"));

		// And with alternatives of two types, gathered into one member.
		var mixed = Compile($$"""
			trivia = { ' '* }
			Rows? : @Item[] = (A | D & 'b')+ & 'c' & eof
			D : @ItemC = 'a' => @(new ItemC())
			{{Elements}}
			parse Rows
			""", carrier, lexical: true);

		Assert.Equal("c", Text(mixed, "abc"));
		Assert.Equal("ac", Text(mixed, "aabc"));
	}

	/// <summary>
	/// A guard inside a repetition sees the rule it was written in: what the rule has read so
	/// far, from where the rule began.
	/// </summary>
	[Theory]
	[InlineData("engine")]
	[InlineData("tape")]
	[InlineData("immediate")]
	public void A_guard_in_a_turn_sees_the_whole_rule(string carrier)
	{
		var same = Compile("""
			Rows : @int[] = 'x' & (A & B & when @(parserText.StartsWith("x")))+
			A : @int = 'a' => @(1)
			B : @int = 'b' => @(2)
			parse Rows
			""", carrier, types: false, buffered: false);

		Assert.Equal([1, 2], Values(same, "xab"));
		Assert.Equal([1, 2, 1, 2], Values(same, "xabab"));

		var mixed = Compile($"""
			Rows : @Item[] = 'x' & (A & B & when @(parserText.StartsWith("x") && parserSpan.Start == 0))+
			{Elements}
			parse Rows
			""", carrier, buffered: false);

		Assert.Equal("abab", Text(mixed, "xabab"));
	}

	/// <summary>
	/// A rule that only forwards is still inlined where a repetition calls it, so a later
	/// failure over tokens can still retry it through its other source.
	/// </summary>
	[Theory]
	[InlineData("engine")]
	[InlineData("tape")]
	[InlineData("immediate")]
	public void A_forwarding_rule_in_a_turn_is_still_transparent(string carrier)
	{
		var host = Compile("""
			trivia = { ' '* }
			Rows? : @object[] = (A | B)+ & 'c' & eof
			A : @string = s: S => @(s) | l: L => @(l)
			S : @string = 'a' => @("a")
			L : @string = 'a' & 'b' => @("ab")
			B : @int = 'd' => @(1)
			parse Rows
			""", carrier, lexical: true, types: false);

		Assert.Equal("ab", Objects(host, "abc"));
		Assert.Equal("a,1,ab", Objects(host, "adabc"));
	}

	/// <summary>Operands of one value type join a sequence of another, boxed, in order.</summary>
	[Theory]
	[InlineData("engine")]
	[InlineData("tape")]
	[InlineData("immediate")]
	public void Operands_of_one_type_box_into_a_sequence_of_another(string carrier)
	{
		var host = Compile("""
			Rows : @object[] = (A | B)+
			A : @int = 'a' => @(1)
			B : @int = 'b' => @(2)
			One : @object[] = A*
			parse Rows
			parse One
			""", carrier, types: false, buffered: false);

		Assert.Equal("1,2,1", Objects(host, "aba"));
		Assert.Equal("1,1", Objects(host, "aa", "TryParseOne"));
	}

	/// <summary>
	/// Nothing is added to the grammar: the rules it has are the rules it had, so nothing the
	/// compiler says about it can name one it did not write.
	/// </summary>
	[Theory]
	[MemberData(nameof(Shapes))]
	public void No_rule_is_made_for_the_order(string shape)
	{
		var typed   = Normalized($"""
			Start = ?=Rows & value: Rows & eof
			Rows : @Item[] = {shape}
			{Elements}
			""");
		var untyped = Normalized($"""
			Start = ?=Rows & value: Rows & eof
			Rows = {shape}
			{Elements}
			""");

		Assert.Equal(
			untyped.Rules.Select(static rule => rule.Name).Order(StringComparer.Ordinal),
			typed.Rules.Select(static rule => rule.Name).Order(StringComparer.Ordinal));

		var result = GramCompiler.Compile($"""
			Start = ?=Rows & value: Rows & eof
			Rows : @Item[] = {shape}
			{Elements}
			parse Start
			""", new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance, ReportCarriers = true });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Message.Contains("Rows_"));
		Assert.DoesNotContain(result.Carriers ?? [], static line => line.Contains("Rows_"));
	}

	/// <summary>
	/// No grammar checked into the repository has such a repetition, so what is generated for
	/// every one of them is what it was: nothing in it is gathered the new way.
	/// </summary>
	[Theory]
	[MemberData(nameof(GraphIntegrityTests.Grammars), MemberType = typeof(GraphIntegrityTests))]
	public void No_checked_in_grammar_gathers_across_captures(string path)
	{
		var graph = Normalized(File.ReadAllText(path));

		Assert.DoesNotContain(graph.Results.Values.SelectMany(static members => members), static member => member.Joins.Count > 0);
	}

	/// <summary>
	/// A recovering repetition among the operands of a joined repetition: what it read and what
	/// its failure factory made arrive in their places, of whichever type each is — on turns
	/// that recover and on turns that do not.
	/// </summary>
	[Theory]
	[MemberData(nameof(Recoveries))]
	public void A_recovery_inside_a_joined_repetition_keeps_its_place(string carrier, string grammar, string input, string expected)
	{
		var host = Compile(grammar, carrier, types: false, buffered: false);

		Assert.Equal(expected, Objects(host, input));
	}

	public static TheoryData<string, string, string, string> Recoveries()
	{
		const string Mixed = """
			A : @int = 'a' => @(1)
			B : @string = 'b' => @("b")
			parse Rows
			""";
		const string Same = """
			A : @int = 'a' => @(1)
			B : @int = 'b' => @(2)
			parse Rows
			""";

		var cases = new (string Grammar, string Input, string Expected)[]
		{
			// The inner repetition recovers with a factory; the outer one joins it with B.
			("Rows : @object[] = ((A* recover ';' => @(0)) & B)+\n" + Mixed, "ab",      "1,b"),
			("Rows : @object[] = ((A* recover ';' => @(0)) & B)+\n" + Mixed, "aabab",   "1,1,b,1,b"),
			("Rows : @object[] = ((A* recover ';' => @(0)) & B)+\n" + Mixed, "a;b",     "1,0,b"),
			("Rows : @object[] = ((A* recover ';' => @(0)) & B)+\n" + Mixed, "ab;bab",  "1,b,0,b,1,b"),
			("Rows : @object[] = ((A* recover ';' => @(0)) & B)+\n" + Same,  "ab",      "1,2"),
			("Rows : @object[] = ((A* recover ';' => @(0)) & B)+\n" + Same,  "a;bab",   "1,0,2,1,2"),
			("Rows : @int[] = ((A* recover ';' => @(0)) & B)+\n" + Same,     "a;bab",   "1,0,2,1,2"),

			// The same with the recovering repetition second in the turn.
			("Rows : @object[] = (B & (A* recover ';' => @(0)))+\n" + Mixed, "bab;ba",  "b,1,b,0,b,1"),

			// Recovering without a factory drops what it could not read.
			("Rows : @object[] = ((A* recover ';') & B)+\n" + Mixed,         "a;bab",   "1,b,1,b"),

			// The outer, joined repetition recovers; a factory there needs one rule, so none.
			("Rows : @object[] = (A | B)* recover ';'\n" + Mixed,            "abx;ba",  "1,b,b,1"),
			("Rows : @object[] = (A | B)* recover ';'\n" + Same,             "abx;ba",  "1,2,2,1"),
		};

		var data = new TheoryData<string, string, string, string>();

		foreach (var carrier in new[] { "engine", "tape", "immediate" })
			foreach (var (grammar, input, expected) in cases)
				data.Add(carrier, grammar, input, expected);

		return data;
	}

	/// <summary>An alternative of a sequence result that reads no element contributes none.</summary>
	[Theory]
	[InlineData("engine")]
	[InlineData("tape")]
	[InlineData("immediate")]
	public void An_alternative_that_reads_nothing_is_an_empty_sequence(string carrier)
	{
		var host = Compile($"""
			Rows : @Item[] = (A | B)+ | ';'
			{Elements}
			parse Rows
			""", carrier);

		Assert.Equal("aba", Text(host, "aba"));
		Assert.Equal("", Text(host, ";"));
	}

	// ── Harness ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Every input up to four characters over the letters and the separator, and a few longer,
	/// each followed by <paramref name="tail"/>.
	/// </summary>
	static IEnumerable<string> Inputs(string tail)
	{
		var found = new List<string> { "" };
		var last  = new List<string> { "" };

		for (var length = 1; length <= 4; length++)
		{
			last = [.. last.SelectMany(static prefix => "abc;".Select(one => prefix + one))];
			found.AddRange(last);
		}

		found.AddRange(["ababab", "abcabcabc", "a;b;a;b;", "aab;bba;", "abababa;cacab", "bbbbbbbbba", "ab;ab;ab;c"]);

		return found.Select(input => input + tail);
	}

	static void Same(string? expected, string? actual, string context)
	{
		Assert.True(
			expected == actual,
			$"{context}: expected {expected ?? "a rejection"}, read {actual ?? "a rejection"}");
	}

	static string Letters(string input)
	{
		return string.Concat(input.Where(static one => one is 'a' or 'b' or 'c'));
	}

	static char Tag(object item)
	{
		return (char)item.GetType().GetProperty("Tag")!.GetValue(item)!;
	}

	static string Tags(object? items)
	{
		return string.Concat(((Array)items!).Cast<object>().Select(Tag));
	}

	static Type Compile(string grammar, string carrier, bool lexical = false, bool types = true, bool buffered = true)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Direct        = carrier != "engine",
			Carrier       = carrier switch { "tape" => CarrierKind.Tape, "immediate" => CarrierKind.Immediate, _ => CarrierKind.Auto },
			BufferedInput = buffered && !lexical,
			BufferedBytes = buffered && !lexical,
			Lexical       = lexical,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		// An element rule a shape does not use is unused, and a shape the reader overload
		// cannot take gets none; the harness reads whatever forms there are.
		Assert.DoesNotContain(
			result.Diagnostics,
			static one => one.Severity != GramSeverity.Info && one.Id is not (GramCompiler.CarrierCaution or GramCompiler.CarrierRefused or "GRAM4018" or "GRAM5001"));

		return EmittedCode.Compile(
			result.Sources[0].Text,
			declarationMembers: types ? Types : null,
			sourceParts: result.Sources.Skip(1).Select(static source => source.Text)).GetType("Grammar")!;
	}

	/// <summary>Whether a rule without a value reads the whole input.</summary>
	static bool Accepts(Type host, string input)
	{
		var match = host.GetMethod("TryParseRows", [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	/// <summary>The values an <c>object[]</c> rule reads the whole input as, joined by commas.</summary>
	static string Objects(Type host, string input, string method = "TryParseRows")
	{
		var match = host.GetMethod(method, [typeof(string)])!.Invoke(null, [input])!;

		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!, $"\"{input}\" was refused");

		return string.Join(",", ((Array)match.GetType().GetProperty("Value")!.GetValue(match)!).Cast<object>());
	}

	/// <summary>The values an <c>int[]</c> rule reads the whole input as.</summary>
	static int[] Values(Type host, string input)
	{
		var match = host.GetMethod("TryParseRows", [typeof(string)])!.Invoke(null, [input])!;

		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!, $"\"{input}\" was refused");

		return (int[])match.GetType().GetProperty("Value")!.GetValue(match)!;
	}

	/// <summary>The value the whole input reads as, or null where it does not.</summary>
	static string? Text(Type host, string input)
	{
		var match = host.GetMethod("TryParseRows", [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? Tags(match.GetType().GetProperty("Value")!.GetValue(match))
			: null;
	}

	/// <summary>The same through a reader or a stream, read a small window at a time.</summary>
	static string? Streamed(Type host, string input, bool chars)
	{
		object source = chars ? new StringReader(input) : new MemoryStream(Encoding.ASCII.GetBytes(input));

		var method = host.GetMethod("ParseRows", [chars ? typeof(TextReader) : typeof(Stream), typeof(int?), typeof(int?)]);

		Assert.NotNull(method);

		try
		{
			return string.Concat(((System.Collections.IEnumerable)method.Invoke(null, [source, 1, 64])!).Cast<object>().Select(Tag));
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is FormatException)
		{
			return null;
		}
	}

	/// <summary>The value read from the start of the input, not held to its end.</summary>
	static string? Positioned(Type host, string input)
	{
		var (read, value, _) = EmittedCode.Answered(host.Assembly, "Grammar", "TryParseRows", input, 0);

		return read ? Tags(value) : null;
	}

	static RecognitionGraph Normalized(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}
}
