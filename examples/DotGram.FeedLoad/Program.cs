using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Examples.Feeds;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
using LinqToDB.Mapping;

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

		var temporary = Path.Combine(Path.GetTempPath(), "dotgram-feedload-" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
		var feed      = options.Feed ?? temporary + Extension(options.Format);
		var database  = options.Database ?? temporary + ".db";

		try
		{
			return Run(options, feed, database);
		}
		finally
		{
			if (!options.Keep)
			{
				if (options.Feed is null)
					File.Delete(feed);

				if (options.Database is null)
					File.Delete(database);
			}
		}
	}

	static int Run(Options options, string feed, string database)
	{
		var timings = new List<(string Phase, TimeSpan Elapsed)>();
		var clock   = Stopwatch.StartNew();

		if (options.Feed is null)
		{
			using (var writer = new StreamWriter(feed, false, new UTF8Encoding(false), 1 << 16))
			{
				var written = TradingFeedGenerator.Write(writer, options.Format, options.Records, options.Bad, options.Seed);

				Console.WriteLine(Invariant($"Feed:     {Name(options.Format)}, {written.Records:N0} records, {written.Bad:N0} bad ({Categories(written.BadByCategory)}), seed {options.Seed}"));
			}

			timings.Add(("generate", clock.Elapsed));
		}

		var bytes = new FileInfo(feed).Length;

		Console.WriteLine(Invariant($"File:     {feed}, {Megabytes(bytes)}"));

		// The parser alone: every record read and dropped.
		clock.Restart();

		long lines = 0;

		using (var reader = File.OpenText(feed))
			foreach (var _ in Read(options.Format, reader))
				lines++;

		var parsed = clock.Elapsed;

		timings.Add(("parse", parsed));
		Console.WriteLine(Invariant($"Parse:    {lines:N0} lines in {Seconds(parsed)} — {Rate(lines, parsed)} lines/s, {Throughput(bytes, parsed)}"));

		// The parser into the database.
		clock.Restart();

		Load load;

		using (var db = new DataConnection(new DataOptions()
			.UseSQLite("Data Source=" + database, SQLiteProvider.Microsoft)
			.UseMappingSchema(Mapping)))
		{
			load = Into(db, options.Format, feed);

			var loaded = clock.Elapsed;

			timings.Add(("parse + load", loaded));
			Console.WriteLine(Invariant($"Load:     {load.Lines:N0} lines in {Seconds(loaded)} — {Rate(load.Lines, loaded)} lines/s, {Throughput(bytes, loaded)}"));

			clock.Restart();
			Report(db, options.Format, load);
			timings.Add(("queries", clock.Elapsed));
		}

		var byCategory = load.Rejected
			.GroupBy(static reject => reject.Category)
			.ToDictionary(static group => group.Key, static group => (long)group.Count(), StringComparer.Ordinal);

		var read  = load.Loaded + load.Rejected.Count;
		var check = load.Trailer == read ? "OK" : "MISMATCH";

		Console.WriteLine(Invariant($"Database: {database}, {Megabytes(new FileInfo(database).Length)}; peak working set {Megabytes(Process.GetCurrentProcess().PeakWorkingSet64)}"));
		Console.WriteLine();
		Console.WriteLine(Invariant($"Loaded {load.Loaded:N0} records, rejected {load.Rejected.Count:N0}{(byCategory.Count == 0 ? "" : " (" + Categories(byCategory) + ")")}; trailer: expected {load.Trailer:N0} — {check}"));
		Console.WriteLine("Timings: " + string.Join(", ", timings.Select(static one => one.Phase + " " + Seconds(one.Elapsed))));

		if (options.Keep)
			Console.WriteLine("Kept:     " + feed + ", " + database);

		return check == "OK" ? 0 : 1;
	}

	/// <summary>What a load came to: the details that went in, the lines that did not, and what the trailer said.</summary>
	sealed record Load(long Lines, long Loaded, List<Reject> Rejected, long? Trailer, decimal? TrailerTotal);

	static Load Into(DataConnection db, FeedFormat format, string feed)
	{
		db.CreateTable<FeedFile>();
		db.CreateTable<Reject>();

		switch (format)
		{
			case FeedFormat.Pipe: db.CreateTable<EodTrade>();        break;
			case FeedFormat.Csv:  db.CreateTable<CorporateAction>(); break;
			default:              db.CreateTable<Confirmation>();    break;
		}

		var rejected = new List<Reject>();
		var file     = new FeedFile { Id = 1, Format = Name(format) };

		long?    trailer = null;
		decimal? total   = null;

		using var transaction = db.BeginTransaction();
		using var reader      = File.OpenText(feed);

		// Everything that is not a detail row, as it passes: a header becomes the file's row, a
		// trailer finishes it, and a rejected line is inserted where it was read.
		void Aside(object line)
		{
			switch (line)
			{
				case EodHeader     header: Header(header.Feed, header.BusinessDate, header.Sender);    break;
				case ActionHeader  header: Header(header.Feed, header.BusinessDate, header.Vendor);    break;
				case ConfirmHeader header: Header(header.File, header.BusinessDate, header.Custodian); break;

				case EodTrailer     end: trailer = end.Count; total = end.Quantity; break;
				case ActionTrailer  end: trailer = end.Count;                       break;
				case ConfirmTrailer end: trailer = end.Count; total = end.Amount;   break;

				case EodReject     reject: Rejected(reject.Line, reject.Column, reject.Field, reject.Text, reject.Message); break;
				case ActionReject  reject: Rejected(reject.Line, reject.Column, reject.Field, reject.Text, reject.Message); break;
				case ConfirmReject reject: Rejected(reject.Line, reject.Column, reject.Field, reject.Text, reject.Message); break;
			}
		}

		void Header(string name, string businessDate, string sender)
		{
			file.Feed         = name;
			file.BusinessDate = businessDate;
			file.Sender       = sender;

			db.Insert(file);
		}

		void Rejected(int line, int column, string field, string text, string message)
		{
			var reject = new Reject
			{
				Line     = line,
				Column   = column,
				Field    = field,
				Category = TradingFeedGenerator.Categorize(field, column, text),
				Text     = text,
				Message  = message,
			};

			db.Insert(reject);
			rejected.Add(reject);
		}

		// MultipleRows: an INSERT of many rows at a time. SQLite has no bulk interface of its own, and
		// ProviderSpecific measured the same.
		var copy = new BulkCopyOptions { BulkCopyType = BulkCopyType.MultipleRows };

		var loaded = format switch
		{
			FeedFormat.Pipe => db.BulkCopy(copy, Details<EodTrade>(EodTrades.Read(reader), Aside)).RowsCopied,
			FeedFormat.Csv  => db.BulkCopy(copy, Details<CorporateAction>(CorporateActions.Read(reader), Aside)).RowsCopied,
			_               => db.BulkCopy(copy, Details<Confirmation>(Confirmations.Read(reader), Aside)).RowsCopied,
		};

		file.TrailerCount = trailer;
		file.TrailerTotal = total;
		file.Loaded       = loaded;
		file.Rejected     = rejected.Count;

		db.Update(file);
		transaction.Commit();

		var lines = loaded + rejected.Count + (file.Feed is null ? 0 : 1) + (trailer is null ? 0 : 1);

		return new Load(lines, loaded, rejected, trailer, total);
	}

	/// <summary>The detail records of a feed for BulkCopy, everything else handed to <paramref name="aside"/> as it is read.</summary>
	static IEnumerable<TDetail> Details<TDetail>(IEnumerable<object> lines, Action<object> aside)
	{
		foreach (var line in lines)
		{
			if (line is TDetail detail)
				yield return detail;
			else
				aside(line);
		}
	}

	static IEnumerable<object> Read(FeedFormat format, TextReader reader)
	{
		return format switch
		{
			FeedFormat.Pipe => EodTrades.Read(reader),
			FeedFormat.Csv  => CorporateActions.Read(reader),
			_               => Confirmations.Read(reader),
		};
	}

	/// <summary>A query or two over what was loaded, and the trailer's total held against SQL's.</summary>
	static void Report(DataConnection db, FeedFormat format, Load load)
	{
		switch (format)
		{
			case FeedFormat.Pipe:
			{
				var top = db.GetTable<EodTrade>()
					.GroupBy(static trade => trade.Isin)
					.Select(static group => new { Isin = group.Key, Trades = group.Count(), Quantity = group.Sum(static trade => trade.Quantity) })
					.OrderByDescending(static row => row.Trades)
					.Take(3)
					.ToList();

				var sum = db.GetTable<EodTrade>().Sum(static trade => (long?)trade.Quantity) ?? 0;

				foreach (var row in top)
					Console.WriteLine(Invariant($"By ISIN:  {row.Isin} {row.Trades:N0} trades, {row.Quantity:N0} shares"));

				Console.WriteLine(Invariant($"Quantity: trailer {load.TrailerTotal:N0}, SUM(Quantity) {sum:N0}{Difference(load)}"));
				break;
			}

			case FeedFormat.Csv:
			{
				var kinds = db.GetTable<CorporateAction>()
					.GroupBy(static action => action.Kind)
					.Select(static group => new { Kind = group.Key, Count = group.Count() })
					.OrderBy(static row => row.Kind)
					.ToList();

				Console.WriteLine("By kind:  " + string.Join(", ", kinds.Select(static row => Invariant($"{row.Kind} {row.Count:N0}"))));
				break;
			}

			default:
			{
				var statuses = db.GetTable<Confirmation>()
					.GroupBy(static confirmation => confirmation.Status)
					.Select(static group => new { Status = group.Key, Count = group.Count() })
					.OrderBy(static row => row.Status)
					.ToList();

				var sum = db.GetTable<Confirmation>().Sum(static confirmation => (decimal?)confirmation.Amount) ?? 0;

				Console.WriteLine("By status: " + string.Join(", ", statuses.Select(static row => Invariant($"{row.Status} {row.Count:N0}"))));
				Console.WriteLine(Invariant($"Amount:   trailer {load.TrailerTotal:N2}, SUM(Amount) {sum:N2}{Difference(load)}"));
				break;
			}
		}

		// A small file's rejects in full, read back from the table.
		if (load.Rejected.Count is > 0 and <= 20)
		{
			Console.WriteLine();
			Console.WriteLine("SELECT Line, Column, Field, Category, Message, Text FROM Rejects");
			Console.WriteLine();
			Console.WriteLine("Line  Column  Field           Category        Message                   Text");

			foreach (var reject in db.GetTable<Reject>().OrderBy(static reject => reject.Line))
				Console.WriteLine(Invariant($"{reject.Line,4}  {reject.Column,6}  {reject.Field,-14}  {reject.Category,-14}  {reject.Message,-24}  {reject.Text}"));

			Console.WriteLine();
		}
	}

	static string Difference(Load load)
	{
		return load.Rejected.Count == 0 ? "" : " (the rejected lines are in the trailer and not in the table)";
	}

	static readonly MappingSchema Mapping = BuildMapping();

	// The records are the grammars' own types, which know nothing of a database; what table and
	// columns they become is said here, beside the one program that stores them.
	static MappingSchema BuildMapping()
	{
		var schema  = new MappingSchema();
		var builder = new FluentMappingBuilder(schema);

		builder.Entity<FeedFile>().HasTableName("Files")
			.Property(static file => file.Id).IsPrimaryKey()
			.Property(static file => file.Format).IsNullable(false)
			.Property(static file => file.Feed)
			.Property(static file => file.BusinessDate)
			.Property(static file => file.Sender)
			.Property(static file => file.TrailerCount)
			.Property(static file => file.TrailerTotal)
			.Property(static file => file.Loaded)
			.Property(static file => file.Rejected);

		builder.Entity<Reject>().HasTableName("Rejects")
			.Property(static reject => reject.Line)
			.Property(static reject => reject.Column)
			.Property(static reject => reject.Field)
			.Property(static reject => reject.Category)
			.Property(static reject => reject.Text)
			.Property(static reject => reject.Message);

		builder.Entity<EodTrade>().HasTableName("Trades")
			.Property(static trade => trade.Id).IsPrimaryKey()
			.Property(static trade => trade.TradeDate)
			.Property(static trade => trade.SettlementDate)
			.Property(static trade => trade.Isin)
			.Property(static trade => trade.Side).HasConversion(static side => side.ToString(), static text => Enum.Parse<Side>(text))
			.Property(static trade => trade.Quantity)
			.Property(static trade => trade.Price)
			.Property(static trade => trade.Currency);

		builder.Entity<CorporateAction>().HasTableName("CorporateActions")
			.Property(static action => action.Id).IsPrimaryKey()
			.Property(static action => action.Isin)
			.Property(static action => action.Issuer)
			.Property(static action => action.Kind).HasConversion(static kind => kind.ToString(), static text => Enum.Parse<ActionKind>(text))
			.Property(static action => action.ExDate)
			.Property(static action => action.PayDate)
			.Property(static action => action.Rate)
			.Property(static action => action.Currency);

		builder.Entity<Confirmation>().HasTableName("Confirmations")
			.Property(static confirmation => confirmation.Reference).IsPrimaryKey()
			.Property(static confirmation => confirmation.Isin)
			.Property(static confirmation => confirmation.SettlementDate)
			.Property(static confirmation => confirmation.Direction).HasConversion(static way => way.ToString(), static text => Enum.Parse<Direction>(text))
			.Property(static confirmation => confirmation.Quantity)
			.Property(static confirmation => confirmation.Amount)
			.Property(static confirmation => confirmation.Currency)
			.Property(static confirmation => confirmation.Status).HasConversion(static status => status.ToString(), static text => Enum.Parse<SettlementStatus>(text));

		builder.Build();

		return schema;
	}

	static string Name(FeedFormat format)
	{
		return format switch
		{
			FeedFormat.Pipe => "pipe",
			FeedFormat.Csv  => "csv",
			_               => "fixed",
		};
	}

	static string Extension(FeedFormat format)
	{
		return format == FeedFormat.Csv ? ".csv" : ".txt";
	}

	static string Categories(IReadOnlyDictionary<string, long> counts)
	{
		return string.Join(", ", counts.OrderBy(static one => one.Key, StringComparer.Ordinal).Select(static one => Invariant($"{one.Key} {one.Value:N0}")));
	}

	static string Seconds(TimeSpan elapsed)
	{
		return Invariant($"{elapsed.TotalSeconds:F1} s");
	}

	static string Rate(long count, TimeSpan elapsed)
	{
		return Invariant($"{count / Math.Max(elapsed.TotalSeconds, 1e-9):N0}");
	}

	static string Throughput(long bytes, TimeSpan elapsed)
	{
		return Invariant($"{bytes / 1048576.0 / Math.Max(elapsed.TotalSeconds, 1e-9):F1} MB/s");
	}

	static string Megabytes(long bytes)
	{
		return bytes < 1048576 ? Invariant($"{bytes / 1024.0:F1} KB") : Invariant($"{bytes / 1048576.0:F1} MB");
	}

	static string Invariant(FormattableString text)
	{
		return FormattableString.Invariant(text);
	}
}

