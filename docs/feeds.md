# Feeds

A feed is a file of records that somebody else writes: a broker's end-of-day trades, a data
vendor's corporate actions, a custodian's settlement confirmations, or a sheet a person keeps
by hand. It is right nearly everywhere and wrong somewhere, and it is usually too large to
hold. .Gram reads such a file as a sequence of typed records, from a `TextReader`, one record
at a time while the file is still being read. A bad line does not cost the file: `recover`
marks the repetition of records, a bad one is handed back in its place with where it is and
why, and reading goes on at the next line.

The repository README has the shortest of these, a price list, and the first of the trading
feeds below. This page has all of them, and then one of them loaded into a database ten
million records at a time.

Every example here is run as it is written: the file at the top of it is the input, and what
the loop prints must be the block below the loop, character for character. The same grammars
are in [`examples/DotGram.Examples/Feeds`](../examples/DotGram.Examples/Feeds/), one file each,
for whoever wants to copy one.

## What the parser checks, and what it leaves to you

The parser tells you where the text does not fit; what the values mean is yours.

A grammar here says what a line must look like — a record type, so many fields, a date written
as four digits, a dash, two digits, a dash and two digits, a quantity of digits, a price of
digits with a point — and nothing about whether the values make sense. A letter in a date, a
thousands separator in a quantity, a field left out, a quote never closed, a line one character
short: those are the parser's, and each comes back as a rejected line with the line, the
column, the field it broke in and what would have fitted there. A 31st of November, a
settlement date before the trade date, a price nobody would pay: those parse, because they are
written correctly, and checking them is business logic, which belongs in the program that
reads the records rather than in the grammar. So dates stay text here, and a number is read
only where its digits are bounded, so that turning it into a `long` or a `decimal` cannot
throw.

Each trading feed has a header and a trailer, and both are typed records of the same sequence:
the header arrives first, the trailer last, once every detail line has been read. That is what
lets a loop that holds nothing still check the trailer — by the time it arrives, the count is
known, and comparing the two is one line of the loop.

## End-of-day trades: pipes

A broker's back-office system sends a fund's back office the day's trades every evening, one
record to a line, the fields between pipes and the record type in the first: `H` the header,
`D` a trade, `T` the trailer with the number of trades and their quantities added up. The
system writes it, but people get into it — a trade amended by hand, a file patched before it
was resent — and so does whatever the sender changed last. `trades.txt`:

```text
H|EOD-TRADES|2026-10-02|NORTHBRIDGE SECURITIES
D|T-0101|2026-10-02|2026-10-05|US0378331005|BUY|1200|227.52|USD
D|T-0102|2026-10-02|2026-10-06|DE0007164600|SELL|350|212.40|EUR
D|T-0103|2026-10-02|2026-10-06|GB0002634946|BUY|1,500|7.384|GBP
D|T-0104|2026-10-02|2026-10-05|US5949181045|SELL|800|USD
D|T-0105|2026-1O-02|2026-10-06|NL0010273215|BUY|40|612.10|EUR
D|T-0106|2026-10-02|2026-10-05|US88160R1014|BUY|600|243.85|USD
D|T-0107|2026-10-02|2026-10-05|US0378331005|HOLD|100|227.60|USD
D|T-0108|2026-10-02|2026-10-05|US0231351067|SELL|250|186.20|USD
T|8|4840
```

Four of the eight trades are wrong: a quantity with a thousands separator, a trade whose price
was left out, a letter O typed for a zero in a date, and a side the file does not know.

```csharp
using System;
using System.Globalization;
using System.IO;

using DotGram;

public abstract record EodLine;

public sealed record EodHeader(string Feed, string BusinessDate, string Sender) : EodLine;

public sealed record EodTrade(
	string Id, string TradeDate, string SettlementDate, string Isin,
	Side Side, long Quantity, decimal Price, string Currency) : EodLine;

public sealed record EodTrailer(long Count, long Quantity) : EodLine;

public sealed record EodReject(int Line, int Column, string Field, string Text, string Message) : EodLine;

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
```

