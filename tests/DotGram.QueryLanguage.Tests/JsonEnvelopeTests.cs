using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>
/// The message around the rows: the status a document opens with, the result it closes with, whether the bound cut
/// it, and the documents of a refusal and of a description.
/// </summary>
[Collection(nameof(Databases))]
public sealed class JsonEnvelopeTests(Databases databases)
{
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

	static Task<string> Document(QlModel<Crm> model, Crm crm, string text, QlJsonFormat format = QlJsonFormat.Array)
	{
		return model.ToJsonAsync(crm, text, format: format, cancellationToken: TestContext.Current.CancellationToken);
	}

	/// <summary>The names of the members of a document's object, in order: <c>status</c> first and <c>result</c> last, none twice.</summary>
	static string[] Members(string json)
	{
		using var document = JsonDocument.Parse(json);

		var names = document.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

		Assert.Equal(names.Length, names.Distinct().Count());

		return names;
	}

	// ── A document: started, the rows, completed ──

	[Theory]
	[MemberData(nameof(Places))]
	public async Task ADocumentOpensWithItsStatusAndClosesWithItsResult(string place)
	{
		foreach (var format in new[] { QlJsonFormat.Array, QlJsonFormat.Object })
		{
			var json = await Document(Crm.Model(), CrmOf(place), "from customers order by id select id, name", format);

			Assert.Equal(["status", "columns", "rows", "count", "truncated", "result"], Members(json));

			using var document = JsonDocument.Parse(json);

			var root = document.RootElement;

			Assert.Equal("started",   root.GetProperty("status").GetString());
			Assert.Equal(3,           root.GetProperty("count").GetInt32());
			Assert.Equal(3,           root.GetProperty("rows").GetArrayLength());
			Assert.False(root.GetProperty("truncated").GetBoolean());
			Assert.Equal("completed", root.GetProperty("result").GetString());

			var row = root.GetProperty("rows")[0];

			if (format == QlJsonFormat.Array)
				Assert.Equal(JsonValueKind.Array, row.ValueKind);
			else
				Assert.Equal(["id", "name"], row.EnumerateObject().Select(p => p.Name));
		}
	}

	[Theory]
	[MemberData(nameof(Places))]
	public async Task TheWarningsFollowTheColumns(string place)
	{
		var json = await Document(Crm.Model(), CrmOf(place), "from customers let x = id + 1 where id == 1 select id");

		Assert.Equal(
			"""{"status":"started","columns":["id"],"warnings":[{"code":"UnusedName","message":"`x` is never used.","position":19}],"rows":[[1]],"count":1,"truncated":false,"result":"completed"}""",
			json);
	}

	// ── truncated: the bound, not the text ──

	[Theory]
	[MemberData(nameof(Places))]
	public async Task TruncatedSaysTheBoundLeftRowsOut(string place)
	{
		static string Tail(int count, bool truncated)
		{
			return "\"count\":" + count + ",\"truncated\":" + (truncated ? "true" : "false") + ",\"result\":\"completed\"}";
		}

		var crm = CrmOf(place);

		const string all = "from customers order by id select id";

		// Three rows: a bound of two cuts them, a bound of three holds exactly as many, and none never cuts.
		Assert.Equal("""{"status":"started","columns":["id"],"rows":[[1],[2]],""" + Tail(2, true), await Document(Crm.Model(new QlOptions { MaxRows = 2 }), crm, all));
		Assert.Equal("""{"status":"started","columns":["id"],"rows":[[1],[2],[3]],""" + Tail(3, false), await Document(Crm.Model(new QlOptions { MaxRows = 3 }), crm, all));
		Assert.Equal("""{"status":"started","columns":["id"],"rows":[[1],[2],[3]],""" + Tail(3, false), await Document(Crm.Model(new QlOptions { MaxRows = null }), crm, all));

		// A take of the text is what was asked for, not a cut; a take past the bound is cut by the bound.
		Assert.Equal("""{"status":"started","columns":["id"],"rows":[[1],[2]],""" + Tail(2, false), await Document(Crm.Model(new QlOptions { MaxRows = 2 }), crm, "from customers order by id take 2 select id"));
		Assert.Equal("""{"status":"started","columns":["id"],"rows":[[1]],""" + Tail(1, true), await Document(Crm.Model(new QlOptions { MaxRows = 1 }), crm, "from customers order by id take 10 select id"));

		// The object form is cut the same way.
		Assert.Equal("""{"status":"started","columns":["id"],"rows":[{"id":1},{"id":2}],""" + Tail(2, true), await Document(Crm.Model(new QlOptions { MaxRows = 2 }), crm, all, QlJsonFormat.Object));
	}

	/// <summary>The row past the bound is not asked for where the bound is the largest an int holds: one more would wrap.</summary>
	[Theory]
	[MemberData(nameof(Places))]
	public async Task TheLargestBoundDoesNotWrap(string place)
	{
		var json = await Document(Crm.Model(new QlOptions { MaxRows = int.MaxValue }), CrmOf(place), "from customers order by id select id");

		Assert.Equal("""{"status":"started","columns":["id"],"rows":[[1],[2],[3]],"count":3,"truncated":false,"result":"completed"}""", json);
	}