/// <summary>A feed that was loaded: its header, what its trailer said, and what became of its lines.</summary>
sealed class FeedFile
{
	public int      Id           { get; set; }
	public string   Format       { get; set; } = "";
	public string?  Feed         { get; set; }
	public string?  BusinessDate { get; set; }
	public string?  Sender       { get; set; }
	public long?    TrailerCount { get; set; }
	public decimal? TrailerTotal { get; set; }
	public long     Loaded       { get; set; }
	public long     Rejected     { get; set; }
}

/// <summary>A line of a feed that is not a record: where it is, where it broke, and what the parser said.</summary>
sealed class Reject
{
	public int    Line     { get; set; }
	public int    Column   { get; set; }
	public string Field    { get; set; } = "";
	public string Category { get; set; } = "";
	public string Text     { get; set; } = "";
	public string Message  { get; set; } = "";
}

/// <summary>The command line.</summary>
sealed record Options(FeedFormat Format, long Records, long Bad, int Seed, string? Feed, string? Database, bool Keep)
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
				case "--seed":    seed    = (int)Count(Value(args, ref at)); break;
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
	static long Count(string text)
	{
		var scale = text[^1] switch { 'k' or 'K' => 1_000L, 'm' or 'M' => 1_000_000L, _ => 1L };
		var digits = scale == 1 ? text : text[..^1];

		if (!long.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var count))
			throw new ArgumentException("Not a count: " + text + ".");

		return count * scale;
	}
}
