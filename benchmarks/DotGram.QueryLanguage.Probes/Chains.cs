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

	static void Case(string what, IQueryable<object?[]> query, int expected, bool stop = true)
	{
		Out.Sub(what);
		Out.Sql(query);

		List<string>? rows = null;

		var error = Out.Try(() => rows = Out.Rows(query));

		if (error != null)
		{
			Out.Line(error);
			Out.Verdict(what, false, error, stop: stop);

			return;
		}

		Out.Show(rows!, 3);
		Out.Verdict(what, rows!.Count == expected, $"{rows.Count} rows, expected {expected}", stop: stop);
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

		// For comparison only (not the agreed shape): the cap after the final projection, so that host paging follows a Take.
		var after = (IQueryable<object?[]>)Chain.Take(Final(Ordered(db)), MaxRows);

		Case("cap after the projection: host Skip(10).Take(5)", after.Skip(10).Take(5), 5, stop: false);
		Case("cap after the projection: host Skip(48).Take(5)", after.Skip(48).Take(5), 2, stop: false);
		Case("cap after the projection: host Skip(MaxRows + 5).Take(10)", after.Skip(MaxRows + 5).Take(10), 0, stop: false);
		Case("cap after the projection: take 2 skip 3 in the text", (IQueryable<object?[]>)Chain.Take(Final(Chain.Skip(Chain.Take(Ordered(db), 2), 3)), MaxRows), 0, stop: false);

		var pagedCapped = Final(Chain.Take(Chain.Skip(Chain.Take(Ordered(db), 2), 3), MaxRows));

		Case("take 2 skip 3 + cap, host Skip(MaxRows + 5).Take(10)", pagedCapped.Skip(MaxRows + 5).Take(10), 0);
		Case("take 2 skip 3 + cap, host Take(1000)", pagedCapped.Take(1000), 0);
	}
}

/// <summary>
/// Probe 1b: a barrier predicate, <c>Where(_ =&gt; barrier.Value)</c> over a <c>StrongBox&lt;bool&gt;</c> holding true, between
/// consecutive take/skip of the text and right after the row cap, before the final projection.
/// </summary>
static class RowCapBarrier
{
	const int MaxRows = 50;

	static IQueryable Ordered(Db db)
	{
		return Chain.OrderBy(db.Orders, r => Expression.Property(r, nameof(Order.Id)));
	}

	/// <summary>A new box per call, as the translator would make one.</summary>
	static IQueryable Barrier(IQueryable source)
	{
		var barrier = new StrongBox<bool>(true);

		return Chain.Where(source, _ => Chain.Value(barrier));
	}

	static IQueryable<object?[]> Final(IQueryable source)
	{
		return Chain.Data(source, r => [Expression.Property(r, nameof(Order.Id))]);
	}

	/// <summary>take @take skip @skip with the barrier between them (<paramref name="between"/>) and after the cap (<paramref name="afterCap"/>).</summary>
	static IQueryable<object?[]> Text(Db db, int take, int skip, bool between, bool afterCap)
	{
		var q = Chain.Take(Ordered(db), new StrongBox<int>(take));

		if (between)
		{
			q = Barrier(q);
		}

		q = Chain.Take(Chain.Skip(q, new StrongBox<int>(skip)), MaxRows);

		if (afterCap)
		{
			q = Barrier(q);
		}

		return Final(q);
	}

	static void Case(string what, IQueryable<object?[]> query, int expected, bool show = true, bool stop = true, int[]? ids = null)
	{
		if (show)
		{
			Out.Sub(what);
			Out.Sql(query);
		}

		List<string>? rows = null;

		var error = Out.Try(() => rows = Out.Rows(query));

		if (error != null)
		{
			Out.Line(error);
			Out.Verdict(what, false, error, stop: stop);

			return;
		}

		if (show)
		{
			Out.Show(rows!, 3);
		}

		var want  = ids?.Select(i => Out.Row([i])).ToList();
		var right = rows!.Count == expected && (want == null || rows.SequenceEqual(want));

		Out.Verdict(what, right, $"{rows.Count} rows [{string.Join(",", rows.Take(6))}], expected {expected}{(want == null ? "" : " [" + string.Join(",", want) + "]")}", stop: stop);
	}

