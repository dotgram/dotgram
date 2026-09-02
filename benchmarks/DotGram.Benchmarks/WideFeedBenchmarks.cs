using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>What a settlement is: the twenty-five fields worth reading out of fifty.</summary>
/// <remarks>
/// A class rather than a record, and constructed by a `=>`: this is what a real consumer
/// binds to, and it is the allocation the parse is really paying for.
/// </remarks>
public sealed class Settlement(
	long     id,
	string   symbol,
	string   venue,
	int      quantity,
	decimal  price,
	decimal  fee,
	decimal  tax,
	DateOnly tradedOn,
	DateOnly settlesOn,
	DateTime bookedAt,
	string   currency,
	string   counterparty,
	string   account,
	string   book,
	string   trader,
	Side     side,
	int      lotSize,
	decimal  gross,
	decimal  net,
	string   status,
	string   reference,
	int      version,
	decimal  rate,
	string   note,
	long     sequence)
{
	public long     Id           { get; } = id;
	public string   Symbol       { get; } = symbol;
	public string   Venue        { get; } = venue;
	public int      Quantity     { get; } = quantity;
	public decimal  Price        { get; } = price;
	public decimal  Fee          { get; } = fee;
	public decimal  Tax          { get; } = tax;
	public DateOnly TradedOn     { get; } = tradedOn;
	public DateOnly SettlesOn    { get; } = settlesOn;
	public DateTime BookedAt     { get; } = bookedAt;
	public string   Currency     { get; } = currency;
	public string   Counterparty { get; } = counterparty;
	public string   Account      { get; } = account;
	public string   Book         { get; } = book;
	public string   Trader       { get; } = trader;
	public Side     Side         { get; } = side;
	public int      LotSize      { get; } = lotSize;
	public decimal  Gross        { get; } = gross;
	public decimal  Net          { get; } = net;
	public string   Status       { get; } = status;
	public string   Reference    { get; } = reference;
	public int      Version      { get; } = version;
	public decimal  Rate         { get; } = rate;
	public string   Note         { get; } = note;
	public long     Sequence     { get; } = sequence;
}

/// <summary>The line a wide feed opens with.</summary>
public sealed record FeedStart(string Source);

/// <summary>The line it closes with.</summary>
public sealed record FeedEnd(int Count);

/// <summary>Which way the trade went — a custom type, parsed from one character.</summary>
public enum Side
{
	Buy,
	Sell,
}

