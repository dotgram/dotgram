using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The two checks at a rule that comes back to itself through a recognizer that does not move (docs/syntax.md §7.2):
/// where nothing is read after the call back, an entry where one already stands is cut at once; elsewhere the entries
/// standing at a position are counted against the input left, and a reading in which too many come back throws.
/// </summary>
/// <remarks>
/// The cut at once is held to the count on every input up to five items over each shape's alphabet: the same verdict,
/// end, outcome and message, where only the count would be what the check reads; and where the shape is not one it
/// applies to, the generated parser is the count's, character for character.
/// </remarks>
public sealed class ReentryTailTests
{
	const string Host = """
		static bool Still(System.ReadOnlySpan<char> input, ref int p) { return true; }
		static bool Ws(System.ReadOnlySpan<char> input, ref int p) { while (p < input.Length && input[p] == ' ') p++; return true; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { if (p < input.Length && input[p] == '.') p++; return true; }
		""";

	/// <summary>The shapes, each with whether nothing is read after its call back, and the items its inputs are made of.</summary>
	static readonly (string Name, string Grammar, bool Tail, string Alphabet)[] Shapes =
	[
		("self",       "T = @Still & T | 'a' & T | eof",                                  true,  "ab"),
		("mutual",     "T = @Still & U | 'a'\nU = @Still & T | 'b' & T",                  true,  "ab"),
		("half",       "T = @Half & T | eof",                                             true,  ".x"),
		("second",     "T = 'a' & T | @Still & T | 'b'",                                  true,  "ab"),
		("giveback",   "T = S & 'c'\nS = @Still & S | 'a' | 'a' & 'b'",                   true,  "abc"),
		("lookahead",  "T = ?!'b' & @Still & T | 'a' & T | 'b'",                          true,  "ab"),
		("list",       "T = S & eof\nS = @Ws & S | Num & ('+' & S)?\nNum = ['0'..'9']+",  true,  "1+ "),
		("value",      "T = d: A & when @(d >= 2)\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)", false, "a"),
		("sum",        "T = Sum & eof\nSum = Num | @Ws & Sum & '+' & Num\nNum = ['0'..'9']+", false, "1+ "),
		("abc",        "T = A & 'c'\nA = 'a' | @Still & A & 'b'",                         false, "abc"),
		("block",      "T = @Still & T & 'e' | 'x'",                                      false, "xe"),
	];

	public static TheoryData<string> Names()
	{
		var names = new TheoryData<string>();

		foreach (var shape in Shapes)
			names.Add(shape.Name);

		return names;
	}

	/// <summary>Which cycles are cut at once: none where something is read after the call back, or what is read depends on more than the position.</summary>
	[Theory]
	[InlineData("T = @Still & T | 'a' & T | eof", true)]
	[InlineData("T = S & eof\nS = @Ws & S | Num & ('+' & S)?\nNum = ['0'..'9']+", true)]
	[InlineData("T = ?='.' & @Still & T | ?!'.' & 'x'", true)]
	[InlineData("T = x: A\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)", true)]
	[InlineData("T = @Still & T & 'e' | 'x'", false)]
	[InlineData("T = (@Still & T | 'a') & 'b'", false)]
	[InlineData("T = ?=(@Still & T) & 'x' | 'y'", false)]
	[InlineData("T = 'x' | ?!'y' & (@Still & T)+", false)]
	[InlineData("T = { @Still & T } | 'a'", false)]
	[InlineData("T = d: A & when @(d >= 2)\nA : @int = 'a' => @(1) | @Still & i: A => @(i + 1)", false)]
	[InlineData("T = E\nE = @Still & E << 2 | E & '+' & E << 1 | 'x'", false)]
	[InlineData("context : @Ctx\nT = A\nA = when @(context.Depth >= 5) & 'x' | when @(context.Bump() > 0) & @Still & A", false)]
	public void Cut_at_once_where_nothing_is_read_after_the_call_back(string grammar, bool tail)
	{
		var graph  = ExponentialRepetitionTests.Graph(grammar + "\nparse T", lexical: false, out _);
		var marked = Reentry.MarkedRules(graph);

		Assert.NotEmpty(marked);

		foreach (var rule in marked)
			Assert.Equal(tail, Reentry.Tail(graph, rule));
	}

