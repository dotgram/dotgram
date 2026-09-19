using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

	public static void Repeat(string? directory, bool rebuild, string? only, string? against, int count)
	{
		var output = directory ?? DefaultDirectory();
		var root   = Root();

		Directory.CreateDirectory(output);

		for (var i = 1; i <= count; i++)
		{
			var arguments = new List<string> { "--stand", Path.Combine(output, $"run-{i}") };

			if (rebuild && i == 1)
				arguments.Add("--rebuild");

			if (only is not null)
				arguments.AddRange(["--only", only]);

			Console.WriteLine($"=== run {i} of {count}, {DateTime.Now:HH:mm:ss}");
			RunAgain(arguments);
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
	public static void RepeatPaired(string beforeDir, string afterDir, string? directory, string? only, int count)
	{
		var output = directory ?? DefaultDirectory();

		Directory.CreateDirectory(output);

		for (var i = 1; i <= count; i++)
		{
			var arguments = new List<string> { "--stand-paired", beforeDir, afterDir, Path.Combine(output, $"run-{i}") };

			if (only is not null)
				arguments.AddRange(["--only", only]);

			Console.WriteLine($"=== run {i} of {count}, {DateTime.Now:HH:mm:ss}");
			RunAgain(arguments);
		}

		var runs   = Enumerable.Range(1, count)
			.Select(i => JsonSerializer.Deserialize<Taken>(File.ReadAllText(Path.Combine(output, $"run-{i}", "paired.json")), Json)!)
			.ToArray();
		var pooled = Pool(runs, output);
		var report = pooled.Note + PairedMarkdown(true, pooled.Control, pooled.Rows);

		File.WriteAllText(Path.Combine(output, "paired.json"), JsonSerializer.Serialize(new Taken(pooled.Control, pooled.Rows), Json));
		File.WriteAllText(Path.Combine(output, "paired.md"), report);

		Console.WriteLine();
		Console.WriteLine(report);
		Console.WriteLine($"Written to {output}");
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
	static void RunAgain(List<string> arguments)
	{
		var path  = Environment.ProcessPath!;
		var start = new ProcessStartInfo(path) { UseShellExecute = false };

		// Started as `dotnet Program.dll`, the host is dotnet and the program is its first argument.
		if (string.Equals(Path.GetFileNameWithoutExtension(path), "dotnet", StringComparison.OrdinalIgnoreCase))
			start.ArgumentList.Add(typeof(Stand).Assembly.Location);

		foreach (var argument in arguments)
			start.ArgumentList.Add(argument);

		using var process = Process.Start(start)!;

		process.WaitForExit();

		if (process.ExitCode != 0)
			throw new InvalidOperationException($"A child run ({string.Join(' ', arguments)}) exited with {process.ExitCode}.");
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
		text.AppendLine($"First call in a fresh process, median of {runs}, in ms. The hand reading is this build's own.");
		text.AppendLine();
		text.AppendLine("| row | reading | ms |");
		text.AppendLine("| --- | --- | ---: |");

		foreach (var workload in workloads)
		{
			var taken = workload.Readings.Select(_ => new List<double>()).ToArray();

			for (var run = 0; run < runs; run++)
				for (var k = 0; k < workload.Readings.Length; k++)
				{
					var i = run % 2 == 0 ? k : workload.Readings.Length - 1 - k;

					taken[i].Add(PairedChild(beforeDir, afterDir, workload.Id, workload.Readings[i].Name));
				}

			for (var i = 0; i < workload.Readings.Length; i++)
				text.AppendLine(CultureInfo.InvariantCulture, $"| {workload.Id} | {workload.Readings[i].Name} | {Median(taken[i]):F2} |");
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
		var watch     = Stopwatch.StartNew();

		_sink = run.Run();

		Console.WriteLine(watch.Elapsed.TotalMilliseconds.ToString("R", CultureInfo.InvariantCulture));
	}

	static double PairedChild(string beforeDir, string afterDir, string id, string reading)
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

		return double.Parse(output.Trim().Split('\n')[^1], CultureInfo.InvariantCulture);
	}
}
