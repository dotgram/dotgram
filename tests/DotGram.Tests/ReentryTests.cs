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
/// grammar compiles and is told so (GRAM5021), and where nothing is read after the call back, an entry that
/// comes back to a rule where it already stands is cut as an alternative that failed, in every reading,
/// instead of going round for ever. Where something is, the rule is read as it is written.
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

	/// <summary>A cycle with something read after the call back: found and told, and not checked.</summary>
	const string Nest = "T = @Still & n: T & 'b' | 'x'+";

	static readonly (string Name, string Grammar, string Inputs)[] Table =
	[
		// A cycle with something read after the call back, over a recognizer that moves or refuses: not checked, and
		// it ends as it always did.
		("dot",       "T = @Dot & n: T & 'b' | 'x'+",            "|x|.xb|..xbb|..x|.x|b"),

		// A cons list: the recursion a repetition is the better spelling of.
		("frames",    "T = @Dot & T | eof",                      "|.|..|.x"),
		("framesh",   "T = @Half & T | eof",                     "|.|..|x"),

		// A recognizer in a repetition before the recursion, which moves or refuses.
		("repeatedd", "T = (@Dot)+ & n: T & 'b' | 'x'",          "|x|.xb|..xbb|..x"),

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
	[InlineData("dot",      "x",     "ok@1")]
	[InlineData("dot",      "..xbb", "ok@5")]
	[InlineData("dot",      ".xb",   "ok@3")]
	[InlineData("dot",      ".x",    "no@2 Starved")]
	[InlineData("frames",   "..",    "ok@2")]
	[InlineData("frames",   ".x",    "no@1 NoMatch")]
	[InlineData("framesh",  "..",    "ok@2")]
	[InlineData("framesh",  "x",     "no@0 NoMatch")]
	[InlineData("repeatedd", "..xbb", "ok@5")]
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
	/// where it has a way back into it, and in a method in front of it where it has none; and nothing of it is printed
	/// for a cycle with something read after the call back.
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

		var plain      = Source(Nest);
		var dispatched = Source(Table.Single(static row => row.Name == "dispatched").Grammar);
		var tail       = Source(Table.Single(static row => row.Name == "tail").Grammar);

		Assert.DoesNotContain("standing", plain, StringComparison.Ordinal);
		Assert.DoesNotContain("reentr", plain, StringComparison.OrdinalIgnoreCase);
		Assert.Contains("var read = Reentering_T(pos);", dispatched, StringComparison.Ordinal);
		Assert.DoesNotContain("Reentering_T", tail, StringComparison.Ordinal);

		foreach (var cut in new[] { dispatched, tail })
			Assert.Contains("standing0 = reentry;", cut, StringComparison.Ordinal);
	}

	/// <summary>A search finds what a cycle cut at once reads, in every form it is read in.</summary>
	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void A_search_reads_through_a_cut(bool buffered, bool direct)
	{
		const string Input = "yb.y.xby";

		var parser = Build("T = @Half & T | 'y'\nfind T", buffered ? Host : SpanHost, direct, tape: false, buffered, publish: false);
		var type   = parser.GetType("Probe")!;
		var forms  = new List<Func<System.Collections.IEnumerable>>
		{
			() => (System.Collections.IEnumerable)type.GetMethod("FindT", [typeof(string)])!.Invoke(null, [Input])!,
		};

		if (type.GetMethod("FindT", [typeof(TextReader), typeof(int?), typeof(int?)]) is { } read)
			forms.Add(() => (System.Collections.IEnumerable)read.Invoke(null, [new StringReader(Input), 1, 1 << 16])!);

		if (type.GetMethod("FindT", [typeof(Stream), typeof(int?), typeof(int?)]) is { } bytes)
			forms.Add(() => (System.Collections.IEnumerable)bytes.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(Input)), 1, 1 << 16])!);

		// A search over a window is not offered where a recognizer is read (GRAM5001): over a reader the search
		// is the buffered one.
		Assert.Equal(buffered ? 3 : 1, forms.Count);

		foreach (var form in forms)
		{
			Reset(parser);

			var found = new List<string>();

			foreach (var match in form())
				found.Add(match.GetType().GetProperty("Position")!.GetValue(match) + "+" + match.GetType().GetProperty("Length")!.GetValue(match));

			Assert.Equal(["0+1", "2+2", "7+1"], found);
		}
	}

	/// <summary>A sequence hands over its elements through a cycle cut at once, and a repetition marked <c>recover</c> steps over one it refuses.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_sequence_reads_through_a_cut(bool buffered)
	{
		const string Feed = "a;a;.a;";

		var parser = Build(
			"Item : @int = 'a' & ';' => @(1) | @Half & i: Item => @(i + 1)\nFeed : @int[] = Item*\nparse Feed yield : @int",
			buffered ? Host : SpanHost, direct: true, tape: false, buffered, publish: false);
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

			Assert.Equal([1, 1, 2], form().ToList());
		}

		foreach (var direct in new[] { true, false })
		{
			var recovering = Build(
				"Item : @int = 'a' & ';' => @(1) | @Still & Item => @(2)\nT : @int[] = Item* recover ';' & eof",
				SpanHost, direct, tape: false, buffered: false);

			Assert.Equal("ok@6", Answer(recovering, "a;a;a;", typeof(string)));
			Assert.Equal("ok@6", Answer(recovering, "a;x;a;", typeof(string)));
		}
	}

	/// <summary>
	/// A recursion that ends only because a construction wrote the context and the recognizer read it on the next
	/// entry at the same position is not checked: what the context says is not the position's. It is written as the
	/// parser without the check is.
	/// </summary>
	[Fact]
	public void A_recursion_that_moves_only_through_the_context_is_not_checked()
	{
		const string Grammar = "context : @Ctx\nA : @int = \"\" => @(context.Bump())\nT : @int = @Stand & a: A & n: T => @(n) | 'x' => @(0)\nparse T";

		foreach (var direct in new[] { true, false })
		{
			var sources = new[] { false, true }.Select(unchecked_ =>
			{
				var was = Reentry.Unchecked;

				try
				{
					Reentry.Unchecked = unchecked_;

					var result = GramCompiler.Compile(
						Grammar,
						new GramCompilerOptions
						{
							ClassName      = "Probe",
							Direct         = direct,
							SymbolResolver = ContextResolver.Instance,
							CSharpScanner  = RoslynCSharpScanner.Instance,
						});

					Assert.Equal(!unchecked_, result.Diagnostics.Any(static one => one.Id == Reentry.Reenters));

					return string.Join("\n", result.Sources.Select(static one => one.Text));
				}
				finally
				{
					Reentry.Unchecked = was;
				}
			}).ToArray();

			Assert.Equal(sources[1], sources[0]);
		}
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
	/// A rule read at a strength reads its operators after the call it climbs from, so it is not checked; over a
	/// recognizer that moves or refuses it reads as it always did.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_rule_read_at_a_strength_is_not_checked(bool direct)
	{
		const string Grammar = "T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'x'";

		var graph = ExponentialRepetitionTests.Graph(Grammar + "\nparse T", lexical: false, out _);
		var e     = graph.Rules.Single(static rule => rule.Name == "E");

		Assert.True(Reentry.Found(graph, e));
		Assert.False(Reentry.Marked(graph, e));

		var moving = Build(Grammar.Replace("@Still", "@Dot", StringComparison.Ordinal), SpanHost, direct, tape: false, buffered: false);

		Assert.Equal("ok@4", Answer(moving, ".x+x", typeof(string)));
		Assert.Equal("ok@3", Answer(moving, "x+x", typeof(string)));
	}

	/// <summary>
	/// A reading carried onto stacks of its own comes back to a rule there, and is cut as on the stack it began on: at
	/// every depth around where the first stack runs low — so that at one of them the entry that came back is the
	/// one carried over, its rule's record crossing with it — the reading enters what it enters on a stack that never
	/// runs low.
	/// </summary>
	[Theory]
	[InlineData("T = '(' & T | @Still & T | 'x'", "")]
	[InlineData("T = S & 'b'\nS = '(' & S | @Still & S | 'x'", "b")]
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
	/// A marked rule behind a caller (`Name = @Pfx & Name | ['a'..'z']+`) reads `.ab=` and `ab=` and refuses `..=`,
	/// by methods and in the engine.
	/// </summary>
	[Fact]
	public void A_marked_rule_read_by_a_caller_parses_as_written()
	{
		const string Grammar = "T = Name & '='\nName = @Pfx & Name | ['a'..'z']+";

		var graph = ExponentialRepetitionTests.Graph(Grammar + "\nparse T", lexical: false, out _);
		var name  = graph.Rules.Single(static rule => rule.Name == "Name");

		Assert.True(Reentry.Marked(graph, name));
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

		Assert.True(Reentry.FoundRules(graph).Count > 0, name);
		Assert.Null(FirstSets.Unsettled(graph));
	}

	/// <summary>
	/// A cycle through a rule made out of a nested recovery is found on the rule as made, and told once; cut at once
	/// where nothing is read after the call back.
	/// </summary>
	[Fact]
	public void A_cycle_through_a_lifted_rule_is_told()
	{
		const string Counted = "T = ((@Still & T & 'b' | 'x')* recover ',' & ';')* recover ';' & eof";
		const string Cut     = "T = ((@Still & S | 'x')* recover ',' & ';')* recover ';' & eof\nS = @Still & S | 'y'";

		var graph = ExponentialRepetitionTests.Graph(Counted + "\nparse T", lexical: false, out _);

		Assert.Contains(graph.Rules, rule => graph.Lifted.ContainsKey(rule) && Reentry.Found(graph, rule));
		Assert.False(Reentry.Any(graph));
		Assert.Single(Reported(Counted), static one => one.Id == Reentry.Reenters);
		Assert.Single(Reported(Cut), static one => one.Id == Reentry.Reenters);

		foreach (var direct in new[] { true, false })
		{
			var parser = Build(Cut, SpanHost, direct, tape: false, buffered: false);

			Assert.Equal("ok@2", Answer(parser, "x;", typeof(string)));
			Assert.Equal("ok@4", Answer(parser, "y,x;", typeof(string)));
		}
	}

	/// <summary>
	/// The build says so once a cycle, as Information, on the rule that closes it, and whether an entry is cut at once
	/// or the rule read as it is written; and nothing of a grammar without one.
	/// </summary>
	[Theory]
	[InlineData(Nest, "'T' can come back to itself without reading anything where @Still succeeds", false)]
	[InlineData("T = @Still & B | 'a'\nB = T & 'b'", "'T' can come back to itself through 'B' without reading anything where @Still succeeds", false)]
	[InlineData("T = @Dot & T | eof", "'T' can come back to itself without reading anything where @Dot succeeds", true)]
	[InlineData("T = @Still & B | 'a'\nB = 'b' | T", "'T' can come back to itself through 'B' without reading anything where @Still succeeds", true)]
	public void A_cycle_is_told_once(string grammar, string told, bool cut)
	{
		var said = Reported(grammar).Where(static one => one.Id == Reentry.Reenters).ToArray();

		Assert.Equal(GramSeverity.Info, Assert.Single(said).Severity);
		Assert.Contains(told, said[0].Message, StringComparison.Ordinal);
		Assert.Equal(cut, said[0].Message.Contains("fails as an alternative does instead of going round for ever", StringComparison.Ordinal));
		Assert.Equal(!cut, said[0].Message.Contains("goes round until the stack or memory runs out", StringComparison.Ordinal));
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
	/// No grammar that ships — the packages' and the examples' — has a rule that can come back to itself where it
	/// stands: the check is for grammars written elsewhere, and every shipped parser is generated as it was before it.
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

					foreach (var rule in Reentry.FoundRules(split?.Syntax ?? graph))
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
		catch (TargetInvocationException thrown) when (thrown.InnerException is InvalidOperationException inner)
		{
			return "threw " + inner.Message;
		}

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
