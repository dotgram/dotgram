using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DotGram.Examples.Feeds;

// The three trading feeds of docs/feeds.md written out at any size — an end-of-day trade file
// (pipes), a corporate-actions file (CSV) and a settlement-confirmation file (fixed width) — so
// that their grammars can be read over something larger than a page. The same seed writes the
// same file, and the bad lines in it are the kinds a person or a feed gets wrong: a letter in a
// date, a number written the way a person writes one, a line that stops a field early, a quote
// left open, a record type nobody knows. Nothing here is semantically wrong and syntactically
// right; that is not the parser's business, so it is not what the parser is tested on.
//
// `Categorize` reads a rejected line back into the same words, from what the parser said about
// it: which field it stopped in, and where. The loader in examples/DotGram.FeedLoad counts its
// rejects with it, and the tests hold the count to what was written.

/// <summary>Which of the three feeds.</summary>
public enum FeedFormat
{
	/// <summary>The end-of-day trade file, <see cref="EodTrades"/>.</summary>
	Pipe,

	/// <summary>The corporate-actions file, <see cref="CorporateActions"/>.</summary>
	Csv,

	/// <summary>The settlement-confirmation file, <see cref="Confirmations"/>.</summary>
	FixedWidth,
}

/// <summary>What was written: how many detail lines, how many of them bad and of which kind, and the totals the trailer carries.</summary>
public sealed record GeneratedFeed(
	long                               Records,
	long                               Bad,
	IReadOnlyDictionary<string, long>  BadByCategory,
	long                               Quantity,
	decimal                            Amount);

/// <summary>Writes a trading feed of any size, with a given number of bad lines among the good ones.</summary>
public static class TradingFeedGenerator
{
	/// <summary>A letter where a date has a digit.</summary>
	public const string BadDate = "bad date";

	/// <summary>A letter or a thousands separator in a number.</summary>
	public const string BadNumber = "bad number";

	/// <summary>A line that ends before its last field.</summary>
	public const string MissingField = "missing field";

	/// <summary>A quoted field whose quote is never closed.</summary>
	public const string UnclosedQuote = "unclosed quote";

	/// <summary>A record type none of the rules starts with.</summary>
	public const string UnknownRecord = "unknown record";

	/// <summary>The business day every generated file is for.</summary>
	public static readonly DateTime BusinessDay = new(2026, 10, 2);

	// What is traded: an ISIN, its issuer, the currency it trades in and about what it costs.
	static readonly (string Isin, string Issuer, string Currency, decimal Price)[] Securities =
	[
		("US0378331005", "Apple Inc.",                     "USD", 227.52m),
		("US5949181045", "Microsoft Corp.",                "USD", 416.25m),
		("US88160R1014", "Tesla, Inc.",                    "USD", 243.85m),
		("US02079K3059", "Alphabet Inc. \"Class A\"",      "USD", 163.40m),
		("US0231351067", "Amazon.com, Inc.",               "USD", 186.20m),
		("DE0007164600", "SAP SE",                         "EUR", 212.40m),
		("DE0007236101", "Siemens AG, Reg.",               "EUR", 176.90m),
		("NL0010273215", "ASML Holding N.V.",              "EUR", 612.10m),
		("FR0000121014", "LVMH",                           "EUR", 598.30m),
		("GB0002634946", "BAE Systems plc",                "GBP",  13.24m),
		("GB00BP6MXD84", "Shell plc",                      "GBP",  27.38m),
		("CH0038863350", "Nestle S.A., Reg.",              "CHF",  86.12m),
	];

	static readonly string[] Categories = [BadDate, BadNumber, MissingField];

	/// <summary>
	/// Writes a feed of <paramref name="records"/> detail lines between a header and a trailer, exactly
	/// <paramref name="bad"/> of them bad, spread over the file by <paramref name="seed"/>.
	/// </summary>
	public static GeneratedFeed Write(TextWriter writer, FeedFormat format, long records, long bad, int seed)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(records);
		ArgumentOutOfRangeException.ThrowIfNegative(bad);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(bad, records);

		var random     = new Random(seed);
		var byCategory = new Dictionary<string, long>(StringComparer.Ordinal);
		var kinds      = format switch
		{
			FeedFormat.Pipe       => (string[])[.. Categories, UnknownRecord],
			FeedFormat.Csv        => [.. Categories, UnclosedQuote],
			FeedFormat.FixedWidth => [.. Categories, UnknownRecord],
			_                     => throw new ArgumentOutOfRangeException(nameof(format)),
		};

		long    quantity = 0;
		decimal amount   = 0;
		var     left     = bad;

		writer.WriteLine(Header(format));

		for (var at = 0L; at < records; at++)
		{
			// Exactly `bad` of the lines, each with the chance the lines still to come leave it.
			var category = left > 0 && random.NextInt64(records - at) < left ? kinds[random.Next(kinds.Length)] : null;

			if (category is not null)
			{
				left--;
				byCategory[category] = byCategory.GetValueOrDefault(category) + 1;
			}

			var (line, shares, value) = Detail(format, at + 1, random, category);

			writer.WriteLine(line);

			quantity += shares;
			amount   += value;
		}

		writer.WriteLine(Trailer(format, records, quantity, amount));

