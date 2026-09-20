using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

using DotGram.Examples.Feeds;
using DotGram.ExpressionLanguage;
using DotGram.Finance.Fix;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;
using DotGram.Web;

namespace DotGram.Benchmarks;

// The linearity of a refusal (architect, 2026-09-20). The accepted ladders of StandLinearity.cs say how a parser reads a long input it takes; a
// parser can read that in a straight line and still have a shape in its grammar, `(A+)*` over an atom, that a REFUSAL after a long head walks
// in every way the head can be cut: 2^(n-1) of them, 0.03 ms at eight characters and 3.2 s at twenty-four. Two of those shipped (a URI template
// with an unclosed `{` after a literal run, an address list with a bare `@` after an atom run, 107 s at twenty-eight characters) because nothing
// timed a refusal at more than one length. A refusal comes from outside and is sent by anyone, so its bar is the harder one: an exponent above
// 1.10 is a defect, where an accepted input is held to 1.2.
//
// A series is a head that grows (the same units the accepted ladder grows by: a run of literal characters, a list, a chain of terms, records)
// and a tail no reader can finish (an unclosed `{`, `<`, `"`, `(`, a dangling operator, a bare `@`, a cut record), so that the input is refused
// after the head is read. The series asserts that it IS refused: a series whose input is accepted checks nothing, and is reported as a fault of
// the series and not counted as a pass.
//
// The budget is what makes it a guard and not a wait. Sizes grow by a quarter, so that a doubling for every character rises sixteen-fold a step
// and not sixty-five-thousand-fold; the first call at a size is untimed and is itself the probe; and a ladder ends at the first size whose call
// takes over 20 ms twice. The worst call a series makes is then a few times 20 ms, not the 100 s the defect it looks for would take.
//
// Rough, in one process, without a window: the shape of a curve is asked for, and a refusal of milliseconds is far from a difference of a few percent.

static partial class Stand
{
	/// <summary>The exponent above which the time of a refusal is a defect.</summary>
	const double RefusalThreshold = 1.10;

	/// <summary>The exponent from which a refusal is called quadratic, and the one from which it is called explosive (a doubling for every character is far above it).</summary>
	const double RefusalQuadratic = 1.5;

	const double RefusalExplosive = 3;

	/// <summary>The call, in milliseconds, at which a ladder ends.</summary>
	const double RefusalBudgetMilliseconds = 20;

	/// <summary>The time a first call at a size is given, in milliseconds, before it is abandoned and the series is called explosive.</summary>
	const int RefusalWatchdogMilliseconds = 2000;

	sealed record Refusal(string Parser, string Shape, string Unit, int Largest, Func<int, string> Text, Func<string, bool> Accepts);

	static string Copies(string text, int count) => string.Concat(Enumerable.Repeat(text, count));

