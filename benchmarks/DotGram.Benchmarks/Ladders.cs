using System;
using System.Diagnostics;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// One arithmetic language written twice — a rule a level, and one rule with binding
/// powers — over the same input.
/// </summary>
/// <remarks>
/// <para>
/// Nesting is where the reading over kinds is furthest from the hand-written parser:
/// `--slope` puts `((((a + 1) * 2) - 3) / 4) > 0` at 1.59 of it where a list of eight is
/// at 0.97. `HandSqlTokens` says why in its own first line — it climbs a precedence where
/// `SqlStandard92.gram` descends a ladder, and a bracket that re-enters the ladder pays
/// every level of it whether anything at that level is there or not.
/// </para>
/// <para>
/// §4.3.1 already offers the other shape, so the question is what it is worth before any
/// grammar is rewritten to use it. Both readings here build the same tree from the same
/// input; what differs is how the precedence is written.
/// </para>
/// </remarks>
[Gram("""
	trivia = ' '*

	Sum     : @long = l: Sum     & '+' & r: Product => @(l + r)
	                | l: Sum     & '-' & r: Product => @(l - r)
	                | one: Product                  => @(one)

	Product : @long = l: Product & '*' & r: Unary   => @(l * r)
	                | l: Product & '/' & r: Unary   => @(l / r)
	                | one: Unary                    => @(one)

	Unary   : @long = '-' & one: Unary              => @(-one)
	                | one: Primary                  => @(one)

	Primary : @long = t: ['0'..'9']+                => @(long.Parse(t))
	                | '(' & inner: Sum & ')'        => @(inner)

	parse Sum as Levelled
	""")]
public static partial class Levels
{
}

/// <summary>The same language as one rule, each alternative stating its own strength.</summary>
[Gram("""
	trivia = ' '*

	Expr : @long = l: Expr & '+' & r: Expr  << 1 => @(l + r)
	             | l: Expr & '-' & r: Expr  << 1 => @(l - r)
	             | l: Expr & '*' & r: Expr  << 2 => @(l * r)
	             | l: Expr & '/' & r: Expr  << 2 => @(l / r)
	             | '-' & one: Expr          >> 3 => @(-one)
	             | '(' & inner: Expr & ')'       => @(inner)
	             | t: ['0'..'9']+                => @(long.Parse(t))

	parse Expr as Climbed
	""")]
public static partial class Climbing
{
}

/// <summary>What each costs on the same inputs, at two depths of bracket.</summary>
static class Ladders
{
	public static void Run(int rounds)
	{
		string[] shapes =
		[
			"1 + 2 * 3 - 4 / 5",
			"(1 + 2) * (3 - 4)",
			"((((1 + 2) * 3) - 4) / 5) + 6",
			"((((((((1 + 2) * 3) - 4) / 5) + 6) * 7) - 8) / 9) + 10",
		];

		foreach (var one in shapes)
		{
			for (var i = 0; i < 300; i++)
			{
				Levels.TryLevelled(one);
				Climbing.TryClimbed(one);
			}

			var levelled = Levels.TryLevelled(one);
			var climbed  = Climbing.TryClimbed(one);

			if (!levelled.IsSuccess || !climbed.IsSuccess || levelled.Value != climbed.Value)
				Console.WriteLine($"!! \"{one}\": {levelled.Value} against {climbed.Value}");
		}

		Console.WriteLine($"{"input",-56} {"levels",9} {"climbing",9}   climb/levels");

		foreach (var one in shapes)
		{
			var levelled = Median(rounds, () => Levels.TryLevelled(one));
			var climbed  = Median(rounds, () => Climbing.TryClimbed(one));

			Console.WriteLine(
				$"{one,-56} {levelled * 1000,7:F3} us {climbed * 1000,7:F3} us   {climbed / levelled,6:F2}x");
		}
	}

	/// <summary>What one parse takes, timed a thousand at a time.</summary>
	/// <remarks>
	/// A parse here is a few hundred nanoseconds and the stopwatch counts in hundreds of
	/// them, so timing one at a time reads 0.300, 0.400, 0.900 and says nothing. A batch
	/// is long enough to measure and the median of the batches is still a median.
	/// </remarks>
	const int Batch = 1000;

	static double Median(int rounds, Action once)
	{
		var taken = new double[rounds];
		var watch = new Stopwatch();

		for (var i = 0; i < rounds; i++)
		{
			watch.Restart();

			for (var turn = 0; turn < Batch; turn++)
				once();

			taken[i] = watch.Elapsed.TotalMilliseconds / Batch;
		}

		Array.Sort(taken);

		return taken[rounds / 2];
	}
}
