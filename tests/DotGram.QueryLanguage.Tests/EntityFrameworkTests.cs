#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>
/// The same model and texts under EF Core over SQLite in memory: a provider that evaluates the closed parts of a
/// projection ahead of the rows, held to writing the JSON a row at a time all the same. EF Core 10 runs on .NET 10
/// alone, so these run on the net10.0 host only.
/// </summary>
public sealed class EntityFrameworkTests : IDisposable
{
	sealed class Store(DbContextOptions<Store> options) : DbContext(options)
	{
		public DbSet<Customer> Customers => Set<Customer>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Customer>().ToTable("Customers").Property(c => c.Id).ValueGeneratedNever();
		}
	}

	readonly SqliteConnection        _connection;
	readonly DbContextOptions<Store> _options;

	public EntityFrameworkTests()
	{
		_connection = new SqliteConnection("Data Source=:memory:");

		_connection.Open();

		_options = new DbContextOptionsBuilder<Store>().UseSqlite(_connection).Options;

		using var store = new Store(_options);

		store.Database.EnsureCreated();
		store.Customers.AddRange(Customer.Rows());
		store.SaveChanges();
	}

	public void Dispose()
	{
		_connection.Dispose();
	}

	/// <summary>A context of its own for each call, as a host has one a request; the rows are untracked, as a read's are.</summary>
	Crm Open(List<Store> opened)
	{
		var store = new Store(_options);

		opened.Add(store);

		return new Crm(store.Customers.AsNoTracking());
	}

	static QlModel<Crm> Model(QlOptions? options = null)
	{
		var builder = new QlModelBuilder<Crm>();

		builder.Like((s, p) => EF.Functions.Like(s, p, "\\"));

		builder.Entity(c => c.Customers)
			.Column("id",      c => c.Id)
			.Column("name",    c => c.Name)
			.Column("total",   c => c.Total)
			.Column("created", c => c.Created);

		return builder.Build(options);
	}

	async Task<T> With<T>(Func<Crm, Task<T>> body)
	{
		var opened = new List<Store>();

		try
		{
			return await body(Open(opened));
		}
		finally
		{
			foreach (var store in opened)
				await store.DisposeAsync();
		}
	}

	static string Document(string rows, int count, bool truncated = false, string columns = "\"id\",\"name\"")
	{
		return "{\"status\":\"started\",\"columns\":[" + columns + "],\"rows\":[" + rows + "],\"count\":" + count + ",\"truncated\":" + (truncated ? "true" : "false") + ",\"result\":\"completed\"}";
	}

	[Fact]
	public async Task TheDocumentInBothForms()
	{
		var ct = TestContext.Current.CancellationToken;

		Assert.Equal(
			Document("""[1,"Ann \"A\" Lee"],[2,"C:\\temp\\x\n\u003Cb\u003E"],[3,"Пётр \u0026 Co"]""", 3),
			await With(c => Model().ToJsonAsync(c, "from customers order by id select id, name", cancellationToken: ct)));

		Assert.Equal(
			Document("""{"id":1,"name":"Ann \"A\" Lee"},{"id":2,"name":"C:\\temp\\x\n\u003Cb\u003E"},{"id":3,"name":"Пётр \u0026 Co"}""", 3),
			await With(c => Model().ToJsonAsync(c, "from customers order by id select id, name", format: QlJsonFormat.Object, cancellationToken: ct)));

		Assert.Equal(Document("", 0), await With(c => Model().ToJsonAsync(c, "from customers where id > 100 select id, name", cancellationToken: ct)));
	}

	/// <summary>Each row is written from its own values: the writer is not evaluated once ahead of the rows.</summary>
	[Fact]
	public async Task EveryRowIsWrittenFromItsOwnValues()
	{
		var ct = TestContext.Current.CancellationToken;

		Assert.Equal(
			Document("""["Ann \"A\" Lee"],["C:\\temp\\x\n\u003Cb\u003E"],["Пётр \u0026 Co"]""", 3, columns: "\"name\""),
			await With(c => Model().ToJsonAsync(c, "from customers order by id select name", cancellationToken: ct)));

		Assert.Equal(
			Document("[1,2],[2,4],[3,6]", 3, columns: "\"id\",\"twice\""),
			await With(c => Model().ToJsonAsync(c, "from customers order by id let twice = id * 2 select id, twice", cancellationToken: ct)));

		var rows = await With(c => Task.FromResult(Model().QueryJsonRow(c, "from customers order by id select id, created").ToList()));

		Assert.Equal(["[1,\"2026-11-01T09:30:00.0000000\"]", "[2,\"2026-11-02T00:00:00.0000000\"]", "[3,\"2026-11-03T18:05:07.0000000\"]"], rows);
	}

	[Fact]
	public async Task TheRowsAreReadAsynchronously()
	{
		Assert.True(await With(c => Task.FromResult(Model().QueryJsonRow(c, "from customers") is IAsyncEnumerable<string>)));
	}

	[Fact]
	public async Task TheBoundCutsAndSaysSo()
	{
		var ct = TestContext.Current.CancellationToken;

		Assert.Equal(Document("[1],[2]", 2, truncated: true, columns: "\"id\""), await With(c => Model(new QlOptions { MaxRows = 2 }).ToJsonAsync(c, "from customers order by id select id", cancellationToken: ct)));
		Assert.Equal(Document("[1],[2],[3]", 3, columns: "\"id\""), await With(c => Model(new QlOptions { MaxRows = 3 }).ToJsonAsync(c, "from customers order by id select id", cancellationToken: ct)));
		Assert.Equal(Document("[1],[2],[3]", 3, columns: "\"id\""), await With(c => Model(new QlOptions { MaxRows = int.MaxValue }).ToJsonAsync(c, "from customers order by id select id", cancellationToken: ct)));
	}

	[Fact]
	public async Task ParametersListsLikeAndAggregates()
	{
		var ct = TestContext.Current.CancellationToken;

		var values = new Dictionary<string, object?> { ["ids"] = new[] { 1, 3 }, ["low"] = 1 };

		Assert.Equal(Document("[1],[3]", 2, columns: "\"id\""), await With(c => Model().ToJsonAsync(c, "from customers where id in @ids order by id select id", values, cancellationToken: ct)));
		Assert.Equal(Document("[1]", 1, columns: "\"id\""), await With(c => Model().ToJsonAsync(c, "from customers where name like 'Ann%' && id >= @low select id", values, cancellationToken: ct)));
		Assert.Equal(Document("[2]", 1, columns: "\"id\""), await With(c => Model().ToJsonAsync(c, "from customers where name like 'C:\\\\\\\\%' select id", cancellationToken: ct)));
		Assert.Equal(Document("[3,3]", 1, columns: "\"c\",\"m\""), await With(c => Model().ToJsonAsync(c, "from customers select Count(*) as c, Max(id) as m", cancellationToken: ct)));
	}

	[Fact]
	public async Task TheDataAndACountOverTheRows()
	{
		var data = await With(c => Task.FromResult(Model().QueryData(c, "from customers order by id select id, name").ToList()));

		Assert.Equal([1, 2, 3], data.Select(r => (int)r[0]!));

		Assert.Equal(3, await With(c => Task.FromResult(Model().QueryJsonRow(c, "from customers").Count())));
	}
}
#endif
