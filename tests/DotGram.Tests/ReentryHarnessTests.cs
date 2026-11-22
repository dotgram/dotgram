using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The bound on a rule that comes back to itself through a recognizer that does not move, held to the parser without
/// it: wherever that parser answers, the bounded one answers the same — verdict, position and message — in every
/// reading form; where it goes round for ever, the bounded one ends.
/// </summary>
/// <remarks>
/// <para>
/// The oracle is the same grammar compiled with the bound off (<see cref="Reentry.Unchecked"/>), its recognizers
/// counting their calls and throwing past a cap no answer that ends comes near: that is what going round for ever is
/// taken to be. The grammars are small and many: one and two recursive alternatives in either order, segments after
/// the call that read something or may read nothing, an operand before it that may read nothing, mutual recursion,
/// recursion in a repetition and in lookaheads of either sign, in the rule and in another rule of the cycle, a
/// recognizer that stands at some positions only, sequential entries, a rule read at a strength. The inputs are every
/// string up to a length over each grammar's alphabet.
/// </para>
/// <para>
/// Run on request (<c>DOTGRAM_REENTRY_HARNESS=1</c>; <c>DOTGRAM_REENTRY_HARNESS_OUT</c> for the report): it compiles
/// thousands of parsers. <c>Fixtures</c> runs the short list on every request too.
/// </para>
/// </remarks>
public sealed class ReentryHarnessTests
{
	const string Counter = """
		[System.ThreadStatic] static int calls;
		public static int Cap = 3000;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > Cap) throw new System.InvalidOperationException("went round"); }
		""";

	const string Context = """
		public sealed class Ctx { public int Depth; public int Bump() { return ++Depth; } }
		""";

	const string SpanHost = Counter + Context + """
		static bool Still(System.ReadOnlySpan<char> input, ref int p) { Count(); return true; }
		static bool Dot(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') { p++; return true; } return false; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == '.') p++; return true; }
		static bool OnB(System.ReadOnlySpan<char> input, ref int p) { Count(); return p < input.Length && input[p] == 'b'; }
		""";

	const string ViewHost = Counter + Context + """
		static bool Still(ParserInput<char> input, ref int p) { Count(); return true; }
		static bool Still(ParserInput<byte> input, ref int p) { Count(); return true; }
		static bool Dot(ParserInput<char> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == '.') { p++; return true; } return false; }
		static bool Dot(ParserInput<byte> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == (byte)'.') { p++; return true; } return false; }
		static bool Half(ParserInput<char> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == '.') p++; return true; }
		static bool Half(ParserInput<byte> input, ref int p) { Count(); if (input.Peek(p, out var c) && c == (byte)'.') p++; return true; }
		static bool OnB(ParserInput<char> input, ref int p) { Count(); return input.Peek(p, out var c) && c == 'b'; }
		static bool OnB(ParserInput<byte> input, ref int p) { Count(); return input.Peek(p, out var c) && c == (byte)'b'; }
		""";

