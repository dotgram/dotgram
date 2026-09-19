using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Examples.Feeds;
using DotGram.Finance.Fix;
using DotGram.Handwritten.Fix;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

namespace DotGram.Benchmarks;

// The forms of the published API that had no row (the architect's order, 2026-09-19, after docs/design/stand-coverage-2026-09-19.md):
// they are paired rows, so that a change to how they are read is measured beside the form, and a baseline of them is a pair of one
// side against itself. The hand-written parser, where there is one, or this process's own generated parser is the constant.

static partial class Stand
{
	/// <summary>A parser's token scanner: where the token at <paramref name="pos"/> ends, and its kind.</summary>
	delegate int ScanFunction(ReadOnlySpan<char> text, int pos, out int kind);

	static int ScanCount(ScanFunction scan, string text)
	{
		var pos   = 0;
		var count = 0;

		while (pos < text.Length)
		{
			var end = scan(text, pos, out _);

			if (end > pos)
			{
				count++;
				pos = end;
			}
			else
				pos++;
		}

		return count;
	}

	static IEnumerable<Workload> PairedForms(PairedSide before, PairedSide after)
	{
		// ── the positional and the window forms ──
		var select20  = "SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(i => "a" + i)) + " FROM t WHERE a0 = 1";
		var prefix    = "SELECT 1; ";
		var positions = prefix + select20;
		var window    = prefix + select20 + "; SELECT 2";

		yield return PairedFormRow("sql", "select20.at", () => HandAccepts("TryParseQueryExpression", select20) ? 1 : 0,
			before.SqlPositional(false, "TryParseQueryExpression", positions, prefix.Length, null),
			after.SqlPositional(false, "TryParseQueryExpression", positions, prefix.Length, null));

		yield return PairedFormRow("sql", "select20.window", () => HandAccepts("TryParseQueryExpression", select20) ? 1 : 0,
			before.SqlPositional(false, "TryParseQueryExpression", window, prefix.Length, select20.Length),
			after.SqlPositional(false, "TryParseQueryExpression", window, prefix.Length, select20.Length));

		var insert = "INSERT INTO t (a, b) VALUES (1, 2), (3, 4), (5, 6)";

		yield return PairedFormRow("tsql", "insert-values.at", () => ScriptDomAccepts(insert) ? 1 : 0,
			before.SqlPositional(true, "TryParseStatement", prefix + insert, prefix.Length, null),
			after.SqlPositional(true, "TryParseStatement", prefix + insert, prefix.Length, null));

		// ── the token scanner ──
		var conditions = SqlConditions(100);
		var ladder     = "(int x, int y) => (x + y) * 3 - x / 5";

		yield return PairedFormRow("sql", "select20.scan", () => ScanCount(SqlStandardParser.Scan, select20), before.Scan("sql", select20), after.Scan("sql", select20));
		yield return PairedFormRow("sql", "conditions100.scan", () => ScanCount(SqlStandardParser.Scan, conditions), before.Scan("sql", conditions), after.Scan("sql", conditions));
		yield return PairedFormRow("tsql", "select20.scan", () => ScanCount(TransactSqlParser.Scan, select20), before.Scan("tsql", select20), after.Scan("tsql", select20));
		yield return PairedFormRow("el", "ladder.scan", () => ScanCount(DotGram.ExpressionLanguage.ExpressionParser.Scan, ladder), before.Scan("el", ladder), after.Scan("el", ladder));

		// ── FixMessages: the streams, the readers, the lazy reading and a span ──
		var wire = FixMessageWire();

		foreach (var form in new[] { "parse-stream", "parse-reader", "parse-span" })
			yield return PairedFormRow("fixmsg", "Order." + form, () => FixMessages.Parse(wire, default(FixParseMode)) is null ? 0 : 1, before.FixMessagesForm(form, wire, 1), after.FixMessagesForm(form, wire, 1));

		var many = string.Concat(Enumerable.Repeat(wire, 100));

		foreach (var form in new[] { "read-stream", "read-reader" })
			yield return PairedFormRow("fixmsg", "Order." + form + "100", () => FixMessages.ReadMessages(new StringReader(many), default(FixParseMode), 4096).Count(), before.FixMessagesForm(form, wire, 100), after.FixMessagesForm(form, wire, 100));

		// ── one span row for FIX ──
		var order = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";

		yield return PairedFormRow("fix", "Order.span", () => HandFixParser.Parse(order).Count(), before.FixSpan(order), after.FixSpan(order));

		// ── the lazy feed of the examples ──
		if (before.HasStock && after.HasStock)
		{
			var feed = "H|2026-08-13|ACME" + (char)10 + string.Concat(Enumerable.Repeat("R|AAPL|100|2026-08-12" + (char)10, 1000)) + "T|1000" + (char)10;

			yield return PairedFormRow("feeds", "streaming.1000", () => StreamingFeedReader.Read(new StringReader(feed)).Count(), before.StreamingFeed(feed), after.StreamingFeed(feed));
		}
	}

	/// <summary>A row of a published form: the constant, and the two sides, each asked to give the same answer.</summary>
	static Workload PairedFormRow(string family, string name, Func<int> hand, Func<int> before, Func<int> after) =>
		new(family, name,
			[new Reading("hand", hand), new Reading("before", before), new Reading("after", after)],
			() =>
			{
				var h = hand();
				var b = before();
				var a = after();

				return h == b && b == a ? null : $"  {family}/{name}: the constant says {h}, before {b}, after {a}";
			});
}
