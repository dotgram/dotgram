using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples.Feeds;

// An end-of-day trade file a broker sends a fund's back office every evening, one record
// to a line, fields between pipes, a record type in the first field:
//
//     H|EOD-TRADES|2026-10-02|NORTHBRIDGE SECURITIES
//     D|T-0101|2026-10-02|2026-10-05|US0378331005|BUY|1200|227.52|USD
//     D|T-0103|2026-10-02|2026-10-06|GB0002634946|BUY|1,500|7.384|GBP     <- a thousands separator
//     D|T-0104|2026-10-02|2026-10-05|US5949181045|SELL|800|USD            <- the price left out
//     T|7|4490
//
// It is an example of docs/feeds.md and of the repository README, and the same grammar;
// the tests hold the copies to one text.
//
// The header and the trailer are typed records of the same sequence as the trades, so they
// arrive in their places: the header first, the trailer last, once every trade has been
// read. That is what lets a reader that holds nothing still check the trailer — by the time
// it arrives, the count is known. `Trailer & eof` gets the first refusal at every line
// (docs/syntax.md §8.2), so the line that closes the file is never read as a bad trade.
//
// The grammar says what the text must look like and nothing more. A date is four digits, a
// dash, two, a dash, two — whether the month has a 31st is the reader's business, so dates
// stay text and the parse cannot throw on one. A quantity is digits and a price digits with
// an optional point, which is what `long` and `decimal` read without a check of their own;
// both are bounded, so no line can overflow either. A line that does not fit is rejected
// where reading it stopped, and `FieldAt` turns that column into the name of the field.

/// <summary>A line of an end-of-day trade file.</summary>
public abstract record EodLine;

/// <summary>The header: which feed, for which business day, from whom.</summary>
public sealed record EodHeader(string Feed, string BusinessDate, string Sender) : EodLine;

/// <summary>A trade.</summary>
public sealed record EodTrade(
	string Id, string TradeDate, string SettlementDate, string Isin,
	Side Side, long Quantity, decimal Price, string Currency) : EodLine;

/// <summary>The trailer: how many trades the sender wrote, and their quantities added up.</summary>
public sealed record EodTrailer(long Count, long Quantity) : EodLine;

/// <summary>A line that is not a trade: where it is, which field broke it, and why.</summary>
public sealed record EodReject(int Line, int Column, string Field, string Text, string Message) : EodLine;

/// <summary>Which way a trade went.</summary>
public enum Side
{
	Buy,
	Sell,
}

[Gram("""
	EodFile : @EodLine[] = Header
	                     & Trade* recover eol
	                         => @(new EodReject(parserLine, parserFailureColumn, FieldAt(parserText, parserFailureColumn), parserText, parserMessage))
	                     & Trailer & eof

	Header  : @EodLine = "H|" & feed: Text & '|' & day: Date & '|' & sender: Text & eol
	                  => @(new EodHeader(feed, day, sender))

	Trade   : @EodLine = "D|" & id: Text & '|' & traded: Date & '|' & settles: Date & '|' & isin: Isin
	                   & '|' & side: BuySell & '|' & quantity: Quantity & '|' & price: Price & '|' & currency: Currency & eol
	                  => @(new EodTrade(id, traded, settles, isin, side, quantity, price, currency))

	Trailer : @EodLine = "T|" & count: Quantity & '|' & total: Quantity & eol
	                  => @(new EodTrailer(count, total))

	BuySell  : @Side     = "BUY" => @(Side.Buy) | "SELL" => @(Side.Sell)
	Quantity : @long     = Digit{1,15} => @(Number(parserText))
	Price    : @decimal  = Digit{1,12} & ('.' & Digit{1,6})? => @(Amount(parserText))

	Date     = Digit{4} & '-' & Digit{2} & '-' & Digit{2}
	Isin     = ['A'..'Z']{2} & ['A'..'Z' | '0'..'9']{9} & Digit
	Currency = ['A'..'Z']{3}
	Text     = [^ '|' | '\r' | '\n']+
	Digit    = ['0'..'9']

	parse EodFile as Read
	""")]
public static partial class EodTrades
{
	static readonly string[] Fields =
		["Record", "Id", "TradeDate", "SettlementDate", "Isin", "Side", "Quantity", "Price", "Currency"];

	static long Number(string digits)
	{
		return long.Parse(digits, CultureInfo.InvariantCulture);
	}

	static decimal Amount(string text)
	{
		return decimal.Parse(text, CultureInfo.InvariantCulture);
	}

	/// <summary>The field a column of a line falls in: the pipes in front of it say which.</summary>
	public static string FieldAt(string line, int column)
	{
		var pipes = 0;

		for (var at = 0; at < column - 1 && at < line.Length; at++)
			if (line[at] == '|')
				pipes++;

		// A pipe where the line should have ended: one field more than a trade has.
		if (pipes == Fields.Length - 1 && column - 1 < line.Length && line[column - 1] == '|')
			return "(extra)";

		return pipes < Fields.Length ? Fields[pipes] : "(extra)";
	}
}