```csharp
using var feed = File.OpenText("trades.txt");

var trades   = 0;
var rejected = 0;

foreach (var line in EodTrades.Read(feed))
{
	switch (line)
	{
		case EodHeader header:
			Console.WriteLine($"{header.Feed} {header.BusinessDate} from {header.Sender}");
			break;

		case EodTrade trade:
			trades++;
			Console.WriteLine($"{trade.Id} {trade.Side} {trade.Quantity} {trade.Isin} at {trade.Price} {trade.Currency}");
			break;

		case EodReject reject:
			rejected++;
			Console.WriteLine($"line {reject.Line}, {reject.Field}: {reject.Message}");
			break;

		case EodTrailer trailer:
			var check = trades + rejected == trailer.Count ? "OK" : "MISMATCH";
			Console.WriteLine($"read {trades}, rejected {rejected}; trailer: expected {trailer.Count} — {check}");
			break;
	}
}
```

```text
EOD-TRADES 2026-10-02 from NORTHBRIDGE SECURITIES
T-0101 Buy 1200 US0378331005 at 227.52 USD
T-0102 Sell 350 DE0007164600 at 212.40 EUR
line 4, Quantity: Expected '|' at 4:50.
line 5, Price: Expected Digit at 5:54.
line 6, TradeDate: Expected Date at 6:16.
T-0106 Buy 600 US88160R1014 at 243.85 USD
line 8, Side: Expected ['B' | 'S'] at 8:45.
T-0108 Sell 250 US0231351067 at 186.20 USD
read 4, rejected 4; trailer: expected 8 — OK
```

`Read` returns an `IEnumerable<EodLine>` that reads a line when the loop asks for the next one,
so a file of any size costs one line at a time. The `=>` after `recover` says what a bad line
becomes, from names the parser fills in: `parserLine` is the line a person opens the file at,
`parserFailureColumn` the column reading stopped at, `parserText` the line, and `parserMessage`
what would have fitted there. `FieldAt` is ordinary C# of the class: it counts the pipes in
front of that column and names the field. So the quantity `1,500` is refused at the comma,
where a pipe should have followed the digits, and the trade with no price at `USD`, where a
digit should have been.

The record types matter. `H|`, `D|` and `T|` are what a line is recognised by, so a trade
never begins like the trailer, and `Trailer & eof` gets the first refusal at every line — the
line that closes the file is never read as a bad trade. The trailer's count is the sender's
count of detail lines, good and bad, so `trades + rejected` is what it is held against: a
mismatch would mean lines lost on the way, not lines that were wrong.

## Corporate actions: CSV with quoted fields

A market-data vendor publishes the corporate actions announced for the securities a client
holds — dividends, splits, rights issues — as a CSV file every morning, `actions.csv`. A field
that holds a comma or a quote of its own is quoted, and a quote inside it is doubled; an
issuer's name is the usual one. A split pays nothing, so its currency is empty:

```text
H,CORPORATE ACTIONS,2026-10-02,Meridian Data Services
D,CA-88121,DE0007236101,"Siemens AG, Reg.",DIV,2027-02-06,2027-02-10,5.20,EUR
D,CA-88122,US02079K3059,"Alphabet Inc. ""Class A""",SPLIT,2026-11-03,2026-11-03,20,
D,CA-88123,GB00BP6MXD84,"Shell plc,DIV,2026-11-13,2026-12-22,0.358,USD
D,CA-88124,NL0010273215,ASML Holding,DIV,2026-10-28,2026-11-05,1.6O,EUR
D,CA-88125,FR0000121014,LVMH,DIV,2026-12-02,2026-12-04,EUR
D,CA-88126,US0378331005,Apple Inc.,DIV,2026-11-10,2026-11-13,0.26,USD
T,6
```

Three lines are wrong: a quote opened in front of an issuer and never closed, a letter O for a
zero in a dividend rate, and a rate left out.

