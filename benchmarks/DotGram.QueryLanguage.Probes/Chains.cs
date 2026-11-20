using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using DotGram.QueryLanguage.Probes.Core;

namespace DotGram.QueryLanguage.Probes;

/// <summary>Probe 1: can a text's own paging lift the row cap the library appends?</summary>
static class RowCap
{
	const int MaxRows = 50;

	static IQueryable Ordered(Db db)
	{
		return Chain.OrderBy(db.Orders, r => Expression.Property(r, nameof(Order.Id)));
	}

	static IQueryable<object?[]> Final(IQueryable source)
	{
		return Chain.Data(source, r => [Expression.Property(r, nameof(Order.Id))]);
	}

	static void Case(string what, IQueryable<object?[]> query, int expected)
	{
		Out.Sub(what);
		Out.Sql(query);

		List<string>? rows = null;

		var error = Out.Try(() => rows = Out.Rows(query));

		if (error != null)
		{
			Out.Line(error);
			Out.Verdict(what, false, error, stop: true);

			return;
		}

		Out.Show(rows!, 3);
		Out.Verdict(what, rows!.Count == expected, $"{rows.Count} rows, expected {expected}", stop: true);
	}

	public static void Run()
	{
		Out.Head($"Row cap: Take({MaxRows}) last, against {Out.TableRows} rows");

		using var db = Provider.Connect();

		var two   = new StrongBox<int>(2);
		var three = new StrongBox<int>(3);

		Case("take 2 skip 3 + Take(MaxRows)", Final(Chain.Take(Chain.Skip(Chain.Take(Ordered(db), 2), 3), MaxRows)), 0);
		Case("take 2 skip 3 alone (no cap)", Final(Chain.Skip(Chain.Take(Ordered(db), 2), 3)), 0);
		Case("take @2 skip @3 + Take(MaxRows) (values)", Final(Chain.Take(Chain.Skip(Chain.Take(Ordered(db), two), three), MaxRows)), 0);
		Case("take 2 skip 3 without order by + Take(MaxRows)", Final(Chain.Take(Chain.Skip(Chain.Take(db.Orders, 2), 3), MaxRows)), 0);
		Case("take 2 where id > 0 skip 3 + Take(MaxRows)", Final(Chain.Take(Chain.Skip(Chain.Where(Chain.Take(Ordered(db), 2), r => Expression.GreaterThan(Expression.Property(r, nameof(Order.Id)), Expression.Constant(0))), 3), MaxRows)), 0);
		Case("take 2 order by id desc skip 3 + Take(MaxRows)", Final(Chain.Take(Chain.Skip(Chain.OrderBy(Chain.Take(Ordered(db), 2), r => Expression.Property(r, nameof(Order.Id)), descending: true), 3), MaxRows)), 0);
		Case("take 2 skip 2 + Take(MaxRows)", Final(Chain.Take(Chain.Skip(Chain.Take(Ordered(db), 2), 2), MaxRows)), 0);
		Case("take 5 skip 3 + Take(MaxRows)", Final(Chain.Take(Chain.Skip(Chain.Take(Ordered(db), 5), 3), MaxRows)), 2);
		Case("skip 3 take 1000 + Take(MaxRows)", Final(Chain.Take(Chain.Take(Chain.Skip(Ordered(db), 3), 1000), MaxRows)), MaxRows);
		Case("take 1000 + Take(MaxRows)", Final(Chain.Take(Chain.Take(Ordered(db), 1000), MaxRows)), MaxRows);

		var capped = Final(Chain.Take(Ordered(db), MaxRows));

		Case("host Skip(MaxRows + 5).Take(10) after the cap", capped.Skip(MaxRows + 5).Take(10), 0);
		Case("host Take(MaxRows * 2) after the cap", capped.Take(MaxRows * 2), MaxRows);
		Case("host Skip(10) after the cap", capped.Skip(10), MaxRows - 10);

		// The same chain written in C#, to rule out the probe's own tree building.
		Case("C#: OrderBy(Id).Take(2).Skip(3).Take(MaxRows)", db.Orders.OrderBy(o => o.Id).Take(2).Skip(3).Take(MaxRows).Select(o => new object?[] { o.Id }), 0);
		Case("C#: OrderBy(Id).Take(MaxRows).Skip(MaxRows + 5).Take(10)", db.Orders.OrderBy(o => o.Id).Take(MaxRows).Skip(MaxRows + 5).Take(10).Select(o => new object?[] { o.Id }), 0);
		Case("C#: OrderBy(Id).Take(MaxRows).Skip(10)", db.Orders.OrderBy(o => o.Id).Take(MaxRows).Skip(10).Select(o => new object?[] { o.Id }), MaxRows - 10);

		// A projection between the cap and the host's paging: the library's final Select sits exactly there.
		Case("C#: Take(MaxRows).Select(new object[]).Skip(10).Take(5)", db.Orders.OrderBy(o => o.Id).Take(MaxRows).Select(o => new object?[] { o.Id }).Skip(10).Take(5), 5);
		Case("C#: Take(MaxRows).Select(o => new { o.Id }).Skip(48).Take(5)", db.Orders.OrderBy(o => o.Id).Take(MaxRows).Select(o => new { o.Id }).Skip(48).Take(5).Select(x => new object?[] { x.Id }), 2);

		var pagedCapped = Final(Chain.Take(Chain.Skip(Chain.Take(Ordered(db), 2), 3), MaxRows));

		Case("take 2 skip 3 + cap, host Skip(MaxRows + 5).Take(10)", pagedCapped.Skip(MaxRows + 5).Take(10), 0);
		Case("take 2 skip 3 + cap, host Take(1000)", pagedCapped.Take(1000), 0);
	}
}

