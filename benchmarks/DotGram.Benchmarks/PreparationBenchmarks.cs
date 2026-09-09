using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// What a parse costs before it has read anything, on five grammars and two sizes each.
/// </summary>
/// <remarks>
/// <para>
/// Every other comparison here times a parse over an input worth parsing, which answers
/// what the engine costs per character and hides what it costs per call. On a grammar the
/// size of <c>TransactSql.gram</c> that hardly matters — the statement is long and the
/// fixed part is lost in it. On a small one it is the question: a parser called once per
/// line of a log or once per cell of a spreadsheet pays whatever is fixed on every one of
/// them, and a total cannot say whether that is a tenth of the work or half.
/// </para>
/// <para>
/// So each grammar is read twice: <c>least</c> is the shortest input its own rules accept,
/// which is the fixed cost of a call and a hair, and <c>real</c> is something somebody would
/// hand it. The interesting number is the share between them, and it is a share rather than
/// a difference because a difference is a fact about one input.
/// </para>
/// <para>
/// <c>Config</c> is the cleanest reading of it: seven rules over four characters and over
/// four hundred entries, so nothing but the input has changed between the two rows.
/// </para>
/// <para>
/// The first call of all is <see cref="Preparation"/>'s question, and it cannot be asked
/// here — warming is exactly what a benchmark is for and exactly what that measures.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class PreparationBenchmarks
{
	/// <summary>A grammar, a size, and the reading of it.</summary>
	public sealed record Case(string Name, string Size, string Input, Func<string, bool> Read)
	{
		public override string ToString() => $"{Name} {Size}";
	}

	public static IEnumerable<Case> Cases()
	{
		yield return new("Levels", "least", "1", static text => Levels.TryLevelled(text).IsSuccess);
		yield return new("Levels", "real", Sum(80), static text => Levels.TryLevelled(text).IsSuccess);

		yield return new("Url", "least", "http://a", static text => Urls.TryParseUrl(text).IsSuccess);
		yield return new("Url", "real", "https://user@example.com:8080/a/b/c?q=1&r=2#top",
			static text => Urls.TryParseUrl(text).IsSuccess);

		yield return new("Config", "least", "a=b;", static text => Config.Read(text).Length >= 0);
		yield return new("Config", "real", Settings(400), static text => Config.Read(text).Length >= 0);

		yield return new("Sql-92", "least", "a > 1",
			static text => SqlStandard92.TryParseSearchCondition(text).IsSuccess);
		yield return new("Sql-92", "real", Condition(),
			static text => SqlStandard92.TryParseSearchCondition(text).IsSuccess);

		yield return new("TransactSql", "least", "SELECT 1",
			static text => TransactSql.TryParseStatement(text).IsSuccess);
		yield return new("TransactSql", "real", Sql(),
			static text => TransactSql.TryParseStatement(text).IsSuccess);
	}

	[ParamsSource(nameof(Cases))]
	public Case Read { get; set; } = null!;

	[Benchmark]
	public bool Parse() => Read.Read(Read.Input);

	static string Sum(int terms) => string.Join(" + ", Enumerable.Range(1, terms));

	static string Settings(int entries) =>
		string.Concat(Enumerable.Range(0, entries).Select(
			static i => "key" + i + " = value" + i + ";" + Environment.NewLine));

	static string Condition() =>
		"a.c1 > 10 AND b.c2 IN (1, 2, 3) AND (c.c3 = 'x' OR c.c4 IS NOT NULL) " +
		"AND d.c5 BETWEEN 1 AND 9";

	static string Sql() =>
		"SELECT c1, c2, SUM(c3) AS total FROM dbo.t1 AS a " +
		"INNER JOIN dbo.t2 AS b ON a.id = b.id " +
		"WHERE a.c1 > 10 AND b.c2 IN (1, 2, 3) " +
		"GROUP BY c1, c2 HAVING SUM(c3) > 100 ORDER BY total DESC";
}
