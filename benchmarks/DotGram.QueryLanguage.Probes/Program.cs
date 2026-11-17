using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using DotGram.QueryLanguage;
using DotGram.QueryLanguage.LinqToDB;

using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using LinqToDB.Internal.Linq;
using LinqToDB.Mapping;

// Probes of the query language against linq2db. Every number is directional: one process, a
// shared machine, no A/A. `dotnet run -c Release -- <mode> [--mssql]`; modes are listed at the end.

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var mode   = args.Length > 0 ? args[0] : "help";
var mssql  = args.Contains("--mssql");
var rest   = args.Skip(1).Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();

switch (mode)
{
	case "chain":      Probes.Chain(rest);        break;
	case "phases":     Probes.Phases(mssql);      break;
	case "values":     Probes.Values();           break;
	case "shapes":     Probes.Shapes(rest);       break;
	case "nodes":      Probes.Nodes(rest);        break;
	case "depth":      Probes.Depth();            break;
	case "in":         Probes.In(rest);           break;
	case "parse":      Probes.Parse();            break;
	case "sql":        Probes.Sql(rest, mssql);   break;
	case "timeout":    Probes.Timeout(mssql);     break;
	case "cold":       Probes.Cold(mssql);        break;

	default:
		Console.WriteLine("modes: chain <n> left|balanced | phases [--mssql] | values | shapes <threads> <seconds> <predicates> | nodes | depth | in | parse | sql <text> [--mssql] | timeout [--mssql] | cold [--mssql]");
		break;
}

[Table("Orders")]
public sealed class Order
{
	[PrimaryKey]       public int      Id       { get; set; }
	[Column, NotNull]  public int      TenantId { get; set; }
	[Column, Nullable] public string?  Status   { get; set; }
	[Column, NotNull]  public decimal  Total    { get; set; }
	[Column, NotNull]  public long     Big      { get; set; }
	[Column, NotNull]  public double   Score    { get; set; }
	[Column, NotNull]  public DateTime Created  { get; set; }
	[Column, Nullable] public int?     Priority { get; set; }
	[Column, NotNull]  public string   Customer { get; set; } = "";
}

public sealed record Tenant(int Id);

static class Probes
{
	static readonly QlEntity<Order> Orders;
	static readonly QlBuiltModel    Model;

	static Probes()
	{
		var model = QlModel.Create();

		Orders = model.Entity<Order>("orders", o => o.Id)
			.Column("id",       o => o.Id)
			.Column("status",   o => o.Status)
			.Column("total",    o => o.Total)
			.Column("big",      o => o.Big)
			.Column("score",    o => o.Score)
			.Column("created",  o => o.Created)
			.Column("priority", o => o.Priority)
			.Column("customer", o => o.Customer);

		Model = model.Build();
	}

	static IEnumerable<Order> Rows(int count)
	{
		string?[] statuses = ["paid", "new", null, "shipped"];

		for (var i = 1; i <= count; i++)
		{
			yield return new Order
			{
				Id       = i,
				TenantId = i % 2,
				Status   = statuses[i % statuses.Length],
				Total    = i * 12.5m,
				Big      = i * 1_000_000_000L,
				Score    = i / 4.0,
				Created  = new DateTime(2026, 1, 1).AddMinutes(i),
				Priority = i % 3 == 0 ? null : i % 3,
				Customer = "c" + (i % 97),
			};
		}
	}

	static DataConnection Connect(bool mssql, int rows = 100)
	{
		if (!mssql)
		{
			var db = new DataConnection(new DataOptions().UseSQLite("Data Source=:memory:"));

			db.CreateTable<Order>();
			db.BulkCopy(Rows(rows));

			return db;
		}

		var connection = Environment.GetEnvironmentVariable("QL_MSSQL") ?? throw new InvalidOperationException("QL_MSSQL is not set.");

		using (var master = new DataConnection(new DataOptions().UseSqlServer(connection)))
			master.Execute("IF DB_ID('QlSpike') IS NULL CREATE DATABASE QlSpike");

		var server = new DataConnection(new DataOptions().UseSqlServer(connection + ";Initial Catalog=QlSpike"));

		server.DropTable<Order>(tableName: "ProbeOrders", throwExceptionIfNotExists: false);
		server.CreateTable<Order>(tableName: "ProbeOrders");
		server.GetTable<Order>().TableName("ProbeOrders").BulkCopy(new BulkCopyOptions { BulkCopyType = BulkCopyType.ProviderSpecific }, Rows(rows));

		return server;
	}

