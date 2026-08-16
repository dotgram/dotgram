using System;
using System.Collections.Generic;

using DotGram;

namespace DotGram.Examples;

// A CSV read into records, with no `=>` anywhere in the grammar.
//
//     TypedCsv.Read("AAPL,100,2026-08-12\nMSFT,250,2026-08-13\n")
//
// Every other example here says how to build its values. This one does not need to:
// §7.3 matches the captures to the type the rule declared, by name, and the shape of
// that type decides how. `Trade` has a constructor its captures cover, so the
// constructor is called; `Session` has none and says `required` instead, so it is made
// and written into. Neither is anything the grammar mentions.
//
// What that removes is the line nobody enjoys writing — `=> @(new Trade(symbol, size,
// on))` — which repeats in the grammar what the C# type already says, and goes stale
// the moment a parameter is added. What it costs is that the names have to agree: a
// capture called `size` fills a parameter called `size`, and renaming one without the
// other is a compile error rather than a silent mismatch.
//
// A rule that wants something else keeps saying so. `Amount` and `Day` build values of
// their own, because turning text into an int or a date is a conversion rather than a
// construction, and §7.3's matching has nothing to do with it.

[Gram("""
	@using DotGram.Examples;
	@using System;
	@using System.Globalization;

	Trivia = none

	Feed : @TradeRow[] = Trade* & eof

	// No `=>`: `Trade` has a constructor these three cover (§7.3).
	Trade : @TradeRow = symbol: Symbol & ',' & size: Amount & ',' & on: Day & eol

	// Nor here: `Session` has no constructor to fill, and two `required` properties that
	// these captures do fill.
	Session : @Session = opened: Day & '/' & closed: Day & eol

	Symbol : @string = text: ['A'..'Z']+ => @(text)

	Amount : @int = digits: ['0'..'9']+ => @(int.Parse(digits, CultureInfo.InvariantCulture))

	Day : @DateOnly = text: (['0'..'9'] | '-')+
	                    => @(DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture))

	parse Feed
	parse Session
	""")]
public sealed partial class TypedCsv
{
	/// <summary>Reads a whole feed of trades, or throws where it is not one.</summary>
	public static IReadOnlyList<TradeRow> Read(string text) => ParseFeed(text);

	/// <summary>Reads the one line that says what a session covered.</summary>
	public static Session ReadSession(string text) => ParseSession(text);
}

/// <summary>Built by its constructor, which the captures of <c>Trade</c> cover.</summary>
public sealed class TradeRow(string symbol, int size, DateOnly on)
{
	public string   Symbol { get; } = symbol;
	public int      Size   { get; } = size;
	public DateOnly On     { get; } = on;
}

/// <summary>Made and then written into: no constructor to fill, two properties that insist.</summary>
public sealed class Session
{
	public required DateOnly Opened { get; init; }
	public required DateOnly Closed { get; init; }

	/// <summary>Neither captured nor required, so it keeps what it was given.</summary>
	public string Note { get; init; } = "";
}
