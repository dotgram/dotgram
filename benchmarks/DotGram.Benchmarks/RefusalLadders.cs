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

namespace DotGram.Refusals;

// The linearity of a refusal (architect, D57, 2026-09-20). The accepted ladders of the stand say how a parser reads a long input it takes; a parser can
// read that in a straight line and still have a shape in its grammar, `(A+)*` over an atom, that a REFUSAL after a long head walks in every way the head
// can be cut: 2^(n-1) of them, 0.03 ms at eight characters and 3.2 s at twenty-four. Two of those shipped (a URI template with an unclosed `{` after a
// literal run, an address list with a bare `@` after an atom run, 107 s at twenty-eight characters) because nothing timed a refusal at more than one
// length. A refusal comes from outside and is sent by anyone, so its bar is the harder one: an exponent above 1.10 is a defect, where an accepted input
// is held to 1.2.
//
// A series is a head that grows (the same units the accepted ladder grows by: a run of literal characters, a list, a chain of terms, records) and a tail
// no reader can finish (an unclosed `{`, `<`, `"`, `(`, a dangling operator, a bare `@`, a cut record), so that the input is refused after the head is
// read. The series asserts that it IS refused: a series whose input is accepted checks nothing, and is reported as a fault of the series and not counted
// as a pass. Note that a CONSTANT part of a head can itself be the run that explodes: the first version began a series with a 24-character literal prefix.
//
// The budget is what makes it a guard and not a wait. Sizes grow by a quarter, so that a doubling for every character rises sixteen-fold a step and not
// sixty-five-thousand-fold; the first call at a size is a probe on a thread of its own that is given up on after two seconds; and a ladder ends at the
// first size whose call takes over 20 ms twice. The worst call a series makes is then a few times 20 ms, not the 100 s the defect it looks for would take.
//
// This file is written once, in the benchmarks, and read by two: the stand (`linearity-refused`, the audit, which prints every series) and
// DotGram.Tests.Slow (the guard, which holds each series to a baseline and never lets an explosion into it). Rough, in one process, without a window: the
// shape of a curve is asked for.

internal static class RefusalLadders
{
	internal static string Id(Series series) => series.Parser + " | " + series.Shape;

	/// <summary>
	/// The readings of the expression language the series run over. The public entry reads the tape, and that is all the slow suite can reach; the stand, which the language
	/// makes its internals visible to, adds the immediate reading ("immediate") before it lists the series, so that the audit covers both machines and the guard the tape.
	/// </summary>
	internal static List<(string Form, Func<string, bool> Accepts)> ExpressionForms { get; } =
	[
		("tape", static t => ExpressionParser.TryParse(t, typeof(RefusalLadders).Assembly).IsSuccess),
	];

	/// <summary>The exponent above which the time of a refusal is a defect.</summary>
	internal const double Threshold = 1.10;

	/// <summary>The exponent from which a refusal is called quadratic, and the one from which it is called explosive (a doubling for every character is far above it).</summary>
	internal const double Quadratic = 1.5;

	internal const double Explosive = 3;

	/// <summary>The call, in milliseconds, at which a ladder ends.</summary>
	internal const double BudgetMilliseconds = 20;

	/// <summary>The time a first call at a size is given, in milliseconds, before it is abandoned and the series is called explosive.</summary>
	internal const int WatchdogMilliseconds = 2000;

	/// <summary>The step, in bytes or time a unit of the head between two sizes a quarter apart, from which a series is said to have a cliff, and the rise over the last sixteenth from which it is said to be a slope.</summary>
	internal const double CliffFactor = 2.5;

	internal const double SlopeFactor = 1.5;

	internal sealed record Series(string Parser, string Shape, string Unit, int Largest, Func<int, string> Text, Func<string, bool> Accepts);

	static string Copies(string text, int count) => string.Concat(Enumerable.Repeat(text, count));

