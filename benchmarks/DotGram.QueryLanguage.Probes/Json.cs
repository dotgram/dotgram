using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DotGram.QueryLanguage.Probes.Core;

using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;

namespace DotGram.QueryLanguage.Probes;

/// <summary>Shapes shared by the JSON probes.</summary>
static class Shapes
{
	public static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	/// <summary>Every kind, non-nullable and nullable, as columns of an order.</summary>
	public static readonly string[] Kinds =
	[
		"Id", "Paid", "Flag", "Qty", "Priority", "Big", "BigN", "Total", "Discount", "Score", "Rating",
		"Customer", "Status", "Created", "Shipped", "Stamp", "StampN", "Uid", "UidN",
	];

	public static List<(string, Expression)> Columns(Expression o, IEnumerable<string> members)
	{
		return members.Select(m => (m.ToLowerInvariant(), P(o, m))).ToList();
	}

	public static IQueryable Ordered(Db db)
	{
		return Chain.OrderBy(db.Orders, o => P(o, "Id"));
	}

	public static IQueryable<string> JsonRows(IQueryable source, Func<Expression, Expression> anchor, Func<Expression, List<(string, Expression)>> columns, bool asObject = false, Func<int, Expression, string?, Expression, Expression?>? write = null)
	{
		return (IQueryable<string>)Chain.Select(source, o => Json.Row(anchor(o), columns(o), asObject, write));
	}

	/// <summary>What the writer prints for a value read through QueryData: the reference for a JSON row.</summary>
	public static string Canon(object? value)
	{
		return value switch
		{
			null             => "null",
			bool b           => b ? "true" : "false",
			string s         => JsonSerializer.Serialize(s),
			DateTime d       => "\"" + d.ToString("O", CultureInfo.InvariantCulture) + "\"",
			DateTimeOffset d => "\"" + d.ToString("O", CultureInfo.InvariantCulture) + "\"",
			Guid g           => "\"" + g.ToString("D") + "\"",
			double d         => d.ToString("R", CultureInfo.InvariantCulture),
			IFormattable f   => f.ToString(null, CultureInfo.InvariantCulture),
			_                => value.ToString()!,
		};
	}

	/// <summary>A JSON row's elements as the writer printed them (raw text).</summary>
	public static List<string> Elements(string row)
	{
		using var document = JsonDocument.Parse(row);

		return document.RootElement.EnumerateArray().Select(e => e.ValueKind == JsonValueKind.String ? JsonSerializer.Serialize(e.GetString()) : e.GetRawText()).ToList();
	}

	public static long StringBytes(string s)
	{
		return Math.Max(24, (22 + 2L * s.Length + 7) / 8 * 8);
	}
}

/// <summary>Probe 5: the QlJson chain in the final projection.</summary>
static class JsonChain
{
	public static void Run()
	{
		Out.Head("JSON chain: per-row evaluation, literal and parameter columns, typed reads, allocations");

		using var db = Provider.Connect(1);

		var members = Shapes.Kinds;

		// A call builds a new tree with a new box for its value each time.
		List<(string, Expression)> Columns(Expression o, StrongBox<string> param)
		{
			var columns = Shapes.Columns(o, members);

			columns.Add(("lit", Expression.Constant(7)));
			columns.Add(("param", Chain.Value(param)));

			return columns;
		}

		IQueryable<string> BuildJson(string value)
		{
			var param = new StrongBox<string>(value);

			return Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Columns(o, param));
		}

		IQueryable<object?[]> BuildData(string value)
		{
			var param = new StrongBox<string>(value);

			return Chain.Data(Shapes.Ordered(db), o => Columns(o, param).Select(c => c.Item2));
		}

		Out.Sub("SQL of QueryJsonRow and of QueryData");

		var jsonSql = Out.SqlText(BuildJson("P1"));
		var dataSql = Out.SqlText(BuildData("P1"));

