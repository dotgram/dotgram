using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using DotGram.QueryLanguage.Probes.Core;

namespace DotGram.QueryLanguage.Probes;

/// <summary>Probe 12: the host's tenant policy under linq2db's query cache.</summary>
static class Tenants
{
	/// <summary>A policy form: the source the host registers, called with the context on every query.</summary>
	sealed record Form(string Name, Func<Db, IQueryable<Order>> Source);

	static readonly Form[] Forms =
	[
		new("context property (db => db.Orders.Where(o => o.TenantId == db.TenantId))", db => db.Orders.Where(o => o.TenantId == db.TenantId)),
		new("captured local (var t = db.TenantId; … == t)", db =>
		{
			var t = db.TenantId;

			return db.Orders.Where(o => o.TenantId == t);
		}),
		new("host StrongBox as a tree constant (Field(Constant(box), Value))", db =>
		{
			var box = new StrongBox<int>(db.TenantId);
			var o   = Expression.Parameter(typeof(Order), "o");

			return db.Orders.Where(Expression.Lambda<Func<Order, bool>>(Expression.Equal(Expression.Property(o, nameof(Order.TenantId)), Chain.Value(box)), o));
		}),
		new("box held by the context (… == db.TenantBox.Value)", db => db.Orders.Where(o => o.TenantId == db.TenantBox.Value)),
		new("context that is not the connection (c => c.Db.Orders.Where(o => o.TenantId == c.TenantId))", db =>
		{
			var context = new AppContext(db, db.TenantId);

			return Source(context);
		}),
	];

	static IQueryable<Order> Source(AppContext c)
	{
		Func<AppContext, IQueryable<Order>> source = c => c.Db.Orders.Where(o => o.TenantId == c.TenantId);

		return source(c);
	}

	static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	/// <summary>One text: where status != null select id, tenant, total where total > @min order by id (+ cap), both outputs.</summary>
	static (IQueryable<object?[]> Data, IQueryable<string> Json) Text(IQueryable<Order> source)
	{
		var min = new StrongBox<decimal>(10m);

		var q = Chain.Where(source, o => Expression.NotEqual(P(o, "Status"), Expression.Constant(null, typeof(string))));

		q = Chain.Select(q, o => Carriers.NewScalars([("Int0", P(o, "Id")), ("Int1", P(o, "TenantId")), ("Decimal0", P(o, "Total"))]));
		q = Chain.Where(q, r => Expression.GreaterThan(Carriers.Read(r, "Decimal0"), Chain.Value(min)));
		q = Chain.OrderBy(q, r => Carriers.Read(r, "Int0"));
		q = Chain.Take(q, 1000);

		var data = Chain.Data(q, r => [Carriers.Read(r, "Int0"), Carriers.Read(r, "Int1")]);
		var json = (IQueryable<string>)Chain.Select(q, r => Json.Row(Carriers.Read(r, "Int0"), [("id", Carriers.Read(r, "Int0")), ("tenant", Carriers.Read(r, "Int1"))], asObject: false));

		return (data, json);
	}

	/// <summary>Rows of another tenant in one execution: by the tenant column and by the id's parity (odd ids are tenant 1).</summary>
	static int Foreign(int tenant, IEnumerable<object?[]> rows, out int count)
	{
		var list = rows.ToList();

		count = list.Count;

		return list.Count(r => (int)r[1]! != tenant || ((int)r[0]! % 2 == 1 ? 1 : 2) != tenant);
	}

	static int Foreign(int tenant, IEnumerable<string> rows, out int count)
	{
		var list = rows.Select(Shapes.Elements).ToList();

		count = list.Count;

		return list.Count(r => int.Parse(r[1]) != tenant || (int.Parse(r[0]) % 2 == 1 ? 1 : 2) != tenant);
	}

