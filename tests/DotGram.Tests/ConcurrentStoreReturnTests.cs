using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using DotGram.ExpressionLanguage;
using DotGram.Sql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Tests;

/// <summary>
/// Several threads read T-SQL, SQL:2023 and the expression language at once, accepted and refused
/// inputs mixed and some deep enough to be carried onto a stack of their own, and every reading
/// answers as a lone thread does; and after every one of them the thread's parked store holds
/// nothing of it.
/// </summary>
/// <remarks>
/// <para>
/// <b>What is held here that <see cref="ConcurrentParsingTests"/> does not.</b> That suite proves the
/// answers; this one proves the stores. A store's <c>Return</c> empties only the tables the parse
/// wrote, by a list it kept of them (SQL:2023's dense store, three hundred tables) or by each
/// table's own high-water mark (T-SQL's adaptive <c>ValueTable&lt;T&gt;</c>), so the defect this
/// exists to catch is a table written and not listed, or written past its mark: the value of one
/// parse would then sit in the store for the next, a reference kept alive on a thread that has
/// finished with it, and on another input it would be read back as that input's own. Neither
/// shows in an answer until it does. So after every parse, on the thread that made it, every slot
/// the pool may hold the store in is read, and the store in it must look new in everything but
/// capacity: the list empty, every count and every mark at zero, and every element of every table,
/// first array and page the default of its type.
/// </para>
/// <para>
/// <b>Deep inputs</b> because a reading carried onto another stack returns to this thread's store
/// through the hand-off, and a store that reached the bound or far outgrew its use is parked in a
/// slot of its own or let go of weakly: the slots are all read, whichever the parse ended in.
/// The threads are a megabyte deep, the size a host's thread has unless it asks for more, so a
/// reading of this depth that did not hand off would end the process rather than fail an assertion
/// (<c>DeepTreeTests</c> says why).
/// </para>
/// </remarks>
public sealed class ConcurrentStoreReturnTests
{
	/// <summary>Deep enough that no megabyte of stack reads it without handing the reading off.</summary>
	const int Depth = 3_000;

	/// <summary>The size of thread a host gets without asking.</summary>
	const int Stack = 1024 * 1024;

	sealed class Row(string name, Type parser, Func<string> compute)
	{
		public readonly string       Name    = name;
		public readonly Type         Parser  = parser;
		public readonly Func<string> Compute = compute;
		public string                Expected = "";
	}

	[Fact]
	public void Stores_come_back_empty_after_every_parse_on_every_thread()
	{
		var rows = Rows();

		// The one reading not in question: single-threaded, on a thread of the same size, before any
		// other thread exists.
		var (_, thrown) = OnStack(() =>
		{
			foreach (var row in rows)
				row.Expected = row.Compute();

			return true;
		});

		Assert.Null(thrown);

		var threads  = Math.Clamp(Environment.ProcessorCount / 2, 3, 8);
		var failures = new ConcurrentQueue<string>();
		var ready    = new CountdownEvent(threads);
		var go       = new ManualResetEventSlim(false);
		var workers  = new Thread[threads];

		for (var t = 0; t < threads; t++)
		{
			var origin = "thread" + t.ToString(CultureInfo.InvariantCulture);

			// Each thread reads the rows in its own order, so that the deep readings of one
			// overlap the one-token readings of another rather than every thread doing the same
			// thing at the same moment.
			var order = Enumerable.Range(0, rows.Length).Select(i => (i + t * 5) % rows.Length).ToArray();

			workers[t] = new Thread(() =>
			{
				ready.Signal();
				go.Wait();

				for (var round = 0; round < 2; round++)
					foreach (var at in order)
					{
						var row = rows[at];
						var where = $"{origin}/round{round}, {row.Name}";

						try
						{
							var actual = row.Compute();

							if (actual != row.Expected)
								failures.Enqueue($"{where}: expected <{row.Expected}> but got <{actual}>");

							foreach (var complaint in StoreComplaints(row.Parser))
								failures.Enqueue($"{where}: after Return, {complaint}");
						}
						catch (Exception exception)
						{
							failures.Enqueue($"{where}: threw {exception}");
						}
					}
			}, Stack)
			{ IsBackground = true };
		}

		foreach (var worker in workers)
			worker.Start();

		ready.Wait(TestContext.Current.CancellationToken);
		go.Set();

		foreach (var worker in workers)
			worker.Join();

		Assert.True(failures.IsEmpty, Environment.NewLine + string.Join(Environment.NewLine, failures.Take(20)));
	}