		Out.Line("```sql");
		Out.Line(jsonSql.TrimEnd());
		Out.Line("```");
		Out.Verdict("SQL of the JSON chain equals QueryData's", jsonSql == dataSql, jsonSql == dataSql ? "identical text" : "differs; QueryData:\n" + dataSql);

		Out.Sub("rows: every column per row, literal and parameter included, three executions with different parameter values");

		var right = true;
		var notes = new List<string>();

		Out.Clear();

		var misses = Out.Misses<string>();

		foreach (var value in new[] { "P1", "P2", "P3" })
		{
			var rows     = BuildJson(value).ToList();
			var expected = BuildData(value).ToList();

			Out.Show(rows, 2);

			right &= rows.Count == expected.Count && rows.Count == Out.TableRows;

			for (var i = 0; i < Math.Min(rows.Count, expected.Count); i++)
			{
				var got  = Shapes.Elements(rows[i]);
				var want = expected[i].Select(Shapes.Canon).ToList();

				if (!got.SequenceEqual(want) || got[^1] != "\"" + value + "\"")
				{
					right = false;
					notes.Add($"row {i}: {rows[i]} vs [{string.Join(",", want)}]");

					break;
				}
			}
		}

		misses = Out.Misses<string>() - misses;

		Out.Verdict("every row printed from its own values, in column order; literal and parameter per execution", right, right ? $"{Out.TableRows} rows × 3 executions equal to QueryData; {misses} cache miss(es) for QueryJsonRow" : string.Join("; ", notes), stop: true);

		var param = new StrongBox<string>("P2");

		Out.Sub("a shape with no row-dependent column: select 1, @p");
		{
			var pure = Shapes.JsonRows(Shapes.Ordered(db), _ => Expression.Constant(1), _ => [("one", Expression.Constant(1)), ("param", Chain.Value(param))]);
			var rows = pure.ToList();

			Out.Sql(pure);
			Out.Show(rows, 2);
			Out.Verdict("constant shape: rows printed (folding harmless)", rows.Count == Out.TableRows && rows.All(r => r == "[1,\"P2\"]"), $"{rows.Count} rows, distinct {rows.Distinct().Count()}");
		}

