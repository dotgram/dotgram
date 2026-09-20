using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace DotGram.Benchmarks;

// `--repeat N`: a run taken N times, each in a process of its own, and the median of what came
// out (architect for Igor, 2026-09-18). One run of the stand, even on a quiet machine, spreads
// 4-16% from the next, and on a machine other people build on a single run can be 85% off; the
// median of five is within a few per cent of the quiet one either way. So a before and an after
// are quoted as medians of at least five, and a run whose control moved more than 5% from the
// medians' is dropped and said so, since that is the one thing a run can ask about itself.

static partial class Stand
{
	/// <summary>How far a run's control may be from the median of the runs' controls and still be kept.</summary>
	const double ControlTolerance = 0.05;

	/// <summary>Fewer clean runs than this and the median is not one to quote: the run refuses to say it is.</summary>
	const int MinimumKept = 3;

	/// <summary>What a run of the paired stand leaves for the runs to be merged from.</summary>
	sealed record Taken(double Control, Row[] Rows);

	/// <summary>The runs' medians, and what to say about which runs were used.</summary>
	sealed record Pooled(Row[] Rows, double Control, int[] Kept, string Note);

	public static void Repeat(string? directory, bool rebuild, string? only, string? against, int count, double? limitMinutes = null)
	{
		var output   = directory ?? DefaultDirectory();
		var root     = Root();
		var deadline = Deadline(limitMinutes);

		Directory.CreateDirectory(output);

		for (var i = 1; i <= count; i++)
		{
			var arguments = new List<string> { "--stand", Path.Combine(output, $"run-{i}") };

			if (rebuild && i == 1)
				arguments.Add("--rebuild");

			if (only is not null)
				arguments.AddRange(["--only", only]);

			if (!Within(deadline, i, count, ref count))
				break;

			Console.WriteLine($"=== run {i} of {count}, {DateTime.Now:HH:mm:ss}");

			if (!RunAgain(arguments, deadline))
			{
				count = i - 1;

				Console.WriteLine($"=== the limit of {limitMinutes} minutes came inside run {i}: it was stopped, {count} runs are used");

				break;
			}
		}

		var runs   = Enumerable.Range(1, count)
			.Select(i => JsonSerializer.Deserialize<Result>(File.ReadAllText(Path.Combine(output, $"run-{i}", "stand.json")), Json)!)
			.ToArray();
		var pooled = Pool([.. runs.Select(static one => new Taken(one.Control, one.Rows))], output);
		var first  = runs[pooled.Kept[0]];
		var merged = first with { Control = pooled.Control, Rows = pooled.Rows };
		var report = pooled.Note + Markdown(merged) + (only is null ? GenerationGate(merged, root, against) : "");

		File.WriteAllText(Path.Combine(output, "stand.json"), JsonSerializer.Serialize(merged, Json));
		File.WriteAllText(Path.Combine(output, "stand.md"), report);

		Console.WriteLine();
		Console.WriteLine(report);
		Console.WriteLine($"Written to {output}");
	}

	/// <summary>The paired stand taken <paramref name="count"/> times, and the medians of it.</summary>
	public static void RepeatPaired(string beforeDir, string afterDir, string? directory, string? only, int count, double? limitMinutes = null)
	{
		var output   = directory ?? DefaultDirectory();
		var deadline = Deadline(limitMinutes);

		Directory.CreateDirectory(output);

		for (var i = 1; i <= count; i++)
		{
			var arguments = new List<string> { "--stand-paired", beforeDir, afterDir, Path.Combine(output, $"run-{i}") };

			if (only is not null)
				arguments.AddRange(["--only", only]);

			if (!Within(deadline, i, count, ref count))
				break;

			Console.WriteLine($"=== run {i} of {count}, {DateTime.Now:HH:mm:ss}");

			if (!RunAgain(arguments, deadline))
			{
				count = i - 1;

				Console.WriteLine($"=== the limit of {limitMinutes} minutes came inside run {i}: it was stopped, {count} runs are used");

				break;
			}
		}

		var runs   = Enumerable.Range(1, count)
			.Select(i => JsonSerializer.Deserialize<Taken>(File.ReadAllText(Path.Combine(output, $"run-{i}", "paired.json")), Json)!)
			.ToArray();
		var pooled = Pool(runs, output);
		var report = pooled.Note + PairedMarkdown(true, pooled.Control, pooled.Rows, SideNote(beforeDir, afterDir));

		File.WriteAllText(Path.Combine(output, "paired.json"), JsonSerializer.Serialize(new Taken(pooled.Control, pooled.Rows), Json));
		File.WriteAllText(Path.Combine(output, "paired.md"), report);

		Console.WriteLine();
		Console.WriteLine(report);
		Console.WriteLine($"Written to {output}");
	}

