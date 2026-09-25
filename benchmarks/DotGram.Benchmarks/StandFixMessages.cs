using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Fix42 = DotGram.Finance.Fix.Fix42;
using Fix50 = DotGram.Finance.Fix.Fix50;

namespace DotGram.Benchmarks;

// The FIX message layer (finance-24, 2026-09-18): FixMessages.Parse of a NewOrderSingle, and
// FixMessages.Build over the fields FixParser.Parse read from it, the first thing a caller of the
// schema-checked layer does. There is no hand-written message layer, so the plain stand has one
// reading of each; the paired stand holds a build against another, with this tree's own layer as
// the control the hand parser is elsewhere.

static partial class Stand
{
	/// <summary>
	/// A NewOrderSingle the strict layer accepts: the stand's Order wire with the HandlInst it requires
	/// (21), BodyLength (9) counting what follows it up to the checksum, and the checksum (10) the sum of
	/// every byte before it, modulo 256.
	/// </summary>
	static string FixMessageWire()
	{
		var body   = "35=D\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u000111=ORDER\u000121=1\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u0001";
		var header = $"8=FIX.4.4\u00019={body.Length}\u0001";
		var sum    = 0;

		foreach (var character in header + body)
			sum += character;

		return $"{header}{body}10={sum % 256:D3}\u0001";
	}

	/// <summary>
	/// A message both this layer and QuickFIX/n accept (finance-24, D26): framed correctly, no tag repeated outside a group and no data field, so
	/// that their validation and ours can be asked the same question. BodyLength and CheckSum are computed, not written by hand: theirs checks both,
	/// and wrong arithmetic would read as a disagreement about parsing.
	/// </summary>
	static string FixAgreedWire()
	{
		var fields = "35=D|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|11=ORDER123|21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|".Replace('|', (char)1);
		var head   = "8=FIX.4.4" + (char)1 + "9=" + fields.Length + (char)1;
		var sum    = 0;

		foreach (var octet in System.Text.Encoding.Latin1.GetBytes(head + fields))
			sum += octet;

		return head + fields + "10=" + (sum % 256).ToString("D3", System.Globalization.CultureInfo.InvariantCulture) + (char)1;
	}

	/// <summary>
	/// A framed message of exactly <paramref name="fields"/> fields (finance-24, D52): BeginString, BodyLength, MsgType (3, Reject, which has Text as a plain field, where News has it inside a group), then the Text tag repeated, then CheckSum, so
	/// four fixed fields and the rest a repeated tag, which only the lenient mode accepts (the strict one refuses a duplicate in an area). BodyLength and CheckSum are
	/// computed. The list that doubles is the reader's stack, and its top is the largest AREA, the body: N - 4 fields, so it doubles as N - 4 passes 8, 16, 32 and 64, and the sizes 12/13, 20/21, 36/37 and 68/69 straddle those (finance-24, who chose them after finding that the total count would have hit the middles of the steps).
	/// </summary>
	static string FixFramedFields(int fields)
	{
		var body = "35=3" + (char)1 + string.Concat(Enumerable.Range(0, fields - 4).Select(static i => "58=text" + i + (char)1));
		var head = "8=FIX.4.4" + (char)1 + "9=" + body.Length + (char)1;
		var sum  = 0;

		foreach (var octet in System.Text.Encoding.Latin1.GetBytes(head + body))
			sum += octet;

		return head + body + "10=" + (sum % 256).ToString("D3", System.Globalization.CultureInfo.InvariantCulture) + (char)1;
	}

	static readonly Lazy<QuickFix.DataDictionary.DataDictionary> QuickFixDictionary = new(static () => new QuickFix.DataDictionary.DataDictionary(System.IO.Path.Combine(AppContext.BaseDirectory, "FIX44.xml")));