		TypedReads();
		Allocations(db);
		Exception(db);
	}

	static void TypedReads()
	{
		Out.Sub("typed reads: the reader calls in linq2db's mapper for the 18 kinds");

		var mappers = new List<string>();

		var options = Provider.Options()
			.UseTraceMapperExpression(true)
			.UseTracing(TraceLevel.Info, info =>
			{
				if (info.TraceInfoStep == TraceInfoStep.MapperCreated && info.MapperExpression != null)
				{
					lock (mappers)
					{
						mappers.Add(info.MapperExpression.ToString());
					}
				}
			});

		using var db = new Db(options, 1);

		Out.Clear();

		var json = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, Shapes.Kinds));

		json.ToList();

		if (mappers.Count == 0)
		{
			Out.Verdict("mapper expression captured", false, "no MapperCreated trace");

			return;
		}

		var mapper = mappers[^1];
		var calls  = Regex.Matches(mapper, @"\.(Get\w+|IsDBNull)\(").Select(m => m.Groups[1].Value).GroupBy(c => c).Select(g => $"{g.Key}×{g.Count()}");

		Out.Line("reader calls: " + string.Join(", ", calls));
		Out.Line("```");
		Out.Line(mapper.Length > 6000 ? mapper[..6000] + " …" : mapper);
		Out.Line("```");

		var getValue = Regex.Matches(mapper, @"\.GetValue\(").Count;

		Out.Verdict("typed reads (no GetValue) for the 18 kinds", getValue == 0, $"GetValue×{getValue}; {string.Join(", ", calls)}");
		Out.Verdict("QlJson (internal) is called by the compiled mapper", mapper.Contains("StartArray", StringComparison.Ordinal) && mapper.Contains("EndArray", StringComparison.Ordinal), "StartArray/EndArray in the mapper", stop: true);
	}

	/// <summary>Bytes per row a JSON shape allocates beyond its result strings and a typed read of the same columns.</summary>
	static (double Extra, double Strings) Measure(IQueryable<string> json, Func<int> typed)
	{
		for (var i = 0; i < 3; i++)
		{
			json.ToList();
			typed();
		}

		const int rounds = 10;

		long jsonBytes = 0, typedBytes = 0, stringBytes = 0, rows = 0;

		for (var i = 0; i < rounds; i++)
		{
			var before = GC.GetAllocatedBytesForCurrentThread();

			foreach (var row in json)
			{
				stringBytes += Shapes.StringBytes(row);
				rows++;
			}

			jsonBytes += GC.GetAllocatedBytesForCurrentThread() - before;

			before = GC.GetAllocatedBytesForCurrentThread();
			typed();
			typedBytes += GC.GetAllocatedBytesForCurrentThread() - before;
		}

		return ((double)(jsonBytes - typedBytes - stringBytes) / rows, (double)stringBytes / rows);
	}

	static int Typed<T>(IQueryable<T> query)
	{
		var n = 0;

		foreach (var _ in query)
		{
			n++;
		}

		return n;
	}

	static Func<int> TypedPair(Db db, string member)
	{
		var o    = Expression.Parameter(typeof(Order), "o");
		var col  = Expression.Property(o, member);
		var type = typeof(ValueTuple<,>).MakeGenericType(typeof(int), col.Type);
		var body = Expression.New(type.GetConstructor([typeof(int), col.Type])!, Expression.Property(o, "Id"), col);

		var query = (IQueryable)Chain.Select(Shapes.Ordered(db), r => body.Replace(o, r));

		var typed = typeof(JsonChain).GetMethod(nameof(Typed), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(type);

		return () => (int)typed.Invoke(null, [query])!;
	}

	static void Allocations(Db db)
	{
		Out.Sub("allocations per row beyond the result string and a typed read of (id, column) — anchor id (int)");

		Out.Line("| column | kind | extra bytes/row | result string bytes/row |");
		Out.Line("|---|---|---:|---:|");

		var worst = 0.0;

		foreach (var member in Shapes.Kinds.Skip(1))
		{
			var json = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Id", member]));

			var (extra, strings) = Measure(json, TypedPair(db, member));

			worst = Math.Max(worst, extra);

			Out.Line($"| {member} | {typeof(Order).GetProperty(member)!.PropertyType.Name}{(Nullable.GetUnderlyingType(typeof(Order).GetProperty(member)!.PropertyType) is { } u ? "<" + u.Name + ">" : "")} | {extra:F1} | {strings:F1} |");
		}

		Out.Verdict("value-kind anchor: no allocation beyond the result string, per kind", worst < 8, $"worst {worst:F1} bytes/row (net10.0, {Provider.Name})");

		Out.Sub("string-first shape: anchor customer (string), columns customer, status");
		{
			var stringFirst = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Customer"), o => Shapes.Columns(o, ["Customer", "Status"]));
			var withId      = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Customer", "Status"]));

			var o2     = Expression.Parameter(typeof(Order), "o");
			var typed  = (IQueryable<ValueTuple<string, string?>>)Chain.Select(Shapes.Ordered(db), r => Expression.New(typeof(ValueTuple<string, string?>).GetConstructor([typeof(string), typeof(string)])!, Shapes.P(r, "Customer"), Shapes.P(r, "Status")));

			var (a, sa) = Measure(stringFirst, () => Typed(typed));
			var (b, sb) = Measure(withId, () => Typed(typed));

			Out.Line($"anchor customer: extra {a:F1} bytes/row (strings {sa:F1}); same columns, anchor id (not printed): extra {b:F1} bytes/row");
			Out.Cost("string anchor: second read of the anchor", $"{a - b:F1} bytes/row more than an int anchor (customer strings are 2-14 chars)");
		}
	}

	static void Exception(Db db)
	{
		Out.Sub("an exception in the middle of a row, then a clean next row");

		var throwAt = new StrongBox<int>(4);
		Func<int, string?> format = id => id == throwAt.Value ? throw new InvalidOperationException("format failed at " + id) : "#" + id;

		var json = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Id", "Customer", "Qty", "Status"]),
			write: (i, s, n, v) => i == 2 ? Json.Formatted(s, n, (Func<int, string?>)(q => format(q)), Expression.Property(((MemberExpression)v).Expression!, "Id")) : null);

		var seen  = new List<string>();
		var error = Out.Try(() =>
		{
			foreach (var row in json)
			{
				seen.Add(row);
			}
		});

		Out.Line($"first run: {seen.Count} rows, then {error}");

		throwAt.Value = -1;

		var rows  = json.ToList();
		var clean = rows.Count == Out.TableRows && rows[0].StartsWith("[1,", StringComparison.Ordinal) && rows.All(r => Shapes.Elements(r).Count == 4);

		Out.Show(rows, 2);
		Out.Verdict("after an exception mid-row the next row is clean", error != null && seen.Count == 3 && clean, $"{seen.Count} rows before the exception; next run {rows.Count} rows, first {rows.FirstOrDefault()}");
	}
}

