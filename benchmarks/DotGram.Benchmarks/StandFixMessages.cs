using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Finance.Fix44;

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
	static readonly Lazy<FixContext> LoadedContext = new(static () => FixContext.Default.Load([
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
		// Validate(FixContext.Default) and theirs through DataDictionary.Validate. Until 2026-09-22 the generated
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
				new Reading("generated", () => FixParser.TryParseMessage(agreed, out var message, out _, null) && message!.Validate(FixContext.Default) ? 1 : 0),
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
}