	static IEnumerable<Refusal> RefusalSeries()
	{
		var soh    = ((char)1).ToString();
		var quote  = "\"";
		var cut    = ((char)1).ToString();

		Refusal R(string parser, string shape, string unit, int largest, Func<int, string> text, Func<string, bool> accepts) => new(parser, shape, unit, largest, text, accepts);

		// ── the web: one reader a publication, the shapes finance-24 found first ──
		yield return R("UriTemplate", "a literal run, then an unclosed {", "characters", 4096, n => new string('a', n) + "{unclosed", static t => UriTemplate.TryParse(t, out _));
		yield return R("UriTemplate", "literals and expressions, then an unclosed {", "expressions", 4096, n => "https://x/" + Copies("a{x}", n) + "{u", static t => UriTemplate.TryParse(t, out _));
		yield return R("UriTemplate", "an expression of n variables, unclosed", "variables", 4096, n => "https://x/{" + Copies("v,", n), static t => UriTemplate.TryParse(t, out _));

		yield return R("EmailAddress.TryParseList", "a long atom, then a bare @", "characters", 4096, n => new string('a', n) + "@", static t => EmailAddress.TryParseList(t, out _));
		yield return R("EmailAddress.TryParseList", "words, then an unclosed <", "words", 4096, n => Copies("a ", n) + "<b@c", static t => EmailAddress.TryParseList(t, out _));
		yield return R("EmailAddress.TryParseList", "addresses, then a bare @", "addresses", 4096, n => Copies("a@example.com, ", n) + "a@", static t => EmailAddress.TryParseList(t, out _));
		yield return R("EmailAddress.TryParseList", "an unclosed quoted string", "characters", 4096, n => quote + new string('a', n), static t => EmailAddress.TryParseList(t, out _));
		yield return R("EmailAddress.TryParseList", "comments nested and unclosed", "levels", 512, n => new string('(', n) + "a@b", static t => EmailAddress.TryParseList(t, out _));
		yield return R("EmailAddress.TryParseList", "an unclosed domain literal", "characters", 4096, n => "a@[" + new string('1', n), static t => EmailAddress.TryParseList(t, out _));
		yield return R("AddrSpec.TryParseStrict", "a long local part, then @@", "characters", 4096, n => new string('a', n) + "@@example.com", static t => AddrSpec.TryParseStrict(t, out _));

		yield return R("MediaType", "parameters, then an unclosed quoted string", "parameters", 4096, n => "text/html" + Copies("; a=1", n) + "; b=" + quote + "x", static t => MediaType.TryParse(t, out _));
		yield return R("MediaType", "an unclosed quoted string", "characters", 4096, n => "text/html; a=" + quote + new string('x', n), static t => MediaType.TryParse(t, out _));
		yield return R("MediaType", "a long subtype, then a slash", "characters", 4096, n => "text/" + new string('a', n) + "/x", static t => MediaType.TryParse(t, out _));
		yield return R("MediaRange.TryParseAccept", "ranges, then a range with no weight", "ranges", 4096, n => Copies("text/x1;q=0.5, ", n) + "text/x;q=", static t => MediaRange.TryParseAccept(t, out _));
		yield return R("MediaRange.TryParseAccept", "a long token, then a weight with no value", "characters", 4096, n => "text/" + new string('a', n) + ";q=", static t => MediaRange.TryParseAccept(t, out _));

		yield return R("StructuredField.TryParseList", "items, then an unclosed (", "items", 4096, n => Copies("1, ", n) + "(", static t => StructuredField.TryParseList(t, out _));
		yield return R("StructuredField.TryParseList", "an unclosed string", "characters", 4096, n => quote + new string('a', n), static t => StructuredField.TryParseList(t, out _));
		yield return R("StructuredField.TryParseList", "an inner list, unclosed", "items", 4096, n => "(" + Copies("1 ", n), static t => StructuredField.TryParseList(t, out _));
		yield return R("StructuredField.TryParseDictionary", "members, then an unclosed string", "members", 4096, n => Copies("a=1, ", n) + "b=" + quote + "x", static t => StructuredField.TryParseDictionary(t, out _));
		yield return R("StructuredField.TryParseItem", "parameters, then a parameter with no value", "parameters", 4096, n => "1" + Copies(";a=1", n) + ";b=", static t => StructuredField.TryParseItem(t, out _));

		yield return R("ForwardedElement.TryParseField", "elements, then for= with no value", "elements", 4096, n => Copies("for=192.0.2.1;proto=https, ", n) + "for=", static t => ForwardedElement.TryParseField(t, out _));
		yield return R("ForwardedElement.TryParseField", "an unclosed quoted string", "characters", 4096, n => "for=" + quote + new string('a', n), static t => ForwardedElement.TryParseField(t, out _));
		yield return R("WebLink.TryParseField", "links, then an unclosed <", "links", 4096, n => Copies("<https://example.com/1>; rel=" + quote + "next" + quote + ", ", n) + "<https://x", static t => WebLink.TryParseField(t, out _));
		yield return R("WebLink.TryParseField", "an unclosed URI reference", "characters", 4096, n => "<https://example.com/" + new string('a', n), static t => WebLink.TryParseField(t, out _));
		yield return R("WebLink.TryParseField", "parameters, then an unclosed quoted string", "parameters", 4096, n => "<https://x>" + Copies("; a=1", n) + "; b=" + quote, static t => WebLink.TryParseField(t, out _));

		yield return R("CookiePair.TryParseField", "pairs, then a control character", "pairs", 4096, n => Copies("a=1; ", n) + "b=" + cut, static t => CookiePair.TryParseField(t, out _));

		yield return R("LanguageTag", "variants, then a bare hyphen", "variants", 4096, n => "sl" + Copies("-1994", n) + "-", static t => LanguageTag.TryParse(t, out _));
		yield return R("LanguageTag", "extension subtags, then a doubled hyphen", "subtags", 4096, n => "en-u" + Copies("-abcd", n) + "--", static t => LanguageTag.TryParse(t, out _));
		yield return R("ContentDisposition", "parameters, then an unclosed quoted string", "parameters", 4096, n => "attachment" + Copies("; a=1", n) + "; b=" + quote + "x", static t => ContentDisposition.TryParse(t, out _));
		yield return R("ContentDisposition", "an extended value, then a cut percent-encoding", "octets", 4096, n => "attachment; filename*=UTF-8''" + Copies("%41", n) + "%4", static t => ContentDisposition.TryParse(t, out _));

		yield return R("JsonPointer", "segments, then a bad escape", "segments", 4096, n => Copies("/a", n) + "~", static t => JsonPointer.TryParse(t, out _));
		yield return R("JsonPointer", "escapes, then a bad escape", "escapes", 4096, n => "/" + Copies("~0", n) + "~", static t => JsonPointer.TryParse(t, out _));
		yield return R("JsonPatch", "operations, then a cut one", "operations", 4096, n => "[" + Copies("{" + quote + "op" + quote + ":" + quote + "remove" + quote + "," + quote + "path" + quote + ":" + quote + "/a" + quote + "},", n) + "{" + quote + "op" + quote + ":", static t => JsonPatchParses(t));

		yield return R("JsonValue", "an array, unclosed", "elements", 4096, n => "[" + Copies("1,", n) + "1", static t => JsonValue.TryParse(t, out _));
		yield return R("JsonValue", "an object, cut at a value", "members", 4096, n => "{" + Copies(quote + "a" + quote + ":1,", n) + quote + "b" + quote + ":", static t => JsonValue.TryParse(t, out _));
		yield return R("JsonValue", "an unclosed string", "characters", 4096, n => quote + new string('a', n), static t => JsonValue.TryParse(t, out _));
		yield return R("JsonValue", "a long number, then a bare point", "digits", 4096, n => "1" + new string('0', n) + ".", static t => JsonValue.TryParse(t, out _));
		yield return R("JsonValue", "arrays nested and unclosed", "levels", 300, n => new string('[', n), static t => JsonValue.TryParse(t, out _));
		yield return R("JsonValue", "objects nested and unclosed", "levels", 300, n => Copies("{" + quote + "a" + quote + ":", n), static t => JsonValue.TryParse(t, out _));

		yield return R("UriReference", "segments, then a bad percent-encoding", "segments", 4096, n => "https://example.com/" + Copies("seg/", n) + "%zz", static t => UriReference.TryParse(t, out _));
		yield return R("UriReference", "a query, then a bad percent-encoding", "pairs", 4096, n => "https://example.com/?" + Copies("a=1&", n) + "%", static t => UriReference.TryParse(t, out _));
		yield return R("UriReference", "an unclosed IPv6 literal", "groups", 4096, n => "http://[" + Copies("1:", n), static t => UriReference.TryParse(t, out _));
		yield return R("UriReference", "a long host, then a bracket", "characters", 4096, n => "https://" + new string('a', n) + "[", static t => UriReference.TryParse(t, out _));

		// ── FIX: the message layer, lenient, over a head that grows and a wire cut where a field or the checksum would end ──
		yield return R("FixMessages.TryParse", "fields, then a checksum cut short", "fields", 4096, n =>
		{
			var body = "35=3" + soh + Copies("58=text" + soh, n);

			return "8=FIX.4.4" + soh + "9=" + body.Length + soh + body + "10=";
		}, static t => FixMessages.TryParse(t, out _, out _, new FixParseOptions(FixParseMode.Lenient)));
		yield return R("FixMessages.TryParse", "one value never terminated", "characters", 4096, n => "8=FIX.4.4" + soh + "9=5" + soh + "35=3" + soh + "58=" + new string('a', n), static t => FixMessages.TryParse(t, out _, out _, new FixParseOptions(FixParseMode.Lenient)));

		// ── the feeds ──
		yield return R("FeedReader", "records, then a cut record", "records", 4096, n => "H|2026-08-13|ACME\n" + Copies("R|AAPL|100|2026-08-12\n", n) + "R|AAPL|100|", static t => FeedAccepts(t));
		yield return R("StockCountReader", "lines, then no closing count", "lines", 4096, n => string.Concat(Enumerable.Range(0, n).Select(static i => $"{StockItem(i)}: {i % 100}\n")), static t => StockCountReader.TryParseCount(t).IsSuccess);

		// ── SQL ──
		string Names(int n) => string.Join(", ", Enumerable.Range(0, n).Select(static i => "c" + i));
		string Conditions(int n) => string.Join(" AND ", Enumerable.Range(0, n).Select(static i => "a" + i + " = 1"));
		string Rows(int n) => string.Join(", ", Enumerable.Repeat("(1, 2)", n));

		yield return R("T-SQL statement", "columns, then a FROM with nothing after it", "columns", 4096, n => $"SELECT {Names(n)} FROM", static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "an unclosed string", "characters", 4096, n => "SELECT '" + new string('a', n), static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "an unclosed comment", "characters", 4096, n => "SELECT 1 /* " + new string('a', n), static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "an unclosed bracketed identifier", "characters", 4096, n => "SELECT [" + new string('a', n), static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "parentheses opened and not closed", "levels", 300, n => "SELECT " + new string('(', n) + "1", static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "subqueries opened and not closed", "levels", 200, n => "SELECT * FROM " + Copies("(SELECT * FROM ", n) + "t", static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "CASE arms, no END", "arms", 4096, n => "SELECT CASE" + Copies(" WHEN 1 = 1 THEN 1", n), static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "rows, then a cut row", "rows", 4096, n => $"INSERT INTO t (a, b) VALUES {Rows(n)}, (1,", static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL search condition", "predicates, then a dangling AND", "predicates", 4096, n => Conditions(n) + " AND", static t => TransactSqlParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("T-SQL search condition", "predicates, then an unclosed (", "predicates", 4096, n => Conditions(n) + " AND (a = 1", static t => TransactSqlParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("T-SQL search condition", "parentheses opened and not closed", "levels", 300, n => new string('(', n) + "a = 1", static t => TransactSqlParser.TryParseSearchCondition(t).IsSuccess);

		yield return R("SQL:2023 query", "columns, then a FROM with nothing after it", "columns", 4096, n => $"SELECT {Names(n)} FROM", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "an unclosed string", "characters", 4096, n => "SELECT '" + new string('a', n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "an unclosed comment", "characters", 4096, n => "SELECT 1 /* " + new string('a', n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "an unclosed quoted identifier", "characters", 4096, n => "SELECT " + quote + new string('a', n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "parentheses opened and not closed", "levels", 300, n => "SELECT " + new string('(', n) + "1", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "subqueries opened and not closed", "levels", 200, n => "SELECT * FROM " + Copies("(SELECT * FROM ", n) + "t", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "CASE arms, no END", "arms", 4096, n => "SELECT CASE" + Copies(" WHEN 1 = 1 THEN 1", n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "joins, then a JOIN with nothing after it", "joins", 4096, n => "SELECT * FROM t" + Copies(" JOIN t ON a = a", n) + " JOIN", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 search condition", "predicates, then a dangling AND", "predicates", 4096, n => Conditions(n) + " AND", static t => SqlStandardParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("SQL:2023 search condition", "predicates, then an unclosed (", "predicates", 4096, n => Conditions(n) + " AND (a = 1", static t => SqlStandardParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("SQL:2023 search condition", "parentheses opened and not closed", "levels", 300, n => new string('(', n) + "a = 1", static t => SqlStandardParser.TryParseSearchCondition(t).IsSuccess);

		// ── the expression language: the tape and the immediate reading are two machines over one grammar ──
		foreach (var (form, accepts) in new (string, Func<string, bool>)[]
		{
			("tape",      static t => ExpressionParser.TryParseLambda(t, new ExpressionParser.State(Caller) { Text = t }).IsSuccess),
			("immediate", static t => ExpressionParser.Immediate.TryParseLambda(t, new ExpressionParser.State(Caller) { Text = t }).IsSuccess),
		})
		{
			yield return R("expression, " + form, "terms, then a dangling +", "terms", 4096, n => "(int x) => x" + Copies(" + x", n) + " +", accepts);
			yield return R("expression, " + form, "parentheses opened and not closed", "levels", 300, n => "(int x) => " + new string('(', n) + "x", accepts);
			yield return R("expression, " + form, "an unclosed string", "characters", 4096, n => "(int x) => " + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an unclosed verbatim string", "characters", 4096, n => "(int x) => @" + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an unclosed interpolated string", "characters", 4096, n => "(int x) => $" + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an interpolated string, an unclosed hole", "characters", 4096, n => "(int x) => $" + quote + new string('a', n) + "{x", accepts);
			yield return R("expression, " + form, "an interpolated string with holes, then no closing quote", "holes", 4096, n => "(int x) => $" + quote + Copies("a{x}", n), accepts);
			yield return R("expression, " + form, "an unclosed character literal", "characters", 4096, n => "(int x) => '" + new string('a', n), accepts);
			yield return R("expression, " + form, "member accesses, then a dangling .", "members", 4096, n => "(string s) => s" + Copies(".ToString()", n) + ".", accepts);
			yield return R("expression, " + form, "conditionals, then a cut one", "levels", 300, n => "(bool b) => " + Copies("b ? b : ", n), accepts);
		}
	}

	/// <summary>What the refusal ladders do not cover, printed under the table so that the list is read with the table and not lost.</summary>
	static readonly string[] RefusalGaps =
	[
		"Timestamp (RFC 3339): a fixed-length form, there is no head that grows before a refusal.",
		"SetCookie: the reader is lenient, and every tail tried (a control character in a value or in an attribute) was accepted, so the series that asserted a refusal was a fault and was dropped; a refused shape with a growing head is not known.",
		"The reader, stream and span forms of every grammar above: the ladders read a string, and the driver that pulls a TextReader or a Stream in pieces is a different piece of code over the same rules.",
		"FixParser.Parse and the streaming FIX forms: they yield fields lazily and do not report a refusal as a value, so a ladder has nothing to read a refusal from.",
		"SQL and T-SQL entry points other than a statement, a query expression and a search condition (scripts, the positional forms, expressions alone).",
		"Rules that are reachable only from a start the ladders do not use: a shape whose bad tail sits inside a rule no series begins in is not seen here, and the static probe is the way to find it.",
		"The examples under examples/ (not shipped), and the hand-written parsers (not a grammar's).",
	];

	static bool FeedAccepts(string text)
	{
		try
		{
			return FeedReader.Read(text).Trades.Count >= 0;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	/// <summary>One call on a thread of its own, given up on after the watchdog: whether it finished, whether it accepted the input, and the milliseconds it took. An abandoned call runs on until the process ends.</summary>
	static (bool Finished, bool Accepted, double Milliseconds) Probe(Func<bool> read)
	{
		var accepted = false;
		var thrown   = (Exception?)null;
		var watch    = Stopwatch.StartNew();
		var thread   = new Thread(() =>
		{
			try
			{
				accepted = read();
			}
			catch (Exception exception)
			{
				thrown = exception;
			}
		}, 16 * 1024 * 1024) { IsBackground = true };

		thread.Start();

		if (!thread.Join(RefusalWatchdogMilliseconds))
			return (false, false, watch.Elapsed.TotalMilliseconds);

		if (thrown is not null)
			ExceptionDispatchInfo.Capture(thrown).Throw();

		return (true, accepted, watch.Elapsed.TotalMilliseconds);
	}

	/// <summary>A time in nanoseconds as nanoseconds, microseconds, milliseconds, seconds or, past a day, years.</summary>
	static string Duration(double nanoseconds) => nanoseconds switch
	{
		< 1e3 => nanoseconds.ToString("F0", CultureInfo.InvariantCulture) + " ns",
		< 1e6 => (nanoseconds / 1e3).ToString("F1", CultureInfo.InvariantCulture) + " us",
		< 1e9 => (nanoseconds / 1e6).ToString("F1", CultureInfo.InvariantCulture) + " ms",
		< 8.64e13 => (nanoseconds / 1e9).ToString("F1", CultureInfo.InvariantCulture) + " s",
		_ => "over a day",
	};

	/// <summary>The sizes of a ladder: four, then a quarter more each step (at least one), up to the largest.</summary>
	static IEnumerable<int> RefusalSizes(int largest)
	{
		for (var n = 4; n <= largest; n = Math.Max(n + 1, n * 5 / 4))
			yield return n;
	}

	/// <summary>The fastest of three batches of at least 8 ms and 2 calls: nanoseconds a call and the bytes it allocates.</summary>
	static (double Nanoseconds, double Bytes) TimeRefusal(Func<bool> run)
	{
		var best  = double.MaxValue;
		var bytes = 0.0;

		for (var batch = 0; batch < 3; batch++)
		{
			var calls     = 0;
			var allocated = GC.GetAllocatedBytesForCurrentThread();
			var watch     = Stopwatch.StartNew();

			do
			{
				run();
				calls++;
			}
			while (watch.ElapsedMilliseconds < 8 || calls < 2);

			var each = watch.Elapsed.TotalMilliseconds * 1e6 / calls;

			if (each < best)
			{
				best  = each;
				bytes = (GC.GetAllocatedBytesForCurrentThread() - allocated) / (double)calls;
			}
		}

		return (best, bytes);
	}

	/// <summary>The slope of log time on log size over the points from a sixteenth of the largest size up (the last four at least).</summary>
	static double RefusalExponent(List<(int N, double Ns, double Bytes)> points, Func<(int N, double Ns, double Bytes), double> value)
	{
		var top = points[^1].N;
		var use = points.Where(p => p.N * 16L >= top && value(p) > 0).ToList();

		if (use.Count < 4)
			use = points.Where(p => value(p) > 0).TakeLast(Math.Min(4, points.Count)).ToList();

		if (use.Count < 2)
			return double.NaN;

		var x    = use.Select(p => Math.Log(p.N)).ToArray();
		var y    = use.Select(p => Math.Log(value(p))).ToArray();
		var mean = (x.Average(), y.Average());
		var sxy  = x.Zip(y, (a, b) => (a - mean.Item1) * (b - mean.Item2)).Sum();
		var sxx  = x.Sum(a => (a - mean.Item1) * (a - mean.Item1));

		return sxx == 0 ? double.NaN : sxy / sxx;
	}

	/// <summary>
	/// Every refusal ladder, the exponent of each, and what was not covered. <paramref name="only"/> keeps the series whose parser or shape contains it.
	/// Returns the number of series that are defects or faults of the series, which is what a guard would fail on.
	/// </summary>
	public static int RefusedLinearity(string? only = null)
	{
		Console.WriteLine("Refused inputs: a growing head and a tail no reader can finish; each series asserts that the input is refused. The exponent is the slope of log time on log size over the largest sixteenth of the ladder (at least four points); above " +
			RefusalThreshold.ToString("F2", CultureInfo.InvariantCulture) + " a refusal is a defect, above " + RefusalExplosive.ToString("F1", CultureInfo.InvariantCulture) + " it explodes. A ladder ends at the first call over " + RefusalBudgetMilliseconds + " ms.");
		Console.WriteLine();
		Console.WriteLine("| parser | shape | unit | sizes | points | input chars at the largest | time at the largest | exponent | projected at 64 KiB | KB a call at the largest | allocation exponent | |");
		Console.WriteLine("| --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |");

		var problems = new List<string>();
		var series   = 0;

		foreach (var refusal in RefusalSeries())
		{
			if (only is not null && !refusal.Parser.Contains(only, StringComparison.OrdinalIgnoreCase) && !refusal.Shape.Contains(only, StringComparison.OrdinalIgnoreCase))
				continue;

			series++;

			var points  = new List<(int N, double Ns, double Bytes)>();
			var budget  = false;
			var hung    = false;
			var faulty  = false;
			var thrown  = (string?)null;

			try
			{
				foreach (var n in RefusalSizes(refusal.Largest))
				{
					var text = refusal.Text(n);
					var read = () => refusal.Accepts(text);

					// The first call at a size is the probe, on a thread of its own so that it can be given up on: a call that would take hours is not waited for.
					var (finished, took, first) = Probe(read);

					if (!finished)
					{
						points.Add((n, RefusalWatchdogMilliseconds * 1e6, 0));

						budget = true;
						hung   = true;

						break;
					}

					if (took)
					{
						faulty = true;

						break;
					}

					// The first size also warms the parser; the call that is timed there is not the probe.
					if (points.Count == 0)
					{
						read();
						read();
					}
					else if (first > RefusalBudgetMilliseconds)
					{
						// Twice: a first call at a new size can be a compile and not the algorithm.
						var (again, _, second) = Probe(read);

						if (!again || second > RefusalBudgetMilliseconds)
						{
							points.Add((n, again ? Math.Min(first, second) * 1e6 : RefusalWatchdogMilliseconds * 1e6, 0));

							budget = true;
							hung   = !again;

							break;
						}
					}

					var (time, bytes) = TimeRefusal(read);

					points.Add((n, time, bytes));
				}
			}
			catch (Exception exception)
			{
				thrown = exception.GetType().Name;
			}

			var exponent   = points.Count < 2 ? double.NaN : RefusalExponent(points, static p => p.Ns);
			var allocation = points.Count < 2 ? double.NaN : RefusalExponent(points, static p => p.Bytes);
			var last       = points.Count == 0 ? (N: 0, Ns: double.NaN, Bytes: double.NaN) : points[^1];
			var length     = points.Count == 0 ? 0 : refusal.Text(last.N).Length;
			var stopped    = budget ? ", stopped at " + last.N.ToString("N0", CultureInfo.InvariantCulture) + " (" + (last.Ns / 1e6).ToString("F0", CultureInfo.InvariantCulture) + " ms)" : "";

			var flag = thrown is not null ? "THREW " + thrown
				: hung ? "EXPLOSIVE: a call at " + last.N.ToString("N0", CultureInfo.InvariantCulture) + " " + refusal.Unit + " had not finished in " + RefusalWatchdogMilliseconds + " ms"
				: faulty ? "SERIES FAULT: the input is accepted"
				: double.IsNaN(exponent) ? "no curve"
				: exponent >= RefusalExplosive ? "EXPLOSIVE" + stopped
				: exponent >= RefusalQuadratic ? "QUADRATIC" + stopped
				: exponent > RefusalThreshold ? "SUPERLINEAR" + stopped
				: budget ? "large and linear" + stopped
				: "";

			// What a defect costs at the size an attacker sends: the time at the largest size carried out along the exponent to 64 KiB of input.
			var projected = hung || exponent >= RefusalExplosive ? "explosive"
				: length == 0 || double.IsNaN(exponent) ? "-"
				: Duration(last.Ns * Math.Pow(65536.0 / length, Math.Max(exponent, 1)));

			if (flag.Length > 0 && !flag.StartsWith("large and linear", StringComparison.Ordinal))
				problems.Add($"{refusal.Parser}, {refusal.Shape}: {flag}" + (double.IsNaN(exponent) ? "" : $", time exponent {exponent.ToString("F2", CultureInfo.InvariantCulture)}, {projected} at 64 KiB"));

			Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
				$"| {refusal.Parser} | {refusal.Shape} | {refusal.Unit} | {(points.Count == 0 ? "-" : points[0].N.ToString("N0", CultureInfo.InvariantCulture) + " - " + last.N.ToString("N0", CultureInfo.InvariantCulture))} | {points.Count} | {length:N0} | " +
				$"{(double.IsNaN(last.Ns) ? "-" : Duration(last.Ns))} | {(double.IsNaN(exponent) ? "-" : exponent.ToString("F2", CultureInfo.InvariantCulture))} | {projected} | " +
				$"{(double.IsNaN(last.Bytes) ? "-" : (last.Bytes / 1024).ToString("F1", CultureInfo.InvariantCulture))} | {(double.IsNaN(allocation) ? "-" : allocation.ToString("F2", CultureInfo.InvariantCulture))} | {flag} |"));

			Console.Out.Flush();
		}

		Console.WriteLine();
		Console.WriteLine(problems.Count == 0
			? $"{series} series, none above the exponent {RefusalThreshold.ToString("F2", CultureInfo.InvariantCulture)}."
			: $"{series} series, {problems.Count} defective or faulty:\n" + string.Join("\n", problems.Select(static one => "- " + one)));
		Console.WriteLine();
		Console.WriteLine("Not covered by a refusal ladder:");

		foreach (var gap in RefusalGaps)
			Console.WriteLine("- " + gap);

		return problems.Count;
	}
}
