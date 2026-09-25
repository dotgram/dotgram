using System;
using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Loader;
using System.Text;

namespace DotGram.Finance.Benchmarks;

// Paired timing probe: --log-performance <previous assembly> <current assembly>.
// Alternates warmed implementations; reports medians and allocations per parse.
public static class FixLogPerformance
{
	public static void Run(string[] args)
	{
		var previous = Load(args[0], "previous");
		var current  = Load(args[1], "current");
		var legacy   = (previous.Assembly.GetType("DotGram.Finance.Fix.Fix44.FixOptions") ?? previous.Assembly.GetType("DotGram.Finance.Fix.FixOptions"))!.GetProperty("Separator") != null;
		var samples = new Dictionary<string,string>
		{
			["Short"] = "55=ABC|38=100|",
			["Order"] = "8=FIX.4.4|9=120|35=D|49=S|56=T|34=1|52=20260915-12:00:00|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|10=000|",
			["Text"]  = "58=" + string.Concat(Enumerable.Repeat("some text with spaces ", 100)) + "end|",
			["Spaces"] = "58=A" + new string(' ', 1024) + "B|"
		};

		Console.WriteLine("Workload,Input,Mode,Previous_ns,Current_ns,Ratio,Previous_B,Current_B");
		foreach (var sample in samples)
		foreach (var input in new[] { typeof(string), typeof(TextReader), typeof(Stream) })
		foreach (var mode in new[] { "Wire", "Pipe", "Padded" })
		{
			var wire = sample.Value.Replace('|', '\u0001');
			var oldText = mode == "Wire" ? wire : mode == "Padded" && !legacy ? sample.Value.Replace("|", " | ") : sample.Value;
			var newText = mode == "Padded" ? sample.Value.Replace("|", " | ") : oldText;
			var oldParse = Bind(previous, input, mode != "Wire", legacy);
			var newParse = Bind(current, input, mode != "Wire", false);
			var oldRun = Operation(oldParse, input, oldText);
			var newRun = Operation(newParse, input, newText);

			if (Signature(oldParse, input, oldText) != Signature(newParse, input, newText))
				throw new InvalidOperationException("Field values differ.");

			Measure(oldRun, 150);
			Measure(newRun, 150);
			var left  = new double[9];
			var right = new double[9];
			for (var i = 0; i < left.Length; i++)
			{
				if (i % 2 == 0)
				{
					left[i]  = Measure(oldRun, 60);
					right[i] = Measure(newRun, 60);
				}
				else
				{
					right[i] = Measure(newRun, 60);
					left[i]  = Measure(oldRun, 60);
				}
			}

			Array.Sort(left);
			Array.Sort(right);
			Console.WriteLine(FormattableString.Invariant($"{sample.Key},{input.Name},{mode},{left[4]:F1},{right[4]:F1},{right[4] / left[4]:F3},{Allocated(oldRun)},{Allocated(newRun)}"));
		}
	}

	static Type Load(string path, string name)
	{
		return new AssemblyLoadContext(name).LoadFromAssemblyPath(Path.GetFullPath(path))
			.GetType("DotGram.Finance.Fix.FixParser", true)!;
	}

	static Func<object,IEnumerable> Bind(Type type, Type input, bool log, bool previous)
	{
		var name = log && !previous ? "ParseLog" : "Parse";
		var method = type.GetMethods().Single(m => m.Name == name && m.GetParameters()[0].ParameterType == input);
		var parameter = Expression.Parameter(typeof(object));
		var arguments = method.GetParameters().Select((p, i) =>
		{
			if (i == 0)
				return (Expression)Expression.Convert(parameter, input);

			if (i == 1 && previous && log)
				return Expression.Constant(Activator.CreateInstance(p.ParameterType, '|', null), p.ParameterType);

			return Expression.Constant(p.DefaultValue, p.ParameterType);
		}).ToArray();

		return Expression.Lambda<Func<object,IEnumerable>>(
			Expression.Convert(Expression.Call(method, arguments), typeof(IEnumerable)), parameter).Compile();
	}

	static Func<int> Operation(Func<object,IEnumerable> parse, Type input, string text)
	{
		var bytes = Encoding.Latin1.GetBytes(text);
		return () =>
		{
			using var reader = input == typeof(TextReader) ? new StringReader(text) : null;
			using var stream = input == typeof(Stream) ? new MemoryStream(bytes, false) : null;
			var fields = parse((object?)reader ?? (object?)stream ?? text);
			if (fields is Array array)
				return array.Length;

			var count = 0;
			foreach (var field in fields)
				count++;

			return count;
		};
	}

	static string Signature(Func<object,IEnumerable> parse, Type input, string text)
	{
		using var reader = new StringReader(text);
		using var stream = new MemoryStream(Encoding.Latin1.GetBytes(text));
		var fields = parse(input == typeof(string) ? text : input == typeof(Stream) ? stream : reader);
		var result = new StringBuilder();
		foreach (var field in fields)
		{
			var type = field.GetType();
			if (type.Name == "Invalid")
				throw new InvalidOperationException("Invalid benchmark input.");

			result.Append(type.Name).Append(':').Append(type.GetProperty("Value")?.GetValue(field)).Append(';');
		}

		return result.ToString();
	}

	static double Measure(Func<int> run, int milliseconds)
	{
		var watch = Stopwatch.StartNew();
		var count = 0;
		do
		{
			for (var i = 0; i < 64; i++)
				_sink = run();
			count += 64;
		}
		while (watch.ElapsedMilliseconds < milliseconds);

		return watch.Elapsed.TotalNanoseconds / count;
	}

	static long Allocated(Func<int> run)
	{
		var start = GC.GetAllocatedBytesForCurrentThread();
		for (var i = 0; i < 128; i++)
			_sink = run();

		return (GC.GetAllocatedBytesForCurrentThread() - start) / 128;
	}

	static int _sink;
}
