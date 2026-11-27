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
/// The check on a rule that comes back to itself through a recognizer that does not move — cut at once where nothing
/// is read after the call back, and nothing elsewhere — held to the parser without it: wherever that parser answers,
/// the checked one answers the same — verdict, position and message — in every reading form; where it goes round for
/// ever, the checked one ends where it cuts, and is the same parser, character for character, where it does not.
/// </summary>
/// <remarks>
/// <para>
/// The oracle is the same grammar compiled with the check off (<see cref="Reentry.Unchecked"/>), its recognizers
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
/// <para>
/// A rule read past a recognizer that stands, as the left recursion it is without it (GrammarNormalizer.Guarded.cs),
/// takes the most steps what follows accepts; the oracle takes the fewest and leaves the rest to what follows. Where
/// the top reads the same steps after the rule (<see cref="ValuedTop"/>), the two answer one verdict at one end with the
/// text split between the rule and what follows in another place, and that, told apart from the rest by
/// <see cref="Resplit"/>, is the one disagreement the harness allows; anything else is a defect.
/// </para>
/// </remarks>
public sealed class ReentryHarnessTests
{
	const string Counter = """
		[System.ThreadStatic] static int calls;
		[System.ThreadStatic] static long since;
		public static int Cap = 1000;
		public static long Ticks = System.Diagnostics.Stopwatch.Frequency / 2;
		public static void Reset() { calls = 0; since = System.Diagnostics.Stopwatch.GetTimestamp(); }
		static void Count() { if (++calls > Cap || System.Diagnostics.Stopwatch.GetTimestamp() - since > Ticks) throw new System.InvalidOperationException("went round"); }
		""";