	static IQueryable<Order> Root(DataConnection db, int tenant)
	{
		var table = db.GetTable<Order>();

		if (db.DataProvider.Name.Contains("SqlServer", StringComparison.Ordinal))
			table = table.TableName("ProbeOrders");

		return table.Where(o => o.TenantId == tenant);
	}

	static readonly QlLinqToDbAdapter Adapter = new(QlLikeDialect.Standard);

	static long Allocated()
	{
		return GC.GetAllocatedBytesForCurrentThread();
	}

	static string Ms(double ms)
	{
		return ms < 1 ? $"{ms * 1000:0.0} µs" : ms < 1000 ? $"{ms:0.00} ms" : $"{ms / 1000:0.00} s";
	}

	static string Mb(long bytes)
	{
		return bytes < 1 << 20 ? $"{bytes / 1024.0:0.0} KB" : $"{bytes / 1048576.0:0.0} MB";
	}

	static double Percentile(List<double> values, double p)
	{
		values.Sort();

		return values[Math.Min(values.Count - 1, (int)(values.Count * p))];
	}

	// ── chain: linq2db on a chain of + built left-deep or balanced, without the query language ──

	public static void Chain(string[] args)
	{
		var counts = args.Length > 0 ? args[0].Split(',').Select(int.Parse).ToArray() : [4, 8, 12, 14, 16];
		var shapes = args.Length > 1 ? args[1].Split(',') : ["left", "balanced"];

		using var db = Connect(false);

		Console.WriteLine("| shape | operands | first execution (translation) | allocated |");
		Console.WriteLine("|---|---|---|---|");

		foreach (var shape in shapes)
		{
			foreach (var n in counts)
			{
				var o      = Expression.Parameter(typeof(Order), "o");
				var leaves = Enumerable.Range(0, n).Select(_ => (Expression)Expression.Property(o, nameof(Order.Id))).ToArray();
				var sum    = shape == "left" ? leaves.Aggregate(Expression.Add) : Balanced(leaves, 0, leaves.Length);
				var where  = Expression.Lambda<Func<Order, bool>>(Expression.Equal(Expression.Property(o, nameof(Order.Id)), sum), o);

				Query<Order>.ClearCache();

				var before = Allocated();
				var clock  = Stopwatch.StartNew();
				var count  = db.GetTable<Order>().Where(where).Count();

				clock.Stop();

				Console.WriteLine($"| {shape} | {n} | {Ms(clock.Elapsed.TotalMilliseconds)} | {Mb(Allocated() - before)} |");
			}
		}
	}

	static Expression Balanced(Expression[] items, int start, int end)
	{
		if (end - start == 1)
			return items[start];

		var middle = start + (end - start) / 2;

		return Expression.Add(Balanced(items, start, middle), Balanced(items, middle, end));
	}

	// ── phases: what each phase of a query costs, ours against linq2db's ──

	static readonly string[] Texts =
	[
		"SELECT id FROM orders WHERE id = 5",
		"SELECT id, status, total FROM orders WHERE total > 100 AND status <> 'new' ORDER BY total DESC LIMIT 20",
		"SELECT * FROM orders WHERE (status = 'paid' OR status = 'shipped') AND created >= TIMESTAMP '2026-01-01 00:30:00' AND priority IS NOT NULL ORDER BY created LIMIT 10 OFFSET 5",
		"SELECT id, customer FROM orders WHERE customer LIKE 'c1%' AND id IN (1, 2, 3, 5, 8, 13, 21, 34) AND total BETWEEN 10 AND 500",
		"SELECT id FROM orders WHERE id = 1 OR id = 2 OR id = 3 OR id = 4 OR id = 5 OR id = 6 OR id = 7 OR id = 8 OR id = 9 OR id = 10 OR id = 11 OR id = 12",
	];