```csharp
using System;
using System.Globalization;
using System.IO;

using DotGram;

public abstract record ActionLine;

public sealed record ActionHeader(string Feed, string BusinessDate, string Vendor) : ActionLine;

public sealed record CorporateAction(
	string Id, string Isin, string Issuer, ActionKind Kind,
	string ExDate, string PayDate, decimal Rate, string Currency) : ActionLine;

public sealed record ActionTrailer(long Count) : ActionLine;

public sealed record ActionReject(int Line, int Column, string Field, string Text, string Message) : ActionLine;

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

	Text     : @string     = quoted: Quoted                        => @(Unquote(quoted))
	                       | plain: [^ ',' | '"' | '\r' | '\n']* => @(plain)
	Kind     : @ActionKind = "DIV" => @(ActionKind.Dividend) | "SPLIT" => @(ActionKind.Split) | "RIGHTS" => @(ActionKind.Rights)
	Rate     : @decimal    = Digit{1,12} & ('.' & Digit{1,8})? => @(Amount(parserText))
	Count    : @long       = Digit{1,15} => @(Number(parserText))

	Date      = Digit{4} & '-' & Digit{2} & '-' & Digit{2}
	Isin      = ['A'..'Z']{2} & ['A'..'Z' | '0'..'9']{9} & Digit
	Currency  = ['A'..'Z']{3} | none
	Quoted    = '"' & Character* & '"'
	Character = [^ '"' | '\r' | '\n'] | "\"\""
	Digit     = ['0'..'9']

	parse ActionFile as Read
	""")]
public static partial class CorporateActions
{
	static readonly string[] Fields =
		["Record", "Id", "Isin", "Issuer", "Kind", "ExDate", "PayDate", "Rate", "Currency"];

	static string Unquote(string quoted)
	{
		return quoted[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal);
	}

	static long Number(string digits)
	{
		return long.Parse(digits, CultureInfo.InvariantCulture);
	}

	static decimal Amount(string text)
	{
		return decimal.Parse(text, CultureInfo.InvariantCulture);
	}

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
```

```csharp
using var feed = File.OpenText("actions.csv");

var actions  = 0;
var rejected = 0;

foreach (var line in CorporateActions.Read(feed))
{
	switch (line)
	{
		case ActionHeader header:
			Console.WriteLine($"{header.Feed} {header.BusinessDate} from {header.Vendor}");
			break;

		case CorporateAction action:
			actions++;
			var pays = action.Kind == ActionKind.Split ? $"{action.Rate} for 1" : $"{action.Rate} {action.Currency}";
			Console.WriteLine($"{action.Id} {action.Kind} {action.Issuer}: {pays}, ex {action.ExDate}");
			break;

		case ActionReject reject:
			rejected++;
			Console.WriteLine($"line {reject.Line}, {reject.Field}: {reject.Message}");
			break;

		case ActionTrailer trailer:
			var check = actions + rejected == trailer.Count ? "OK" : "MISMATCH";
			Console.WriteLine($"read {actions}, rejected {rejected}; trailer: expected {trailer.Count} — {check}");
			break;
	}
}
```

```text
CORPORATE ACTIONS 2026-10-02 from Meridian Data Services
CA-88121 Dividend Siemens AG, Reg.: 5.20 EUR, ex 2027-02-06
CA-88122 Split Alphabet Inc. "Class A": 20 for 1, ex 2026-11-03
line 4, Issuer: Expected Character or '"' at 4:71.
line 5, Rate: Expected ',' at 5:67.
line 6, Rate: Expected Digit at 6:56.
CA-88126 Dividend Apple Inc.: 0.26 USD, ex 2026-11-10
read 3, rejected 3; trailer: expected 6 — OK
```

A quoted field may not run past the end of its line here. CSV in general allows that; a feed
whose records are lines does not, and saying so in the grammar is what keeps a quote left open
from swallowing the records after it. The line is refused where it ends — at column 71, still
inside the issuer, wanting another character or the closing quote — and the next line is read
as if nothing had happened. `Character` is a rule of its own only so that the message can name
it rather than spell out every character a quoted field may hold. `FieldAt` counts the commas
outside quotes. The rate left out on line 6 is refused where its first digit should have been,
at `EUR`: the parser cannot know that a field is missing rather than wrong, only where the text
stopped fitting.

## Settlement confirmations: fixed width

