using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <c>Std.Nested(open, close)</c> (§5.2): read with a counter where a scanner or a lexer reads
/// it, and with its own rule everywhere else — and the two readings agree.
/// </summary>
/// <remarks>
/// <para>
/// Two references. The same rule written by hand under another name is not recognized, so it
/// is read as written — by the engine, which follows a rule that reaches itself, where the
/// library's is read by the counter — and it says what the pair means at any depth. But a
/// scanner cannot read it, and a refusal from a rule read by a scanner names less than one read
/// by the engine; so the messages are held to the rule unrolled to a fixed depth, which a
/// scanner reads, the way a grammar had to write a nesting comment before the library had one.
/// </para>
/// <para>
/// The counter is written where a scanner reads the call — braced trivia over characters on the
/// engine, and the seam between tokens in a grammar cut into a lexer — and where the lexer reads
/// the rest of a token. The reader of <c>Direct</c> reads a rule that reaches itself as it reads
/// any other, by calling it; the tests run it too, holding it to the same answers.
/// </para>
/// </remarks>
public sealed class NestedPairTests
{
	/// <summary>The library's rule, word for word, under a name nothing recognizes.</summary>
	const string Reference = "Pair(open, close) = open & (?!close & (Pair(open, close) | ?!open & any))* & close\n";

	/// <summary>How deep the unrolled reference reads, past the deepest input held to it.</summary>
	const int Unrolled = 14;

	/// <summary>
	/// <c>Pair(open, close)</c> unrolled <paramref name="depth"/> levels deep: an <c>open</c> inside
	/// the deepest leaves it unclosed.
	/// </summary>
	static string Unroll(int depth)
	{
		var body = "(?!close & ?!open & any)*";

		for (var level = 1; level < depth; level++)
			body = "(?!close & (open & " + body + " & close | ?!open & any))*";

		return "Pair(open, close) = open & " + body + " & close\n";
	}

	public static TheoryData<CarrierKind, bool> Readings
	{
		get
		{
			var rows = new TheoryData<CarrierKind, bool>();

			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
				foreach (var direct in new[] { false, true })
					rows.Add(carrier, direct);

			return rows;
		}
	}

	static string Deep(int depth, bool closed = true)
	{
		return string.Concat(Enumerable.Repeat("/* x ", depth)) +
			string.Concat(Enumerable.Repeat(" */", closed ? depth : depth - 1));
	}

	/// <summary>What the comments are read around: two words, the pair between and around them.</summary>
	static IEnumerable<string> Inputs(bool deep = true)
	{
		yield return "a b";
		yield return "a/**/b";
		yield return "a /* x */ b";
		yield return "a /* /* */ */ b";
		yield return "a /*/ b */ b";
		yield return "a /*/**/*/ b";
		yield return "a /* * / */ b";
		yield return "a */ b";
		yield return "a */* b";
		yield return "a /* b";
		yield return "a /* /* */ b";
		yield return "a b /* c */";
		yield return "a b /* c */ ";
		yield return "a b /* c";
		yield return "/* a */ a b";
		yield return "a /* /* */ b */ c";
		yield return "a /* x */ /* y /* z */ */ b";

		for (var depth = 1; depth <= 12; depth++)
			yield return "a " + Deep(depth) + " b";

		for (var depth = 1; depth <= 8; depth++)
		{
			yield return "a " + Deep(depth, closed: false) + " b";
			yield return "a b " + Deep(depth, closed: false);
		}

		if (deep)
			yield return "a " + Deep(50) + " b";
	}

