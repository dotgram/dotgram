using System;
using System.Globalization;
using System.IO;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

namespace DotGram.Finance.Benchmarks;

static class Program
{
	static void Main(string[] args)
	{
		if (args.Length == 1 && args[0] == "--hand-fix-performance")
		{
			HandFixBenchmarks.Compare();
			return;
		}

		if (args.Length == 2 && args[0] == "--hand-fix-first")
		{
			HandFixBenchmarks.FirstCall(args[1] == "handwritten");
			return;
		}

		if (args.Length == 3 && args[0] == "--recovery-performance")
		{
			FixRecoveryPerformance.Run(args[1..]);
			return;
		}

		if (args.Length == 3 && args[0] == "--log-performance")
		{
			FixLogPerformance.Run(args[1..]);
			return;
		}

		if (args.Length is 5 or 6 && args[0] == "profile")
		{
			FixProfile.Run(args[1], args[2], args[3], int.Parse(args[4], CultureInfo.InvariantCulture), args.Length == 6 ? args[5] : "Fix");
			return;
		}
		if (args.Length == 2 && args[0] == "--fix-jit-probe")
		{
			FixInitializationBenchmarks.Probe(args[1] == "previous");
			return;
		}
		if (args.Length == 3 && args[0] == "--memory")
		{
			new Fix44InputBenchmarks { Workload = args[1] }.MeasureMemory(args[2]);
			return;
		}
		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,
			DefaultConfig.Instance.AddColumn(StatisticColumn.OperationsPerSecond));
	}
}

[MemoryDiagnoser]
public class Fix44Benchmarks
{
	string heartbeat = "";
	string order = "";
	string raw = "";
	string groups = "";
	byte[] orderBytes = Array.Empty<byte>();
	byte[] rawBytes = Array.Empty<byte>();

	[GlobalSetup]
	public void Setup()
	{
		heartbeat = Wire("0", "");
		order = Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		raw = Wire("A", "98=0|108=30|95=65536|96=" + new string('X', 65536) + "|");
		var body = new StringBuilder("262=REQ|268=1000|");
		for (var i = 0; i < 1000; i++) body.Append("269=0|270=12.50|271=100|");
		// Instrument precedes the market-data entries in the schema, but body fields
		// may be reordered independently of the entry field order.
		body.Insert(0, "55=ABC|");
		groups = Wire("W", body.ToString());
		foreach (var input in new[] { heartbeat, order, raw, groups }) FixParser.ParseMessage(input);
		orderBytes = Encoding.Latin1.GetBytes(order);
		rawBytes = Encoding.Latin1.GetBytes(raw);
		using var orderInput = new MemoryStream(orderBytes);
		using var rawInput = new MemoryStream(rawBytes);
		if (FixParser.ReadMessage(orderInput).Fields.Count != FixParser.ParseMessage(order).Fields.Count || FixParser.ReadMessage(rawInput).Fields.Count != FixParser.ParseMessage(raw).Fields.Count) throw new InvalidOperationException("Input paths differ.");
	}

	[Benchmark]
	public FixField[] FlatOrderFields()
	{
		return FixParser.ParseFields(order);
	}

	[Benchmark]
	public FixField[] FlatRawFields()
	{
		return FixParser.ParseFields(raw);
	}

	[Benchmark]
	public FixField[] FlatGroupFields()
	{
		return FixParser.ParseFields(groups);
	}

	[Benchmark]
	public FixMessage Heartbeat()
	{
		return FixParser.ParseMessage(heartbeat);
	}

	[Benchmark]
	public FixMessage NewOrderSingle()
	{
		return FixParser.ParseMessage(order);
	}

	[Benchmark]
	public FixMessage LargeRawData()
	{
		return FixParser.ParseMessage(raw);
	}

	[Benchmark]
	public FixMessage RepeatingGroups()
	{
		return FixParser.ParseMessage(groups);
	}

	[Benchmark]
	public FixMessage NewOrderSingleBytes()
	{
		using var input = new MemoryStream(orderBytes, writable: false);
		return FixParser.ReadMessage(input);
	}
	[Benchmark]
	public FixMessage LargeRawDataBytes()
	{
		using var input = new MemoryStream(rawBytes, writable: false);
		return FixParser.ReadMessage(input);
	}

	internal static string Wire(string type, string fields)
	{
		var body = "35=" + type + "|49=S|56=T|34=1|52=20260915-12:00:00|" + fields;
		body = body.Replace('|', '\u0001');
		var wire = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var sum = 0;
		foreach (var c in wire) sum = (sum + c) & 255;
		return wire + "10=" + sum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
