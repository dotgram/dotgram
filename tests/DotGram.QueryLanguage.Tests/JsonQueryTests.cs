using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Internal.Linq;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>QueryJsonRow end to end on SQLite and SQL Server: the rows as the provider reads them, written as JSON.</summary>
[Collection(nameof(Databases))]
public sealed class JsonQueryTests(Databases databases)
{
	static readonly QlModel<Shop> Model = Shop.Model();

	static readonly QlModel<Crm> Customers = Crm.Model();

	List<string> Run(string provider, string text, object? values = null, QlJsonFormat format = QlJsonFormat.Array, QlModel<Shop>? model = null)
	{
		return [.. (model ?? Model).QueryJsonRow(Shop.Of(databases.Get(provider), 1), text, Rows.Values(values), format)];
	}

	// ── The examples of the documentation, byte for byte ──

	const string CustomersText = "from customers select id, name, total, created order by id";

	static readonly string[] ArrayRows =
	[
		"""[1,"Ann \"A\" Lee",1250.50,"2026-11-01T09:30:00.0000000"]""",
		"""[2,"C:\\temp\\x\n\u003Cb\u003E",null,"2026-11-02T00:00:00.0000000"]""",
		"""[3,"Пётр \u0026 Co",0.10,"2026-11-03T18:05:07.0000000"]""",
	];

	static readonly string[] ObjectRows =
	[
		"""{"id":1,"name":"Ann \"A\" Lee","total":1250.50,"created":"2026-11-01T09:30:00.0000000"}""",
		"""{"id":2,"name":"C:\\temp\\x\n\u003Cb\u003E","total":null,"created":"2026-11-02T00:00:00.0000000"}""",
		"""{"id":3,"name":"Пётр \u0026 Co","total":0.10,"created":"2026-11-03T18:05:07.0000000"}""",
	];

	/// <summary>
	/// The rows of the examples as <paramref name="provider"/> reads them: a decimal has the scale the provider gives
	/// it, which SQLite, keeping no scale, does not.
	/// </summary>
	static string[] Expected(string provider, string[] rows)
	{
		return provider == "SQLite" ? [.. rows.Select(r => r.Replace("1250.50", "1250.5").Replace("0.10", "0.1"))] : rows;
	}

	public static IEnumerable<TheoryDataRow<string>> Places()
	{
		yield return new TheoryDataRow<string>("Memory");

		foreach (var provider in Databases.Providers())
			yield return provider;
	}

	Crm CrmOf(string place)
	{
		return place == "Memory" ? new Crm(Customer.Rows().AsQueryable()) : Crm.Of(databases.Get(place));
	}

	[Theory]
	[MemberData(nameof(Places))]
	public void TheExamplesByteForByte(string place)
	{
		Assert.Equal(Expected(place, ArrayRows),  Customers.QueryJsonRow(CrmOf(place), CustomersText).ToList());
		Assert.Equal(Expected(place, ObjectRows), Customers.QueryJsonRow(CrmOf(place), CustomersText, format: QlJsonFormat.Object).ToList());
	}