/// <summary>
/// Fifty pipe-separated fields, of which twenty-five are read into an object.
/// </summary>
/// <remarks>
/// <para>
/// The captured fields are scattered rather than the first twenty-five, because that is
/// what a real feed looks like and because it is the harder case: a skipped field still
/// has to be recognized, and every capture between them is a slot the machine keeps and
/// gives back on backtracking.
/// </para>
/// <para>
/// The types are the ones a settlement file actually carries — <c>long</c>, <c>int</c>,
/// <c>decimal</c>, <c>DateOnly</c>, <c>DateTime</c>, an enum of our own and a good deal
/// of text — and every one of them is converted by C# named from the grammar. What is
/// being measured is therefore the whole job, conversion included, and not a recognizer
/// that hands back substrings for somebody else to parse.
/// </para>
/// </remarks>
[Gram("""
	@using DotGram.Benchmarks;

	Feed    : @object[] = Header & Row* & Trailer & eof

	Header  : @object = "H" & Sep & source: Text & Sep & date: Date & eol
	                 => @(new FeedStart(source))

	Trailer : @object = "T" & Sep & count: Digits & eol
	                 => @(new FeedEnd(Number(count)))

	Row     : @object =
	          "R"
	        & Sep & id: Digits
	        & Sep & Skip
	        & Sep & symbol: Text
	        & Sep & Skip
	        & Sep & venue: Text
	        & Sep & Skip & Sep & Skip
	        & Sep & quantity: Digits
	        & Sep & Skip
	        & Sep & price: Money
	        & Sep & Skip & Sep & Skip
	        & Sep & fee: Money
	        & Sep & tax: Money
	        & Sep & Skip
	        & Sep & tradedOn: Date
	        & Sep & Skip
	        & Sep & settlesOn: Date
	        & Sep & Skip & Sep & Skip
	        & Sep & bookedAt: Stamp
	        & Sep & currency: Text
	        & Sep & Skip
	        & Sep & counterparty: Text
	        & Sep & account: Text
	        & Sep & Skip & Sep & Skip
	        & Sep & book: Text
	        & Sep & trader: Text
	        & Sep & Skip
	        & Sep & side: Way
	        & Sep & Skip
	        & Sep & lotSize: Digits
	        & Sep & gross: Money
	        & Sep & Skip & Sep & Skip
	        & Sep & net: Money
	        & Sep & status: Text
	        & Sep & Skip
	        & Sep & reference: Text
	        & Sep & Skip
	        & Sep & version: Digits
	        & Sep & rate: Money
	        & Sep & Skip & Sep & Skip
	        & Sep & note: Text
	        & Sep & sequence: Digits
	        & Sep & Skip & Sep & Skip & Sep & Skip
	        & eol
	       => @(new Settlement(
	              Big(id), symbol, venue, Number(quantity), Amount(price), Amount(fee),
	              Amount(tax), Day(tradedOn), Day(settlesOn), At(bookedAt), currency,
	              counterparty, account, book, trader, Which(side), Number(lotSize),
	              Amount(gross), Amount(net), status, reference, Number(version),
	              Amount(rate), note, Big(sequence)))

	Skip    = [^ '|' | '\r' | '\n']*
	Text    = [^ '|' | '\r' | '\n']+
	Digits  = ['0'..'9']+
	Money   = '-'? & ['0'..'9']+ & ('.' & ['0'..'9']+)?
	Date    = ['0'..'9']{4} & '-' & ['0'..'9']{2} & '-' & ['0'..'9']{2}
	Stamp   = Date & 'T' & ['0'..'9']{2} & ':' & ['0'..'9']{2} & ':' & ['0'..'9']{2}
	Way     = ['B' | 'S']
	Sep     = '|'

	parse Feed
	""", PartSize = 1500)]
public static partial class Settlements
{
	// The first grammar here that wants the size set, and it wants the coarse one. Most
	// grammars gain from being divided small — `ExpressionLanguage` is two to five times
	// faster for it — because their hot path is a fraction of a large machine and the
	// division puts that fraction in a method the JIT can hold in registers. This one is
	// the other shape: `Row` is a straight line of fifty fields and every record walks all
	// of it, so the hot path *is* the machine and cutting it finely only adds boundaries
	// to something that has to cross them anyway.
	//
	// Measured, interleaved in one process against a copy of this grammar differing only
	// in this number: four parts against twenty-two is 1.05, 0.99, 0.99, 1.00 at twenty
	// thousand records and 0.94, 0.98, 0.96, 0.96 at a hundred thousand — nothing to gain
	// and about four per cent to lose. A line-by-line profile says why: of four parts a
	// parse enters two, 3,015 times; of twenty-two it enters ten, 13,525 times.
	//
	// 1500 is what the generator did before whether-to-divide and how-large-a-part became
	// two numbers, so this is that shape asked for by name.

	static long     Big(string digits)   => long.Parse(digits, CultureInfo.InvariantCulture);
	static int      Number(string digits) => int.Parse(digits, CultureInfo.InvariantCulture);
	static decimal  Amount(string money) => decimal.Parse(money, CultureInfo.InvariantCulture);
	static DateOnly Day(string date)     => DateOnly.Parse(date, CultureInfo.InvariantCulture);
	static DateTime At(string stamp)     => DateTime.Parse(stamp, CultureInfo.InvariantCulture);
	static Side     Which(string side)   => side == "B" ? Side.Buy : Side.Sell;
}

