using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using DotGram.QueryLanguage.LinqToDB;

using LinqToDB;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

public sealed class QueryTests
{
	static async Task<QlError> Refused(Func<Task> action)
	{
		var e = await Assert.ThrowsAsync<QlException>(action);

		return e.Error;
	}

	static QlError RefusedToParse(string text, QlLimits? limits = null)
	{
		return Assert.Throws<QlException>(() => QlQuery.Parse(text, Shop.Model, limits)).Error;
	}

	[Fact]
	public async Task SelectsFiltersSortsAndPages()
	{
		using var db = Shop.Sqlite();

		var rows = await Shop.Run(db, "SELECT id, total AS amount FROM orders o WHERE o.total > 50 AND status <> 'new' ORDER BY amount DESC LIMIT 3 OFFSET 1");

		Assert.Equal([[8, 100m], [7, 87.5m], [6, 75m]], rows);
	}

	[Fact]
	public async Task StarIsTheFieldsExposedForSelect()
	{
		using var db = Shop.Sqlite();

		var query = QlQuery.Parse("SELECT * FROM orders", Shop.Model);

		Assert.Equal(["id", "status", "total", "big", "score", "created", "priority", "customer", "limit"], query.Columns);

		var rows = await Shop.Run(db, "SELECT * FROM orders WHERE id = 1");

		Assert.Equal([1, "paid", 12.5m, 1_000_000_000L, 0.25, new DateTime(2026, 1, 2), 1, "c1", "l1"], rows.Single());
	}

	[Fact]
	public async Task TheRootsPolicyHoldsWhateverTheTextSays()
	{
		using var db = Shop.Sqlite();

		Assert.Equal(10, (await Shop.Ids(db, "id > 0 OR 1 = 1")).Length);
		Assert.Empty(await Shop.Ids(db, "id = 11"));
	}

	[Theory]
	[InlineData("SELECT secret FROM orders")]
	[InlineData("SELECT tenantid FROM orders")]
	[InlineData("SELECT id FROM orders WHERE secret = 's1'")]
	[InlineData("SELECT id FROM orders ORDER BY customer")]
	[InlineData("SELECT id FROM orders WHERE \"limit\" = 'l1'")]
	[InlineData("SELECT id FROM orders WHERE nosuch = 1")]
	public void AFieldHiddenOrNotAllowedIsUnknown(string text)
	{
		var error = RefusedToParse(text);

		Assert.Equal(QlErrorKind.UnknownName, error.Kind);
		Assert.StartsWith("Unknown field '", error.Message);
	}

	[Fact]
	public async Task AReservedWordIsANameInQuotes()
	{
		using var db = Shop.Sqlite();

		Assert.Equal(QlErrorKind.Syntax, RefusedToParse("SELECT limit FROM orders").Kind);
		Assert.Equal("l2", (await Shop.Run(db, "SELECT \"limit\" FROM orders WHERE id = 2")).Single()[0]);
	}

	[Fact]
	public void SyntaxErrorsSayWhere()
	{
		var error = RefusedToParse("SELECT id FROM orders WHERE id = = 1");

		Assert.Equal(QlErrorKind.Syntax, error.Kind);
		Assert.Equal(33, error.Position);
	}

	[Fact]
	public void TypesMeetOnlyByExactWidening()
	{
		Assert.Equal(QlErrorKind.Type, RefusedToParse("SELECT id FROM orders WHERE big = score").Kind);
		Assert.Equal(QlErrorKind.Type, RefusedToParse("SELECT id FROM orders WHERE total = score").Kind);
		Assert.Equal(QlErrorKind.Type, RefusedToParse("SELECT id FROM orders WHERE status = 1").Kind);
		Assert.Equal(QlErrorKind.Type, RefusedToParse("SELECT id FROM orders WHERE id").Kind);

		QlQuery.Parse("SELECT id FROM orders WHERE id = big AND id < total AND id < score", Shop.Model);
	}

	[Fact]
	public async Task IntegerDivisionTruncatesInTheDatabase()
	{
		using var db = Shop.Sqlite();

		Assert.Equal(new[] { 2, 3 }, await Shop.Ids(db, "id / 2 = 1"));
		Assert.Equal(new[] { 3, 6, 9 }, await Shop.Ids(db, "id % 3 = 0"));
		Assert.Equal(new[] { 1 }, await Shop.Ids(db, "-id / 2 = 0 AND id > 0"));
	}

