using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using DotGram.Finance.Fix44;
using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

/// <summary>Full strict parsing with identical owned message results; transport data is prepared outside timing.</summary>
[MemoryDiagnoser]
public partial class Fix44InputBenchmarks
{
	[Params("Orders", "RawData", "Groups")]
	public string Workload { get; set; } = "Orders";
	string message = "";
	string corpus = "";
	byte[] bytes = Array.Empty<byte>();
	int count;

	void PrepareMessage()
	{
		switch (Workload)
		{
			case "Orders":
				count = 100000;
				message = Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
				break;
			case "RawData":
				count = 64;
				message = Fix44Benchmarks.Wire("A", "98=0|108=30|95=1048576|96=" + new string('X', 1048576) + "|");
				break;
			case "Groups":
				count = 1000;
				var body = new StringBuilder("55=ABC|262=REQ|268=1000|");
				for (var i = 0; i < 1000; i++) body.Append("269=0|270=12.50|271=100|");
				message = Fix44Benchmarks.Wire("W", body.ToString());
				break;
			default: throw new InvalidOperationException(Workload);
		}
	}

	[GlobalSetup]
	public void Setup()
	{
		PrepareMessage();
		var builder = new StringBuilder(message.Length * count);
		for (var i = 0; i < count; i++) builder.Append(message);
		corpus = builder.ToString();
		bytes = new byte[corpus.Length];
		for (var i = 0; i < bytes.Length; i++) bytes[i] = checked((byte)corpus[i]);
		var expected = String();
		if (Characters() != expected || Bytes() != expected) throw new InvalidOperationException("Input results differ.");
		var type = FixParser.ParseMessage(message).GetType();
		foreach (var parsed in FixParser.ReadMessages(new StringReader(message)))
			if (parsed.OriginalWire != message || parsed.GetType() != type) throw new InvalidOperationException("Character result differs.");
		using var one = new MemoryStream(bytes, 0, message.Length, false);
		var result = FixParser.ReadMessage(one);
		if (result.OriginalWire != message || result.GetType() != type) throw new InvalidOperationException("Byte result differs.");
		Console.WriteLine($"FIX corpus: {Workload}, {count} messages, {message.Length} octets/message, {corpus.Length} octets total.");
	}

	[Benchmark(Baseline = true)]
	public long String()
	{
		long total = 0;
		for (var i = 0; i < count; i++) total += FixParser.ParseMessage(message).OriginalWire.Length;
		return total;
	}

	[Benchmark]
	public long Characters()
	{
		using var input = new StringReader(corpus);
		long total = 0;
		foreach (var parsed in FixParser.ReadMessages(input)) total += parsed.OriginalWire.Length;
		return total;
	}

	[Benchmark]
	public long Bytes()
	{
		using var input = new MemoryStream(bytes, false);
		long total = 0;
		foreach (var parsed in FixParser.ReadMessages(input)) total += parsed.OriginalWire.Length;
		return total;
	}
}
