using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// The same feed again, for the caller who wants only the good records and the bad ones
// in a log — docs/syntax.md §8.3's fourth row, "successful records only, failures to a
// log", which declares nothing.
//
// RecoveringFeedExample gives its `recover` a `=>`, so a rejection becomes an element of
// the sequence and everything downstream has to be able to hold one: the records are
// `FeedLine` and not `Trade`, and every caller filters. That is the right shape when a
// rejection is data. It is the wrong shape when a rejection is an operational event —
// something to write to a log and count, not something to hand to the code that prices
// trades.
//
// So this one leaves the `=>` off. The broken record is dropped from the sequence, and
// what it was arrives at a `partial void` the generated class declares:
//
//     static partial void OnRecovered(
//         string rule, string text, int position, int line, int column, int ordinal, string message);
//
// Implement it and you are told about every one. Leave it alone and the compiler removes
// the declaration, every call to it, and everything in the argument lists — the text is
// never materialized, the line is never counted, and a feed of a hundred million records
// pays nothing at all for a channel nobody listens on. That erasure is why this is a
// classic `partial void` rather than an event, a delegate or an `ILogger`: those cost
// something even when null.
//
// The cost of it is that the hook is static and per class, so what it reports cannot be
// scoped to one call. Here the reports are gathered into a [ThreadStatic] list for the
// duration of a read, which is the ordinary way to make a static sink re-entrant enough
// for tests; a real application would write to its logger and not care.

/// <summary>A record the feed could not read, as an operational event rather than data.</summary>
public sealed record FeedRejection(string Rule, int Line, string Text, string Message)
{
	public override string ToString() =>
		$"line {Line.ToString(CultureInfo.InvariantCulture)}: {Message} — {Text}";
}

[Gram("""
	Feed    = header: Header & rows: Row* recover eol & trailer: Trailer & eof

	Header  = "H" & '|' & date: Date & '|' & source: Text & eol
	Trailer = "T" & '|' & count: Digit+ & eol
	Row     = "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol

	Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	Text    = [^ '|' | '\r' | '\n']+
	Digit   = ['0'..'9']

	parse Feed
	""")]
public static partial class LoggingFeedReader
{
	[ThreadStatic]
	static List<FeedRejection>? _rejected;

	/// <summary>
	/// Reads a feed, keeping the records it could read and reporting the rest.
	/// </summary>
	/// <remarks>
	/// The records come back as themselves — <c>Row[]</c>, with nothing in the array that
	/// is not a record — because the rejections left by another door.
	/// </remarks>
	/// <exception cref="FormatException">A missing header or trailer, or anything after it.</exception>
	public static (IReadOnlyList<Row> Rows, IReadOnlyList<FeedRejection> Rejected) Read(string text)
	{
		var reports = _rejected = [];

		try
		{
			return (ParseFeed(text).Rows, reports);
		}
		finally
		{
			_rejected = null;
		}
	}

	/// <summary>
	/// Told about every record the feed could not read (docs/syntax.md §8.3).
	/// </summary>
	/// <remarks>
	/// The declaration is generated; this is the other half of it. Delete this method and
	/// the parser stops paying for the channel entirely — that is the whole point of the
	/// mechanism, and it is why the generated side can afford to always be there.
	/// </remarks>
	static partial void OnRecovered(
		string rule, string text, int position, int line, int column, int ordinal, string message) =>
		_rejected?.Add(new FeedRejection(rule, line, text, message));
}
