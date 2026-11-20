using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using DotGram.QueryLanguage.Probes.Core;

namespace DotGram.QueryLanguage.Probes;

/// <summary>Probe 3: whole-set aggregates, GroupBy(r => 0), on empty and non-empty input.</summary>
static class Aggregates
{
	static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	static Expression N(Expression value)
	{
		return Expression.Convert(value, typeof(Nullable<>).MakeGenericType(value.Type));
	}

	static List<Expression> Columns(ParameterExpression g, bool scalars)
	{
		Expression Field(ParameterExpression x, string member)
		{
			if (!scalars)
			{
				return P(x, member);
			}

			// Where the carrier select below put each member.
			var slot = member switch
			{
				"Total"    => "Decimal0",
				"Priority" => "IntN1",
				"Score"    => "Double0",
				"Customer" => "String0",
				"Created"  => "Timestamp0",
				_          => "String1",
			};

			return Carriers.Read(x, slot);
		}

		return
		[
			Chain.Aggregate(g, "Count"),
			Chain.Aggregate(g, "Count", x => Expression.NotEqual(Field(x, "Status"), Expression.Constant(null, typeof(string)))),
			Chain.Aggregate(g, "Sum", x => N(Field(x, "Total"))),
			Chain.Aggregate(g, "Average", x => Field(x, "Priority")),
			Chain.Aggregate(g, "Average", x => N(Field(x, "Score"))),
			Chain.Aggregate(g, "Min", x => Field(x, "Customer")),
			Chain.Aggregate(g, "Max", x => N(Field(x, "Created"))),
		];
	}

	static string Expected(IEnumerable<Order> rows)
	{
		var list = rows.ToList();

		return Out.Row(
		[
			list.Count,
			list.Count(o => o.Status != null),
			list.Count == 0 ? null : list.Sum(o => (decimal?)o.Total),   // SQL: SUM of no rows is NULL, as the language says
			list.Average(o => o.Priority),
			list.Average(o => (double?)o.Score),
			list.Min(o => o.Customer),
			list.Max(o => (DateTime?)o.Created),
		]);
	}

	static void Case(string what, IQueryable<object?[]> query, string expected)
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

		Out.Show(rows!);
		Out.Line("expected: " + expected);

		Out.Verdict(what + ": one row", rows!.Count == 1, $"{rows.Count} rows", stop: true);

