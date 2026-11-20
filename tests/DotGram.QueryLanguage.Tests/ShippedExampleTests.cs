using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>
/// The examples of the package's README and skill, run rather than read: each holds what the page says it
/// gives, over the smallest context of the page's shape.
/// </summary>
public sealed class ShippedExampleTests
{
	public sealed class AppOrder
	{
		public int      Id       { get; set; }
		public int      TenantId { get; set; }
		public string   Status   { get; set; } = "";
		public decimal  Total    { get; set; }
		public DateTime Created  { get; set; }
		public decimal  Margin   { get; set; }
		public byte[]?  Blob     { get; set; }
	}

	public sealed class AppDb(int tenant)
	{
		public int TenantId { get; } = tenant;

		public IQueryable<AppOrder> Orders { get; } = new[]
		{
			new AppOrder { Id = 1, TenantId = 1, Status = "paid", Total = 120.50m, Created = new DateTime(2026, 11, 1), Margin = 10m, Blob = [1, 2, 3] },
			new AppOrder { Id = 2, TenantId = 1, Status = "new",  Total = 80.00m,  Created = new DateTime(2026, 11, 2), Margin = 30m },
			new AppOrder { Id = 3, TenantId = 1, Status = "paid", Total = 250.00m, Created = new DateTime(2026, 11, 3), Margin = 50m },
			new AppOrder { Id = 4, TenantId = 2, Status = "paid", Total = 999.00m, Created = new DateTime(2026, 11, 4), Margin = 90m },
		}.AsQueryable();
	}

	/// <summary>The README's model, as it declares it.</summary>
	static QlModel<AppDb> Model()
	{
		var builder = new QlModelBuilder<AppDb>();

		builder.Entity("orders", db => db.Orders.Where(o => o.TenantId == db.TenantId))
			.Column("id",      o => o.Id)
			.Column("status",  o => o.Status)
			.Column("total",   o => o.Total)
			.Column("created", o => o.Created)
			.Column("margin",  o => o.Margin, QlUse.Filter);

		return builder.Build();
	}

	[Fact]
	public async Task TheFirstExampleOfTheReadme()
	{
		var json = await Model().ToJsonAsync(new AppDb(1), "from orders where total > 100 order by total desc take 2 select id, total", cancellationToken: TestContext.Current.CancellationToken);

		Assert.Equal("""{"columns":["id","total"],"rows":[[3,250.00],[1,120.50]]}""", json);
	}

	[Fact]
	public void ThePosterTextReads()
	{
		var info = Model().Describe(
			"""
			from orders
			where status in @statuses && total >= @min
			order by created desc, id
			take 20
			select id, total, created
			""");

		Assert.Equal(["id", "total", "created"], info.Columns.Select(c => c.Name));
		Assert.Equal(["statuses", "min"], info.Parameters.Select(p => p.Name));
	}

	[Fact]
	public void AFilterOnlyFieldIsTestedNeverReturned()
	{
		var model = Model();

		Assert.Single(model.QueryData(new AppDb(1), "from orders where margin > 40 select id").ToList());
		Assert.Equal(QlErrorCode.UnknownName, Assert.Throws<QlException>(() => model.QueryData(new AppDb(1), "from orders select margin")).Error.Code);
	}

	// ── The web endpoint ──

	sealed record QueryRequest(string Text, Dictionary<string, JsonElement>? Values);

	/// <summary>The README's <c>Loose</c>, as it is written there.</summary>
	static object? Loose(JsonElement value)
	{
		switch (value.ValueKind)
		{
			case JsonValueKind.Number:
				if (value.TryGetInt64(out var integer))
					return integer;

				if (value.TryGetDecimal(out var exact))
					return exact;

				return value.GetDouble();

			case JsonValueKind.String: return value.GetString();
			case JsonValueKind.True:   return true;
			case JsonValueKind.False:  return false;
			case JsonValueKind.Array:  return value.EnumerateArray().Select(Loose).ToList();
			default:                   return null;
		}
	}

	/// <summary>What the endpoint does with a request: the document, or the refusal it answers 400 with.</summary>
	static async Task<string> Endpoint(string body)
	{
		var request = JsonSerializer.Deserialize<QueryRequest>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
		var values  = request.Values?.ToDictionary(v => v.Key, v => Loose(v.Value));

		try
		{
			return await Model().ToJsonAsync(new AppDb(1), request.Text, values);
		}
		catch (QlException e)
		{
			return JsonSerializer.Serialize(new { e.Error.Code, e.Error.Message, e.Error.Position });
		}
	}

	[Fact]
	public async Task TheEndpointTakesValuesFromJson()
	{
		var json = await Endpoint("""{"text":"from orders where status in @statuses && total >= @min order by id select id","values":{"statuses":["paid","new"],"min":100.5}}""");

		Assert.Equal("""{"columns":["id"],"rows":[[1],[3]]}""", json);

		var refused = await Endpoint("""{"text":"from orders where price > 1"}""");

		using var error = JsonDocument.Parse(refused);

		Assert.Equal((2, "Unknown name `price`.", 18), (error.RootElement.GetProperty("Code").GetInt32(), error.RootElement.GetProperty("Message").GetString(), error.RootElement.GetProperty("Position").GetInt32()));
	}

	[Fact]
	public void FormatsAsTheReadmeRegistersThem()
	{
		var builder = new QlModelBuilder<AppDb>()
			.Format<byte[]>(b => Convert.ToBase64String(b))
			.Format<DateTime>(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

		builder.Entity("orders", db => db.Orders.Where(o => o.TenantId == db.TenantId)).Column("id", o => o.Id).Column("created", o => o.Created).Column("blob", o => o.Blob);

		var rows = builder.Build().QueryJsonRow(new AppDb(1), "from orders order by id take 2").ToList();

		Assert.Equal(["[1,\"2026-11-01\",\"AQID\"]", "[2,\"2026-11-02\",null]"], rows);
	}

	// ── The skill ──

	[Fact]
	public void TheSkillsValues()
	{
		var values = new Dictionary<string, object?> { ["min"] = 100, ["statuses"] = new[] { "paid", "new" } };

		var rows = Model().QueryData(new AppDb(1), "from orders where total > @min && status in @statuses select id", values).ToList();

		Assert.Equal([1, 3], rows.Select(r => (int)r[0]!));
	}

	[Fact]
	public void CountAndAnyFollowTheRows()
	{
		var rows = Model().QueryJsonRow(new AppDb(1), "from orders where total > 100 select id");

		Assert.Equal(2, rows.Count());
		Assert.True(rows.Any());
	}
}