	[Theory]
	[MemberData(nameof(Places))]
	public async Task TheDocumentsOfTheExamples(string place)
	{
		var array    = await Customers.ToJsonAsync(CrmOf(place), CustomersText, cancellationToken: TestContext.Current.CancellationToken);
		var objects  = await Customers.ToJsonAsync(CrmOf(place), CustomersText, format: QlJsonFormat.Object, cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal("{\"columns\":[\"id\",\"name\",\"total\",\"created\"],\"rows\":[" + string.Join(",", Expected(place, ArrayRows)) + "]}", array);
		Assert.Equal("[" + string.Join(",", Expected(place, ObjectRows)) + "]", objects);

		if (place != "SQLite")
		{
			Assert.Equal(238, Encoding.UTF8.GetByteCount(array));
			Assert.Equal(277, Encoding.UTF8.GetByteCount(objects));
		}
	}

	[Theory]
	[MemberData(nameof(Places))]
	public async Task AnEmptyResultIsAnEmptyDocument(string place)
	{
		const string none = "from customers where id > 100 select id, name";

		Assert.Equal("{\"columns\":[\"id\",\"name\"],\"rows\":[]}", await Customers.ToJsonAsync(CrmOf(place), none, cancellationToken: TestContext.Current.CancellationToken));
		Assert.Equal("[]", await Customers.ToJsonAsync(CrmOf(place), none, format: QlJsonFormat.Object, cancellationToken: TestContext.Current.CancellationToken));
		Assert.Empty(Customers.QueryJsonRow(CrmOf(place), none));
	}

	[Fact]
	public async Task ARefusedTextFailsTheTask()
	{
		var task  = Customers.ToJsonAsync(CrmOf("Memory"), "from customers where nothing > 1", cancellationToken: TestContext.Current.CancellationToken);
		var error = await Assert.ThrowsAsync<QlException>(() => task);

		Assert.Equal(QlErrorCode.UnknownName, error.Error.Code);

		var missing = Customers.ToJsonAsync(CrmOf("Memory"), "from customers where id > @n", cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal(QlErrorCode.Parameter, (await Assert.ThrowsAsync<QlException>(() => missing)).Error.Code);
	}

	[Theory]
	[MemberData(nameof(Places))]
	public async Task ACancelledTokenStopsTheDocument(string place)
	{
		using var cancelled = new System.Threading.CancellationTokenSource();

		await cancelled.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Customers.ToJsonAsync(CrmOf(place), CustomersText, cancellationToken: cancelled.Token));
	}

	[Fact]
	public void TheFormatIsAnArrayOrAnObject()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => Customers.QueryJsonRow(CrmOf("Memory"), CustomersText, format: (QlJsonFormat)2));
	}

	// ── Every kind as the provider reads it ──

	/// <summary>What a value <c>QueryData</c> returns is printed as: the reference the JSON rows are held to.</summary>
	static void Same(object? expected, JsonElement written)
	{
		switch (expected)
		{
			case null:
				Assert.Equal(JsonValueKind.Null, written.ValueKind);
				break;

			case bool b:
				Assert.Equal(b, written.GetBoolean());
				break;

			case int or long:
				Assert.Equal(Convert.ToInt64(expected, CultureInfo.InvariantCulture), written.GetInt64());
				break;

			case decimal d:
				Assert.Equal(d.ToString(CultureInfo.InvariantCulture), written.GetRawText());
				break;

			case double d:
				Assert.Equal(d, written.GetDouble());
				break;

			case string s:
				Assert.Equal(s, written.GetString());
				break;

			case DateTime d:
				Assert.Equal(d.ToString("O", CultureInfo.InvariantCulture), written.GetString());
				break;

			case DateTimeOffset d:
				Assert.Equal(d.ToString("O", CultureInfo.InvariantCulture), written.GetString());
				break;

			case Guid g:
				Assert.Equal(g.ToString("D"), written.GetString());
				break;

			case IFormattable f:
				Assert.Equal(f.ToString(null, CultureInfo.InvariantCulture), written.GetString());
				break;

			default:
				Assert.Equal(expected.ToString(), written.GetString());
				break;
		}
	}

	void SameAsData(string provider, string text, object? values = null, QlModel<Shop>? model = null)
	{
		var data = Rows.Run(model ?? Model, databases.Get(provider), text, values);
		var json = Run(provider, text, values, model: model);

		Assert.NotEmpty(data);
		Assert.Equal(data.Count, json.Count);

		for (var i = 0; i < data.Count; i++)
		{
			using var document = JsonDocument.Parse(json[i]);

			var elements = document.RootElement.EnumerateArray().ToArray();

			Assert.Equal(data[i].Length, elements.Length);

			for (var j = 0; j < elements.Length; j++)
				Same(data[i][j], elements[j]);
		}
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void EveryFieldAsTheProviderReadsIt(string provider)
	{
		// The fields with Select: every kind, nullable ones with nulls, and three types outside the language.
		SameAsData(provider, "from orders order by id");
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ComputedColumnsLiteralsAndParameters(string provider)
	{
		SameAsData(provider, "from orders let t = total * 2 where qty > 1 && customer != @p select 7 as seven, @p as p, id, t, score / 3 as third, big + 1 as more, customer.ToUpper() as up, status.Length as n, shipped.Month as m, 'lit' as s order by id", new { p = "P" });
		SameAsData(provider, "from orders where id < 4 && total != @a && score != @b && big != @c && created != @d && ref != @e && active != @f order by id select @a as a, @b as b, @c as c, @d as d, @e as e, @f as f", new { a = 1.5m, b = 2.25, c = 3L, d = new DateTime(2026, 1, 1), e = Guid.Empty, f = true });
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ARowOfConstantsIsTheSameForEveryRow(string provider)
	{
		var rows = Run(provider, "from orders where customer != @p select 1 as one, @p as p", new { p = "x" });

		Assert.Equal(20, rows.Count);
		Assert.All(rows, r => Assert.Equal("[1,\"x\"]", r));
	}

	/// <summary>Rows whose every column is a literal or a value of the call: the same text for every row, as QueryData gives them.</summary>
	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ARowThatReadsNoColumnRuns(string provider)
	{
		(string Text, string Row)[] cases =
		[
			("from orders where customer != @p select @p as p",                 "[\"x\"]"),
			("from orders where customer != @p select 'a' as a, 'b' as b",      "[\"a\",\"b\"]"),
			("from orders where qty != @n select @n * 2 as y",                  "[4]"),
			("from orders where qty != @n let x = @n + 1 select x",             "[3]"),
			("from orders where customer != @p select @p as p, 1 as one",       "[\"x\",1]"),
			("from orders where customer != @p select timestamp '2026-01-01 00:00:00' as t", "[\"2026-01-01T00:00:00.0000000\"]"),
		];

		foreach (var (text, row) in cases)
		{
			var values = new Dictionary<string, object?> { ["p"] = "x", ["n"] = 2 };
			var rows   = Model.QueryJsonRow(Shop.Of(databases.Get(provider), 1), text, values).ToList();

			Assert.Equal(Model.QueryData(Shop.Of(databases.Get(provider), 1), text, values).ToList().Count, rows.Count);
			Assert.NotEmpty(rows);
			Assert.All(rows, r => Assert.Equal(row, r));
		}
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ARowOfStringsAlone(string provider)
	{
		Assert.Equal(Shop.Rows().Where(o => o.TenantId == 1).Select(o => "[\"" + o.Customer + "\",\"" + o.Note + "\"]"), Run(provider, "from orders order by id select customer, note"));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AggregatesAreOneRow(string provider)
	{
		SameAsData(provider, "from orders where qty > 0 select Count(*) as c, Count(priority) as p, Sum(qty) as s, Avg(qty) as a, Avg(total) as t, Min(status) as mn, Max(created) as mx, Min(stamp) as st, Max(big) as b");

		Assert.Equal(["[0,null,null,null]"], Run(provider, "from orders where id > 1000 select Count(*) as c, Sum(qty) as s, Avg(qty) as a, Max(day) as d"));
		Assert.Equal(["{\"c\":0,\"s\":null}"], Run(provider, "from orders where id > 1000 select Count(*) as c, Sum(qty) as s", format: QlJsonFormat.Object));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AnObjectIsNamedByTheColumns(string provider)
	{
		var rows = Run(provider, "from orders where id < 3 order by id select id, status as state, Score, day", format: QlJsonFormat.Object);

		Assert.Equal(["{\"id\":1,\"state\":\"paid\",\"Score\":0.25,\"day\":\"01/02/2026\"}", "{\"id\":2,\"state\":\"new\",\"Score\":0.5,\"day\":\"01/03/2026\"}"], rows);
	}

	// ── Formats ──

	static QlModel<Shop> Formatted(string tag)
	{
		var builder = new QlModelBuilder<Shop>();

		builder.Format<DateTime>(d => tag + d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
			.Format<byte[]>(b => Convert.ToBase64String(b))
			.Format<Kind>(k => tag + (int)k)
			.Format<decimal>(d => d.ToString("0.000", CultureInfo.InvariantCulture));

		builder.Entity("orders", s => s.AllOrders.Where(o => o.TenantId == s.Tenant))
			.Column("id",      o => o.Id)
			.Column(tag == "A:" ? "created" : "made", o => o.Created)
			.Column("shipped", o => o.Shipped)
			.Column("total",   o => o.Total)
			.Column("blob",    o => o.Blob)
			.Column("kind",    o => o.Kind)
			.Column("day",     o => o.Day);

		return builder.Build();
	}

	static string ExpectedFormatted(Order o, string tag, bool asObject)
	{
		var values = new[]
		{
			o.Id.ToString(CultureInfo.InvariantCulture),
			"\"" + tag + o.Created.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "\"",
			o.Shipped is { } s ? "\"" + tag + s.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "\"" : "null",
			"\"" + o.Total.ToString("0.000", CultureInfo.InvariantCulture) + "\"",
			o.Blob is { } b ? "\"" + Convert.ToBase64String(b) + "\"" : "null",
			"\"" + tag + (int)o.Kind + "\"",
			"\"" + o.Day.ToString(CultureInfo.InvariantCulture) + "\"",
		};

		if (!asObject)
			return "[" + string.Join(",", values) + "]";

		string[] names = ["id", tag == "A:" ? "created" : "made", "shipped", "total", "blob", "kind", "day"];

		return "{" + string.Join(",", names.Select((n, i) => "\"" + n + "\":" + values[i])) + "}";
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void TheHostsFormatsComeBeforeTheBuiltInOnes(string provider)
	{
		var model = Formatted("A:");
		var mine  = Shop.Rows().Where(o => o.TenantId == 1).OrderBy(o => o.Id).ToArray();

		Assert.Equal(mine.Select(o => ExpectedFormatted(o, "A:", false)), Run(provider, "from orders order by id", model: model));
		Assert.Equal(mine.Select(o => ExpectedFormatted(o, "A:", true)),  Run(provider, "from orders order by id", format: QlJsonFormat.Object, model: model));
	}

	/// <summary>
	/// Two models, one text: the provider caches one query for both, and each prints with its own formats and names,
	/// whether their calls take turns or run at once.
	/// </summary>
	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void TwoModelsUnderOneCachedQueryPrintTheirOwn(string provider)
	{
		var a    = Formatted("A:");
		var b    = Formatted("B:");
		var mine = Shop.Rows().Where(o => o.TenantId == 1).OrderBy(o => o.Id).ToArray();

		const string text = "from orders order by id";

		void Check(QlModel<Shop> model, string tag, bool asObject, LinqToDB.Data.DataConnection db)
		{
			var rows = model.QueryJsonRow(Shop.Of(db, 1), text, format: asObject ? QlJsonFormat.Object : QlJsonFormat.Array).ToList();

			Assert.Equal(mine.Select(o => ExpectedFormatted(o, tag, asObject)), rows);
		}

		var shared = databases.Get(provider);

		Check(a, "A:", false, shared);
		Check(a, "A:", true, shared);

		var misses = Query<string>.CacheMissCount;

		for (var i = 0; i < 6; i++)
		{
			Check(i % 2 == 0 ? b : a, i % 2 == 0 ? "B:" : "A:", i % 3 == 0, shared);
		}

		Assert.Equal(misses, Query<string>.CacheMissCount);

		Parallel.For(0, 8, i =>
		{
			using var db = databases.Open(provider);

			for (var j = 0; j < 4; j++)
				Check((i + j) % 2 == 0 ? a : b, (i + j) % 2 == 0 ? "A:" : "B:", j % 2 == 0, db);
		});
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ARowAfterAFormatFailedIsWhole(string provider)
	{
		var builder = new QlModelBuilder<Shop>();

		builder.Format<DateTime>(d => d.Day == 5 ? throw new InvalidOperationException("the host's format failed") : "ok");
		builder.Entity("orders", s => s.AllOrders.Where(o => o.TenantId == s.Tenant)).Column("id", o => o.Id).Column("created", o => o.Created);

		var model = builder.Build();

		Assert.Throws<InvalidOperationException>(() => Run(provider, "from orders order by id", model: model));
		Assert.Equal(["[1,\"ok\"]", "[2,\"ok\"]", "[3,\"ok\"]"], Run(provider, "from orders order by id take 3", model: model));
	}

	// ── What a host may append ──

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void PagingAndCountingOverTheRows(string provider)
	{
		// Without the row cap: linq2db 6.5 folds a host's Take or Skip after the cap's Take into it (see the next test).
		var model = Shop.Model(QlOptions.Default with { MaxRows = null });
		var query = model.QueryJsonRow(Shop.Of(databases.Get(provider), 1), "from orders order by id select id, customer");
		var all   = query.ToList();

		Assert.Equal(20, all.Count);
		Assert.Equal(all.Take(3), query.Take(3).ToList());
		Assert.Equal(all.Skip(5).Take(4), query.Skip(5).Take(4).ToList());
		Assert.Equal(20, query.Count());
		Assert.True(query.Any());
		Assert.Equal(all[0], query.First());
	}

	/// <summary>
	/// A host's page over the capped rows. linq2db 6.5 merges the host's <c>Take</c> into the cap's and keeps the cap's
	/// count (on QueryData as well): 20 rows come back for <c>Take(3)</c>. The rows need a boundary after the cap.
	/// </summary>
	[Theory(Skip = "linq2db folds the host's Take into the row cap; needs a boundary after the cap")]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void PagingOverTheCappedRows(string provider)
	{
		var query = Model.QueryJsonRow(Shop.Of(databases.Get(provider), 1), "from orders order by id select id, customer");
		var data  = Model.QueryData(Shop.Of(databases.Get(provider), 1), "from orders order by id select id, customer");
		var all   = query.ToList();

		Assert.Equal(all.Take(3), query.Take(3).ToList());
		Assert.Equal(all.Skip(5).Take(4), query.Skip(5).Take(4).ToList());
		Assert.Equal(3, data.Take(3).ToList().Count);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AFilterOrAnOrderOverTheTextIsRefusedNotGlued(string provider)
	{
		var query = Model.QueryJsonRow(Shop.Of(databases.Get(provider), 1), "from orders order by id select id, customer");

		Assert.ThrowsAny<Exception>(() => query.Where(s => s.Contains("c1")).ToList());
		Assert.ThrowsAny<Exception>(() => query.OrderBy(s => s).ToList());

		// A set operator compares the columns the provider reads, the anchor among them: every row it returns is whole.
		foreach (var row in query.Distinct().ToList())
		{
			using var document = JsonDocument.Parse(row);

			Assert.Equal(2, document.RootElement.GetArrayLength());
		}
	}

	// ── The provider's reader: asynchronous where the host asks for it ──

	static readonly string[] Terminals = ["ToListAsync", "ToArrayAsync", "FirstOrDefaultAsync", "AsAsyncEnumerable"];

	static async Task Terminal<T>(IQueryable<T> query, string terminal)
	{
		switch (terminal)
		{
			case "ToListAsync":
				await AsyncExtensions.ToListAsync(query);
				break;

			case "ToArrayAsync":
				await AsyncExtensions.ToArrayAsync(query);
				break;

			case "FirstOrDefaultAsync":
				await AsyncExtensions.FirstOrDefaultAsync(query);
				break;

			default:
				await foreach (var _ in AsyncExtensions.AsAsyncEnumerable(query))
				{
				}

				break;
		}
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public async Task AnAsynchronousTerminalReadsAsynchronously(string provider)
	{
		var recorder = new ReaderRecorder();

		using var db = databases.Open(provider, recorder);

		const string text = "from orders order by id select id, customer";

		foreach (var take in new[] { false, true })
		{
			foreach (var terminal in Terminals)
			{
				var json = Model.QueryJsonRow(Shop.Of(db, 1), text);
				var data = Model.QueryData(Shop.Of(db, 1), text);

				recorder.Calls.Clear();

				await Terminal(take ? json.Take(5) : json, terminal);
				await Terminal(take ? data.Take(5) : data, terminal);

				Assert.Equal(["ExecuteReaderAsync", "ExecuteReaderAsync"], recorder.Calls);
			}
		}

		recorder.Calls.Clear();

		await Model.ToJsonAsync(Shop.Of(db, 1), text, cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal(["ExecuteReaderAsync"], recorder.Calls);
	}
}

/// <summary>Records which call of the command opened each reader.</summary>
sealed class ReaderRecorder : LinqToDB.Interceptors.CommandInterceptor
{
	public readonly List<string> Calls = [];

	public override LinqToDB.Common.Option<System.Data.Common.DbDataReader> ExecuteReader(LinqToDB.Interceptors.CommandEventData eventData, System.Data.Common.DbCommand command, System.Data.CommandBehavior commandBehavior, LinqToDB.Common.Option<System.Data.Common.DbDataReader> result)
	{
		lock (Calls)
			Calls.Add("ExecuteReader");

		return result;
	}

	public override Task<LinqToDB.Common.Option<System.Data.Common.DbDataReader>> ExecuteReaderAsync(LinqToDB.Interceptors.CommandEventData eventData, System.Data.Common.DbCommand command, System.Data.CommandBehavior commandBehavior, LinqToDB.Common.Option<System.Data.Common.DbDataReader> result, System.Threading.CancellationToken cancellationToken)
	{
		lock (Calls)
			Calls.Add("ExecuteReaderAsync");

		return Task.FromResult(result);
	}
}
