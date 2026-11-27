using System;
using System.Linq;

using LinqToDB;
using LinqToDB.Mapping;

namespace DotGram.QueryLanguage.Tests;

/// <summary>A customer of the JSON examples: three rows with a quote, a backslash, a line break, markup and a non-ASCII name.</summary>
[Table("Customers")]
public sealed class Customer
{
	[PrimaryKey]                                                         public int      Id      { get; set; }
	[Column, NotNull]                                                    public string   Name    { get; set; } = "";
	[Column(DataType = DataType.Decimal, Precision = 10, Scale = 2), Nullable] public decimal? Total   { get; set; }
	[Column, NotNull]                                                    public DateTime Created { get; set; }

	public static Customer[] Rows()
	{
		return
		[
			new Customer { Id = 1, Name = "Ann \"A\" Lee",            Total = 1250.50m, Created = new DateTime(2026, 11, 1, 9, 30, 0) },
			new Customer { Id = 2, Name = "C:\\temp\\x\n<b>",         Total = null,     Created = new DateTime(2026, 11, 2) },
			new Customer { Id = 3, Name = "Пётр & Co",                Total = 0.10m,    Created = new DateTime(2026, 11, 3, 18, 5, 7) },
		];
	}
}

/// <summary>The context of the customer examples.</summary>
public sealed class Crm(IQueryable<Customer> customers)
{
	public IQueryable<Customer> Customers { get; } = customers;

	public static Crm Of(IDataContext db)
	{
		return new Crm(db.GetTable<Customer>());
	}

	public static QlModel<Crm> Model(QlOptions? options = null)
	{
		var builder = new QlModelBuilder<Crm>();

		builder.Entity(c => c.Customers)
			.Column("id",      c => c.Id)
			.Column("name",    c => c.Name)
			.Column("total",   c => c.Total)
			.Column("created", c => c.Created);

		return builder.Build(options);
	}
}