	public static void Run()
	{
		Out.Head("Tenant policy under the cache: two contexts, one text, interleaved and parallel");

		var expected = new Dictionary<int, int>
		{
			[1] = Out.Data.Count(o => o.TenantId == 1 && o.Status != null && o.Total > 10m),
			[2] = Out.Data.Count(o => o.TenantId == 2 && o.Status != null && o.Total > 10m),
		};

		Out.Line($"expected rows: tenant 1 — {expected[1]}, tenant 2 — {expected[2]}");

		foreach (var form in Forms)
		{
			Out.Sub(form.Name);

			Out.Clear();

			var dataBefore = Out.Misses<object?[]>();
			var jsonBefore = Out.Misses<string>();
			var foreign    = 0;
			var wrongCount = 0;
			var runs       = 0;

			using (var a = Provider.Connect(1))
			using (var b = Provider.Connect(2))
			{
				Out.Sql(Text(form.Source(a)).Data);

				for (var i = 0; i < 25; i++)
				{
					foreach (var db in new[] { a, b, b, a })
					{
						var (data, json) = Text(form.Source(db));

						foreign += Foreign(db.TenantId, data, out var n1);
						foreign += Foreign(db.TenantId, json, out var n2);

						wrongCount += (n1 != expected[db.TenantId] ? 1 : 0) + (n2 != expected[db.TenantId] ? 1 : 0);
						runs       += 2;
					}
				}
			}

			var interleaved = (Data: Out.Misses<object?[]>() - dataBefore, Json: Out.Misses<string>() - jsonBefore);

			var watch = Stopwatch.StartNew();

			Parallel.For(0, 16, new ParallelOptions { MaxDegreeOfParallelism = 8 }, t =>
			{
				var tenant = t % 2 + 1;

				using var db = Provider.Connect(tenant);

				for (var i = 0; i < 50; i++)
				{
					var (data, json) = Text(form.Source(db));

					Interlocked.Add(ref foreign, Foreign(tenant, data, out var n1) + Foreign(tenant, json, out var n2));
					Interlocked.Add(ref wrongCount, (n1 != expected[tenant] ? 1 : 0) + (n2 != expected[tenant] ? 1 : 0));
					Interlocked.Add(ref runs, 2);
				}
			});

			var misses = (Data: Out.Misses<object?[]>() - dataBefore, Json: Out.Misses<string>() - jsonBefore);

			Out.Line($"executions {runs} (interleaved 200, parallel 16×50×2 in {watch.ElapsedMilliseconds} ms); rows of another tenant: {foreign}; executions with a wrong row count: {wrongCount}");
			Out.Line($"cache misses: QueryData {interleaved.Data}, QueryJsonRow {interleaved.Json} after interleaved; {misses.Data}, {misses.Json} after parallel");

			Out.Verdict($"{form.Name}: no row of another tenant", foreign == 0 && wrongCount == 0, $"{foreign} foreign rows, {wrongCount} wrong counts in {runs} executions", stop: true);
			Out.Verdict($"{form.Name}: one cache entry per output", misses.Data == 1 && misses.Json == 1, $"misses {misses.Data} / {misses.Json}");
		}
	}
}

/// <summary>Probe 10: what nested page levels cost.</summary>
static class Paging
{
	const int Rows = 20000;

	static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	/// <summary><paramref name="levels"/> × (skip 1 take 100000 where id &gt; i order by id, alternating direction), then the cap.</summary>
	static IQueryable<object?[]> Paged(Db db, int levels, bool paging)
	{
		var q = (IQueryable)db.Orders;

		for (var i = 0; i < levels; i++)
		{
			var bound = i;

			if (paging)
			{
				q = Chain.Skip(q, 1);
				q = Chain.Take(q, 100000);
			}

			q = Chain.Where(q, o => Expression.GreaterThan(P(o, "Id"), Expression.Constant(bound)));
			q = Chain.OrderBy(q, o => P(o, "Id"), descending: i % 2 == 0);
		}

		return Chain.Data(Chain.Take(q, 50), o => [P(o, "Id")]);
	}

	static List<string> Expected(int levels, bool paging)
	{
		IEnumerable<Order> q = Provider.Generate(Rows);

		for (var i = 0; i < levels; i++)
		{
			var bound = i;

			if (paging)
			{
				q = q.Skip(1).Take(100000);
			}

			q = q.Where(o => o.Id > bound);
			q = i % 2 == 0 ? q.OrderByDescending(o => o.Id) : q.OrderBy(o => o.Id);
		}

		return q.Take(50).Select(o => Out.Row([o.Id])).ToList();
	}

