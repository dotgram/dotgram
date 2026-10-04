using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples.Feeds;

// A corporate-actions file a market-data vendor publishes every morning: dividends, splits
// and rights issues announced for the securities a client holds. Comma-separated, a record
// type in the first field, and a field quoted wherever it holds a comma or a quote of its
// own — an issuer's name is the usual one, and a quote inside a quoted field is doubled:
//
//     H,CORPORATE ACTIONS,2026-10-02,Meridian Data Services
//     D,CA-88121,DE0007236101,"Siemens AG, Reg.",DIV,2027-02-06,2027-02-10,5.20,EUR
//     D,CA-88122,US02079K3059,"Alphabet Inc. ""Class A""",SPLIT,2026-11-03,2026-11-03,20,
//     D,CA-88123,GB00BP6MXD84,"Shell plc,DIV,2026-11-13,2026-12-22,0.358,USD     <- the quote never closes
//     T,6
//
// It is an example of docs/feeds.md, and the same grammar; the tests hold the two to one text.
//
// A quoted field may not run past the end of its line here. CSV in general allows it; a feed
// whose records are lines does not, and saying so in the grammar is what keeps a quote left
// open from swallowing the records after it: the line is rejected where it ends, and the
// next one is read as if nothing had happened.
//
// The currency of a split is empty — a split pays nothing — so `Currency` is three letters
// or nothing at all, and the comma before it is still required: a line one field short is
// a line that does not fit.

/// <summary>A line of a corporate-actions file.</summary>
public abstract record ActionLine;

/// <summary>The header: which feed, for which day, from which vendor.</summary>
public sealed record ActionHeader(string Feed, string BusinessDate, string Vendor) : ActionLine;

/// <summary>An announced corporate action.</summary>
public sealed record CorporateAction(
	string Id, string Isin, string Issuer, ActionKind Kind,
	string ExDate, string PayDate, decimal Rate, string Currency) : ActionLine;

/// <summary>The trailer: how many actions the vendor sent.</summary>
public sealed record ActionTrailer(long Count) : ActionLine;

/// <summary>A line that is not an action: where it is, which field broke it, and why.</summary>
public sealed record ActionReject(int Line, int Column, string Field, string Text, string Message) : ActionLine;

/// <summary>What kind of action was announced.</summary>
public enum ActionKind
{
	Dividend,
	Split,
	Rights,
}

[Gram("""
	ActionFile : @ActionLine[] = Header
	                           & Action* recover eol
	                               => @(new ActionReject(parserLine, parserFailureColumn, FieldAt(parserText, parserFailureColumn), parserText, parserMessage))
	                           & Trailer & eof

	Header  : @ActionLine = "H," & feed: Text & ',' & day: Date & ',' & vendor: Text & eol
	                     => @(new ActionHeader(feed, day, vendor))

	Action  : @ActionLine = "D," & id: Text & ',' & isin: Isin & ',' & issuer: Text & ',' & kind: Kind
	                      & ',' & exDate: Date & ',' & payDate: Date & ',' & rate: Rate & ',' & currency: Currency & eol
	                     => @(new CorporateAction(id, isin, issuer, kind, exDate, payDate, rate, currency))

	Trailer : @ActionLine = "T," & count: Count & eol => @(new ActionTrailer(count))

	Text     : @string     = '"' & body: ([^ '"' | '\r' | '\n'] | '"' & '"')* & '"' => @(Unquote(body))
	                       | plain: [^ ',' | '"' | '\r' | '\n']*                  => @(plain)
	Kind     : @ActionKind = "DIV" => @(ActionKind.Dividend) | "SPLIT" => @(ActionKind.Split) | "RIGHTS" => @(ActionKind.Rights)
	Rate     : @decimal    = Digit{1,12} & ('.' & Digit{1,8})? => @(Amount(parserText))
	Count    : @long       = Digit{1,15} => @(Number(parserText))

	Date     = Digit{4} & '-' & Digit{2} & '-' & Digit{2}
	Isin     = ['A'..'Z']{2} & ['A'..'Z' | '0'..'9']{9} & Digit
	Currency = ['A'..'Z']{3} | none
	Digit    = ['0'..'9']

	parse ActionFile as Read
	""")]
public static partial class CorporateActions
{
	static readonly string[] Fields =
		["Record", "Id", "Isin", "Issuer", "Kind", "ExDate", "PayDate", "Rate", "Currency"];

	static string Unquote(string body)
	{
		return body.Replace("\"\"", "\"", StringComparison.Ordinal);
	}

	static long Number(string digits)
	{
		return long.Parse(digits, CultureInfo.InvariantCulture);
	}

	static decimal Amount(string text)
	{
		return decimal.Parse(text, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// The field a column of a line falls in: the commas in front of it say which, except the
	/// ones inside quotes.
	/// </summary>
	public static string FieldAt(string line, int column)
	{
		var commas = 0;
		var quoted = false;

		for (var at = 0; at < column - 1 && at < line.Length; at++)
			if (line[at] == '"')
				quoted = !quoted;
			else if (line[at] == ',' && !quoted)
				commas++;

		// A comma where the line should have ended: one field more than an action has.
		if (commas == Fields.Length - 1 && column - 1 < line.Length && line[column - 1] == ',')
			return "(extra)";

		return commas < Fields.Length ? Fields[commas] : "(extra)";
	}
}
