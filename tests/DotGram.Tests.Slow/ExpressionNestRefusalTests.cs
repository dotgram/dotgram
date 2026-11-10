using System;
using System.Diagnostics;
using System.Linq;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Three nests of the expression language that cost the square of their depth, read or refused, and
/// what each costs now: about as much a level at every depth.
/// </summary>
/// <remarks>
/// <para>
/// <b>Generic arguments nested</b>, <c>List&lt;List&lt;…&gt;&gt;</c>: a named type kept the text of its
/// arguments, which is the text of every nest inside them, so every level copied what was below it — 2.6 GB
/// for 6,400 levels never closed, and half that closed.
/// </para>
/// <para>
/// <b>A conditional chain cut short</b>, <c>b ? 1 : b ? 1 : … : b</c> with branches that meet in no type:
/// each such conditional asks its branches whether they convert to a type, and its branch is another one,
/// so every level asked the whole chain below it — 1.8 s for 12,800 levels, bytes flat.
/// </para>
/// <para>
/// <b>Comparisons a type could begin</b>, <c>a &lt; a &lt; … &gt; b</c> over a parameter <c>a</c>: every
/// <c>a</c> was tried as a generic type and read the rest of the chain as its arguments before the head was
/// found to be the parameter — 1.2 s for 6,400, bytes flat.
/// </para>
/// <para>
/// The two whose bytes are flat are held to time, as the doubling sizes are: at most twice as many times
/// as long as there are times the levels, where a square takes that many times that many. The sizes are
/// far enough apart for the square to be told from a slow machine.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class ExpressionNestRefusalTests
{
	[Fact]
	public void A_nest_of_generic_arguments_allocates_about_as_many_bytes_a_level_at_every_size()
	{
		AssertLinear("never closed", static n => "(" + Repeated("System.Collections.Generic.List<", n) + "int" + Repeated(">", n - 1) + " x) => x", accepted: false);
		AssertLinear("closed", static n => "(" + Repeated("System.Collections.Generic.List<", n) + "int" + Repeated(">", n) + " x) => 1", accepted: true);
	}

	[Fact]
	public void A_conditional_chain_cut_short_takes_about_as_long_a_level_at_every_size()
	{
		AssertLinearTime("a conditional chain, cut", 1600, 12800, static n => "(bool b) => " + Repeated("b ? 1 : ", n));
	}

	[Fact]
	public void Comparisons_a_type_could_begin_take_about_as_long_a_level_at_every_size()
	{
		AssertLinearTime("a < … > b", 1600, 12800, static n => "(int a, int b) => " + string.Join(" < ", Enumerable.Repeat("a", n)) + " > b");
	}

	/// <summary>Bytes a level at four doubling depths stay within a quarter of one another.</summary>
	static void AssertLinear(string shape, Func<int, string> textFor, bool accepted)
	{
		int[] depths = [400, 800, 1600, 3200];

		var texts = depths.Select(textFor).ToArray();

		Assert.Equal(accepted, ExpressionParser.TryParse(texts[^1]).IsSuccess);

		var each = new double[depths.Length];

		for (var i = 0; i < depths.Length; i++)
		{
			var fewest = long.MaxValue;

			for (var run = 0; run < 3; run++)
			{
				var before = GC.GetAllocatedBytesForCurrentThread();

				ExpressionParser.TryParse(texts[i]);

				fewest = Math.Min(fewest, GC.GetAllocatedBytesForCurrentThread() - before);
			}

			each[i] = fewest / (double)depths[i];
			TestContext.Current.TestOutputHelper?.WriteLine($"{shape}: {depths[i]} levels, {each[i]:F1} bytes each");
		}

		for (var i = 1; i < depths.Length; i++)
			Assert.True(
				each[i] <= each[i - 1] * 1.25,
				$"{shape}: at {depths[i - 1]} levels {each[i - 1]:F1} bytes each, at {depths[i]} {each[i]:F1} bytes each — " +
				$"{each[i] / each[i - 1]:F2} times as many, where a square doubles them.");
	}

	/// <summary>
	/// <paramref name="large"/> levels take at most twice as many times as long as <paramref name="small"/> as
	/// there are times the levels; the fewest of three readings of each.
	/// </summary>
	static void AssertLinearTime(string shape, int small, int large, Func<int, string> textFor)
	{
		var (few, many) = (textFor(small), textFor(large));

		Assert.False(ExpressionParser.TryParse(many).IsSuccess);

		var times = new[] { Fewest(few), Fewest(many) };

		TestContext.Current.TestOutputHelper?.WriteLine($"{shape}: {small} levels {times[0]:F1} ms, {large} levels {times[1]:F1} ms");
		Assert.True(
			times[1] <= times[0] * (large / small) * 2,
			$"{shape}: {small} levels took {times[0]:F1} ms and {large} took {times[1]:F1} ms — " +
			$"{times[1] / times[0]:F1} times as long for {large / small} times the levels.");

		static double Fewest(string text)
		{
			var fewest = double.MaxValue;

			for (var run = 0; run < 3; run++)
			{
				var clock = Stopwatch.StartNew();

				ExpressionParser.TryParse(text);

				fewest = Math.Min(fewest, clock.Elapsed.TotalMilliseconds);
			}

			return fewest;
		}
	}

	static string Repeated(string unit, int times)
	{
		return string.Concat(Enumerable.Repeat(unit, times));
	}
}
