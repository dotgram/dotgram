using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A recognizer of the host that succeeds without moving, inside a repetition (docs/syntax.md §7.2):
/// the turn that ended where it began ends the repetition, in every reading.
/// </summary>
/// <remarks>
/// <para>
/// Nothing in the contract asks a recognizer to move when it succeeds, and a repetition of one is
/// not refused when it is generated (GRAM4001 sees grammar, not C#). Every turn after one that stood
/// still would stand still the same way, since a recognizer's answer depends only on its arguments
/// and the text, so the loop went round for ever — and, where its turns build values, grew until the
/// machine ran out of memory.
/// </para>
/// <para>
/// Bounded by count and not by a clock: the recognizers count their calls and throw past a bound no
/// input here comes near, so a regression fails at once instead of hanging the run.
/// </para>
/// </remarks>
public sealed class StandingTurnTests
{
	/// <summary>The host: recognizers over a span and over the buffered input view, which count their calls.</summary>
	const string Host = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > 1000) throw new System.InvalidOperationException("A repetition went round without moving."); }
		static bool ToEnd(ParserInput<char> input, ref int p) { Count(); while (input.Peek(p, out _)) p++; return true; }
		static bool ToEnd(ParserInput<byte> input, ref int p) { Count(); while (input.Peek(p, out _)) p++; return true; }
		static bool Nothing(ParserInput<char> input, ref int p) { Count(); return true; }
		static bool Nothing(ParserInput<byte> input, ref int p) { Count(); return true; }
		static bool Half(ParserInput<char> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == 'x') p++; return true; }
		static bool Half(ParserInput<byte> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == (byte)'x') p++; return true; }
		""";

	/// <summary>The same recognizers over a span, which have no buffered forms.</summary>
	const string SpanHost = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > 1000) throw new System.InvalidOperationException("A repetition went round without moving."); }
		static bool ToEnd(System.ReadOnlySpan<char> input, ref int p) { Count(); p = input.Length; return true; }
		static bool Nothing(System.ReadOnlySpan<char> input, ref int p) { Count(); return true; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == 'x') p++; return true; }
		""";

	static readonly (string Name, string Grammar, string Inputs)[] Table =
	[
		// The repetitions that went round for ever, and what each reads.
		("plus",       "T = (@ToEnd)+ & eof",            "|a|ab"),
		("star",       "T = (@ToEnd)* & eof",            "|a|ab"),
		("rule",       "E = @ToEnd\nT = E* & eof",       "|a|ab"),
		("nothing",    "T = (@Nothing)* & 'a'",          "|a|b|aa"),

		// Turns short of the fewest are counted however far they read; one past them that stands still ends the repetition.
		("least",      "T = (@Nothing){3,} & 'a'",       "|a|b"),
		("bounded",    "T = (@Nothing){2,4} & 'a'",      "|a|b"),
		("half",       "T = (@Half)+ & eof",             "|x|xx|xy"),

		// Ordered choice: a turn that moves where it can, and one that stood still, which ends the
		// repetition without its other alternatives asked for, also where what follows refuses.
		("after",      "T = ('b' | @Nothing)* & 'a'",    "|a|ba|bba|bb"),
		("before",     "T = (@Nothing | 'b')* & 'a'",    "|a|ba"),
		("braced",     "T = {(@Nothing | 'b')*} & 'b' & eof", "|b|bb"),

		// Inside a sequence of the turn, and a turn that moves on some inputs and not on others.
		("sequence",   "T = (@Nothing & 'b'?)* & 'a'",   "|a|ba|bba|bb"),
		("nested",     "T = ((@Half)* & ';')* & eof",    "|;|x;|x;xx;|x"),

		// Values: a value for every turn counted, none for the turn past the fewest that stood still.
		("values",     "E : @int = @Nothing => @(1)\nT : @int = xs: E+ & 'a' => @(xs.Length)", "|a|b"),
		("owed",       "E : @int = @Nothing => @(1)\nT : @int = xs: E{3,} & 'a' => @(xs.Length)", "|a"),
		("counted",    "E : @int = @Half => @(1)\nT : @int = xs: E* & eof => @(xs.Length)", "|x|xx|xy"),

		// A count by a length: the turns owed are read where the count is reached.
		("length",     "T = $q: \"bb\" & (@Nothing){#q,} & 'a'", "|bb|bba|ba"),
		("lengths",    "E : @int = @Half => @(1)\nT : @int = $q: \"bb\" & xs: E{#q,} & eof => @(xs.Length)", "|bb|bbx|bbxxx"),
		("exact",      "T = $q: \"bb\" & (@Nothing){#q} & eof", "|bb|bbx"),
		("crossed",    "T = $q: \"bb\" & (@Nothing){#q,1} & eof", "|bb|bbx"),

		// A recognizer met only in a recovery's synchronization.
		("sync",       "T = 'a'* recover ((@Nothing)+ & ';') & eof", "|x;|a|ax;|x"),

		// A negative lookahead in the turn that stood still, met at the end of the input.
		("notahead",   "T = (?!'a' & @Nothing)+ & eof", "|x"),
		("notahead2",  "T = (?!'a' & @Half){2} & eof", "|x|xx"),
		("lookedfor",  "T = (?='a' & @Nothing)+ & 'a' & eof", "|a|b|aa"),
	];

	public static TheoryData<string, string, string> Rows()
	{
		var rows = new TheoryData<string, string, string>();

		foreach (var (name, grammar, inputs) in Table)
			rows.Add(name, grammar, inputs);

		return rows;
	}

	/// <summary>
	/// Every reading answers within the bound, and all of them answer alike: the outcome, where the
	/// parse stopped and the value (§7.5).
	/// </summary>
	[Theory]
	[MemberData(nameof(Rows))]
	public void A_turn_that_stands_still_ends_the_repetition(string name, string grammar, string inputs)
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
	[InlineData("plus",    "",    "ok@0")]
	[InlineData("plus",    "ab",  "ok@2")]
	[InlineData("star",    "ab",  "ok@2")]
	[InlineData("rule",    "",    "ok@0")]
	[InlineData("nothing", "a",   "ok@1")]
	[InlineData("nothing", "",    "no@0")]
	[InlineData("least",   "a",   "ok@1")]
	[InlineData("bounded", "a",   "ok@1")]
	[InlineData("half",    "xy",  "no@1")]
	[InlineData("after",   "bba", "ok@3")]
	[InlineData("before",  "ba",  "no@0")]
	[InlineData("braced",  "b",   "ok@1")]
	[InlineData("values",  "a",   "ok@1 1")]
	[InlineData("owed",    "a",   "ok@1 3")]
	[InlineData("counted", "",    "ok@0 0")]
	[InlineData("counted", "xx",  "ok@2 2")]
	[InlineData("length",  "bba", "ok@3")]
	[InlineData("lengths", "bb",  "ok@2 2")]
	[InlineData("lengths", "bbxxx", "ok@5 3")]
	[InlineData("exact",   "bb",  "ok@2")]
	[InlineData("crossed", "bb",  "no@2")]
	[InlineData("sync",    "x;",  "ok@2")]
	[InlineData("notahead",  "",   "ok@0")]
	[InlineData("notahead2", "x",  "ok@1")]
	public void The_answers_are_the_rule_s(string name, string input, string expected)
	{
		var grammar = Table.Single(row => row.Name == name).Grammar;

		foreach (var direct in new[] { true, false })
			Assert.Equal(expected, Trim(Answer(Build(grammar, SpanHost, direct, tape: false, buffered: false), input, typeof(string))));

		// The value where the row says one, which only the rows that count their turns do.
		string Trim(string answer)
		{
			return expected.Contains(' ', StringComparison.Ordinal) ? answer : answer.Split(' ')[0];
		}
	}

	/// <summary>The segments of a grammar that names <c>eos</c>, read through the same repetition.</summary>
	[Theory]
	[InlineData("T : @int = (@ToEnd)* & eos & eof => @(0)", new[] { "ab" }, "ok@3 0")]
	[InlineData("T : @int = (@ToEnd)* & eos & eof => @(0)", new[] { "" }, "ok@1 0")]
	[InlineData("T : @int = ((@Half)* & eos)* & eof => @(0)", new[] { "x", "", "xx" }, "ok@6 0")]
	[InlineData("T : @int = (@Nothing)+ & eos & eof => @(0)", new[] { "a" }, "no@0")]
	[InlineData("E : @int = @Half => @(1)\nT : @int = xs: E* & eos & eof => @(xs.Length)", new[] { "xx" }, "ok@3 2")]
	public void A_turn_that_stands_still_in_a_segment_ends_the_repetition(string grammar, string[] segments, string expected)
	{
		foreach (var direct in new[] { true, false })
		{
			var parser = Build(grammar, SpanHost, direct, tape: false, buffered: false);

			Reset(parser);

			var match = parser.GetType("Probe")!.GetMethod("TryParseT", [typeof(IReadOnlyList<string>)])!.Invoke(null, [segments])!;

			Assert.Equal(expected, expected.Contains(' ', StringComparison.Ordinal) ? Said(match) : Said(match).Split(' ')[0]);
		}
	}

	/// <summary>A repetition marked <c>recover</c> whose element may stand still, and a search for one.</summary>
	[Theory]
	[InlineData("T = (@Nothing & 'a'?)* recover ';' & eof", "", "ok@0")]
	[InlineData("T = (@Nothing & 'a'?)* recover ';' & eof", "aa", "ok@2")]
	[InlineData("T = (@Nothing & 'a'?)* recover ';' & eof", "ab;a", "no@1")]
	[InlineData("T = (@Half & ?!'z')* recover ';' & eof", "xx", "ok@2")]
	[InlineData("T = (@Half & ?!'z')* recover ';' & eof", "xxz;x", "ok@5")]
	[InlineData("T = (@Half & ?!'z')* recover ';' & eof", "xxy", "no@2")]
	public void A_recovering_repetition_ends_at_a_turn_that_stands_still(string grammar, string input, string expected)
	{
		foreach (var direct in new[] { true, false })
		{
			var parser = Build(grammar, SpanHost, direct, tape: false, buffered: false);

			Assert.Equal(expected, Answer(parser, input, typeof(string)).Split(' ')[0]);
		}
	}

	[Fact]
	public void A_search_for_a_repetition_that_stands_still_finds_each_occurrence()
	{
		foreach (var direct in new[] { true, false })
		{
			var parser = Build("T = (@Nothing)* & 'a'\nfind T", SpanHost, direct, tape: false, buffered: false, publish: false);

			Reset(parser);

			Assert.Equal([1L, 3L], EmittedCode.FoundAt(parser, "Probe", "FindT", "xaya"));
		}
	}

	static Assembly Build(string grammar, string host, bool direct, bool tape, bool buffered, bool publish = true)
	{
		var result = GramCompiler.Compile(grammar + (publish ? "\nparse T" : ""), new GramCompilerOptions
		{
			ClassName      = "Probe",
			Direct         = direct,
			Carrier        = tape ? CarrierKind.Tape : CarrierKind.Auto,
			BufferedInput  = buffered,
			BufferedBytes  = buffered,
			SymbolResolver = buffered ? ViewResolver.Instance : PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		return EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: host,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>Says every recognizer reads the input view, as the host's overloads over <c>ParserInput</c> do.</summary>
	internal sealed class ViewResolver : ISymbolResolver
	{
		public static readonly ViewResolver Instance = new();

		static readonly PermissiveSymbolResolver Permissive = PermissiveSymbolResolver.Instance;

		public bool TypeExists(string qualifiedName)
		{
			return Permissive.TypeExists(qualifiedName);
		}

		public bool IsAssignable(string from, string to)
		{
			return Permissive.IsAssignable(from, to);
		}

		public bool TryResolveConstructors(string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
		{
			return Permissive.TryResolveConstructors(qualifiedName, out constructors);
		}

		public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
		{
			return Permissive.TryResolveSettableProperties(qualifiedName, out properties);
		}

		public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
		{
			return Permissive.TryResolveExternalValue(methodName, against, out valueType);
		}

		public ExternalMethodResolution ResolveExternalMethod(string methodName, ExternalMethodRole role)
		{
			return role == ExternalMethodRole.Recognizer ? ExternalMethodResolution.FoundInput : Permissive.ResolveExternalMethod(methodName, role);
		}

		public bool Rewinds(string qualifiedName)
		{
			return Permissive.Rewinds(qualifiedName);
		}

		public bool IsReferenceType(string qualifiedName)
		{
			return Permissive.IsReferenceType(qualifiedName);
		}

		public bool IsNullable(string qualifiedName)
		{
			return Permissive.IsNullable(qualifiedName);
		}
	}

	static void Reset(Assembly parser)
	{
		parser.GetType("Probe")!.GetMethod("Reset")!.Invoke(null, null);
	}

	/// <summary>The outcome, where it stopped and the value, read from what the form of <paramref name="over"/> answered.</summary>
	/// <remarks>"none" where the form is not offered: a grammar that tracks lines reads no bytes.</remarks>
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
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!}" + (Read("Value") is int count ? $" {count}" : "")
			: $"no@{Read("Position")} {Read("Outcome")}";
	}
}