		return new GeneratedFeed(records, bad, byCategory, quantity, amount);
	}

	/// <summary>
	/// What kind of mistake a rejected line holds, from the field the parser stopped in, the column it
	/// stopped at, and the line itself — the categories <see cref="Write"/> writes.
	/// </summary>
	public static string Categorize(string field, int column, string text)
	{
		if (field == "Record")
			return UnknownRecord;

		if (CountQuotes(text) % 2 == 1)
			return UnclosedQuote;

		if (column > text.Length)
			return MissingField;

		return field switch
		{
			"TradeDate" or "SettlementDate" or "ExDate" or "PayDate" => BadDate,
			"Quantity"  or "Price"          or "Rate"   or "Amount"  => BadNumber,
			"(extra)"                                                 => "extra field",
			_                                                         => "bad " + field,
		};
	}

	static int CountQuotes(string text)
	{
		var count = 0;

		foreach (var c in text)
			if (c == '"')
				count++;

		return count;
	}

	static string Header(FeedFormat format)
	{
		return format switch
		{
			FeedFormat.Pipe => "H|EOD-TRADES|" + Iso(BusinessDay) + "|NORTHBRIDGE SECURITIES",
			FeedFormat.Csv  => "H,CORPORATE ACTIONS," + Iso(BusinessDay) + ",Meridian Data Services",
			_               => "H" + "SETTLCONF".PadRight(10) + Compact(BusinessDay) + "NORTH HARBOUR CUSTODY SERVICES",
		};
	}

	static string Trailer(FeedFormat format, long records, long quantity, decimal amount)
	{
		return format switch
		{
			FeedFormat.Pipe => Invariant($"T|{records}|{quantity}"),
			FeedFormat.Csv  => Invariant($"T,{records}"),
			_               => Invariant($"T{records:D9}{(long)(amount * 100):D18}"),
		};
	}

	static (string Line, long Quantity, decimal Amount) Detail(FeedFormat format, long number, Random random, string? category)
	{
		var security = Securities[random.Next(Securities.Length)];
		var shares   = random.Next(1, 100_000);
		var price    = decimal.Round(security.Price * (0.9m + (decimal)random.NextDouble() * 0.2m), 2);
		var settles  = BusinessDay.AddDays(random.Next(1, 4));

		switch (format)
		{
			case FeedFormat.Pipe:
			{
				if (category == BadNumber && shares < 1000)
					shares += 1000;

				var traded = Iso(BusinessDay);
				var side   = random.Next(2) == 0 ? "BUY" : "SELL";
				var count  = category == BadNumber ? shares.ToString("N0", CultureInfo.InvariantCulture) : Invariant($"{shares}");
				var line   = Invariant($"D|T-{number:D7}|{(category == BadDate ? Spoiled(traded) : traded)}|{Iso(settles)}|{security.Isin}|{side}|{count}|{price}");

				line = category switch
				{
					MissingField  => line,
					UnknownRecord => "X" + line[1..] + "|" + security.Currency,
					_             => line + "|" + security.Currency,
				};

				return (line, shares, 0);
			}

			case FeedFormat.Csv:
			{
				var kind    = random.Next(10) switch { < 7 => "DIV", < 9 => "SPLIT", _ => "RIGHTS" };
				var rate    = kind == "SPLIT" ? Invariant($"{random.Next(2, 21)}") : Invariant($"{decimal.Round(price / 100, 3)}");
				var paid    = kind == "SPLIT" ? "" : security.Currency;
				var exDate  = Iso(settles.AddDays(30));
				var issuer  = category == UnclosedQuote ? "\"" + security.Issuer.Replace("\"", "\"\"", StringComparison.Ordinal) : Quoted(security.Issuer);
				var line    = Invariant($"D,CA-{number:D7},{security.Isin},{issuer},{kind},{(category == BadDate ? Spoiled(exDate) : exDate)},{Iso(settles.AddDays(35))},{(category == BadNumber ? rate[..^1] + "O" : rate)}");

				return (category == MissingField ? line : line + "," + paid, 0, 0);
			}

			default:
			{
				var cents  = (long)(shares * price * 100);
				var status = random.Next(10) switch { < 7 => 'S', < 9 => 'P', _ => 'F' };
				var date   = Compact(settles);
				var line   = Invariant($"D{"T-" + number.ToString("D7", CultureInfo.InvariantCulture),-12}{security.Isin}{(category == BadDate ? date[..^1] + "O" : date)}{(random.Next(2) == 0 ? 'R' : 'D')}{(category == BadNumber ? shares.ToString(CultureInfo.InvariantCulture).PadLeft(12) : shares.ToString("D12", CultureInfo.InvariantCulture))}{cents:D15}{security.Currency}{status}");

				line = category switch
				{
					MissingField  => line[..^1],
					UnknownRecord => "X" + line[1..],
					_             => line,
				};

				return (line, shares, cents / 100m);
			}
		}
	}

	// A letter O where the day's first digit is: 2026-10-O2, the kind of thing a person types.
	static string Spoiled(string date)
	{
		return date[..^2] + "O" + date[^1..];
	}

	static string Quoted(string text)
	{
		return text.Contains(',', StringComparison.Ordinal) || text.Contains('"', StringComparison.Ordinal)
			? "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
			: text;
	}

	static string Iso(DateTime date)
	{
		return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
	}

	static string Compact(DateTime date)
	{
		return date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
	}

	static string Invariant(FormattableString text)
	{
		return FormattableString.Invariant(text);
	}
}