	const string Context = """
		public sealed class Ctx { public int Depth; public int Bump() { return ++Depth; } }
		static string Join(string a, string b) { return a + "|" + b; }
		static string Join(System.ReadOnlySpan<char> a, System.ReadOnlySpan<char> b) { return a.ToString() + "|" + b.ToString(); }
		static string Join(System.ReadOnlySpan<byte> a, System.ReadOnlySpan<byte> b) { return System.Text.Encoding.ASCII.GetString(a) + "|" + System.Text.Encoding.ASCII.GetString(b); }
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

	/// <summary>The breaker's repros, the reviews' fixtures and the shapes the check has to be held to by name.</summary>
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

		// A negative lookahead left of the answer: a success it would see deeper than a bound on the entries would flip it.
		"T = A & 'c'\nA = ?!(@Still & A & 'b' & 'b') & 'a' | @Still & A & 'b'",

		// A rule calling a marked rule at the position from inside a lookahead.
		"T = ?=(A & 'b') & A & 'b' & 'c' | A & 'c'\nA = 'a' | @Still & A & 'b'",

		// A recognizer that stands at some positions only, and a rule read at a strength.
		"T = A & 'c'\nA = 'a' | @Half & A & 'b'",
		"T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'a'",
		"T = E & 'c'\nE = 'a' | @Half & E >> 1 | E & 'b' & E << 2",

		// A second entry inside a lookahead, which a bound of the input left plus one would cut: the refusal says
		// what the lookahead wanted only where such a bound leaves room for it.
		"T = R & 'c'\nR = @Still | @Still & ?=R & 'x'",

		// Two recursive alternatives: the search is exponential in the input left, accepted or not.
		"T = A & 'c'\nA = 'a' | @Still & A & 'b' | @Still & A & 'd'",

		// The rule's text and what a loop after it reads: where the steps are split between the two.
		ValuedTop + "\nA = 'a' | @Still & A & 'b'",
		ValuedTop + "\nA = 'a' | @Half & A & 'b'",
		ValuedTop + "\nA = @Still & A & 'b' | 'a'",
		ValuedTop + "\nA = 'a' | @Still & A & 'b' | @Still & A & 'c'",
		ValuedTop + "\nA = 'a' | @Still & C & 'b'\nC = ?='a' & A | ?!'a' & A",

		// Nothing read after the call back, cut at once: by itself, mutual, moving at some positions, as the second
		// alternative, given back into by the caller, behind a lookahead, and a list with what follows it optional.
		"T = @Still & T | 'a' & T | eof",
		"T = @Still & U | 'a'\nU = @Still & T | 'b' & T",
		"T = @Half & T | eof",
		"T = 'a' & T | @Still & T | 'b'",
		"T = S & 'c'\nS = @Still & S | 'a' | 'a' & 'b'",
		"T = ?!'b' & @Still & T | 'a' & T | 'b'",
		"T = S & eof\nS = @Still & S | 'a' & ('+' & S)?",
	];

	/// <summary>
	/// Grammars a check on the rule and the position would answer otherwise than the parser without it, each with why:
	/// what reads on the cycle is not only the text and the position. They are not checked, and are generated as the
	/// parser without the check is.
	/// </summary>
	static readonly (string Grammar, string Why)[] Known =
	[
		(
			"T = d: A & when @(d >= 4)\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)",
			"a guard reads a value the cycle builds, which grows with the depth while the span stays the same"
		),
		(
			"context : @Ctx\nT = A\nA = when @(context.Depth >= 5) & 'x' | when @(context.Bump() > 0) & @Still & A",
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

		// Nothing read after the call back.
		"A = {B} & {S} | {R} & A",
		"A = {R} & A | {B} & {S}",
		"A = {B} | {R} & C\nC = {S} & A | A",
		"A = {B} & ({S} & A)? | {R} & A",
	];

	/// <summary>Whether the wide grid is asked for (<c>DOTGRAM_REENTRY_HARNESS_WIDE=1</c>): segments that read a run, more recognizers, more callers.</summary>
	static bool Wide => Environment.GetEnvironmentVariable("DOTGRAM_REENTRY_HARNESS_WIDE") is not null;

	static string[] Recognizers => Wide ? ["@Still", "@Half", "@OnB"] : ["@Still", "@Half"];

	static readonly string[] Bases = ["'a'", "'a'+"];

	static string[] Segments => Wide ? ["'b'", "'b'?", "('b')*", "'b' & 'b'?"] : ["'b'", "'b'?"];

	/// <summary>
	/// A top that reads the steps again after the rule and answers how the text was split between the two: the rule's
	/// text, a bar, and what the loop after it read.
	/// </summary>
	const string ValuedTop = "T : @string = r: A & t: ('b')* & eof => @(Join(r, t))";

	static string[] Tops => Wide ? ["T = A", "T = A & 'c'", "T = A & A & 'c'", ValuedTop] : ["T = A & 'c'", "T = A & A & 'c'", ValuedTop];

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
	/// <param name="Source">What the generator wrote, every file of it.</param>
	sealed record Built(Assembly Assembly, bool Marked, string Source);

	static Built? Build(string grammar, bool direct, bool tape, bool buffered, bool unchecked_, bool lexical = false)
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
				Lexical        = lexical,
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
			result.Diagnostics.Any(static one => one.Id == Reentry.Reenters),
			string.Join("\n", result.Sources.Select(static one => one.Text)));
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
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!} {(Read("Value") is byte[] bytes ? Encoding.ASCII.GetString(bytes) : Read("Value"))}"
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
		public long Slow;
		public long Same;
		public long Resplit;
		public int Valued;
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
					var checkedOne = Build(grammar, direct, tape, buffered, unchecked_: false);
					var oracle     = Build(grammar, direct, tape, buffered, unchecked_: true);

					if (checkedOne is null || oracle is null)
						return;

					// The checked parser is let read far longer than the oracle: going round is the oracle's, and a
					// search that is long but ends is told from one that does not.
					checkedOne.Assembly.GetType("Probe")!.GetField("Cap")!.SetValue(null, int.Parse(Environment.GetEnvironmentVariable("DOTGRAM_REENTRY_HARNESS_CAP") ?? "30000"));
					checkedOne.Assembly.GetType("Probe")!.GetField("Ticks")!.SetValue(null, System.Diagnostics.Stopwatch.Frequency * 10);

					marked |= checkedOne.Marked;

					var reading = (direct ? "direct" : "engine") + (tape ? "/tape" : "/imm") + (buffered ? "/view" : "/span");

					// A cycle the check does not cut is the parser without it, character for character: nothing to read.
					if (checkedOne.Source == oracle.Source)
					{
						System.Threading.Interlocked.Increment(ref tally.Same);

						continue;
					}

					foreach (var form in buffered ? new[] { typeof(string), typeof(TextReader), typeof(Stream) } : [typeof(string)])
						foreach (var input in form == typeof(string) ? strings : streamed)
						{
							var expected = Answer(oracle.Assembly, input, form);
							var got      = Answer(checkedOne.Assembly, input, form);

							if (expected == "none")
								continue;

							if (expected == "LOOP")
							{
								System.Threading.Interlocked.Increment(ref tally.Loops);

								// Past the checked parser's own, far larger cap: a search that ends, but not soon, where the
								// oracle never does — told apart, and not a disagreement.
								if (got == "LOOP")
								{
									System.Threading.Interlocked.Increment(ref tally.Slow);
									tally.Problems.Enqueue($"SLOW {reading}/{form.Name} \"{input}\": the oracle goes round, the checked parser reads past its cap\n{grammar}");
								}
								else if (got.StartsWith("EXC", StringComparison.Ordinal))
								{
									System.Threading.Interlocked.Increment(ref tally.Differ);
									tally.Problems.Enqueue($"{reading}/{form.Name} \"{input}\": the oracle goes round, the checked parser gives {got}\n{grammar}");
								}
								else
									System.Threading.Interlocked.Increment(ref tally.Ended);
							}
							else if (expected == got)
								System.Threading.Interlocked.Increment(ref tally.Equal);
							else if (grammar.StartsWith("T : @string", StringComparison.Ordinal) && Resplit(expected, got))
								System.Threading.Interlocked.Increment(ref tally.Resplit);
							else
							{
								System.Threading.Interlocked.Increment(ref tally.Differ);
								tally.Problems.Enqueue($"{reading}/{form.Name} \"{input}\": the oracle {expected} | the checked parser {got}\n{grammar}");
							}
						}
				}

		System.Threading.Interlocked.Increment(ref tally.Grammars);

		if (grammar.StartsWith("T : @string", StringComparison.Ordinal))
			System.Threading.Interlocked.Increment(ref tally.Valued);

		if (marked)
			System.Threading.Interlocked.Increment(ref tally.Marked);
	}

	/// <summary>
	/// Whether two answers of a <see cref="ValuedTop"/> are one verdict at one end with the text split elsewhere: both
	/// accepted, at the same place, the rule's text and what the loop read making the same text, the rule's text no
	/// shorter where the rule reads as the left recursion it is, which takes the most steps.
	/// </summary>
	static bool Resplit(string oracle, string checkedOne)
	{
		if (!oracle.StartsWith("ok@", StringComparison.Ordinal) || !checkedOne.StartsWith("ok@", StringComparison.Ordinal))
			return false;

		var (end, split) = Cut(oracle);
		var (now, again) = Cut(checkedOne);

		return end == now && split is { } one && again is { } other && one.Whole == other.Whole && one.Rule.Length <= other.Rule.Length;

		static (string End, (string Rule, string Whole)? Split) Cut(string answer)
		{
			var space = answer.IndexOf(' ', StringComparison.Ordinal);
			var value = answer[(space + 1)..];
			var bar   = value.LastIndexOf('|');

			return bar < 0 ? (answer[..space], null) : (answer[..space], (value[..bar], value[..bar] + value[(bar + 1)..]));
		}
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
			tally.Problems.Enqueue("THREW " + one.InnerException!.GetType().Name + ": " + string.Join(" | ", one.InnerException.Message.Split('\n').Take(4)) + "\n" + one.Message);

		var problems = tally.Problems.ToArray();

		File.WriteAllText(
			Path.Combine(Out, report),
			$"grammars {tally.Grammars}, marked {tally.Marked}; readings generated as without the check {tally.Same}; " +
			$"answers equal {tally.Equal}, same verdict and end with the steps split elsewhere {tally.Resplit} (of {tally.Valued} grammars with a top that shows it), oracle went round {tally.Loops} " +
			$"(checked ended {tally.Ended}, checked past its cap {tally.Slow}); differing {tally.Differ}\n\n" + string.Join("\n\n", problems));

		return tally;
	}

	/// <summary>
	/// A cycle cut at once, read from a position, in a window and searched for, answers as without the check wherever
	/// that one ends, and ends where it goes round.
	/// </summary>
	[Theory]
	[InlineData(true, "T = A & 'c'\nA = 'a' & A | @Still & A | 'b'")]
	[InlineData(false, "T = A & 'c'\nA = 'a' & A | @Still & A | 'b'")]
	[InlineData(true, "T = A & 'c'\nA = @Half & A | 'a' & A | 'b'")]
	[InlineData(false, "T = A & 'c'\nA = @Half & A | 'a' & A | 'b'")]
	public void A_cut_reads_from_a_position_and_in_a_search_as_without_the_check(bool direct, string grammar)
	{
		var oracle     = BuildPublished(grammar + "\npositional parse T\nfind T", direct, unchecked_: true);
		var checkedOne = Patient(BuildPublished(grammar + "\npositional parse T\nfind T", direct, unchecked_: false));

		foreach (var input in new[] { "xabc", "abcabbc", "xabbbcy", "abc;abbc" })
		{
			Agrees(Found(oracle, input), Found(checkedOne, input));

			for (var at = 0; at < input.Length; at++)
			{
				Agrees(From(oracle, input, at, null), From(checkedOne, input, at, null));

				for (var length = 0; at + length <= input.Length; length++)
					Agrees(From(oracle, input, at, length), From(checkedOne, input, at, length));
			}
		}
	}

	/// <summary>
	/// A parser that ends let read as long as the harness lets the checked one read, by calls and by the clock: the half
	/// second an oracle is given to show it goes round is less than a first reading takes on a loaded machine.
	/// </summary>
	static Assembly Patient(Assembly parser)
	{
		var type = parser.GetType("Probe")!;

		type.GetField("Cap")!.SetValue(null, 30000);
		type.GetField("Ticks")!.SetValue(null, System.Diagnostics.Stopwatch.Frequency * 10);

		return parser;
	}

	/// <summary>The same answer where the oracle gives one; an end where it goes round.</summary>
	static void Agrees(string oracle, string checkedOne)
	{
		if (oracle.Contains("LOOP", StringComparison.Ordinal))
			Assert.DoesNotContain("LOOP", checkedOne, StringComparison.Ordinal);
		else
			Assert.Equal(oracle, checkedOne);
	}

	static Assembly BuildPublished(string grammar, bool direct, bool unchecked_)
	{
		var was = Reentry.Unchecked;

		try
		{
			Reentry.Unchecked = unchecked_;

			var result = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName      = "Probe",
				Direct         = direct,
				SymbolResolver = PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(result.Diagnostics);

			return EmittedCode.Compile(
				result.Sources[0].Text, className: "Probe", declarationMembers: SpanHost,
				sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		}
		finally
		{
			Reentry.Unchecked = was;
		}
	}

	static string Found(Assembly parser, string input)
	{
		parser.GetType("Probe")!.GetMethod("Reset")!.Invoke(null, null);

		var found = new List<string>();

		try
		{
			foreach (var match in (System.Collections.IEnumerable)parser.GetType("Probe")!.GetMethod("FindT", [typeof(string)])!.Invoke(null, [input])!)
				found.Add(match.GetType().GetProperty("Position")!.GetValue(match) + "+" + match.GetType().GetProperty("Length")!.GetValue(match));
		}
		catch (InvalidOperationException thrown) when (thrown.Message == "went round")
		{
			found.Add("LOOP");
		}

		return string.Join(" ", found);
	}

	static string From(Assembly parser, string input, int at, int? length)
	{
		var type = parser.GetType("Probe")!;

		type.GetMethod("Reset")!.Invoke(null, null);

		var method = length is null
			? type.GetMethod("TryParseT", [typeof(string), typeof(int)])!
			: type.GetMethod("TryParseT", [typeof(string), typeof(int), typeof(int)])!;

		try
		{
			var match = method.Invoke(null, length is null ? [input, at] : [input, at, length.Value])!;

			object? Read(string name)
			{
				return match.GetType().GetProperty(name)!.GetValue(match);
			}

			return (bool)Read("IsSuccess")! ? $"ok@{Read("Position")}+{Read("Length")}" : $"no@{Read("Position")} {Read("Outcome")} {Read("Error")}";
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException?.Message == "went round")
		{
			return "LOOP";
		}
	}

	/// <summary>A grammar cut in two whose tokenizer reads a rule that comes back to itself compiles, and reads as without the check.</summary>
	[Theory]
	[InlineData("Rest   = @Dot & Rest & 'b' | \"*)\"", "ab (*.*)b")]
	[InlineData("Rest   = ([^ '*'] | @Dot & Rest)* & \"*)\"", "ab (**)")]
	[InlineData("Rest   = @Dot & Rest | [^ '*'] & Rest | \"*)\"", "ab (*.x.*)")]
	public void A_tokenizer_rule_that_comes_back_compiles(string rest, string input)
	{
		var grammar = "using Lexical;\ntrivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\tName   = ['a'..'z']+\n\tRemark = \"(*\" & Rest\n\t" + rest + "\n}\nT = Name & Remark";

		foreach (var direct in new[] { true, false })
		{
			var oracle     = Build(grammar, direct, tape: false, buffered: false, unchecked_: true, lexical: true)!;
			var checkedOne = Build(grammar, direct, tape: false, buffered: false, unchecked_: false, lexical: true)!;

			Assert.True(checkedOne.Marked);
			Assert.StartsWith("ok@", Answer(Patient(oracle.Assembly), input, typeof(string)), StringComparison.Ordinal);
			Assert.Equal(Answer(oracle.Assembly, input, typeof(string)), Answer(Patient(checkedOne.Assembly), input, typeof(string)));
		}
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

		if (only.Any(static one => one.StartsWith(ValuedTop, StringComparison.Ordinal)))
			Assert.True(tally.Resplit > 0, "no valued fixture was split differently from the oracle: the classifier saw nothing");
	}

	/// <summary>The grammars a check on the position would answer otherwise: found, told and generated as without the check, in every reading.</summary>
	[Fact]
	public void Known_disagreements_are_not_checked()
	{
		foreach (var (grammar, why) in Known)
			foreach (var direct in new[] { true, false })
				foreach (var tape in new[] { false, true })
					foreach (var buffered in new[] { false, true })
					{
						var checkedOne = Build(grammar, direct, tape, buffered, unchecked_: false)!;
						var oracle     = Build(grammar, direct, tape, buffered, unchecked_: true)!;

						Assert.True(checkedOne.Marked, why);
						Assert.Equal(oracle.Source, checkedOne.Source);
					}
	}

	[Fact]
	public void Enumeration()
	{
		if (!Requested)
			return;

		var tally = Run(Enumerated(), Wide ? 4 : 3, 2, Wide ? "reentry-harness-wide.txt" : "reentry-harness-enumeration.txt");

		Assert.True(tally.Marked > 0);
		Assert.True(tally.Valued > 0 && tally.Resplit > 0, "no valued top was built and split differently from the oracle: the classifier saw nothing");
		Assert.True(tally.Differ == 0, string.Join("\n\n", tally.Problems.Take(10)));
	}
}
