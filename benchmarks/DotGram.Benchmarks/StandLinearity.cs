using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Examples.Feeds;
using DotGram.ExpressionLanguage;
using DotGram.Finance.Fix;
using DotGram.Handwritten;
using DotGram.Handwritten.Feeds;
using DotGram.Handwritten.Fix;
using DotGram.Handwritten.Web;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;
using DotGram.Web;

namespace DotGram.Benchmarks;

// The linearity family (architect for Igor, 2026-09-19): a parser that reads a long input is timed at
// three sizes ten times apart, and the exponent of each step — log(t2/t1) / log(n2/n1) — is a column of
// its own. A parser that reads in time proportional to the input has an exponent of 1; the flag is
// raised above 1.2. It would have been worth having long ago: the stock count's generated parser
// counted newlines from the start of the input for every rejected line, and nobody saw it until a row
// happened to hold a thousand of them.
//
// A step above the threshold is not necessarily the algorithm's: a growing list in the parser's result
// lands on the large object heap and is collected in generation 2, and the time of a call then grows
// with the size of what it allocates. So beside each exponent the report gives the generation 2
// collections a call caused at the largest size and the bytes it allocated at each size, and a series
// above the threshold whose largest size collects in generation 2 is flagged "GC" and not "algorithm".
//
// Rough, in one process, without a window: it is the shape of a curve that is asked for, not a number.
// A reading that refuses its input is named and left out, the way a row of the stand would not be timed.

static partial class Stand
{
	/// <summary>The exponent above which a series is flagged.</summary>
	const double LinearityThreshold = 1.2;

	sealed record Series(string Parser, string Form, string Unit, int[] Sizes, Func<int, Func<bool>> Make);

