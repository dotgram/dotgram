using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// FIX, the protocol every trading system speaks:
//
//     8=FIX.4.4|9=65|35=D|49=SENDER|56=TARGET|11=ORD1|55=AAPL|54=1|38=100|44=150.25|10=004|
//
// (`|` stands for SOH, 0x01, which is what actually separates the fields.)
//
// A message is `tag=value` repeated, which is a small grammar; what makes FIX itself is
// everything around that. The fields are ordered — `8` first, `9` second, `10` last —
// the body length and the checksum are computed over the bytes rather than over the
// values, and what a tag means depends on the message type. So the grammar reads the
// frame and the C# beside it says what the frame means:
//
//     var order = FixMessage.Read(text);
//
//     order.Type          // "D", a new order single
//     order["55"]         // "AAPL"
//     order.Symbol        // the same, by the name a trader uses
//     order.Price         // 150.25m, a decimal rather than a string
//     order.ChecksumHolds // whether tag 10 matches what the bytes say
//
// Two things worth looking at:
//
//   * a repeated capture of a rule that builds gives `FixField[]` (§7.3), so the
//     grammar hands over the fields in order — which matters, because FIX allows a tag
//     to repeat and the order is what tells one occurrence from another;
//   * the checksum is checked in C# and not in the grammar, deliberately. It is a sum
//     over every byte before tag 10, which a grammar cannot express and should not
//     pretend to — `parserText` hands the matched extent over and arithmetic happens
//     where arithmetic belongs.

[Gram("""
	@using DotGram.Examples;
	@using System.Globalization;

	Trivia = none

	// The frame, and nothing about what it means.
	Message : @FixMessage = whole: Body => @(new FixMessage(whole, parserText))

	Body : @FixField[] = Field+ & eof

	Field : @FixField = tag: Tag & '=' & value: Value & Soh
	                      => @(new FixField(tag, value))

	Tag   : @string = text: ['0'..'9']+ => @(text)

	// Everything up to the separator: a value may hold anything else, spaces and `=`
	// included, which is why the separator is a control character in the first place.
	Value : @string = text: [^ '\u0001' | '|']* => @(text)

	// Real FIX uses SOH (0x01); `|` is what every log and every example prints instead,
	// so both are read and neither is written into a value. Written `` rather than
	// pasted in: a control character in a source file is invisible to whoever reads it
	// next, survives a copy only by luck, and is the sort of thing an editor eats.
	Soh   = '\u0001' | '|'

	parse Message
	""")]
public sealed partial class FixParser
{
	public static FixMessage Read(string text) => ParseMessage(text);
}

/// <summary>One `tag=value`, in the order it arrived.</summary>
public sealed record FixField(string Tag, string Value);

/// <summary>
/// A message, with the fields both as they came and by the names a trader uses.
/// </summary>
public sealed class FixMessage(IReadOnlyList<FixField> fields, string raw)
{
	/// <summary>Every field, in order — a tag may repeat, and the order tells them apart.</summary>
	public IReadOnlyList<FixField> Fields { get; } = fields;

	/// <summary>The first value carried under this tag, or null.</summary>
	public string? this[string tag] =>
		Fields.FirstOrDefault(field => field.Tag == tag)?.Value;

	public string? Type     => this["35"];
	public string? Symbol   => this["55"];
	public string? Sender   => this["49"];
	public string? Target   => this["56"];

	public decimal? Price =>
		this["44"] is { } text && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
			? price
			: null;

	public decimal? Quantity =>
		this["38"] is { } text && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity)
			? quantity
			: null;

	/// <summary>
	/// Whether tag 10 is the sum FIX says it should be.
	/// </summary>
	/// <remarks>
	/// The sum of every byte before the checksum field, modulo 256, written as three
	/// digits. A grammar cannot say that — it is arithmetic over what was matched rather
	/// than a shape — so the extent comes across as text and the sum is taken here.
	/// </remarks>
	public bool ChecksumHolds
	{
		get
		{
			if (this["10"] is not { } stated)
				return false;

			var upto = raw.LastIndexOf("10=", StringComparison.Ordinal);

			if (upto < 0)
				return false;

			var sum = 0;

			for (var i = 0; i < upto; i++)
				sum += raw[i] == '|' ? 1 : raw[i];      // `|` stands in for SOH, which is 1

			return stated == (sum % 256).ToString("000", CultureInfo.InvariantCulture);
		}
	}
}