A custodian sends a fund the day's settlement confirmations at the end of the day, every field
in its own columns and the record type in column 1. Numbers are padded with zeros to their
width and amounts carry two implied decimals; text is padded with spaces. The layout of a
confirmation:

```text
col  1      record type      D
     2-13   trade reference  T-0101, padded with spaces
    14-25   ISIN             US0378331005
    26-33   settlement date  20261005
    34      direction        R receive, D deliver
    35-46   quantity         000000001200
    47-61   amount           000000027302400 = 273,024.00
    62-64   currency         USD
    65      status           S settled, P pending, F failed
```

The header is the file's name in ten columns, the business day and the custodian's name in
thirty; the trailer is the number of confirmations in nine and their amounts added up in
eighteen. `confirmations.txt`:

```text
HSETTLCONF 20261002NORTH HARBOUR CUSTODY SERVICES
DT-0101      US037833100520261005R000000001200000000027302400USDS
DT-0102      DE000716460020261006D000000000350000000007434000EURP
DT-0103      GB000263494620261006R        1500000000001107600GBPS
DT-0104      US594918104520261005D000000000800000000041625600USD
DT-0105      NL00102732152026100OR000000000040000000002448400EURS
DT-0106      US88160R101420261005R000000000600000000014631000USDS
XT-0107      US88160R1014
T000000007000000000094549000
```

Four lines are wrong: a quantity somebody typed with spaces in front of it instead of zeros, a
line one character short, a letter O in a date, and a line of a record type the file does not
have, cut short as well.

```csharp
using System;
using System.Globalization;
using System.IO;

using DotGram;

public abstract record ConfirmLine;

public sealed record ConfirmHeader(string File, string BusinessDate, string Custodian) : ConfirmLine;

public sealed record Confirmation(
	string Reference, string Isin, string SettlementDate, Direction Direction,
	long Quantity, decimal Amount, string Currency, SettlementStatus Status) : ConfirmLine;

public sealed record ConfirmTrailer(long Count, decimal Amount) : ConfirmLine;

public sealed record ConfirmReject(int Line, int Column, string Field, string Text, string Message) : ConfirmLine;

public enum Direction
{
	Receive,
	Deliver,
}

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
	                           => @(new Confirmation(reference.Trim(), isin, settles, direction, Number(quantity), Cents(amount), currency, status))

	Trailer      : @ConfirmLine = 'T' & count: Digit{9} & amount: Digit{18} & eol
	                           => @(new ConfirmTrailer(Number(count), Cents(amount)))

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

	// Two implied decimals: 000000027302400 is 273,024.00, and keeps its two places.
	static decimal Cents(string digits)
	{
		return Number(digits) * 0.01m;
	}

	public static string FieldAt(string line, int column)
	{
		var name = Layout[0].Name;

		foreach (var field in Layout)
			if (field.Column <= column)
				name = field.Name;

		return name;
	}
}
```

```csharp
using var feed = File.OpenText("confirmations.txt");

var confirmed = 0;
var rejected  = 0;

foreach (var line in Confirmations.Read(feed))
{
	switch (line)
	{
		case ConfirmHeader header:
			Console.WriteLine($"{header.File} {header.BusinessDate} from {header.Custodian}");
			break;

		case Confirmation confirmation:
			confirmed++;
			Console.WriteLine($"{confirmation.Reference} {confirmation.Status} {confirmation.Direction} {confirmation.Quantity} {confirmation.Isin}: {confirmation.Amount} {confirmation.Currency}");
			break;

		case ConfirmReject reject:
			rejected++;
			Console.WriteLine($"line {reject.Line}, {reject.Field}: {reject.Message}");
			break;

		case ConfirmTrailer trailer:
			var check = confirmed + rejected == trailer.Count ? "OK" : "MISMATCH";
			Console.WriteLine($"read {confirmed}, rejected {rejected}; trailer: expected {trailer.Count} — {check}");
			break;
	}
}
```