static class ExpressionReplace
{
	sealed class Replacer(Expression from, Expression to) : ExpressionVisitor
	{
		public override Expression? Visit(Expression? node)
		{
			return node == from ? to : base.Visit(node);
		}
	}

	public static Expression Replace(this Expression expression, Expression from, Expression to)
	{
		return new Replacer(from, to).Visit(expression)!;
	}
}

/// <summary>Probe 6: which reader call each host terminal makes.</summary>
static class AsyncPath
{
	public static void Run()
	{
		Out.Head("Async path: ExecuteReader or ExecuteReaderAsync per terminal");

		var recorder = new ReaderRecorder();

		using var db = Provider.Connect(1, recorder);

		var data = Chain.Data(Shapes.Ordered(db), o => [Shapes.P(o, "Id"), Shapes.P(o, "Customer")]);
		var json = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Id", "Customer"]));

		Out.Line("| element | terminal | host Take | reader call | rows |");
		Out.Line("|---|---|---|---|---:|");

		var allAsync = true;

		void Row<T>(string element, string terminal, bool take, Func<IQueryable<T>, Task<int>> run, IQueryable<T> query)
		{
			recorder.Calls.Clear();

			var rows = run(take ? query.Take(5) : query).GetAwaiter().GetResult();
			var call = string.Join("+", recorder.Calls);

			allAsync &= call == "ExecuteReaderAsync";

			Out.Line($"| {element} | {terminal} | {(take ? "Take(5)" : "—")} | {call} | {rows} |");
		}

		async Task<int> ToList<T>(IQueryable<T> q)
		{
			return (await AsyncExtensions.ToListAsync(q)).Count;
		}

		async Task<int> ToArray<T>(IQueryable<T> q)
		{
			return (await AsyncExtensions.ToArrayAsync(q)).Length;
		}

		async Task<int> First<T>(IQueryable<T> q)
		{
			return await AsyncExtensions.FirstOrDefaultAsync(q) == null ? 0 : 1;
		}

		async Task<int> Stream<T>(IQueryable<T> q)
		{
			var n = 0;

			await foreach (var _ in AsyncExtensions.AsAsyncEnumerable(q))
			{
				n++;
			}

			return n;
		}

		async Task<int> Bcl<T>(IQueryable<T> q)
		{
			var n = 0;

			await foreach (var _ in (IAsyncEnumerable<T>)q)
			{
				n++;
			}

			return n;
		}

		foreach (var take in new[] { false, true })
		{
			Row("object?[]", "ToListAsync", take, ToList, data);
			Row("object?[]", "ToArrayAsync", take, ToArray, data);
			Row("object?[]", "FirstOrDefaultAsync", take, First, data);
			Row("object?[]", "AsAsyncEnumerable", take, Stream, data);
			Row("object?[]", "IAsyncEnumerable<T> (BCL cast)", take, Bcl, data);
			Row("string", "ToListAsync", take, ToList, json);
			Row("string", "ToArrayAsync", take, ToArray, json);
			Row("string", "FirstOrDefaultAsync", take, First, json);
			Row("string", "AsAsyncEnumerable", take, Stream, json);
			Row("string", "IAsyncEnumerable<T> (BCL cast)", take, Bcl, json);
		}

		Out.Verdict("every async terminal runs ExecuteReaderAsync", allAsync, "see table");
	}
}

