using System;
using System.Linq;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refused SQL condition allocates what its prefix needs, not its square: SQL-92 and T-SQL, a chain
/// of predicates cut short and a nest of brackets never closed.
/// </summary>
/// <remarks>
/// <para>
/// Both parsers are carried immediately, so a refusal is no longer nearly free: the quiet reading
/// builds every node as its rule is read, and what it built is thrown away when the text is refused.
/// That costs once what accepting the prefix would have cost, which an accepted text of the same length
/// costs anyway. What it must not do is cost more for every element than the one before it — a reading
/// given back and read again from its start, a value built again for each shorter reading — which is
/// how a refused text becomes a way to make a server allocate the square of what it was sent.
/// </para>
/// <para>
/// <b>The gate is what each doubling adds, an element</b>, warmed at the top size and the fewest of
/// several runs kept: what the text pays once whatever its length cancels out of an increment, where it
/// would hide a slope in bytes an element of the totals. The bound is tolerant: a quarter more an element
/// at a doubling, where a square doubles it.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class SqlRefusalAllocationTests
{
	/// <summary>Predicates in a chain, doubling.</summary>
	static readonly int[] Chains = [128, 256, 512, 1024];

	/// <summary>Brackets in a nest, doubling.</summary>
	static readonly int[] Nests = [16, 32, 64, 128];

	/// <summary>How many runs a size is measured over; the smallest of them is kept.</summary>
	const int Runs = 5;

	/// <summary>How much more an element may allocate at one size than at the half of it.</summary>
	const double Tolerance = 1.25;

	[Fact]
	public void Every_text_here_is_refused_at_every_size()
	{
		foreach (var size in Chains)
		{
			Assert.False(Sql92Parser.TryParseSearchCondition(CutChain(size)).IsSuccess, $"SQL-92 read a cut chain of {size}.");
			Assert.False(Sql92Parser.TryParseSelect(Selected(CutChain(size))).IsSuccess, $"SQL-92 read a query with a cut chain of {size}.");
			Assert.False(TransactSqlParser.TryParseSearchCondition(CutChain(size)).IsSuccess, $"T-SQL read a cut chain of {size}.");
			Assert.False(TransactSqlParser.TryParseStatement(Selected(CutChain(size))).IsSuccess, $"T-SQL read a query with a cut chain of {size}.");
		}

		foreach (var size in Nests)
		{
			Assert.False(Sql92Parser.TryParseSearchCondition(OpenNest(size)).IsSuccess, $"SQL-92 read {size} brackets never closed.");
			Assert.False(Sql92Parser.TryParseSelect(Selected(StrayNest(size))).IsSuccess, $"SQL-92 read {size} brackets and a stray one.");
			Assert.False(TransactSqlParser.TryParseSearchCondition(OpenNest(size)).IsSuccess, $"T-SQL read {size} brackets never closed.");
			Assert.False(TransactSqlParser.TryParseStatement(Selected(StrayNest(size))).IsSuccess, $"T-SQL read {size} brackets and a stray one.");
		}
	}

	[Fact]
	public void A_cut_chain_allocates_about_as_many_bytes_a_predicate_at_every_size_in_SQL_92()
	{
		AssertLinear("SQL-92, condition", Chains, CutChain, static text => Sql92Parser.TryParseSearchCondition(text).IsSuccess);
		AssertLinear("SQL-92, query", Chains, size => Selected(CutChain(size)), static text => Sql92Parser.TryParseSelect(text).IsSuccess);
	}

	[Fact]
	public void A_cut_chain_allocates_about_as_many_bytes_a_predicate_at_every_size_in_T_SQL()
	{
		AssertLinear("T-SQL, condition", Chains, CutChain, static text => TransactSqlParser.TryParseSearchCondition(text).IsSuccess);
		AssertLinear("T-SQL, statement", Chains, size => Selected(CutChain(size)), static text => TransactSqlParser.TryParseStatement(text).IsSuccess);
	}

	[Fact]
	public void A_nest_of_brackets_refused_allocates_about_as_many_bytes_a_bracket_at_every_size_in_SQL_92()
	{
		AssertLinear("SQL-92, never closed", Nests, OpenNest, static text => Sql92Parser.TryParseSearchCondition(text).IsSuccess);
		AssertLinear("SQL-92, a stray bracket", Nests, size => Selected(StrayNest(size)), static text => Sql92Parser.TryParseSelect(text).IsSuccess);
	}

	[Fact]
	public void A_nest_of_brackets_refused_allocates_about_as_many_bytes_a_bracket_at_every_size_in_T_SQL()
	{
		AssertLinear("T-SQL, never closed", Nests, OpenNest, static text => TransactSqlParser.TryParseSearchCondition(text).IsSuccess);
		AssertLinear("T-SQL, a stray bracket", Nests, size => Selected(StrayNest(size)), static text => TransactSqlParser.TryParseStatement(text).IsSuccess);
	}

	/// <summary>
	/// A chain of set operators, accepted or cut short, allocates what its branches need. Each operator
	/// was the list of the ones before it copied with one more: 17 KB a branch at 2,048 branches and twice
	/// that at every doubling, 2.1 GB at 16,384 — accepted texts as much as refused ones.
	/// </summary>
	[Theory]
	[InlineData("UNION")]
	[InlineData("UNION ALL")]
	[InlineData("INTERSECT")]
	[InlineData("EXCEPT")]
	public void A_set_operator_chain_allocates_about_as_many_bytes_a_branch_at_every_size(string op)
	{
		AssertLinear($"SQL-92, {op}", Chains, size => Combined(op, size), static text => Sql92Parser.TryParseSelect(text).IsSuccess);
		AssertLinear($"SQL-92, {op}, cut", Chains, size => Combined(op, size) + " " + op + " SELECT", static text => Sql92Parser.TryParseSelect(text).IsSuccess);
		AssertLinear($"SQL-92, {op}, in a subquery", Chains, size => Selected("a IN (" + Combined(op, size) + ")"), static text => Sql92Parser.TryParseSelect(text).IsSuccess);
		AssertLinear($"T-SQL, {op}", Chains, size => Combined(op, size), static text => TransactSqlParser.TryParseStatement(text).IsSuccess);
		AssertLinear($"T-SQL, {op}, cut", Chains, size => Combined(op, size) + " " + op + " SELECT", static text => TransactSqlParser.TryParseStatement(text).IsSuccess);
		AssertLinear($"T-SQL, {op}, in a subquery", Chains, size => Selected("a IN (" + Combined(op, size) + ")"), static text => TransactSqlParser.TryParseStatement(text).IsSuccess);
	}

	[Fact]
	public void A_set_operator_chain_still_reads_as_it_did()
	{
		Assert.True(Sql92Parser.TryParseSelect(Combined("UNION", 1024)).IsSuccess);
		Assert.True(TransactSqlParser.TryParseStatement(Combined("UNION ALL", 1024)).IsSuccess);
		Assert.False(Sql92Parser.TryParseSelect(Combined("EXCEPT", 1024) + " EXCEPT SELECT").IsSuccess);
		Assert.False(TransactSqlParser.TryParseStatement(Combined("INTERSECT", 1024) + " INTERSECT SELECT").IsSuccess);
	}

	/// <summary><paramref name="branches"/> queries joined by <paramref name="op"/>.</summary>
	static string Combined(string op, int branches)
	{
		return string.Join(" " + op + " ", Enumerable.Range(0, branches).Select(static i => "SELECT a" + i + " FROM t"));
	}

	/// <summary>
	/// Asserts bytes an element grow by no more than <see cref="Tolerance"/> at every doubling in
	/// <paramref name="sizes"/>: a square doubles them at each of the three steps.
	/// </summary>
	static void AssertLinear(string shape, int[] sizes, Func<int, string> textFor, Func<string, bool> parse)
	{
		var texts = sizes.Select(textFor).ToArray();

		// The largest size, run once and discarded, before any size below it is measured: whatever
		// growing the parser's thread-static pools to fit it costs is paid here.
		parse(texts[^1]);

		var bytes = new long[sizes.Length];

		for (var i = 0; i < sizes.Length; i++)
		{
			bytes[i] = Allocated(texts[i], parse);
			TestContext.Current.TestOutputHelper?.WriteLine($"{shape}: {sizes[i]} elements, {bytes[i] / (double)sizes[i]:F1} bytes each");
		}

		// What each step to the next size adds, an element: a cost the text pays once whatever its
		// length cancels out of it, where in bytes an element of the totals it would hide a slope.
		// Held to a floor of a few bytes, since a nest that builds nothing adds next to nothing.
		var added = new double[sizes.Length];

		for (var i = 1; i < sizes.Length; i++)
			added[i] = (bytes[i] - bytes[i - 1]) / (double)(sizes[i] - sizes[i - 1]);

		for (var i = 2; i < sizes.Length; i++)
			Assert.True(
				added[i] <= Math.Max(added[i - 1], 8) * Tolerance,
				$"{shape}: from {sizes[i - 2]} to {sizes[i - 1]} elements a refusal allocated {added[i - 1]:F1} bytes more an " +
				$"element, and from {sizes[i - 1]} to {sizes[i]} it allocated {added[i]:F1} more — {added[i] / Math.Max(added[i - 1], 8):F2} " +
				"times as many, where a reading that builds its prefix once would stay flat.");
	}

	/// <summary>The fewest bytes this thread allocates refusing <paramref name="text"/>, across <see cref="Runs"/> runs.</summary>
	static long Allocated(string text, Func<string, bool> parse)
	{
		var fewest = long.MaxValue;

		for (var run = 0; run < Runs; run++)
		{
			var before = GC.GetAllocatedBytesForCurrentThread();

			parse(text);

			fewest = Math.Min(fewest, GC.GetAllocatedBytesForCurrentThread() - before);
		}

		return fewest;
	}

	/// <summary><paramref name="predicates"/> predicates joined by AND, then an AND whose predicate stops at its <c>=</c>.</summary>
	static string CutChain(int predicates)
	{
		return string.Join(" AND ", Enumerable.Range(0, predicates).Select(static i => "a" + i + " = 1")) + " AND a" + predicates + " =";
	}

	/// <summary>A predicate inside <paramref name="depth"/> brackets, none of them closed.</summary>
	static string OpenNest(int depth)
	{
		return new string('(', depth) + "a = 1";
	}

	/// <summary>A predicate inside <paramref name="depth"/> brackets, all closed, and one more closing bracket.</summary>
	static string StrayNest(int depth)
	{
		return new string('(', depth) + "a = 1" + new string(')', depth + 1);
	}

	static string Selected(string condition)
	{
		return "SELECT a FROM t WHERE " + condition;
	}
}
