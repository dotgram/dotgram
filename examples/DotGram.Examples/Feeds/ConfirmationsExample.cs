using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples.Feeds;

// A settlement-confirmation file a custodian sends at the end of the day: one record to a
// line, every field in its own columns, and a record type in column 1 — `H` the header,
// `D` a confirmation, `T` the trailer. Numbers are zero-padded to their width and amounts
// carry two implied decimals; text is padded with spaces.
//
//     col  1      record type      D
//          2-13   trade reference  T-0101 (padded with spaces)
//         14-25   ISIN             US0378331005
//         26-33   settlement date  20261005 (yyyyMMdd)
//         34      direction        R receive, D deliver
//         35-46   quantity         000000001200
//         47-61   amount           000000027302400 = 273,024.00
//         62-64   currency         USD
//         65      status           S settled, P pending, F failed
//
// The trailer counts the confirmations and adds up their amounts. It is an example of
// docs/feeds.md, and the same grammar; the tests hold the two to one text.
//
// Nothing here looks for a delimiter: a field is so many characters, `Digit{12}` or
// `Column(12)`, and a line ends where the layout says it does. So a line one character
// short is refused where the missing character should have been, one character long where
// the line should have ended, and a quantity somebody typed with spaces in front of it
// instead of zeros at the first space. `FieldAt` reads the same layout backwards, from a
// column to the name of the field it falls in.

/// <summary>A line of a settlement-confirmation file.</summary>
public abstract record ConfirmLine;

/// <summary>The header: which file, for which business day, from which custodian.</summary>
public sealed record ConfirmHeader(string File, string BusinessDate, string Custodian) : ConfirmLine;

/// <summary>A settlement, confirmed or not.</summary>
public sealed record Confirmation(
	string Reference, string Isin, string SettlementDate, Direction Direction,
	long Quantity, decimal Amount, string Currency, SettlementStatus Status) : ConfirmLine;

/// <summary>The trailer: how many confirmations, and their amounts added up.</summary>
public sealed record ConfirmTrailer(long Count, decimal Amount) : ConfirmLine;

/// <summary>A line that is not a confirmation: where it is, which field broke it, and why.</summary>
public sealed record ConfirmReject(int Line, int Column, string Field, string Text, string Message) : ConfirmLine;

/// <summary>Which way the securities moved.</summary>
public enum Direction
{
	Receive,
	Deliver,
}

/// <summary>Where a settlement stands.</summary>
public enum SettlementStatus
{
	Settled,
	Pending,
	Failed,
}

[Gram("""
	ConfirmFile : @ConfirmLine[] = Header
	                             & Confirmation* recover eol
	                                 => @(new ConfirmReject(parserLine, parserFailureColumn, FieldAt(parserText, parserFailureColumn), parserText, parserMessage))
	                             & Trailer & eof

	Header       : @ConfirmLine = 'H' & file: Column(10) & day: Digit{8} & custodian: Column(30) & eol
	                           => @(new ConfirmHeader(file.Trim(), day, custodian.Trim()))

	Confirmation : @ConfirmLine = 'D' & reference: Column(12) & isin: Column(12) & settles: Digit{8} & direction: Way
	                            & quantity: Digit{12} & amount: Digit{15} & currency: ['A'..'Z']{3} & status: Status & eol
	                           => @(new Confirmation(reference.Trim(), isin, settles, direction, Number(quantity), Number(amount) / 100m, currency, status))

	Trailer      : @ConfirmLine = 'T' & count: Digit{9} & amount: Digit{18} & eol
	                           => @(new ConfirmTrailer(Number(count), Number(amount) / 100m))

	Way    : @Direction        = 'R' => @(Direction.Receive) | 'D' => @(Direction.Deliver)
	Status : @SettlementStatus = 'S' => @(SettlementStatus.Settled) | 'P' => @(SettlementStatus.Pending) | 'F' => @(SettlementStatus.Failed)

	Column(width: int) = [^ '\r' | '\n']{width}
	Digit              = ['0'..'9']

	parse ConfirmFile as Read
	""")]
public static partial class Confirmations
{
	// Where each field of a confirmation begins, 1-based, and what it is called.
	static readonly (int Column, string Name)[] Layout =
	[
		(1, "Record"), (2, "Reference"), (14, "Isin"), (26, "SettlementDate"), (34, "Direction"),
		(35, "Quantity"), (47, "Amount"), (62, "Currency"), (65, "Status"), (66, "(extra)"),
	];

	static long Number(string digits)
	{
		return long.Parse(digits, CultureInfo.InvariantCulture);
	}

	/// <summary>The field a column of a confirmation falls in, read off the layout.</summary>
	public static string FieldAt(string line, int column)
	{
		var name = Layout[0].Name;

		foreach (var field in Layout)
			if (field.Column <= column)
				name = field.Name;

		return name;
	}
}
