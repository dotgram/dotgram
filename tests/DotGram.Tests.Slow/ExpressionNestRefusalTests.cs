using System;
using System.Diagnostics;
using System.Linq;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Two nests of the expression language that cost the square of their depth, read or refused, and
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
/// The one whose bytes are flat is held to time, against the same chain closed, which was always
/// linear: the two read as deep a nest on the same machine, so what is left between them is the asking.
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

	/// <summary>A conditional chain cut short takes about as long as the same chain closed, which was always linear.</summary>
	/// <remarks>
	/// Held against the closed chain of the same length rather than against itself at another length: the two
	/// read the same nest, as deep, on the same machine, so what a deep stack or a loaded machine costs is in
	/// both, and what is left is the asking. Cut, 12,800 levels took 1.9 s where closed they take tens of
	/// milliseconds; a run of the full suite on a loaded Windows host read a doubling of a cut chain alone at
	/// three times its size once, with the square gone.
	/// </remarks>
	[Fact]
	public void A_conditional_chain_cut_short_takes_about_as_long_as_one_closed()
	{
		const int levels = 12800;

		var cut    = "(bool b) => " + Repeated("b ? 1 : ", levels);
		var closed = cut + "1";

		Assert.False(ExpressionParser.TryParse(cut).IsSuccess);
		Assert.True(ExpressionParser.TryParse(closed).IsSuccess);

		var (fewestCut, fewestClosed) = (double.MaxValue, double.MaxValue);

		for (var run = 0; run < 5; run++)
		{
			var clock = Stopwatch.StartNew();

			ExpressionParser.TryParse(cut);

			fewestCut = Math.Min(fewestCut, clock.Elapsed.TotalMilliseconds);
			clock     = Stopwatch.StartNew();

			ExpressionParser.TryParse(closed);

			fewestClosed = Math.Min(fewestClosed, clock.Elapsed.TotalMilliseconds);
		}

		TestContext.Current.TestOutputHelper?.WriteLine($"{levels} levels: cut {fewestCut:F1} ms, closed {fewestClosed:F1} ms");
		Assert.True(
			fewestCut <= fewestClosed * 6,
			$"A conditional chain of {levels} levels cut short took {fewestCut:F1} ms and closed {fewestClosed:F1} ms — " +
			$"{fewestCut / fewestClosed:F1} times as long.");
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

	static string Repeated(string unit, int times)
	{
		return string.Concat(Enumerable.Repeat(unit, times));
	}
}
