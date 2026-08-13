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
	match Header
	find all Row as AllRows
	find Trailer
	""")]
public static partial class FeedReader
{
	// ParseFeed, MatchHeader, AllRows, FindTrailer and the types Header, Row, Trailer
	// and Date are generated into this class.

	/// <summary>Reads a feed, refusing anything malformed.</summary>
	/// <exception cref="FormatException">
	/// A missing header or trailer, a bad record, anything after the trailer, or a
	/// declared count that does not match the records.
	/// </exception>
	public static Feed Read(string text)
	{
		// Checks what no per-record reader can: exactly one header, a trailer, and
		// nothing after it. Nothing is read out of the result — its job is to refuse.
		ParseFeed(text);

		var header  = MatchHeader(text)!;
		var trailer = FindTrailer(text)!;
		var trades  = ReadRecords(text);

		if (Number(trailer.Count) != trades.Count)
			throw new FormatException(
				$"The trailer declares {trailer.Count} records and the feed has {trades.Count}.");

		return new Feed(ToDate(header.Date), header.Source, trades);
	}

	/// <summary>Every record, whether or not the feed as a whole is well formed.</summary>
	public static IReadOnlyList<Trade> ReadRecords(string text)
	{
		var trades = new List<Trade>();

		foreach (var row in AllRows(text))
			trades.Add(new Trade(row.Symbol, Number(row.Qty), ToDate(row.Date)));

		return trades;
	}

	// The grammar has already said these are digits, so neither can fail.
	static int Number(string digits) => int.Parse(digits, CultureInfo.InvariantCulture);

	static DateOnly ToDate(Date date) =>
		new(Number(date.Year), Number(date.Month), Number(date.Day));
}