	public static void Phases(bool mssql)
	{
		using var db = Connect(mssql);

		var root = Root(db, 1);

		Console.WriteLine($"provider: {db.DataProvider.Name}");
		Console.WriteLine();
		Console.WriteLine("| text | nodes | depth | parse | bind | translate + audit | parse+bind+translate | linq2db miss (execute) | linq2db hit (execute) | alloc ours | alloc miss |");
		Console.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|");

		for (var t = 0; t < Texts.Length; t++)
		{
			var text = Texts[t];

			// Warm the code paths once, outside the numbers.
			Execute(QlQuery.Parse(text, Model).Translate(root, null, Adapter, out _));

			var parse     = new List<double>();
			var bind      = new List<double>();
			var translate = new List<double>();
			var total     = new List<double>();
			var miss      = new List<double>();
			var hit       = new List<double>();
			long oursBytes = 0;
			long missBytes = 0;
			Audit.Stats stats = default;

			const int rounds = 200;

			for (var i = 0; i < rounds; i++)
			{
				var a0 = Allocated();
				var c  = Stopwatch.StartNew();

				var syntax = QlQuery.Read(text, QlLimits.Default);
				var p      = c.Elapsed.TotalMilliseconds;
				var query  = QlQuery.Parse(text, Model);
				var pb     = c.Elapsed.TotalMilliseconds;
				var q      = query.Translate(root, null, Adapter, out stats);
				var all    = c.Elapsed.TotalMilliseconds;

				// Parse ran twice above (Read, then Parse); bind is what Parse costs beyond a read.
				parse.Add(p);
				bind.Add(pb - p - p);
				translate.Add(all - pb);
				total.Add(all - p);
				oursBytes += Allocated() - a0;

				Query<object?[]>.ClearCache();

				var a1 = Allocated();
				var m  = Stopwatch.StartNew();

				Execute(q);
				miss.Add(m.Elapsed.TotalMilliseconds);
				missBytes += Allocated() - a1;

				var h = Stopwatch.StartNew();

				Execute(query.Translate(root, null, Adapter, out _));
				hit.Add(h.Elapsed.TotalMilliseconds);
			}

			string P(List<double> v)
			{
				return $"{Ms(Percentile(v, 0.5))} / {Ms(Percentile(v, 0.99))}";
			}

			Console.WriteLine($"| T{t + 1} | {stats.Nodes} | {stats.Depth} | {P(parse)} | {P(bind)} | {P(translate)} | {P(total)} | {P(miss)} | {P(hit)} | {Mb(oursBytes / rounds)} | {Mb(missBytes / rounds)} |");
		}

		Console.WriteLine();
		Console.WriteLine("p50 / p99 of 200 rounds. 'miss' is the first execution after Query<object[]>.ClearCache(): linq2db translation, SQL, the command and its rows; 'hit' the same query again, a new tree with new holders.");

		for (var t = 0; t < Texts.Length; t++)
			Console.WriteLine($"T{t + 1}: {Texts[t]}");

		Console.WriteLine();
		Console.WriteLine("SQL of T2:");
		Console.WriteLine(QlQuery.Parse(Texts[1], Model).Translate(root, null, Adapter, out _).ToSqlQuery().Sql);
	}

	static int Execute(IQueryable<object?[]> query)
	{
		return query.ToList().Count;
	}

	// ── values: is a new value a new shape for linq2db's cache? ──

