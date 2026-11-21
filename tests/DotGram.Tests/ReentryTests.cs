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

	/// <summary>A search hands over what it found before the start the grammar cannot be read at, and then throws, in every form it is read in.</summary>
	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void A_search_throws_where_a_rule_comes_back(bool buffered, bool direct)
	{
		const string Said = "Rule 'T' came back to itself through @Half without reading anything at 3.";

		var parser = Build("T = 'y' | @Half & T & 'b'\nfind T", buffered ? Host : SpanHost, direct, tape: false, buffered, publish: false);
		var type   = parser.GetType("Probe")!;
		var forms  = new List<Func<System.Collections.IEnumerable>>
		{
			() => (System.Collections.IEnumerable)type.GetMethod("FindT", [typeof(string)])!.Invoke(null, ["yy.x"])!,
		};

		if (type.GetMethod("FindT", [typeof(TextReader)]) is { } streamed)
			forms.Add(() => (System.Collections.IEnumerable)streamed.Invoke(null, [new StringReader("yy.x")])!);

		if (type.GetMethod("FindT", [typeof(TextReader), typeof(int?), typeof(int?)]) is { } read)
			forms.Add(() => (System.Collections.IEnumerable)read.Invoke(null, [new StringReader("yy.x"), 1, 1 << 16])!);

		if (type.GetMethod("FindT", [typeof(Stream), typeof(int?), typeof(int?)]) is { } bytes)
			forms.Add(() => (System.Collections.IEnumerable)bytes.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes("yy.x")), 1, 1 << 16])!);

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
			Assert.Equal(Said, thrown.Message);
		}
	}

	/// <summary>A sequence hands over the elements before the one that comes back, and then throws what a malformed input throws there.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_sequence_throws_where_a_rule_comes_back(bool buffered)
	{
		var parser = Build(
			"Item : @int = 'a' & ';' => @(1) | @Still & Item & 'b' => @(2)\nFeed : @int[] = Item*\nparse Feed yield : @int",
			buffered ? Host : SpanHost, direct: true, tape: false, buffered, publish: false);
		var type  = parser.GetType("Probe")!;
		var forms = new List<Func<IEnumerable<int>>>
		{
			() => (IEnumerable<int>)type.GetMethod("ParseFeed", [typeof(string)])!.Invoke(null, ["a;a;x"])!,
		};

		if (type.GetMethod("ParseFeed", [typeof(TextReader), typeof(int?), typeof(int?)]) is { } chars)
			forms.Add(() => (IEnumerable<int>)chars.Invoke(null, [new StringReader("a;a;x"), 1, 1 << 16])!);

		if (type.GetMethod("ParseFeed", [typeof(Stream), typeof(int?), typeof(int?)]) is { } bytes)
			forms.Add(() => (IEnumerable<int>)bytes.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes("a;a;x")), 1, 1 << 16])!);

		Assert.True(forms.Count > (buffered ? 1 : 0), $"{forms.Count} forms of the sequence");

		foreach (var form in forms)
		{
			Reset(parser);

			var elements   = new List<int>();
			var enumerator = form().GetEnumerator();
			var thrown     = Assert.Throws<FormatException>(() =>
			{
				while (enumerator.MoveNext())
					elements.Add(enumerator.Current);
			});

			Assert.Equal([1, 1], elements);
			Assert.Equal("Rule 'Item' came back to itself through @Still without reading anything at 4.", thrown.Message);
		}
	}

	/// <summary>A repetition marked <c>recover</c> does not step over an element that came back: the input shows a defect of the grammar, not a bad element.</summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_recovery_does_not_step_over_it(bool direct)
	{
		var parser = Build(
			"Item : @int = 'a' & ';' => @(1) | @Still & Item & 'b' => @(2)\nT : @int[] = Item* recover ';' & eof",
			SpanHost, direct, tape: false, buffered: false);

		Assert.Equal("ok@6", Answer(parser, "a;a;a;", typeof(string)));
		Assert.Equal("no@2 NoMatch Rule 'Item' came back to itself through @Still without reading anything.", Answer(parser, "a;x;a;", typeof(string)));
	}

	/// <summary>What a construction threw and is held until the parse is accepted is dropped with it, as for any refusal.</summary>
	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Deferred)]
	public void A_held_fault_is_dropped_with_the_parse(CarrierKind carrier)
	{
		var result = GramCompiler.Compile(
			"A : @int = 'q' => @(Boom())\nT : @int = a: A & n: N => @(a)\nN : @int = @Still & n: N & 'b' => @(n) | 'x' => @(0)\nparse T",
			new GramCompilerOptions
			{
				ClassName             = "Probe",
				Carrier               = carrier,
				ConstructionsMayThrow = true,
				SymbolResolver        = PermissiveSymbolResolver.Instance,
				CSharpScanner         = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		var parser = EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe",
			declarationMembers: SpanHost + "static int Boom() { throw new System.InvalidOperationException(\"boom\"); }",
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		Assert.Equal("no@1 NoMatch Rule 'N' came back to itself through @Still without reading anything.", Answer(parser, "qx", typeof(string)));
	}

	/// <summary>
	/// A recursion that ends only because a construction wrote the context and the recognizer read it on the next
	/// entry at the same position is refused there: progress belongs in the position, not in the context.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_recursion_that_moves_only_through_the_context_is_refused(bool direct)
	{
		const string Members = """
			[System.ThreadStatic] static int calls;
			public static void Reset() { calls = 0; }
			public sealed class Ctx { public int Bumps; public int Bump() { return ++Bumps; } }
			static bool Stand(ParserInput<char> input, ref int p, Ctx context) { if (++calls > 10000) throw new System.InvalidOperationException("went round"); if (context.Bumps > 0 && input.Peek(p, out var c) && c == '.') p++; return true; }
			public static object Read(string input) { return TryParseT(input, new Ctx()); }
			""";

		var result = GramCompiler.Compile(
			"context : @Ctx\nA : @int = \"\" => @(context.Bump())\nT : @int = @Stand & a: A & n: T & 'b' => @(n) | 'x' => @(0)\nparse T",
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

		Reset(parser);

		var match = parser.GetType("Probe")!.GetMethod("Read")!.Invoke(null, [".xb"])!;

		Assert.Equal("no@0 NoMatch Rule 'T' came back to itself through @Stand without reading anything.", Said(match));
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

	/// <summary>A rule read at a strength comes back with the same strength only after as many entries as it has strengths, and is refused then.</summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_rule_read_at_a_strength_is_refused_by_its_key(bool direct)
	{
		const string Grammar = "T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'x'";

		var parser = Build(Grammar, SpanHost, direct, tape: false, buffered: false);

		Assert.Equal("no@0 NoMatch Rule 'E' came back to itself through @Still without reading anything.", Answer(parser, "x+x", typeof(string)));

		var moving = Build(Grammar.Replace("@Still", "@Dot", StringComparison.Ordinal), SpanHost, direct, tape: false, buffered: false);

		Assert.Equal("ok@4", Answer(moving, ".x+x", typeof(string)));
		Assert.Equal("ok@3", Answer(moving, "x+x", typeof(string)));
	}

	/// <summary>Two specializations of one rule that call each other with their arguments turned round: each is its own rule, and each comes back.</summary>
	[Fact]
	public void Alternating_arguments_come_back()
	{
		var parser = Build("T = P('a', 'b')\nP(x, y) = @Still & P(y, x) & x | y", SpanHost, direct: false, tape: false, buffered: false);

		Assert.StartsWith("no@0 NoMatch Rule 'P' came back to itself", Answer(parser, "b", typeof(string)), StringComparison.Ordinal);
	}

	/// <summary>The trace is told the refusal as the match says it, and GramWhy explains it so.</summary>
	[Fact]
	public void A_trace_says_the_refusal()
	{
		var parser = Traced(Nest);
		var lines  = Recorded(parser, "xb");

		Assert.Contains("rejected 0 " + NestSaid, lines);
		Assert.StartsWith(NestSaid, Explained(parser, "xb"), StringComparison.Ordinal);

		// And a parse after it is told as its own: the reading the refusal left is not still open.
		Assert.Contains("rejected 0 " + NestSaid, Recorded(parser, "xb"));
	}

	/// <summary>
	/// A reading carried onto stacks of its own comes back to a rule there, and is refused with the message; at
	/// every depth around where the first stack runs low — so that at one of them the entry that came back is the
	/// one carried over, its rule's record crossing with it — the rule is entered once at the position it comes back to.
	/// </summary>
	[Fact]
	public void A_deep_reading_is_refused_on_another_stack()
	{
		var parser = Traced("T = '(' & T & ')' | @Still & T & 'b' | 'x'");
		var lines  = OnSmallStack(() => Recorded(parser, new string('(', 5000) + "x"));
		var first  = lines.First(static line => line.StartsWith("deepened ", StringComparison.Ordinal));
		var at     = int.Parse(first.Substring("deepened ".Length), System.Globalization.CultureInfo.InvariantCulture);

		Assert.Contains("rejected 5000 " + Said("T", "Still"), lines);

		var split = 0;

		for (var depth = Math.Max(0, at - 24); depth <= at + 8; depth++)
		{
			var read = OnSmallStack(() => Recorded(parser, new string('(', depth) + "x"));

			Assert.Contains("rejected " + depth + " " + Said("T", "Still"), read);
			Assert.Single(read, line => line == "enter " + depth);

			if (read.Contains("deepened " + depth))
				split++;
		}

		Assert.True(split > 0, $"No depth around {at} carried the entry that came back onto another stack.");
	}

	static string Said(string rule, string recognizer)
	{
		return $"Rule '{rule}' came back to itself through @{recognizer} without reading anything.";
	}

	static string[] OnSmallStack(Func<string[]> read)
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
		}, 256 * 1024);

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
