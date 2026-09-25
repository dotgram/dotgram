using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Examples.Feeds;
using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;
using DotGram.Handwritten.Fix;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Fix42 = DotGram.Finance.Fix.Fix42;
using Fix50 = DotGram.Finance.Fix.Fix50;

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

		yield return PairedFormRow("sql", "select20.at", "hand", () => HandAccepts("TryParseQueryExpression", select20) ? 1 : 0,
			before.SqlPositional(false, "TryParseQueryExpression", positions, prefix.Length, null),
			after.SqlPositional(false, "TryParseQueryExpression", positions, prefix.Length, null));

		yield return PairedFormRow("sql", "select20.window", "hand", () => HandAccepts("TryParseQueryExpression", select20) ? 1 : 0,
			before.SqlPositional(false, "TryParseQueryExpression", window, prefix.Length, select20.Length),
			after.SqlPositional(false, "TryParseQueryExpression", window, prefix.Length, select20.Length));

		var insert = "INSERT INTO t (a, b) VALUES (1, 2), (3, 4), (5, 6)";

		yield return PairedFormRow("tsql", "insert-values.at", "scriptdom", () => ScriptDomAccepts(insert) ? 1 : 0,
			before.SqlPositional(true, "TryParseStatement", prefix + insert, prefix.Length, null),
			after.SqlPositional(true, "TryParseStatement", prefix + insert, prefix.Length, null));

		// ── the token scanner ──
		var conditions = SqlConditions(100);
		var ladder     = "(int x, int y) => (x + y) * 3 - x / 5";

		yield return PairedFormRow("sql", "select20.scan", "control", () => ScanCount(SqlStandardParser.Scan, select20), before.Scan("sql", select20), after.Scan("sql", select20));
		yield return PairedFormRow("sql", "conditions100.scan", "control", () => ScanCount(SqlStandardParser.Scan, conditions), before.Scan("sql", conditions), after.Scan("sql", conditions));
		yield return PairedFormRow("tsql", "select20.scan", "control", () => ScanCount(TransactSqlParser.Scan, select20), before.Scan("tsql", select20), after.Scan("tsql", select20));
		yield return PairedFormRow("el", "ladder.scan", "control", () => ScanCount(DotGram.ExpressionLanguage.ExpressionParser.Scan, ladder), before.Scan("el", ladder), after.Scan("el", ladder));

		// ── FixMessages: the streams, the readers, the lazy reading and a span ──
		//
		// THE THREE PARSE ROWS ANSWERED THE WRONG WAY ROUND, and that is what killed the paired
		// stand: the stand reads a non-zero answer as an acceptance, and these controls answered 0
		// for a valid message. The row was right when it was written -- `Parse(...) is null ? 0 : 1`,
		// one when a message came back -- became vacuous when it was ported to `Validate() is null`
		// (an array is never null, so it was always one), and became INVERTED on 2026-09-22 when
		// `Validate()` gave way to `InvalidFindings`, where null means valid. The shape of the
		// expression survived three rewrites and its meaning turned over inside it.
		//
		// EVERY CONTROL HERE IS ALSO THE FORM'S OWN SHAPE NOW, and it was not: all three parse forms
		// shared one control, a plain string parse, while their sides went through the streaming door
		// with a fresh source per call. A control is this process's own build of
		// the SAME form -- that is what the README promises and what makes it a constant the row can
		// be read against. A string parse holding a stream parse to account is a different code path
		// at a different scale: it can stand still while the row moves, and move while the row
		// stands still. The lazy rows had a second mismatch of the same kind: their control passed
		// 4096 where the door's third parameter is the largest message it will read, not a buffer
		// size, so the control was capped where the side was not.
		var wire = FixMessageWire();

		// Every reading of these rows answers ONE for a message read, and zero for none: the stand
		// reads a non-zero answer as an acceptance. Not "how many findings" -- that counts the other
		// way and is what broke them.
		foreach (var form in new[] { "parse-stream", "parse-reader", "parse-span" })
			yield return PairedFormRow("fixmsg", "Order." + form, "control", OwnFixMessagesForm(form, wire, 1), before.FixMessagesForm(form, wire, 1), after.FixMessagesForm(form, wire, 1));

		foreach (var form in new[] { "read-stream", "read-reader" })
			yield return PairedFormRow("fixmsg", "Order." + form + "100", "control", OwnFixMessagesForm(form, wire, 100), before.FixMessagesForm(form, wire, 100), after.FixMessagesForm(form, wire, 100));

		// ── FIX 4.2 and FIX 5.0 SP2: the string door, and the door with the schema ──
		//
		// Each version is its own generated reader, so a change to how a reader is written moves them and
		// nothing about the 4.4 rows says by how much. The report is the deep one: its sides carry parties and
		// its parties sub-parties, three levels of the walk where an order is one, which is where a change that
		// costs a level at a time shows. A side older than the versions has neither, and the rows are left out
		// rather than failing the pair.
		if (before.HasFixVersions && after.HasFixVersions)
		{
			var order42  = FixVersionWire("FIX.4.2", "D", Order42Body);
			var order50  = FixVersionWire("FIXT.1.1", "D", Order50Body);
			var report50 = FixVersionWire("FIXT.1.1", "AE", Report50Body());

			foreach (var (row, version, one) in new[] { ("Order42", "42", order42), ("Order50", "50", order50), ("Report50", "50", report50) })
				foreach (var form in new[] { "parse-string", "strict-string" })
					yield return PairedFormRow("fixmsg", row + "." + form, "control",
						OwnFixVersionForm(version, form, one), before.FixVersionForm(version, form, one), after.FixVersionForm(version, form, one));
		}

		// ── one span row for FIX ──
		var order = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";

		yield return PairedFormRow("fix", "Order.span", "hand", () => HandFixParser.Parse(order).Count(), before.FixSpan(order), after.FixSpan(order));

		// ── the lazy feed of the examples ──
		if (before.HasStock && after.HasStock)
		{
			var feed = "H|2026-08-13|ACME" + (char)10 + string.Concat(Enumerable.Repeat("R|AAPL|100|2026-08-12" + (char)10, 1000)) + "T|1000" + (char)10;

			yield return PairedFormRow("feeds", "streaming.1000", "control", () => StreamingFeedReader.Read(new StringReader(feed)).Count(), before.StreamingFeed(feed), after.StreamingFeed(feed));
		}
	}

	/// <summary>
	/// This process's own reading of one FIX 4.2 or FIX 5.0 SP2 form: the same door, the same wire, the same
	/// schema as the sides read by reflection. A control is the form itself, built here, so that a row is held
	/// to a constant doing its own work and not to another door at another scale.
	/// </summary>
	static Func<int> OwnFixVersionForm(string version, string form, string wire)
	{
		return (version, form) switch
		{
			("42", "parse-string")  => () => Fix42.FixParser.ParseMessage(wire) is null ? 0 : 1,
			("42", "strict-string") => () => Fix42.FixParser.ParseMessage(wire).Validate(Fix42.Fix42Context.Default) ? 1 : 0,
			("50", "parse-string")  => () => Fix50.FixParser.ParseMessage(wire) is null ? 0 : 1,
			("50", "strict-string") => () => Fix50.FixParser.ParseMessage(wire).Validate(Fix50.Fix50Context.Default) ? 1 : 0,
			_ => throw new ArgumentException($"no reading of this process's own FIX {version} {form}", nameof(form)),
		};
	}

	/// <summary>
	/// This process's own reading of one <c>fixmsg</c> form: the same door, the same kind of input,
	/// made fresh on every call the way the sides make theirs.
	/// </summary>
	/// <remarks>
	/// The source is built inside the returned delegate and not outside it, because a stream read to
	/// its end cannot be read again, and because that is what the sides do: a row whose control
	/// reused one exhausted stream would measure a refusal.
	/// </remarks>
	static Func<int> OwnFixMessagesForm(string form, string wire, int messages)
	{
		var text   = string.Concat(Enumerable.Repeat(wire, messages));
		var octets = System.Text.Encoding.Latin1.GetBytes(text);

		// ONE ON A VALID MESSAGE, NOT ZERO. The stand reads a reading's answer as work done:
		// Unasserted takes `Run() != 0` as "this reading accepted the input", so a row not named
		// "refused" must answer non-zero. These three controls answered the other way round from
		// 2026-09-22 (3e9a4293) until today, and the whole paired stand died on the first of them
		// before timing anything -- see the remarks above the rows.
		return form switch
		{
			"parse-stream" => () => FixParser.ReadMessage(new MemoryStream(octets, false)) is { InvalidFindings: null } ? 1 : 0,
			"parse-reader" => () => FixParser.ReadMessage(new StringReader(text))          is { InvalidFindings: null } ? 1 : 0,

			// The door takes no span, so a caller holding one copies it into a string first, and the
			// copy is part of the form -- the same sentence the side's own comment carries.
			"parse-span"   => () => FixParser.ParseMessage(wire.AsSpan().ToString())       is { InvalidFindings: null } ? 1 : 0,

			"read-stream"  => () => FixParser.ReadMessages(new MemoryStream(octets, false)).Count(static one => one.InvalidFindings is null),
			"read-reader"  => () => FixParser.ReadMessages(new StringReader(text)).Count(static one => one.InvalidFindings is null),

			_ => throw new ArgumentException($"no reading of this process's own {form}", nameof(form)),
		};
	}

	/// <summary>A row of a published form: the constant, and the two sides, each asked to give the same answer.</summary>
	static Workload PairedFormRow(string family, string name, string baseName, Func<int> hand, Func<int> before, Func<int> after)
	{
		return new(family, name,
			[new Reading(baseName, hand), new Reading("before", before), new Reading("after", after)],
			() =>
			{
				var h = hand();
				var b = before();
				var a = after();

				return h == b && b == a ? null : $"  {family}/{name}: the constant says {h}, before {b}, after {a}";
			});
	}
}