```text
SETTLCONF 20261002 from NORTH HARBOUR CUSTODY SERVICES
T-0101 Settled Receive 1200 US0378331005: 273024.00 USD
T-0102 Pending Deliver 350 DE0007164600: 74340.00 EUR
line 4, Quantity: Expected Digit at 4:35.
line 5, Status: Expected ['S' | 'P' | 'F'] at 5:65.
line 6, SettlementDate: Expected Digit at 6:33.
T-0106 Settled Receive 600 US88160R1014: 146310.00 USD
line 8, Record: Expected 'D' at 8:1.
read 3, rejected 4; trailer: expected 7 — OK
```

Nothing here looks for a delimiter: a field is so many characters, `Digit{12}` or
`Column(12)`, and a line ends where the layout says it does. So the short line is refused at
column 65, where its status should have been, the padded quantity at its first space, and a
line one character too long would be refused where the line should have ended. `FieldAt` reads
the same layout backwards, from a column to the name of the field it falls in.

## Files kept by hand

Not every feed comes from a system. Two that people write themselves, smaller and looser than
the trading feeds, and two more things `recover` can do.

An order sheet a wholesaler's clerk fills in from a phone call, `order.txt` — two header
lines, then items and remarks in any order:

```text
Order for: Hill Street Cafe
Deliver: Monday, before 8
12 x milk 1l
3 x oat milk, the barista one
# the croissants were stale last week, ask
x2 sugar
4 x butter
```

```csharp
public abstract record SheetLine;
public sealed record Customer(string Name) : SheetLine;
public sealed record Delivery(string When) : SheetLine;
public sealed record Ordered(int Quantity, string Product) : SheetLine;
public sealed record Remark(string Text) : SheetLine;
public sealed record Unclear(int Line, string Text, string Message) : SheetLine;

[Gram("""
	Sheet : @SheetLine[] = Customer & Delivery
	                     & Line* recover eol => @(new Unclear(parserLine, parserText, parserMessage))
	                     & eof

	Customer : @SheetLine = "Order for:" & ' '* & name: Rest & eol => @(new Customer(name))
	Delivery : @SheetLine = "Deliver:"   & ' '* & time: Rest & eol => @(new Delivery(time))

	Line   : @SheetLine = (v: Item | v: Remark) => @(v)
	Item   : @SheetLine = quantity: Digit+ & ' '* & 'x' & ' '+ & product: Rest & eol
	                   => @(new Ordered(Number(quantity), product))
	Remark : @SheetLine = '#' & ' '* & text: Rest & eol => @(new Remark(text))

	Rest  = [^ '\r' | '\n']+
	Digit = ['0'..'9']

	parse Sheet
	""")]
public static partial class OrderSheet
{
	static int Number(string digits)
	{
		return int.Parse(digits, CultureInfo.InvariantCulture);
	}
}
```

```csharp
using var sheet = File.OpenText("order.txt");

foreach (var line in OrderSheet.ParseSheet(sheet))
	Console.WriteLine(line);
```

```text
Customer { Name = Hill Street Cafe }
Delivery { When = Monday, before 8 }
Ordered { Quantity = 12, Product = milk 1l }
Ordered { Quantity = 3, Product = oat milk, the barista one }
Remark { Text = the croissants were stale last week, ask }
Unclear { Line = 6, Text = x2 sugar, Message = Expected ['0'..'9' | '#'] at 6:1. }
Ordered { Quantity = 4, Product = butter }
```

The header lines are part of the same sequence, in the order they were read. They are also
required: a sheet without them is refused with a `FormatException`, thrown by the loop at the
first record, because nothing is read before the loop asks. Recovery is for the lines that
repeat; the frame around them still has to be there.

A maintenance logbook the people who run the machines fill in by hand on the shop floor,
`log.txt`, where the third entry has a day of one digit:

```text
2026-10-01 07:55 press-4 oil topped up
2026-10-01 09:10 press-2 belt replaced
2026-10-1 11:30 press-4 noise from the bearing
2026-10-01 14:05 lathe-1 calibrated
```

