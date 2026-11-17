using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DotGram.QueryLanguage.LinqToDB;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace DotGram.QueryLanguage.Tests;

[Table("Orders")]
public sealed class Order
{
	[PrimaryKey]        public int      Id       { get; set; }
	[Column, NotNull]   public int      TenantId { get; set; }
	[Column, Nullable]  public string?  Status   { get; set; }
	[Column, NotNull]   public decimal  Total    { get; set; }
	[Column, NotNull]   public long     Big      { get; set; }
	[Column, NotNull]   public double   Score    { get; set; }
	[Column, NotNull]   public DateTime Created  { get; set; }
	[Column, Nullable]  public int?     Priority { get; set; }
	[Column, NotNull]   public string   Customer { get; set; } = "";
	[Column, NotNull]   public string   Secret   { get; set; } = "";
	[Column, NotNull]   public string   Limit    { get; set; } = "";
}

public sealed record Tenant(int Id);

/// <summary>
/// Orders of two tenants in a database; the model exposes some of their fields, and the root
/// shows a request only its own tenant's.
/// </summary>
public static class Shop
{
	public static readonly QlEntity<Order> Orders;
	public static readonly QlBuiltModel    Model;

	static Shop()
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
			.Column("customer", o => o.Customer, QlUse.Select | QlUse.Filter)
			.Column("limit",    o => o.Limit,    QlUse.Select);

		Model = model.Build();
	}

	public static QlRoots<Tenant> Roots(IDataContext db)
	{
		return new QlRoots<Tenant>().Root(Orders, tenant => db.GetTable<Order>().Where(o => o.TenantId == tenant.Id));
	}

	public static IEnumerable<Order> Rows()
	{
		string?[] statuses = ["paid", "new", null, "a[b%c", "a_b", "100%", "x!y", "paid", "shipped", "new"];

		for (var i = 1; i <= 20; i++)
		{
			yield return new Order
			{
				Id       = i,
				TenantId = i <= 10 ? 1 : 2,
				Status   = statuses[(i - 1) % statuses.Length],
				Total    = i * 12.5m,
				Big      = i * 1_000_000_000L,
				Score    = i / 4.0,
				Created  = new DateTime(2026, 1, 1).AddDays(i),
				Priority = i % 3 == 0 ? null : i % 3,
				Customer = "c" + (i % 4),
				Secret   = "s" + i,
				Limit    = "l" + i,
			};
		}
	}

	public static DataConnection Sqlite()
	{
		var db = new DataConnection(new DataOptions().UseSQLite("Data Source=:memory:"));

		db.CreateTable<Order>();
		db.BulkCopy(Rows());

		return db;
	}

	/// <summary>SQL Server when QL_MSSQL names a connection; null otherwise.</summary>
	public static DataConnection? SqlServer()
	{
		var connection = Environment.GetEnvironmentVariable("QL_MSSQL");

		if (string.IsNullOrEmpty(connection))
			return null;

		using (var master = new DataConnection(new DataOptions().UseSqlServer(connection)))
			master.Execute("IF DB_ID('QlSpike') IS NULL CREATE DATABASE QlSpike");

		var db = new DataConnection(new DataOptions().UseSqlServer(connection + ";Initial Catalog=QlSpike"));

		db.DropTable<Order>(throwExceptionIfNotExists: false);
		db.CreateTable<Order>();
		db.BulkCopy(new BulkCopyOptions { BulkCopyType = BulkCopyType.MultipleRows }, Rows());

		return db;
	}

	public static async Task<List<object?[]>> Run(IDataContext db, string text, Dictionary<string, object?>? parameters = null, QlLikeDialect dialect = QlLikeDialect.Standard, QlLimits? limits = null, int tenant = 1)
	{
		var query  = QlQuery.Parse(text, Model, limits);
		var rows   = new List<object?[]>();

		await foreach (var row in query.ExecuteAsync(Roots(db), new Tenant(tenant), parameters, new QlLinqToDbAdapter(dialect)))
			rows.Add(row);

		return rows;
	}

	public static async Task<int[]> Ids(IDataContext db, string condition, Dictionary<string, object?>? parameters = null, QlLikeDialect dialect = QlLikeDialect.Standard)
	{
		var rows = await Run(db, "SELECT id FROM orders WHERE " + condition + " ORDER BY id", parameters, dialect);

		return rows.Select(r => (int)r[0]!).ToArray();
	}
}
