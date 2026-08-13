using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
public sealed record Feed(DateOnly Date, string Source, IReadOnlyList<Trade> Trades);

[Gram("""
	Feed    = Header & Row* & Trailer & eof

	Header  = "H" & '|' & date: Date & '|' & source: Text & eol
	Row     = "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol
	Trailer = "T" & '|' & count: Digit+ & eol

	Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	// Anything up to the next separator or the end of the line.
	Text    = [^ '|' | '\r' | '\n']+
	Digit   = ['0'..'9']

	parse Feed
	find Header
	find Row as AllRows
	find Trailer
	""")]
public static partial class FeedReader
{
	// ParseFeed, FindHeader, AllRows, FindTrailer and the types Header, Row, Trailer
	// and Date are generated into this class. `find` hands back a lazy sequence, so
	// picking one out of it is LINQ's job rather than another directive's.

	/// <summary>
	/// Reads a feed, refusing anything malformed.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Four passes over the text, and the text has to be a string.</b> Checking the
	/// shape and reading the records are separate directives today, and there is nothing
	/// to join them by — so the feed is parsed once to be refused, and scanned again for
	/// each part of it. Fine for a feed of a few megabytes and wrong for one of a few
	/// gigabytes, which cannot be a string at all.
	/// </para>
	/// <para>
	/// Use <see cref="ReadRecords"/> instead when the feed is large: one pass, and no
	/// check of the envelope. What replaces both is <c>Feed : FeedItem[]</c> with
	/// <c>recover</c> over a <c>TextReader</c> — one pass, memory bounded by a line,
	/// header, records and trailer arriving in order (docs/syntax.md §8). This method
	/// becomes a <c>foreach</c> when it lands.
	/// </para>
	/// </remarks>
	/// <exception cref="FormatException">
	/// A missing header or trailer, a bad record, anything after the trailer, or a
	/// declared count that does not match the records.
	/// </exception>
	public static Feed Read(string text)
	{
		// Checks what no per-record reader can: exactly one header, a trailer, and
		// nothing after it. Nothing is read out of the result — its job is to refuse.
		ParseFeed(text);

		// Safe only because of the line above: these are the first line shaped like a
		// header and the first shaped like a trailer, which are the header and the trailer
		// only in a feed already known to have exactly one of each.
		var header  = FindHeader(text).First().Value!;
		var trailer = FindTrailer(text).First().Value!;
		var trades  = ReadRecords(text);

		if (Number(trailer.Count) != trades.Count)
			throw new FormatException(
				$"The trailer declares {trailer.Count} records and the feed has {trades.Count}.");

		return new Feed(ToDate(header.Date), header.Source, trades);
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
