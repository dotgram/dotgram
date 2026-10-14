using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using DotGram.Examples.Feeds;

namespace DotGram.FeedLoad;

// A trading feed of docs/feeds.md, written at size, read by its grammar and loaded into SQLite.
//
//     dotnet run -c Release --project examples/DotGram.FeedLoad -- [options]
//
//     --format pipe|csv|fixed  which feed (pipe: end-of-day trades)              default pipe
//     --records N              detail lines to write; 20, 50k, 1M, 10M ...         default 10M
//     --bad N                  how many of them are bad                            default N / 5,000
//     --seed N                 the same seed writes the same file                  default 1
//     --feed FILE              read this file instead of writing one
//     --db FILE                the database; kept. Without it, a temporary one is made and deleted
//     --keep                   keep the temporary feed and database, and say where they are
//
// The feed is read twice: once with nothing done with the records, which is what the parser
// costs, and once into the database. The second reading is never held anywhere: the parser's
// IEnumerable goes straight into linq2db's BulkCopy, the details as rows, and everything else
// — the header, the trailer, a rejected line — is handled as it passes, on the same connection,
// in the same transaction. A rejected line is a row of Rejects with where it is, the field it
// broke in, what kind of mistake that is, the line itself and what the parser said.
//
// What a value means — a settlement date before the trade date, a price nobody would pay — is
// not looked at. The grammar says what a line must look like; the rest is the business rules'.
static class Program
{
	static int Main(string[] args)
	{
		Options options;

		try
		{
			options = Options.Read(args);
		}
		catch (ArgumentException e)
		{
			Console.Error.WriteLine(e.Message);
			Console.Error.WriteLine("usage: [--format pipe|csv|fixed] [--records N] [--bad N] [--seed N] [--feed FILE] [--db FILE] [--keep]");
			return 2;
		}

		return FeedLoader.Run(options, Console.Out).TrailerMatches ? 0 : 1;
	}
}

/// <summary>The command line.</summary>
public sealed record Options(FeedFormat Format, long Records, long Bad, int Seed, string? Feed, string? Database, bool Keep)
{
	public static Options Read(string[] args)
	{
		var format   = FeedFormat.Pipe;
		var records  = 10_000_000L;
		long? bad    = null;
		var seed     = 1;
		string? feed = null;
		string? db   = null;
		var keep     = false;

		for (var at = 0; at < args.Length; at++)
		{
			switch (args[at])
			{
				case "--format":  format  = FormatOf(Value(args, ref at)); break;
				case "--records": records = Count(Value(args, ref at));  break;
				case "--bad":     bad     = Count(Value(args, ref at));  break;
				case "--seed":    seed    = SeedOf(Value(args, ref at));   break;
				case "--feed":    feed    = Value(args, ref at);         break;
				case "--db":      db      = Value(args, ref at);         break;
				case "--keep":    keep    = true;                        break;
				default:          throw new ArgumentException("Unknown option " + args[at] + ".");
			}
		}

		var wrong = bad ?? records / 5000;

		if (wrong > records)
			throw new ArgumentException("--bad cannot be more than --records.");

		return new Options(format, records, wrong, seed, feed, db, keep);
	}

	static int SeedOf(string text)
	{
		var count = Count(text);

		if (count > int.MaxValue)
			throw new ArgumentException("--seed is at most " + int.MaxValue.ToString(CultureInfo.InvariantCulture) + ".");

		return (int)count;
	}

	static string Value(string[] args, ref int at)
	{
		if (at + 1 >= args.Length)
			throw new ArgumentException(args[at] + " needs a value.");

		return args[++at];
	}

	static FeedFormat FormatOf(string text)
	{
		return text switch
		{
			"pipe"  => FeedFormat.Pipe,
			"csv"   => FeedFormat.Csv,
			"fixed" => FeedFormat.FixedWidth,
			_       => throw new ArgumentException("--format is pipe, csv or fixed."),
		};
	}

	// 20, 50k, 1M, 10M.
	public static long Count(string text)
	{
		if (text.Length == 0)
			throw new ArgumentException("Not a count: " + text + ".");

		var scale = text[^1] switch { 'k' or 'K' => 1_000L, 'm' or 'M' => 1_000_000L, _ => 1L };
		var digits = scale == 1 ? text : text[..^1];

		if (!long.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var count))
			throw new ArgumentException("Not a count: " + text + ".");

		try
		{
			return checked(count * scale);
		}
		catch (OverflowException)
		{
			throw new ArgumentException("Too large a count: " + text + ".");
		}
	}
}
