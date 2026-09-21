using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The steps of the SQL:2023 refusal ladders (benchmarks/results/refused-linearity-2026-09-20: audit-shape-bin60.txt and the points-*.txt dumps), each on both sides of its step:
	/// CASE arms with no END between 1,391 and 1,738, joins then a bare JOIN between 891 and 1,113 (bytes) and 1,738 and 2,172 (time), predicates then an unclosed ( and then a dangling
	/// AND between 2,715 and 3,393. A pair reads them to say whether a change to what a pool keeps moves a step: flat per unit below it, a jump above. "control" is this process's own build.
	/// All the inputs are refused by every side; a side that accepts one is a fault of the row.
	/// </summary>
	static IEnumerable<Workload> PairedCliffs(PairedSide before, PairedSide after)
	{
		string Predicates(int n)
		{
			return string.Join(" AND ", Enumerable.Range(0, n).Select(static i => "a" + i + " = 1"));
		}

		var rows = new List<(string Name, string Method, string Text)>();

		foreach (var n in new[] { 1_391, 1_738 })
			rows.Add(($"refused-cliff-case-{n}", "TryParseQueryExpression", "SELECT CASE" + string.Concat(Enumerable.Repeat(" WHEN 1 = 1 THEN 1", n))));

		foreach (var n in new[] { 891, 1_113, 1_738, 2_172 })
			rows.Add(($"refused-cliff-joins-{n}", "TryParseQueryExpression", "SELECT * FROM t" + string.Concat(Enumerable.Repeat(" JOIN t ON a = a", n)) + " JOIN"));

		foreach (var n in new[] { 2_715, 3_393 })
		{
			rows.Add(($"refused-cliff-paren-{n}", "TryParseSearchCondition", Predicates(n) + " AND (a = 1"));
			rows.Add(($"refused-cliff-and-{n}", "TryParseSearchCondition", Predicates(n) + " AND"));
		}

		foreach (var (name, method, text) in rows)
		{
			var own = method == "TryParseQueryExpression"
				? (Func<int>)(() => SqlStandardParser.TryParseQueryExpression(text).IsSuccess ? 1 : 0)
				: () => SqlStandardParser.TryParseSearchCondition(text).IsSuccess ? 1 : 0;
			var b = before.Sql(method, text);
			var a = after.Sql(method, text);

			yield return new Workload("sql", name,
				[
					new Reading("control", own),
					new Reading("before", b),
					new Reading("after",  a),
				],
				() => own() == 0 && b() == 0 && a() == 0 ? null : $"  the input is refused by every side, and control reads {own()}, before {b()}, after {a()}");
		}
	}
}