	static IEnumerable<Series> LinearitySeries()
	{
		// ── the stock count ──
		foreach (var broken in new[] { false, true })
		{
			var kind = broken ? "stock count, a tenth broken" : "stock count";

			yield return new Series(kind, "generated text",   "lines", [100, 1000, 10000], n => { var t = StockText(n, broken); return () => StockCountReader.TryParseCount(t).IsSuccess; });
			yield return new Series(kind, "hand text",        "lines", [100, 1000, 10000], n => { var t = StockText(n, broken); return () => HandStockCount.TryRead(t, out _, out _); });
			yield return new Series(kind, "generated reader", "lines", [100, 1000, 10000], n => { var t = StockText(n, broken); return () => StockCountReader.TryParseCount(new StringReader(t)).IsSuccess; });
			yield return new Series(kind, "hand reader",      "lines", [100, 1000, 10000], n => { var t = StockText(n, broken); return () => HandStockCount.TryRead(new StringReader(t), out _, out _); });
		}

		// ── FIX ──
		var order = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";

		foreach (var (form, generated, hand) in new (string, Func<string, Func<bool>>, Func<string, Func<bool>>)[]
		{
			("text",  t => () => FixParser.Parse(t).Count() > 0, t => () => HandFixParser.Parse(t).Count() > 0),
			("bytes", t => { var b = Encoding.Latin1.GetBytes(t); return () => FixParser.Parse(b).Count() > 0; },
			          t => { var b = Encoding.Latin1.GetBytes(t); return () => HandFixParser.Parse(b).Count() > 0; }),
		})
		{
			yield return new Series("FIX orders", "generated " + form, "orders", [4, 40, 400], n => generated(string.Concat(Enumerable.Repeat(order, n))));
			yield return new Series("FIX orders", "hand " + form,      "orders", [4, 40, 400], n => hand(string.Concat(Enumerable.Repeat(order, n))));
		}

		yield return new Series("FIX fields", "generated text", "fields", [16, 160, 1600], n => { var t = FixSlopeText(n); return () => FixParser.Parse(t).Count() > 0; });
		yield return new Series("FIX fields", "hand text",      "fields", [16, 160, 1600], n => { var t = FixSlopeText(n); return () => HandFixParser.Parse(t).Count() > 0; });

		// ── the web ──
		foreach (var (kind, text) in new (string, Func<int, string>)[]
		{
			("JSON array",  n => "[" + string.Join(",", Enumerable.Range(0, n)) + "]"),
			("JSON object", n => "{" + string.Join(",", Enumerable.Range(0, n).Select(static i => $"\"k{i}\":{i}")) + "}"),
		})
		{
			yield return new Series(kind, "generated",         "elements", [100, 1000, 10000], n => { var t = text(n); return () => JsonValue.TryParse(t, out _); });
			yield return new Series(kind, "hand",              "elements", [100, 1000, 10000], n => { var t = text(n); return () => HandJson.TryParse(t, out _, out _); });
			yield return new Series(kind, "System.Text.Json",  "elements", [100, 1000, 10000], n => { var t = text(n); return () => SystemTextJson(t); });
		}

		yield return new Series("URL path", "generated", "segments", [10, 100, 1000], n => { var t = "https://example.com/" + string.Concat(Enumerable.Repeat("seg/", n)); return () => UriReference.TryParse(t, out _); });
		yield return new Series("URL path", "hand",      "segments", [10, 100, 1000], n => { var t = "https://example.com/" + string.Concat(Enumerable.Repeat("seg/", n)); return () => HandUrl.TryParseReference(t, out _, out _); });
		yield return new Series("media type parameters", "generated", "parameters", [10, 100, 1000], n => { var t = "text/html" + string.Concat(Enumerable.Range(0, n).Select(static i => $"; a{i}=1")); return () => MediaType.TryParse(t, out _); });
		yield return new Series("structured list", "generated", "items", [100, 1000, 10000], n => { var t = string.Join(", ", Enumerable.Range(0, n)); return () => StructuredField.TryParseList(t, out _); });

		// The Web's lists and fields, one publication each (docs/design/stand-coverage-2026-09-19.md): a loop in the grammar, so a curve.
		yield return new Series("email address list",   "generated", "addresses", [100, 1000, 10000], n => { var t = string.Join(", ", Enumerable.Range(0, n).Select(static i => $"a{i}@example.com")); return () => EmailAddress.TryParseList(t, out _); });
		// Three points ten times apart can straddle a step: finance-24 found the Accept curve to be flat up to 3,000 ranges, a drop between 3,000 and 4,000
		// (the emitted parser pool keeps no more than 65,536 entries) and flat again, so the exponent 2.07 of the three points was a cliff between two of them.
		yield return new Series("accept, between",      "generated", "ranges",    [500, 1000, 2000, 3000, 4000, 6000, 10000], n => { var t = string.Join(", ", Enumerable.Range(0, n).Select(static i => $"text/x{i};q=0.5")); return () => MediaRange.TryParseAccept(t, out _); });
		yield return new Series("accept",               "generated", "ranges",    [100, 1000, 10000], n => { var t = string.Join(", ", Enumerable.Range(0, n).Select(static i => $"text/x{i};q=0.5")); return () => MediaRange.TryParseAccept(t, out _); });
		yield return new Series("forwarded",            "generated", "elements",  [100, 1000, 10000], n => { var t = string.Join(", ", Enumerable.Range(0, n).Select(static i => $"for=192.0.2.{i % 250};proto=https")); return () => ForwardedElement.TryParseField(t, out _); });
		yield return new Series("link",                 "generated", "links",     [100, 1000, 10000], n => { var t = string.Join(", ", Enumerable.Range(0, n).Select(static i => $"<https://example.com/{i}>; rel=\"next\"")); return () => WebLink.TryParseField(t, out _); });
		yield return new Series("cookie",               "generated", "pairs",     [100, 1000, 10000], n => { var t = string.Join("; ", Enumerable.Range(0, n).Select(static i => $"a{i}=1")); return () => CookiePair.TryParseField(t, out _); });

		// ── a feed ──
		yield return new Series("feed", "generated", "records", [100, 1000, 10000], n =>
		{
			var t = "H|2026-08-13|ACME\n" + string.Concat(Enumerable.Repeat("R|AAPL|100|2026-08-12\n", n)) + $"T|{n}\n";

			return () =>
			{
				try
				{
					return FeedReader.Read(t).Trades.Count == n;
				}
				catch (FormatException)
				{
					return false;
				}
			};
		});

		// ── SQL ──
		string Names(int n) => string.Join(", ", Enumerable.Range(0, n).Select(static i => "c" + i));
		string Conditions(int n) => string.Join(" AND ", Enumerable.Range(0, n).Select(static i => "a" + i + " = 1"));
		string Rows(int n) => string.Join(", ", Enumerable.Repeat("(1, 2)", n));

		yield return new Series("T-SQL columns",    "generated", "columns",    [10, 100, 1000], n => { var t = $"SELECT {Names(n)} FROM t"; return () => TransactSqlParser.TryParseStatement(t).IsSuccess; });
		yield return new Series("T-SQL conditions", "generated", "predicates", [10, 100, 1000], n => { var t = Conditions(n); return () => TransactSqlParser.TryParseSearchCondition(t).IsSuccess; });
		yield return new Series("T-SQL rows",       "generated", "rows",       [10, 100, 1000], n => { var t = $"INSERT INTO t (a, b) VALUES {Rows(n)}"; return () => TransactSqlParser.TryParseStatement(t).IsSuccess; });
		// A script read a statement at a time through the positional form, which tokenizes the whole text on every call (expr, 2026-09-19).
		yield return new Series("T-SQL script, a statement at a time", "generated", "statements", [10, 100, 400], n =>
		{
			var t = string.Concat(Enumerable.Repeat("SELECT a, b FROM t WHERE a = 1;" + (char)10, n));

			return () =>
			{
				var at    = 0;
				var count = 0;

				while (at < t.Length)
				{
					var match = TransactSqlParser.TryParseStatement(t, at);

					if (!match.IsSuccess)
						return false;

					count++;
					at = (int)(match.Position + match.Length);

					while (at < t.Length && (char.IsWhiteSpace(t[at]) || t[at] == ';'))
						at++;
				}

				return count == n;
			};
		});

		yield return new Series("SQL:2023 conditions", "generated", "predicates", [10, 100, 1000], n => { var t = Conditions(n); return () => SqlStandardParser.TryParseSearchCondition(t).IsSuccess; });
		yield return new Series("SQL:2023 conditions", "hand",      "predicates", [10, 100, 1000], n => { var t = Conditions(n); return () => HandSqlStandard.TryParseSearchCondition(t, out _); });

		// Parentheses nested to a depth and closed: the accepted twin of the refusal ladders' "nested parentheses, never closed" (sql-39, 2026-09-20: the refused form
		// and this one cost alike, about n^2.8, so what the ladders found is a cost of nesting and not of refusing).
		string Nest(int n) => new string('(', n) + "a = 1" + new string(')', n);

		yield return new Series("T-SQL search condition, nested parentheses",   "generated", "levels", [32, 64, 128, 300], n => { var t = Nest(n); return () => TransactSqlParser.TryParseSearchCondition(t).IsSuccess; });
		yield return new Series("T-SQL statement, nested parentheses",          "generated", "levels", [32, 64, 128, 300], n => { var t = "SELECT " + new string('(', n) + "1" + new string(')', n); return () => TransactSqlParser.TryParseStatement(t).IsSuccess; });
		yield return new Series("SQL:2023 search condition, nested parentheses", "generated", "levels", [32, 64, 128, 300], n => { var t = Nest(n); return () => SqlStandardParser.TryParseSearchCondition(t).IsSuccess; });
		yield return new Series("SQL:2023 query, nested parentheses",           "generated", "levels", [32, 64, 128, 300], n => { var t = "SELECT " + new string('(', n) + "1" + new string(')', n); return () => SqlStandardParser.TryParseQueryExpression(t).IsSuccess; });
		// ── the expression language ──
		var chain = (int n) => "(int x) => x" + string.Concat(Enumerable.Repeat(" + x", n));

		yield return new Series("expression", "hand",      "terms", [10, 100, 1000], n => { var t = chain(n); return () => HandExpression.TryParseLambda(t, new ExpressionParser.State(Caller) { Text = t }).IsSuccess; });
		yield return new Series("expression", "tape",      "terms", [10, 100, 1000], n => { var t = chain(n); return () => ExpressionParser.TryParseLambda(t, new ExpressionParser.State(Caller) { Text = t }).IsSuccess; });
		yield return new Series("expression", "immediate", "terms", [10, 100, 1000], n => { var t = chain(n); return () => ExpressionParser.Immediate.TryParseLambda(t, new ExpressionParser.State(Caller) { Text = t }).IsSuccess; });
	}

