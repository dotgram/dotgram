using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

/// <summary>Loads the previous assembly separately and measures identical public calls.</summary>
[MemoryDiagnoser]
public class FixGrammarComparisonBenchmarks
{
	[Params("Order", "Raw", "Groups")]
	public string Workload { get; set; } = "Order";
	[Params("String", "Characters", "Bytes")]
	public string Input { get; set; } = "String";
	static Type? baseline;
	Func<int> previous = null!;
	Func<int> simplified = null!;

	[GlobalSetup]
	public void Setup()
	{
		var path = Environment.GetEnvironmentVariable("DOTGRAM_FIX_BASELINE")
			?? throw new InvalidOperationException("Set DOTGRAM_FIX_BASELINE to the previous DotGram.Finance.dll.");
		var old = baseline ??= new AssemblyLoadContext("Previous FIX").LoadFromAssemblyPath(Path.GetFullPath(path)).GetType(typeof(Fix44).FullName!)!;
		var current = typeof(Fix44);
		var order = Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		var raw = Fix44Benchmarks.Wire("A", "98=0|108=30|95=65536|96=" + new string('X', 65536) + "|");
		var body = new StringBuilder("55=ABC|262=REQ|268=1000|");
		for (var i = 0; i < 1000; i++) body.Append("269=0|270=12.50|271=100|");
		var groups = Fix44Benchmarks.Wire("W", body.ToString());
		var wire = Workload == "Order" ? order : Workload == "Raw" ? raw : groups;
		foreach (var input in new[] { "String", "Characters", "Bytes" })
		{
			var left = Bind(old, input);
			var right = Bind(current, input);
			foreach (var sample in new[] { order, raw, groups }) Verify(left, right, sample, input);
			var fixtures = Environment.GetEnvironmentVariable("DOTGRAM_FIX_FIXTURES");
			if (fixtures is not null)
			{
				using var json = JsonDocument.Parse(File.ReadAllText(fixtures));
				foreach (var item in json.RootElement.EnumerateArray()) Verify(left, right, item.GetProperty("wire").GetString()!, input);
			}
		}
		previous = Operation(Bind(old, Input), wire, Input);
		simplified = Operation(Bind(current, Input), wire, Input);
	}

	internal static Func<object, IEnumerable> Bind(Type type, string input)
	{
		var domain = input == "String" ? typeof(string) : input == "Bytes" ? typeof(Stream) : typeof(TextReader);
		var method = type.GetMethods().Single(m => m.Name == "Parse" && m.GetParameters()[0].ParameterType == domain);
		var argument = Expression.Parameter(typeof(object));
		var parameters = method.GetParameters();
		var arguments = parameters.Select((p, i) => i == 0 ? (Expression)Expression.Convert(argument, domain)
			: Expression.Constant(p.DefaultValue, p.ParameterType)).ToArray();
		return Expression.Lambda<Func<object, IEnumerable>>(Expression.Convert(Expression.Call(method, arguments), typeof(IEnumerable)), argument).Compile();
	}

	static Func<int> Operation(Func<object, IEnumerable> parse, string wire, string input)
	{
		var bytes = Encoding.Latin1.GetBytes(wire);
		return () =>
		{
			using var reader = input == "Characters" ? new StringReader(wire) : null;
			using var stream = input == "Bytes" ? new MemoryStream(bytes, false) : null;
			var fields = parse(input == "String" ? wire : input == "Bytes" ? stream! : reader!);
			if (fields is Array array) return array.Length;
			var count = 0;
			foreach (var field in fields) count++;
			return count;
		};
	}

	static void Verify(Func<object, IEnumerable> left, Func<object, IEnumerable> right, string wire, string input)
	{
		string[] Read(Func<object, IEnumerable> parse)
		{
			using var reader = new StringReader(wire);
			using var stream = new MemoryStream(Encoding.Latin1.GetBytes(wire));
			var source = input == "String" ? (object)wire : input == "Bytes" ? stream : reader;
			var combine = Environment.GetEnvironmentVariable("DOTGRAM_FIX_COMBINED_BINARY") == "1";
			var lengthTags = new[] { 90, 93, 95, 212, 348, 350, 352, 354, 356, 358, 360, 362, 364, 445, 618, 621 };
			var dataTags = new[] { 89, 91, 96, 213, 349, 351, 353, 355, 357, 359, 361, 363, 365, 446, 619, 622 };
			return parse(source).Cast<object>().Select(field =>
			{
				var json = JsonSerializer.SerializeToNode(field, field.GetType())!;
				var tag = json["Tag"]!.GetValue<int>();
				if (combine && lengthTags.Contains(tag)) return null;
				// A binary result now spans the whole pair; its payload coordinates stay identical.
				if (combine && dataTags.Contains(tag)) json.AsObject().Remove("Position");
				return field.GetType().Name + ":" + json.ToJsonString();
			}).Where(value => value != null).Select(value => value!).ToArray();
		}
		if (!Read(left).SequenceEqual(Read(right))) throw new InvalidOperationException("Field types, values or locations differ.");
	}

	[Benchmark(Baseline = true)] public int Previous() => previous();
	[Benchmark] public int Simplified() => simplified();
}
