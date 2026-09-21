using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// What a side's parser HOLDS after it has read a row's input: the retained bytes, beside the bytes a call allocates that a pair prints (architect, D97: a change to what a pool keeps has two sides, what a call
	/// no longer allocates and what the pool now keeps resident, and a pair that sees one cannot price the trade). For each row and each of the two sides in turn: a full collection, the managed heap, the row's
	/// reading twice (the second call is the one that finds the pool at its size), a full collection, the managed heap again; the difference is what stayed reachable, which for a parser is its static pools.
	/// The two sides run one after the other in one process and their retentions add up in the heap, so each side's figure is a difference against the heap just before it, never against a fixed base.
	/// Times nothing: a garbage collection is the cost, and a machine that is loaded changes nothing in a byte count.
	/// </summary>
	public static void PairedRetained(string beforeDir, string afterDir, string? only)
	{
		PairedFixContent(beforeDir, afterDir);

		var workloads = PairedWorkloads(new PairedSide("before", beforeDir), new PairedSide("after", afterDir))
			.Where(one => only is null || Matches(one.Id, only))
			.ToArray();

		Console.WriteLine($"Retained after a parse, {DateTime.Now:yyyy-MM-dd HH:mm}. Built from {BinaryCommit()}. before = {beforeDir}, after = {afterDir}.");
		Console.WriteLine();
		Console.WriteLine("| row | reading | retained before KB | retained after KB | difference KB | bytes a call before | bytes a call after |");
		Console.WriteLine("| --- | --- | ---: | ---: | ---: | ---: | ---: |");

		foreach (var workload in workloads)
		{
			var before = workload.Readings.FirstOrDefault(one => one.Name == "before");
			var after  = workload.Readings.FirstOrDefault(one => one.Name == "after");

			if (before is null || after is null)
				continue;

			var (heldBefore, allocatedBefore) = Retained(before.Run);
			var (heldAfter, allocatedAfter)   = Retained(after.Run);

			Console.WriteLine(string.Create(System.Globalization.CultureInfo.InvariantCulture,
				$"| {workload.Id} | generated | {heldBefore / 1024.0:N1} | {heldAfter / 1024.0:N1} | {(heldAfter - heldBefore) / 1024.0:+#,##0.0;-#,##0.0;0.0} | {allocatedBefore:N0} | {allocatedAfter:N0} |"));
		}
	}

	/// <summary>The bytes that stay reachable after a reading has run twice, and the bytes the second call allocated.</summary>
	static (long Held, long Allocated) Retained(Func<int> run)
	{
		Collect();

		var before = GC.GetTotalMemory(true);

		run();

		var allocated = GC.GetAllocatedBytesForCurrentThread();

		run();

		allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;

		Collect();

		return (Math.Max(0, GC.GetTotalMemory(true) - before), allocated);
	}

	static void Collect()
	{
		for (var pass = 0; pass < 3; pass++)
		{
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
			GC.WaitForPendingFinalizers();
		}
	}
}
