using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The rows of the pair of a change to what the value stores and the lexer's buffer keep (performance-ff, 5de9a75d against 5ed9682b, 2026-09-20), each defined by the author of the change:
	/// the expression language's tape parse at about 200,000 tokens (BELOW the lexer buffer's bound of 1,048,576 entries, three arrays of one per token, about 350,000 tokens: a control, flat)
	/// and at about 500,000 (above it: the row that tests the change); and the WORST SHAPE for the dense value store: one SELECT of a great many simple column references, which build one value
	/// type, so that one table grows past 65,536 entries while the record tables stay under the bound and the store goes back to the ordinary spare and is never parked. 100,000 columns is well
	/// inside that window, 300,000 near its top, 400,000 above it (parked instead, and behaving differently for that reason). What such a row is for is the retained bytes after the parse
	/// (`--stand-retained`), not the time, and the largest of them are too heavy for a timed window.
	/// </summary>
	static IEnumerable<Workload> PairedRetention(PairedSide before, PairedSide after)
	{
		// "(int x) => x" and then " + x": two tokens a term, six for the head. A PARSE, not the scan: the scan function allocates nothing and never touches the lexer's token buffer (first try, 2026-09-20).
		foreach (var (name, terms) in new[] { ("parse-200k-tokens", 100_000), ("parse-500k-tokens", 250_000) })
		{
			var text = "(int x) => x" + string.Concat(Enumerable.Repeat(" + x", terms));
			var own  = (Func<int>)(() => DotGram.ExpressionLanguage.ExpressionParser.TryParseLambda(text, new DotGram.ExpressionLanguage.ExpressionParser.State(Caller) { Text = text }).IsSuccess ? 1 : 0);
			var b    = before.El("TryParseLambda", text, immediate: false);
			var a    = after.El("TryParseLambda", text, immediate: false);

			yield return new Workload("el", name,
				[
					new Reading("control", own),
					new Reading("before", b),
					new Reading("after",  a),
				],
				() => own() == 1 && b() == 1 && a() == 1 ? null : $"  a chain of {terms:N0} terms is read by every side, and control reads {own()}, before {b()}, after {a()}");
		}
		foreach (var (name, columns) in new[] { ("worst-columns-100k", 100_000), ("worst-columns-300k", 300_000), ("worst-columns-400k", 400_000) })
		{
			var text = "SELECT " + string.Join(", ", Enumerable.Range(0, columns).Select(static i => "c" + i)) + " FROM t";
			var own  = (Func<int>)(() => SqlStandardParser.TryParseQueryExpression(text).IsSuccess ? 1 : 0);
			var b    = before.Sql("TryParseQueryExpression", text);
			var a    = after.Sql("TryParseQueryExpression", text);

			yield return new Workload("sql", name,
				[
					new Reading("control", own),
					new Reading("before", b),
					new Reading("after",  a),
				],
				() => own() == 1 && b() == 1 && a() == 1 ? null : $"  a column list of {columns:N0} is read by every side, and control reads {own()}, before {b()}, after {a()}");
		}
	}
}