		if (rows.Count == 1)
		{
			Out.Verdict(what + ": values", rows[0] == expected, rows[0] == expected ? "equal to LINQ to Objects" : $"got {rows[0]}");
		}
	}

	static void JsonCase(string what, IQueryable<string> query, int expected)
	{
		Out.Sub(what);
		Out.Sql(query);

		List<string>? rows = null;

		var error = Out.Try(() => rows = query.ToList());

		if (error != null)
		{
			Out.Line(error);
			Out.Verdict(what, false, error, stop: true);

			return;
		}

		Out.Show(rows!);
		Out.Verdict(what + ": one row", rows!.Count == expected, $"{rows.Count} rows", stop: true);
	}

	public static void Run()
	{
		Out.Head("Aggregates: GroupBy(r => 0) on empty and non-empty input");

		using var db = Provider.Connect(1);

		var some  = new StrongBox<int>(1);
		var none  = new StrongBox<int>(99);

		foreach (var (label, tenant) in new[] { ("non-empty", some), ("empty", none) })
		{
			var expected = Expected(Out.Data.Where(o => o.TenantId == tenant.Value));
			var source   = Chain.Where(db.Orders, o => Expression.Equal(P(o, "TenantId"), Chain.Value(tenant)));

			// Straight into the final projection.
			Case($"{label}: aggregates into object?[]", (IQueryable<object?[]>)Chain.Select(Chain.GroupAll(source), g => Expression.NewArrayInit(typeof(object), Columns(g, false).Select(c => Expression.Convert(c, typeof(object))))), expected);

			// After a carrier select, into a carrier, then the final projection (with the cap).
			var scalars = Chain.Select(source, o => Carriers.NewScalars(
			[
				("Decimal0", P(o, "Total")), ("IntN1", P(o, "Priority")), ("Double0", P(o, "Score")), ("String0", P(o, "Customer")),
				("Timestamp0", P(o, "Created")), ("String1", P(o, "Status")),
			]));

			var grouped = Chain.Select(Chain.GroupAll(scalars), g =>
			{
				var c = Columns(g, true);

				return Carriers.NewScalars(
				[
					("Int0", c[0]), ("Int1", c[1]), ("DecimalN0", c[2]), ("DoubleN0", c[3]), ("DoubleN1", c[4]), ("String0", c[5]), ("TimestampN0", c[6]),
				]);
			});

			Case($"{label}: carriers → aggregates into a carrier → Take → object?[]", Chain.Data(Chain.Take(grouped, 50), r => new[] { "Int0", "Int1", "DecimalN0", "DoubleN0", "DoubleN1", "String0", "TimestampN0" }.Select(s => Carriers.Read(r, s))), expected);

			// Into the JSON chain, anchored on Count.
			var json = (IQueryable<string>)Chain.Select(Chain.GroupAll(source), g =>
			{
				var c = Columns(g, false);

				return Json.Row(c[0], c.Select((e, i) => ("c" + i, e)).ToList(), asObject: false);
			});

			JsonCase($"{label}: aggregates into the JSON chain", json, 1);
		}

		// Avg over an int column whose mean is not whole (priority 1, 2 or NULL): SQL Server's AVG(int) is integer division.
		{
			var tenant = new StrongBox<int>(1);
			var query  = (IQueryable<object?[]>)Chain.Select(Chain.GroupAll(Chain.Where(db.Orders, o => Expression.Equal(P(o, "TenantId"), Chain.Value(tenant)))),
				g => Expression.NewArrayInit(typeof(object), Expression.Convert(Chain.Aggregate(g, "Average", x => P(x, "Priority")), typeof(object))));

			Out.Sub("Avg(int) precision");
			Out.Sql(query);

			var rows     = Out.Rows(query);
			var expected = Out.Row([Out.Data.Where(o => o.TenantId == 1).Average(o => o.Priority)]);

			Out.Show(rows);
			Out.Line("expected: " + expected);
			Out.Verdict("Avg(int) is the C# mean", rows.Count == 1 && rows[0] == expected, rows.FirstOrDefault() ?? "no row");

			// The same with the value converted to double? before the aggregate, as the translator could write it.
			var cast = (IQueryable<object?[]>)Chain.Select(Chain.GroupAll(Chain.Where(db.Orders, o => Expression.Equal(P(o, "TenantId"), Chain.Value(tenant)))),
				g => Expression.NewArrayInit(typeof(object), Expression.Convert(Chain.Aggregate(g, "Average", x => Expression.Convert(P(x, "Priority"), typeof(double?))), typeof(object))));

			Out.Sub("Avg((double?)int)");
			Out.Sql(cast);

			var castRows = Out.Rows(cast);

			Out.Show(castRows);
			Out.Verdict("Avg((double?)int) is the C# mean", castRows.Count == 1 && castRows[0] == expected, castRows.FirstOrDefault() ?? "no row");
		}
	}
}

/// <summary>Probe 4: values through StrongBox, and lists.</summary>
static class Values
{
	static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	/// <summary>Runs <paramref name="build"/> once per value, each time a new tree with new boxes, as a call does.</summary>
	static (long Misses, List<string> Lines, bool Right) Executions<T>(IEnumerable<T> values, Func<T, IQueryable<object?[]>> build, Func<T, int> expected, Func<T, string> show)
	{
		Out.Clear();

		var lines = new List<string>();
		var right = true;
		var total = 0L;

		foreach (var value in values)
		{
			var before = Out.Misses<object?[]>();
			var rows   = Out.Rows(build(value)).Count;
			var miss   = Out.Misses<object?[]>() - before;

			total += miss;
			right &= rows == expected(value);
			lines.Add($"{show(value)} → {rows} rows (expected {expected(value)}), miss {miss}");
		}

		return (total, lines, right);
	}