/// <summary>Probe 7: operators a host appends to QueryJsonRow.</summary>
static class HostOperators
{
	public static void Run()
	{
		Out.Head("Host operators over QueryJsonRow");

		using var db = Provider.Connect(1);

		var json  = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Id", "Customer", "Status"]));
		var other = Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Qty", "Customer", "Status"]));
		var all   = json.ToList();

		Out.Line("| operator | result | rows printed |");
		Out.Line("|---|---|---:|");

		var glued = false;

		void Case(string what, Func<object> run, Func<object, bool> right, bool mayThrow)
		{
			var before = Json.Starts;

			object? result = null;

			var error   = Out.Try(() => result = run());
			var printed = Json.Starts - before;

			string shown;

			if (error != null)
			{
				shown = error;
			}
			else if (result is List<string> rows)
			{
				var bad = rows.Where(r => Out.Try(() => Shapes.Elements(r)) != null || Shapes.Elements(r).Count != 3).ToList();

				glued |= bad.Count > 0;
				shown  = $"{rows.Count} rows: {string.Join(" ", rows.Take(2))}{(bad.Count > 0 ? " — MALFORMED: " + bad[0] : "")}";

				if (!right(rows))
				{
					shown += " — UNEXPECTED";
				}
			}
			else
			{
				shown = result + (right(result!) ? "" : " — UNEXPECTED");
			}

			Out.Line($"| {what} | {shown.Replace("|", "\\|")} | {printed} |");

			if (error == null && !right(result!))
			{
				Out.Verdict(what, false, shown, stop: !mayThrow);
			}
		}

		Case("Take(3)", () => json.Take(3).ToList(), r => ((List<string>)r).SequenceEqual(all.Take(3)), false);
		Case("Skip(195)", () => json.Skip(195).ToList(), r => ((List<string>)r).SequenceEqual(all.Skip(195)), false);
		Case("Skip(10).Take(2)", () => json.Skip(10).Take(2).ToList(), r => ((List<string>)r).SequenceEqual(all.Skip(10).Take(2)), false);
		Case("Count()", () => json.Count(), r => (int)r == all.Count, false);
		Case("Any()", () => json.Any(), r => (bool)r, false);
		Case("First()", () => json.First(), r => (string)r == all[0], false);
		Case("Distinct()", () => json.Distinct().ToList(), r => ((List<string>)r).OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(all.OrderBy(x => x, StringComparer.Ordinal)), true);
		Case("Where(s => s.Contains(\"c1\"))", () => json.Where(s => s.Contains("c1")).ToList(), r => ((List<string>)r).SequenceEqual(all.Where(s => s.Contains("c1"))), true);
		Case("Where(s => s.Length > 10)", () => json.Where(s => s.Length > 10).ToList(), r => ((List<string>)r).SequenceEqual(all.Where(s => s.Length > 10)), true);
		Case("OrderBy(s => s)", () => json.OrderBy(s => s).ToList(), r => ((List<string>)r).Count == all.Count, true);
		Case("Concat(other)", () => json.Concat(other).ToList(), r => ((List<string>)r).Count == all.Count * 2, true);
		Case("Union(other)", () => json.Union(other).ToList(), r => ((List<string>)r).Count <= all.Count * 2, true);
		Case("GroupBy(s => s).Select(g => g.Key)", () => json.GroupBy(s => s).Select(g => g.Key).ToList(), r => ((List<string>)r).Count == all.Count, true);
		Case("Select(s => s + \"!\")", () => json.Select(s => s + "!").ToList(), r => ((List<string>)r).Count == all.Count, true);
		Case("Contains(all[0])", () => json.Contains(all[0]), r => (bool)r, true);

		Out.Verdict("no glued or malformed rows under any host operator", !glued, glued ? "malformed rows seen" : "every returned row is one 3-element JSON array", stop: true);

		var before = Json.Starts;

		json.Count();

		Out.Verdict("Count() prints no rows", Json.Starts == before, $"{Json.Starts - before} rows started");
	}
}

