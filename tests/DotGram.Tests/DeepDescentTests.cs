using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Deep nests whose innermost reading can be split many ways, each refused at the top: what the
/// reader answers for them, written down before anything is done about how long it takes.
/// </summary>
/// <remarks>
/// <para>
/// <b>The shape.</b> <c>Start = P &amp; '!'</c> over <c>P = '(' &amp; P &amp; ')' | Bs &amp; Cs</c>: every
/// way of cutting the innermost run of <c>b</c> between <c>Bs</c> and <c>Cs</c> stands inside the
/// nest, and only the <c>'!'</c> at the top refuses it. A refusal at the top asks the innermost
/// reading again, and every attempt goes down the whole nest to it and back up: the depth times the
/// number of cuts.
/// </para>
/// <para>
/// Each grammar here pins one thing a remedy has to keep: values built while the text is read, with
/// a construction that throws on one cut (<c>Built</c>, <c>BuiltZero</c>, <c>Stood</c> — the last one
/// where a frame in the middle of the nest stops building and builds again once it stands); a guard
/// over a value read before the call and one read after it, as XML closes a tag (<c>Built</c>); a
/// guard over the value the nest itself built, which no remedy that does not look at values may skip
/// (<c>Guard</c>); a repetition before the call (<c>Prefix</c>); braces on the way back up
/// (<c>Sealed</c>); and a nest inside the element of a recovering repetition (<c>Recover</c>).
/// <c>Open</c> and <c>Vary</c> are the controls: one with nothing to read on the way up, one with a
/// single way to read the innermost run.
/// </para>
/// <para>
/// The answers are what the reader gives today, refusals and exceptions included; a change to how
/// it reads these nests must leave every one of them as it is.
/// </para>
/// </remarks>
public sealed class DeepDescentTests
{
	const string Members = """
		static int Split(int b, int c)
		{
			if (b == 3 && c == 1)
				throw new global::System.InvalidOperationException("split 3/1");

			return b * 100 + c;
		}

		static int Empty(int b, int c)
		{
			if (b == 0 && c == 0)
				throw new global::System.InvalidOperationException("empty split");

			return b * 100 + c;
		}

		static int Pair(int x, int y)
		{
			if (x == 1 && y == 3)
				throw new global::System.InvalidOperationException("pair 1/3");

			return x * 100 + y;
		}
		""";

	public const string Split = """
		Start = P & '!'
		P     = '(' & P & ')' | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	public const string Open = """
		Start = P & '!'
		P     = '(' & P | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	public const string Vary = """
		Start = P & '!'
		P     = '(' & P & ')' | Bs
		Bs    = 'b'*
		parse Start
		""";

	public const string Built = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = t: D & '(' & inner: P & ')' & u: D & when @(t == u) => @(inner + t)
		             | b: Bs & c: Cs => @(Split(b, c))
		D     : @int = d: ['0'..'9'] => @(d[0] - '0')
		Bs    : @int = t: 'b'* => @(t.Length)
		Cs    : @int = t: 'b'* => @(t.Length)
		parse Start
		""";

	public const string BuiltZero = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = t: D & '(' & inner: P & ')' & u: D & when @(t == u) => @(inner + t)
		             | b: Bs & c: Cs => @(Empty(b, c))
		D     : @int = d: ['0'..'9'] => @(d[0] - '0')
		Bs    : @int = t: 'b'* => @(t.Length)
		Cs    : @int = t: 'b'* => @(t.Length)
		parse Start
		""";

	public const string Stood = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = '(' & inner: P & ')' => @(inner)
		             | x: Xs & y: Ys => @(Pair(x, y))
		Xs    : @int = t: 'a'* => @(t.Length)
		Ys    : @int = t: ('a' & 'a' & 'a')* => @(t.Length)
		parse Start
		""";

	public const string Guard = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = '(' & inner: P & ')' & when @(inner >= 0) => @(inner + 1)
		             | b: Bs & c: Cs => @(b - c)
		Bs    : @int = t: 'b'* => @(t.Length)
		Cs    : @int = t: 'b'* => @(t.Length)
		parse Start
		""";

	public const string Prefix = """
		Start = P & '!'
		P     = '(' & Item* & P & ')' | Bs & Cs
		Item  = 'a'
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	public const string Sealed = """
		Start = P & '!'
		P     = '(' & P & { A } & ')' | Bs & Cs
		A     = 'a'* & 'a'
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	/// <summary>A chain of tail calls with the split at the bottom: every level is entered where no way of its own stands.</summary>
	public const string Tail = """
		Start = P & '!'
		P     = '(' & P | 'x' & Xs & Ys
		Xs    = 'a'*
		Ys    = 'a'*
		parse Start
		""";

