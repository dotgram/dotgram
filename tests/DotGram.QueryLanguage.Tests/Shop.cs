using System;
using System.Linq;

using LinqToDB;
using LinqToDB.Mapping;

namespace DotGram.QueryLanguage.Tests;

public enum Kind
{
	New,
	Paid,
	Shipped,
}

/// <summary>An order of one of two tenants: a field of every type the language has, and a few it does not.</summary>
[Table("Orders")]
public sealed class Order
{
	[PrimaryKey]                public int             Id       { get; set; }
	[Column, NotNull]           public int             TenantId { get; set; }
	[Column, Nullable]          public string?         Status   { get; set; }
	[Column, NotNull]           public decimal         Total    { get; set; }
	[Column, NotNull]           public int             Qty      { get; set; }
	[Column, NotNull]           public long            Big      { get; set; }
	[Column, NotNull]           public double          Score    { get; set; }
	[Column, NotNull]           public DateTime        Created  { get; set; }
	[Column, Nullable]          public DateTime?       Shipped  { get; set; }
	[Column, NotNull]           public DateTimeOffset  Stamp    { get; set; }
	[Column, Nullable]          public int?            Priority { get; set; }
	[Column, NotNull]           public string          Customer { get; set; } = "";
	[Column, NotNull]           public decimal         Salary   { get; set; }
	[Column, NotNull]           public int             Rank     { get; set; }
	[Column, NotNull]           public string          Note     { get; set; } = "";
	[Column, NotNull]           public Guid            Ref      { get; set; }
	[Column, NotNull]           public bool            Active   { get; set; }
	[Column, Nullable]          public bool?           Flag     { get; set; }
	[Column, NotNull]           public DateOnly        Day      { get; set; }
	[Column, NotNull]           public Kind            Kind     { get; set; }
	[Column, Nullable]          public byte[]?         Blob     { get; set; }
}

/// <summary>What every query is handed: where the rows are, and whose they are.</summary>
public sealed class Shop(IQueryable<Order> orders, int tenant)
{
	public IQueryable<Order> AllOrders { get; } = orders;
	public int               Tenant    { get; } = tenant;

	public static Shop Of(IDataContext db, int tenant)
	{
		return new Shop(db.GetTable<Order>(), tenant);
	}

	public static Shop InMemory(int tenant = 1)
	{
		return new Shop(Rows().AsQueryable(), tenant);
	}

	/// <summary>The model the tests use: each right missing somewhere, and fields outside the language's types.</summary>
	public static QlModel<Shop> Model(QlOptions? options = null, bool salary = true)
	{
		var builder = new QlModelBuilder<Shop>();
		var orders  = builder.Entity("orders", s => s.AllOrders.Where(o => o.TenantId == s.Tenant));

		orders
			.Column("id",       o => o.Id)
			.Column("status",   o => o.Status, QlUse.Select | QlUse.Filter)
			.Column("total",    o => o.Total)
			.Column("qty",      o => o.Qty)
			.Column("big",      o => o.Big)
			.Column("score",    o => o.Score)
			.Column("created",  o => o.Created)
			.Column("shipped",  o => o.Shipped)
			.Column("stamp",    o => o.Stamp)
			.Column("priority", o => o.Priority)
			.Column("customer", o => o.Customer, QlUse.Select | QlUse.Filter)
			.Column("rank",     o => o.Rank, QlUse.Sort)
			.Column("note",     o => o.Note, QlUse.Select)
			.Column("ref",      o => o.Ref)
			.Column("active",   o => o.Active)
			.Column("flag",     o => o.Flag)
			.Column("day",      o => o.Day)
			.Column("kind",     o => o.Kind)
			.Column("blob",     o => o.Blob, QlUse.Select);

		if (salary)
			orders.Column("salary", o => o.Salary, QlUse.Filter);

		return builder.Build(options);
	}

	/// <summary>Forty orders, twenty a tenant; a null in every nullable field somewhere.</summary>
	public static Order[] Rows()
	{
		string?[] statuses = ["paid", "new", null, "a[b%c", "a_b", "100%", "x'y", "paid", "shipped", "new"];

		return Enumerable.Range(1, 40).Select(i => new Order
		{
			Id       = i,
			TenantId = i <= 20 ? 1 : 2,
			Status   = statuses[(i - 1) % statuses.Length],
			Total    = i * 12.5m,
			Qty      = i % 7,
			Big      = i * 1_000_000_000L,
			Score    = i / 4.0,
			Created  = new DateTime(2026, 1, 1, 10, 0, 0).AddDays(i),
			Shipped  = i % 4 == 0 ? null : new DateTime(2026, 2, 1).AddDays(i),
			Stamp    = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero).AddHours(i),
			Priority = i % 3 == 0 ? null : i % 3,
			Customer = "c" + (i % 4),
			Salary   = 1000 + i,
			Rank     = 100 - i,
			Note     = "n" + i,
			Ref      = new Guid(i, 0, 0, new byte[8]),
			Active   = i % 2 == 0,
			Flag     = i % 5 == 0 ? null : i % 2 == 1,
			Day      = new DateOnly(2026, 1, 1).AddDays(i % 10),
			Kind     = (Kind)(i % 3),
			Blob     = i % 6 == 0 ? null : [(byte)i],
		}).ToArray();
	}
}