	static readonly MethodInfo CreateMethod = typeof(EmittedCode).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)!;

	static bool Requested => Environment.GetEnvironmentVariable("DOTGRAM_REENTRY_HARNESS") is not null;

	static string Out => Environment.GetEnvironmentVariable("DOTGRAM_REENTRY_HARNESS_OUT") ?? Path.GetTempPath();

	/// <summary>The breaker's repros, the reviews' fixtures and the shapes the bound has to be held to by name.</summary>
	static readonly string[] Named =
	[
		// A retry comes back into a rule live at the same position, and the inner entry answers with its first
		// alternative while the outer one is on its second: the parser without the check accepts.
		"T = A & 'c'\nA = 'a' | @Still & A & 'b'",
		"T = V & ';'\nV = ['0'..'9']+ | @Still & V & '%'",
		"T = Name & '='\nName = ['a'..'z']+ | @Still & Name & '\\''",
		"T = A & 'c'\nA = 'a' | @Still & B\nB = A & 'b'",

		// The reader enters a recursion the engine never enters, inside a lookahead's optional.
		"T = @OnB & A & T | ?=(('.')* & (A & T & @Dot)?) & 'a'\nA = @OnB & A & 'z'",
		"T = ?=(('.')* & (A & T & @Dot)?) & 'a'\nA = @OnB & A & 'z'",
		"T = ?=(('.')* & (A & T)?) & 'a'\nA = @OnB & A",

		// The breaker's fuzzer findings.
		"T = @Dot | B | @Dot & A\nA = (@Still | 'a') & B & @Dot & @Still | @OnB & T & @Still & (A)*\nB = (@Half)* & 'b' & @Half | @Half & T & T | 'b' & A & '.'",
		"T = A & ?=(?=('b')) | @Dot | @Still & T & @Dot & 'a'\nA = '.' & ?!(?!(A & @Dot & B))\nB = (@Half & A)* & @OnB & T | @OnB & B & @Dot & T",

		// Recursion first, and the base first: a cut entry fails as an alternative does.
		"T = A\nA = @Still & A & 'b' | 'a'",
		"T = A\nA = 'a' | @Still & A & 'b'",

		// Sequential entries at one position, which are not nested and do not add up.
		"T = A & A & 'c'\nA = 'a'? | @Still & A & 'b'",

		// Backtracking into an entry that returned, which puts it back among the live ones.
		"T = A & 'c' | A & 'b' & 'c'\nA = 'a' | @Still & A & 'b'",

		// A lookahead in another rule of the cycle, and in mutual recursion.
		"T = A & 'c'\nA = 'a' | @Still & C & 'b'\nC = ?='a' & A | ?!'a' & A",
		"T = A & 'c'\nA = 'a' | @Still & B\nB = ?!(A & 'c') & A & 'b' | A & 'b' & 'b'",

		// A negative lookahead left of the answer: a success it would see deeper than the bound flips it.
		"T = A & 'c'\nA = ?!(@Still & A & 'b' & 'b') & 'a' | @Still & A & 'b'",

		// A rule calling a marked rule at the position from inside a lookahead.
		"T = ?=(A & 'b') & A & 'b' & 'c' | A & 'c'\nA = 'a' | @Still & A & 'b'",

		// A recognizer that stands at some positions only, and a rule read at a strength.
		"T = A & 'c'\nA = 'a' | @Half & A & 'b'",
		"T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'a'",
		"T = E & 'c'\nE = 'a' | @Half & E >> 1 | E & 'b' & E << 2",

		// A second entry inside a lookahead, which is past a bound of the input left plus one: the refusal says
		// what the lookahead wanted only where the bound leaves room for it.
		"T = R & 'c'\nR = @Still | @Still & ?=R & 'x'",

		// Two recursive alternatives: the search is exponential in the input left, accepted or not.
		"T = A & 'c'\nA = 'a' | @Still & A & 'b' | @Still & A & 'd'",
	];

	/// <summary>
	/// Grammars where the bound is known to answer otherwise than the parser without it, each with why: what reads
	/// on the cycle is not only the text and the position, so a chain deeper than the input left can end otherwise.
	/// They are run and reported apart, and not counted as the bound's failures.
	/// </summary>
	static readonly (string Grammar, string Why)[] Known =
	[
		(
			"T = d: A & when @(d >= 3)\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)",
			"a guard reads a value the cycle builds, which grows with the depth while the span stays the same"
		),
		(
			"context : @Ctx\nT = A\nA = when @(context.Depth >= 5) & 'x' | M & @Still & A\nM : @int = \"\" => @(context.Bump())",
			"a guard reads the context a construction on the cycle writes, which changes between entries at one position"
		),
	];

	/// <summary>The shapes the enumeration is made of: each with <c>{R}</c> a recognizer, <c>{B}</c> a base, <c>{S}</c> a segment.</summary>
	static readonly string[] Shapes =
	[
		"A = {B} | {R} & A & {S}",
		"A = {R} & A & {S} | {B}",
		"A = {B} | {R} & A & {S} | {R} & A & 'c'",
		"A = {R} & A & {S} | {B} | {R} & A & 'c'",
		"A = {B} | 'c'? & {R} & A & {S}",
		"A = {B} | {R} & 'c'? & A & {S}",
		"A = {B} | {R} & C\nC = A & {S}",
		"A = {B} | ({R} & A & {S})+",
		"A = {B} | ?=({R} & A) & 'b' & {S}",
		"A = ?!({R} & A & 'b' & 'b') & {B} | {R} & A & {S}",
		"A = {B} | {R} & C & {S}\nC = ?='a' & A | ?!'a' & A",
		"A = {B} | {R} & C\nC = ?!(A & 'c') & A & {S} | A & 'b' & 'b'",
	];

	static readonly string[] Recognizers = ["@Still", "@Half", "@OnB"];

	static readonly string[] Bases = ["'a'", "'a'+"];

	static readonly string[] Segments = ["'b'", "'b'?", "('b')*", "'b' & 'b'?"];

	static readonly string[] Tops = ["T = A", "T = A & 'c'", "T = A & A & 'c'"];

	static IEnumerable<string> Enumerated()
	{
		foreach (var shape in Shapes)
			foreach (var recognizer in Recognizers)
				foreach (var basis in Bases)
					foreach (var segment in Segments)
						foreach (var top in Tops)
							yield return top + "\n" + shape.Replace("{R}", recognizer).Replace("{B}", basis).Replace("{S}", segment);
	}

	/// <summary>One parser, compiled into a context of its own so that thousands of them come and go.</summary>
	sealed record Built(Assembly Assembly, bool Marked);

	static Built? Build(string grammar, bool direct, bool tape, bool buffered, bool unchecked_)
	{
		GramCompilation result;
		var was = Reentry.Unchecked;

		try
		{
			Reentry.Unchecked = unchecked_;
			result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
			{
				ClassName      = "Probe",
				Direct         = direct,
				Carrier        = tape ? CarrierKind.Deferred : CarrierKind.Auto,
				BufferedInput  = buffered,
				BufferedBytes  = buffered,
				SymbolResolver = buffered ? StandingTurnTests.ViewResolver.Instance : PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});
		}
		finally
		{
			Reentry.Unchecked = was;
		}

		if (result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error) || result.Sources.Count == 0)
			return null;

		var compilation = (CSharpCompilation)CreateMethod.Invoke(null,
		[
			result.Sources[0].Text, "Probe", null, buffered ? ViewHost : SpanHost,
			result.Sources.Skip(1).Select(static one => one.Text).ToArray(), null, false,
		])!;

		using var stream = new MemoryStream();

		var emitted = compilation.Emit(stream);

		if (!emitted.Success)
			throw new InvalidOperationException(
				"C# failed:\n" + string.Join("\n", emitted.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error).Take(5)) + "\n" + grammar);

		stream.Position = 0;

		return new Built(
			Assembly.Load(stream.ToArray()),
			result.Diagnostics.Any(static one => one.Id == Reentry.Reenters));
	}

	/// <summary>The verdict, where it stopped and what it said; <c>LOOP</c> where the recognizers went past the cap.</summary>
	static string Answer(Assembly parser, string input, Type over)
	{
		var type = parser.GetType("Probe")!;

		type.GetMethod("Reset")!.Invoke(null, null);

		// A grammar with a context is handed a fresh one, after the input.
		var context = type.GetNestedType("Ctx");
		var method  = type.GetMethods().FirstOrDefault(one =>
			one.Name == "TryParseT" && one.ReturnType != typeof(bool) &&
			one.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(
				(over == typeof(string) ? new[] { typeof(string) } : [over]).Concat(
					one.GetParameters().Any(parameter => parameter.ParameterType == context) ? [context!] : []).Concat(
					over == typeof(string) ? [] : [typeof(int?), typeof(int?)])));

		if (method is null)
			return "none";

		object match;

		var text = over == typeof(string) ? input : over == typeof(Stream) ? new MemoryStream(Encoding.ASCII.GetBytes(input)) : (object)new StringReader(input);
		var args = new List<object?> { text };

		if (method.GetParameters().Any(parameter => parameter.ParameterType == context))
			args.Add(Activator.CreateInstance(context!));

		if (over != typeof(string))
		{
			args.Add(1);
			args.Add(1 << 16);
		}

		try
		{
			match = method.Invoke(null, [.. args])!;
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
		{
			return inner.Message == "went round" || inner is InsufficientExecutionStackException ? "LOOP" : "EXC " + inner.GetType().Name + ": " + inner.Message;
		}

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!} {Read("Value")}"
			: $"no@{Read("Position")} {Read("Outcome")} {Read("Error")}";
	}

	static IEnumerable<string> Inputs(string alphabet, int longest)
	{
		var all  = new List<string> { "" };
		var last = new List<string> { "" };

		for (var length = 1; length <= longest; length++)
		{
			last = [.. last.SelectMany(prefix => alphabet.Select(c => prefix + c))];
			all.AddRange(last);
		}

		return all;
	}

	/// <summary>The characters a grammar's literals name, and the dot its recognizers move over.</summary>
	static string Alphabet(string grammar)
	{
		var letters = new SortedSet<char>();

		for (var at = 0; at + 2 < grammar.Length; at++)
			if (grammar[at] == '\'' && grammar[at + 2] == '\'' && grammar[at + 1] != '\\')
				letters.Add(grammar[at + 1]);

		if (grammar.Contains("..", StringComparison.Ordinal))
			letters.Add('1');

		letters.Add('.');
		letters.Add('b');

		return string.Concat(letters);
	}

	/// <summary>What one grammar answered, form by form and input by input, against its oracle.</summary>
	sealed class Tally
	{
		public int Grammars;
		public int Marked;
		public long Equal;
		public long Ended;
		public long Differ;
		public long Loops;
		public readonly ConcurrentQueue<string> Problems = new();
	}

	static void Compare(string grammar, int longest, int longestStreamed, Tally tally)
	{
		var alphabet = Alphabet(grammar);

		// As long as keeps a few hundred inputs a form: a wide alphabet is read shorter.
		while (longest > 2 && Math.Pow(alphabet.Length, longest) > 400)
			longest--;

		var strings  = Inputs(alphabet, longest).ToArray();
		var streamed = strings.Where(one => one.Length <= longestStreamed).ToArray();
		var marked   = false;

		foreach (var direct in new[] { true, false })
			foreach (var tape in new[] { false, true })
				foreach (var buffered in new[] { false, true })
				{
					var bounded = Build(grammar, direct, tape, buffered, unchecked_: false);
					var oracle  = Build(grammar, direct, tape, buffered, unchecked_: true);

					if (bounded is null || oracle is null)
						return;

					// The bounded parser is let read far longer than the oracle: going round is the oracle's, and a
					// search that is long but ends is told from one that does not.
					bounded.Assembly.GetType("Probe")!.GetField("Cap")!.SetValue(null, int.Parse(Environment.GetEnvironmentVariable("DOTGRAM_REENTRY_HARNESS_CAP") ?? "300000"));

					marked |= bounded.Marked;

					var reading = (direct ? "direct" : "engine") + (tape ? "/tape" : "/imm") + (buffered ? "/view" : "/span");

					foreach (var form in buffered ? new[] { typeof(string), typeof(TextReader), typeof(Stream) } : [typeof(string)])
						foreach (var input in form == typeof(string) ? strings : streamed)
						{
							var expected = Answer(oracle.Assembly, input, form);
							var got      = Answer(bounded.Assembly, input, form);

							if (expected == "none")
								continue;

							if (expected == "LOOP")
							{
								System.Threading.Interlocked.Increment(ref tally.Loops);

								if (got == "LOOP" || got.StartsWith("EXC", StringComparison.Ordinal))
								{
									System.Threading.Interlocked.Increment(ref tally.Differ);
									tally.Problems.Enqueue($"{reading}/{form.Name} \"{input}\": the oracle goes round, the bound gives {got}\n{grammar}");
								}
								else
									System.Threading.Interlocked.Increment(ref tally.Ended);
							}
							else if (expected == got)
								System.Threading.Interlocked.Increment(ref tally.Equal);
							else
							{
								System.Threading.Interlocked.Increment(ref tally.Differ);
								tally.Problems.Enqueue($"{reading}/{form.Name} \"{input}\": the oracle {expected} | the bound {got}\n{grammar}");
							}
						}
				}

		System.Threading.Interlocked.Increment(ref tally.Grammars);

		if (marked)
			System.Threading.Interlocked.Increment(ref tally.Marked);
	}

	static Tally Run(IEnumerable<string> grammars, int longest, int longestStreamed, string report)
	{
		var tally   = new Tally();
		var pending = new ConcurrentQueue<string>(grammars);
		var thrown  = new ConcurrentQueue<Exception>();

		// Threads of their own, with stacks deep enough that an oracle going round reaches the cap before the
		// stack's margin: an exception thrown there, near the end of a stack, would take the process with it.
		var workers = Enumerable.Range(0, Math.Max(1, Environment.ProcessorCount / 3)).Select(_ => new System.Threading.Thread(() =>
		{
			while (pending.TryDequeue(out var grammar))
			{
				try
				{
					var watch = System.Diagnostics.Stopwatch.StartNew();

					Compare(grammar, longest, longestStreamed, tally);

					lock (pending)
						File.AppendAllText(Path.Combine(Out, report + ".progress"), $"{watch.ElapsedMilliseconds} ms {grammar.Replace("\n", " ;; ")}\n");
				}
				catch (Exception caught)
				{
					thrown.Enqueue(new InvalidOperationException(grammar, caught));
				}
			}
		}, 256 << 20)).ToList();

		foreach (var worker in workers)
			worker.Start();

		foreach (var worker in workers)
			worker.Join();

		foreach (var one in thrown)
			tally.Problems.Enqueue("THREW " + one.InnerException!.GetType().Name + ": " + one.InnerException.Message.Split('\n')[0] + "\n" + one.Message);

		var problems = tally.Problems.ToArray();

		File.WriteAllText(
			Path.Combine(Out, report),
			$"grammars {tally.Grammars}, marked {tally.Marked}; answers equal {tally.Equal}, oracle went round {tally.Loops} " +
			$"(bound ended {tally.Ended}); differing {tally.Differ}\n\n" + string.Join("\n\n", problems));

		return tally;
	}

	[Fact]
	public void Fixtures()
	{
		if (!Requested)
			return;

		// One or some of them, by their places in the list, where a run is to be narrowed.
		var only = Environment.GetEnvironmentVariable("DOTGRAM_REENTRY_HARNESS_ONLY") is { } some
			? some.Split(',').Select(int.Parse).Select(at => Named[at]).ToArray()
			: Named;

		var tally = Run(only, 5, 3, "reentry-harness-fixtures.txt");

		Assert.Equal(only.Length, tally.Grammars);
		Assert.True(tally.Differ == 0, string.Join("\n\n", tally.Problems.Take(10)));
	}

	/// <summary>The known disagreements, written down as they are: each one disagrees, and nowhere else than where it says.</summary>
	[Fact]
	public void Known_disagreements()
	{
		if (!Requested)
			return;

		var tally = Run(Known.Select(static one => one.Grammar), 5, 3, "reentry-harness-known.txt");

		Assert.Equal(Known.Length, tally.Grammars);
		Assert.True(tally.Differ > 0);
	}

	[Fact]
	public void Enumeration()
	{
		if (!Requested)
			return;

		var tally = Run(Enumerated(), 5, 3, "reentry-harness-enumeration.txt");

		Assert.True(tally.Marked > 0);
		Assert.True(tally.Differ == 0, string.Join("\n\n", tally.Problems.Take(10)));
	}
}
