using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refusal after adjacent repetitions that can take the same characters: the direct reader must grow
/// as the engine does.
/// </summary>
/// <remarks>
/// <para>
/// <b>The shape.</b> <c>R0 = 'o'* &amp; 'o'?</c> written k times before a character the input does not
/// have. Every reading of the k repetitions is tried before the refusal, which is about n^k ways for
/// both machines: that is the language (nothing over characters is memoised), and it is not what this
/// holds. What it holds is the price of one way. The engine resumes a way where it was opened. The
/// direct reader replays: a failed rule moves its tape on and runs its body again from the top, and
/// the replay used to read again everything in front of the way it moved, a run scanned to its end
/// and then cut back, every turn of a repetition and every host recognizer in it called again. One
/// way cost O(n), and the reader grew n^3.7 where the engine grew n^2.9 (k = 3). The same shape is
/// `Ows &amp; X? &amp; Ows` in a header of the web formats, and a value of spaces reached it.
/// </para>
/// <para>
/// <b>Two gates, both on the ratio of the two machines.</b> What is held is not the cost of the
/// language but how the direct reader's cost grows against the engine's: from 25 characters to the
/// largest size, the ratio may not grow more than twice, which over 25 to 200 is an exponent at most a
/// third above the engine's. The host calls of the <c>@Odd</c> form are a count, with no clock in it.
/// The plain form is timed in one process, the two machines interleaved, each sample long enough to
/// time. Before the replay stepped over runs and turns, the count's ratio went from 8 to 100 over 16 to
/// 200 characters (k = 3), and the clock's grew about eight times.
/// </para>
/// <para>
/// What is left is a constant: a replay still calls again the alternative a choice took, <c>@Nothing</c>
/// or <c>@Odd</c> once in each copy of <c>R0</c>, where the engine resumes it, so the direct reader
/// makes about 1.6 (k = 2) and 2.3 (k = 3) times the engine's calls at every size.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class ReaderGiveBackGrowthTests
{
	const string Host = """
		public static long Calls;
		static bool Odd(System.ReadOnlySpan<char> input, ref int p) { Calls++; if (p < input.Length && input[p] == 'o') { p++; return true; } return false; }
		static bool Nothing(System.ReadOnlySpan<char> input, ref int p) { Calls++; return true; }
		""";

	/// <summary>The longest one refusal may take before the series stops and fails.</summary>
	const double BudgetMilliseconds = 2000;

	/// <summary>How long a sample is timed for: a refusal of 25 characters is a fraction of a millisecond.</summary>
	const double SampleMilliseconds = 5;

	static string Family(string r0, int copies)
	{
		return r0 + "\nT = " + string.Join(" & ", Enumerable.Repeat("R0", copies)) + " & 'a' & eof\nparse T\n";
	}

	[Theory]
	[InlineData(2, new[] { 25, 50, 100, 200 })]
	[InlineData(3, new[] { 25, 50, 100 })]
	public void A_host_recognizer_is_called_in_step_with_the_engine(int copies, int[] sizes)
	{
		var grammar = Family("R0 = (@Odd)* & (@Nothing | @Odd)", copies);
		var direct  = Compiled(grammar, direct: true, Host);
		var engine  = Compiled(grammar, direct: false, Host);
		var seen    = new List<string>();
		var first   = 0.0;

		foreach (var n in sizes)
		{
			var text = new string('o', n) + "b";
			var held = Calls(engine, text);
			var read = Calls(direct, text);

			Assert.True(held > 0, $"k = {copies}, n = {n}: the engine called the host {held} times, so nothing is counted.");

			var ratio = (double)read / held;

			if (seen.Count == 0)
				first = ratio;

			seen.Add($"{n}: {read:N0}/{held:N0}");

			// Asked at every size, so that a reader that has lost the bound stops at the first size
			// that shows it rather than counting its way through the largest.
			Assert.True(
				ratio <= 2 * first,
				$"k = {copies}: the direct reader went from {first:F1} to {ratio:F1} times the engine's host calls; {string.Join(", ", seen)}.");
		}
	}

	[Fact]
	public void A_refusal_after_overlapping_repetitions_grows_as_the_engine_does()
	{
		// Two copies: three are the same shape a power higher, and a size of 200 then takes seconds
		// a call in this harness, which compiles without optimisation.
		const int copies = 2;

		int[] sizes = [25, 50, 100, 200];

		var grammar = Family("R0 = 'o'* & 'o'?", copies);
		var direct  = Compiled(grammar, direct: true);
		var engine  = Compiled(grammar, direct: false);

		Assert.True(Accepts(direct, "oooa") && Accepts(engine, "oooa"), "the family's accepted control is refused.");

		var read = new List<double>();
		var held = new List<double>();

		foreach (var n in sizes)
		{
			var text = new string('o', n) + "b";

			Assert.False(Accepts(direct, text), $"n = {n}: the refused input is accepted, so it measures nothing.");

			// A guard, not a wait: one call over the budget is the answer.
			var once = Clock(direct, text, 1);

			Assert.True(once <= BudgetMilliseconds, $"k = {copies}, n = {n}: one refusal took {once:F0} ms in the direct reader.");

			// Interleaved, best of five: a neighbour slows both alike.
			var a = double.MaxValue;
			var b = double.MaxValue;

			for (var round = 0; round < 5; round++)
			{
				a = Math.Min(a, Clock(direct, text, SampleMilliseconds));
				b = Math.Min(b, Clock(engine, text, SampleMilliseconds));
			}

			read.Add(a);
			held.Add(b);
		}

		var first = read[0] / held[0];
		var last  = read[^1] / held[^1];

		Assert.True(
			last <= 2 * first,
			$"k = {copies}: the direct reader went from {first:F1} to {last:F1} times the engine's time; " +
			$"{string.Join(", ", sizes.Select((n, i) => $"{n}: {read[i]:F3}/{held[i]:F3} ms"))}.");
	}

	static Type Compiled(string grammar, bool direct, string? host = null)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grown",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = direct,
		});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(
			result.Sources[0].Text, "Grown", declarationMembers: host,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text)).GetType("Grown")!;
	}

	static bool Accepts(Type parser, string text)
	{
		var match = parser.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, [text])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	static long Calls(Type parser, string text)
	{
		var calls = parser.GetField("Calls", BindingFlags.Public | BindingFlags.Static)!;

		calls.SetValue(null, 0L);
		Assert.False(Accepts(parser, text), $"\"{text}\" is accepted, so it measures nothing.");

		return (long)calls.GetValue(null)!;
	}

	/// <summary>Milliseconds a refusal, read again until <paramref name="least"/> milliseconds have passed.</summary>
	static double Clock(Type parser, string text, double least)
	{
		var watch = Stopwatch.StartNew();
		var calls = 0;

		do
		{
			Accepts(parser, text);
			calls++;
		}
		while (watch.Elapsed.TotalMilliseconds < least);

		return watch.Elapsed.TotalMilliseconds / calls;
	}
}