```csharp
public sealed record LogEntry(DateTime At, string Machine, string Note);

[Gram("""
	Log   : @LogEntry[] = Entry* recover eol

	Entry : @LogEntry = at: Stamp & ' '+ & machine: Name & ' '+ & note: Rest & eol
	                 => @(new LogEntry(ToTime(at), machine, note))

	Stamp = Digit{4} & '-' & Digit{2} & '-' & Digit{2} & ' ' & Digit{2} & ':' & Digit{2}
	Name  = [^ ' ' | '\r' | '\n']+
	Rest  = [^ '\r' | '\n']+
	Digit = ['0'..'9']

	parse Log as Read stream yield
	""")]
public static partial class Logbook
{
	static DateTime ToTime(string text)
	{
		return DateTime.ParseExact(text, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
	}

	static partial void OnRecovered(
		string rule, string text, long position, int line, int column, int ordinal, string message)
	{
		Console.Error.WriteLine($"line {line}: {message} ({text})");
	}
}
```

```csharp
using var log = File.OpenText("log.txt");

foreach (var entry in Logbook.Read(log))
	Console.WriteLine($"{entry.At.TimeOfDay} {entry.Machine}: {entry.Note}");
```

```text
07:55:00 press-4: oil topped up
09:10:00 press-2: belt replaced
line 3: Expected ['0'..'9'] at 3:10. (2026-10-1 11:30 press-4 noise from the bearing)
14:05:00 lathe-1: calibrated
```

Here the `recover` has no `=>`, so a bad line is dropped and the records come back as
themselves, `IEnumerable<LogEntry>`, with nothing to filter out. What was dropped goes to
`OnRecovered`, a `partial void` the generated class declares; left unimplemented, the compiler
removes every call to it and the parse pays nothing for it. The report lands between the
second entry and the fourth because that is when the third line was read.

## Ten million records into a database

