using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Handwritten.Fix;
using DotGram.Handwritten.Web;
using DotGram.Web;

namespace DotGram.Benchmarks;

// The size-sweep rows of the paired stand (architect for Igor, 2026-09-19): 584a7c1f made the expression
// language's tape quadratic in the terms of a list and no pair saw it, because no row of the stand was longer
// than twenty terms. So the largest size of each series of the linearity family is a row of the paired stand
// too, beside the small rows the family already has: a change that costs more at ten times the size than at
// one tenth of it shows in the same run that asks about the change.

static partial class Stand
{
	const string SweepOrder = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";

	static IEnumerable<Workload> PairedSweeps(PairedSide before, PairedSide after)
	{
		var orders = string.Concat(Enumerable.Repeat(SweepOrder, 400));
		var fields = FixSlopeText(1600);

		yield return PairedFixForm("orders400.text", () => HandFixParser.Parse(orders), before.FixText(orders), after.FixText(orders));
		yield return PairedFixForm("slope-1600.text", () => HandFixParser.Parse(fields), before.FixText(fields), after.FixText(fields));

		var columns    = "SELECT " + string.Join(", ", Enumerable.Range(0, 1000).Select(static i => "c" + i)) + " FROM t";
		var conditions = "SELECT 1 WHERE " + string.Join(" AND ", Enumerable.Range(0, 1000).Select(static i => "a" + i + " = 1"));
		var rows       = "INSERT INTO t (a, b) VALUES " + string.Join(", ", Enumerable.Repeat("(1, 2)", 1000));

		// A script of statements read one at a time (expr, 2026-09-19): the positional form tokenizes the whole text on every call, so the loop
		// is a square today, and lazy tokens are to make it a line. ScriptDom reads the whole script once, as the constant.
		foreach (var statements in new[] { 100, 400 })
		{
			var script = string.Concat(Enumerable.Repeat("SELECT a, b FROM t WHERE a = 1;\n", statements));
			var b      = before.TsqlScript(script);
			var a      = after.TsqlScript(script);

			// The bool positional form on the after side against the match form on the before side, where the after side has one (expr).
			if (after.TsqlScriptBool(script) is { } quiet)
			{
				yield return new Workload("tsql", $"script{statements}.bool",
					[
						new Reading("hand",   () => ScriptDomAccepts(script) ? statements : 0),
						new Reading("before", b),
						new Reading("after",  quiet),
					],
					() => b() == statements && quiet() == statements ? null : $"  the script has {statements} statements: the match form read {b()}, the bool form {quiet()}");
			}

			// The bool positional form on both sides, where both have it: what the same form costs before and after.
			if (before.TsqlScriptBool(script) is { } quietBefore && after.TsqlScriptBool(script) is { } quietAfter)
			{
				yield return new Workload("tsql", $"script{statements}.boolboth",
					[
						new Reading("hand",   () => ScriptDomAccepts(script) ? statements : 0),
						new Reading("before", quietBefore),
						new Reading("after",  quietAfter),
					],
					() => quietBefore() == statements && quietAfter() == statements ? null : $"  the script has {statements} statements: before read {quietBefore()}, after {quietAfter()}");
			}

			yield return new Workload("tsql", $"script{statements}",
				[
					new Reading("hand",   () => ScriptDomAccepts(script) ? statements : 0),
					new Reading("before", b),
					new Reading("after",  a),
				],
				() => b() == statements && a() == statements ? null : $"  the script has {statements} statements: before read {b()}, after {a()}");
		}

		yield return PairedTsql("columns1000", columns, before, after);
		yield return PairedTsql("conditions1000", conditions, before, after);
		yield return PairedTsql("rows1000", rows, before, after);

		if (!before.HasWeb || !after.HasWeb)
			yield break;

		var array  = "[" + string.Join(",", Enumerable.Range(0, 10000)) + "]";
		var obj    = "{" + string.Join(",", Enumerable.Range(0, 10000).Select(static i => $"\"k{i}\":{i}")) + "}";
		var path   = "https://example.com/" + string.Concat(Enumerable.Repeat("seg/", 1000));
		var media  = "text/html" + string.Concat(Enumerable.Range(0, 1000).Select(static i => $"; a{i}=1"));
		var list   = string.Join(", ", Enumerable.Range(0, 10000));

		yield return PairedWebSweep("json.array10000", () => HandJson.TryParse(array, out _, out _), before.WebJson(array), after.WebJson(array));
		yield return PairedWebSweep("json.object10000", () => HandJson.TryParse(obj, out _, out _), before.WebJson(obj), after.WebJson(obj));
		yield return PairedWebSweep("url.path1000", () => HandUrl.TryParseReference(path, out _, out _), before.WebUrl(path), after.WebUrl(path));
		yield return PairedWebSweep("media-type.params1000", () => MediaType.TryParse(media, out _), before.WebTry("MediaType", "TryParse", media), after.WebTry("MediaType", "TryParse", media));
		yield return PairedWebSweep("sf.list10000", () => StructuredField.TryParseList(list, out _), before.WebTry("StructuredField", "TryParseList", list), after.WebTry("StructuredField", "TryParseList", list));
	}

	/// <summary>A sweep row of the web: this process's own reading is the constant, and each side must accept the text.</summary>
	static Workload PairedWebSweep(string name, Func<bool> own, Func<int> before, Func<int> after)
	{
		return new Workload("web", name,
			[
				new Reading("hand",   () => own() ? 1 : 0),
				new Reading("before", before),
				new Reading("after",  after),
			],
			() => own() && before() == 1 && after() == 1 ? null : $"  the text is accepted by this build {own()}, before {before()}, after {after()}");
	}
}
