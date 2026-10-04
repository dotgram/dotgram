using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The nests of brackets that read the same text again and again, counted in rule entries with the
/// memo of failures: each costs its depth, but for one, held to the square it is known to cost.
/// </summary>
/// <remarks>
/// <para>
/// <b>What the memo is for.</b> A bracket is tried as one thing after another, and each attempt that
/// fails reads everything inside it first. Without the memo T-SQL's nest of conditions cost the
/// cube of its depth — 87,922,456 rule entries refused at 400 levels, 163 ms — and SQL:2023's
/// unclosed nests the square: almost every entry was a rule entered at a token where it had
/// already failed. With it, each is read once at each place (<c>Machine.Memo.cs</c>): 16,100 entries
/// where there were 87.9 million.
/// </para>
/// <para>
/// <b>What it leaves.</b> A success is read again. <c>((a)) =</c>, refused, reads the value inside
/// every bracket once as a value and again under each bracket around it: the square of its depth,
/// in successful readings, which a memo of failures does not touch. Held to that class here, so that
/// it cannot get worse unseen; remembering successes is what would make it linear.
/// </para>
/// <para>
/// <b>Counted, not timed</b> (D144), as <c>BracketNestingCountTests</c> counts: every rule the reader
/// reads by a method of its own counts its entries where the compilation defines
/// <c>DOTGRAM_COUNTS</c>, and an entry the memo answers counts as one. Four depths a doubling apart;
/// what each doubling adds may grow by at most the factor of the nest's class, with a tenth to spare
/// for the ends, and each depth is held under a bound a level, so that a constant creeping up is
/// seen too. Counted at 50, 100, 200 and 400 levels (25 to 200 for the two whose levels are wider):
/// </para>
/// <code>
///   T-SQL
///   a nest of conditions, accepted        1,050    2,050    4,050    8,050   n
///   a nest of conditions, never closed    2,122    4,122    8,122   16,122   n
///   values nested in a predicate            178      328      628    1,228   n
///   values nested, the predicate cut     15,302   55,552  211,052  822,052   n^2
///   conditions around values (25..200)      825    1,600    3,150    6,250   n
///   a value nest never closed               829    1,629    3,229    6,429   n
///   subqueries in FROM (25..200)            826    1,601    3,151    6,251   n
///   SQL:2023
///   a value nest in SELECT, never closed  1,738    3,388    6,688   13,288   n
///   a nest of conditions, never closed    1,755    3,405    6,705   13,305   n
///   a bracketed sum, never closed         1,749    3,399    6,699   13,299   n
///   subqueries in FROM (25..200)            884    1,709    3,359    6,659   n
/// </code>
/// <para>
/// A refused nest costs more than it did before the reading that records tried an optional's or
/// a loop's turn behind a shut door, as the engine does, so that what refused the turn is said:
/// at every level of the nest that is a rule entered at the token where the parse stops. The
/// predicate cut read 9,896 to 498,846 before, and subqueries in FROM 578 to 4,428 — the same
/// class, a larger constant. A parse that is accepted is not read twice and does not pay it.
/// </para>
/// <para>
/// <b>Beside what.</b> The counters are static, and <c>DeepNestingCountTests</c> reads the same
/// variants; it is in <see cref="Alone"/>, which runs after this class and every other parallel one,
/// so none of its entries is counted here. Asked for by name rather than by type, that collection was
/// an ordinary one, and its readings landed in these counts: 4,185 entries at 100 levels where a
/// reading alone makes 3,246.
/// </para>
/// </remarks>
public sealed class FailureMemoCountTests(ITestOutputHelper output)
{
	/// <summary>How much more than its class a doubling may add, for what the ends of the text cost.</summary>
	const double Slack = 1.1;

	static readonly int[] Depths = [50, 100, 200, 400];

	static readonly int[] Wide = [25, 50, 100, 200];

	[Theory]
	[InlineData("a nest of conditions, accepted",       2, 22)]
	[InlineData("a nest of conditions, never closed",   2, 44)]
	[InlineData("values nested in a predicate",         2, 4)]
	[InlineData("values nested, the predicate cut",     4, 300)]
	[InlineData("conditions around values",             2, 34)]
	[InlineData("a value nest never closed",            2, 17)]
	[InlineData("subqueries in FROM",                   2, 32)]
	public void A_TSql_nest_costs_no_more_than_its_class(string shape, int growth, int perLevel)
	{
		var parser = SqlVariants.Parser(SqlVariants.TransactSql, counts: true);

		var (publication, accepted, depths, text) = shape switch
		{
			"a nest of conditions, accepted"     => ("TryParseSearchCondition", true, Depths, (Func<int, string>)(n => Open(n) + "a = 1" + Close(n))),
			"a nest of conditions, never closed" => ("TryParseSearchCondition", false, Depths, n => Open(n) + "a = 1"),
			"values nested in a predicate"       => ("TryParseSearchCondition", true, Depths, n => Open(n) + "a" + Close(n) + " = 1"),
			"values nested, the predicate cut"   => ("TryParseSearchCondition", false, Depths, n => Open(n) + "a" + Close(n) + " ="),
			"conditions around values"           => ("TryParseSearchCondition", true, Wide, n => Open(n) + Open(n) + "a" + Close(n) + " = 1" + Close(n)),
			"a value nest never closed"          => ("TryParseStatement", false, Depths, n => "SELECT " + Open(n) + "1"),
			"subqueries in FROM"                 => ("TryParseStatement", false, Wide, n => "SELECT * FROM " + Repeated("(SELECT * FROM ", n) + "t"),
			_                                    => throw new ArgumentOutOfRangeException(nameof(shape)),
		};

		Holds(parser, publication, shape, accepted, depths, text, growth, perLevel);
	}

