using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

using DotGram.QueryLanguage.Probes.Core;

namespace DotGram.QueryLanguage.Probes;

/// <summary>Probe 11: QueryJsonRow against QueryData + System.Text.Json at the host; Array against Object. Directional.</summary>
static class Speed
{
	const int Rows   = 1000;
	const int Rounds = 41;

	static Expression P(Expression row, string member)
	{
		return Expression.Property(row, member);
	}

	static List<(string, Expression)> Mixed(Expression o)
	{
		return Shapes.Columns(o, ["Id", "Customer", "Total", "Created", "Paid", "Rating"]);
	}

	static List<(string, Expression)> Strings(Expression o)
	{
		return
		[
			("customer", P(o, "Customer")),
			("status", P(o, "Status")),
			("upper", Expression.Call(P(o, "Customer"), typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!)),
			("lower", Expression.Call(P(o, "Customer"), typeof(string).GetMethod("ToLower", Type.EmptyTypes)!)),
			("trim", Expression.Call(P(o, "Customer"), typeof(string).GetMethod("Trim", Type.EmptyTypes)!)),
			("status2", Expression.Call(P(o, "Status"), typeof(string).GetMethod("ToUpper", Type.EmptyTypes)!)),
		];
	}

	static List<(string, Expression)> Wide(Expression o)
	{
		var columns = Shapes.Columns(o, Shapes.Kinds);

		foreach (var member in new[] { "Id", "Qty", "Big", "Total", "Score", "Created", "Customer", "Status", "Paid", "Uid", "Stamp" })
		{
			columns.Add((member.ToLowerInvariant() + "2", P(o, member)));
		}

		return columns;
	}

	sealed class Variant(string name, Func<int> run)
	{
		public string Name { get; } = name;

		public Func<int> Run { get; } = run;

		public List<double> Ms { get; } = [];

		public List<long> Bytes { get; } = [];
	}

	static Func<int> JsonRun(IQueryable<string> query)
	{
		return () =>
		{
			var n = 0;

			foreach (var row in query)
			{
				n += row.Length;
			}

			return n;
		};
	}

	static Func<int> DataWithStj(IQueryable<object?[]> query)
	{
		return () =>
		{
			var n = 0;

			foreach (var row in query)
			{
				n += JsonSerializer.Serialize(row).Length;
			}

			return n;
		};
	}

	public static void Run()
	{
		Out.Head($"Allocation and speed, {Rows} rows, directional (shared machine, one process, interleaved rounds)");

		Provider.Setup(Provider.Name, Rows);

		using var db = Provider.Connect(1);

		IQueryable Source()
		{
			return Chain.OrderBy(db.Orders, o => P(o, "Id"));
		}

		var variants = new List<Variant>
		{
			new("mixed 6: QueryJsonRow", JsonRun(Shapes.JsonRows(Source(), o => P(o, "Id"), Mixed))),
			new("mixed 6: QueryData + STJ", DataWithStj(Chain.Data(Source(), o => Mixed(o).Select(c => c.Item2)))),
			new("strings 6: QueryJsonRow (string anchor)", JsonRun(Shapes.JsonRows(Source(), o => P(o, "Customer"), Strings))),
			new("strings 6: QueryData + STJ", DataWithStj(Chain.Data(Source(), o => Strings(o).Select(c => c.Item2)))),
			new("wide 30: QueryJsonRow Array", JsonRun(Shapes.JsonRows(Source(), o => P(o, "Id"), Wide))),
			new("wide 30: QueryJsonRow Object", JsonRun(Shapes.JsonRows(Source(), o => P(o, "Id"), Wide, asObject: true))),
			new("wide 30: QueryData + STJ", DataWithStj(Chain.Data(Source(), o => Wide(o).Select(c => c.Item2)))),
		};

		foreach (var v in variants)
		{
			for (var i = 0; i < 5; i++)
			{
				v.Run();
			}
		}

		var watch = new Stopwatch();

		for (var round = 0; round < Rounds; round++)
		{
			foreach (var v in round % 2 == 0 ? variants : Enumerable.Reverse(variants))
			{
				var before = GC.GetAllocatedBytesForCurrentThread();

				watch.Restart();
				v.Run();
				watch.Stop();

				v.Ms.Add(watch.Elapsed.TotalMilliseconds);
				v.Bytes.Add(GC.GetAllocatedBytesForCurrentThread() - before);
			}
		}

		Out.Line("| variant | p50 ms / 1000 rows | p10–p90 ms | bytes/row | chars/row |");
		Out.Line("|---|---:|---|---:|---:|");

		foreach (var v in variants)
		{
			var ms = v.Ms.OrderBy(x => x).ToList();

			Out.Line($"| {v.Name} | {ms[ms.Count / 2]:F2} | {ms[ms.Count / 10]:F2}–{ms[ms.Count * 9 / 10]:F2} | {v.Bytes.OrderBy(x => x).ElementAt(v.Bytes.Count / 2) / (double)Rows:F0} | {v.Run() / (double)Rows:F1} |");
		}

		double P50(string name)
		{
			var ms = variants.Single(v => v.Name == name).Ms.OrderBy(x => x).ToList();

			return ms[ms.Count / 2];
		}

		Out.Cost("QueryJsonRow vs QueryData + STJ (mixed 6)", $"×{P50("mixed 6: QueryJsonRow") / P50("mixed 6: QueryData + STJ"):F2} time");
		Out.Cost("QueryJsonRow vs QueryData + STJ (strings 6)", $"×{P50("strings 6: QueryJsonRow (string anchor)") / P50("strings 6: QueryData + STJ"):F2} time");
		Out.Cost("Object vs Array (wide 30)", $"×{P50("wide 30: QueryJsonRow Object") / P50("wide 30: QueryJsonRow Array"):F2} time");
	}
}