	public static void Values()
	{
		using var db = Connect(false);

		var root = Root(db, 1);

		Console.WriteLine("| what changes between executions | executions | linq2db cache misses |");
		Console.WriteLine("|---|---|---|");

		Count("nothing (same text, new holders)", 50, i => ("SELECT id FROM orders WHERE id = 5", null));
		Count("a literal of the text", 50, i => ($"SELECT id FROM orders WHERE id = {i}", null));
		Count("a parameter's value", 50, i => ("SELECT id FROM orders WHERE id = @p", new Dictionary<string, object?> { ["p"] = i }));
		Count("an IN list's values (same length)", 50, i => ($"SELECT id FROM orders WHERE id IN ({i}, {i + 1}, {i + 2})", null));
		Count("an IN list's length", 50, i => ($"SELECT id FROM orders WHERE id IN ({string.Join(", ", Enumerable.Range(1, i + 1))})", null));
		Count("an IN array parameter's length", 50, i => ("SELECT id FROM orders WHERE id IN @ids", new Dictionary<string, object?> { ["ids"] = Enumerable.Range(1, i + 1).ToArray() }));
		Count("a LIKE pattern's value", 50, i => ($"SELECT id FROM orders WHERE customer LIKE 'c{i}%'", null));
		Count("LIMIT's value", 50, i => ($"SELECT id FROM orders LIMIT {i + 1}", null));
		Count("the shape (OR of i+1 predicates)", 30, i => ("SELECT id FROM orders WHERE " + string.Join(" OR ", Enumerable.Range(1, i + 1).Select(k => $"id = {k}")), null));

		void Count(string what, int executions, Func<int, (string Text, Dictionary<string, object?>? Parameters)> make)
		{
			Query<object?[]>.ClearCache();

			var before = Query<object?[]>.CacheMissCount;

			for (var i = 0; i < executions; i++)
			{
				var (text, parameters) = make(i);
				var limits = QlLimits.Default with { MaxNodes = 1000, MaxTreeDepth = 64 };

				Execute(QlQuery.Parse(text, Model, limits).Translate(root, parameters, Adapter, out _));
			}

			Console.WriteLine($"| {what} | {executions} | {Query<object?[]>.CacheMissCount - before} |");
		}
	}

	// ── shapes: executions per second when every text is a new shape ──