	static double P50(Func<IQueryable<object?[]>> build, int rounds)
	{
		var times = new List<double>();
		var watch = new System.Diagnostics.Stopwatch();

		for (var i = 0; i < rounds; i++)
		{
			watch.Restart();
			Out.Rows(build());
			times.Add(watch.Elapsed.TotalMilliseconds);
		}

		times.Sort();

		return times[times.Count / 2];
	}

	public static void Run()
	{
		Out.Head("Row cap with a barrier predicate (StrongBox<bool> true): between take and skip, and after the cap");

		using var db = Provider.Connect();

		// C1: the text's own take/skip.
		Case("C1 take 2 skip 3 + cap, no barrier (reference)", Text(db, 2, 3, between: false, afterCap: false), 0, stop: false);
		Case("C1 take 2 skip 3 + cap, barrier between", Text(db, 2, 3, between: true, afterCap: false), 0);
		Case("C1 take 2 skip 3 + cap, barrier after the cap only", Text(db, 2, 3, between: false, afterCap: true), 0, stop: false);
		Case("C1 take 2 skip 3 + cap, barrier between and after the cap", Text(db, 2, 3, between: true, afterCap: true), 0);
		Case("C1 take 2 skip 2 + cap, both barriers", Text(db, 2, 2, between: true, afterCap: true), 0);
		Case("take 5 skip 3 + cap, both barriers", Text(db, 5, 3, between: true, afterCap: true), 2);
		Case("take 1000 skip 3 + cap, both barriers", Text(db, 1000, 3, between: true, afterCap: true), MaxRows);
		Case("take 1000 skip 190 + cap, both barriers", Text(db, 1000, 190, between: true, afterCap: true), 10);

		// The order the text asked for must survive the subqueries the barrier makes: order by total desc (ids 200, 199, …),
		// where the first selected column (id) sorts the other way.
		IQueryable ByTotal()
		{
			return Chain.OrderBy(db.Orders, r => Expression.Property(r, nameof(Order.Total)), descending: true);
		}

		Case("order by total desc take 5 skip 3 + cap, no barrier", Final(Chain.Take(Chain.Skip(Chain.Take(ByTotal(), 5), 3), MaxRows)), 2, stop: false, ids: [197, 196]);
		Case("order by total desc take 5 skip 3 + cap, both barriers", Final(Barrier(Chain.Take(Chain.Skip(Barrier(Chain.Take(ByTotal(), 5)), 3), MaxRows))), 2, ids: [197, 196]);

		var byTotal = Final(Barrier(Chain.Take(ByTotal(), MaxRows)));

		Case("order by total desc + cap + barrier, host Skip(10).Take(3)", byTotal.Skip(10).Take(3), 3, ids: [190, 189, 188]);
		Case("order by total desc + cap + barrier, host Skip(48).Take(5)", byTotal.Skip(48).Take(5), 2, ids: [152, 151]);
		Case("order by total desc + cap + barrier, no host operator: output order", byTotal, MaxRows, ids: Enumerable.Range(151, 50).Reverse().ToArray());

		// Variant: after each barrier, the translator repeats the order in force.
		IQueryable ReOrder(IQueryable q)
		{
			return Chain.OrderBy(q, r => Expression.Property(r, nameof(Order.Total)), descending: true);
		}

		Case("re-ordered: order by total desc take 5 skip 3 + cap, both barriers", Final(ReOrder(Barrier(Chain.Take(Chain.Skip(ReOrder(Barrier(Chain.Take(ByTotal(), 5))), 3), MaxRows)))), 2, ids: [197, 196]);
		Case("re-ordered: take 2 skip 3 + cap, both barriers", Final(ReOrder(Barrier(Chain.Take(Chain.Skip(ReOrder(Barrier(Chain.Take(ByTotal(), 2))), 3), MaxRows)))), 0);

		var reordered = Final(ReOrder(Barrier(Chain.Take(ByTotal(), MaxRows))));

		Case("re-ordered: cap + barrier + order, host Skip(10).Take(3)", reordered.Skip(10).Take(3), 3, ids: [190, 189, 188]);
		Case("re-ordered: cap + barrier + order, host Skip(48).Take(5)", reordered.Skip(48).Take(5), 2, ids: [152, 151]);
		Case("re-ordered: cap + barrier + order, host Skip(MaxRows + 5).Take(10)", reordered.Skip(MaxRows + 5).Take(10), 0);
		Case("re-ordered: cap + barrier + order, no host operator", reordered, MaxRows, ids: Enumerable.Range(151, 50).Reverse().ToArray());

		Case("order by total desc + cap, no barrier, host Skip(10).Take(3)", Final(Chain.Take(ByTotal(), MaxRows)).Skip(10).Take(3), 3, stop: false, ids: [190, 189, 188]);

		// C2: the host's paging after the final projection.
		var capped = Final(Barrier(Chain.Take(Ordered(db), MaxRows)));

		Case("C2 host Skip(48).Take(5), barrier after the cap", capped.Skip(48).Take(5), 2);
		Case("C2 host Skip(10).Take(5), barrier after the cap", capped.Skip(10).Take(5), 5);
		Case("C2 host Skip(10), barrier after the cap", capped.Skip(10), MaxRows - 10);
		Case("C2 host Skip(MaxRows + 5).Take(10), barrier after the cap", capped.Skip(MaxRows + 5).Take(10), 0);
		Case("C2 host Take(100), barrier after the cap", capped.Take(100), MaxRows);

		var count = Final(Barrier(Chain.Take(Ordered(db), MaxRows))).Count();

		Out.Verdict("C2 host Count(), barrier after the cap", count == MaxRows, $"{count}", stop: true);

		var cappedJson = Shapes.JsonRows(Barrier(Chain.Take(Ordered(db), MaxRows)), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Id", "Customer"]));

		Out.Sub("C2 over QueryJsonRow: host Skip(48).Take(5)");
		Out.Sql(cappedJson.Skip(48).Take(5));

		var jsonRows = cappedJson.Skip(48).Take(5).ToList();

		Out.Show(jsonRows, 3);
		Out.Verdict("C2 QueryJsonRow host Skip(48).Take(5), barrier after the cap", jsonRows.Count == 2 && jsonRows[0].StartsWith("[49,", StringComparison.Ordinal), $"{jsonRows.Count} rows, expected 2 (49, 50)", stop: true);

		// Cache: one entry for the text shape across values and box instances.
		Out.Sub("cache entries across values (new tree and boxes per call)");

		Out.Clear();

		var before = Out.Misses<object?[]>();
		var lines  = new List<string>();

		foreach (var (take, skip) in new[] { (2, 3), (5, 3), (1000, 3), (3, 3), (1000, 190), (2, 3) })
		{
			var rows = Out.Rows(Text(db, take, skip, between: true, afterCap: true)).Count;

			lines.Add($"take {take} skip {skip} → {rows}");
		}

		var misses = Out.Misses<object?[]>() - before;

		Out.Line(string.Join("; ", lines));
		Out.Verdict("one cache entry across take/skip values and barrier boxes", misses == 1, $"{misses} misses for 6 calls");

		Out.Clear();

		before = Out.Misses<object?[]>();

		for (var i = 0; i < 5; i++)
		{
			Out.Rows(Final(Barrier(Chain.Take(Ordered(db), MaxRows))).Skip(i * 3).Take(5));
		}

		misses = Out.Misses<object?[]>() - before;

		Out.Verdict("C2: one cache entry across host Skip values", misses == 1, $"{misses} misses for 5 calls");

		// Cost: the same capped query with and without the barrier after the cap, and the text with and without both.
		Out.Sub("cost (warm p50 of 200 runs, new tree per run)");

		P50(() => Final(Chain.Take(Ordered(db), MaxRows)), 50);
		P50(() => Final(Barrier(Chain.Take(Ordered(db), MaxRows))), 50);

		var plain   = P50(() => Final(Chain.Take(Ordered(db), MaxRows)), 200);
		var guarded = P50(() => Final(Barrier(Chain.Take(Ordered(db), MaxRows))), 200);
		var text0   = P50(() => Text(db, 1000, 3, between: false, afterCap: false), 200);
		var text2   = P50(() => Text(db, 1000, 3, between: true, afterCap: true), 200);

		Out.Line($"cap only {plain:F3} ms, cap + barrier {guarded:F3} ms; take 1000 skip 3 + cap {text0:F3} ms, with both barriers {text2:F3} ms");
		Out.Cost("barrier", $"cap: ×{guarded / plain:F2}; take/skip text: ×{text2 / text0:F2} (directional, {Out.TableRows} rows, {Provider.Name})");
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