	static Row[] Rows()
	{
		var tsql  = typeof(TransactSqlParser);
		var sql   = typeof(SqlStandardParser);
		var el    = typeof(ExpressionParser);
		var caller = typeof(ConcurrentStoreReturnTests).Assembly;

		// A tree a megabyte of stack does not hold, and the same tree a bracket short.
		var deepCase    = Nested("SELECT ", "CASE WHEN 1 = 1 THEN 1 ELSE ", "1", " END", "", Depth);
		var deepNest    = Nested("", "(", "a", ")", "", Depth);
		var deepLambda  = Nested("(int x) => ", "(", "x", ")", "", Depth);

		return
		[
			new Row("T-SQL: a join", tsql, () => Tsql("SELECT c.CustomerId, c.Name, o.OrderId, o.Total FROM dbo.Customers AS c INNER JOIN dbo.Orders AS o ON o.CustomerId = c.CustomerId WHERE o.Total > 100 AND c.Region = 'EU' ORDER BY o.Total DESC")),
			new Row("T-SQL: an insert", tsql, () => Tsql("INSERT INTO dbo.Orders (OrderId, CustomerId, Total, Placed) VALUES (1, 42, 99.50, '2026-09-18'), (2, 43, 10, '2026-09-19'), (3, 44, 0, NULL)")),
			new Row("T-SQL: one token", tsql, () => Tsql("SELECT 1")),
			new Row("T-SQL: a condition that is refused", tsql, () => Tsql("SELECT * FROM t WHERE c = 1 AND dbo.f(1)")),
			new Row("T-SQL: nothing to select", tsql, () => Tsql("SELECT FROM")),
			new Row("T-SQL: a deep CASE", tsql, () => Tsql(deepCase)),
			new Row("T-SQL: a deep CASE an END short", tsql, () => Tsql(deepCase[..^4])),
			new Row("SQL:2023: a literal", sql, () => Sql(SqlStandardParser.TryParseValue("1"))),
			new Row("SQL:2023: a column", sql, () => Sql(SqlStandardParser.TryParseExpression("a.b.c"))),
			new Row("SQL:2023: a query", sql, () => Sql(SqlStandardParser.TryParseStatement("SELECT a, b + 1 FROM t WHERE a = 1 AND b IS NOT NULL"))),
			new Row("SQL:2023: TOP is refused", sql, () => Sql(SqlStandardParser.TryParseStatement("SELECT TOP 1 a FROM t"))),
			new Row("SQL:2023: a dangling operator", sql, () => Sql(SqlStandardParser.TryParseExpression("a + "))),
			new Row("SQL:2023: a deep nest", sql, () => Sql(SqlStandardParser.TryParseExpression(deepNest))),
			new Row("SQL:2023: a deep nest a bracket short", sql, () => Sql(SqlStandardParser.TryParseExpression(deepNest[..^1]))),
			new Row("EL: the floor", el, () => El(ExpressionParser.TryParse("(int x) => x", caller))),
			new Row("EL: a ladder", el, () => El(ExpressionParser.TryParse("(int x, int y) => (x + y) * 3 - x / 5", caller))),
			new Row("EL: an unresolved call is refused", el, () => El(ExpressionParser.TryParse("() => Abs(-2)", caller))),
			new Row("EL: a deep nest", el, () => El(ExpressionParser.TryParse(deepLambda, caller))),
			new Row("EL: a deep nest a bracket short", el, () => El(ExpressionParser.TryParse(deepLambda[..^1], caller))),
		];
	}

	static string Tsql(string text)
	{
		var match = TransactSqlParser.TryParseStatement(text);

		return match.IsSuccess ? "OK:" + SqlWriter.Write(match.Value) : Refused(match.Position, match.Error);
	}

	static string Sql<T>(SqlStandardParser.Match<T> match)
		where T : Ast.ISqlNode
	{
		return match.IsSuccess ? "OK:" + Ast.Sql2023Writer.Write(match.Value) : Refused(match.Position, match.Error);
	}

	static string El(ExpressionParser.Match<System.Linq.Expressions.LambdaExpression> match)
	{
		return match.IsSuccess ? "OK:" + match.Value.ToString() : Refused(match.Position, match.Error);
	}

	static string Refused(long position, string? error)
	{
		return "REFUSED:" + position.ToString(CultureInfo.InvariantCulture) + ":" + error;
	}

	// ── The stores ───────────────────────────────────────────────────────────────

	const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

	/// <summary>
	/// Every store of <paramref name="parser"/>'s <c>DirectValues</c> that this thread's pool holds —
	/// the ordinary spare, the deeper spares, the parked large store and the two let go of weakly —
	/// and what is wrong with each, if anything.
	/// </summary>
	static IEnumerable<string> StoreComplaints(Type parser)
	{
		// The store is the carrier's: DirectValues on the tape, ImmediateValues where the parser is
		// carried immediately (T-SQL). Both pool their stores the same way, in the same slots.
		var store = parser.GetNestedType("DirectValues", Any)
			?? parser.GetNestedType("ImmediateValues", Any)
			?? throw new InvalidOperationException($"{parser.Name} has neither DirectValues nor ImmediateValues");
		var held  = new List<(string Slot, object Store)>();

		void Slot(string name)
		{
			var field = store.GetField(name, Any);
			var value = field?.GetValue(null);

			switch (value)
			{
				case null:
					break;
				case Array deeper:
					foreach (var one in deeper)
						if (one is not null)
							held.Add((name, one));
					break;
				default:
					if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(WeakReference<>))
					{
						var target = value.GetType().GetMethod("TryGetTarget")!;
						var args   = new object?[] { null };

						if ((bool)target.Invoke(value, args)!)
							held.Add((name, args[0]!));
					}
					else
						held.Add((name, value));
					break;
			}
		}

