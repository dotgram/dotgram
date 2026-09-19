using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The quiet form, <c>bool TryParseR(input, out value)</c>, against the match form the before side has: what a caller who
	/// only asks whether pays, on the refused rows above all. The rows are named <c>.bool</c>; "before" is the Match form
	/// of the before side, "after" the bool form of the after side, and "hand" this process's own Match form; a side that
	/// has no bool form leaves the row out.
	/// </summary>
	static IEnumerable<Workload> PairedBool(PairedSide before, PairedSide after)
	{
		foreach (var (name, text) in new[]
		{
			("refused-early", "(int x) => x +"),
			("refused-late",  "(int x) => x + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 + 10 +"),
			("ladder",        "(int x, int y) => (x + y) * 3 - x / 5"),
		})
		{
			var quiet = after.ElBool("TryParseLambda", text, immediate: false);

			if (quiet is null)
				continue;

			var match = before.El("TryParseLambda", text, immediate: false);
			var own   = HandExpressionAccepts(text);

			yield return new Workload("el", name + ".bool",
				[
					new Reading("hand",   () => own() ? 1 : 0),
					new Reading("before", match),
					new Reading("after",  quiet),
				],
				() => match() == quiet() ? null : $"  the match form says {match()}, the bool form {quiet()}");
		}

		foreach (var (name, method, text) in new[]
		{
			("refused-late", "TryParseQueryExpression", "SELECT a, b, c FROM t WHERE a = 1 AND b = 2 AND c = "),
			("select20",     "TryParseQueryExpression", "SELECT " + string.Join(", ", System.Linq.Enumerable.Range(0, 20).Select(i => "a" + i)) + " FROM t WHERE a0 = 1"),
		})
		{
			var quiet = after.SqlBool(method, text);

			if (quiet is null)
				continue;

			var match = before.Sql(method, text);

			yield return new Workload("sql", name + ".bool",
				[
					new Reading("hand",   () => HandAccepts(method, text) ? 1 : 0),
					new Reading("before", match),
					new Reading("after",  quiet),
				],
				() => match() == quiet() ? null : $"  the match form says {match()}, the bool form {quiet()}");
		}
	}

	static Func<bool> HandExpressionAccepts(string text) =>
		() => DotGram.Handwritten.HandExpression.TryParseLambda(text, new DotGram.ExpressionLanguage.ExpressionParser.State(Caller) { Text = text }).IsSuccess;
}