/// <summary>
/// A wide feed — fifty fields a record, twenty-five of them built into an object.
/// </summary>
/// <remarks>
/// <para>
/// The size worth caring about. A million records is around 190 MB of text, which the
/// string overload has to hold entire before it can answer anything, on top of a million
/// settlements; the streamed one holds a 4 KB window and whatever the caller keeps. That
/// is the difference this exists to show, and at ten million the string case does not
/// run at all — which is itself the answer.
/// </para>
/// <para>
/// The caller here keeps nothing: it adds up two columns and counts. Keeping the objects
/// would hold the feed in every case and measure the consumer instead of the parse.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class WideFeedBenchmarks
{
	/// <summary>How many records the feed carries.</summary>
	/// <remarks>
	/// A hundred thousand to have both sides in the table, and a million because that is
	/// where holding the input stops being a choice.
	/// <para>
	/// Ten million was run once by hand and is not in the list because it takes a quarter
	/// of an hour to set up: 1.9 GB of feed, read through the same 4 KB window in 13.6
	/// seconds, with no second-generation collection at all. The string overload was not
	/// asked — 1.9 billion characters is 3.8 GB in one object, which stops being a choice
	/// somewhere well before that.
	/// </para>
	/// </remarks>
	[Params(100_000, 1_000_000)]
	public int Records { get; set; }

	string _path = "";

	[GlobalSetup]
	public void Setup()
	{
		_path = Path.Combine(Path.GetTempPath(), $"dotgram-wide-{Records}.feed");

		if (!File.Exists(_path))
			Write(_path, Records);

		// Both doors must answer the same, or the numbers are about two different things.
		// On the largest size this is the one time the whole file is held.
		if (Records <= 100_000)
		{
			using var reader = File.OpenText(_path);

			if (Total(Settlements.ParseFeed(File.ReadAllText(_path))) != Total(Settlements.ParseFeed(reader)))
				throw new InvalidOperationException("The two overloads disagree about the same feed.");
		}
	}

	static void Write(string path, int records)
	{
		using var file = new StreamWriter(path);

		file.Write("H|ACME|2026-08-14\n");

		var row = new StringBuilder();

		for (var i = 0; i < records; i++)
		{
			row.Clear().Append('R');

			// Fifty fields in all: the ones read out carry values, the rest carry filler
			// of a length that varies, so the window boundary walks through a record
			// rather than sitting at one offset in every one of them.
			for (var field = 1; field <= 50; field++)
				row.Append('|').Append(Field(field, i));

			file.Write(row.Append('\n').ToString());
		}

		file.Write("T|" + records.ToString(CultureInfo.InvariantCulture) + "\n");
	}

	/// <summary>What each field carries, by its place in the record.</summary>
	static string Field(int field, int record) => field switch
	{
		1      => record.ToString(CultureInfo.InvariantCulture),
		3      => "AAPL",
		5      => "XLON",
		8      => (record % 5000 + 1).ToString(CultureInfo.InvariantCulture),
		10     => (record % 10000 + 1) + ".25",
		13     => "1.5",
		14     => "0.75",
		16     => "2026-08-12",
		18     => "2026-08-14",
		21     => "2026-08-12T09:30:00",
		22     => "USD",
		24     => "CPTY" + record % 97,
		25     => "ACC" + record % 41,
		28     => "BOOK" + record % 13,
		29     => "TRD" + record % 29,
		31     => record % 2 == 0 ? "B" : "S",
		33     => "100",
		34     => (record % 7919 + 1) + ".00",
		37     => (record % 7919 + 1) + ".50",
		38     => "SETTLED",
		40     => "REF" + record,
		42     => (record % 9 + 1).ToString(CultureInfo.InvariantCulture),
		43     => "1.0",
		46     => "note " + record % 3,
		47     => record.ToString(CultureInfo.InvariantCulture),
		_      => "x" + record % 100,
	};

	/// <summary>What a caller does with the records, keeping none of them.</summary>
	static (int Count, long Quantity, decimal Net) Total(IEnumerable<object> parts)
	{
		var count    = 0;
		var quantity = 0L;
		var net      = 0m;

		foreach (var part in parts)
			if (part is Settlement settlement)
			{
				count++;
				quantity += settlement.Quantity;
				net      += settlement.Net;
			}

		return (count, quantity, net);
	}

	[Benchmark(Baseline = true, Description = "string")]
	public int Whole() => Total(Settlements.ParseFeed(File.ReadAllText(_path))).Count;

	[Benchmark(Description = "TextReader")]
	public int Reader()
	{
		using var reader = File.OpenText(_path);

		return Total(Settlements.ParseFeed(reader)).Count;
	}

	[Benchmark(Description = "File.ReadLines")]
	public int Lines() => Total(Settlements.ParseFeed(File.ReadLines(_path))).Count;
}