	static DateTime? Deadline(double? minutes) => minutes is { } limit ? DateTime.Now.AddMinutes(limit) : null;

	/// <summary>Whether another run may begin: the limit has not come, and there is time for one as long as the last (unknown: half the limit's remainder).</summary>
	static bool Within(DateTime? deadline, int run, int planned, ref int count)
	{
		if (deadline is { } end && DateTime.Now >= end)
		{
			count = run - 1;

			Console.WriteLine($"=== the limit has come before run {run} of {planned}: {count} runs are used");

			return false;
		}

		return true;
	}

	static Pooled Pool(Taken[] runs, string output)
	{
		var controls = runs.Select(static one => one.Control).ToList();
		var middle   = Median(controls);
		var keep     = Enumerable.Range(0, runs.Length).Where(i => Math.Abs(runs[i].Control / middle - 1) <= ControlTolerance).ToArray();
		var dropped  = Enumerable.Range(0, runs.Length).Except(keep).ToArray();
		var listed   = string.Join(", ", controls.Select(static one => one.ToString("F1", CultureInfo.InvariantCulture)));

		if (keep.Length < MinimumKept)
			throw new InvalidOperationException(
				$"Only {keep.Length} of {runs.Length} runs kept a control within {ControlTolerance:P0} of {middle:F1} ns ({listed}): " +
				$"the machine was not quiet enough for a median to be quoted. The runs are in {output}.");

		var kept    = keep.Select(i => runs[i]).ToArray();
		var control = Median([.. kept.Select(static one => one.Control)]);
		var rows    = kept[0].Rows.Select(row => Merge(row, kept)).ToArray();

		var note = new StringBuilder();

		note.AppendLine(CultureInfo.InvariantCulture,
			$"Median of {keep.Length} of {runs.Length} runs, each in a process of its own; control {control:F1} ns (the runs' controls: {listed}).");
		note.AppendLine(dropped.Length == 0
			? "No run was dropped."
			: $"Dropped for a control more than {ControlTolerance:P0} off the median: run {string.Join(", ", dropped.Select(static i => i + 1))}.");
		note.AppendLine("The spread column is the spread of the base reading between runs, not between rounds.");
		note.AppendLine();

		return new Pooled(rows, control, keep, note.ToString());
	}

	/// <summary>One row with each reading's median over the runs, and the base's run-to-run spread.</summary>
	static Row Merge(Row row, Taken[] runs)
	{
		var others = runs.Select(one => Array.Find(one.Rows, other => other.Id == row.Id) ?? throw new InvalidOperationException($"{row.Id} is missing from a run.")).ToArray();

		var readings = row.Readings.Select((reading, index) => new Timed(
			reading.Reading,
			Median([.. others.Select(other => other.Readings[index].Nanoseconds)]),
			Median([.. others.Select(other => other.Readings[index].Bytes)]),
			reading.Warmup,
			[])).ToArray();

		var baseline = others.Select(static other => other.Readings[0].Nanoseconds).ToArray();

		return new Row(row.Id, readings, (baseline.Max() - baseline.Min()) / Median([.. baseline]));
	}

	/// <summary>Runs this program again with these arguments, as a process of its own, and waits.</summary>
	static bool RunAgain(List<string> arguments, DateTime? deadline = null)
	{
		var path  = Environment.ProcessPath!;
		var start = new ProcessStartInfo(path) { UseShellExecute = false };

		// Started as `dotnet Program.dll`, the host is dotnet and the program is its first argument.
		if (string.Equals(Path.GetFileNameWithoutExtension(path), "dotnet", StringComparison.OrdinalIgnoreCase))
			start.ArgumentList.Add(typeof(Stand).Assembly.Location);

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		using var process = Process.Start(start)!;

		if (deadline is { } end)
		{
			var remaining = end - DateTime.Now;

			if (remaining <= TimeSpan.Zero || !process.WaitForExit(remaining))
			{
				process.Kill(true);
				process.WaitForExit();

				return false;
			}
		}
		else
			process.WaitForExit();

		if (process.ExitCode != 0)
			throw new InvalidOperationException($"A child run ({string.Join(' ', arguments)}) exited with {process.ExitCode}.");

		return true;
	}

	// ── First calls of the paired stand ─────────────────────────────────────────