/// <summary>Probe 8: two models, one text — each prints with its own format and names.</summary>
static class TwoModels
{
	public static void Run()
	{
		Out.Head("Two models, one text: Format<DateTime> delegates and object-format names under linq2db's cache");

		Func<DateTime, string?> formatA = d => "A:" + d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		Func<DateTime, string?> formatB = d => "B:" + d.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

		var ok = Check("delegate constant", (db, a) => Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Id", "Created", "Shipped"]),
			write: (i, s, n, v) => i > 0 ? Json.Formatted(s, n, a ? formatA : formatB, v) : null), a => a ? "\"A:" : "\"B:", stop: false);

		if (!ok)
		{
			Check("int id fallback", (db, a) => Shapes.JsonRows(Shapes.Ordered(db), o => Shapes.P(o, "Id"), o => Shapes.Columns(o, ["Id", "Created"]),
				write: (i, s, n, v) => i > 0 ? Json.FormattedById(s, n, a ? 1 : 2, a ? formatA : formatB, v) : null), a => a ? "\"A:" : "\"B:", stop: true);
		}

		Check("object-format names", (db, a) => (IQueryable<string>)Chain.Select(Shapes.Ordered(db), o => Json.Row(Shapes.P(o, "Id"),
			a ? [("id", Shapes.P(o, "Id")), ("created", Shapes.P(o, "Created"))] : [("ident", Shapes.P(o, "Id")), ("when", Shapes.P(o, "Created"))], asObject: true)),
			a => a ? "{\"id\":" : "{\"ident\":", stop: true);
	}

	static bool Check(string what, Func<Db, bool, IQueryable<string>> build, Func<bool, string> mark, bool stop)
	{
		Out.Sub(what);

		Out.Clear();

		var before = Out.Misses<string>();
		var wrong  = 0;
		var total  = 0;

		using (var dbA = Provider.Connect(1))
		using (var dbB = Provider.Connect(2))
		{
			Out.Sql(build(dbA, true));

			for (var i = 0; i < 20; i++)
			{
				foreach (var a in new[] { true, false })
				{
					var rows = build(a ? dbA : dbB, a).ToList();

					total += rows.Count;
					wrong += rows.Count(r => !r.Contains(mark(a), StringComparison.Ordinal) || r.Contains(mark(!a), StringComparison.Ordinal));

					if (i == 0)
					{
						Out.Line($"model {(a ? "A" : "B")}: {rows[0]}");
					}
				}
			}
		}

		var interleavedMisses = Out.Misses<string>() - before;

		Parallel.For(0, 16, new ParallelOptions { MaxDegreeOfParallelism = 8 }, t =>
		{
			var a = t % 2 == 0;

			using var db = Provider.Connect(a ? 1 : 2);

			for (var i = 0; i < 30; i++)
			{
				var rows = build(db, a).ToList();

				System.Threading.Interlocked.Add(ref total, rows.Count);
				System.Threading.Interlocked.Add(ref wrong, rows.Count(r => !r.Contains(mark(a), StringComparison.Ordinal) || r.Contains(mark(!a), StringComparison.Ordinal)));
			}
		});

		var misses = Out.Misses<string>() - before;

		Out.Line($"rows checked {total}, printed with the other model's format or names: {wrong}; cache misses: {interleavedMisses} interleaved, {misses} after parallel");

		Out.Verdict($"{what}: each model prints its own", wrong == 0, $"{wrong} wrong of {total} rows (interleaved 40 runs, parallel 16×30)", stop: stop);
		Out.Verdict($"{what}: one cache entry", misses == 1, $"{misses} misses");

		return wrong == 0;
	}
}