	public static void Run()
	{
		Out.Head("Values: StrongBox<T> as a parameter; in @list");

		using var db = Provider.Connect(1);

		{
			Out.Sub("where total > @limit, three values, a new tree and box per execution");

			IQueryable<object?[]> Build(decimal value)
			{
				var box = new StrongBox<decimal>(value);

				return Chain.Data(Chain.Where(db.Orders, o => Expression.GreaterThan(P(o, "Total"), Chain.Value(box))), r => [P(r, "Id")]);
			}

			Out.Sql(Build(100m));

			var (misses, lines, right) = Executions([100m, 1000m, 2400m], Build, v => Out.Data.Count(o => o.Total > v), v => v.ToString(CultureInfo.InvariantCulture));

			lines.ForEach(Out.Line);
			Out.Verdict("StrongBox value is a SQL parameter; right rows per value", right, string.Join("; ", lines));
			Out.Verdict("one cache entry for three values", misses == 1, $"{misses} misses");
		}

		{
			Out.Sub("in @list, lengths and values, a new tree and box per execution");

			IQueryable<object?[]> Build(int[] values)
			{
				var list = new StrongBox<int[]>(values);

				return Chain.Data(Chain.Where(db.Orders, o => Chain.In(list, P(o, "Id"))), r => [P(r, "Id")]);
			}

			Out.Sql(Build([1, 2, 3]));

			int[][] lists = [[1], [2], [1, 2], [3, 4], [1, 2, 3], [7, 8, 9], [1, 2, 3, 4, 5], [10, 20, 30, 40, 50], [1, 2, 3, 4, 5], [], [1, 1, 1]];

			var (misses, lines, right) = Executions(lists, Build, v => v.Distinct().Count(), v => "[" + string.Join(",", v) + "]");

			lines.ForEach(Out.Line);
			Out.Verdict("in @list: right rows", right, $"{lists.Length} lists");
			Out.Cost("in @list: cache entries", $"{misses} misses for {lists.Length} executions ({lists.Select(l => string.Join(",", l)).Distinct().Count()} distinct lists, {lists.Select(l => l.Length).Distinct().Count()} lengths)");
		}

		{
			Out.Sub("in (@a, @b): a written-out list of values");

			IQueryable<object?[]> Build((int, int) v)
			{
				var a = new StrongBox<int>(v.Item1);
				var b = new StrongBox<int>(v.Item2);

				return Chain.Data(Chain.Where(db.Orders, o => Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(int)],
					Expression.NewArrayInit(typeof(int), Chain.Value(a), Chain.Value(b)), P(o, "Id"))), r => [P(r, "Id")]);
			}

			Out.Sql(Build((1, 2)));

			var (misses, lines, right) = Executions([(1, 2), (3, 4), (5, 5)], Build, v => v.Item1 == v.Item2 ? 1 : 2, v => v.ToString());

			lines.ForEach(Out.Line);
			Out.Verdict("in (@a, @b): right rows", right, string.Join("; ", lines));
			Out.Verdict("in (@a, @b): one cache entry", misses == 1, $"{misses} misses");
		}

		{
			Out.Sub("a decimal value against a computed decimal: where total * 2 > @limit");

			var box   = new StrongBox<decimal>(100m);
			var query = Chain.Data(Chain.Where(db.Orders, o => Expression.GreaterThan(Expression.Multiply(P(o, "Total"), Expression.Constant(2m)), Chain.Value(box))), r => [P(r, "Id")]);
			var csharp = db.Orders.Where(o => o.Total * 2 > box.Value).Select(o => new object?[] { o.Id });

			Out.Sql(query);

			var rows     = Out.Rows(query).Count;
			var rowsCs   = Out.Rows(csharp).Count;
			var expected = Out.Data.Count(o => o.Total * 2 > 100m);

			Out.Line($"rows {rows} (the same in C#: {rowsCs}), expected {expected}");
			Out.Verdict("decimal value compared with a computed decimal", rows == expected, $"{rows} rows, expected {expected} ({Provider.Name})");
		}
	}
}

