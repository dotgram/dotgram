using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// The same feed as FeedExample, read so that one malformed record does not cost the
// file:
//
//     H|2026-08-13|ACME
//     R|AAPL|100|2026-08-12
//     R|MSFT|two hundred|2026-08-12    <- broken
//     R|NVDA|75|2026-08-11
//     T|3
//
// `recover eol` marks the repetition. Inside it, a record that starts and then fails is
// an error rather than the end of the sequence: the parser skips to the next end of
// line and reads on. The `=>` beside it turns what it skipped into an element of the
// same sequence, so a rejection arrives in its place, in order, carrying its own
// ordinal — and nothing has to be joined back up afterwards.
//
// That is why every record here is declared `: @FeedLine`: the good ones and the
// rejected ones have to fit the same array.

/// <summary>A line of the feed: either a record or the reason it is not one.</summary>
public abstract record FeedLine;

/// <summary>A record that was read.</summary>
public sealed record TradeLine(string Symbol, int Quantity, DateOnly TradedOn) : FeedLine;

/// <summary>A record that was not, and the text it was written on.</summary>
public sealed record RejectedLine(int Ordinal, int Position, string Text) : FeedLine;

[Gram("""
	@using DotGram.Examples;

	Feed    = header: Header
	        & lines:  Row* recover eol => @(new RejectedLine(ordinal, position, text.Trim()))
	        & trailer: Trailer & eof

	Header  = "H" & '|' & date: Date & '|' & source: Text & eol
	Trailer = "T" & '|' & count: Digit+ & eol

	Row : @FeedLine =
	          "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol
	       => @(new TradeLine(symbol, Number(qty), ToDate(date)))

	Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	// Anything up to the next separator or the end of the line.
	Text    = [^ '|' | '\r' | '\n']+
	Digit   = ['0'..'9']

	parse Feed
	""")]
public static partial class RecoveringFeedReader
{
	/// <summary>
	/// Reads a feed, keeping the records it could read and the lines it could not.
	/// </summary>
	/// <remarks>
	/// One pass, and one array. A caller that wants only the good records filters for
	/// <see cref="TradeLine"/>; a caller that wants to report the bad ones filters for
	/// <see cref="RejectedLine"/> and has, for each, which record it was and where in the
	/// file it started.
	/// </remarks>
	/// <exception cref="FormatException">
	/// A missing header or trailer, or anything after it. Recovery is for the records:
	/// the frame around them still has to be there.
	/// </exception>
	public static IReadOnlyList<FeedLine> Read(string text) => ParseFeed(text).Lines;

	/// <summary>Every line the feed did not yield a record for.</summary>
	public static IEnumerable<RejectedLine> Rejected(string text)
	{
		foreach (var line in Read(text))
			if (line is RejectedLine rejected)
				yield return rejected;
	}

	// Reachable from the grammar's `=>`, which becomes a method of this same class.
	static int Number(string digits) => int.Parse(digits, CultureInfo.InvariantCulture);

	static DateOnly ToDate(Date date) =>
		new(Number(date.Year), Number(date.Month), Number(date.Day));
}