	/// <summary>
	/// <see cref="Tail"/> closed again on the way up: every level begins where the one above it does,
	/// so a memo that finds a frame by walking everything begun there walks the whole nest.
	/// </summary>
	public const string Closed = """
		Start = P & '!'
		P     = '(' & P & ')' | 'x' & Xs & Ys
		Xs    = 'a'*
		Ys    = 'a'*
		parse Start
		""";

	public const string Recover = """
		Start : @int[] = Element* recover ';' & eof
		Element : @int = P & '!' & ';' => @(1)
		P     = '(' & P & ')' | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	static readonly Dictionary<string, string> _grammars = new()
	{
		["Split"]     = Split,
		["Open"]      = Open,
		["Vary"]      = Vary,
		["Built"]     = Built,
		["BuiltZero"] = BuiltZero,
		["Stood"]     = Stood,
		["Guard"]     = Guard,
		["Prefix"]    = Prefix,
		["Sealed"]    = Sealed,
		["Recover"]   = Recover,
		["Tail"]      = Tail,
		["Closed"]    = Closed,
	};

	[Theory]
	[InlineData("Split",     "((bbb))!",           "accepted 0..8")]
	[InlineData("Split",     "((bbb))",            "refused at 7: Expected '!'.")]
	[InlineData("Split",     "((bbb)",             "refused at 6: Expected ')'.")]
	[InlineData("Open",      "((bbb!",             "accepted 0..6")]
	[InlineData("Open",      "((bbb",              "refused at 5: Expected '!'.")]
	[InlineData("Vary",      "((bbb))",            "refused at 7: Expected '!'.")]
	[InlineData("Built",     "1(2(bbbbb)2)1!",     "accepted 503")]
	[InlineData("Built",     "1(2(bbbb)2)1",       "throws split 3/1")]
	[InlineData("Built",     "1(2(bbbbb)2)1",      "refused at 13: Expected '!'.")]
	[InlineData("Built",     "1(2(bb)1)1!",        "refused at 8: when @(t == u) said no.")]
	[InlineData("BuiltZero", "1(1(bbbb)1)1",       "refused at 12: Expected '!'.")]
	[InlineData("BuiltZero", "1(bbbb)1!",          "accepted 401")]
	[InlineData("Stood",     "(aaaa)",             "throws pair 1/3")]
	[InlineData("Stood",     "((aaaaaa))!",        "accepted 600")]
	[InlineData("Guard",     "((bbb))!",           "accepted 5")]
	[InlineData("Guard",     "((bbb))",            "refused at 7: Expected '!'.")]
	[InlineData("Prefix",    "(a(aa(bbb)))",       "refused at 12: Expected '!'.")]
	[InlineData("Prefix",    "(a(aa(bbb)))!",      "accepted 0..13")]
	[InlineData("Sealed",    "((bbbaa)aa)",        "refused at 11: Expected '!'.")]
	[InlineData("Sealed",    "((bbbaa)aa)!",       "accepted 0..12")]
	[InlineData("Recover",   "((bb))!;(b);(()!;",  "accepted 1")]
	public void A_nest_is_answered_as_it_was(string grammar, string input, string answer)
	{
		Assert.Equal(answer, Answer(Compiled(_grammars[grammar]), input));
	}

	/// <summary>
	/// The reader that turns a lost answer at once answers every input as the one without it does:
	/// the outcome, the position, the message, the value and what a construction threw.
	/// </summary>
	[Theory]
	[InlineData("Split",     "()b!",   1)]
	[InlineData("Open",      "()b!",   0)]
	[InlineData("Vary",      "()b!",   1)]
	[InlineData("Built",     "()b!12", 2)]
	[InlineData("BuiltZero", "()b!12", 2)]
	[InlineData("Stood",     "()a!",   1)]
	[InlineData("Guard",     "()b!",   1)]
	[InlineData("Prefix",    "()ab!",  1)]
	[InlineData("Sealed",    "()ab!",  1)]
	[InlineData("Recover",   "()b!;",  1)]
	[InlineData("Tail",      "(xa!",   0)]
	[InlineData("Closed",    "()xa!",  1)]
	public void The_memo_of_lost_answers_answers_as_the_reader_without_it(string grammar, string alphabet, int closing)
	{
		var with    = Compiled(_grammars[grammar]);
		var without = Compiled(_grammars[grammar], dooms: false);
		var random  = new Random(20261120);

		// Three thousand inputs a grammar by default; a longer run sets DOTGRAM_DEEP_DESCENT_SAMPLES.
		var samples = int.TryParse(Environment.GetEnvironmentVariable("DOTGRAM_DEEP_DESCENT_SAMPLES"), out var asked) ? asked : 3000;

		for (var i = 0; i < samples; i++)
		{
			var input = Input(random, alphabet, closing, i);

			Assert.True(Answer(without, input) == Answer(with, input), $"'{input}': {Answer(without, input)} without the memo, {Answer(with, input)} with it.");
		}
	}

	/// <summary>
	/// A random input: a few characters of the alphabet, and three times in four a nest of up to twelve
	/// levels around a run, with something inserted, a closing taken out, and something after.
	/// </summary>
	static string Input(Random seeded, string alphabet, int closing, int index)
	{
		var random = new Random(seeded.Next() ^ index);
		var noise  = new System.Text.StringBuilder();

		for (var k = random.Next(0, 16); k > 0; k--)
			noise.Append(alphabet[random.Next(alphabet.Length)]);

		if (index % 4 == 0)
			return noise.ToString();

		var open  = new System.Text.StringBuilder();
		var close = new System.Text.StringBuilder();
		var digit = alphabet.Contains('1', StringComparison.Ordinal);

		for (var level = random.Next(0, 13); level > 0; level--)
		{
			var tag = digit ? (random.Next(5) == 0 ? "2" : "1") : "";

			open.Append(tag).Append('(');

			if (closing > 0)
				close.Insert(0, ")" + (digit ? (random.Next(5) == 0 ? "2" : "1") : ""));
		}

		var run = alphabet.Contains('x', StringComparison.Ordinal) ? "x" : "";

		run += new string(alphabet.Contains('a', StringComparison.Ordinal) && !alphabet.Contains('b', StringComparison.Ordinal) ? 'a' : 'b', random.Next(0, 13));

		if (random.Next(3) == 0)
			run += noise;

		var closed = close.ToString();

		if (closed.Length > 0 && random.Next(4) == 0)
			closed = closed.Remove(random.Next(closed.Length), 1);

		var tail = random.Next(4) switch
		{
			0 => "",
			1 => "!",
			2 => noise.ToString(),
			_ => "!" + noise,
		};

		return open + run + closed + tail + (alphabet.Contains(';', StringComparison.Ordinal) && random.Next(2) == 0 ? ";" : "");
	}

	/// <summary>Which rules are places: the ones nothing after them can see into, and no others.</summary>
	[Theory]
	[InlineData("Split",   true)]
	[InlineData("Built",   true)]
	[InlineData("Stood",   true)]
	[InlineData("Closed",  true)]
	[InlineData("Guard",   false)]
	[InlineData("Recover", false)]
	public void A_rule_is_a_place_where_nothing_after_it_can_see_into_it(string grammar, bool place)
	{
		Assert.Equal(place, Source(_grammars[grammar], dooms: true, counts: false).Contains("ways.Returned(", StringComparison.Ordinal));
	}

	/// <summary>
	/// The closed nest is read in time linear in its depth, counted in rule entries at four depths a
	/// doubling apart; and the memo finds a frame in a constant number of steps however many frames
	/// begin where it does, counted in the slots its index looks at against the searches.
	/// </summary>
	[Fact]
	public void A_closed_nest_split_at_the_bottom_costs_its_depth()
	{
		var parser  = Compiled(Closed, dooms: true, counts: true);
		var ways    = parser.GetNestedType("Ways_DotGram", BindingFlags.NonPublic)!;
		var probes  = ways.GetField("CountDoomProbes", BindingFlags.NonPublic | BindingFlags.Static)!;
		var asked   = ways.GetField("CountDoomSearches", BindingFlags.NonPublic | BindingFlags.Static)!;
		var entries = new List<long>();

		foreach (var depth in new[] { 250, 500, 1000, 2000 })
		{
			probes.SetValue(null, 0L);
			asked.SetValue(null, 0L);

			var text = new string('(', depth) + "x" + new string('a', depth) + new string(')', depth);

			entries.Add(Entries(parser, text));

			Assert.True(
				(long)probes.GetValue(null)! <= 4 * (long)asked.GetValue(null)! + 16,
				$"{probes.GetValue(null)} slots looked at for {asked.GetValue(null)} searches at {depth} levels.");
		}

		Assert.True(
			entries[3] - entries[2] <= 2.2 * (entries[2] - entries[1]) && entries[2] - entries[1] <= 2.2 * (entries[1] - entries[0]),
			$"{string.Join(", ", entries)} rule entries at 250, 500, 1000 and 2000 levels.");
	}

	/// <summary>
	/// What the reader does on the nests, counted, with the memo and without it: written to the file
	/// <c>DOTGRAM_DEEP_DESCENT_COUNTS</c> names, for a measurement and not a gate; without it, nothing.
	/// </summary>
	[Fact]
	public void The_nests_are_counted_where_asked()
	{
		var file = Environment.GetEnvironmentVariable("DOTGRAM_DEEP_DESCENT_COUNTS");

		if (string.IsNullOrEmpty(file))
			return;

		var rows = new (string Name, string Grammar, Func<int, string> Text)[]
		{
			("Sealed",  Sealed, d => new string('(', d) + new string('b', d) + string.Concat(Enumerable.Repeat("aa)", d))),
			("Open(b)", Open,   d => new string('(', d) + "b"),
			("Open()",  Open,   d => new string('(', d)),
			("Open",    Open,   d => new string('(', d) + new string('b', d)),
			("Vary",    Vary,   d => new string('(', d) + new string('b', d) + new string(')', d)),
			("Split",   Split,  d => new string('(', d) + new string('b', d) + new string(')', d)),
			("Prefix",  Prefix, d => string.Concat(Enumerable.Repeat("(a", d)) + new string('b', d) + new string(')', d)),
		};

		var lines = new List<string>();

		foreach (var (name, grammar, text) in rows)
		{
			foreach (var dooms in new[] { false, true })
			{
				var parser = Compiled(grammar, dooms, counts: true);
				var fields = parser.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
					.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
					.Where(static field => field.FieldType == typeof(long) && field.Name.StartsWith("Count", StringComparison.Ordinal))
					.ToList();

				foreach (var depth in new[] { 2000, 4000, 8000 })
				{
					var input = text(depth);

					Answer(parser, input);

					foreach (var field in fields)
						field.SetValue(null, 0L);

					var before = GC.GetTotalAllocatedBytes(true);

					Answer(parser, input);

					var allocated = GC.GetTotalAllocatedBytes(true) - before;
					var entered   = fields.Where(static field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal)).Sum(static field => (long)field.GetValue(null)!);
					var counted   = fields
						.Where(static field => !field.Name.StartsWith("CountEntered_", StringComparison.Ordinal))
						.GroupBy(static field => field.Name)
						.Select(static group => $"{group.Key}={group.Sum(static field => (long)field.GetValue(null)!)}");

					lines.Add($"{name}\t{(dooms ? "on" : "off")}\t{depth}\tentered={entered}\tallocated={allocated}\t{string.Join("\t", counted)}");
				}
			}
		}

		System.IO.File.WriteAllLines(file, lines);
	}

	/// <summary>Rule entries of one reading, from a fresh zero; the reading refuses.</summary>
	static long Entries(Type parser, string input)
	{
		var counters = parser.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
			.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
			.Where(static field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal))
			.ToList();

		Assert.NotEmpty(counters);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		Assert.StartsWith("refused", Answer(parser, input), StringComparison.Ordinal);

		return counters.Sum(static counter => (long)counter.GetValue(null)!);
	}

	/// <summary>What one reading answers, in a line: the value or the span, the refusal, or what was thrown.</summary>
	static string Answer(Type parser, string input)
	{
		try
		{
			var match = EmittedCode.Match(parser.Assembly, "Grammar", "TryParseStart", input);

			if (!match.IsSuccess)
				return $"refused at {match.Position}: {match.Error}";

			return match.Value switch
			{
				int[] values => "accepted " + string.Join(",", values),
				string text  => $"accepted 0..{text.Length}",
				var value    => "accepted " + value,
			};
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is InvalidOperationException inner)
		{
			return "throws " + inner.Message;
		}
	}

	static readonly Dictionary<(string, bool, bool), Type> _compiled = [];

	static Type Compiled(string grammar, bool dooms = true, bool counts = false)
	{
		lock (_compiled)
		{
			if (!_compiled.TryGetValue((grammar, dooms, counts), out var parser))
			{
				var source = Source(grammar, dooms, counts);

				_compiled[(grammar, dooms, counts)] = parser = EmittedCode.Compile(
					source, declarationMembers: Members, symbols: counts ? ["DOTGRAM_COUNTS"] : null).GetType("Grammar")!;
			}

			return parser;
		}
	}

	static string Source(string grammar, bool dooms, bool counts)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			CSharpScanner     = RoslynCSharpScanner.Instance,
			Carrier           = CarrierKind.Immediate,
			DoomContinuations = dooms,
			CountRules        = counts,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return Assert.Single(result.Sources).Text;
	}
}