	public static void Shapes(string[] args)
	{
		var threads    = args.Length > 0 ? int.Parse(args[0]) : 8;
		var seconds    = args.Length > 1 ? double.Parse(args[1]) : 5;
		var predicates = args.Length > 2 ? int.Parse(args[2]) : 6;

		Console.WriteLine($"threads {threads}, {seconds} s each, {predicates} predicates a query");
		Console.WriteLine();
		Console.WriteLine("| texts | executions/s | linq2db misses/s | p50 | p99 |");
		Console.WriteLine("|---|---|---|---|---|");

		Run("one shape, values vary", false);
		Run("every text a new shape", true);

		void Run(string label, bool vary)
		{
			Query<object?[]>.ClearCache();

			var stop     = DateTime.UtcNow.AddSeconds(seconds);
			var before   = Query<object?[]>.CacheMissCount;
			var times    = new List<double>();
			var total    = 0L;
			var lockTime = new object();
			var clock    = Stopwatch.StartNew();

			Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, worker =>
			{
				using var db     = Connect(false);
				var       root   = Root(db, 1);
				var       random = new Random(worker * 7919 + 17);
				var       local  = new List<double>();

				while (DateTime.UtcNow < stop)
				{
					var text = vary ? RandomShape(random, predicates) : "SELECT id FROM orders WHERE " + string.Join(" OR ", Enumerable.Range(0, predicates).Select(k => $"id = {random.Next(100)}"));
					var c    = Stopwatch.StartNew();

					try
					{
						Execute(QlQuery.Parse(text, Model).Translate(root, null, Adapter, out _));
					}
					catch (QlException)
					{
						// A shape over a cap is refused and still counted: it is what an attacker pays for.
					}

					local.Add(c.Elapsed.TotalMilliseconds);
				}

				lock (lockTime)
				{
					times.AddRange(local);
					total += local.Count;
				}
			});

			var elapsed = clock.Elapsed.TotalSeconds;
			var misses  = Query<object?[]>.CacheMissCount - before;

			Console.WriteLine($"| {label} | {total / elapsed:0} | {misses / elapsed:0} | {Ms(Percentile(times, 0.5))} | {Ms(Percentile(times, 0.99))} |");
		}
	}

	static readonly string[] Fields    = ["id", "status", "total", "big", "score", "created", "priority", "customer"];
	static readonly string[] Operators = ["=", "<>", "<", ">", "<=", ">="];

	static string RandomShape(Random random, int predicates)
	{
		var parts = new List<string>();

		for (var i = 0; i < predicates; i++)
		{
			var field = Fields[random.Next(Fields.Length)];
			var value = field switch
			{
				"status" or "customer" => "'x'",
				"created"              => "TIMESTAMP '2026-01-01 00:00:00'",
				"total"                => "1.5",
				"score"                => "1E0",
				_                      => "3",
			};

			parts.Add(random.Next(5) switch
			{
				0 => $"{field} IS NULL",
				1 => $"NOT {field} {Operators[random.Next(Operators.Length)]} {value}",
				_ => $"{field} {Operators[random.Next(Operators.Length)]} {value}",
			});
		}

		var text = new System.Text.StringBuilder("SELECT id FROM orders WHERE ").Append(parts[0]);

		for (var i = 1; i < parts.Count; i++)
			text.Append(random.Next(2) == 0 ? " AND " : " OR ").Append(parts[i]);

		return text.ToString();
	}

	// ── nodes: linq2db's translation against the size of the tree ──

	public static void Nodes(string[] args)
	{
		using var db = Connect(false);

		var root   = Root(db, 1);
		var limits = QlLimits.Default with { MaxNodes = 100_000, MaxTreeDepth = 1000, MaxSyntaxDepth = 10_000 };
		var counts = args.Length > 0 ? args[0].Split(',').Select(int.Parse).ToArray() : [1, 4, 8, 16, 32, 64, 128, 256];

		Console.WriteLine("| OR of k distinct predicates | tree nodes | tree depth | miss (execute) | allocated | hit |");
		Console.WriteLine("|---|---|---|---|---|---|");

		foreach (var k in counts)
		{
			var text = "SELECT id FROM orders WHERE " + string.Join(" OR ", Enumerable.Range(0, k).Select(i => i % 3 == 0 ? $"id = {i}" : i % 3 == 1 ? $"total > {i}" : $"status = 's{i}'"));

			var query = QlQuery.Parse(text, Model, limits);
			var q     = query.Translate(root, null, Adapter, out var stats);

			Query<object?[]>.ClearCache();

			var a = Allocated();
			var c = Stopwatch.StartNew();

			Execute(q);

			var miss  = c.Elapsed.TotalMilliseconds;
			var bytes = Allocated() - a;

			c.Restart();
			Execute(query.Translate(root, null, Adapter, out _));

			Console.WriteLine($"| {k} | {stats.Nodes} | {stats.Depth} | {Ms(miss)} | {Mb(bytes)} | {Ms(c.Elapsed.TotalMilliseconds)} |");
		}
	}

	// ── depth: nested NOT and parentheses, as written and as built ──

	public static void Depth()
	{
		using var db = Connect(false);

		var root   = Root(db, 1);
		var limits = QlLimits.Default with { MaxNodes = 100_000, MaxTreeDepth = 100_000, MaxSyntaxDepth = 100_000, MaxTextLength = 1 << 20 };

		Console.WriteLine("| text | k | syntax depth | tree nodes | tree depth | parse+bind+translate | miss (execute) | allocated |");
		Console.WriteLine("|---|---|---|---|---|---|---|---|");

		foreach (var k in new[] { 8, 32, 64, 256, 1000 })
		{
			Row("NOT × k", k, "SELECT id FROM orders WHERE " + string.Concat(Enumerable.Repeat("NOT ", k)) + "id = 1");
			Row("( × k", k, "SELECT id FROM orders WHERE " + new string('(', k) + "id = 1" + new string(')', k));
		}

		void Row(string label, int k, string text)
		{
			try
			{
				var c     = Stopwatch.StartNew();
				var query = QlQuery.Parse(text, Model, limits);
				var q     = query.Translate(root, null, Adapter, out var stats);
				var ours  = c.Elapsed.TotalMilliseconds;

				Query<object?[]>.ClearCache();

				var a = Allocated();

				c.Restart();
				Execute(q);

				Console.WriteLine($"| {label} | {k} | {QlQuery.Read(text, limits).Where!.Depth} | {stats.Nodes} | {stats.Depth} | {Ms(ours)} | {Ms(c.Elapsed.TotalMilliseconds)} | {Mb(Allocated() - a)} |");
			}
			catch (Exception e)
			{
				Console.WriteLine($"| {label} | {k} | | | | {e.GetType().Name}: {e.Message.Split('\n')[0]} | | |");
			}
		}
	}

	// ── in: IN list cardinality ──

	public static void In(string[] args)
	{
		using var db = Connect(false, 5000);

		var root   = Root(db, 1);
		var limits = QlLimits.Default with { MaxInList = 100_000, MaxTextLength = 1 << 20 };

		Console.WriteLine("| IN of n | written: miss | written: hit | array parameter: miss | array parameter: hit | SQL length |");
		Console.WriteLine("|---|---|---|---|---|---|");

		foreach (var n in new[] { 10, 100, 1000, 5000 })
		{
			var list    = Enumerable.Range(1, n).ToArray();
			var written = QlQuery.Parse($"SELECT id FROM orders WHERE id IN ({string.Join(", ", list)})", Model, limits);
			var passed  = QlQuery.Parse("SELECT id FROM orders WHERE id IN @ids", Model, limits);
			var values  = new Dictionary<string, object?> { ["ids"] = list };

			var (wm, wh) = MissHit(() => written.Translate(root, null, Adapter, out _));
			var (pm, ph) = MissHit(() => passed.Translate(root, values, Adapter, out _));
			var sql      = passed.Translate(root, values, Adapter, out _).ToSqlQuery().Sql.Length;

			Console.WriteLine($"| {n} | {Ms(wm)} | {Ms(wh)} | {Ms(pm)} | {Ms(ph)} | {sql} |");
		}

		static (double Miss, double Hit) MissHit(Func<IQueryable<object?[]>> make)
		{
			Query<object?[]>.ClearCache();

			var c = Stopwatch.StartNew();

			Execute(make());

			var miss = c.Elapsed.TotalMilliseconds;

			c.Restart();
			Execute(make());

			return (miss, c.Elapsed.TotalMilliseconds);
		}
	}

	// ── parse: the grammar on texts at the length cap ──

	public static void Parse()
	{
		var limits = QlLimits.Default with { MaxTextLength = 1 << 20 };

		Console.WriteLine("| text | length | read | result |");
		Console.WriteLine("|---|---|---|---|");

		Row("( × 4000, refused by depth", "SELECT id FROM orders WHERE " + new string('(', 4000) + "id = 1" + new string(')', 4000));
		Row("( × 4000 unclosed", "SELECT id FROM orders WHERE " + new string('(', 4000) + "id = 1");
		Row("NOT × 2000", "SELECT id FROM orders WHERE " + string.Concat(Enumerable.Repeat("NOT ", 2000)) + "id = 1");
		Row("- × 4000", "SELECT id FROM orders WHERE id = " + new string('-', 4000) + "1");
		Row("OR × 1000", "SELECT id FROM orders WHERE " + string.Join(" OR ", Enumerable.Repeat("id = 1", 1000)));
		Row("+ × 1000", "SELECT id FROM orders WHERE id = " + string.Join(" + ", Enumerable.Repeat("id", 1000)));
		Row("8 KB string", "SELECT id FROM orders WHERE status = '" + new string('x', 8000) + "'");
		Row("8 KB of a refused text", "SELECT id FROM orders WHERE " + string.Concat(Enumerable.Repeat("id = 1 AND ", 700)) + "=");

		void Row(string label, string text)
		{
			var c = Stopwatch.StartNew();
			string result;

			try
			{
				QlQuery.Parse(text, Model, limits);
				result = "bound";
			}
			catch (QlException e)
			{
				result = e.Error.Kind + ": " + e.Error.Message;
			}

			Console.WriteLine($"| {label} | {text.Length} | {Ms(c.Elapsed.TotalMilliseconds)} | {result} |");
		}
	}

	// ── sql: the command a text becomes ──

	public static void Sql(string[] args, bool mssql)
	{
		using var db = Connect(mssql);

		var text = args.Length > 0 ? args[0] : Texts[1];
		var q    = QlQuery.Parse(text, Model).Translate(Root(db, 1), null, new QlLinqToDbAdapter(mssql ? QlLikeDialect.SqlServer : QlLikeDialect.Standard), out var stats);

		Console.WriteLine($"-- {text}");
		Console.WriteLine($"-- nodes {stats.Nodes}, depth {stats.Depth}");
		Console.WriteLine(q.ToSqlQuery().Sql);
	}

	static async Task<int> Drain(IAsyncEnumerable<object?[]> rows)
	{
		var count = 0;

		await foreach (var _ in rows)
			count++;

		return count;
	}

	// ── cold: the first query of a process, phase by phase ──

	public static void Cold(bool mssql)
	{
		var process = Stopwatch.StartNew();

		using var db = Connect(mssql);

		var connect = process.Elapsed.TotalMilliseconds;
		var root    = Root(db, 1);
		var text    = Texts[1];
		var c       = Stopwatch.StartNew();
		var syntax  = QlQuery.Read(text, QlLimits.Default);
		var read    = c.Elapsed.TotalMilliseconds;

		c.Restart();

		var query = QlQuery.Parse(text, Model);
		var bound = c.Elapsed.TotalMilliseconds;

		c.Restart();

		var q          = query.Translate(root, null, Adapter, out _);
		var translated = c.Elapsed.TotalMilliseconds;

		c.Restart();
		Execute(q);

		var executed = c.Elapsed.TotalMilliseconds;

		c.Restart();
		Execute(QlQuery.Parse(Texts[2], Model).Translate(root, null, Adapter, out _));

		var second = c.Elapsed.TotalMilliseconds;

		Console.WriteLine($"provider {db.DataProvider.Name}; connect and fill {Ms(connect)}");
		Console.WriteLine($"| first read | first parse+bind (read again) | first translate + audit | first execution (linq2db cold) | another shape after that |");
		Console.WriteLine($"|---|---|---|---|---|");
		Console.WriteLine($"| {Ms(read)} | {Ms(bound)} | {Ms(translated)} | {Ms(executed)} | {Ms(second)} |");
		GC.KeepAlive(syntax);
	}

	// ── timeout: does the adapter's timeout stop a slow scan? ──

	public static void Timeout(bool mssql)
	{
		using var db = Connect(mssql, 2_000_000);

		var root    = Root(db, 1);
		var adapter = new QlLinqToDbAdapter(mssql ? QlLikeDialect.SqlServer : QlLikeDialect.Standard);
		var text    = "SELECT id FROM orders WHERE customer LIKE '%zz%' OR status LIKE '%q%' ORDER BY total DESC";

		foreach (var ms in new[] { 60_000, 200, 20 })
		{
			var limits = QlLimits.Default with { CommandTimeout = TimeSpan.FromMilliseconds(ms), MaxRows = 10 };
			var query  = QlQuery.Parse(text, Model, limits);
			var c      = Stopwatch.StartNew();

			try
			{
				var rows = Drain(query.ExecuteAsync(new QlRoots<Tenant>().Root(Orders, _ => root), new Tenant(1), null, adapter)).GetAwaiter().GetResult();

				Console.WriteLine($"timeout {ms} ms: finished, {rows} rows in {Ms(c.Elapsed.TotalMilliseconds)}");
			}
			catch (QlException e)
			{
				Console.WriteLine($"timeout {ms} ms: {e.Error.Kind} after {Ms(c.Elapsed.TotalMilliseconds)}: {e.Error.Message}; inner {e.InnerException?.GetType().Name}");
			}
		}
	}
}
