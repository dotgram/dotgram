using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

using BenchmarkDotNet.Attributes;

using DotGram.Finance.Fix;
using DotGram.Handwritten.Fix;

namespace DotGram.Finance.Benchmarks;

[MemoryDiagnoser]
public class HandFixBenchmarks
{
	[Params("One", "Order", "Binary64", "Binary4096", "BinaryMany", "Orders128", "Recovery")]
	public string Workload { get; set; } = "Order";

	[Params("Text", "Bytes", "Reader", "Stream")]
	public string InputForm { get; set; } = "Text";

	string _text = "";
	byte[] _bytes = [];
	bool _log;
	static int _sink;

	[GlobalSetup]
	public void Setup()
	{
		var order = Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		_log = Workload == "Recovery";
		_text = Workload switch
		{
			"One" => "55=ABC\u0001",
			"Order" => order,
			"Binary64" => "95=64\u000196=" + new string('X', 64) + "\u0001",
			"Binary4096" => "95=4096\u000196=" + new string('X', 4096) + "\u0001",
			"BinaryMany" => string.Concat(Enumerable.Repeat("95=3\u000196=a\u0001b\u0001", 64)),
			"Orders128" => string.Concat(Enumerable.Repeat(order, 128)),
			"Recovery" => "broken" + new string(' ', 4096) + "x | 55=END",
			_ => throw new ArgumentOutOfRangeException(nameof(Workload)),
		};
		_bytes = Encoding.Latin1.GetBytes(_text);
		var expected = Fields(false).Select(Describe).ToArray();
		var actual = Fields(true).Select(Describe).ToArray();
		if (!expected.SequenceEqual(actual))
			throw new InvalidOperationException("The two FIX parsers produced different fields.");
	}

	[Benchmark(Baseline = true)]
	public int Generated()
	{
		return Count(false);
	}

	[Benchmark]
	public int Handwritten()
	{
		return Count(true);
	}

	int Count(bool hand)
	{
		var count = 0;
		foreach (var field in Fields(hand))
			count += field.Tag;
		return count;
	}

	IEnumerable<FixField> Fields(bool hand)
	{
		if (InputForm == "Text")
			return hand
				? _log ? HandFixParser.Parse(_text, FixFieldOptions.Log) : HandFixParser.Parse(_text)
				: _log ? FixParser.ParseFields(_text, FixFieldOptions.Log) : FixParser.ParseFields(_text);
		if (InputForm == "Bytes")
			return hand
				? _log ? HandFixParser.Parse(_bytes, FixFieldOptions.Log) : HandFixParser.Parse(_bytes)
				: _log ? FixParser.ParseFields(_bytes, FixFieldOptions.Log) : FixParser.ParseFields(_bytes);

		return Read();

		IEnumerable<FixField> Read()
		{
			using var reader = new StringReader(_text);
			using var stream = new MemoryStream(_bytes, false);
			var fields = InputForm == "Reader"
				? hand
					? _log ? HandFixParser.Parse(reader, FixFieldOptions.Log) : HandFixParser.Parse(reader)
					: _log ? FixParser.ReadFields(reader, FixFieldOptions.Log) : FixParser.ReadFields(reader)
				: hand
					? _log ? HandFixParser.Parse(stream, FixFieldOptions.Log) : HandFixParser.Parse(stream)
					: _log ? FixParser.ReadFields(stream, FixFieldOptions.Log) : FixParser.ReadFields(stream);
			foreach (var field in fields)
				yield return field;
		}
	}

	static string Describe(FixField field)
	{
		if (field is FixField.Invalid invalid)
			return $"Invalid:{invalid.Position}:{invalid.Length}:{invalid.RawText}:{Convert.ToHexString(invalid.RawBytes.Span)}";
		return field.GetType().Name + JsonSerializer.Serialize(field, field.GetType());
	}

	// A short exploratory comparison. Use BenchmarkDotNet for publication-quality runs.
	public static void Compare()
	{
		Console.WriteLine("Workload,Input,Generated_us,Handwritten_us,Generated_B,Handwritten_B");
		foreach (var workload in new[] { "One", "Order", "Binary64", "Binary4096", "BinaryMany", "Orders128", "Recovery" })
		foreach (var form in new[] { "Text", "Bytes", "Reader", "Stream" })
		{
			var sample = new HandFixBenchmarks { Workload = workload, InputForm = form };
			sample.Setup();
			Measure(sample.Generated, 150);
			Measure(sample.Handwritten, 150);
			var generated = new double[9];
			var handwritten = new double[9];
			for (var i = 0; i < 9; i++)
			{
				if (i % 2 == 0)
				{
					generated[i] = Measure(sample.Generated, 50);
					handwritten[i] = Measure(sample.Handwritten, 50);
				}
				else
				{
					handwritten[i] = Measure(sample.Handwritten, 50);
					generated[i] = Measure(sample.Generated, 50);
				}
			}
			Array.Sort(generated);
			Array.Sort(handwritten);
			Console.WriteLine(FormattableString.Invariant($"{workload},{form},{generated[4]:F3},{handwritten[4]:F3},{Allocated(sample.Generated)},{Allocated(sample.Handwritten)}"));
		}
	}

	public static void FirstCall(bool hand)
	{
		const string input = "55=ABC\u0001";
		var watch = Stopwatch.StartNew();
		var fields = hand ? HandFixParser.Parse(input) : FixParser.ParseFields(input);
		var elapsed = watch.Elapsed.TotalMilliseconds;
		Console.WriteLine(FormattableString.Invariant($"{(hand ? "handwritten" : "generated")},{elapsed:F3},{fields.Length}"));
	}

	static double Measure(Func<int> operation, int milliseconds)
	{
		var watch = Stopwatch.StartNew();
		var count = 0;
		do
		{
			_sink = operation();
			count++;
		}
		while (watch.ElapsedMilliseconds < milliseconds);
		return watch.Elapsed.TotalMicroseconds / count;
	}

	static long Allocated(Func<int> operation)
	{
		var start = GC.GetAllocatedBytesForCurrentThread();
		for (var i = 0; i < 32; i++)
			_sink = operation();
		return (GC.GetAllocatedBytesForCurrentThread() - start) / 32;
	}
}
