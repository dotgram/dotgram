using System;
using System.Collections.Generic;
using System.IO;

using DotGram.Sql.TransactSql;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

// T-SQL against Microsoft's ScriptDom, as rows of the stand (architect for Igor, 2026-09-18).
//
// ScriptDom is the yardstick here, a third party, and not a hand-written parser of this
// repository's: it is the first reading of each row, so it is the base every ratio is taken
// against, and the report groups these rows under it. Both sides are asked for the whole
// tree of one statement. The generated reading is asked twice, without positions and with
// them ("located"), because ScriptDom always carries where each part was and so "located"
// is the one to hold against it.
//
// Agreement is that both accept the statement. Whether the two trees say the same thing is
// what `--roundtrip` and `--kinds` are for, over the corpus, and not something a stand row
// can do.
//
// The statements are of different shapes on purpose: a join, an insert of rows, a table, an
// update with a subquery and one long select. The long one is written in the shape of the
// corpus's long selects (a common table expression, four joins, a CASE, a correlated
// subquery, GROUP BY, HAVING and ORDER BY) and not cut from a file of the corpus: a row is
// rebuilt in every fresh process a first call is taken in, and scanning eleven hundred files
// for one statement each time would cost more than the call.

static partial class Stand
{
	static readonly Lazy<TSql170Parser> ScriptDomParser = new(static () => new TSql170Parser(true));

	static IEnumerable<Workload> TsqlWorkloads()
	{
		return
		[
			Tsql("select-join",
				"SELECT c.CustomerId, c.Name, o.OrderId, o.Total FROM dbo.Customers AS c INNER JOIN dbo.Orders AS o ON o.CustomerId = c.CustomerId WHERE o.Total > 100 AND c.Region = 'EU' ORDER BY o.Total DESC"),
			Tsql("insert-values",
				"INSERT INTO dbo.Orders (OrderId, CustomerId, Total, Placed) VALUES (1, 42, 99.50, '2026-09-18'), (2, 43, 10, '2026-09-19'), (3, 44, 0, NULL)"),
			Tsql("create-table",
				"CREATE TABLE dbo.Orders (OrderId INT NOT NULL PRIMARY KEY, CustomerId INT NOT NULL REFERENCES dbo.Customers (CustomerId), Total DECIMAL(18, 2) NOT NULL DEFAULT 0, Placed DATETIME2 NULL, Note NVARCHAR(200) NULL)"),
			Tsql("update-subquery",
				"UPDATE o SET o.Total = (SELECT SUM(l.Price * l.Quantity) FROM dbo.Lines AS l WHERE l.OrderId = o.OrderId) FROM dbo.Orders AS o WHERE o.Placed >= '2026-01-01'"),
			Tsql("select-long",
				"WITH recent AS (SELECT o.OrderId, o.CustomerId, o.Placed FROM dbo.Orders AS o WHERE o.Placed >= DATEADD(month, -3, GETDATE())) " +
				"SELECT c.CustomerId, c.Name, COUNT(*) AS Orders, SUM(l.Price * l.Quantity) AS Revenue, MAX(r.Placed) AS Last, " +
				"CASE WHEN SUM(l.Price * l.Quantity) > 10000 THEN 'gold' WHEN SUM(l.Price * l.Quantity) > 1000 THEN 'silver' ELSE 'bronze' END AS Tier " +
				"FROM dbo.Customers AS c JOIN recent AS r ON r.CustomerId = c.CustomerId LEFT JOIN dbo.Lines AS l ON l.OrderId = r.OrderId " +
				"LEFT JOIN dbo.Regions AS g ON g.RegionId = c.RegionId " +
				"WHERE c.Active = 1 AND (g.Name IN ('EU', 'US', 'APAC') OR c.Vip = 1) " +
				"AND NOT EXISTS (SELECT 1 FROM dbo.Blocked AS b WHERE b.CustomerId = c.CustomerId) " +
				"GROUP BY c.CustomerId, c.Name HAVING COUNT(*) > 1 ORDER BY Revenue DESC, c.Name"),
		];
	}

	static Workload Tsql(string name, string text)
	{
		return new Workload(
			"tsql",
			name,
			[
				new Reading("scriptdom",  () => ScriptDomAccepts(text) ? 1 : 0),
				new Reading("generated",  () => TransactSqlParser.TryParseStatement(text).IsSuccess ? 1 : 0),
				new Reading("located",    () => TransactSqlParser.Located.TryParseStatement(text).IsSuccess ? 1 : 0),
			],
			() =>
			{
				var reference = ScriptDomAccepts(text);
				var plain     = TransactSqlParser.TryParseStatement(text).IsSuccess;
				var located   = TransactSqlParser.Located.TryParseStatement(text).IsSuccess;

				return reference && plain && located
					? null
					: $"  every reading must accept it: ScriptDom {(reference ? "accepts" : "refuses")}, generated {(plain ? "accepts" : "refuses")}, located {(located ? "accepts" : "refuses")}";
			});
	}

	static bool ScriptDomAccepts(string text)
	{
		using var reader = new StringReader(text);

		return ScriptDomParser.Value.Parse(reader, out var errors) is not null && errors.Count == 0;
	}
}