	/// <summary>
	/// Every series at three sizes and the exponent of each step. Returns the series that passed the
	/// threshold, which is what the report flags.
	/// </summary>
	public static void Linearity()
	{
		Console.WriteLine("Built from " + BinaryCommit() + ".");
		Console.WriteLine();
		Console.WriteLine("| parser | form | unit | sizes | µs at each size | exponents | KB a call | gen2 a call at the largest | |");
		Console.WriteLine("| --- | --- | --- | --- | --- | --- | --- | ---: | --- |");

		var flagged = new List<string>();

		foreach (var series in LinearitySeries())
		{
			var times     = new List<double>();
			var kilobytes = new List<double>();
			var gen2      = 0.0;

			foreach (var size in series.Sizes)
			{
				var read = series.Make(size);

				if (!read())
				{
					times.Add(double.NaN);

					break;
				}

				var (time, bytes, collected) = TimeRough(read);

				times.Add(time);
				kilobytes.Add(bytes / 1024);
				gen2 = collected;

				// A call of seconds is the answer already; the next size would be a hundred times as long.
				if (time > 3e9)
					break;
			}

			var exponents = new List<double>();

			for (var i = 1; i < times.Count; i++)
				exponents.Add(Math.Log(times[i] / times[i - 1]) / Math.Log((double)series.Sizes[i] / series.Sizes[i - 1]));

			var worst = exponents.Count == 0 ? double.NaN : exponents.Max();

			// What it allocates has an exponent of its own: a list copied on every element allocates with the
			// square of the size, and that is the algorithm's and not the collector's however often generation 2 runs.
			var allocated = Enumerable.Range(1, Math.Max(0, kilobytes.Count - 1))
				.Select(i => Math.Log(kilobytes[i] / kilobytes[i - 1]) / Math.Log((double)series.Sizes[i] / series.Sizes[i - 1]))
				.DefaultIfEmpty(0)
				.Max();

			var flag = times.Any(double.IsNaN)
				? "REFUSED"
				: worst <= LinearityThreshold ? ""
				: allocated > LinearityThreshold ? "ALGORITHM (allocation)"
				: gen2 > 0 ? "GC"
				: "ALGORITHM";

			if (flag is not "" and not "REFUSED")
				flagged.Add($"{series.Parser}, {series.Form}: time exponent {worst:F2}, allocation exponent {allocated:F2}, generation 2 a call {gen2:F2}: {flag}");

			Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
				$"| {series.Parser} | {series.Form} | {series.Unit} | {string.Join(" / ", series.Sizes.Take(times.Count).Select(static one => one.ToString("N0", CultureInfo.InvariantCulture)))} | " +
				$"{string.Join(" / ", times.Select(static one => double.IsNaN(one) ? "-" : (one / 1000).ToString("F1", CultureInfo.InvariantCulture)))} | " +
				$"{string.Join(" / ", exponents.Select(static one => one.ToString("F2", CultureInfo.InvariantCulture)))} | " +
				$"{string.Join(" / ", kilobytes.Select(static one => one.ToString("N0", CultureInfo.InvariantCulture)))} | {gen2:F2} | {flag} |"));
		}

		Console.WriteLine();
		Console.WriteLine(flagged.Count == 0
			? $"No series above the exponent {LinearityThreshold}."
			: $"Above the exponent {LinearityThreshold} (GC: linear allocation and generation 2 collected at the largest size; ALGORITHM: the time grows without either, or (allocation) the allocation grows too):\n" + string.Join("\n", flagged.Select(static one => "- " + one)));
	}

	/// <summary>
	/// Nanoseconds a call takes, the bytes it allocates and the generation 2 collections it causes:
	/// warmed for 100 ms, then timed for at least 200 ms and 3 calls.
	/// </summary>
	static (double Nanoseconds, double Bytes, double Gen2) TimeRough(Func<bool> run)
	{
		var watch = Stopwatch.StartNew();

		do
			run();
		while (watch.ElapsedMilliseconds < 100);

		var calls     = 0;
		var allocated = GC.GetAllocatedBytesForCurrentThread();
		var gen2      = GC.CollectionCount(2);

		watch.Restart();

		do
		{
			run();
			calls++;
		}
		while (watch.ElapsedMilliseconds < 200 || calls < 3);

		return (watch.Elapsed.TotalMilliseconds * 1e6 / calls,
			(GC.GetAllocatedBytesForCurrentThread() - allocated) / (double)calls,
			(GC.CollectionCount(2) - gen2) / (double)calls);
	}
}
