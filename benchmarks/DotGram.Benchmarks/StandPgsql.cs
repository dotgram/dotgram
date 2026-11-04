using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.PostgreSql;

namespace DotGram.Benchmarks;

// PostgreSQL as rows of the stand: directional numbers for the grammar's own readings, taken
// from the first slice that reads a whole query, so that every later slice is measured against
// something. There is no third party to hold them against here, as ScriptDom holds the T-SQL
// rows: the generated reading without positions is the base, and the located one is read
// against it. The statements are PostgreSQL's own shapes — `::`, LIMIT, DISTINCT ON, a dollar
// quote — in the sizes the regression suite writes: a short join, one long select, and an
// expression alone through the expression entry.
static partial class Stand
{
	/// <summary>The statements, by name; a row named as a refusal is one the stand expects refused (Unasserted).</summary>
	static readonly (string Name, string Text)[] PgsqlRows =
	[
		("select-join",
			"SELECT c.customer_id, c.name, o.order_id, o.total::numeric(18, 2) FROM customers AS c JOIN orders AS o ON o.customer_id = c.customer_id WHERE o.total > 100 AND c.region = 'EU' ORDER BY o.total DESC LIMIT 50"),
		("select-long",
			"WITH recent AS (SELECT o.order_id, o.customer_id, o.placed FROM orders AS o WHERE o.placed >= now() - interval '3 months') " +
			"SELECT DISTINCT ON (c.customer_id) c.customer_id, c.name, count(*) AS orders, sum(l.price * l.quantity) AS revenue, max(r.placed) AS last, " +
			"CASE WHEN sum(l.price * l.quantity) > 10000 THEN 'gold' WHEN sum(l.price * l.quantity) > 1000 THEN 'silver' ELSE 'bronze' END AS tier " +
			"FROM customers AS c JOIN recent AS r ON r.customer_id = c.customer_id LEFT JOIN lines AS l ON l.order_id = r.order_id " +
			"LEFT JOIN regions AS g ON g.region_id = c.region_id " +
			"WHERE c.active AND (g.name = ANY (ARRAY['EU', 'US', 'APAC']) OR c.vip) " +
			"AND NOT EXISTS (SELECT 1 FROM blocked AS b WHERE b.customer_id = c.customer_id) " +
			"GROUP BY c.customer_id, c.name HAVING count(*) > 1 ORDER BY c.customer_id, revenue DESC NULLS LAST FETCH FIRST 100 ROWS ONLY"),
		("insert-conflict",
			"INSERT INTO orders AS o (order_id, customer_id, total, placed) VALUES (1, 42, 99.50, '2026-09-18'), (2, 43, 10, now()) " +
			"ON CONFLICT (order_id) DO UPDATE SET total = EXCLUDED.total, placed = EXCLUDED.placed WHERE o.total <> EXCLUDED.total RETURNING o.order_id, o.total"),
		("refused-late", "SELECT a, b, c FROM t WHERE a = 1 AND b = 2 AND c ="),
	];

	/// <summary>An expression alone, through the expression entry: the ladder and a typed literal, in the shape of a WHERE.</summary>
	const string PgsqlExpression = "a + b * c - d / 2 > 10 AND s ~ '^x' AND t::timestamptz < timestamp '2026-01-01' + interval '1 day' AND j->>'k' IS NOT NULL AND x = ANY (ARRAY[1, 2, 3])";

	static IEnumerable<Workload> PgsqlWorkloads()
	{
		foreach (var row in PgsqlRows)
			yield return Pgsql(row.Name, row.Text);

		yield return new Workload(
			"pgsql",
			"expression",
			[
				new Reading("generated", static () => PostgreSqlParser.TryParseExpression(PgsqlExpression).IsSuccess ? 1 : 0),
				new Reading("located",   static () => PostgreSqlParser.Located.TryParseExpression(PgsqlExpression).IsSuccess ? 1 : 0),
			],
			static () => PostgreSqlParser.TryParseExpression(PgsqlExpression).IsSuccess && PostgreSqlParser.Located.TryParseExpression(PgsqlExpression).IsSuccess
				? null
				: "  both readings must accept the expression");
	}

	static Workload Pgsql(string name, string text)
	{
		return new Workload(
			"pgsql",
			name,
			[
				new Reading("generated", () => PostgreSqlParser.TryParseStatement(text).IsSuccess ? 1 : 0),
				new Reading("located",   () => PostgreSqlParser.Located.TryParseStatement(text).IsSuccess ? 1 : 0),
			],
			() =>
			{
				var plain    = PostgreSqlParser.TryParseStatement(text).IsSuccess;
				var located  = PostgreSqlParser.Located.TryParseStatement(text).IsSuccess;
				var expected = !name.Contains("refused", StringComparison.Ordinal);

				return plain == expected && located == expected
					? null
					: $"  every reading must {(expected ? "accept" : "refuse")} it: generated {(plain ? "accepts" : "refuses")}, located {(located ? "accepts" : "refuses")}";
			});
	}
}
