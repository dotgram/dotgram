using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// GRAM4052 held to what the parsers do: every repetition it tells is driven exponentially by a
/// refused input, every one it is silent about is not, and the cure of each told one reads what
/// the shape read and is linear.
/// </summary>
/// <remarks>
/// <para>
/// A ladder is a run of the text the repetition reads two ways, repeated, then a tail that no
/// reading can finish. Judged by ratio, two lengths timed in one run (<see cref="ScalingClock"/>):
/// a told shape's longer run takes hundreds of times what the shorter does, where the text grows
/// by a handful of turns; a silent shape's run twice as long takes about twice as long, four
/// times where a known square is paid, and never more than <see cref="Linear"/>. The lengths
/// stay small (twenty turns at most), so that an exponential shape is shown and not suffered.
/// </para>
/// <para>
/// Two shapes are pinned against the analysis rather than with it: a look of more than one
/// character that removes the second reading is told and is linear, the false report the
/// diagnostics page names; and a recursive rule reading what a character beside it reads is
/// not told and is exponential, the miss it names.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class ExponentialRepetitionScalingTests
{
	/// <summary>A shape: the grammar, the text it repeats, a tail it refuses and one it accepts, and the lengths timed.</summary>
	/// <param name="Read">A text the shape reads, where the run with the accepted tail is not one.</param>
	/// <param name="Recovers">Whether the shape recovers from what it refuses, and so accepts every ladder.</param>
	/// <param name="Text">The text of n turns, where it is not the unit n times.</param>
	public sealed record Shape(
		string Grammar, string Unit, string Refused, string Accepted, int Small, int Large, string? Cure = null,
		bool Lexical = false, string Start = "R", string? Read = null, bool Recovers = false, Func<int, string>? Text = null)
	{
		public override string ToString()
		{
			return Grammar.Replace("\n", " ; ", StringComparison.Ordinal);
		}
	}

	/// <summary>A told shape's longer run against its shorter one, at least.</summary>
	const double Exponential = 20;

	/// <summary>
	/// A silent shape's run of twice the length against the shorter one, at most: twice is linear,
	/// and four times the square that a refusal pays where a repetition gives a turn back and the
	/// turns before it are read again (a known limit of the reader, and two runs one after the
	/// other, <c>'a'* &amp; 'a'*</c>, cut a text as many ways as it is long). Not exponential is what
	/// is held; a told shape pays hundreds of times.
	/// </summary>
	const double Linear = 6;

	const string Template =
		"LiteralChar = ['a'..'z' | '0'..'9']\n" +
		"Pct = '%' & ['0'..'9'] & ['0'..'9']\n" +
		"Literals = (LiteralChar | Pct)+\n" +
		"Part = Literals | '{' & ['a'..'z']+ & '}'\n" +
		"R = Part* & eof";

	const string Split = "trivia = { ' '* }\nnamespace Token { trivia = none\n Id = ['a'..'z']+ }\n";

	/// <summary>The repetitions GRAM4052 tells, each with a cure that reads the same texts.</summary>
	public static TheoryData<Shape> Told { get; } = new()
	{
		new Shape("R = (('a' | 'a' & 'a')+)* & 'b'", "a", "!", "b", 5, 11, Cure: "R = ({ ('a' | 'a' & 'a')+ })* & 'b'"),
		new Shape("R = ('x'+)* & 'b'", "x", "!", "b", 8, 16, Cure: "R = 'x'* & 'b'"),
		new Shape("R = ('x'+)+ & 'b'", "x", "!", "b", 8, 16, Cure: "R = 'x'+ & 'b'"),
		new Shape("R = ('a' | 'a' & 'a')+ & 'b'", "a", "!", "b", 10, 20, Cure: "R = 'a'+ & 'b'"),
		new Shape(
			"Cfws = ' '+\nW = { ['a'..'z']+ }\nR = (Cfws? & W & Cfws?)+ & ';'", "a ", "!", ";", 6, 14,
			Cure: "Cfws = ' '+\nW = { ['a'..'z']+ }\nR = Cfws? & (W & Cfws?)+ & ';'"),
		new Shape(Template, "a", "{", "", 8, 16, Cure: Template.Replace("Literals = (LiteralChar | Pct)+", "Literals = { (LiteralChar | Pct)+ }", StringComparison.Ordinal)),
		new Shape(Address(braced: false), "a ", "<", "<b>", 6, 14, Cure: Address(braced: true)),
		new Shape("X = 'x'\nY = 'x'\nR = ((X | Y) & 'z')* & 'b'", "xz", "!", "b", 6, 14, Cure: "X = 'x'\nY = 'x'\nR = (X & 'z')* & 'b'"),

		// Trivia joined with `~` is no seam: `(a* a)*`.
		new Shape("trivia = 'a'*\nR = (trivia ~ 'a')* ~ 'b'", "a", "!", "b", 8, 16, Cure: "trivia = 'a'*\nR = 'a'* ~ 'b'"),

		// Braces that never succeed never commit: as many turns as the `q`s, one `a` short.
		new Shape(
			"R = $q:{ 'q'+ } ~ ':' ~ { ('a' | 'a' & 'a'){#q,} } ~ 'b'", "", "!", "ab", 16, 24,
			Cure: "R = $q:{ 'q'+ } ~ ':' ~ { 'a'{#q,} } ~ 'b'",
			Text: static turns => new string('q', Math.Max(turns, 1)) + ":" + new string('a', Math.Max(turns - 1, 0))),

		// Inside braces, what follows the run can fail and asks for every other reading of it.
		new Shape("R = { (('a' | 'a' & 'a')+ & 'c')* } & 'b'", "a", "!", "cb", 10, 20, Cure: "R = { ('a'+ & 'c')* } & 'b'"),

		// A turn that never finishes: every reading of the run is tried before the class that
		// admits nothing refuses each of them.
		new Shape("R = ((\"xy\" | \"xy\")+ & ['b'..'a'] | 'z')* & eof", "xy", "!", "", 6, 14, Cure: "R = (\"xy\"+ & ['b'..'a'] | 'z')* & eof", Read: "zz"),
	};

	/// <summary>The shapes GRAM4052 is silent about.</summary>
	public static TheoryData<Shape> Silent { get; } = new()
	{
		new Shape("R = { ('x'+)* } & 'b'", "x", "!", "b", 200, 400),
		new Shape("R = ({ 'x'+ })* & 'b'", "x", "!", "b", 200, 400),
		new Shape("R = { ('a' | 'a' & 'a')+ } & 'b'", "a", "!", "b", 200, 400),
		new Shape("R = (',' & ['a'..'z'] | ';' & ['0'..'9'])* & eof", ",a", "!", "", 200, 400),
		new Shape("R = ('x' & 'y' | 'x' & 'y')* recover ';' & eof", "xy", "!", "", 200, 400, Recovers: true),
		new Shape("X = 'x'\nY = 'x'\nR = (X | Y)* & 'b'", "x", "!", "b", 200, 400),
		new Shape(Split + "R = (Token.Id | Token.Id & Token.Id)+ & ';'\nStart = R & eof", "a ", "", ";", 200, 400, Lexical: true, Start: "Start"),
		new Shape(Split + "R = (Token.Id | Token.Id & Token.Id)+ & (';' | '(' & R & ')')\nStart = R & eof", "a ", "", ";", 200, 400, Lexical: true, Start: "Start"),
		new Shape(
			"trivia = { ' '* }\nnamespace Token { trivia = none\n Id = (['a'..'z']+)+ }\nStart = Token.Id+ & eof", "ab ", "!", "", 200, 400,
			Lexical: true, Start: "Start"),
		new Shape("trivia = ' '*\nW = ['a'..'z']+\nR = (W & ',')+ & eof", "a, ", "!", "", 200, 400),
		new Shape("R = ('\\'' & ?!'\\'' & any)* & eof", "'a", "'", "", 200, 400),
		new Shape("R = (['a'..'z']+ & ':' & [^ '\\n']* & ('\\n' | ?=eof))* & eof", "a:b\n", "!", "", 200, 400),
		new Shape("C = ';' & { [^ '\\n']* }\nT = ' '* & C | C\nR = ('[' & ['a'..'z'] & ']' & T?)* & eof", "[a]", "!", "", 200, 400),
		new Shape("R = 'a'* & 'a'* & 'b'", "a", "!", "b", 200, 400),

		// Nothing after the run inside the braces or the look can fail: its first reading stands.
		new Shape("R = { (('a' | 'a' & 'a')+)* } & 'b'", "a", "!", "b", 200, 400),
		new Shape("R = (?=(('a' | 'a' & 'a')+)) & 'a'+ & 'b'", "a", "!", "b", 200, 400),

		// The first braced call takes the whole run.
		new Shape("X = { 'a'+ }\nR = (X | X & X)+ & 'b'", "a", "!", "b", 200, 400),
	};

	[Theory]
	[MemberData(nameof(Told))]
	public void A_told_repetition_refuses_in_exponential_time(Shape shape)
	{
		Assert.True(Says(shape.Grammar, shape.Lexical, shape.Start), "GRAM4052 is not said of the shape.");

		Exponentially(shape);
	}

	[Theory]
	[MemberData(nameof(Told))]
	public void The_cure_of_a_told_repetition_reads_the_same_texts_in_linear_time(Shape shape)
	{
		var cure = shape with { Grammar = shape.Cure!, Small = 100, Large = 200 };

		Assert.False(Says(cure.Grammar, cure.Lexical, cure.Start), "GRAM4052 is said of the cure.");

		var told  = Parser(shape.Grammar, shape.Lexical);
		var cured = Parser(cure.Grammar, cure.Lexical);

		foreach (var turns in new[] { 0, 1, 2, 3, 5 })
			foreach (var tail in new[] { shape.Accepted, shape.Refused, "" })
			{
				var input = Input(shape, turns, tail);

				Assert.True(
					Accepts(told, shape.Start, input) == Accepts(cured, shape.Start, input),
					$"The shape and its cure disagree on \"{input}\".");
			}

		Linearly(cure);
	}

	[Theory]
	[MemberData(nameof(Silent))]
	public void A_silent_repetition_refuses_in_linear_time(Shape shape)
	{
		Assert.False(Says(shape.Grammar, shape.Lexical, shape.Start), "GRAM4052 is said of the shape.");

		Linearly(shape);
	}

	/// <summary>The false report the diagnostics page names: told, and linear.</summary>
	[Fact]
	public void A_look_of_more_than_one_character_is_told_and_linear()
	{
		var shape = new Shape("R = (?!\"aa\" & 'a' | \"aa\")+ & 'b'", "a", "!", "b", 200, 400);

		Assert.True(Says(shape.Grammar, shape.Lexical, shape.Start));

		Linearly(shape);
	}

	/// <summary>The miss the diagnostics page names: a recursive rule is one opaque step, so this is not told, and is exponential.</summary>
	[Fact]
	public void A_recursive_rule_reading_what_a_character_reads_is_not_told_and_exponential()
	{
		var shape = new Shape("X = 'a' | '(' & X & ')'\nR = (X | 'a')+ & 'b'", "a", "!", "b", 8, 16);

		Assert.False(Says(shape.Grammar, shape.Lexical, shape.Start));

		Exponentially(shape);
	}

	/// <summary>
	/// The work of the analysis on the grammars read over characters that have the most
	/// repetitions, held under a fixed bound: the address grammar of the web library, and T-SQL
	/// read without its lexer. More work shows here as a count, not as a time; the time is
	/// written to the output, for a reader to set beside the compile's.
	/// </summary>
	[Theory]
	[InlineData("src/DotGram.Sql/TransactSql/TransactSql.gram", Bounds.TransactSql)]
	[InlineData("src/DotGram.Web/Rfc5322.cs", Bounds.Rfc5322)]
	public void The_work_on_the_largest_grammars_over_characters_is_bounded(string path, int bound)
	{
		var text  = Text(Path.Combine(SourceRoot.Directory, path));
		var watch = Stopwatch.StartNew();
		var graph = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramCompiler.Read(text, RoslynCSharpScanner.Instance).File, null, text.Length),
			scanner: RoslynCSharpScanner.Instance);
		var built = watch.Elapsed.TotalMilliseconds;

		watch.Restart();

		var result = ExponentialRepetition.Check(graph, null, out var steps);
		var asked  = watch.Elapsed.TotalMilliseconds;

		Said($"{path}: {result.Loops} loops, {result.Skipped} past the budget, {steps} steps, {asked:F1} ms against {built:F1} ms to build the graph");

		Assert.Equal(0, result.Skipped);
		Assert.True(steps <= bound, $"{path}: {steps} steps, over the bound of {bound}.");
	}

	/// <summary>The chains of the fast suite's deep call graphs, eight thousand calls long (<see cref="DeepCallGraphs"/>).</summary>
	[Theory]
	[InlineData("chain of guards", 1, 0, 0)]
	[InlineData("chain of guards, then a run read two ways", 2, 1, 0)]
	[InlineData("chain of aliases repeated", 1, 0, 0)]
	public void A_chain_thousands_of_calls_deep_is_asked_in_bounded_work(string shape, int loops, int told, int skipped)
	{
		DeepCallGraphs.Ask(shape, 8000, loops, told, skipped);
	}

	/// <summary>
	/// Random chains and diamonds of calls, up to ten thousand deep and one or two calls a rule,
	/// under every kind of repetition the analysis walks calls from: each is asked on a thread of a
	/// megabyte, within a fixed allocation and a time that its counted steps account for, so that a
	/// walk that did work it did not count would show as time. A repetition past the budget is
	/// skipped and counted. The graph is built on a thread of its own with room for the normalizer;
	/// the graph's own first sets, which every stage shares, are not counted.
	/// </summary>
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(9)]
	[InlineData(10)]
	public void A_random_call_graph_of_any_depth_is_asked_within_bounds(int seed)
	{
		var random  = new Random(seed);
		var depth   = seed <= 2 ? 10_000 : random.Next(1, 10_001);
		var grammar = new System.Text.StringBuilder(random.Next(2) == 0 ? "A0 = 'a'\n" : "A0 = 'a' | 'a' & 'a'\n");

		string[] shapes = ["@", "@ | @", "@ | @ & any", "when @(true) & @", "@ & 'b'?"];
		string[] roots  =
		[
			"R = A# & 'z'", "R = A#* & 'z'", "R = (A# & 'a')* & 'z'", "R = (?!A# & 'x')* & 'z'",
			"R = { ('x' | 'x' & 'x')+ & A# } & 'z'", "R = (A# | 'a')+ & 'z'",
		];

		for (var i = 1; i <= depth; i++)
			grammar.Append($"A{i} = {shapes[random.Next(shapes.Length)].Replace("@", $"A{i - 1}", StringComparison.Ordinal)}\n");

		var root = roots[random.Next(roots.Length)].Replace("#", depth.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);

		// And one repetition known to be told, beside the random ones, so that a check that asked
		// nothing would not pass.
		grammar.Append(root).Append(" | P\nP = ('p' | 'p' & 'p')+ & 'q'\nparse R\n");

		var text = grammar.ToString();

		RecognitionGraph? graph = null;

		OnThread(
			() =>
			{
				var parsed = GramCompiler.Read(text, RoslynCSharpScanner.Instance);

				Assert.DoesNotContain(parsed.Diagnostics, static one => one.Severity == GramSeverity.Error);

				graph = GrammarNormalizer.Normalize(GrammarBinder.Bind(parsed.File, null, text.Length), scanner: RoslynCSharpScanner.Instance);
			},
			1 << 30);

		ExponentialRepetition.Result? result = null;

		var steps = 0;
		var bytes = 0L;
		var watch = Stopwatch.StartNew();

		OnThread(
			() =>
			{
				bytes  = GC.GetAllocatedBytesForCurrentThread();
				result = ExponentialRepetition.Check(graph!, null, out steps);
				bytes  = GC.GetAllocatedBytesForCurrentThread() - bytes;
			},
			1 << 20);

		Said($"seed {seed}, depth {depth}, {root}: {result!.Loops} loops, {result.Skipped} skipped, {result.Found.Count} told, {steps} steps, {bytes} bytes, {watch.Elapsed.TotalMilliseconds:F1} ms");

		var planted = Assert.Single(result.Found, static one => one.Rule.Name == "P");

		Assert.Equal("'p' 'p'", planted.Witness);
		Assert.True(result.Loops >= 1, "No repetition was asked.");
		Assert.True(result.Skipped < result.Loops, "Every repetition was skipped.");
		Assert.True(steps > 0, "No work was counted.");
		Assert.True(bytes < 64 << 20, $"{bytes} bytes allocated");

		// Half a second, and twenty microseconds a step: what an uncounted walk down ten thousand
		// rules, or every path of a diamond, would overrun.
		Assert.True(
			watch.Elapsed.TotalMilliseconds <= 500 + steps * 0.02,
			$"{watch.Elapsed.TotalMilliseconds:F0} ms for {steps} steps");
	}

	static void OnThread(Action run, int stack)
	{
		Exception? failed = null;

		var thread = new System.Threading.Thread(
			() =>
			{
				try
				{
					run();
				}
				catch (Exception error)
				{
					failed = error;
				}
			},
			stack);

		thread.Start();
		thread.Join();

		if (failed is not null)
			System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failed).Throw();
	}

	/// <summary>The bounds of the work: what was measured, and a quarter more.</summary>
	static class Bounds
	{
		// 768,397 steps on 31.10.2026, building the automata and what can fail counted.
		public const int TransactSql = 960_000;

		// 2,041 steps on 31.10.2026.
		public const int Rfc5322 = 2_600;
	}

	/// <summary>A grammar file's text, or the grammar of the one <c>[Gram]</c> in a C# file.</summary>
	static string Text(string path)
	{
		var source = File.ReadAllText(path);

		if (path.EndsWith(".gram", StringComparison.Ordinal))
			return source;

		var attribute = CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>()
			.First(static one => one.Name.ToString() is "Gram" or "DotGram.Gram");

		return (string)((LiteralExpressionSyntax)attribute.ArgumentList!.Arguments[0].Expression).Token.Value!;
	}

	/// <summary>A copy of the address phrase, its word braced as the library braces it or not.</summary>
	static string Address(bool braced)
	{
		var word = braced ? "{ Cfws? & (AtomText | QuotedBody) & Cfws? }" : "Cfws? & (AtomText | QuotedBody) & Cfws?";

		return
			"Fws        = [' ' | '\\t']+\n" +
			"Comment    = '(' & { [^ ')']* } & ')'\n" +
			"Cfws       = { (Fws? & Comment)+ & Fws? } | Fws\n" +
			"AtomText   = { ['a'..'z' | 'A'..'Z' | '0'..'9' | '!' | '#']+ }\n" +
			"QuotedBody = '\"' & { [^ '\"']* } & '\"'\n" +
			$"WordText   = {word}\n" +
			"ObsPhrase  = WordText & (WordText | { '.' & Cfws? })*\n" +
			"R          = ObsPhrase & '<' & ['a'..'z']+ & '>'";
	}

	static void Exponentially(Shape shape)
	{
		var parser = Parser(shape.Grammar, shape.Lexical);
		var small  = Input(shape, shape.Small, shape.Refused);
		var large  = Input(shape, shape.Large, shape.Refused);

		Assert.False(Accepts(parser, shape.Start, large), "The ladder's input is accepted, and so says nothing.");
		Assert.True(Accepts(parser, shape.Start, shape.Read ?? Input(shape, shape.Small, shape.Accepted)), "The shape refuses what it should read.");

		var reading = ScalingClock.Settle(() => Accepts(parser, shape.Start, small), () => Accepts(parser, shape.Start, large), Exponential);

		Said($"{shape}: {shape.Small} -> {shape.Large} turns, {reading.Ratio:F1} times ({reading.Readings})");

		Assert.False(
			reading.Within,
			$"{shape.Large} turns took {reading.Ratio:F1} times what {shape.Small} took ({reading.Shorter:F0} µs against {reading.Longer:F0} µs): not exponential.");
	}

	static void Linearly(Shape shape)
	{
		var parser = Parser(shape.Grammar, shape.Lexical);
		var small  = Input(shape, shape.Small, shape.Refused);
		var large  = Input(shape, shape.Large, shape.Refused);

		Assert.True(shape.Recovers || !Accepts(parser, shape.Start, large), "The ladder's input is accepted, and so says nothing.");

		var reading = ScalingClock.Settle(() => Accepts(parser, shape.Start, small), () => Accepts(parser, shape.Start, large), Linear);

		Said($"{shape}: {shape.Small} -> {shape.Large} turns, {reading.Ratio:F1} times ({reading.Readings})");

		Assert.True(
			reading.Within,
			$"{shape.Large} turns took {reading.Ratio:F1} times what {shape.Small} took ({reading.Shorter:F0} µs against {reading.Longer:F0} µs): not linear.");
	}

	static void Said(string line)
	{
		TestContext.Current.SendDiagnosticMessage(line);
		Console.WriteLine(line);
	}

	static string Input(Shape shape, int turns, string tail)
	{
		return (shape.Text?.Invoke(turns) ?? string.Concat(Enumerable.Repeat(shape.Unit, turns))) + tail;
	}

	/// <summary>Whether GRAM4052 is said of a grammar.</summary>
	static bool Says(string grammar, bool lexical, string start)
	{
		return GramCompiler.Compile(grammar + $"\nparse {start}\n", Options(lexical)).Diagnostics
			.Any(static one => one.Id == GramCompiler.ExponentialRepetitions);
	}

	static readonly ConcurrentDictionary<(string, bool), Assembly> Parsers = new();

	static Assembly Parser(string grammar, bool lexical)
	{
		return Parsers.GetOrAdd((grammar, lexical), static key =>
		{
			var start  = key.Item1.Contains("Start =", StringComparison.Ordinal) ? "Start" : "R";
			var result = GramCompiler.Compile(key.Item1 + $"\nparse {start}\n", Options(key.Item2));

			Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

			return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		});
	}

	static GramCompilerOptions Options(bool lexical)
	{
		return new GramCompilerOptions
		{
			ClassName      = "Grammar",
			Lexical        = lexical,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		};
	}

	static bool Accepts(Assembly parser, string start, string input)
	{
		return EmittedCode.Match(parser, "Grammar", "TryParse" + start, input).IsSuccess;
	}
}