	/// <summary>The cheapest level to write: take 100000 order by id (alternating direction); translation alone, then execution.</summary>
	static void Minimal(Db db)
	{
		Out.Line("");
		Out.Line("Minimal level `take 100000 order by id` (direction alternating), then the cap. Translation = ToSqlQuery on a cleared cache, p50 of 3.");
		Out.Line("");
		Out.Line("| levels | SQL chars | SELECTs | translation p50, ms | warm run p50, ms | right rows |");
		Out.Line("|---:|---:|---:|---:|---:|---|");

		IQueryable<object?[]> Build(int levels)
		{
			var q = (IQueryable)db.Orders;

			for (var i = 0; i < levels; i++)
			{
				q = Chain.Take(q, 100000);
				q = Chain.OrderBy(q, o => P(o, "Id"), descending: i % 2 == 0);
			}

			return Chain.Data(Chain.Take(q, 50), o => [P(o, "Id")]);
		}

		foreach (var levels in new[] { 8, 16, 32, 48, 64 })
		{
			var translate = new List<double>();
			var sql       = "";
			var watch     = new Stopwatch();

			try
			{
				for (var i = 0; i < 3; i++)
				{
					Out.Clear();
					watch.Restart();
					sql = Out.SqlText(Build(levels));
					translate.Add(watch.Elapsed.TotalMilliseconds);
				}
			}
			catch (Exception e)
			{
				Out.Line($"| {levels} | — | — | {Out.Error(e)} | — | — |");

				continue;
			}

			var rows  = Out.Rows(Build(levels));
			var times = new List<double>();

			for (var i = 0; i < 5; i++)
			{
				watch.Restart();
				Out.Rows(Build(levels));
				times.Add(watch.Elapsed.TotalMilliseconds);
			}

			translate.Sort();
			times.Sort();

			IEnumerable<Order> expected = Provider.Generate(Rows);

			for (var i = 0; i < levels; i++)
			{
				expected = i % 2 == 0 ? expected.Take(100000).OrderByDescending(o => o.Id) : expected.Take(100000).OrderBy(o => o.Id);
			}

			var right = rows.SequenceEqual(expected.Take(50).Select(o => Out.Row([o.Id])));

			Out.Line($"| {levels} | {sql.Length} | {Out.Selects(sql)} | {translate[1]:F1} | {times[2]:F1} | {(right ? "yes" : "NO")} |");
		}
	}

	public static void Run()
	{
		Out.Head($"Paging cost: nested page levels over {Rows} rows");

		Provider.Setup(Provider.Name, Rows);

		using var db = Provider.Connect(1);

		Out.Line("| levels | paging | SQL chars | SELECTs | first run (translate + execute), ms | warm run p50, ms | right rows |");
		Out.Line("|---:|---|---:|---:|---:|---:|---|");

		var warm = new Dictionary<(int, bool), double>();

		foreach (var paging in new[] { true, false })
		{
			foreach (var levels in new[] { 1, 2, 4, 8, 16, 32 })
			{
				var query = Paged(db, levels, paging);

				Out.Clear();

				string sql;
				double first;
				List<string> rows;

				var watch = Stopwatch.StartNew();

				try
				{
					rows  = Out.Rows(query);
					first = watch.Elapsed.TotalMilliseconds;
					sql   = Out.SqlText(query);
				}
				catch (Exception e)
				{
					Out.Line($"| {levels} | {paging} | — | — | {Out.Error(e)} | — | — |");

					continue;
				}

				var times = new List<double>();

				for (var i = 0; i < 7; i++)
				{
					watch.Restart();
					Out.Rows(Paged(db, levels, paging));
					times.Add(watch.Elapsed.TotalMilliseconds);
				}

				times.Sort();
				warm[(levels, paging)] = times[times.Count / 2];

				var right = rows.SequenceEqual(Expected(levels, paging));

				Out.Line($"| {levels} | {(paging ? "skip/take + where + order by" : "where + order by")} | {sql.Length} | {Out.Selects(sql)} | {first:F1} | {times[times.Count / 2]:F1} | {(right ? "yes" : "NO")} |");

				if (levels == 4 && paging)
				{
					Out.Line("");
					Out.Line("```sql");
					Out.Line(sql.TrimEnd());
					Out.Line("```");
					Out.Line("");
				}
			}
		}

		Minimal(db);

		if (warm.TryGetValue((1, true), out var one) && warm.TryGetValue((32, true), out var many))
		{
			Out.Cost("32 page levels against 1", $"warm {many:F1} ms vs {one:F1} ms (×{many / one:F1}) over {Rows} rows; without paging 32 levels {warm.GetValueOrDefault((32, false)):F1} ms");
		}
	}
}