	/// <summary>
	/// The cut at once answers as the count does, input for input, in the reader by methods and in the engine; and where
	/// a shape is not cut at once, its parser is the count's.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void A_cut_at_once_answers_as_the_count(string name)
	{
		var (_, grammar, tail, alphabet) = Shapes.Single(shape => shape.Name == name);

		var inputs = 0;

		foreach (var direct in new[] { true, false })
		{
			var (cut, cutSource)         = Build(grammar, direct, untailed: false);
			var (counted, countedSource) = Build(grammar, direct, untailed: true);

			Assert.Equal(tail, cutSource != countedSource);

			foreach (var input in Inputs(alphabet, 5))
			{
				Assert.True(
					Answer(counted, input) == Answer(cut, input),
					$"{name} {(direct ? "direct" : "engine")} \"{input}\": counted {Answer(counted, input)} | cut {Answer(cut, input)}");

				inputs++;
			}
		}

		TestContext.Current.SendDiagnosticMessage($"{name}: {inputs} inputs answered alike");
	}

	/// <summary>
	/// A cycle cut at once reads long input in time linear in it, valid or not, where the count goes round about the
	/// square of the input with a deep stack, or a factorial of it where nothing derives the input.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_cut_at_once_reads_long_input_in_linear_time(bool direct)
	{
		var (parser, _) = Build("T = @Still & T | 'a' & T | eof", direct, untailed: false);

		var valid = new string('a', 65536);

		// The engine's first reading compiles what it reads; time the second.
		Answer(parser, "aab");

		var watch = Stopwatch.StartNew();

		Assert.Equal("ok@65536", Answer(parser, valid));

		var accepted = watch.Elapsed;

		watch.Restart();

		Assert.StartsWith("no@65536 ", Answer(parser, valid + "b"), StringComparison.Ordinal);

		var refused = watch.Elapsed;

		TestContext.Current.SendDiagnosticMessage(
			$"{(direct ? "direct" : "engine")}: valid 64k {accepted.TotalMilliseconds:F1} ms, invalid 64k {refused.TotalMilliseconds:F1} ms");

		// Far above what it takes (tens of milliseconds), far below the count's minutes.
		Assert.True(accepted.TotalSeconds < 5 && refused.TotalSeconds < 5, $"{accepted} / {refused}");

		// Where the count would reach its ceiling, from nine items on: a refusal, the same as at eight.
		for (var length = 8; length <= 12; length++)
			Assert.StartsWith($"no@{length - 1} ", Answer(parser, new string('a', length - 1) + "b"), StringComparison.Ordinal);
	}

	/// <summary>
	/// A counted cycle whose search doubles with every two items throws past the ceiling, 2²⁴ entries that came back on
	/// input this short, rather than reading on for minutes; one whose valid input needs about the square of it does not.
	/// </summary>
	[Fact]
	public void A_counted_cycle_throws_past_the_ceiling()
	{
		const string Twice = "T = Sum & eof\nSum = Num | @Ws & Sum & '+' & Num | @Ws & Sum & '-' & Num\nNum = ['0'..'9']+";
		const string Once  = "T = Sum & eof\nSum = Num | @Ws & Sum & '+' & Num\nNum = ['0'..'9']+";

		var twice = Build(Twice, direct: true, untailed: false).Parser;
		var once  = Build(Once, direct: true, untailed: false).Parser;

		Assert.Equal("ok@11", Answer(twice, "1+1-1+1-1+1"));

		var thrown = Assert.Throws<TargetInvocationException>(() => Answer(twice, "1+1-1+1-1+1-1+1-1+1-1+1-1+1"));

		Assert.Equal(
			"Rule 'Sum' came back to itself through @Ws without reading anything too many times at 0; the input is too costly for this grammar.",
			Assert.IsType<InvalidOperationException>(thrown.InnerException).Message);

		var sum = string.Join("+", Enumerable.Repeat("1", 1000));

		Assert.Equal($"ok@{sum.Length}", Answer(once, sum));
	}

	static (Assembly Parser, string Source) Build(string grammar, bool direct, bool untailed)
	{
		GramCompilation result;

		Reentry.Untailed = untailed;

		try
		{
			result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
			{
				ClassName      = "Probe",
				Direct         = direct,
				SymbolResolver = PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});
		}
		finally
		{
			Reentry.Untailed = false;
		}

		EmittedCode.Quiet(result.Diagnostics);

		var parser = EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: Host,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		return (parser, result.Sources[0].Text);
	}

	/// <summary>The verdict, where it ended, and for a refusal its outcome and message.</summary>
	static string Answer(Assembly parser, string input)
	{
		var match = parser.GetType("Probe")!.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, [input])!;

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"ok@{(long)Read("Position")! + (int)Read("Length")!}"
			: $"no@{Read("Position")} {Read("Outcome")} {Read("Error")}";
	}

	static IEnumerable<string> Inputs(string alphabet, int longest)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= longest; length++)
		{
			level = [.. level.SelectMany(prefix => alphabet.Select(c => prefix + c))];

			foreach (var input in level)
				yield return input;
		}
	}
}