/// <summary>Probe 2: Select → Where → Select in the bounded carriers and in object[] rows.</summary>
static class Chains
{
	static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	static Expression Slot(Expression row, Type type, int index)
	{
		return Carriers.Read(row, Carriers.Slot(type, index));
	}

	static Expression Item(Expression row, int index, Type type)
	{
		return Expression.Convert(Expression.ArrayIndex(row, Expression.Constant(index)), type);
	}

	static Expression Boxed(Expression value)
	{
		return Expression.Convert(value, typeof(object));
	}

	static IQueryable Policy(Db db)
	{
		return db.Orders.Where(o => o.TenantId == db.TenantId);
	}

	static void Compare(string scheme, string what, IQueryable<object?[]> query, List<string> expected)
	{
		Out.Sub($"{what} — {scheme}");

		string sql;

		try
		{
			sql = Out.SqlText(query);
		}
		catch (Exception e)
		{
			Out.Line(Out.Error(e));
			Out.Verdict($"{what}: {scheme} translates", false, Out.Error(e), stop: false);

			return;
		}

		Out.Line("```sql");
		Out.Line(sql.TrimEnd());
		Out.Line("```");

		List<string>? rows = null;

		var error = Out.Try(() => rows = Out.Rows(query));

		if (error != null)
		{
			Out.Line(error);
			Out.Verdict($"{what}: {scheme} translates", false, error, stop: false);

			return;
		}

		Out.Show(rows!, 4);

		var same = rows!.SequenceEqual(expected);

		Out.Verdict($"{what}: {scheme}", same, $"{rows!.Count} rows ({(same ? "equal to" : "DIFFERENT from")} LINQ to Objects, {expected.Count}); SELECTs in SQL: {Out.Selects(sql)}");
	}

