using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using DotGram;

namespace DotGram.Examples;

// The same feed once more, read from a `TextReader` instead of a string — the file may
// be larger than memory, and nothing here holds more than the record being read.
//
// Two things in the grammar make that possible, and neither is a directive: there is no
// notation for streaming, because §6.3 puts the choice at the call site. It is a property
// of the data, not of the grammar, so the same `parse Feed` serves both and the caller
// picks by which overload it calls.
//
//   Feed : @FeedPart[]        the result comes in parts. A stream can only hand over
//                             something that has parts, so this is what makes an
//                             `IEnumerable<FeedPart>` possible at all (§4.1 case 2).
//                             Header, records and trailer are all `FeedPart` and enter
//                             the sequence in the order they are read.
//
//   Row* recover eol          the repetition commits. Handing an element to the caller
//                             cannot be undone, so the parse may only read what the
//                             grammar says it will not go back past — and §8.2 makes a
//                             marked repetition possessive, which is exactly that.
//
// Leave either of them off and the reader overload is not generated, and the compiler
// says which one is missing and why.
//
// What the envelope is for shows up here in a way it does not over a string. A caller
// that chopped the file into lines and parsed each on its own would have to check the
// header, count the records and match the trailer itself; here the grammar does it, and
// the header and trailer arrive in the stream, in their own place, between the records
// they belong to.

/// <summary>One part of a feed: the opening line, a trade, or the closing line.</summary>
public abstract record FeedPart;

/// <summary>The line a feed opens with.</summary>
public sealed record FeedOpening(DateOnly Date, string Source) : FeedPart;

/// <summary>A trade.</summary>
public sealed record FeedTrade(string Symbol, int Quantity, DateOnly TradedOn) : FeedPart;

/// <summary>The line a feed closes with, and how many records it claims.</summary>
public sealed record FeedClosing(int Count) : FeedPart;

[Gram("""
	@using DotGram.Examples;

	Feed    : @FeedPart[] = Header & Row* recover eol & Trailer & eof

	Header  : @FeedPart = "H" & '|' & date: Date & '|' & source: Text & eol
	                   => @(new FeedOpening(ToDate(date), source))

	Row     : @FeedPart = "R" & '|' & symbol: Text & '|' & qty: Digit+ & '|' & date: Date & eol
	                   => @(new FeedTrade(symbol, Number(qty), ToDate(date)))

	Trailer : @FeedPart = "T" & '|' & count: Digit+ & eol
	                   => @(new FeedClosing(Number(count)))

	Date    = year: Digit{4} & '-' & month: Digit{2} & '-' & day: Digit{2}

	Text    = [^ '|' | '\r' | '\n']+
	Digit   = ['0'..'9']

	parse Feed
	""")]
public static partial class StreamingFeedReader
{
	/// <summary>
	/// Reads a feed from a reader, a part at a time.
	/// </summary>
	/// <remarks>
	/// Walked once, and while it is being walked the reader is being read: what is held is
	/// the part in hand and a buffer that is reused, not the file. A caller that wants the
	/// whole thing in memory calls <c>ParseFeed(string)</c> instead and gets an array —
	/// same grammar, same parts, same order.
	/// </remarks>
	/// <exception cref="FormatException">
	/// A missing header or trailer, or anything after it — thrown where the sequence is
	/// walked rather than where it was asked for, because nothing is read until then.
	/// </exception>
	public static IEnumerable<FeedPart> Read(TextReader input) => ParseFeed(input);

	/// <summary>
	/// What the feed adds up to, without ever holding it.
	/// </summary>
	/// <remarks>
	/// The point of the exercise. A hundred million trades cost one <c>FeedTrade</c> at a
	/// time — the loop keeps two integers, and every record it has finished with is
	/// garbage by the time the next one is read.
	/// </remarks>
	public static (int Trades, long Quantity) Total(TextReader input)
	{
		var trades   = 0;
		var quantity = 0L;

		foreach (var part in Read(input))
			if (part is FeedTrade trade)
			{
				trades++;
				quantity += trade.Quantity;
			}

		return (trades, quantity);
	}

	/// <summary>
	/// Whether the feed's own count agrees with the records it carried.
	/// </summary>
	/// <remarks>
	/// The envelope check that a line-by-line reader cannot make, and the reason the
	/// trailer is in the stream rather than thrown away: it arrives after the records, so
	/// by the time it is read the count is known.
	/// </remarks>
	public static bool Balances(TextReader input)
	{
		var counted = 0;

		foreach (var part in Read(input))
			switch (part)
			{
				case FeedTrade:            counted++;                    break;
				case FeedClosing(var said): return said == counted;
			}

		return false;
	}

	// Reachable from the grammar's `=>`, which becomes a method of this same class.
	static int Number(string digits) => int.Parse(digits, CultureInfo.InvariantCulture);

	static DateOnly ToDate(Date date) =>
		new(Number(date.Year), Number(date.Month), Number(date.Day));
}