[`examples/DotGram.FeedLoad`](../examples/DotGram.FeedLoad/) is a console program that does
with one of the feeds above what a client would: it writes the feed at any size, reads it with
its grammar, and bulk-loads it into SQLite with [linq2db](https://github.com/linq2db/linq2db),
the bad lines into a table of their own. It takes linq2db and Microsoft.Data.Sqlite from NuGet,
the only project in the repository that takes a database at all, and for that reason it is not
in `DotGram.slnx`: a solution build, CI's included, has no reason to restore them. Run it on its
own, from the repository root:

```sh
dotnet run -c Release --project examples/DotGram.FeedLoad                       # 10M end-of-day trades
dotnet run -c Release --project examples/DotGram.FeedLoad -- --records 1M       # a quick run
dotnet run -c Release --project examples/DotGram.FeedLoad -- --format csv --records 1M
```

| Option | | Default |
| --- | --- | --- |
| `--format pipe\|csv\|fixed` | which feed: end-of-day trades, corporate actions or settlement confirmations | `pipe` |
| `--records N` | detail lines to write: `20`, `50k`, `1M`, `10M` | `10M` |
| `--bad N` | how many of them are broken | one in 5,000 |
| `--seed N` | the same seed writes the same file | `1` |
| `--feed FILE` | read this file instead of writing one | |
| `--db FILE` | the database, kept afterwards | a temporary file, deleted |
| `--keep` | keep the temporary feed and database | |

A million records is a run of a few seconds; the default ten million takes about a minute and
a half and 1.5 GB of temporary disk, the feed and the database together, both deleted at the
end.

What it does, in order:

1. **Writes the feed** to a temporary file: a header, the detail lines, and a trailer with
   their count (and, for trades and confirmations, their quantities or amounts added up). The
   broken lines are spread over the file by the seed, and they are the kinds the examples
   above have: a letter in a date, a number with a letter or a thousands separator in it, a
   line that ends before its last field, a quote never closed, a record type nobody knows.
2. **Reads it with nothing done with the records** — what the parser alone costs.
3. **Reads it again into the database**, in one transaction. The parser's `IEnumerable` goes
   straight into linq2db's `BulkCopy`; a small iterator passes the detail records through and
   hands everything else to a local function on the way, on the same connection:

   ```csharp
   var loaded = db.BulkCopy(copy, Details<EodTrade>(EodTrades.Read(reader), Aside)).RowsCopied;
   ```

   The header becomes a row of `Files`; the trailer completes it with its count and total; a
   rejected line becomes a row of `Rejects` — the line, the column, the field, a category, the
   line's text and the parser's message — inserted when it is read. The records go to the
   database as they are read, and the file is never in memory; what is held is the rejected
   lines, for the summary, and BulkCopy's batch of rows. The record
   types are the grammars' own and know nothing of a database; their tables and columns are a
   fluent mapping in the program.
4. **Asks the database a question or two** — the most traded ISINs, the quantity the trailer
   claims against `SUM(Quantity)` — and prints one line: how many records were loaded, how
   many rejected and why, and whether the trailer's count is what was read.

The category is the program's, not the parser's: `TradingFeedGenerator.Categorize` reads it off
the field the parser stopped in and where — a stop in a date field is a bad date, a line that
ended before the record did is a missing field, a quote left open is an unclosed quote. It is
there so the summary can say what went wrong in words; the message is what the parser said.

### A small run

Twenty trades with three broken, kept so the file can be shown:

```sh
dotnet run -c Release --project examples/DotGram.FeedLoad -- --records 20 --bad 3 --seed 7 --keep
```

The file it wrote:

```text
H|EOD-TRADES|2026-10-02|NORTHBRIDGE SECURITIES
D|T-0000001|2026-10-02|2026-10-04|GB00BP6MXD84|BUY|2503|28.73|GBP
X|T-0000002|2026-10-02|2026-10-05|US0378331005|BUY|48031|237.47|USD
D|T-0000003|2026-10-02|2026-10-04|US0378331005|SELL|82884|211.66|USD
D|T-0000004|2026-10-02|2026-10-05|US0378331005|SELL|59775|248.42|USD
D|T-0000005|2026-10-02|2026-10-04|GB00BP6MXD84|SELL|70713|29.65|GBP
D|T-0000006|2026-10-02|2026-10-04|US0231351067|BUY|72379|168.23|USD
D|T-0000007|2026-10-02|2026-10-04|US02079K3059|SELL|2436|173.71|USD
D|T-0000008|2026-10-02|2026-10-05|US5949181045|BUY|44142|383.73|USD
D|T-0000009|2026-10-02|2026-10-04|US0231351067|BUY|49530|201.01|USD
D|T-0000010|2026-10-02|2026-10-03|NL0010273215|SELL|48522|575.87|EUR
D|T-0000011|2026-10-02|2026-10-04|DE0007236101|BUY|1506|179.49|EUR
D|T-0000012|2026-10-02|2026-10-03|US0231351067|BUY|58698|178.85|USD
D|T-0000013|2026-10-02|2026-10-05|US88160R1014|SELL|56180|249.81
D|T-0000014|2026-10-02|2026-10-05|NL0010273215|BUY|72631|585.73|EUR
D|T-0000015|2026-10-02|2026-10-05|GB00BP6MXD84|SELL|84298|25.62|GBP
D|T-0000016|2026-10-02|2026-10-04|US0378331005|BUY|89274|240.40|USD
D|T-0000017|2026-10-O2|2026-10-04|US0231351067|SELL|79615|200.50|USD
D|T-0000018|2026-10-02|2026-10-04|GB0002634946|BUY|56944|13.63|GBP
D|T-0000019|2026-10-02|2026-10-05|US0378331005|BUY|48346|232.57|USD
D|T-0000020|2026-10-02|2026-10-04|GB00BP6MXD84|SELL|95177|25.50|GBP
T|20|1123584
```

What it printed:

```text
Feed:     pipe, 20 records, 3 bad (bad date 1, missing field 1, unknown record 1), seed 7
File:     /tmp/dotgram-feedload-39259.txt, 1.4 KB
Parse:    22 lines in 0.0 s — 1,420 lines/s, 0.1 MB/s
Load:     22 lines in 0.4 s — 53 lines/s, 0.0 MB/s
By ISIN:  GB00BP6MXD84 4 trades, 252,691 shares
By ISIN:  US0378331005 4 trades, 280,279 shares
By ISIN:  US0231351067 3 trades, 180,607 shares
Quantity: trailer 1,123,584, SUM(Quantity) 939,758 (the rejected lines are in the trailer and not in the table)

SELECT Line, Column, Field, Category, Message, Text FROM Rejects

Line  Column  Field           Category        Message                   Text
   3       1  Record          unknown record  Expected "D|" at 3:1.     X|T-0000002|2026-10-02|2026-10-05|US0378331005|BUY|48031|237.47|USD
  14      65  Price           missing field   Expected '|' at 14:65.    D|T-0000013|2026-10-02|2026-10-05|US88160R1014|SELL|56180|249.81
  18      21  TradeDate       bad date        Expected Date at 18:21.   D|T-0000017|2026-10-O2|2026-10-04|US0231351067|SELL|79615|200.50|USD

Database: /tmp/dotgram-feedload-39259.db, 20.0 KB; peak working set 87.2 MB

Loaded 17 records, rejected 3 (bad date 1, missing field 1, unknown record 1); trailer: expected 20 — OK
Timings: generate 0.0 s, parse 0.0 s, parse + load 0.4 s, queries 0.2 s
Kept:     /tmp/dotgram-feedload-39259.txt, /tmp/dotgram-feedload-39259.db
```

Line 14 lost its currency, so it ended where a pipe should have followed the price: the field
is the one reading was in when the line ran out. The trailer counts twenty, seventeen were
loaded and three rejected, and so the trailer is right — the file arrived whole. The quantity
it claims is larger than the table's sum by the three rejected lines, which is what the
rejects table is for: those are the lines somebody has to look at.

### Ten million

The default run, once, on the machine this was written on: an AMD Ryzen 9 5950X, Linux, .NET 10,
Release, the feed and the database on a RAM disk, and the garbage collector's heap capped at
256 MB (`DOTNET_GCHeapHardLimit=0x10000000`) to show that the cap is never near. The temporary
directory is shown as `/tmp`:

```text
Feed:     pipe, 10,000,000 records, 2,000 bad (bad date 498, bad number 492, missing field 515, unknown record 495), seed 1
File:     /tmp/dotgram-feedload-25958.txt, 649.8 MB
Parse:    10,000,002 lines in 12.6 s — 792,636 lines/s, 51.5 MB/s
Load:     10,000,002 lines in 58.0 s — 172,278 lines/s, 11.2 MB/s
By ISIN:  GB0002634946 834,868 trades, 41,716,273,797 shares
By ISIN:  US5949181045 834,594 trades, 41,725,018,446 shares
By ISIN:  CH0038863350 833,831 trades, 41,700,252,865 shares
Quantity: trailer 499,862,109,042, SUM(Quantity) 499,761,938,354 (the rejected lines are in the trailer and not in the table)
Database: /tmp/dotgram-feedload-25958.db, 910.1 MB; peak working set 116.6 MB

Loaded 9,998,000 records, rejected 2,000 (bad date 498, bad number 492, missing field 515, unknown record 495); trailer: expected 10,000,000 — OK
Timings: generate 8.0 s, parse 12.6 s, parse + load 58.0 s, queries 5.7 s
```

| | end-of-day trades, 10M | corporate actions, 1M | confirmations, 1M |
| --- | ---: | ---: | ---: |
| file | 649.8 MB | 73.1 MB | 62.9 MB |
| parse only | 12.6 s — 793 K lines/s, 51.5 MB/s | 1.6 s — 643 K lines/s, 47.0 MB/s | 1.0 s — 1.0 M lines/s, 63.0 MB/s |
| parse and load | 58.0 s — 172 K lines/s, 11.2 MB/s | 5.1 s — 197 K lines/s, 14.4 MB/s | 4.9 s — 203 K lines/s, 12.8 MB/s |
| database | 910.1 MB | 104.4 MB | 88.3 MB |
| peak working set | 116.6 MB | 116.4 MB | 113.3 MB |

The parser reads the trades at about 50 MB a second, and SQLite takes about a fifth of that:
loading is the database's time, not the parser's. Memory is flat — the peak working set was
109 MB for a million trades and 117 MB for ten million, and most of it is the runtime and
linq2db themselves.
`BulkCopyType.MultipleRows`, an `INSERT` of many rows at a time, is what the program uses;
`ProviderSpecific` measured the same, SQLite having no bulk interface of its own.
