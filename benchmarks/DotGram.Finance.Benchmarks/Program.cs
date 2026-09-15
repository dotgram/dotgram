using System;
using System.Globalization;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Running;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

static class Program
{
	static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,
		DefaultConfig.Instance.AddColumn(StatisticColumn.OperationsPerSecond));
}

[MemoryDiagnoser]
public class Fix44Benchmarks
{
	string heartbeat = "";
	string order = "";
	string raw = "";
	string groups = "";

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
		foreach (var input in new[] { heartbeat, order, raw, groups }) Fix44.Parse(input);
	}

	[Benchmark] public FixMessage Heartbeat() => Fix44.Parse(heartbeat);
	[Benchmark] public FixMessage NewOrderSingle() => Fix44.Parse(order);
	[Benchmark] public FixMessage LargeRawData() => Fix44.Parse(raw);
	[Benchmark] public FixMessage RepeatingGroups() => Fix44.Parse(groups);

	static string Wire(string type, string fields)
	{
		var body = "35=" + type + "|49=S|56=T|34=1|52=20260915-12:00:00|" + fields;
		body = body.Replace('|', '\u0001');
		var wire = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var sum = 0;
		foreach (var c in wire) sum = (sum + c) & 255;
		return wire + "10=" + sum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
