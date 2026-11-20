using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.Interceptors;
using LinqToDB.Mapping;

using Microsoft.Data.Sqlite;

using Nullable = LinqToDB.Mapping.NullableAttribute;

namespace DotGram.QueryLanguage.Probes;

public enum Kind
{
	[MapValue("new")]     New,
	[MapValue("paid")]    Paid,
	[MapValue("shipped")] Shipped,
}

[Table("Orders")]
public sealed class Order
{
	[PrimaryKey]       public int             Id        { get; set; }
	[Column, NotNull]  public int             TenantId  { get; set; }
	[Column, Nullable] public string?         Status    { get; set; }
	[Column, NotNull]  public string          Customer  { get; set; } = "";
	[Column, NotNull]  public bool            Paid      { get; set; }
	[Column, Nullable] public bool?           Flag      { get; set; }
	[Column, NotNull]  public int             Qty       { get; set; }
	[Column, Nullable] public int?            Priority  { get; set; }
	[Column, NotNull]  public long            Big       { get; set; }
	[Column, Nullable] public long?           BigN      { get; set; }
	[Column(DataType = DataType.Decimal, Precision = 18, Scale = 2), NotNull]  public decimal  Total    { get; set; }
	[Column(DataType = DataType.Decimal, Precision = 18, Scale = 2), Nullable] public decimal? Discount { get; set; }
	[Column, NotNull]  public double          Score     { get; set; }
	[Column, Nullable] public double?         Rating    { get; set; }
	[Column, NotNull]  public DateTime        Created   { get; set; }
	[Column, Nullable] public DateTime?       Shipped   { get; set; }
	[Column, NotNull]  public DateTimeOffset  Stamp     { get; set; }
	[Column, Nullable] public DateTimeOffset? StampN    { get; set; }
	[Column, NotNull]  public Guid            Uid       { get; set; }
	[Column, Nullable] public Guid?           UidN      { get; set; }
	[Column, NotNull]  public DateOnly        Day       { get; set; }
	[Column, NotNull]  public Kind            Kind      { get; set; }
}

/// <summary>The host's context: a linq2db connection that knows its tenant.</summary>
public sealed class Db : DataConnection
{
	public Db(DataOptions options, int tenant) : base(options)
	{
		TenantId  = tenant;
		TenantBox = new StrongBox<int>(tenant);
	}

	public int TenantId { get; }

	public StrongBox<int> TenantBox { get; }

	public ITable<Order> Orders => this.GetTable<Order>();
}

/// <summary>A host context that is not the connection: the connection and the tenant side by side.</summary>
public sealed class AppContext
{
	public AppContext(Db db, int tenant)
	{
		Db       = db;
		TenantId = tenant;
	}

	public Db Db { get; }

	public int TenantId { get; }
}

/// <summary>Records which reader call linq2db made for each command.</summary>
public sealed class ReaderRecorder : CommandInterceptor
{
	public readonly List<string> Calls = [];

	public override Option<DbDataReader> ExecuteReader(CommandEventData eventData, DbCommand command, CommandBehavior commandBehavior, Option<DbDataReader> result)
	{
		lock (Calls)
		{
			Calls.Add("ExecuteReader");
		}

		return result;
	}

	public override Task<Option<DbDataReader>> ExecuteReaderAsync(CommandEventData eventData, DbCommand command, CommandBehavior commandBehavior, Option<DbDataReader> result, CancellationToken cancellationToken)
	{
		lock (Calls)
		{
			Calls.Add("ExecuteReaderAsync");
		}

		return Task.FromResult(result);
	}
}

public static class Provider
{
	public const string SqlServerDatabase = "QlPchain";

	static SqliteConnection? keeper;

	public static string Name { get; private set; } = "sqlite";

	public static bool IsSqlServer => Name == "mssql";

	static string SqlServerConnection()
	{
		return SqlServerMaster() + ";Initial Catalog=" + SqlServerDatabase;
	}

	static string SqlServerMaster()
	{
		return Environment.GetEnvironmentVariable("DOTGRAM_SQLSERVER")?.Trim().TrimEnd(';')
			?? throw new InvalidOperationException("DOTGRAM_SQLSERVER is not set: the SQL Server connection string, without a database.");
	}

	const string SqliteConnection = "Data Source=QlPchain;Mode=Memory;Cache=Shared";

	public static DataOptions Options(params IInterceptor[] interceptors)
	{
		var options = IsSqlServer
			? new DataOptions().UseSqlServer(SqlServerConnection())
			: new DataOptions().UseSQLite(SqliteConnection);

		if (interceptors.Length > 0)
		{
			options = options.UseInterceptors(interceptors);
		}

		return options;
	}

	public static Db Connect(int tenant = 1, params IInterceptor[] interceptors)
	{
		return new Db(Options(interceptors), tenant);
	}

	/// <summary>Creates the table with <paramref name="rows"/> rows: ids 1..n, tenants 1 and 2 alternating.</summary>
	public static void Setup(string provider, int rows)
	{
		Name = provider;

		if (IsSqlServer)
		{
			using var master = new DataConnection(new DataOptions().UseSqlServer(SqlServerMaster()));

			master.Execute($"IF DB_ID('{SqlServerDatabase}') IS NULL CREATE DATABASE [{SqlServerDatabase}]");
		}
		else
		{
			keeper ??= new SqliteConnection(SqliteConnection);
			keeper.Open();
		}

		using var db = Connect();

		db.DropTable<Order>(throwExceptionIfNotExists: false);
		db.CreateTable<Order>();
		db.BulkCopy(Generate(rows));
	}

	public static void DropSqlServer()
	{
		using var master = new DataConnection(new DataOptions().UseSqlServer(SqlServerMaster()));

		master.Execute($"IF DB_ID('{SqlServerDatabase}') IS NOT NULL BEGIN ALTER DATABASE [{SqlServerDatabase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{SqlServerDatabase}]; END");
	}

	public static IEnumerable<Order> Generate(int count)
	{
		string?[] statuses = ["paid", "new", null, "shipped"];

		for (var i = 1; i <= count; i++)
		{
			yield return new Order
			{
				Id       = i,
				TenantId = i % 2 == 1 ? 1 : 2,
				Status   = statuses[i % statuses.Length],
				Customer = "c" + (i % 7) + (i % 5 == 0 ? " \"q\" <b>&" : ""),
				Paid     = i % 2 == 0,
				Flag     = i % 3 == 0 ? null : i % 3 == 1,
				Qty      = i % 10,
				Priority = i % 3 == 0 ? null : i % 3,
				Big      = i * 1_000_000_007L,
				BigN     = i % 4 == 0 ? null : i * 3L,
				Total    = i * 12.5m,
				Discount = i % 4 == 0 ? null : i * 0.25m,
				Score    = i / 4.0,
				Rating   = i % 5 == 0 ? null : i / 3.0,
				Created  = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMinutes(i * 37),
				Shipped  = i % 3 == 0 ? null : new DateTime(2026, 2, 1).AddHours(i),
				Stamp    = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(3)).AddMinutes(i),
				StampN   = i % 2 == 0 ? null : new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero).AddMinutes(i),
				Uid      = new Guid(i, 0, 0, [0, 0, 0, 0, 0, 0, 0, 1]),
				UidN     = i % 2 == 0 ? null : new Guid(i, 1, 1, [1, 1, 1, 1, 1, 1, 1, 1]),
				Day      = new DateOnly(2026, 1, 1).AddDays(i % 30),
				Kind     = (Kind)(i % 3),
			};
		}
	}
}