	// The same file loaded over this package's own schema, once, with the errata that puts back the places it
	// departs from FIX 4.4: what a consumer who holds a venue's dictionary validates with. Its checks are the
	// expression language's, compiled at the load, so this reading is what that road costs against the
	// compiled-in one beside it.
	static readonly Lazy<Fix44Context> LoadedContext = new(static () => Fix44Context.Default.Load([
		System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "FIX44.xml")),
		System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "quickfixn-fix44-errata.xml"))]));

	/// <summary>Whether QuickFIX/n accepts the wire as a session would take it: parsed with the dictionary and validated against it.</summary>
	/// <summary>Whether QuickFIX/n reads the wire at all: parsed with the dictionary, BodyLength and CheckSum checked, and not held to the schema.</summary>
	static bool QuickFixParses(string wire)
	{
		try
		{
			var dictionary = QuickFixDictionary.Value;
			var message    = new QuickFix.Message();

			message.FromString(wire, true, dictionary, dictionary, new QuickFix.FIX44.MessageFactory());

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	static bool QuickFixAccepts(string wire)
	{
		try
		{
			var dictionary = QuickFixDictionary.Value;
			var message    = new QuickFix.Message();

			message.FromString(wire, true, dictionary, dictionary, new QuickFix.FIX44.MessageFactory());
			QuickFix.DataDictionary.DataDictionary.Validate(message, dictionary, dictionary, "FIX.4.4", message.Header.GetString(35));

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	static IEnumerable<Workload> FixMessageWorkloads()
	{
		var wire = FixMessageWire();
		var agreed = FixAgreedWire();

		// The message layer's slope: FixMessages.TryParse, lenient mode, over messages of 8 to 132 fields, one row a size, with the bytes a call (D52).
		foreach (var fields in new[] { 8, 12, 13, 20, 21, 36, 37, 68, 69, 132 })
		{
			var framed = FixFramedFields(fields);

			yield return new Workload(
				"fixmsg",
				$"slope-{fields}",
				[new Reading("generated", () => FixParser.TryParseMessage(framed, out _, out _, null) ? 1 : 0)],
				() => FixParser.TryParseMessage(framed, out _, out var error, null) ? null : $"  the reader refuses the {fields}-field message: {error}");
		}

		// The message layer against QuickFIX/n on a message both accept. The second reading is a reference: another library's reader, doing the work a
		// session needs (a dictionary, BodyLength and CheckSum checked). It says how fast that reader is here, and not what this grammar costs against a hand
		// reading; there is no field row, since QuickFIX/n's message is a sorted map that drops the order of the wire and folds a repeated tag.
		// Two rows, so that each side is read doing the same work as the other. `.parse` is the reading of the wire
		// alone, on both sides with the envelope checked; `.strict` is that and then the schema, ours through
		// Validate(Fix44Context.Default) and theirs through DataDictionary.Validate. Until 2026-09-22 the generated
		// reading of `.strict` was the parse alone against their parse and check, and read as 2.5x when it was not one.
		yield return new Workload(
			"fixmsg",
			"Order44.parse",
			[
				new Reading("generated", () => FixParser.TryParseMessage(agreed, out _, out _, null) ? 1 : 0),
				new Reading("reference-QuickFIXn", () => QuickFixParses(agreed) ? 1 : 0),
			],
			() => FixParser.TryParseMessage(agreed, out _, out var error, null)
				? (QuickFixParses(agreed) ? null : "  the message layer reads the wire and QuickFIX/n does not")
				: $"  the message layer refuses the wire: {error}");

		yield return new Workload(
			"fixmsg",
			"Order44.strict",
			[
				new Reading("generated", () => FixParser.TryParseMessage(agreed, out var message, out _, null) && message!.Validate(Fix44Context.Default) ? 1 : 0),
				new Reading("generated-loaded", () => FixParser.TryParseMessage(agreed, out var message, out _, null) && message!.Validate(LoadedContext.Value) ? 1 : 0),
				new Reading("reference-QuickFIXn", () => QuickFixAccepts(agreed) ? 1 : 0),
			],
			() => FixParser.TryParseMessage(agreed, out _, out var error, null)
				? (QuickFixAccepts(agreed) ? null : "  the message layer accepts the wire and QuickFIX/n does not")
				: $"  the message layer refuses the wire: {error}");

		yield return new Workload(
			"fixmsg",
			"Order.parse",
			[new Reading("generated", () => FixParser.ParseMessage(wire) is null ? 0 : 1)],
			() => FixParser.TryParseMessage(wire, out _, out var error) ? null : $"  the strict layer refuses the wire: {error}");

		yield return new Workload(
			"fixmsg",
			"Order.build",
			[new Reading("generated", () => FixParser.BuildMessage(wire, [.. FixParser.ParseFields(wire)]) is null ? 0 : 1)],
			() => FixParser.BuildMessage(wire, [.. FixParser.ParseFields(wire)]) is null ? "  Build returned nothing" : null);
	}

	// ── The other two versions: FIX 4.2 and FIX 5.0 SP2 ─────────────────────────
	//
	// Each version is its own generated reader over its own schema, and nothing about the 4.4 rows
	// above says what they cost: 4.2 writes its allocations as a group inline where 4.4 has a
	// component, and 5.0 carries the application's version in the header and reads zoned values.
	// A row's name says WHICH DOOR it goes through -- `parse-string` and not `parse` -- because a
	// row that does not name its form cannot be held to a control of the same form, which is how
	// three fixmsg rows of the paired stand came to be divided by a door they never called. The
	// 4.4 rows keep their older names so that the results already filed still read against them.

	/// <summary>
	/// A framed message of a version that writes its own BeginString: the fields of <paramref name="body"/>
	/// under a standard header, BodyLength (9) counting what follows it up to the checksum, and CheckSum
	/// (10) the sum of every octet before it, modulo 256. The framing of each version's own tests, computed
	/// and not written by hand: a wrong envelope is a finding of validation, so a hand-written one would
	/// read as the schema refusing the message.
	/// </summary>
	static string FixVersionWire(string beginString, string type, string body)
	{
		var fields = ("35=" + type + "|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|" + body).Replace('|', (char)1);
		var head   = "8=" + beginString + (char)1 + "9=" + fields.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) + (char)1;
		var sum    = 0;

		foreach (var octet in System.Text.Encoding.Latin1.GetBytes(head + fields))
			sum += octet;

		return head + fields + "10=" + (sum % 256).ToString("D3", System.Globalization.CultureInfo.InvariantCulture) + (char)1;
	}

	// The order of Fix42Tests: HandlInst is required of a 4.2 order, the quantity is OrderQty's, and the
	// allocations are a group written inline rather than a component.
	const string Order42Body = "11=ORD-1|21=1|55=IBM|54=1|60=20260920-12:00:00|38=300|40=2|44=101.25|78=2|79=ACC-A|80=100|79=ACC-B|80=200|";

	// The order of Fix50Tests: the application's version in the header (1128), the instrument with a
	// maturity time at an offset (1079), and the parties as a group of a component (453).
	const string Order50Body = "1128=9|11=ORD-1|55=IBM|541=20270115|1079=13:09+05:30|54=1|60=20260920-12:00:00|38=300|40=2|44=101.25|" +
		"453=2|448=BROKER|447=D|452=1|448=CLIENT|447=D|452=3|";

	/// <summary>
	/// One side of the deep report: Side, the group's delimiter, and then two parties, each carrying a
	/// sub-party — <c>NoPartySubIDs</c> inside <c>Parties</c> inside <c>NoSides</c>, three levels of the
	/// reader's stack where an order's parties are one. A walk that costs a level at a time shows here and
	/// not in a flat row.
	/// </summary>
	static string Report50Side(string side)
	{
		return "54=" + side + "|453=2|448=BROKER|447=D|452=1|802=1|523=SUB-A|803=1|448=CLIENT|447=D|452=3|802=1|523=SUB-B|803=2|";
	}

	/// <summary>
	/// A TradeCaptureReport of FIX 5.0 SP2 with two sides, each carrying parties and sub-parties. What the
	/// schema requires of it: the instrument (55), LastQty (32), LastPx (31) and NoSides (552); of a side,
	/// Side (54). The rest is what a report of a trade says.
	/// </summary>
	static string Report50Body()
	{
		return "571=TRD-1|487=0|570=N|55=IBM|32=100|31=101.25|75=20260920|60=20260920-12:00:00|552=2|" + Report50Side("1") + Report50Side("2");
	}

	static IEnumerable<Workload> FixVersionWorkloads()
	{
		var order42  = FixVersionWire("FIX.4.2", "D", Order42Body);
		var order50  = FixVersionWire("FIXT.1.1", "D", Order50Body);
		var report50 = FixVersionWire("FIXT.1.1", "AE", Report50Body());

		// Parse: the string door alone, reading the wire into its class and holding it to no schema. One
		// for a message read and zero for none, which is what the stand reads as an acceptance.
		yield return new Workload(
			"fixmsg",
			"Order42.parse-string",
			[new Reading("generated", () => Fix42.FixParser.TryParseMessage(order42, out _, out _, null) ? 1 : 0)],
			() => Fix42.FixParser.TryParseMessage(order42, out _, out var error, null) ? null : $"  the FIX 4.2 reader refuses the wire: {error}");

		yield return new Workload(
			"fixmsg",
			"Order50.parse-string",
			[new Reading("generated", () => Fix50.FixParser.TryParseMessage(order50, out _, out _, null) ? 1 : 0)],
			() => Fix50.FixParser.TryParseMessage(order50, out _, out var error, null) ? null : $"  the FIX 5.0 reader refuses the wire: {error}");

		yield return new Workload(
			"fixmsg",
			"Report50.parse-string",
			[new Reading("generated", () => Fix50.FixParser.TryParseMessage(report50, out _, out _, null) ? 1 : 0)],
			() => Fix50.FixParser.TryParseMessage(report50, out _, out var error, null) ? null : $"  the FIX 5.0 reader refuses the deep report: {error}");

		// Parse and then hold to the schema, the two acts a session performs. The check names the findings,
		// since a run that only knew the message was invalid would say nothing about which rule it broke.
		yield return new Workload(
			"fixmsg",
			"Order42.strict-string",
			[new Reading("generated", () => Fix42.FixParser.TryParseMessage(order42, out var message, out _, null) && message!.Validate(Fix42.Fix42Context.Default) ? 1 : 0)],
			() => Refused(Fix42.FixParser.ParseMessage(order42), static one => one.Validate(Fix42.Fix42Context.Default), static one => one.InvalidFindings));

		yield return new Workload(
			"fixmsg",
			"Order50.strict-string",
			[new Reading("generated", () => Fix50.FixParser.TryParseMessage(order50, out var message, out _, null) && message!.Validate(Fix50.Fix50Context.Default) ? 1 : 0)],
			() => Refused(Fix50.FixParser.ParseMessage(order50), static one => one.Validate(Fix50.Fix50Context.Default), static one => one.InvalidFindings));

		yield return new Workload(
			"fixmsg",
			"Report50.strict-string",
			[new Reading("generated", () => Fix50.FixParser.TryParseMessage(report50, out var message, out _, null) && message!.Validate(Fix50.Fix50Context.Default) ? 1 : 0)],
			() => Refused(Fix50.FixParser.ParseMessage(report50), static one => one.Validate(Fix50.Fix50Context.Default), static one => one.InvalidFindings));

		// Build over the fields a reader has already found, the form 4.4 has as `Order.build`: the message
		// layer's own work with the tokenizing paid for outside the timing.
		yield return new Workload(
			"fixmsg",
			"Order42.build-string",
			[new Reading("generated", () => Fix42.FixParser.BuildMessage(order42, [.. Fix42.FixParser.ParseFields(order42)]) is null ? 0 : 1)],
			() => Fix42.FixParser.BuildMessage(order42, [.. Fix42.FixParser.ParseFields(order42)]) is null ? "  Build returned nothing for the FIX 4.2 order" : null);

		yield return new Workload(
			"fixmsg",
			"Order50.build-string",
			[new Reading("generated", () => Fix50.FixParser.BuildMessage(order50, [.. Fix50.FixParser.ParseFields(order50)]) is null ? 0 : 1)],
			() => Fix50.FixParser.BuildMessage(order50, [.. Fix50.FixParser.ParseFields(order50)]) is null ? "  Build returned nothing for the FIX 5.0 order" : null);
	}

	/// <summary>
	/// Null where the message is valid; otherwise every finding of the schema. A check that said only
	/// "invalid" would leave a failing run to be diagnosed by hand, and the findings are the one thing
	/// this layer has to say about a message it refuses.
	/// </summary>
	static string? Refused<TMessage>(TMessage message, Func<TMessage, bool> validate, Func<TMessage, List<FixFinding>?> findings)
	{
		return validate(message) ? null : "  the schema refuses the wire: " + string.Join("; ", findings(message) ?? []);
	}
}