	[Fact]
	public async Task DivisionByZeroOnTheHostIsAQlError()
	{
		using var db = Shop.Sqlite();

		// `10 / @z` reads no row, so linq2db computes it once, on the host, as a parameter.
		var error = await Refused(() => Shop.Ids(db, "id = 10 / @z", new() { ["z"] = 0 }));

		Assert.Equal(QlErrorKind.DivisionByZero, error.Kind);
		Assert.DoesNotContain("System", error.Message);
	}

	[Fact]
	public async Task DivisionByZeroInSqliteIsNull()
	{
		using var db = Shop.Sqlite();

		// SQLite answers NULL for x / 0; the row is not selected and nothing is thrown.
		Assert.Empty(await Shop.Ids(db, "id / @z = 1", new() { ["z"] = 0 }));
	}

	[Fact]
	public async Task InWithLiteralsAndWithAnArray()
	{
		using var db = Shop.Sqlite();

		Assert.Equal(new[] { 2, 4 }, await Shop.Ids(db, "id IN (2, 4, 12)"));
		Assert.Equal(new[] { 1, 3, 5, 6, 7, 8, 9, 10 }, await Shop.Ids(db, "id NOT IN (2, 4)"));
		Assert.Equal(new[] { 3, 5 }, await Shop.Ids(db, "id IN @ids", new() { ["ids"] = new[] { 3, 5 } }));
		Assert.Equal(new[] { 1, 2, 5 }, await Shop.Ids(db, "id IN (1, @a, 5)", new() { ["a"] = 2 }));
	}

	[Fact]
	public async Task AListArrivesWholeAndBounded()
	{
		using var db = Shop.Sqlite();

		var lazy = await Refused(() => Shop.Ids(db, "id IN @ids", new() { ["ids"] = Enumerable.Range(1, 3) }));

		Assert.Equal(QlErrorKind.Parameter, lazy.Kind);

		var many = await Refused(() => Shop.Ids(db, "id IN @ids", new() { ["ids"] = Enumerable.Range(1, 101).ToArray() }));

		Assert.Equal(QlErrorKind.Limit, many.Kind);

		var written = string.Join(", ", Enumerable.Range(1, 101));

		Assert.Equal(QlErrorKind.Limit, RefusedToParse($"SELECT id FROM orders WHERE id IN ({written})").Kind);
	}

	[Fact]
	public async Task LikeIsPortable()
	{
		using var db = Shop.Sqlite();

		await Like(db, QlLikeDialect.Standard);
	}

	internal static async Task Like(global::LinqToDB.Data.DataConnection db, QlLikeDialect dialect)
	{
		// Statuses: 1 paid, 2 new, 3 NULL, 4 a[b%c, 5 a_b, 6 100%, 7 x!y, 8 paid, 9 shipped, 10 new.
		Assert.Equal(new[] { 1, 8 },  await Shop.Ids(db, "status LIKE 'pa%'", dialect: dialect));
		Assert.Equal(new[] { 4 },     await Shop.Ids(db, "status LIKE 'a[b%'", dialect: dialect));
		Assert.Equal(new[] { 4, 5 },  await Shop.Ids(db, "status LIKE 'a_b%'", dialect: dialect));
		Assert.Equal(new[] { 5 },     await Shop.Ids(db, "status LIKE 'a!_b' ESCAPE '!'", dialect: dialect));
		Assert.Equal(new[] { 6 },     await Shop.Ids(db, "status LIKE '%!%' ESCAPE '!'", dialect: dialect));
		Assert.Equal(new[] { 7 },     await Shop.Ids(db, "status LIKE 'x!!y' ESCAPE '!'", dialect: dialect));
		Assert.Equal(new[] { 4 },     await Shop.Ids(db, "status LIKE @p", new() { ["p"] = "%[b%" }, dialect));
		Assert.Equal(new[] { 4 },     await Shop.Ids(db, "status LIKE @p ESCAPE '#'", new() { ["p"] = "a[b#%c" }, dialect));
		Assert.Empty(await Shop.Ids(db, "status LIKE '%\\%'", dialect: dialect));

		var bad = await Assert.ThrowsAsync<QlException>(() => Shop.Ids(db, "status LIKE @p ESCAPE '#'", new() { ["p"] = "a#" }, dialect));

		Assert.Equal(QlErrorKind.Parameter, bad.Error.Kind);
		Assert.Equal(QlErrorKind.Type, Assert.Throws<QlException>(() => QlQuery.Parse("SELECT id FROM orders WHERE status LIKE 'a#b' ESCAPE '#'", Shop.Model)).Error.Kind);
	}