	static Assembly Build(string grammar, CarrierKind carrier, bool direct, bool lexical = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier = carrier, Direct = direct, Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text);
	}

	static string Source(string grammar, bool lexical = false, bool direct = true)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Lexical = lexical, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return Assert.Single(result.Sources).Text;
	}

	/// <param name="positions">Whether where a refusal stands is held to the reference.</param>
	/// <param name="messages">And what it says.</param>
	static void Agree(Assembly expected, Assembly actual, string input, bool positions = true, bool messages = true)
	{
		var want = EmittedCode.Match(expected, "Grammar", "TryParseStart", input);
		var got  = EmittedCode.Match(actual,   "Grammar", "TryParseStart", input);

		Assert.True(want.IsSuccess == got.IsSuccess, $"{input}: {want} against {got}");
		Assert.Equal(want.Value, got.Value);

		if (positions)
			Assert.True(want.Position == got.Position, $"{input}: {want} against {got}");

		if (messages)
			Assert.True(want.Error == got.Error, $"{input}: {want} against {got}");
	}

	const string Words = "Start : @string = a: Std.Identifier & b: Std.Identifier & eof => @(a + \"|\" + b)\nparse Start\n";

	/// <summary>
	/// In trivia: a scanner reads the library's pair with its counter, and every answer, every
	/// position and every message is the one the rule read by the engine gives.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void Trivia_reads_as_the_rule_does(CarrierKind carrier, bool direct)
	{
		const string trivia = "trivia = { (Std.Spacing | Pair(\"/*\", \"*/\"))* }\n";

		var counted   = Build("trivia = { (Std.Spacing | Std.Nested(\"/*\", \"*/\"))* }\n" + Words, carrier, direct);
		var reference = Build(trivia + Reference + Words, carrier, direct);
		var unrolled  = Build(trivia + Unroll(Unrolled) + Words, carrier, direct);

		// The engine reads the hand-written rule in trivia step by step, recording, where the
		// counter's scanner refuses in silence; the reader of Direct reads both by calling.
		foreach (var input in Inputs())
			Agree(reference, counted, input, positions: direct, messages: direct);

		foreach (var input in Inputs(deep: false))
			Agree(unrolled, counted, input);
	}

	/// <summary>
	/// A grammar cut into a lexer skips its trivia with the same scanner, and reads what the
	/// grammar over characters reads.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void Trivia_between_tokens_reads_as_the_rule_does(CarrierKind carrier, bool direct)
	{
		const string grammar = "trivia = { (Std.Spacing | Std.Nested(\"/*\", \"*/\"))* }\n" + Words;

		var cut       = Build(grammar, carrier, direct, lexical: true);
		var reference = Build("trivia = { (Std.Spacing | Pair(\"/*\", \"*/\"))* }\n" + Reference + Words, carrier, direct);
		var unrolled  = Build("trivia = { (Std.Spacing | Pair(\"/*\", \"*/\"))* }\n" + Unroll(Unrolled) + Words, carrier, direct, lexical: true);

		// Over characters and over tokens a refusal is not said alike, whatever the comments.
		foreach (var input in Inputs())
			Agree(reference, cut, input, positions: false, messages: false);

		foreach (var input in Inputs(deep: false))
			Agree(unrolled, cut, input);
	}

	/// <summary>
	/// No depth is too deep for the counter, which holds a number and not a stack: ten
	/// thousand levels read, and ten thousand left open are refused where the first opened.
	/// </summary>
	/// <remarks>The reader of <c>Direct</c> over characters calls the rule, and goes as deep.</remarks>
	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void The_counter_has_no_depth_to_run_out_of(bool lexical, bool direct)
	{
		var built = Build("trivia = { (Std.Spacing | Std.Nested(\"/*\", \"*/\"))* }\n" + Words, CarrierKind.Auto, direct, lexical);

		var read = EmittedCode.Match(built, "Grammar", "TryParseStart", "a " + Deep(10_000) + " b");

		Assert.True(read.IsSuccess, read.Error);
		Assert.Equal("a|b", read.Value);

		Assert.False(EmittedCode.Match(built, "Grammar", "TryParseStart", "a " + Deep(10_000, closed: false) + " b").IsSuccess);
	}

	/// <summary>
	/// Delimiters of three characters, and delimiters that fold: the counter stops at every
	/// case of what begins them, and compares as the literals do.
	/// </summary>
	[Theory]
	[InlineData("\"<<<\"", "\">>>\"")]
	[InlineData("\"rem<\"i", "\">rem\"i")]
	[InlineData("\"(*\"", "\"*)\"")]
	[InlineData("'{'", "'}'")]
	public void Delimiters_of_any_length_and_case(string open, string close)
	{
		var counted   = Build($"trivia = {{ (Std.Spacing | Std.Nested({open}, {close}))* }}\n" + Words, CarrierKind.Auto, direct: false);
		var reference = Build($"trivia = {{ (Std.Spacing | Pair({open}, {close}))* }}\n" + Unroll(Unrolled) + Words, CarrierKind.Auto, direct: false);

		foreach (var input in new[]
		{
			"a <<< x >>> b", "a <<< <<< >>> >>> b", "a <<< << >> >>> b", "a <<< <<< >>> b", "a <<<>>> b", "a >>> b",
			"a rem< x >rem b", "a REM< Rem< >REM >rEm b", "a rem< x b", "a ReM< >re b",
			"a (* (* *) *) b", "a (*) *) b", "a (* x b",
			"a { { } } b", "a { b", "a {} b", "a } b",
		})
		{
			// A folded guard is not one a scanner reads as a search, so the unrolled rule is read
			// by the engine there, which records how far its trivia reached where a scanner does
			// not: the answers agree, and where and what a refusal says do not.
			var scanned = !open.EndsWith("i", StringComparison.Ordinal);

			Agree(reference, counted, input, positions: scanned, messages: scanned);
		}
	}

	/// <summary>
	/// Outside trivia, where the scanner reads the call alone and where the engine reads the
	/// rule as written: the same answers.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_pair_read_as_a_value_reads_as_the_rule_does(CarrierKind carrier, bool direct)
	{
		const string start = "Start : @string = c: Block & eof => @(c)\nparse Start\n";

		var counted   = Build("Block = Std.Nested(\"(\", \")\")\n" + start, carrier, direct);
		var reference = Build("Block = Pair(\"(\", \")\")\n" + Reference + start, carrier, direct);
		var unrolled  = Build("Block = Pair(\"(\", \")\")\n" + Unroll(Unrolled) + start, carrier, direct);
		var inputs    = new[] { "()", "(a)", "(a(b)c)", "((()))", "(()", "())", "(a(b)", "", "a", "(a))" };

		// A scanner's refusal names the rule it reads, and the engine's what the rule expected.
		foreach (var input in inputs.Append(new string('(', 30) + new string(')', 30)))
			Agree(reference, counted, input, messages: false);

		foreach (var input in inputs)
			Agree(unrolled, counted, input);
	}

	/// <summary>
	/// A token that is a pair: the lexer begins it at <c>open</c> and the counter reads the
	/// rest, where an automaton cannot. A pair the text ends inside of is cut short, as the
	/// rule over characters says.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_token_that_is_a_pair_is_read_by_the_lexer(CarrierKind carrier, bool direct)
	{
		const string grammar =
			"trivia = { Std.Spacing* }\n" +
			"Comment = Std.Nested(\"(*\", \"*)\")\n" +
			"Tagged  = '#' & Std.Nested(\"(*\", \"*)\")\n" +
			"Start : @string = 'x' & c: Comment & t: Tagged? & 'y' & eof => @(c + t)\n" +
			"parse Start\n";

		var source = Source(grammar, lexical: true);

		// Cut, and the rest read by the counter rather than by a machine over characters.
		Assert.Contains("Tokenize_DotGram", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Measure_", source, StringComparison.Ordinal);

		var cut   = Build(grammar, carrier, direct, lexical: true);
		var chars = Build(grammar, carrier, direct);

		foreach (var input in new[]
		{
			"x (* a *) y", "x (*(*a*)*) y", "x (* (* *) y", "x (* a y", "x (**) #(* b (* c *) *) y",
			"x (*) *) y", "x (* *) # (* *) y", "x (* *) #(* y", "x y", "x " + Deep(20).Replace("/*", "(*").Replace("*/", "*)") + " y",
		})
		{
			Agree(chars, cut, input, messages: false);
		}
	}

	/// <summary>The counter is what is written, in trivia and in a token alike.</summary>
	[Fact]
	public void The_counter_is_what_is_emitted()
	{
		const string grammar = "trivia = { (Std.Spacing | Std.Nested(\"/*\", \"*/\"))* }\n" + Words;

		Assert.Contains("IndexOfAny(text.Slice(p), '/', '*')", Source(grammar, direct: false), StringComparison.Ordinal);
		Assert.Contains("IndexOfAny(text.Slice(p), '/', '*')", Source(grammar, lexical: true), StringComparison.Ordinal);
	}

	/// <summary>
	/// Arguments a counter cannot tell apart are refused at the call, naming what is wrong:
	/// not literals, empty, equal, or beginning alike — under folding too.
	/// </summary>
	[Theory]
	[InlineData("Std.Nested(\"/*\", \"/*\")", "can begin at one place")]
	[InlineData("Std.Nested(\"ab\", \"ac\")", "can begin at one place")]
	[InlineData("Std.Nested(\"A\"i, \"a>\")", "can begin at one place")]
	[InlineData("Std.Nested(['a'..'b'], \"c\")", "literals")]
	[InlineData("Std.Nested((\"a\" | \"b\"), \"c\")", "literals")]
	[InlineData("Std.Nested(\"\", \"c\")", "empty")]
	[InlineData("Wrap(\"<\", \"<\")", "can begin at one place")]
	public void Arguments_a_counter_cannot_tell_apart_are_refused(string call, string why)
	{
		var result = GramCompiler.Compile(
			"Wrap(o, c) = Std.Nested(o, c)\n" +
			"Start = " + call + " & eof\nparse Start\n",
			new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		var refusal = Assert.Single(result.Diagnostics, one => one.Id == GrammarNormalizer.NestedNeedsLiterals);

		Assert.Contains(why, refusal.Message, StringComparison.Ordinal);
		Assert.Equal(GramSeverity.Error, refusal.Severity);
	}

	/// <summary>A rule of the same shape the author wrote is the author's, and is read as written.</summary>
	[Fact]
	public void Only_the_librarys_rule_is_recognized()
	{
		var source = Source("Start : @string = c: Block & eof => @(c)\nparse Start\nBlock = Pair(\"(\", \")\")\n" + Reference);

		Assert.DoesNotContain("depth", source, StringComparison.Ordinal);
	}
}