	internal static IEnumerable<Series> All()
	{
		var soh    = ((char)1).ToString();
		var quote  = "\"";
		var cut    = ((char)1).ToString();

		Series R(string parser, string shape, string unit, int largest, Func<int, string> text, Func<string, bool> accepts) => new(parser, shape, unit, largest, text, accepts);

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
		yield return R("JsonPatch", "operations, then a cut one", "operations", 4096, n => "[" + Copies("{" + quote + "op" + quote + ":" + quote + "remove" + quote + "," + quote + "path" + quote + ":" + quote + "/a" + quote + "},", n) + "{" + quote + "op" + quote + ":", static t => JsonPatchAccepts(t));

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
		yield return R("T-SQL statement", "nested parentheses, never closed (the closed form is as dear: a cost of nesting)", "levels", 300, n => "SELECT " + new string('(', n) + "1", static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "subqueries opened and not closed", "levels", 200, n => "SELECT * FROM " + Copies("(SELECT * FROM ", n) + "t", static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "CASE arms, no END", "arms", 4096, n => "SELECT CASE" + Copies(" WHEN 1 = 1 THEN 1", n), static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL statement", "rows, then a cut row", "rows", 4096, n => $"INSERT INTO t (a, b) VALUES {Rows(n)}, (1,", static t => TransactSqlParser.TryParseStatement(t).IsSuccess);
		yield return R("T-SQL search condition", "predicates, then a dangling AND", "predicates", 4096, n => Conditions(n) + " AND", static t => TransactSqlParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("T-SQL search condition", "predicates, then an unclosed (", "predicates", 4096, n => Conditions(n) + " AND (a = 1", static t => TransactSqlParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("T-SQL search condition", "nested parentheses, never closed (the closed form is as dear: a cost of nesting)", "levels", 300, n => new string('(', n) + "a = 1", static t => TransactSqlParser.TryParseSearchCondition(t).IsSuccess);

		yield return R("SQL:2023 query", "columns, then a FROM with nothing after it", "columns", 4096, n => $"SELECT {Names(n)} FROM", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "an unclosed string", "characters", 4096, n => "SELECT '" + new string('a', n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "an unclosed comment", "characters", 4096, n => "SELECT 1 /* " + new string('a', n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "an unclosed quoted identifier", "characters", 4096, n => "SELECT " + quote + new string('a', n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "nested parentheses, never closed (the closed form is as dear: a cost of nesting)", "levels", 300, n => "SELECT " + new string('(', n) + "1", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "subqueries opened and not closed", "levels", 200, n => "SELECT * FROM " + Copies("(SELECT * FROM ", n) + "t", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "CASE arms, no END", "arms", 4096, n => "SELECT CASE" + Copies(" WHEN 1 = 1 THEN 1", n), static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 query", "joins, then a JOIN with nothing after it", "joins", 4096, n => "SELECT * FROM t" + Copies(" JOIN t ON a = a", n) + " JOIN", static t => SqlStandardParser.TryParseQueryExpression(t).IsSuccess);
		yield return R("SQL:2023 search condition", "predicates, then a dangling AND", "predicates", 4096, n => Conditions(n) + " AND", static t => SqlStandardParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("SQL:2023 search condition", "predicates, then an unclosed (", "predicates", 4096, n => Conditions(n) + " AND (a = 1", static t => SqlStandardParser.TryParseSearchCondition(t).IsSuccess);
		yield return R("SQL:2023 search condition", "nested parentheses, never closed (the closed form is as dear: a cost of nesting)", "levels", 300, n => new string('(', n) + "a = 1", static t => SqlStandardParser.TryParseSearchCondition(t).IsSuccess);

		// ── the expression language: the public reading everywhere (the tape), and whatever machines a caller with access to the internals adds ──
		foreach (var (form, accepts) in ExpressionForms)
		{
			yield return R("expression, " + form, "terms, then a dangling +", "terms", 4096, n => "(int x) => x" + Copies(" + x", n) + " +", accepts);
			yield return R("expression, " + form, "parentheses opened and not closed", "levels", 300, n => "(int x) => " + new string('(', n) + "x", accepts);
			yield return R("expression, " + form, "an unclosed string", "characters", 4096, n => "(int x) => " + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an unclosed verbatim string", "characters", 4096, n => "(int x) => @" + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an unclosed interpolated string", "characters", 4096, n => "(int x) => $" + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an interpolated string, an unclosed hole", "characters", 4096, n => "(int x) => $" + quote + new string('a', n) + "{x", accepts);
			yield return R("expression, " + form, "an unclosed interpolated verbatim string", "characters", 4096, n => "(int x) => $@" + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an interpolated verbatim string, an unclosed hole", "characters", 4096, n => "(int x) => $@" + quote + new string('a', n) + "{x", accepts);
			yield return R("expression, " + form, "an unclosed raw interpolated string", "characters", 4096, n => "(int x) => $" + quote + quote + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "a raw interpolated string, an unclosed hole", "characters", 4096, n => "(int x) => $" + quote + quote + quote + new string('a', n) + "{x", accepts);
			yield return R("expression, " + form, "an unclosed raw interpolated string, two dollars", "characters", 4096, n => "(int x) => $$" + quote + quote + quote + new string('a', n), accepts);
			yield return R("expression, " + form, "an interpolated string ending in a backslash", "characters", 4096, n => "(int x) => $" + quote + new string('a', n) + "\\", accepts);
			yield return R("expression, " + form, "an interpolated string with holes, then no closing quote", "holes", 4096, n => "(int x) => $" + quote + Copies("a{x}", n), accepts);
			yield return R("expression, " + form, "an unclosed character literal", "characters", 4096, n => "(int x) => '" + new string('a', n), accepts);
			yield return R("expression, " + form, "member accesses, then a dangling .", "members", 4096, n => "(string s) => s" + Copies(".ToString()", n) + ".", accepts);
			yield return R("expression, " + form, "conditionals, then a cut one", "levels", 300, n => "(bool b) => " + Copies("b ? b : ", n), accepts);
		}
	}

	/// <summary>What the refusal ladders do not cover, printed under the table so that the list is read with the table and not lost.</summary>
	internal static readonly string[] Gaps =
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

		if (!thread.Join(WatchdogMilliseconds))
			return (false, false, watch.Elapsed.TotalMilliseconds);

		if (thrown is not null)
			ExceptionDispatchInfo.Capture(thrown).Throw();

		return (true, accepted, watch.Elapsed.TotalMilliseconds);
	}

	/// <summary>A time in nanoseconds as nanoseconds, microseconds, milliseconds, seconds or, past a day, years.</summary>
	internal static string Duration(double nanoseconds) => nanoseconds switch
	{
		< 1e3 => nanoseconds.ToString("F0", CultureInfo.InvariantCulture) + " ns",
		< 1e6 => (nanoseconds / 1e3).ToString("F1", CultureInfo.InvariantCulture) + " us",
		< 1e9 => (nanoseconds / 1e6).ToString("F1", CultureInfo.InvariantCulture) + " ms",
		< 8.64e13 => (nanoseconds / 1e9).ToString("F1", CultureInfo.InvariantCulture) + " s",
		_ => "over a day",
	};

	/// <summary>The sizes of a ladder: four, then a quarter more each step (at least one), up to the largest.</summary>
	static IEnumerable<int> Sizes(int largest)
	{
		for (var n = 4; n <= largest; n = Math.Max(n + 1, n * 5 / 4))
			yield return n;
	}

	/// <summary>The fastest of three batches of at least 8 ms and 2 calls: nanoseconds a call and the bytes it allocates.</summary>
	static (double Nanoseconds, double Bytes) Time(Func<bool> run)
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
	static double Exponent(List<(int N, double Ns, double Bytes)> points, Func<(int N, double Ns, double Bytes), double> value)
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

	static bool JsonPatchAccepts(string text)
	{
		try
		{
			return JsonPatch.Parse(text) is not null;
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>A name the stock count reads (letters only: an item called "item12" is a broken line), base 26.</summary>
	static string StockItem(int index)
	{
		var name = "";

		do
		{
			name = (char)('a' + index % 26) + name;
			index /= 26;
		}
		while (index > 0);

		return "item" + name;
	}

	internal enum Class
	{
		Linear,
		Superlinear,
		Quadratic,
		Explosive,
	}

	/// <summary>What one ladder found.</summary>
	internal sealed record Result(Series Series, List<(int N, double Ns, double Bytes)> Points, bool Budget, bool Hung, bool Faulty, string? Thrown, double Exponent, double Allocation, int Length)
	{
		/// <summary>The class of the time's exponent; a ladder that was abandoned is explosive whatever its points say.</summary>
		internal Class Class => Hung || Exponent >= Explosive ? Class.Explosive
			: Exponent >= Quadratic ? Class.Quadratic
			: Exponent > Threshold ? Class.Superlinear
			: Class.Linear;

		/// <summary>
		/// A cliff or a slope, told apart by the number every series already has (architect, D67): the bytes a call per unit of the head, and the time a call per unit.
		/// Flat and then a jump is a CLIFF (a bound somewhere in the parser, a pool that stops keeping what it built, a list that grows by doubling), and the fitted
		/// exponent over the last sixteenth of the ladder turns the two points above it into a slope that is not there; a steady rise is a SLOPE. The class of the series
		/// is not changed by this: a cliff arrives as a worse class and fails the guard, a cured one arrives as a better one. What it corrects is the name.
		/// </summary>
		internal string Shape => Describe("bytes", Points.Where(static p => p.N >= 32 && p.Bytes > 0).Select(static p => (p.N, p.Bytes / p.N)).ToList())
			+ "; " + Describe("time", Points.Where(static p => p.N >= 32).Select(static p => (p.N, p.Ns / p.N)).ToList());

		static string Describe(string what, List<(int N, double PerUnit)> series)
		{
			if (series.Count < 3)
				return what + " -";

			var jump = 0.0;
			var at   = 0;

			for (var i = 1; i < series.Count; i++)
			{
				var step = series[i].PerUnit / series[i - 1].PerUnit;

				if (step > jump)
				{
					jump = step;
					at   = i;
				}
			}

			if (jump >= CliffFactor)
				return what + string.Create(System.Globalization.CultureInfo.InvariantCulture, $" CLIFF x{jump:F1} between {series[at - 1].N:N0} and {series[at].N:N0}");

			var top  = series[^1].N;
			var tail = series.Where(p => p.N * 16L >= top).ToList();
			var rise = tail.Count < 2 ? 1.0 : tail[^1].PerUnit / tail[0].PerUnit;

			return what + (rise >= SlopeFactor ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $" rising x{rise:F1}") : " flat");
		}

		internal (int N, double Ns, double Bytes) Last => Points.Count == 0 ? (0, double.NaN, double.NaN) : Points[^1];
	}

	/// <summary>Walks one ladder under the budget and the watchdog. Nothing it meets ends the process: a call that hangs is abandoned and the series is explosive.</summary>
	internal static Result Run(Series series)
	{
		var points = new List<(int N, double Ns, double Bytes)>();
		var budget = false;
		var hung   = false;
		var faulty = false;
		var thrown = (string?)null;

		try
		{
			foreach (var n in Sizes(series.Largest))
			{
				var text = series.Text(n);
				var read = () => series.Accepts(text);

				// The first call at a size is the probe, on a thread of its own so that it can be given up on: a call that would take hours is not waited for.
				var (finished, took, first) = Probe(read);

				if (!finished)
				{
					points.Add((n, WatchdogMilliseconds * 1e6, 0));

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
				else if (first > BudgetMilliseconds)
				{
					// Twice: a first call at a new size can be a compile and not the algorithm.
					var (again, _, second) = Probe(read);

					if (!again || second > BudgetMilliseconds)
					{
						points.Add((n, again ? Math.Min(first, second) * 1e6 : WatchdogMilliseconds * 1e6, 0));

						budget = true;
						hung   = !again;

						break;
					}
				}

				var (time, bytes) = Time(read);

				points.Add((n, time, bytes));
			}
		}
		catch (Exception exception)
		{
			thrown = exception.GetType().Name;
		}

		var exponent   = points.Count < 2 ? double.NaN : Exponent(points, static p => p.Ns);
		var allocation = points.Count < 2 ? double.NaN : Exponent(points, static p => p.Bytes);
		var length     = points.Count == 0 ? 0 : series.Text(points[^1].N).Length;

		return new Result(series, points, budget, hung, faulty, thrown, exponent, allocation, length);
	}
}
