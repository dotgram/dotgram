using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// A line-oriented feed: one header, any number of records, one trailer.
//
//     H|2026-08-13|ACME
//     R|AAPL|100|2026-08-12
//     T|1
//
// Every rule that carries captures becomes a type, and a capture of such a rule holds
// its value rather than its text — so row.Date.Year is three rules deep and still
// checked by the compiler.

/// <summary>One record, in the shape the rest of an application wants.</summary>
public sealed record Trade(string Symbol, int Quantity, DateOnly TradedOn);

/// <summary>A whole feed, read and checked.</summary>
public sealed record TradeFile(DateOnly Date, string Source, IReadOnlyList<Trade> Trades);

[Gram("""
	Feed    = header: Header & rows: Row* & trailer: Trailer & eof

	Header  = "H" & '|' & date: Date & '|' & source: Text & eol
	Row     = "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol
	Trailer = "T" & '|' & count: Digit+ & eol

	Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	// Anything up to the next separator or the end of the line.
	Text    = [^ '|' | '\r' | '\n']+
	Digit   = ['0'..'9']

	parse Feed
	find Row as AllRows
	""")]
public static partial class FeedReader
{
	// ParseFeed, AllRows and the types Feed, Header, Row, Trailer and Date are generated
	// into this class. `rows: Row*` is a repeated capture of a rule that builds, so it
	// comes back as Row[].

	/// <summary>
	/// Reads a feed, refusing anything malformed.
	/// </summary>
	/// <remarks>
	/// One pass. <c>parse</c> checks what no per-record reader can — exactly one header,
	/// a trailer, nothing after it — and the captures hand back everything it saw on the
	/// way, so nothing is scanned twice. The text still has to be a string; reading a
	/// feed too large for one is <c>recover</c> over a <c>TextReader</c>, docs/syntax.md
	/// §8.
	/// </remarks>
	/// <exception cref="FormatException">
	/// A missing header or trailer, a bad record, anything after the trailer, or a
	/// declared count that does not match the records.
	/// </exception>
	public static TradeFile Read(string text)
	{
		var feed   = ParseFeed(text);
		var trades = new List<Trade>();

		foreach (var row in feed.Rows)
			trades.Add(new Trade(row.Symbol, Number(row.Qty), ToDate(row.Date)));

		// The one check the grammar cannot make today: `when @(count == rows.Length)` is
		// a guard over a value, and guards over values are docs/syntax.md §8.1.
		if (Number(feed.Trailer.Count) != trades.Count)
			throw new FormatException(
				$"The trailer declares {feed.Trailer.Count} records and the feed has {trades.Count}.");

		return new TradeFile(ToDate(feed.Header.Date), feed.Header.Source, trades);
	}

	/// <summary>
	/// Every record, whether or not the feed as a whole is well formed. One pass.
	/// </summary>
	/// <remarks>
	/// It cannot say what it skipped: <c>find</c> passes over anything that is not a
	/// record without a word, so a line broken in the middle is indistinguishable from a
	/// blank one. Saying which line was wrong and why, and going on, is <c>recover</c>
	/// (docs/syntax.md §8.2).
	/// </remarks>
	public static IReadOnlyList<Trade> ReadRecords(string text)
	{
		var trades = new List<Trade>();

		foreach (var found in AllRows(text))
		{
			var row = found.Value!;

			trades.Add(new Trade(row.Symbol, Number(row.Qty), ToDate(row.Date)));
		}

		return trades;
	}

	// The grammar has already said these are digits, so neither can fail.
	static int Number(string digits) => int.Parse(digits, CultureInfo.InvariantCulture);

	static DateOnly ToDate(Date date) =>
		new(Number(date.Year), Number(date.Month), Number(date.Day));
}
