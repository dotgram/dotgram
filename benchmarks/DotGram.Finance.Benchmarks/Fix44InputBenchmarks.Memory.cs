using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

namespace DotGram.Finance.Benchmarks;

public partial class Fix44InputBenchmarks
{
	// Run one case per fresh process. Sampling is deliberately separate from timing benchmarks.
	[MethodImpl(MethodImplOptions.NoInlining)]
	void PrepareMemoryInput(string input)
	{
		if (input != "String" && input != "Characters" && input != "Bytes") throw new ArgumentException("Expected String, Characters or Bytes.", nameof(input));

		PrepareMessage();

		if (input == "Characters")
		{
			var builder = new StringBuilder(message.Length * count);
			for (var i = 0; i < count; i++) builder.Append(message);
			corpus = builder.ToString();
		}
		else if (input == "Bytes")
		{
			bytes = new byte[message.Length * count];
			for (var i = 0; i < bytes.Length; i++) bytes[i] = checked((byte)message[i % message.Length]);
		}

		// Warm the selected parsing path without processing or retaining a whole batch.
		for (var i = 0; i < 16; i++)
		{
			if (input == "String") FixParser.ParseMessage(message);
			else if (input == "Characters")
			{
				using var reader = new StringReader(message);
				foreach (var parsed in FixParser.ReadMessages(reader)) GC.KeepAlive(parsed);
			}
			else
			{
				using var stream = new MemoryStream(bytes, 0, message.Length, false);
				foreach (var parsed in FixParser.ReadMessages(stream)) GC.KeepAlive(parsed);
			}
		}

	}

	public void MeasureMemory(string input)
	{
		PrepareMemoryInput(input);
		using var process = Process.GetCurrentProcess();
		using var start = new ManualResetEventSlim();
		using var stop = new ManualResetEventSlim();
		long peakManaged = 0, peakWorking = 0, peakPrivate = 0;
		var samples = 0;
		void Sample()
		{
			peakManaged = Math.Max(peakManaged, GC.GetTotalMemory(false));
			process.Refresh();
			peakWorking = Math.Max(peakWorking, process.WorkingSet64);
			peakPrivate = Math.Max(peakPrivate, process.PrivateMemorySize64);
			samples++;
		}
		var sampler = new Thread(() =>
		{
			start.Wait();
			do { Sample(); } while (!stop.Wait(1));
			Sample();
		});
		sampler.Start();
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		var baselineManaged = GC.GetTotalMemory(false);
		process.Refresh();
		var baselineWorking = process.WorkingSet64;
		var baselinePrivate = process.PrivateMemorySize64;
		peakManaged = baselineManaged;
		peakWorking = baselineWorking;
		peakPrivate = baselinePrivate;
		var allocatedStart = GC.GetAllocatedBytesForCurrentThread();
		long total;
		start.Set();
		try { total = input == "String" ? String() : input == "Characters" ? Characters() : Bytes(); }
		finally { stop.Set(); sampler.Join(); }
		var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedStart;
		if (total != (long)message.Length * count) throw new InvalidOperationException("Incomplete batch.");
		var retainedManaged = GC.GetTotalMemory(true);
		GC.KeepAlive(this);
		Console.WriteLine(JsonSerializer.Serialize(new
		{
			Workload, Input = input, Count = count, MessageOctets = message.Length, BatchOctets = total,
			InputPayloadBytes = (long)message.Length * 2 + (long)corpus.Length * 2 + bytes.Length,
			BaselineManaged = baselineManaged, PeakManaged = peakManaged, RetainedManaged = retainedManaged,
			BaselineWorking = baselineWorking, PeakWorking = peakWorking,
			BaselinePrivate = baselinePrivate, PeakPrivate = peakPrivate,
			Allocated = allocated, Samples = samples,
		}));
	}
}