	public static void Run()
	{
		Out.Head("Chains: carriers (QlScalars, QlRow<TE>) and object[] rows");

		using var db = Provider.Connect(1);

		// An int value: a decimal value against a computed decimal is a separate SQLite finding (probe 4).
		var limit = new StrongBox<int>(20);

		// from orders let t = total * 2 let u = qty + id where u > @limit select id, status, t, u where id > 3 select id, t, u
		// order by t desc, id  (+ Take(50))
		var expectedA = Out.Data.Where(o => o.TenantId == 1)
			.Select(o => new { o, t = o.Total * 2 })
			.Select(x => new { x.o, x.t, u = x.o.Qty + x.o.Id })
			.Where(x => x.u > 20)
			.Select(x => new { x.o.Id, x.o.Status, x.t, x.u })
			.Where(x => x.Id > 3)
			.Select(x => new { x.Id, x.t, x.u })
			.OrderByDescending(x => x.t).ThenBy(x => x.Id)
			.Take(50)
			.Select(x => Out.Row([x.Id, x.t, x.u]))
			.ToList();

		{
			var q = Chain.Select(Policy(db), o => Carriers.NewRow(o, Carriers.NewScalars([("Decimal0", Expression.Multiply(P(o, "Total"), Expression.Constant(2m)))])));

			q = Chain.Select(q, r => Carriers.NewRow(Carriers.Entity(r), Carriers.NewScalars(
			[
				("Decimal0", Slot(r, typeof(decimal), 0)),
				("Int0",     Expression.Add(P(Carriers.Entity(r), "Qty"), P(Carriers.Entity(r), "Id"))),
			])));
			q = Chain.Where(q, r => Expression.GreaterThan(Slot(r, typeof(int), 0), Chain.Value(limit)));
			q = Chain.Select(q, r => Carriers.NewScalars(
			[
				("Int0",     P(Carriers.Entity(r), "Id")),
				("String0",  P(Carriers.Entity(r), "Status")),
				("Decimal0", Slot(r, typeof(decimal), 0)),
				("Int1",     Slot(r, typeof(int), 0)),
			]));
			q = Chain.Where(q, r => Expression.GreaterThan(Slot(r, typeof(int), 0), Expression.Constant(3)));
			q = Chain.Select(q, r => Carriers.NewScalars(
			[
				("Int0",     Slot(r, typeof(int), 0)),
				("Decimal0", Slot(r, typeof(decimal), 0)),
				("Int1",     Slot(r, typeof(int), 1)),
			]));
			q = Chain.OrderBy(q, r => Slot(r, typeof(decimal), 0), descending: true);
			q = Chain.ThenBy(q, r => Slot(r, typeof(int), 0));
			q = Chain.Take(q, 50);

			Compare("carriers", "let, let, where, select, where, select, order by", Chain.Data(q, r => [Slot(r, typeof(int), 0), Slot(r, typeof(decimal), 0), Slot(r, typeof(int), 1)]), expectedA);
		}

		{
			var q = Chain.Select(Policy(db), o => Expression.NewArrayInit(typeof(object), o, Boxed(Expression.Multiply(P(o, "Total"), Expression.Constant(2m)))));

			q = Chain.Select(q, r => Expression.NewArrayInit(typeof(object),
				Expression.ArrayIndex(r, Expression.Constant(0)),
				Expression.ArrayIndex(r, Expression.Constant(1)),
				Boxed(Expression.Add(P(Item(r, 0, typeof(Order)), "Qty"), P(Item(r, 0, typeof(Order)), "Id")))));
			q = Chain.Where(q, r => Expression.GreaterThan(Item(r, 2, typeof(int)), Chain.Value(limit)));
			q = Chain.Select(q, r => Expression.NewArrayInit(typeof(object),
				Boxed(P(Item(r, 0, typeof(Order)), "Id")),
				P(Item(r, 0, typeof(Order)), "Status"),
				Expression.ArrayIndex(r, Expression.Constant(1)),
				Expression.ArrayIndex(r, Expression.Constant(2))));
			q = Chain.Where(q, r => Expression.GreaterThan(Item(r, 0, typeof(int)), Expression.Constant(3)));
			q = Chain.Select(q, r => Expression.NewArrayInit(typeof(object),
				Expression.ArrayIndex(r, Expression.Constant(0)),
				Expression.ArrayIndex(r, Expression.Constant(2)),
				Expression.ArrayIndex(r, Expression.Constant(3))));
			q = Chain.OrderBy(q, r => Item(r, 1, typeof(decimal)), descending: true);
			q = Chain.ThenBy(q, r => Item(r, 0, typeof(int)));
			q = Chain.Take(q, 50);

			Compare("object[] rows", "let, let, where, select, where, select, order by", Chain.Data(q, r => [Item(r, 0, typeof(int)), Item(r, 1, typeof(decimal)), Item(r, 2, typeof(int))]), expectedA);
		}

		// select a0 = id, a1 = id + 1, …, a11 = qty + 11 where a0 + a11 > 30 && a9 > 15 order by a10 desc, a11, a9 (+ Take(50))
		const int width = 12;

		Expression Column(Expression o, int i)
		{
			return i % 2 == 0
				? Expression.Add(P(o, "Id"), Expression.Constant(i))
				: Expression.Add(P(o, "Qty"), Expression.Constant(i));
		}

		var expectedB = Out.Data.Where(o => o.TenantId == 1)
			.Select(o => Enumerable.Range(0, width).Select(i => i % 2 == 0 ? o.Id + i : o.Qty + i).ToArray())
			.Where(a => a[0] + a[11] > 30 && a[9] > 15)
			.OrderByDescending(a => a[10]).ThenBy(a => a[11]).ThenBy(a => a[9])
			.Take(50)
			.Select(a => Out.Row(a.Cast<object?>().ToArray()))
			.ToList();

		{
			var q = Chain.Select(Policy(db), o => Carriers.NewScalars(Enumerable.Range(0, width).Select(i => ("Int" + i, Column(o, i)))));

			q = Chain.Where(q, r => Expression.AndAlso(
				Expression.GreaterThan(Expression.Add(Slot(r, typeof(int), 0), Slot(r, typeof(int), 11)), Expression.Constant(30)),
				Expression.GreaterThan(Slot(r, typeof(int), 9), Expression.Constant(15))));
			q = Chain.OrderBy(q, r => Slot(r, typeof(int), 10), descending: true);
			q = Chain.ThenBy(q, r => Slot(r, typeof(int), 11));
			q = Chain.ThenBy(q, r => Slot(r, typeof(int), 9));
			q = Chain.Take(q, 50);

			Compare("carriers", "12 int columns in where and order by", Chain.Data(q, r => Enumerable.Range(0, width).Select(i => Slot(r, typeof(int), i))), expectedB);
		}

		{
			var q = Chain.Select(Policy(db), o => Expression.NewArrayInit(typeof(object), Enumerable.Range(0, width).Select(i => Boxed(Column(o, i)))));

			q = Chain.Where(q, r => Expression.AndAlso(
				Expression.GreaterThan(Expression.Add(Item(r, 0, typeof(int)), Item(r, 11, typeof(int))), Expression.Constant(30)),
				Expression.GreaterThan(Item(r, 9, typeof(int)), Expression.Constant(15))));
			q = Chain.OrderBy(q, r => Item(r, 10, typeof(int)), descending: true);
			q = Chain.ThenBy(q, r => Item(r, 11, typeof(int)));
			q = Chain.ThenBy(q, r => Item(r, 9, typeof(int)));
			q = Chain.Take(q, 50);

			Compare("object[] rows", "12 int columns in where and order by", Chain.Data(q, r => Enumerable.Range(0, width).Select(i => Item(r, i, typeof(int)))), expectedB);
		}

		// Columns of a type outside the nine (DateOnly, an enum stored as text) after a select:
		// select d = day, k = kind, id where d == @day order by k, id (+ Take(50))
		var day = new StrongBox<DateOnly>(new DateOnly(2026, 1, 8));

		var expectedC = Out.Data.Where(o => o.TenantId == 1 && o.Day == day.Value)
			.OrderBy(o => o.Kind).ThenBy(o => o.Id)   // stored as 'new' < 'paid' < 'shipped': the same order
			.Take(50)
			.Select(o => Out.Row([o.Day, o.Kind, o.Id]))
			.ToList();

		{
			var q = Chain.Select(Policy(db), o => Carriers.NewScalars([("Other0", P(o, "Day")), ("Other1", P(o, "Kind")), ("Int0", P(o, "Id"))]));

			q = Chain.Where(q, r => Expression.Equal(Expression.Convert(Slot(r, typeof(DateOnly), 0), typeof(DateOnly)), Chain.Value(day)));
			q = Chain.OrderBy(q, r => Expression.Convert(Slot(r, typeof(Kind), 1), typeof(Kind)));
			q = Chain.ThenBy(q, r => Slot(r, typeof(int), 0));
			q = Chain.Take(q, 50);

			Compare("carriers, object slots", "DateOnly and enum columns after select", Chain.Data(q, r => [Slot(r, typeof(DateOnly), 0), Slot(r, typeof(Kind), 1), Slot(r, typeof(int), 0)]), expectedC);
		}

		{
			var q = Chain.Select(Policy(db), o => Expression.NewArrayInit(typeof(object), Boxed(P(o, "Day")), Boxed(P(o, "Kind")), Boxed(P(o, "Id"))));

			q = Chain.Where(q, r => Expression.Equal(Item(r, 0, typeof(DateOnly)), Chain.Value(day)));
			q = Chain.OrderBy(q, r => Item(r, 1, typeof(Kind)));
			q = Chain.ThenBy(q, r => Item(r, 2, typeof(int)));
			q = Chain.Take(q, 50);

			Compare("object[] rows", "DateOnly and enum columns after select", Chain.Data(q, r => [Expression.ArrayIndex(r, Expression.Constant(0)), Expression.ArrayIndex(r, Expression.Constant(1)), Expression.ArrayIndex(r, Expression.Constant(2))]), expectedC);
		}
	}
}
