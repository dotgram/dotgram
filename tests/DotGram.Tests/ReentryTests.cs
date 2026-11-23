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
/// grammar compiles and is told so (GRAM5021), and an entry that comes back to a rule where it already
/// stands is cut as an alternative that failed, in every reading, instead of going round for ever — at once
/// where nothing is read after the call back, past the input left otherwise — and a reading in which too many
/// come back throws.
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
		static bool Ws(ParserInput<char> input, ref int p) { Count(); while (input.Peek(p, out var c) && c == ' ') p++; return true; }
		static bool Ws(ParserInput<byte> input, ref int p) { Count(); while (input.Peek(p, out var c) && c == (byte)' ') p++; return true; }
		""";

	/// <summary>The same recognizers over a span, which have no buffered forms.</summary>
	const string SpanHost = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > 10000) throw new System.InvalidOperationException("A recursion went round without moving."); }
		static bool Still(System.ReadOnlySpan<char> input, ref int p) { Count(); return true; }
		static bool Dot(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') { p++; return true; } return false; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') p++; return true; }
		static bool Ws(System.ReadOnlySpan<char> input, ref int p) { Count(); while (p < input.Length && input[p] == ' ') p++; return true; }
		""";

	const string Nest = "T = @Still & n: T & 'b' | 'x'+";

	/// <summary>What a reading of <see cref="Nest"/> past its ceiling throws, before the position.</summary>
	const string NestSaid = "Rule 'T' came back to itself through @Still without reading anything too many times at ";

	/// <summary>And after it.</summary>
	const string Costly = "; the input is too costly for this grammar.";

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

		// A rule with no way back into it, dispatched by its lookaheads: checked by a method in front of it.
		("dispatched", "T = ?='.' & @Still & T | ?!'.' & 'x'",   "|x|.x"),
		("dispatchedd", "T = ?='.' & @Dot & T | ?!'.' & 'x'",    "|x|.x|..x"),

		// Nothing read after the call back: an entry where one stands is cut at once.
		("tail",      "T = @Still & T | 'a' & T | eof",          "|a|aa|ab|b"),
		("tailmutual", "T = @Still & U | 'a'\nU = @Still & T | 'b' & T", "|a|ba|bb|b"),
		("tailback",  "T = S & 'c'\nS = @Still & S | 'a' | 'a' & 'b'", "|ac|abc|ab|c"),
		("taillist",  "T = S & eof\nS = @Ws & S | Num & ('+' & S)?\nNum = ['0'..'9']+", "|1|1+1| 1 + 1|1+|+"),
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
	/// <remarks>
	/// Where the recursion is tried before the base, the parser without the check went round for ever on every input;
	/// cut, the entry that came back fails and the base is read, as rewritten left recursion would read it.
	/// </remarks>
	[Theory]
	[InlineData("nest",     "",      "no@0 Starved")]
	[InlineData("nest",     "x",     "ok@1")]
	[InlineData("nest",     "xxbb",  "ok@4")]
	[InlineData("nest",     "xb",    "ok@2")]
	[InlineData("nest",     "xbbb",  "ok@4")]
	[InlineData("nest",     "xbbbbb", "ok@6")]
	[InlineData("dot",      "x",     "ok@1")]
	[InlineData("dot",      "..xbb", "ok@5")]
	[InlineData("dot",      ".xb",   "ok@3")]
	[InlineData("dot",      ".x",    "no@2 Starved")]
	[InlineData("half",     ".xb",   "ok@3")]
	[InlineData("mutual",   "a",     "ok@1")]
	[InlineData("mutual",   "abb",   "ok@3")]
	[InlineData("frames",   "..",    "ok@2")]
	[InlineData("frames",   ".x",    "no@1 NoMatch")]
	[InlineData("framesh",  "..",    "ok@2")]
	[InlineData("framesh",  "x",     "no@0 NoMatch")]
	[InlineData("repeated", ".xb",   "ok@3")]
	[InlineData("repeatedd", "..xbb", "ok@5")]
	[InlineData("ahead",    "y",     "no@0 NoMatch")]
	[InlineData("ahead",    "x",     "ok@1")]
	[InlineData("notahead", "y",     "no@0 NoMatch")]
	[InlineData("notahead", "x",     "ok@1")]
	[InlineData("inahead",  "y",     "ok@1")]
	[InlineData("inahead",  "x",     "no@0 NoMatch")]
	[InlineData("dispatched", ".x",  "no@0 NoMatch")]
	[InlineData("dispatchedd", "..x", "ok@3")]
	[InlineData("tail",     "aa",    "ok@2")]
	[InlineData("tail",     "ab",    "no@1 NoMatch")]
	[InlineData("tailmutual", "ba",  "ok@2")]
	[InlineData("tailback", "abc",   "ok@3")]
	[InlineData("taillist", " 1+ 1", "ok@5")]
	[InlineData("taillist", " 1 + 1", "no@2 NoMatch")]
	[InlineData("taillist", "1+",    "no@2 Starved")]
	public void The_answers_are_the_rule_s(string name, string input, string expected)
	{
		var grammar = Table.Single(row => row.Name == name).Grammar;

		foreach (var direct in Environment.GetEnvironmentVariable("REENTRY_DIRECT") is null ? new[] { true, false } : new[] { true })
			Assert.Equal(expected, Answer(Build(grammar, SpanHost, direct, tape: false, buffered: false), input, typeof(string)));
	}

	/// <summary>
	/// The check is printed in both shapes a rule read by methods takes: in the method that reads the rule again
	/// where it has a way back into it, and in a method in front of it where it has none.
	/// </summary>
	[Fact]
	public void Both_shapes_of_the_reader_are_checked()
	{
		static string Source(string grammar)
		{
			return GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
			{
				ClassName      = "Probe",
				SymbolResolver = PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			}).Sources[0].Text;
		}

		var tape       = Source(Nest);
		var dispatched = Source(Table.Single(static row => row.Name == "dispatched").Grammar);
		var tail       = Source(Table.Single(static row => row.Name == "tail").Grammar);

		Assert.Contains("standing0 = reentry;", tape, StringComparison.Ordinal);
		Assert.Contains("standingAlike0 = reentryAlike;", tape, StringComparison.Ordinal);
		Assert.DoesNotContain("Reentering_T", tape, StringComparison.Ordinal);
		Assert.Contains("var read = Reentering_T(pos);", dispatched, StringComparison.Ordinal);

		// Where nothing is read after the call back, the entry is compared and nothing is counted.
		foreach (var cut in new[] { dispatched, tail })
		{
			Assert.Contains("standing0 = reentry;", cut, StringComparison.Ordinal);
			Assert.DoesNotContain("standingAlike0", cut, StringComparison.Ordinal);
			Assert.DoesNotContain("reentries", cut, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// A search hands over what it found before the start where too many entries came back, and then throws what the
	/// parse throws, in every form it is read in.
	/// </summary>
	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void A_search_throws_past_the_ceiling(bool buffered, bool direct)
	{
		var parser = Lowered(() => Build("T = 'y' | @Half & T & 'b'\nfind T", buffered ? Host : SpanHost, direct, tape: false, buffered, publish: false));
		var type   = parser.GetType("Probe")!;
		var forms  = new List<Func<System.Collections.IEnumerable>>
		{
			() => (System.Collections.IEnumerable)type.GetMethod("FindT", [typeof(string)])!.Invoke(null, ["yy.xbbbbbbbbbbbb"])!,
		};

		if (type.GetMethod("FindT", [typeof(TextReader)]) is { } streamed)
			forms.Add(() => (System.Collections.IEnumerable)streamed.Invoke(null, [new StringReader("yy.xbbbbbbbbbbbb")])!);

		if (type.GetMethod("FindT", [typeof(TextReader), typeof(int?), typeof(int?)]) is { } read)
			forms.Add(() => (System.Collections.IEnumerable)read.Invoke(null, [new StringReader("yy.xbbbbbbbbbbbb"), 1, 1 << 16])!);

		if (type.GetMethod("FindT", [typeof(Stream), typeof(int?), typeof(int?)]) is { } bytes)
			forms.Add(() => (System.Collections.IEnumerable)bytes.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes("yy.xbbbbbbbbbbbb")), 1, 1 << 16])!);

		// A search over a window is not offered where a recognizer is read (GRAM5001): over a reader the search
		// is the buffered one.
		Assert.Equal(buffered ? 3 : 1, forms.Count);

		foreach (var form in forms)
		{
			Reset(parser);

			var found      = new List<long>();
			var enumerator = form().GetEnumerator();
			var thrown     = Assert.Throws<InvalidOperationException>(() =>
			{
				while (enumerator.MoveNext())
					found.Add((long)enumerator.Current!.GetType().GetProperty("Position")!.GetValue(enumerator.Current)!);
			});

			Assert.Equal([0L, 1L], found);
			Assert.Equal("Rule 'T' came back to itself through @Half without reading anything too many times at 3" + Costly, thrown.Message);
		}
	}

	/// <summary>A sequence hands over the elements before the one where too many entries came back, and then throws.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_sequence_throws_past_the_ceiling(bool buffered)
	{
		var parser = Lowered(() => Build(
			"Item : @int = 'a' & ';' => @(1) | @Still & Item & 'b' => @(2)\nFeed : @int[] = Item*\nparse Feed yield : @int",
			buffered ? Host : SpanHost, direct: true, tape: false, buffered, publish: false));
		var type  = parser.GetType("Probe")!;
		var forms = new List<Func<IEnumerable<int>>>
		{
			() => (IEnumerable<int>)type.GetMethod("ParseFeed", [typeof(string)])!.Invoke(null, [Feed])!,
		};

		if (type.GetMethod("ParseFeed", [typeof(TextReader), typeof(int?), typeof(int?)]) is { } chars)
			forms.Add(() => (IEnumerable<int>)chars.Invoke(null, [new StringReader(Feed), 1, 1 << 16])!);

		if (type.GetMethod("ParseFeed", [typeof(Stream), typeof(int?), typeof(int?)]) is { } bytes)
			forms.Add(() => (IEnumerable<int>)bytes.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(Feed)), 1, 1 << 16])!);

		Assert.True(forms.Count > (buffered ? 1 : 0), $"{forms.Count} forms of the sequence");

		foreach (var form in forms)
		{
			Reset(parser);

			var elements   = new List<int>();
			var enumerator = form().GetEnumerator();
			var thrown     = Assert.Throws<InvalidOperationException>(() =>
			{
				while (enumerator.MoveNext())
					elements.Add(enumerator.Current);
			});

			Assert.Equal([1, 1], elements);
			Assert.Equal("Rule 'Item' came back to itself through @Still without reading anything too many times at 4" + Costly, thrown.Message);
		}
	}

	/// <summary>A sequence whose third element comes back past the ceiling: <c>x</c> and then the <c>b</c>s it would need.</summary>
	const string Feed = "a;a;xbbbbbbbbbbbb";

	/// <summary>A repetition marked <c>recover</c> does not step over an element past the ceiling: the reading throws.</summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_recovery_does_not_step_over_it(bool direct)
	{
		var parser = Lowered(() => Build(
			"Item : @int = 'a' & ';' => @(1) | @Still & Item & 'b' => @(2)\nT : @int[] = Item* recover ';' & eof",
			SpanHost, direct, tape: false, buffered: false));

		Assert.Equal("ok@6", Answer(parser, "a;a;a;", typeof(string)));
		Assert.Equal("threw Rule 'Item' came back to itself through @Still without reading anything too many times at 2" + Costly, Answer(parser, "a;xbbbbbbbbbbbb;a;", typeof(string)));
	}

	/// <summary>What a construction threw and is held until the parse is accepted does not stand in for the throw at the ceiling.</summary>
	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Deferred)]
	public void A_held_fault_is_dropped_with_the_parse(CarrierKind carrier)
	{
		var result = Lowered(() => GramCompiler.Compile(
			"A : @int = 'q' => @(Boom())\nT : @int = a: A & n: N => @(a)\nN : @int = @Still & n: N & 'b' => @(n) | 'x' => @(0)\nparse T",
			new GramCompilerOptions
			{
				ClassName             = "Probe",
				Carrier               = carrier,
				ConstructionsMayThrow = true,
				SymbolResolver        = PermissiveSymbolResolver.Instance,
				CSharpScanner         = RoslynCSharpScanner.Instance,
			}));

		EmittedCode.Quiet(result.Diagnostics);

		var parser = EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe",
			declarationMembers: SpanHost + "static int Boom() { throw new System.InvalidOperationException(\"boom\"); }",
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		Assert.Equal("threw Rule 'N' came back to itself through @Still without reading anything too many times at 1" + Costly, Answer(parser, "qxbbbbbbbbbbbb", typeof(string)));
	}

	/// <summary>
	/// A recursion that ends only because a construction wrote the context and the recognizer read it on the next
	/// entry at the same position has its entries counted, not compared: what the context says is not the position's.
	/// The count assumes it is, so the answer may differ from the parser's without the check; it ends.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_recursion_that_moves_only_through_the_context_ends(bool direct)
	{
		const string Members = """
			[System.ThreadStatic] static int calls;
			public static void Reset() { calls = 0; }
			public sealed class Ctx { public int Bumps; public int Bump() { return ++Bumps; } }
			static bool Stand(ParserInput<char> input, ref int p, Ctx context) { if (++calls > 10000) throw new System.InvalidOperationException("went round"); if (context.Bumps > 0 && input.Peek(p, out var c) && c == '.') p++; return true; }
			public static object Read(string input) { return TryParseT(input, new Ctx()); }
			""";

		const string Grammar = "context : @Ctx\nA : @int = \"\" => @(context.Bump())\nT : @int = @Stand & a: A & n: T => @(n) | 'x' => @(0)\nparse T";

		var result = GramCompiler.Compile(
			Grammar,
			new GramCompilerOptions
			{
				ClassName      = "Probe",
				Direct         = direct,
				SymbolResolver = ContextResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		var parser = EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: Members,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		Assert.Contains("reentries", result.Sources[0].Text, StringComparison.Ordinal);

		Reset(parser);

		var match = parser.GetType("Probe")!.GetMethod("Read")!.Invoke(null, [".x"])!;

		// What it answers depends on the reading: the context moves the recognizer at one entry and not at another,
		// and the reader and the engine enter in other orders.
		Assert.Matches("^(ok|no)@", Said(match));
	}

	/// <summary>Says every recognizer reads the input view and the context.</summary>
	sealed class ContextResolver : ISymbolResolver
	{
		public static readonly ContextResolver Instance = new();

		static readonly StandingTurnTests.ViewResolver View = StandingTurnTests.ViewResolver.Instance;

		public bool TypeExists(string qualifiedName)
		{
			return View.TypeExists(qualifiedName);
		}

		public bool IsAssignable(string from, string to)
		{
			return View.IsAssignable(from, to);
		}

		public bool TryResolveConstructors(string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
		{
			return View.TryResolveConstructors(qualifiedName, out constructors);
		}

		public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
		{
			return View.TryResolveSettableProperties(qualifiedName, out properties);
		}

		public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
		{
			return View.TryResolveExternalValue(methodName, against, out valueType);
		}

		public ExternalMethodResolution ResolveExternalMethod(string methodName, ExternalMethodRole role)
		{
			return role == ExternalMethodRole.Recognizer ? ExternalMethodResolution.FoundInputWithContext : View.ResolveExternalMethod(methodName, role);
		}

		public bool Rewinds(string qualifiedName)
		{
			return View.Rewinds(qualifiedName);
		}

		public bool IsReferenceType(string qualifiedName)
		{
			return View.IsReferenceType(qualifiedName);
		}

		public bool IsNullable(string qualifiedName)
		{
			return View.IsNullable(qualifiedName);
		}
	}

	/// <summary>
	/// A rule read at a strength reads its operators after the call it climbs from, so its entries are counted, by its
	/// keys: as many a position as strengths it is entered at.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_rule_read_at_a_strength_is_counted_by_its_key(bool direct)
	{
		const string Grammar = "T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'x'";

		var graph = ExponentialRepetitionTests.Graph(Grammar + "\nparse T", lexical: false, out _);

		Assert.False(Reentry.Tail(graph, graph.Rules.Single(static rule => rule.Name == "E")));

		var parser = Build(Grammar, SpanHost, direct, tape: false, buffered: false);

		Assert.Equal("ok@3", Answer(parser, "x+x", typeof(string)));
		Assert.Equal("no@0 NoMatch", Answer(parser, "+", typeof(string)));

		var moving = Build(Grammar.Replace("@Still", "@Dot", StringComparison.Ordinal), SpanHost, direct, tape: false, buffered: false);

		Assert.Equal("ok@4", Answer(moving, ".x+x", typeof(string)));
		Assert.Equal("ok@3", Answer(moving, "x+x", typeof(string)));
	}

	/// <summary>Two specializations of one rule that call each other with their arguments turned round: each is its own rule, and each is counted.</summary>
	[Fact]
	public void Alternating_arguments_come_back()
	{
		var parser = Build("T = P('a', 'b')\nP(x, y) = @Still & P(y, x) & x | y", SpanHost, direct: false, tape: false, buffered: false);

		Assert.Equal("ok@1", Answer(parser, "b", typeof(string)));
		Assert.Equal("ok@2", Answer(parser, "aa", typeof(string)));
		Assert.Equal("no@1 NoMatch", Answer(parser, "ab", typeof(string)));
	}

	/// <summary>A trace build throws past the ceiling as the parse does, and a parse after it is told as its own.</summary>
	[Fact]
	public void A_trace_build_throws_past_the_ceiling()
	{
		var parser = Lowered(() => Traced(Nest));
		var thrown = Assert.Throws<TargetInvocationException>(() => Recorded(parser, "xbbbbbbbbbbbb"));

		Assert.Equal(NestSaid + "0" + Costly, Thrown(thrown));

		var lines = Recorded(parser, "x");

		Assert.Equal("enter 0", lines[0]);
		Assert.DoesNotContain(lines, static line => line.StartsWith("rejected", StringComparison.Ordinal));
	}

	/// <summary>
	/// A reading carried onto stacks of its own comes back to a rule there, and is cut as on the stack it began on: at
	/// every depth around where the first stack runs low — so that at one of them the entry that came back is the
	/// one carried over, its rule's record crossing with it — the reading enters what it enters on a stack that never
	/// runs low, for an entry cut at once and for one counted.
	/// </summary>
	[Theory]
	[InlineData("T = '(' & T | @Still & T | 'x'", "")]
	[InlineData("T = '(' & T | @Still & T & 'b' | 'x'", "b")]
	public void A_deep_reading_is_cut_on_another_stack(string grammar, string after)
	{
		var parser = Traced(grammar);
		var lines  = OnStack(256 * 1024, () => Recorded(parser, new string('(', 5000) + "x" + after));
		var first  = lines.First(static line => line.StartsWith("deepened ", StringComparison.Ordinal));
		var at     = int.Parse(first.Substring("deepened ".Length), System.Globalization.CultureInfo.InvariantCulture);
		var split  = 0;

		for (var depth = Math.Max(1, at - 24); depth <= at + 8; depth++)
		{
			var input = new string('(', depth) + "x" + after;
			var read  = OnStack(256 * 1024, () => Recorded(parser, input));
			var whole = OnStack(64 << 20, () => Recorded(parser, input));

			Assert.Equal(whole, read.Where(static line => !line.StartsWith("deepened ", StringComparison.Ordinal)));
			Assert.Contains("enter " + depth, read);

			if (read.Contains("deepened " + depth))
				split++;
		}

		Assert.True(split > 0, $"No depth around {at} carried the entry that came back onto another stack.");
	}

	static string[] OnStack(int size, Func<string[]> read)
	{
		string[]?  lines  = null;
		Exception? thrown = null;

		var thread = new System.Threading.Thread(() =>
		{
			try
			{
				lines = read();
			}
			catch (Exception caught)
			{
				thrown = caught;
			}
		}, size);

		thread.Start();
		thread.Join();

		Assert.Null(thrown);

		return lines!;
	}

	/// <summary>The host of a trace build: the span recognizers, and a sink that writes down what it is told.</summary>
	const string TracedHost = SpanHost + """
		public sealed class Recording : GramTrace
		{
			public readonly System.Collections.Generic.List<string> Lines = new System.Collections.Generic.List<string>();
			public override void Enter(int rule, long position) { Lines.Add("enter " + position); }
			public override void Deepened(long position) { Lines.Add("deepened " + position); }
			public override void Rejected(long position, string message) { Lines.Add("rejected " + position + " " + message); }
		}
		public static string[] Recorded(string input)
		{
			var sink = new Recording();
			using (Tracing(sink))
				TryParseT(input);
			return sink.Lines.ToArray();
		}
		public static string Explained(string input)
		{
			var why = new GramWhy();
			using (Tracing(why))
				TryParseT(input);
			return why.ToString();
		}
		""";

	static Assembly Traced(string grammar)
	{
		var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
		{
			ClassName      = "Probe",
			Trace          = true,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: TracedHost,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static string[] Recorded(Assembly parser, string input)
	{
		Reset(parser);

		return (string[])parser.GetType("Probe")!.GetMethod("Recorded")!.Invoke(null, [input])!;
	}

	static string Explained(Assembly parser, string input)
	{
		Reset(parser);

		return (string)parser.GetType("Probe")!.GetMethod("Explained")!.Invoke(null, [input])!;
	}

	/// <summary>
	/// A pass that copies a rule for the site it is read at — a run settled for what follows it — copies no rule
	/// that reads a marked one: the copy would be read past the check. The parse reads as the rule itself does.
	/// </summary>
	[Fact]
	public void A_rule_that_reads_a_marked_one_is_not_copied()
	{
		const string Grammar = "T = Name & '='\nName = @Pfx & Name | ['a'..'z']+";

		var graph = ExponentialRepetitionTests.Graph(Grammar + "\nparse T", lexical: false, out _);
		var name  = graph.Rules.Single(static rule => rule.Name == "Name");

		Assert.True(Reentry.Marked(graph, name));
		Assert.False(Reentry.MayCopy(graph, name));
		Assert.False(Reentry.MayCopy(graph, graph.Rules.Single(static rule => rule.Name == "T")));
		Assert.Single(graph.Rules, rule => ReferenceEquals(graph.Origin(rule), name) || rule.Declaration?.Name == "Name");

		const string Moves = "static bool Pfx(System.ReadOnlySpan<char> input, ref int p) { if (p < input.Length && input[p] == '.') p++; return true; }";

		foreach (var direct in new[] { true, false })
		{
			var parser = Build(Grammar, SpanHost + Moves, direct, tape: false, buffered: false);

			Assert.Equal("ok@4", Answer(parser, ".ab=", typeof(string)));
			Assert.Equal("ok@3", Answer(parser, "ab=", typeof(string)));
			Assert.Equal("no@2 NoMatch", Answer(parser, "..=", typeof(string)));
		}
	}

	/// <summary>
	/// What a rule begins with settles on every marked grammar: a recognizer begins with anything, so what is
	/// read after it is never asked, and a cycle through one is no ring of rules leading with each other (GRAM4057).
	/// </summary>
	[Theory]
	[MemberData(nameof(Rows))]
	public void What_a_marked_rule_begins_with_settles(string name, string grammar, string inputs)
	{
		_ = inputs;

		var graph = ExponentialRepetitionTests.Graph(grammar + "\nparse T", lexical: false, out _);

		Assert.True(Reentry.Any(graph), name);
		Assert.Null(FirstSets.Unsettled(graph));
	}

	/// <summary>A cycle through a rule made out of a nested recovery is found on the rule as made, and told once.</summary>
	[Fact]
	public void A_cycle_through_a_lifted_rule_is_told()
	{
		const string Grammar = "T = ((@Still & T & 'b' | 'x')* recover ',' & ';')* recover ';' & eof";

		var graph = ExponentialRepetitionTests.Graph(Grammar + "\nparse T", lexical: false, out _);

		Assert.Contains(graph.Rules, rule => graph.Lifted.ContainsKey(rule) && Reentry.Marked(graph, rule));
		Assert.Single(Reported(Grammar), static one => one.Id == Reentry.Reenters);

		foreach (var direct in new[] { true, false })
			Assert.Equal("ok@2", Answer(Build(Grammar, SpanHost, direct, tape: false, buffered: false), "x;", typeof(string)));
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

	/// <summary>
	/// No grammar that ships — the packages' and the examples' — has a rule the check is written into: the
	/// check is for grammars written elsewhere, and every shipped parser is generated as it was before it.
	/// </summary>
	/// <remarks>
	/// Read as <c>ExponentialRepetitionTests</c> reads the repository: every file and every <c>[Gram]</c>,
	/// joined with what it includes, over characters and over kinds where it is compiled so.
	/// </remarks>
	[Fact]
	public void No_shipped_grammar_is_checked()
	{
		var read   = 0;
		var marked = new List<string>();

		foreach (var below in new[] { "src", "examples" })
			foreach (var grammar in ExponentialRepetitionTests.Grammars(Path.Combine(SourceRoot.Directory, below)))
				foreach (var lexical in grammar.Modes)
				{
					var graph = ExponentialRepetitionTests.Built(grammar, lexical, out var split);

					if (graph.HasErrors)
						continue;

					read++;

					foreach (var rule in Reentry.MarkedRules(split?.Syntax ?? graph))
						marked.Add($"{below}: {grammar.Name} [{(lexical ? "kinds" : "chars")}] {rule.Name}");
				}

		TestContext.Current.SendDiagnosticMessage($"{read} readings of shipped grammars, {marked.Count} rules checked.");

		Assert.True(read > 30, $"Only {read} shipped grammars were read; the walk over the repository is broken.");
		Assert.Empty(marked);
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

		object match;

		try
		{
			match = over == typeof(string)
				? method.Invoke(null, [input])!
				: method.Invoke(null, [over == typeof(Stream) ? new MemoryStream(bytes) : new StringReader(input), 1, 1 << 16])!;
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is InvalidOperationException)
		{
			return "threw " + Thrown(thrown);
		}

		return Said(match);
	}

	/// <summary>What a reading past its ceiling threw, said by its message.</summary>
	static string Thrown(TargetInvocationException thrown)
	{
		var inner = Assert.IsType<InvalidOperationException>(thrown.InnerException);

		return inner.Message;
	}

	/// <summary>What <paramref name="make"/> makes, with the ceiling on the entries that come back lowered to three.</summary>
	static T Lowered<T>(Func<T> make)
	{
		Reentry.Ceiling = 3;

		try
		{
			return make();
		}
		finally
		{
			Reentry.Ceiling = null;
		}
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