	[Theory]
	[InlineData("a value nest in SELECT, never closed", 35)]
	[InlineData("a nest of conditions, never closed",   35)]
	[InlineData("a bracketed sum, never closed",        35)]
	[InlineData("subqueries in FROM",                   35)]
	public void A_SQL_2023_nest_costs_its_depth(string shape, int perLevel)
	{
		var parser = SqlVariants.Parser(SqlVariants.Standard, counts: true);

		var (publication, depths, text) = shape switch
		{
			"a value nest in SELECT, never closed" => ("TryParseQueryExpression", Depths, (Func<int, string>)(n => "SELECT " + Open(n) + "a")),
			"a nest of conditions, never closed"   => ("TryParseSearchCondition", Depths, n => Open(n) + "a = 1"),
			"a bracketed sum, never closed"        => ("TryParseValueExpression", Depths, n => Open(n) + "a + 1"),
			"subqueries in FROM"                   => ("TryParseQueryExpression", Wide, n => "SELECT * FROM " + Repeated("(SELECT * FROM ", n) + "t"),
			_                                      => throw new ArgumentOutOfRangeException(nameof(shape)),
		};

		Holds(parser, publication, shape, accepted: false, depths, text, growth: 2, perLevel);
	}

	void Holds(Type parser, string publication, string shape, bool accepted, int[] depths, Func<int, string> text, int growth, int perLevel)
	{
		var counts = new List<long>();

		foreach (var depth in depths)
		{
			counts.Add(Count(parser, publication, text(depth), accepted));
			output.WriteLine($"{shape}: {counts[^1]:N0} rule entries at {depth} levels");

			// Or the counters are not being written and everything below passes on nothing.
			Assert.True(counts[0] > 0, $"{shape}: nothing was counted at {depths[0]} levels.");

			Assert.True(
				counts[^1] <= (long)perLevel * depth * (growth == 2 ? 1 : depth / depths[0]) + 500,
				$"{shape}: {counts[^1]:N0} rule entries at {depth} levels, over the bound of {perLevel} a level" +
				(growth == 2 ? "." : $" times the depth over {depths[0]}."));

			if (counts.Count < 3)
				continue;

			var earlier = counts[^2] - counts[^3];
			var later   = counts[^1] - counts[^2];

			Assert.True(
				earlier > 0 && later <= earlier * growth * Slack,
				$"{shape}: {string.Join(", ", counts)} rule entries at {string.Join(", ", depths.Take(counts.Count))} " +
				$"levels, so a doubling added {later} where the one before added {earlier}, " +
				$"{(double)later / earlier:F2} times as much against {growth} for the class this nest is held to.");
		}
	}

	static string Open(int n)
	{
		return new string('(', n);
	}

	static string Close(int n)
	{
		return new string(')', n);
	}

	static string Repeated(string text, int n)
	{
		return string.Concat(Enumerable.Repeat(text, n));
	}

	/// <summary>Rule entries of one reading by a counted parser's publication, from a fresh zero.</summary>
	static long Count(Type parser, string publication, string text, bool accepted)
	{
		var counters = parser.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
			.SelectMany(static type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
			.Where(static field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal))
			.ToList();

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var read = parser.GetMethod(publication, BindingFlags.Public | BindingFlags.Static, binder: null, [typeof(string)], modifiers: null)!;

		object?    match = null;
		Exception? error = null;

		// On a stack with room for the nest, so that no reading is carried onto another thread.
		var thread = new Thread(() =>
		{
			try { match = read.Invoke(null, [text]); }
			catch (Exception e) { error = e; }
		}, 256 * 1024 * 1024);

		thread.Start();
		thread.Join();

		if (error is not null)
			throw new InvalidOperationException("the counted reading threw", error);

		Assert.Equal(accepted, (bool)match!.GetType().GetProperty("IsSuccess")!.GetValue(match)!);

		return counters.Sum(static counter => (long)counter.GetValue(null)!);
	}
}
