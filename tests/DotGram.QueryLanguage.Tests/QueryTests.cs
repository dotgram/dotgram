using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>Texts run end to end on SQLite and SQL Server, held to what the same chain gives in memory.</summary>
[Collection(nameof(Databases))]
public sealed class QueryTests(Databases databases)
{
	static readonly QlModel<Shop> Model = Shop.Model();

	/// <summary>The tenant's own rows, as the source gives them.</summary>
	static readonly Order[] Mine = [.. Shop.Rows().Where(o => o.TenantId == 1)];

	List<object?[]> Run(string provider, string text, object? values = null, int tenant = 1, QlModel<Shop>? model = null)
	{
		return Rows.Run(model ?? Model, databases.Get(provider), text, values, tenant);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void WithoutClausesTheRowsAreTheFieldsWithSelect(string provider)
	{
		var rows  = Run(provider, "from orders order by id");
		var first = rows[0];

		Assert.Equal(20, rows.Count);
		Assert.Equal(18, first.Length);
		Assert.Equal(1, first[0]);
		Assert.Equal("paid", first[1]);
		Assert.Equal(12.5m, first[2]);
		Assert.Equal(new DateOnly(2026, 1, 2), first[15]);
		Assert.Equal(Kind.Paid, first[16]);
		Assert.Equal(new byte[] { 1 }, first[17]);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void TheThreeHeadsAndTheLinqSpellingOfSortingGiveTheSameRows(string provider)
	{
		var expected = Run(provider, "from orders o where o.qty > 1 order by id desc select id");

		Assert.NotEmpty(expected);
		Assert.Equal(expected, Run(provider, "from orders as o where o.qty > 1 order by id desc select id"));
		Assert.Equal(expected, Run(provider, "from o in orders where o.qty > 1 orderby id descending select id"));
		Assert.Equal(expected, Run(provider, "from orders where orders.qty > 1 orderby id desc select id"));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void StarInAListSpreadsTheRowBesideTheOtherItems(string provider)
	{
		var rows = Run(provider, "from orders order by id select 1 as one, *, total");

		Assert.Equal(20, rows.Count);
		Assert.Equal(19, rows[0].Length);
		Assert.Equal(1, rows[0][0]);
		Assert.Equal(1, rows[0][1]);
		Assert.Equal(12.5m, rows[0][^1]);
		Assert.Equal(Mine.OrderBy(o => o.Id).Select(o => (object?)o.Total), rows.Select(r => r[^1]));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void WithoutASelectALetIsPartOfTheRowAsStarGivesIt(string provider)
	{
		var plain = Run(provider, "from orders order by id let t = total * 2");
		var star  = Run(provider, "from orders order by id let t = total * 2 select *");

		Assert.Equal(19, plain[0].Length);
		Assert.Equal(star, plain);
		Assert.Equal(Mine.OrderBy(o => o.Id).Select(o => (object?)(o.Total * 2)), plain.Select(r => r[^1]));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void TheSourceChoosesTheRows(string provider)
	{
		Assert.Equal(Enumerable.Range(21, 20), Run(provider, "from orders order by id select id", tenant: 2).First<int>());
		Assert.Equal(Enumerable.Range(1, 20),  Run(provider, "from orders order by id select id", tenant: 1).First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void EachClauseIsACallAsWritten(string provider)
	{
		var rows = Run(provider, "from orders let t = total * 2 where t > 100 select id, t where id < 15 order by t desc take 3");

		var expected = Mine.Select(o => new { o.Id, T = o.Total * 2 }).Where(r => r.T > 100).Where(r => r.Id < 15).OrderByDescending(r => r.T).Take(3);

		Assert.Equal(expected.Select(r => new object?[] { r.Id, r.T }), rows);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void LetsOverTheEntityAndAfterASelect(string provider)
	{
		var rows = Run(provider, "from orders o let a = qty + 1 let b = a * 2 where b > 6 && o.qty < 5 select id, a, b let c = b - a where c > 3 order by id select id, c");

		var expected = Mine
			.Select(o => new { o.Id, o.Qty, A = o.Qty + 1 })
			.Select(r => new { r.Id, r.Qty, r.A, B = r.A * 2 })
			.Where(r => r.B > 6 && r.Qty < 5)
			.Select(r => new { r.Id, C = r.B - r.A })
			.Where(r => r.C > 3)
			.OrderBy(r => r.Id);

		Assert.Equal(expected.Select(r => new object?[] { r.Id, r.C }), rows);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void PagesAreTakenAsWritten(string provider)
	{
		Assert.Equal([3, 4, 5],       Run(provider, "from orders order by id skip 2 take 3 select id").First<int>());
		Assert.Equal([2, 3, 4, 5],    Run(provider, "from orders order by id take 5 skip 1 select id").First<int>());
		Assert.Equal([5, 4, 3],       Run(provider, "from orders order by id take 5 order by id desc take 3 select id").First<int>());
		Assert.Equal([3, 4],          Run(provider, "from orders order by id skip @s take @t select id", new { s = 2, t = 2 }).First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void MaxRowsIsTheLastTake(string provider)
	{
		Assert.Equal(5,  Run(provider, "from orders order by id select id", model: Shop.Model(QlOptions.Default with { MaxRows = 5 })).Count);
		Assert.Equal(20, Run(provider, "from orders order by id select id", model: Shop.Model(QlOptions.Default with { MaxRows = null })).Count);
		Assert.Equal(3,  Run(provider, "from orders order by id take 3 select id", model: Shop.Model(QlOptions.Default with { MaxRows = 5 })).Count);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ParametersAreValuesOfTheCall(string provider)
	{
		const string text = "from orders where total > @min && status in @statuses order by id select id";

		var paid = Mine.Where(o => o.Total > 100 && o.Status is "paid" or "new").Select(o => o.Id);

		Assert.Equal(paid, Run(provider, text, new { min = 100, statuses = new[] { "paid", "new" } }).First<int>());
		Assert.Equal(Mine.Where(o => o.Total > 200 && o.Status == "shipped").Select(o => o.Id), Run(provider, text, new { min = 200m, statuses = new List<string> { "shipped" } }).First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void NullComparesAsCSharp(string provider)
	{
		var none = Mine.Where(o => o.Status == null).Select(o => o.Id);

		Assert.Equal(none, Run(provider, "from orders where status == @s order by id select id", new { s = (string?)null }).First<int>());
		Assert.Equal(none, Run(provider, "from orders where status is null order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Status != "paid").Select(o => o.Id), Run(provider, "from orders where status != 'paid' order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Priority == null).Select(o => o.Id), Run(provider, "from orders where priority == null order by id select id").First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void PredicatesTranslate(string provider)
	{
		Assert.Equal(Mine.Where(o => o.Qty is 1 or 3).Select(o => o.Id),             Run(provider, "from orders where qty in (1, 3) order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Qty is not (1 or 3)).Select(o => o.Id),       Run(provider, "from orders where qty not in (1, 3) order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Qty >= 2 && o.Qty <= 4).Select(o => o.Id),    Run(provider, "from orders where qty between 2 and 4 order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Qty is < 2 or > 4).Select(o => o.Id),         Run(provider, "from orders where qty not between 2 and 4 order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Priority is not null).Select(o => o.Id),      Run(provider, "from orders where priority is not null order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Flag == true).Select(o => o.Id),              Run(provider, "from orders where flag order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => !(o.Active && o.Qty > 2)).Select(o => o.Id),    Run(provider, "from orders where !(active && qty > 2) order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => o.Flag == !o.Active).Select(o => o.Id),         Run(provider, "from orders where flag == !active order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => !o.Flag == o.Active).Select(o => o.Id),         Run(provider, "from orders where (!flag) == active order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => !o.Customer.StartsWith("c1", StringComparison.Ordinal)).Select(o => o.Id), Run(provider, "from orders where !customer.StartsWith('c1') order by id select id").First<int>());
		Assert.Equal(Mine.Where(o => string.CompareOrdinal(o.Customer, "c2") > 0).Select(o => o.Id), Run(provider, "from orders where customer > 'c2' order by id select id").First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void MembersTranslate(string provider)
	{
		var rows = Run(provider, "from orders where customer.StartsWith('c1') && created.Year == 2026 order by id select id, customer.ToUpper() as up, note.Length as n, shipped.Month as m");

		var expected = Mine.Where(o => o.Customer.StartsWith("c1", StringComparison.Ordinal)).Select(o => new object?[] { o.Id, o.Customer.ToUpperInvariant(), o.Note.Length, o.Shipped?.Month });

		Assert.Equal(expected, rows);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void NullsGoWhereTheTextSays(string provider)
	{
		var last  = Run(provider, "from orders order by priority nulls last, id select priority").First<int?>();
		var first = Run(provider, "from orders order by priority desc nulls first, id select priority").First<int?>();

		Assert.Equal(Mine.OrderBy(o => o.Priority is null ? 1 : 0).ThenBy(o => o.Priority).ThenBy(o => o.Id).Select(o => o.Priority), last);
		Assert.Equal(Mine.OrderBy(o => o.Priority is null ? 0 : 1).ThenByDescending(o => o.Priority).ThenBy(o => o.Id).Select(o => o.Priority), first);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void RepeatedOrderIsEmittedAsWritten(string provider)
	{
		// linq2db by default keeps the last order of two in a row, which is its reading of the chain, not ours.
		Assert.Equal(Mine.OrderByDescending(o => o.Id).Select(o => o.Id), Run(provider, "from orders order by qty order by id desc select id").First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AggregatesAreOneRow(string provider)
	{
		var row = Assert.Single(Run(provider, "from orders where qty > 0 select Count(*) as c, Count(priority) as p, Sum(qty) as s, Avg(qty) as a, Avg(total) as t, Min(status) as mn, Max(created) as mx"));
		var set = Mine.Where(o => o.Qty > 0).ToArray();

		Assert.Equal(set.Length,                                                 row[0]);
		Assert.Equal(set.Count(o => o.Priority != null),                         row[1]);
		Assert.Equal(set.Sum(o => o.Qty),                                        row[2]);
		Assert.Equal(set.Average(o => (double)o.Qty),                            (double)row[3]!, 6);
		Assert.Equal(set.Average(o => o.Total),                                  (decimal)row[4]!, 2);
		Assert.Equal(set.Where(o => o.Status != null).Select(o => o.Status!).Min(StringComparer.Ordinal), row[5]);
		Assert.Equal(set.Max(o => o.Created),                                    row[6]);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AnAggregateOfNoRowsIsOneRow(string provider)
	{
		var row = Assert.Single(Run(provider, "from orders where id > 1000 select Count(*) as c, Sum(qty) as s, Avg(qty) as a, Max(day) as d"));

		Assert.Equal(new object?[] { 0, null, null, null }, row);
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void AnAggregateRowGoesOn(string provider)
	{
		var row = Assert.Single(Run(provider, "from orders select Count(*) as c, Sum(total) as s where c > 1 select s / c as average"));

		Assert.Equal(Mine.Average(o => o.Total), (decimal)row[0]!, 2);
		Assert.Empty(Run(provider, "from orders select Count(*) as c where c > 100"));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void FieldsOutsideTheTypesAreComparedSortedAndReturnedAsThemselves(string provider)
	{
		var day  = new DateOnly(2026, 1, 4);
		var rows = Run(provider, "from orders where day == @d || kind == @k order by kind desc, day, id select id, day, kind", new { d = day, k = Kind.Shipped });

		var expected = Mine.Where(o => o.Day == day || o.Kind == Kind.Shipped).OrderByDescending(o => o.Kind).ThenBy(o => o.Day).ThenBy(o => o.Id);

		Assert.Equal(expected.Select(o => new object?[] { o.Id, o.Day, o.Kind }), rows);
		Assert.Equal(Mine.Where(o => o.Day < day).Select(o => o.Id), Run(provider, "from orders where day < @d order by id select id", new { d = day }).First<int>());
		Assert.Equal(Mine.Where(o => o.Kind is Kind.New or Kind.Paid).Select(o => o.Id), Run(provider, "from orders where kind in @ks order by id select id", new { ks = new[] { Kind.New, Kind.Paid } }).First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ArithmeticOfParametersIsComputedByTheCall(string provider)
	{
		const string text = "from orders where qty > @a / @b order by id select id";

		Assert.Equal(Mine.Where(o => o.Qty > 3).Select(o => o.Id), Run(provider, text, new { a = 7, b = 2 }).First<int>());

		var error = Assert.Throws<QlException>(() => Run(provider, text, new { a = 7, b = 0 })).Error;

		Assert.Equal((QlErrorCode.DivisionByZero, (int?)27), (error.Code, error.Position));

		var overflow = Assert.Throws<QlException>(() => Run(provider, "from orders where qty > @a * 2 select id", new { a = int.MaxValue })).Error;

		Assert.Equal((QlErrorCode.Overflow, (int?)27), (overflow.Code, overflow.Position));
	}

	/// <summary>Double arithmetic of literals and values is computed as the other kinds are: no infinity and no NaN reaches the provider.</summary>
	[Theory]
	[InlineData("from orders where score > 1e308 * 10 select id",       QlErrorCode.Overflow,       32)]
	[InlineData("from orders where score > -1e308 - 1e308 select id",   QlErrorCode.Overflow,       33)]
	[InlineData("from orders where score > 1e0 / 0 select id",          QlErrorCode.DivisionByZero, 30)]
	[InlineData("from orders where score > 0e0 / 0 select id",          QlErrorCode.DivisionByZero, 30)]
	[InlineData("from orders where score > 1e0 % 0 select id",          QlErrorCode.DivisionByZero, 30)]
	[InlineData("from orders where score > @d * 1e308 select id",       QlErrorCode.Overflow,       29)]
	[InlineData("from orders where score > @d / @z select id",          QlErrorCode.DivisionByZero, 29)]
	public void ADoubleThatIsNotFiniteIsTheTextsError(string text, QlErrorCode code, int position)
	{
		var error = Assert.Throws<QlException>(() => Model.QueryData(Shop.InMemory(), text, Rows.Values(new { d = 10.0, z = 0.0 })).ToList()).Error;

		Assert.Equal((code, (int?)position), (error.Code, error.Position));
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void ValuesOfOtherTypesConvertWithoutLoss(string provider)
	{
		Assert.Equal([2, 3], Run(provider, "from orders where id in (@a, @b) order by id select id", new { a = 2L, b = (byte)3 }).First<int>());
		Assert.Equal([1],    Run(provider, "from orders where created < @t select id", new { t = "2026-01-02T12:00:00" }).First<int>());
		Assert.Equal([2],    Run(provider, "from orders where ref == @g select id", new { g = new Guid(2, 0, 0, new byte[8]).ToString("D") }).First<int>());
		Assert.Equal([5],    Run(provider, "from orders where total == @p select id", new { p = 62.5 }).First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void WhatNothingReadsIsNotBuilt(string provider)
	{
		// `salary` may only be filtered on: a value computed from it and never read must not reach the database,
		// where it would be computed for every row (and here divide by zero for none, but by the database's rules).
		string[] texts =
		[
			"from orders let x = 1 / (salary - 1000) order by id select id",
			"from orders let a = qty let x = salary * 2 let b = a + 1 order by id select id, b",
			"from orders select id, salary * 2 as s, qty order by id select id, qty",
			"from orders where qty > 0 select Count(*) as c, Sum(salary) as s select c",
		];

		foreach (var text in texts)
		{
			var query = Model.QueryData(Shop.Of(databases.Get(provider), 1), text);
			var sql   = LinqToDB.LinqExtensions.ToSqlQuery(query).Sql;

			Assert.DoesNotContain("Salary", sql, StringComparison.Ordinal);
			Assert.Contains(Model.Describe(text).Warnings, w => w.Code == QlWarningCode.UnusedName);
			Assert.NotEmpty(query.ToList());
		}

		Assert.Equal(Mine.Select(o => new object?[] { o.Id, o.Qty + 1 }), Run(provider, texts[1]));
		Assert.Equal(Mine.Select(o => new object?[] { o.Id, o.Qty }), Run(provider, texts[2]));
		Assert.Equal([Mine.Count(o => o.Qty > 0)], Run(provider, texts[3]).First<int>());
	}

	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void TheRowsBetweenClausesFlattenIntoOneSelect(string provider)
	{
		var query = Model.QueryData(Shop.Of(databases.Get(provider), 1), "from orders let t = total * 2 let u = qty + id where u > 3 select id, status, t, u where id > 3 select id, t, u order by t desc, id");
		var sql   = LinqToDB.LinqExtensions.ToSqlQuery(query).Sql;

		Assert.Equal(1, sql.Split("SELECT").Length - 1);

		// An integer's average is a double on every database: SQL Server averages integers in integers otherwise.
		var average = LinqToDB.LinqExtensions.ToSqlQuery(Model.QueryData(Shop.Of(databases.Get(provider), 1), "from orders select Avg(qty) as a")).Sql;

		Assert.Contains("AVG(CAST(", average, StringComparison.Ordinal);
	}

	/// <summary>A chain of `let`s that double: as many as the bound on nodes lets through run, and their SQL is as large as they count.</summary>
	[Theory]
	[MemberData(nameof(Databases.Providers), MemberType = typeof(Databases))]
	public void DoublingLetsRunWithinTheBound(string provider)
	{
		var lengths = new List<int>();

		for (var levels = 1; levels <= 4; levels++)
		{
			var text  = BinderTests.Doubling(levels);
			var rows  = Run(provider, text + " order by id");
			var query = Model.QueryData(Shop.Of(databases.Get(provider), 1), text);
			var sql   = LinqToDB.LinqExtensions.ToSqlQuery(query).Sql;

			Assert.Equal(Mine.Select(o => new object?[] { o.Id, o.Qty << levels }), rows);

			// Each level at most doubles what is read: the provider writes no more than the text counts.
			Assert.True(sql.Split("Qty").Length - 1 <= 1 << levels, sql);

			lengths.Add(sql.Length);
		}

		Assert.True(lengths[3] < 4 * lengths[0], string.Join(", ", lengths));
	}

	[Fact]
	public void TheTreeRunsInMemoryToo()
	{
		var rows = Model.QueryData(Shop.InMemory(), "from orders let t = total * 2 where t > 100 && status != null select id, t, status.Length as n order by t desc skip 1 take 2").ToList();

		var expected = Mine.Where(o => o.Total * 2 > 100 && o.Status != null).OrderByDescending(o => o.Total).Skip(1).Take(2).Select(o => new object?[] { o.Id, o.Total * 2, o.Status!.Length });

		Assert.Equal(expected, rows);
	}
}
