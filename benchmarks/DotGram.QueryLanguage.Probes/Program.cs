using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using DotGram.QueryLanguage.Probes;

using LinqToDB;
using LinqToDB.Internal.Linq;

// `dotnet run -c Release -- <probe|all|drop> [sqlite|mssql]`. Every probe prints its SQL, its rows and one verdict line per
// claim; `all` runs probes 1-10 and 12 (11 is the timing, run on its own). The SQL Server probes read the connection from
// /work/secrets/mssql.connection and work in their own database, which `drop` removes.

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture              = CultureInfo.InvariantCulture;

var probe    = args.Length > 0 ? args[0] : "help";
var provider = args.Length > 1 ? args[1] : "sqlite";

if (probe == "drop")
{
	Provider.DropSqlServer();
	Console.WriteLine("dropped");

	return 0;
}

Dictionary<string, Action> probes = new()
{
	["1"]  = RowCap.Run,
	["2"]  = Chains.Run,
	["3"]  = Aggregates.Run,
	["4"]  = Values.Run,
	["5"]  = JsonChain.Run,
	["6"]  = AsyncPath.Run,
	["7"]  = HostOperators.Run,
	["8"]  = TwoModels.Run,
	["9"]  = Exotic.Run,
	["10"] = Paging.Run,
	["11"] = Speed.Run,
	["12"] = Tenants.Run,
};

if (probe != "all" && !probes.ContainsKey(probe))
{
	Console.WriteLine("usage: <1..12|all|drop> [sqlite|mssql]");

	return 2;
}

Console.WriteLine($"# P-CHAIN probes — provider {provider}, linq2db {typeof(DataExtensions).Assembly.GetName().Version}, {Environment.Version}");

foreach (var (id, run) in probes)
{
	if ((probe == "all" && id != "11") || probe == id)
	{
		Provider.Setup(provider, Out.TableRows);
		Out.Probe = id;
		run();
	}
}

Out.Summary();

return Out.Stops > 0 ? 1 : 0;

namespace DotGram.QueryLanguage.Probes
{
	static class Out
	{
		public const int TableRows = 200;

		public static string Probe = "";

		public static int Stops;

		static readonly List<string> verdicts = [];

		public static void Head(string title)
		{
			Console.WriteLine();
			Console.WriteLine($"## {Probe}. {title}");
			Console.WriteLine();
		}

		public static void Sub(string title)
		{
			Console.WriteLine();
			Console.WriteLine($"### {title}");
			Console.WriteLine();
		}

		public static void Line(string text)
		{
			Console.WriteLine(text);
		}

		/// <summary>One claim: <paramref name="holds"/> false on a stop claim counts as a stop.</summary>
		public static void Verdict(string claim, bool holds, string detail, bool stop = false)
		{
			var word = holds ? "holds" : stop ? "FAILS (STOP)" : "fails";

			if (!holds && stop)
			{
				Stops++;
			}

			var line = $"VERDICT {Probe} | {claim} | {word} | {detail}";

			Console.WriteLine(line);
			verdicts.Add(line);
		}

		public static void Cost(string claim, string detail)
		{
			var line = $"VERDICT {Probe} | {claim} | cost | {detail}";

			Console.WriteLine(line);
			verdicts.Add(line);
		}

		public static void Summary()
		{
			Console.WriteLine();
			Console.WriteLine("## Verdicts");
			Console.WriteLine();

			foreach (var v in verdicts)
			{
				Console.WriteLine(v);
			}

			Console.WriteLine();
			Console.WriteLine($"stops: {Stops}");
		}

		static readonly MethodInfo toSql = typeof(LinqExtensions).GetMethods()
			.Single(m => m.Name == nameof(LinqExtensions.ToSqlQuery) && m.GetParameters()[0].ParameterType.IsGenericType && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>));

		public static string SqlText(IQueryable query)
		{
			var sql        = toSql.MakeGenericMethod(query.ElementType).Invoke(null, [query, null])!;
			var text       = (string)sql.GetType().GetProperty("Sql")!.GetValue(sql)!;
			var parameters = (IEnumerable)sql.GetType().GetProperty("Parameters")!.GetValue(sql)!;

			foreach (var parameter in parameters)
			{
				var name  = parameter.GetType().GetProperty("Name")!.GetValue(parameter);
				var value = parameter.GetType().GetProperty("Value")!.GetValue(parameter);

				text = text.TrimEnd() + $"\n-- @{name} = {Value(value)}";
			}

			return text;
		}

		public static void Sql(IQueryable query)
		{
			string text;

			try
			{
				text = SqlText(query);
			}
			catch (TargetInvocationException e)
			{
				text = "-- " + Error(e.InnerException!);
			}

			Console.WriteLine("```sql");
			Console.WriteLine(text.TrimEnd());
			Console.WriteLine("```");
		}

		/// <summary>How many SELECTs the statement has: 1 = flat.</summary>
		public static int Selects(string sql)
		{
			var count = 0;
			var at    = 0;

			while ((at = sql.IndexOf("SELECT", at, StringComparison.OrdinalIgnoreCase)) >= 0)
			{
				count++;
				at += 6;
			}

			return count;
		}

		public static string Error(Exception e)
		{
			while (e is TargetInvocationException or AggregateException && e.InnerException != null)
			{
				e = e.InnerException;
			}

			return e.GetType().Name + ": " + e.Message.Split('\n')[0].Trim();
		}

		public static string Value(object? value)
		{
			return value switch
			{
				null               => "NULL",
				string s           => "'" + s + "'",
				DateTime d         => d.ToString("O", CultureInfo.InvariantCulture),
				DateTimeOffset d   => d.ToString("O", CultureInfo.InvariantCulture),
				decimal m          => m.ToString("0.############################", CultureInfo.InvariantCulture) + "m",   // the value; the scale is the provider's
				double d           => d.ToString("R", CultureInfo.InvariantCulture),
				IFormattable f     => f.ToString(null, CultureInfo.InvariantCulture),
				_                  => value.ToString() ?? "",
			};
		}

		public static string Row(object?[] row)
		{
			return string.Join("|", row.Select(Value));
		}

		public static List<object> List(IQueryable query)
		{
			var rows = new List<object>();

			foreach (var row in (IEnumerable)query)
			{
				rows.Add(row);
			}

			return rows;
		}

		public static List<string> Rows(IQueryable<object?[]> query)
		{
			return query.AsEnumerable().Select(Row).ToList();
		}

		public static void Show(IReadOnlyList<string> rows, int max = 6)
		{
			Console.WriteLine($"rows: {rows.Count}");

			foreach (var row in rows.Take(max))
			{
				Console.WriteLine("    " + row);
			}

			if (rows.Count > max)
			{
				Console.WriteLine("    …");
			}
		}

		public static long Misses<T>()
		{
			return Query<T>.CacheMissCount;
		}

		public static void Clear()
		{
			Query.ClearCaches();
		}

		/// <summary>Runs <paramref name="action"/>; on an exception returns its first line.</summary>
		public static string? Try(Action action)
		{
			try
			{
				action();

				return null;
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		public static IEnumerable<Order> Data => Provider.Generate(TableRows);
	}
}