		Slot("_spare");
		Slot("_deeper");
		Slot("_large");
		Slot("_spareLetGo");
		Slot("_largeLetGo");

		// A thread that has parsed holds its store somewhere: nothing is dropped.
		if (held.Count == 0)
			yield return "no store is held in any slot of the pool";

		foreach (var (slot, one) in held)
			foreach (var complaint in Complaints(one))
				yield return $"{slot}: {complaint}";
	}

	/// <summary>What is not as new about one store: a count, a mark, a list or an element that is not zero.</summary>
	static IEnumerable<string> Complaints(object store)
	{
		var type = store.GetType();

		foreach (var field in type.GetFields(Any).Where(static one => !one.IsStatic))
		{
			var name = field.Name;

			if (name == "_used" || name == "WrittenCount" || (name.Length > 1 && name[0] == 'N' && char.IsDigit(name[1])))
			{
				if ((int)field.GetValue(store)! != 0)
					yield return $"{name} is {field.GetValue(store)}";
			}
			else if (name.Length > 1 && name[0] == 'V' && char.IsDigit(name[1]))
			{
				var table = field.GetValue(store)!;

				if (table is Array array)
				{
					if (FirstNonDefault(array) is { } at)
						yield return $"{name}[{at}] holds a value";
				}
				else
				{
					// An adaptive table: its mark, its first array and every page it has made.
					var high = table.GetType().GetField("High", Any)!;

					if ((int)high.GetValue(table)! != 0)
						yield return $"{name}.High is {high.GetValue(table)}";

					var first = (Array)table.GetType().GetField("_first", Any)!.GetValue(table)!;

					if (FirstNonDefault(first) is { } at)
						yield return $"{name}._first[{at}] holds a value";

					if (table.GetType().GetField("_pages", Any)!.GetValue(table) is Array pages)
						for (var page = 0; page < pages.Length; page++)
							if (pages.GetValue(page) is Array made && FirstNonDefault(made) is { } atPage)
								yield return $"{name} page {page}[{atPage}] holds a value";
				}
			}
			else if (name == "Built")
			{
				var built = (bool[])field.GetValue(store)!;

				if (Array.IndexOf(built, true) is var at && at >= 0)
					yield return $"Built[{at}] is set";
			}
			else if ((name.Length > 5 && name.StartsWith("Count", StringComparison.Ordinal) && char.IsDigit(name[5]))
				|| (name.Length > 4 && name.StartsWith("High", StringComparison.Ordinal) && char.IsDigit(name[4])))
			{
				// The immediate store: one stack a type, its count and the high-water mark Return clears to.
				if ((int)field.GetValue(store)! != 0)
					yield return $"{name} is {field.GetValue(store)}";
			}
			else if (name.Length > 5 && name.StartsWith("Stack", StringComparison.Ordinal) && char.IsDigit(name[5]))
			{
				var stack = (Array)field.GetValue(store)!;

				for (var at = 0; at < stack.Length; at++)
					if (stack.GetValue(at) is not null)
					{
						yield return $"{name}[{at}] holds a value";
						break;
					}
			}
		}
	}

	/// <summary>The first element of a table of <c>Held&lt;T&gt;</c> that is not the default of its type, or null.</summary>
	static int? FirstNonDefault(Array table)
	{
		var method = Scan.MakeGenericMethod(table.GetType().GetElementType()!);

		return (int?)method.Invoke(null, [table]);
	}

	static readonly MethodInfo Scan = typeof(ConcurrentStoreReturnTests).GetMethod(nameof(FirstNonDefaultOf), Any)!;

	static int? FirstNonDefaultOf<T>(T[] table)
		where T : struct
	{
		var comparer = EqualityComparer<T>.Default;

		for (var at = 0; at < table.Length; at++)
			if (!comparer.Equals(table[at], default))
				return at;

		return null;
	}

	// ── Helpers ──────────────────────────────────────────────────────────────────

	static string Nested(string prefix, string open, string middle, string close, string suffix, int depth)
	{
		var text = new StringBuilder(prefix.Length + suffix.Length + middle.Length + (open.Length + close.Length) * depth);

		text.Append(prefix);

		for (var i = 0; i < depth; i++)
			text.Append(open);

		text.Append(middle);

		for (var i = 0; i < depth; i++)
			text.Append(close);

		return text.Append(suffix).ToString();
	}

	/// <summary>Runs <paramref name="function"/> on a thread of <see cref="Stack"/> bytes, and says what it answered or threw.</summary>
	static (T Answer, Exception? Thrown) OnStack<T>(Func<T> function)
	{
		T          answer = default!;
		Exception? caught = null;

		var thread = new Thread(() =>
		{
			try
			{
				answer = function();
			}
			catch (Exception exception)
			{
				caught = exception;
			}
		}, Stack);

		thread.Start();
		thread.Join();

		return (answer, caught);
	}
}