	/// <summary>
	/// The first call of each reading of each row of the paired stand, in a fresh process each,
	/// median of five: what a build costs the first time something asks it a question, before
	/// and after. Nothing else is timed, so it is a run of its own.
	/// </summary>
	public static void PairedFirstCalls(string beforeDir, string afterDir, string? directory, string? only)
	{
		var output = directory ?? DefaultDirectory();

		Directory.CreateDirectory(output);

		var workloads = PairedWorkloads(new PairedSide("before", beforeDir), new PairedSide("after", afterDir));

		if (only is not null)
			workloads = [.. workloads.Where(one => Matches(one.Id, only))];

		const int runs = 5;

		var text = new StringBuilder();

		text.AppendLine(CultureInfo.InvariantCulture, $"# Paired first calls, {DateTime.Now:yyyy-MM-dd HH:mm}");
		text.AppendLine();
		text.AppendLine($"First call in a fresh process, median of {runs}: milliseconds, methods the runtime compiled during it (the median, then the smallest and the largest of the runs: a count of a few dozen moves in steps, and a median of five is then a vote between two values), and the time it spent compiling them (JitInfo, no events). The hand reading is this build's own.");
		text.AppendLine();
		text.AppendLine("| row | reading | ms | methods | JIT ms |");
		text.AppendLine("| --- | --- | ---: | ---: | ---: |");

		foreach (var workload in workloads)
		{
			var taken = workload.Readings.Select(_ => new List<(double Ms, double Methods, double Jit)>()).ToArray();

			for (var run = 0; run < runs; run++)
				for (var k = 0; k < workload.Readings.Length; k++)
				{
					var i = run % 2 == 0 ? k : workload.Readings.Length - 1 - k;

					taken[i].Add(PairedChild(beforeDir, afterDir, workload.Id, workload.Readings[i].Name));
				}

			for (var i = 0; i < workload.Readings.Length; i++)
				text.AppendLine(CultureInfo.InvariantCulture,
					$"| {workload.Id} | {workload.Readings[i].Name} | {Median([.. taken[i].Select(static one => one.Ms)]):F2} | {Median([.. taken[i].Select(static one => one.Methods)]):F0} ({taken[i].Min(static one => one.Methods):F0}-{taken[i].Max(static one => one.Methods):F0}) | {Median([.. taken[i].Select(static one => one.Jit)]):F2} |");
		}

		File.WriteAllText(Path.Combine(output, "paired-first.md"), text.ToString());
		Console.WriteLine(text);
		Console.WriteLine($"Written to {output}");
	}

	/// <summary>The child a paired first call is taken in: builds both sides, then calls one reading once.</summary>
	public static void PairedFirst(string beforeDir, string afterDir, string id, string reading)
	{
		Pin();

		var workloads = PairedWorkloads(new PairedSide("before", beforeDir), new PairedSide("after", afterDir));
		var workload  = Array.Find(workloads, one => one.Id == id) ?? throw new ArgumentException($"No row {id}.");
		var run       = Array.Find(workload.Readings, one => one.Name == reading) ?? throw new ArgumentException($"{id} has no reading {reading}.");
		var methods   = JitInfo.GetCompiledMethodCount();
		var compiling = JitInfo.GetCompilationTime();
		var watch     = Stopwatch.StartNew();

		_sink = run.Run();

		var elapsed = watch.Elapsed.TotalMilliseconds;

		Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
			$"{elapsed:R} {JitInfo.GetCompiledMethodCount() - methods} {(JitInfo.GetCompilationTime() - compiling).TotalMilliseconds:R}"));
	}

	static (double Ms, double Methods, double Jit) PairedChild(string beforeDir, string afterDir, string id, string reading)
	{
		var host  = Environment.ProcessPath!;
		var start = new ProcessStartInfo(host) { RedirectStandardOutput = true, UseShellExecute = false };

		if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
			start.ArgumentList.Add(typeof(Stand).Assembly.Location);

		foreach (var argument in new[] { "--stand-paired-first", beforeDir, afterDir, id, reading })
			start.ArgumentList.Add(argument);

		using var process = Process.Start(start)!;
		var       output  = process.StandardOutput.ReadToEnd();

		process.WaitForExit();

		if (process.ExitCode != 0)
			throw new InvalidOperationException($"A paired first call of {id} {reading} exited with {process.ExitCode}.");

		var numbers = output.Trim().Split('\n')[^1].Split(' ');

		return (double.Parse(numbers[0], CultureInfo.InvariantCulture), double.Parse(numbers[1], CultureInfo.InvariantCulture), double.Parse(numbers[2], CultureInfo.InvariantCulture));
	}
}
