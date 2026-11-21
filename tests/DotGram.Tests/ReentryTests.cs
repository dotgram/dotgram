using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A recursion through a recognizer of the host that succeeds without moving (docs/syntax.md §7.2): the
/// grammar compiles and is told so (GRAM5021), and a parse that comes back to a rule where it already
/// stands is refused there with a message, in every reading, instead of going round for ever.
/// </summary>
/// <remarks>
/// <para>
/// The check for left recursion takes a recognizer at its word that it consumes, so
/// <c>Nest = @Still &amp; n: Nest &amp; 'b' | 'x'+</c> is not left-recursive to it; where <c>@Still</c> does
/// not move, the parser entered <c>Nest</c> at the same position for ever, and since a recognizer's answer
/// depends only on its arguments and the text, no later entry could have moved either.
/// </para>
/// <para>
/// Bounded by count and not by a clock: the recognizers count their calls and throw past a bound no input
/// here comes near, so a regression fails at once instead of hanging the run.
/// </para>
/// </remarks>
public sealed class ReentryTests
{
	/// <summary>The host: recognizers over a span and over the buffered input view, which count their calls.</summary>
	/// <remarks>
	/// <c>Still</c> never moves; <c>Dot</c> moves over a <c>.</c> and refuses anything else; <c>Half</c>
	/// moves over a <c>.</c> and stands still on anything else.
	/// </remarks>
	const string Host = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > 10000) throw new System.InvalidOperationException("A recursion went round without moving."); }
		static bool Still(ParserInput<char> input, ref int p) { Count(); return true; }
		static bool Still(ParserInput<byte> input, ref int p) { Count(); return true; }
		static bool Dot(ParserInput<char> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == '.') { p++; return true; } return false; }
		static bool Dot(ParserInput<byte> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == (byte)'.') { p++; return true; } return false; }
		static bool Half(ParserInput<char> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == '.') p++; return true; }
		static bool Half(ParserInput<byte> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == (byte)'.') p++; return true; }
		""";

	/// <summary>The same recognizers over a span, which have no buffered forms.</summary>
	const string SpanHost = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > 10000) throw new System.InvalidOperationException("A recursion went round without moving."); }
		static bool Still(System.ReadOnlySpan<char> input, ref int p) { Count(); return true; }
		static bool Dot(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') { p++; return true; } return false; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') p++; return true; }
		""";

	const string Nest = "T = @Still & n: T & 'b' | 'x'+";

	/// <summary>What the refusal of <see cref="Nest"/> says.</summary>
	const string NestSaid = "Rule 'T' came back to itself through @Still without reading anything.";

	static readonly (string Name, string Grammar, string Inputs)[] Table =
	[
		// The rule that went round for ever, and the same rule over a recognizer that moves or refuses.
		("nest",      Nest,                                      "|x|xb|xxbb"),
		("dot",       "T = @Dot & n: T & 'b' | 'x'+",            "|x|.xb|..xbb|..x|.x|b"),
		("half",      "T = @Half & n: T & 'b' | 'x'+",           "|x|.xb|..xbb"),

		// Through another rule, and through one that forwards.
		("mutual",    "T = @Still & B | 'a'\nB = T & 'b'",       "|a|ab|b"),
		("through",   "T = A\nA = B & 'x' | 'y'\nB = @Still & A", "|y|yx"),

		// A cons list: the recursion a repetition is the better spelling of.
		("frames",    "T = @Dot & T | eof",                      "|.|..|.x"),
		("framesh",   "T = @Half & T | eof",                     "|.|..|x"),

		// A recognizer in a repetition that may stand still before the recursion.
		("repeated",  "T = (@Half)+ & n: T & 'b' | 'x'",         "|x|.xb|..xbb"),
		("repeatedd", "T = (@Dot)+ & n: T & 'b' | 'x'",          "|x|.xb|..xbb|..x"),

		// Behind a lookahead, positive and negative, and under a recovery.
		("ahead",     "T = ?='x' & @Still & T & 'b' | 'x'",      "|x|y"),
		// Not at the end of the input: there the reader by methods refuses on the first character before
		// the alternative is entered (what follows the lookahead wants one, the recognizer taken at its
		// word), and the engine enters it and comes back. Both refuse; only one of them reaches the check.
		("notahead",  "T = ?!'y' & @Still & T & 'b' | 'x'",      "x|y"),
		("inahead",   "T = ?=(@Still & T) & 'x' | 'y'",          "|x|y"),
	];

	public static TheoryData<string, string, string> Rows()
	{
		var rows = new TheoryData<string, string, string>();

		foreach (var (name, grammar, inputs) in Table)
			rows.Add(name, grammar, inputs);

		return rows;
	}

	/// <summary>Every reading answers within the bound, and all of them answer alike: the outcome, where and what it said.</summary>
	[Theory]
	[MemberData(nameof(Rows))]
	public void Every_reading_answers_alike(string name, string grammar, string inputs)
	{
		var answers = new List<(string Reading, string Input, string Answer)>();

		foreach (var direct in new[] { true, false })
		foreach (var tape in new[] { false, true })
		{
			var reading = (direct ? "direct" : "engine") + (tape ? "/tape" : "/immediate");
			var view    = Build(grammar, Host, direct, tape, buffered: true);
			var span    = Build(grammar, SpanHost, direct, tape, buffered: false);

			foreach (var input in inputs.Split('|'))
			{
				answers.Add((reading + "/span", input, Answer(span, input, typeof(string))));
				answers.Add((reading + "/view", input, Answer(view, input, typeof(string))));
				answers.Add((reading + "/reader", input, Answer(view, input, typeof(TextReader))));
				answers.Add((reading + "/bytes", input, Answer(view, input, typeof(Stream))));
			}
		}

		foreach (var input in answers.Select(static one => one.Input).Distinct())
		{
			var all = answers.Where(one => one.Input == input && one.Answer != "none").ToArray();

			Assert.True(
				all.All(one => one.Answer == all[0].Answer),
				$"{name} on \"{input}\":\n" + string.Join("\n", all.Select(static one => $"  {one.Reading}: {one.Answer}")));
		}
	}

	/// <summary>What the rows answer, so that agreeing on a wrong answer is not passing.</summary>
	[Theory]
	[InlineData("nest",     "",      "no@0 NoMatch " + NestSaid)]
	[InlineData("nest",     "x",     "no@0 NoMatch " + NestSaid)]
	[InlineData("nest",     "xxbb",  "no@0 NoMatch " + NestSaid)]
	[InlineData("dot",      "x",     "ok@1")]
	[InlineData("dot",      "..xbb", "ok@5")]
	[InlineData("dot",      ".xb",   "ok@3")]
	[InlineData("dot",      ".x",    "no@2 Starved")]
	[InlineData("half",     ".xb",   "no@1 NoMatch Rule 'T' came back to itself through @Half without reading anything.")]
	[InlineData("mutual",   "a",     "no@0 NoMatch Rule 'T' came back to itself through 'B' and @Still without reading anything.")]
	[InlineData("frames",   "..",    "ok@2")]
	[InlineData("frames",   ".x",    "no@1 NoMatch")]
	[InlineData("framesh",  "..",    "no@2 NoMatch Rule 'T' came back to itself through @Half without reading anything.")]
	[InlineData("repeated", ".xb",   "no@1 NoMatch Rule 'T' came back to itself through @Half without reading anything.")]
	[InlineData("repeatedd", "..xbb", "ok@5")]
	[InlineData("ahead",    "y",     "no@0 NoMatch")]
	[InlineData("ahead",    "x",     "no@0 NoMatch " + NestSaid)]
	[InlineData("notahead", "y",     "no@0 NoMatch")]
	[InlineData("notahead", "x",     "no@0 NoMatch " + NestSaid)]
	[InlineData("inahead",  "y",     "ok@1")]
	[InlineData("inahead",  "x",     "no@0 NoMatch " + NestSaid)]
	public void The_answers_are_the_rule_s(string name, string input, string expected)
	{
		var grammar = Table.Single(row => row.Name == name).Grammar;

		foreach (var direct in Environment.GetEnvironmentVariable("REENTRY_DIRECT") is null ? new[] { true, false } : new[] { true })
			Assert.Equal(expected, Answer(Build(grammar, SpanHost, direct, tape: false, buffered: false), input, typeof(string)));
	}

	/// <summary>The build says so once a cycle, as Information, on the rule that closes it; and nothing of a grammar without one.</summary>
	[Theory]
	[InlineData(Nest, "'T' can come back to itself without reading anything where @Still succeeds")]
	[InlineData("T = @Still & B | 'a'\nB = T & 'b'", "'T' can come back to itself through 'B' without reading anything where @Still succeeds")]
	[InlineData("T = @Dot & T | eof", "'T' can come back to itself without reading anything where @Dot succeeds")]
	public void A_cycle_is_told_once(string grammar, string told)
	{
		var said = Reported(grammar).Where(static one => one.Id == Reentry.Reenters).ToArray();

		Assert.Equal(GramSeverity.Info, Assert.Single(said).Severity);
		Assert.Contains(told, said[0].Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("T = @Still* & 'x'")]
	[InlineData("T = 'a' & @Still & T | 'x'")]
	[InlineData("T = @Still & 'a' & T | 'x'")]
	[InlineData("T = E\nE = E & '+' & @Still & 'n' | 'n'")]
	public void A_grammar_without_one_is_not_told(string grammar)
	{
		Assert.DoesNotContain(Reported(grammar), static one => one.Id == Reentry.Reenters);
	}

	[Fact]
	public void A_grammar_refused_as_left_recursion_is_not_told_of_it()
	{
		var said = Reported("T = A | 'x'\nA = @Still & T & 'b' | B\nB = T & 'c'");

		Assert.Contains(said, static one => one.Id == "GRAM4002");
		Assert.DoesNotContain(said, static one => one.Id == Reentry.Reenters);
	}

	static IReadOnlyList<GramDiagnostic> Reported(string grammar)
	{
		return GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
		{
			ClassName      = "Probe",
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		}).Diagnostics;
	}

	static Assembly Build(string grammar, string host, bool direct, bool tape, bool buffered, bool publish = true)
	{
		var result = GramCompiler.Compile(grammar + (publish ? "\nparse T" : ""), new GramCompilerOptions
		{
			ClassName      = "Probe",
			Direct         = direct,
			Carrier        = tape ? CarrierKind.Deferred : CarrierKind.Auto,
			BufferedInput  = buffered,
			BufferedBytes  = buffered,
			SymbolResolver = buffered ? StandingTurnTests.ViewResolver.Instance : PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		return EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: host,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static void Reset(Assembly parser)
	{
		parser.GetType("Probe")!.GetMethod("Reset")!.Invoke(null, null);
	}

	/// <summary>The outcome, where it stopped and what it said, read from what the form of <paramref name="over"/> answered.</summary>
	/// <remarks>"none" where the form is not offered.</remarks>
	static string Answer(Assembly parser, string input, Type over)
	{
		Reset(parser);

		var type   = parser.GetType("Probe")!;
		var bytes  = Encoding.ASCII.GetBytes(input);
		var method = over == typeof(string)
			? type.GetMethod("TryParseT", [typeof(string)])
			: type.GetMethod("TryParseT", [over, typeof(int?), typeof(int?)]);

		if (method is null)
			return "none";

		var match = over == typeof(string)
			? method.Invoke(null, [input])!
			: method.Invoke(null, [over == typeof(Stream) ? new MemoryStream(bytes) : new StringReader(input), 1, 1 << 16])!;

		return Said(match);
	}

	static string Said(object match)
	{
		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!}"
			: $"no@{Read("Position")} {Read("Outcome")}" + (Read("Error") is string said && said.StartsWith("Rule '", StringComparison.Ordinal) ? " " + said : "");
	}
}