	[Fact]
	public void ArithmeticChainsAreCapped()
	{
		var eight = string.Join(" + ", Enumerable.Repeat("id", 8));
		var nine  = string.Join(" + ", Enumerable.Repeat("id", 9));

		QlQuery.Parse($"SELECT id FROM orders WHERE id = {eight}", Shop.Model);

		Assert.Equal(QlErrorKind.Limit, RefusedToParse($"SELECT id FROM orders WHERE id = {nine}").Kind);

		// Parentheses do not reset the count: ((id + id) + id) + … is the same left-deep tree.
		var nested = "id";

		for (var i = 0; i < 8; i++)
			nested = $"({nested} + id)";

		Assert.Equal(QlErrorKind.Limit, RefusedToParse($"SELECT id FROM orders WHERE id = {nested}").Kind);
	}

	[Fact]
	public async Task LongOrChainsAreBalancedAndCountedOnTheTree()
	{
		using var db = Shop.Sqlite();

		var ors   = string.Join(" OR ", Enumerable.Range(1, 12).Select(i => $"id = {i}"));
		var query = QlQuery.Parse("SELECT id FROM orders WHERE " + ors, Shop.Model);

		query.Translate(Shop.Roots(db).Resolve(Shop.Orders, new Tenant(1)), null, new QlLinqToDbAdapter(QlLikeDialect.Standard), out var stats);

		Assert.True(stats.Depth < 16, $"depth {stats.Depth}");
		Assert.Equal(10, (await Shop.Ids(db, ors)).Length);

		var many = string.Join(" OR ", Enumerable.Range(1, 40).Select(i => $"id = {i}"));

		Assert.Equal(QlErrorKind.Limit, (await Refused(() => Shop.Ids(db, many))).Kind);
	}

	[Fact]
	public void DeepTextIsRefusedBeforeBinding()
	{
		var deep = new string('(', 200) + "id = 1" + new string(')', 200);

		Assert.Equal(QlErrorKind.Limit, RefusedToParse("SELECT id FROM orders WHERE " + deep).Kind);

		var nots = string.Concat(Enumerable.Repeat("NOT ", 100)) + "id = 1";

		Assert.Equal(QlErrorKind.Limit, RefusedToParse("SELECT id FROM orders WHERE " + nots).Kind);

		var huge = "SELECT id FROM orders WHERE " + new string('(', 4000) + "id = 1" + new string(')', 4000);

		Assert.Equal(QlErrorKind.Limit, RefusedToParse(huge, QlLimits.Default with { MaxTextLength = 64 * 1024 }).Kind);
	}

	[Fact]
	public async Task ParametersAreTypedByWhereTheyStand()
	{
		using var db = Shop.Sqlite();

		Assert.Equal(new[] { 5 }, await Shop.Ids(db, "id = @p", new() { ["p"] = 5 }));
		Assert.Equal(new[] { 5 }, await Shop.Ids(db, "big = @p", new() { ["p"] = 5_000_000_000L }));
		Assert.Equal(new[] { 1, 2 }, await Shop.Ids(db, "big < @p", new() { ["p"] = 2_000_000_001 }));
		Assert.Equal(new[] { 1, 2 }, await Shop.Ids(db, "created < @p", new() { ["p"] = new DateTime(2026, 1, 4) }));

		Assert.Equal(QlErrorKind.Parameter, (await Refused(() => Shop.Ids(db, "id = @p", new() { ["p"] = "5" }))).Kind);
		Assert.Equal(QlErrorKind.Parameter, (await Refused(() => Shop.Ids(db, "score = @p", new() { ["p"] = double.NaN }))).Kind);
		Assert.Equal(QlErrorKind.Parameter, (await Refused(() => Shop.Ids(db, "id = @p", new()))).Kind);
		Assert.Equal(QlErrorKind.Type, RefusedToParse("SELECT id FROM orders WHERE @a = @b").Kind);
		Assert.Equal(QlErrorKind.Type, RefusedToParse("SELECT id FROM orders WHERE id = @p AND status = @p").Kind);
	}

