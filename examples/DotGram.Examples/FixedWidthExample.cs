using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// A fixed-width record file, where a field is found by counting rather than by looking
// for a delimiter:
//
//     000123ACME CORP       20260816USD000001250000
//     ├────┤├─────────────┤├──────┤├─┤├──────────┤
//      id    name           date    ccy  amount
//
// This is the format banks and exchanges still send, and it is a different problem from
// everything else in this folder: there is nothing between the fields to find. A parser
// that miscounts one column reads every later field wrong and usually still succeeds,
// which is why the widths belong in the grammar rather than in a loop with substring
// arithmetic in it.
//
// What it leans on:
//
//   * `Text(15)` and `Digits(6)` are one rule each, parameterized by width (§4.2) and
//     specialized per call — so `Digits(6)` and `Digits(12)` are separate recognizers
//     with the count compiled in, not a loop reading a variable;
//   * `{n}` is an exact count, so a short record fails at the field that ran out rather
//     than silently borrowing from the next line;
//   * the amount is minor units, which is what these formats carry, and the `=>` turns
//     it into a decimal once instead of at every use.
//
// The trailing spaces in a name are padding rather than data, and trimming them is a
// decision about this format that belongs with the format — so `Text` trims, and a rule
// that wants the padding uses `Raw`.

[Gram("""
	@using DotGram.Examples;
	@using System;
	@using System.Globalization;

	Trivia = none

	Feed : @Settlement[] = Row* & eof

	Row : @Settlement = id: Digits(6) & name: Text(20) & on: Date & currency: Raw(3)
	                  & amount: Digits(12) & eol
	                    => @(new Settlement(id, name, on, currency, amount / 100m))

	// Parameterized by width and specialized per call site: `Digits(6)` and `Digits(12)`
	// become two recognizers, each with its count compiled in.
	Digits(n) : @decimal = text: ['0'..'9']{n} => @(decimal.Parse(text, CultureInfo.InvariantCulture))

	// Padding is not data: a name is what is left when the spaces on the right go.
	Text(n)   : @string  = text: Raw(n) => @(text.TrimEnd())

	Raw(n)    : @string  = text: any{n} => @(text)

	Date : @DateOnly = text: ['0'..'9']{8}
	                     => @(DateOnly.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture))

	parse Feed
	""")]
public sealed partial class FixedWidth
{
	/// <summary>Reads a whole file of settlement records.</summary>
	public static IReadOnlyList<Settlement> Read(string text) => ParseFeed(text);
}

/// <summary>One record, with its fields already the types they mean.</summary>
public sealed record Settlement(
	decimal  Id,
	string   Name,
	DateOnly On,
	string   Currency,
	decimal  Amount);
