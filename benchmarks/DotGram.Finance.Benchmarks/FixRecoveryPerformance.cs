using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Loader;
using System.Text.Json;

namespace DotGram.Finance.Benchmarks;

public static class FixRecoveryPerformance
{
	public static void Run(string[] args)
	{
		var previous = Bind(args[0], "previous");
		var current  = Bind(args[1], "current");
		Console.WriteLine("Spaces,Previous_us,Current_us,Ratio,Previous_B,Current_B");

		foreach (var length in new[] { 256, 512, 1024, 2048, 4096 })
		{
			var input = "broken" + new string(' ', length) + "x|55=END";
			var oldFields = previous(input);
			var newFields = current(input);
			if (oldFields.Length != 2 || newFields.Length != 2)
				throw new InvalidOperationException("Expected an invalid field and a valid field.");

			for (var i = 0; i < oldFields.Length; i++)
			{
				var left  = oldFields.GetValue(i)!;
				var right = newFields.GetValue(i)!;
				if (left.GetType().Name != right.GetType().Name ||
					JsonSerializer.Serialize(left, left.GetType()) != JsonSerializer.Serialize(right, right.GetType()))
					throw new InvalidOperationException("Recovery values, locations or diagnostics differ.");
			}

			Measure(previous, input, 150);
			Measure(current, input, 150);
			var oldTimes = new double[9];
			var newTimes = new double[9];
			for (var i = 0; i < oldTimes.Length; i++)
			{
				if (i % 2 == 0)
				{
					oldTimes[i] = Measure(previous, input, 60);
					newTimes[i] = Measure(current, input, 60);
				}
				else
				{
					newTimes[i] = Measure(current, input, 60);
					oldTimes[i] = Measure(previous, input, 60);
				}
			}
			Array.Sort(oldTimes);
			Array.Sort(newTimes);
			Console.WriteLine(FormattableString.Invariant(
				$"{length},{oldTimes[4]:F3},{newTimes[4]:F3},{newTimes[4] / oldTimes[4]:F4},{Allocated(previous, input)},{Allocated(current, input)}"));
		}
	}

	static Func<string,Array> Bind(string path, string name)
	{
		var assembly = new AssemblyLoadContext(name).LoadFromAssemblyPath(Path.GetFullPath(path));
		var type = assembly.GetType("DotGram.Finance.Fix.FixParser", true)!;
		var method = type.GetMethods().Single(m => m.Name == "ParseLog" && m.GetParameters()[0].ParameterType == typeof(string));
		var input = Expression.Parameter(typeof(string));
		var call = Expression.Call(method, input, Expression.Constant(null, method.GetParameters()[1].ParameterType));

		return Expression.Lambda<Func<string,Array>>(Expression.Convert(call, typeof(Array)), input).Compile();
	}

	static double Measure(Func<string,Array> parse, string input, int milliseconds)
	{
		var watch = Stopwatch.StartNew();
		var count = 0;
		do
		{
			_sink = parse(input);
			count++;
		}
		while (watch.ElapsedMilliseconds < milliseconds);

		return watch.Elapsed.TotalMicroseconds / count;
	}

	static long Allocated(Func<string,Array> parse, string input)
	{
		var start = GC.GetAllocatedBytesForCurrentThread();
		for (var i = 0; i < 32; i++)
			_sink = parse(input);

		return (GC.GetAllocatedBytesForCurrentThread() - start) / 32;
	}

	static Array? _sink;
}
