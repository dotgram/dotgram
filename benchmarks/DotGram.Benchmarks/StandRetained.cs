using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// What each side's parser HOLDS after it has read a row's input: the retained bytes, beside the bytes a call allocates that a pair prints (architect, D97: a change to what a pool keeps has two sides, what a call
	/// no longer allocates and what the pool now keeps resident, and a pair that sees one cannot price the trade). For each row and each side, in a process of its own (see Stand-Retained.ps1: the pools are
	/// thread-static and shared by every row of a process): a full collection, the managed heap, the row's reading once (the room the parse needed: on a thread that has never parsed it is exactly the store that parse grew;
	/// performance-ff, e1c2e8f1), collected and measured; once more (the peak); then eight small parses of the same parser and measured, and eight more and measured (a store released once as designed, or released and
	/// parked again by something). Each figure is a difference against the heap just before it, so the sides are read one after the other and never against a fixed base. The ratio of the peak to the first
	/// retention is what condition 2 of that commit is read from; the first retention also holds whatever else a thread keeps after a parse, so the ratio is a little flattering near 1, and the same on every side.
	/// Times nothing: a garbage collection is the cost, and a machine that is loaded changes nothing in a byte count.
	/// </summary>
	public static void PairedRetained(string[] directories, string? only)
	{
		var sides = directories.Select(directory => (Directory: directory, Workloads: PairedWorkloadsOf(directory))).ToArray();

		Console.WriteLine($"Retained after a parse, {DateTime.Now:yyyy-MM-dd HH:mm}. Built from {BinaryCommit()}. One row per process, each side in turn.");
		Console.WriteLine();
		Console.WriteLine("| row | side | after one parse KB | after two KB | two / one | bytes a call | after eight small parses KB | after sixteen KB |");
		Console.WriteLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |");

		foreach (var id in sides[0].Workloads.Where(one => only is null || Matches(one.Id, only)).Select(static one => one.Id))
		{
			foreach (var (directory, workloads) in sides)
			{
				var workload = workloads.FirstOrDefault(one => one.Id == id);
				var big      = workload?.Readings.FirstOrDefault(one => one.Name == "before")?.Run;

				if (workload is null || big is null)
					continue;

				// Something small of the same parser, read after the big parse: a slot that parks a store demotes it after eight idle rentals.
				var smallId = workload.Family == "el" ? "el/ladder" : "sql/select1";
				var small   = workloads.FirstOrDefault(one => one.Id == smallId)?.Readings.FirstOrDefault(one => one.Name == "before")?.Run;
				var held    = Retained(big, small);

				Console.WriteLine(string.Create(System.Globalization.CultureInfo.InvariantCulture,
					$"| {id} | {System.IO.Path.GetFileName(directory)} | {held.One / 1024.0:N1} | {held.Two / 1024.0:N1} | {(held.One > 0 ? (held.Two / (double)held.One).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) : "-")} | {held.Allocated:N0} | {held.Eight / 1024.0:N1} | {held.Sixteen / 1024.0:N1} |"));
			}
		}
	}

	/// <summary>One side's workloads, its own reading standing for both sides of the pair (only the "before" reading is read).</summary>
	static Workload[] PairedWorkloadsOf(string directory)
	{
		PairedFixContent(directory, directory);

		return PairedWorkloads(new PairedSide("side", directory), new PairedSide("side-again", directory));
	}

	sealed record Kept(long One, long Two, long Allocated, long Eight, long Sixteen);

	/// <summary>The bytes that stay reachable after a reading has run once, twice, and after eight and sixteen further small parses.</summary>
	static Kept Retained(Func<int> run, Func<int>? small)
	{
		Collect();

		var before = GC.GetTotalMemory(true);

		run();
		Collect();

		var one = Math.Max(0, GC.GetTotalMemory(true) - before);

		var allocated = GC.GetAllocatedBytesForCurrentThread();

		run();

		allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;

		Collect();

		var two = Math.Max(0, GC.GetTotalMemory(true) - before);

		if (small is null)
			return new Kept(one, two, allocated, -1, -1);

		for (var i = 0; i < 8; i++)
			small();

		Collect();

		var eight = Math.Max(0, GC.GetTotalMemory(true) - before);

		for (var i = 0; i < 8; i++)
			small();

		Collect();

		return new Kept(one, two, allocated, eight, Math.Max(0, GC.GetTotalMemory(true) - before));
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