	/// <summary>The other methods keep their Take of the bound: no row past it.</summary>
	[Theory]
	[MemberData(nameof(Places))]
	public void TheRowsAndTheDataKeepTheBound(string place)
	{
		var model = Crm.Model(new QlOptions { MaxRows = 2 });

		Assert.Equal(2, model.QueryJsonRow(CrmOf(place), "from customers order by id select id").ToList().Count);
		Assert.Equal(2, model.QueryData(CrmOf(place), "from customers order by id select id").ToList().Count);
	}

	// ── A refusal ──

	public static IEnumerable<TheoryDataRow<QlErrorCode, string>> Refusals()
	{
		yield return new(QlErrorCode.Syntax,         "from customers where");
		yield return new(QlErrorCode.UnknownName,    "from customers where nothing > 1");
		yield return new(QlErrorCode.Type,           "from customers where name > 1");
		yield return new(QlErrorCode.Limit,          "from customers where id in (" + string.Join(",", Enumerable.Range(0, 101)) + ")");
		yield return new(QlErrorCode.Parameter,      "from customers where id > @n");
		yield return new(QlErrorCode.DivisionByZero, "from customers where id > 1 / 0");
		yield return new(QlErrorCode.Overflow,       "from customers where id > 2147483647 + 1");
	}

	[Theory]
	[MemberData(nameof(Refusals))]
	public async Task ARefusalIsItsOwnDocument(QlErrorCode code, string text)
	{
		var error = (await Assert.ThrowsAsync<QlException>(() => Document(Crm.Model(), CrmOf("Memory"), text))).Error;
		var json  = error.ToJson();

		Assert.Equal(code, error.Code);
		Assert.Equal(["status", "error"], Members(json));

		using var document = JsonDocument.Parse(json);

		var root  = document.RootElement;
		var inner = root.GetProperty("error");

		Assert.Equal("error",         root.GetProperty("status").GetString());
		Assert.Equal(code.ToString(), inner.GetProperty("code").GetString());
		Assert.Equal(error.Message,   inner.GetProperty("message").GetString());

		if (error.Position is { } position)
		{
			Assert.Equal(["code", "message", "position"], inner.EnumerateObject().Select(p => p.Name));
			Assert.Equal(position, inner.GetProperty("position").GetInt32());
		}
		else
		{
			Assert.Equal(["code", "message"], inner.EnumerateObject().Select(p => p.Name));
		}
	}

	[Fact]
	public void ARefusalByteForByte()
	{
		var error = Assert.Throws<QlException>(() => Crm.Model().Describe("from customers where totl > 1")).Error;

		Assert.Equal("""{"status":"error","error":{"code":"UnknownName","message":"Unknown name `totl`.","position":21}}""", error.ToJson());

		var missing = Assert.Throws<QlException>(() => Crm.Model().QueryData(CrmOf("Memory"), "from customers where id > @low")).Error;

		Assert.Equal("""{"status":"error","error":{"code":"Parameter","message":"`@low` has no value.","position":26}}""", missing.ToJson());

		// Every refusal of the language today has a position; one without would leave the member out.
		Assert.Equal("""{"status":"error","error":{"code":"Limit","message":"\u003Cno\u003E \"place\""}}""", new QlError(QlErrorCode.Limit, "<no> \"place\"", null).ToJson());
	}

	// ── The provider's failure: its exception, never its text ──

	[Theory]
	[MemberData(nameof(Places))]
	public async Task TheProvidersFailureIsItsOwnException(string place)
	{
		var builder = new QlModelBuilder<Crm>();

		builder.Format<DateTime>(d => d.Day == 2 ? throw new InvalidOperationException("secret: the host's format failed") : "ok");
		builder.Entity(c => c.Customers).Column("id", c => c.Id).Column("created", c => c.Created);

		var model = builder.Build();

		var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => Document(model, CrmOf(place), "from customers order by id"));

		Assert.Contains("secret", failure.Message);
	}

	// ── A description ──

	[Fact]
	public void ADescriptionAsJson()
	{
		var info = Crm.Model().Describe("from customers let x = id where total > @low && name in @names select id, total");

		Assert.Equal(
			"""{"status":"started","columns":[{"name":"id","type":"Int","nullable":false},{"name":"total","type":"Decimal","nullable":true}],"parameters":[{"name":"low","type":"Decimal","nullable":true,"list":false},{"name":"names","type":"String","nullable":false,"list":true}],"warnings":[{"code":"UnusedName","message":"`x` is never used.","position":19}],"result":"completed"}""",
			info.ToJson());

		Assert.Equal(["status", "columns", "parameters", "result"], Members(Crm.Model().Describe("from customers select id").ToJson()));
		Assert.Equal(["status", "columns", "parameters", "warnings", "result"], Members(info.ToJson()));
		Assert.Equal("""{"status":"started","columns":[{"name":"id","type":"Int","nullable":false}],"parameters":[],"result":"completed"}""", Crm.Model().Describe("from customers select id").ToJson());
	}

	/// <summary>The description the reference page shows is what <c>Describe</c> writes, byte for byte.</summary>
	[Fact]
	public void TheDescriptionOnTheReferencePageIsTheRealOne()
	{
		var json = Crm.Model().Describe("from customers where total > @low select id, total").ToJson();

		var directory = new DirectoryInfo(AppContext.BaseDirectory);

		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs", "query-language.md")))
			directory = directory.Parent;

		Assert.NotNull(directory);

		var page = File.ReadAllText(Path.Combine(directory.FullName, "docs", "query-language.md"));

		Assert.Contains("\n" + json + "\n", page.Replace("\r\n", "\n"), StringComparison.Ordinal);
	}
}