/// <summary>Probe 9: members linq2db reads through its own conversion (DateOnly, an enum stored as text).</summary>
static class Exotic
{
	static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	public static void Run()
	{
		Out.Head("Exotic members: DateOnly and an enum mapped to text");

		using var db = Provider.Connect(1);

		Func<DateOnly, string?> dayFormat  = d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		Func<Kind, string?>     kindFormat = k => "kind:" + (int)k;

		{
			Out.Sub("JSON: Formatted<T> and Default<T> receive T");

			var json = (IQueryable<string>)Chain.Select(Chain.OrderBy(db.Orders, o => P(o, "Id")), o => Json.Row(P(o, "Id"),
			[
				("id", P(o, "Id")), ("day", P(o, "Day")), ("dayDefault", P(o, "Day")), ("kind", P(o, "Kind")), ("kindDefault", P(o, "Kind")),
			], asObject: false, (i, s, n, v) => i switch
			{
				1 => Json.Formatted(s, n, dayFormat, v),
				2 => Json.Default(s, n, v),
				3 => Json.Formatted(s, n, kindFormat, v),
				4 => Json.Default(s, n, v),
				_ => null,
			}));

			Out.Sql(json);

			var rows     = json.ToList();
			var expected = Out.Data.OrderBy(o => o.Id).Select(o =>
				$"[{o.Id},\"{dayFormat(o.Day)}\",\"{o.Day.ToString(null, CultureInfo.InvariantCulture).Replace("/", "/")}\",\"{kindFormat(o.Kind)}\",\"{o.Kind}\"]").ToList();

			Out.Show(rows, 3);
			Out.Verdict("DateOnly and enum reach the writer as T (format and default print)", rows.SequenceEqual(expected), rows.SequenceEqual(expected) ? $"{rows.Count} rows equal" : $"first: {rows.FirstOrDefault()} vs {expected.FirstOrDefault()}");
		}

		{
			Out.Sub("QueryData: raw T");

			var data = Chain.Data(Chain.OrderBy(db.Orders, o => P(o, "Id")), o => [P(o, "Day"), P(o, "Kind")]);
			var row  = data.First();

			Out.Line($"types: {row[0]?.GetType().Name}, {row[1]?.GetType().Name}");
			Out.Verdict("QueryData returns DateOnly and Kind", row[0] is DateOnly && row[1] is Kind, $"{row[0]?.GetType().Name}, {row[1]?.GetType().Name}");
		}

		{
			var day   = new StrongBox<DateOnly>(new DateOnly(2026, 1, 8));
			var kind  = new StrongBox<Kind>(Kind.Paid);
			var query = Chain.Where(db.Orders, o => Expression.AndAlso(Expression.Equal(P(o, "Day"), Chain.Value(day)), Expression.Equal(P(o, "Kind"), Chain.Value(kind))));

			query = Chain.ThenBy(Chain.OrderBy(query, o => P(o, "Kind"), descending: true), o => P(o, "Day"));

			var data = Chain.Data(Chain.ThenBy(query, o => P(o, "Id")), o => [P(o, "Id"), P(o, "Day"), P(o, "Kind")]);

			Out.Sub("where day == @day && kind == @kind order by kind desc, day, id");
			Out.Sql(data);

			var rows     = Out.Rows(data);
			var expected = Out.Data.Where(o => o.Day == day.Value && o.Kind == kind.Value).OrderByDescending(o => o.Kind).ThenBy(o => o.Day).ThenBy(o => o.Id).Select(o => Out.Row([o.Id, o.Day, o.Kind])).ToList();

			Out.Show(rows);
			Out.Verdict("where == @p and order by on DateOnly and enum translate", rows.SequenceEqual(expected) && rows.Count > 0, $"{rows.Count} rows, expected {expected.Count}");
		}

		{
			var days  = new StrongBox<DateOnly[]>([new DateOnly(2026, 1, 2), new DateOnly(2026, 1, 3)]);
			var kinds = new StrongBox<Kind[]>([Kind.New, Kind.Shipped]);
			var data  = Chain.Data(Chain.OrderBy(Chain.Where(db.Orders, o => Expression.AndAlso(Chain.In(days, P(o, "Day")), Chain.In(kinds, P(o, "Kind")))), o => P(o, "Id")), o => [P(o, "Id")]);

			Out.Sub("in @days and in @kinds");
			Out.Sql(data);

			var rows     = Out.Rows(data);
			var expected = Out.Data.Where(o => days.Value.Contains(o.Day) && kinds.Value.Contains(o.Kind)).OrderBy(o => o.Id).Select(o => Out.Row([o.Id])).ToList();

			Out.Show(rows, 3);
			Out.Verdict("in @list on DateOnly and enum", rows.SequenceEqual(expected) && rows.Count > 0, $"{rows.Count} rows, expected {expected.Count}");
		}

		{
			var data  = (IQueryable<object?[]>)Chain.Select(Chain.GroupAll(db.Orders), g => Expression.NewArrayInit(typeof(object),
				Expression.Convert(Chain.Aggregate(g, "Min", x => P(x, "Day")), typeof(object)),
				Expression.Convert(Chain.Aggregate(g, "Max", x => P(x, "Kind")), typeof(object)),
				Expression.Convert(Chain.Aggregate(g, "Count", x => Expression.Equal(P(x, "Kind"), Expression.Constant(Kind.Paid))), typeof(object))));

			Out.Sub("Min(day), Max(kind), Count(kind == Paid)");
			Out.Sql(data);

			var rows     = Out.Rows(data);
			var expected = Out.Row([Out.Data.Min(o => o.Day), Out.Data.Max(o => o.Kind), Out.Data.Count(o => o.Kind == Kind.Paid)]);

			Out.Show(rows);
			Out.Verdict("Min/Max/Count over DateOnly and enum", rows.Count == 1 && rows[0] == expected, $"got {rows.FirstOrDefault()}, expected {expected}");
		}
	}
}
