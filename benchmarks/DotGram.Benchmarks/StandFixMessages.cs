using System;
using System.Collections.Generic;

using DotGram.Finance.Fix;

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

	static IEnumerable<Workload> FixMessageWorkloads()
	{
		var wire = FixMessageWire();

		yield return new Workload(
			"fixmsg",
			"Order.parse",
			[new Reading("generated", () => FixMessages.Parse(wire) is null ? 0 : 1)],
			() => FixMessages.TryParse(wire, out _, out var error) ? null : $"  the strict layer refuses the wire: {error}");

		yield return new Workload(
			"fixmsg",
			"Order.build",
			[new Reading("generated", () => FixMessages.Build(wire, [.. FixParser.Parse(wire)]) is null ? 0 : 1)],
			() => FixMessages.Build(wire, [.. FixParser.Parse(wire)]) is null ? "  Build returned nothing" : null);
	}
}