	[Fact]
	public async Task NullIsAskedWithIsNull()
	{
		using var db = Shop.Sqlite();

		Assert.Equal(new[] { 3, 6, 9 }, await Shop.Ids(db, "priority IS NULL"));
		Assert.Equal(new[] { 3 }, await Shop.Ids(db, "status IS NULL"));
		Assert.Empty(await Shop.Ids(db, "id IS NULL"));
		Assert.Equal(QlErrorKind.Type, RefusedToParse("SELECT id FROM orders WHERE status = NULL").Kind);
	}

	[Fact]
	public async Task NotEqualIsLeftToTheProvidersNullSetting()
	{
		using var db = Shop.Sqlite();

		// `<>` is `!=` in the tree; linq2db's default compares NULL as C# does, so the NULL status is selected.
		Assert.Equal(new[] { 3, 4, 5, 6, 7, 9 }, await Shop.Ids(db, "status <> 'paid' AND status <> 'new'"));
	}

	[Fact]
	public async Task BetweenAndPaging()
	{
		using var db = Shop.Sqlite();

		Assert.Equal(new[] { 3, 4, 5 }, await Shop.Ids(db, "id BETWEEN 3 AND 5"));
		Assert.Equal(new[] { 1, 2, 6, 7, 8, 9, 10 }, await Shop.Ids(db, "id NOT BETWEEN 3 AND 5"));

		var page = await Shop.Run(db, "SELECT id FROM orders ORDER BY status LIMIT @n OFFSET @o", new() { ["n"] = 2, ["o"] = 1 }, limits: null);

		Assert.Equal(2, page.Count);
		Assert.Equal(QlErrorKind.Limit, RefusedToParse("SELECT id FROM orders OFFSET 10001").Kind);
	}

	[Fact]
	public async Task RowsAreCappedWhateverLimitSays()
	{
		using var db = Shop.Sqlite();

		var limits = QlLimits.Default with { MaxRows = 4 };

		Assert.Equal(4, (await Shop.Run(db, "SELECT id FROM orders", limits: limits)).Count);
		Assert.Equal(4, (await Shop.Run(db, "SELECT id FROM orders LIMIT 100", limits: limits)).Count);
	}

	[Fact]
	public async Task CancellationIsAQlError()
	{
		using var db  = Shop.Sqlite();
		using var cts = new System.Threading.CancellationTokenSource();

		cts.Cancel();

		var query = QlQuery.Parse("SELECT id FROM orders", Shop.Model);
		var error = await Refused(async () =>
		{
			await foreach (var _ in query.ExecuteAsync(Shop.Roots(db), new Tenant(1), null, new QlLinqToDbAdapter(QlLikeDialect.Standard), cts.Token))
			{
			}
		});

		Assert.Equal(QlErrorKind.Canceled, error.Kind);
	}

	[Fact]
	public void TheAuditRefusesWhatTheTranslatorDoesNotBuild()
	{
		using var db = Shop.Sqlite();

		var adapter = new QlLinqToDbAdapter(QlLikeDialect.Standard);
		var root    = db.GetTable<Order>().Where(o => o.TenantId == 1);
		var query   = QlQuery.Parse("SELECT id FROM orders WHERE status = 'paid'", Shop.Model);

		query.Translate(root, null, adapter, out _);

		Expression<Func<Order, bool>> upper  = o => o.Status!.ToUpper() == "PAID";
		Expression<Func<Order, bool>> secret = o => o.Secret == "s1";
		Expression<Func<Order, bool>> host   = o => o.Id == int.Parse("1");

		foreach (var predicate in new[] { upper, secret, host })
		{
			var tree  = Expression.Call(typeof(Queryable), "Where", [typeof(Order)], root.Expression, Expression.Quote(predicate));
			var error = Assert.Throws<QlException>(() => Audit.Check(tree, root.Expression, Shop.Orders, adapter, QlLimits.Default)).Error;

			Assert.Equal(QlErrorKind.Audit, error.Kind);
		}
	}

	[Fact]
	public void TheCacheAdmitsOnlyWhatBound()
	{
		var cache = new QlQueryCache(Shop.Model, QlLimits.Default);

		Assert.Same(cache.Get("SELECT id FROM orders"), cache.Get("SELECT id FROM orders"));
		Assert.Throws<QlException>(() => cache.Get("SELECT nosuch FROM orders"));
		Assert.Equal(1, cache.Count);
	}
}
