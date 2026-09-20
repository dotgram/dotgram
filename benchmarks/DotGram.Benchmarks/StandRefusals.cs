using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram.ExpressionLanguage;
using DotGram.Refusals;

namespace DotGram.Benchmarks;

// The audit of the refusal ladders: every series of RefusalLadders.cs, run and printed as a table, with what is not covered under it. The series, the
// budget and the classes are in that file, which DotGram.Tests.Slow reads too (there as a guard held to a baseline); this is the form that shows them all.

static partial class Stand
{
	/// <summary>
	/// Every refusal ladder, the exponent of each, and what was not covered. <paramref name="only"/> keeps the series whose parser or shape contains it;
	/// <paramref name="baseline"/> prints, after the table, the lines of the guard's baseline (the class of every series that may be baselined).
	/// Returns the number of series that are defects or faults of the series, which is what a guard would fail on.
	/// </summary>
	public static int RefusedLinearity(string? only = null, bool baseline = false)
	{
		var invariant = CultureInfo.InvariantCulture;

		Console.WriteLine("Built from " + BinaryCommit() + ": the libraries measured are this build's own (DotGram.Web, DotGram.Sql, DotGram.ExpressionLanguage, DotGram.Finance, the examples).");
		Console.WriteLine();
		Console.WriteLine("Refused inputs: a growing head and a tail no reader can finish; each series asserts that the input is refused. The exponent is the slope of log time on log size over the largest sixteenth of the ladder (at least four points); above " +
			RefusalLadders.Threshold.ToString("F2", invariant) + " a refusal is a defect, from " + RefusalLadders.Quadratic.ToString("F1", invariant) + " quadratic, from " + RefusalLadders.Explosive.ToString("F1", invariant) + " explosive. A ladder ends at the first call over " + RefusalLadders.BudgetMilliseconds + " ms.");
		Console.WriteLine();
		Console.WriteLine("| parser | shape | unit | sizes | points | input chars at the largest | time at the largest | exponent | shape of the growth | projected at 64 KiB | KB a call at the largest | allocation exponent | |");
		Console.WriteLine("| --- | --- | --- | --- | ---: | ---: | ---: | ---: | --- | ---: | ---: | ---: | --- |");

		// The stand can reach the immediate reading of the expression language through the internals; the slow suite cannot, and reads the tape alone.
		if (RefusalLadders.ExpressionForms.All(static one => one.Form != "immediate"))
			RefusalLadders.ExpressionForms.Add(("immediate", static text => ExpressionParser.Immediate.TryParseLambda(text, new ExpressionParser.State(Caller) { Text = text }).IsSuccess));

		var problems = new List<string>();
		var lines    = new List<string>();
		var series   = 0;

		foreach (var one in RefusalLadders.All())
		{
			if (only is not null && !one.Parser.Contains(only, StringComparison.OrdinalIgnoreCase) && !one.Shape.Contains(only, StringComparison.OrdinalIgnoreCase))
				continue;

			series++;

			var result   = RefusalLadders.Run(one);
			var exponent = result.Exponent;
			var last     = result.Last;
			var stopped  = result.Budget ? ", stopped at " + last.N.ToString("N0", invariant) + " (" + (last.Ns / 1e6).ToString("F0", invariant) + " ms)" : "";

			var flag = result.Thrown is not null ? "THREW " + result.Thrown
				: result.Hung ? "EXPLOSIVE: a call at " + last.N.ToString("N0", invariant) + " " + one.Unit + " had not finished in " + RefusalLadders.WatchdogMilliseconds + " ms"
				: result.Faulty ? "SERIES FAULT: the input is accepted"
				: double.IsNaN(exponent) ? "no curve"
				: result.Class == RefusalLadders.Class.Explosive ? "EXPLOSIVE" + stopped
				: result.Class == RefusalLadders.Class.Quadratic ? "QUADRATIC" + stopped
				: result.Class == RefusalLadders.Class.Superlinear ? "SUPERLINEAR" + stopped
				: result.Budget ? "large and linear" + stopped
				: "";

			// What a defect costs at the size an attacker sends: the time at the largest size carried out along the exponent to 64 KiB of input.
			var projected = result.Hung || result.Class == RefusalLadders.Class.Explosive ? "explosive"
				: result.Length == 0 || double.IsNaN(exponent) ? "-"
				: RefusalLadders.Duration(last.Ns * Math.Pow(65536.0 / result.Length, Math.Max(exponent, 1)));

			if (flag.Length > 0 && !flag.StartsWith("large and linear", StringComparison.Ordinal))
				problems.Add($"{one.Parser}, {one.Shape}: {flag}" + (double.IsNaN(exponent) ? "" : $", time exponent {exponent.ToString("F2", invariant)}, {result.Shape}, {projected} at 64 KiB"));

			// Only what may be held to a baseline: an explosion, a series that is not refused and one that threw are never written down as normal.
			lines.Add(result.Thrown is not null || result.Faulty || result.Class == RefusalLadders.Class.Explosive || double.IsNaN(exponent)
				? $"# not baselined ({flag}): {RefusalLadders.Id(one)}"
				: $"{RefusalLadders.Id(one)} = {result.Class}");

			Console.WriteLine(string.Create(invariant,
				$"| {one.Parser} | {one.Shape} | {one.Unit} | {(result.Points.Count == 0 ? "-" : result.Points[0].N.ToString("N0", invariant) + " - " + last.N.ToString("N0", invariant))} | {result.Points.Count} | {result.Length:N0} | " +
				$"{(double.IsNaN(last.Ns) ? "-" : RefusalLadders.Duration(last.Ns))} | {(double.IsNaN(exponent) ? "-" : exponent.ToString("F2", invariant))} | {result.Shape} | {projected} | " +
				$"{(double.IsNaN(last.Bytes) ? "-" : (last.Bytes / 1024).ToString("F1", invariant))} | {(double.IsNaN(result.Allocation) ? "-" : result.Allocation.ToString("F2", invariant))} | {flag} |"));

			// With a filter the reader is looking at one curve: give every point of it, time and bytes a call, and what each costs per unit of the head.
			if (only is not null)
			{
				foreach (var point in result.Points)
					Console.WriteLine(string.Create(invariant, $"    {point.N,8:N0} {one.Unit,-12} {point.Ns / 1000.0,12:F2} us {point.Bytes / 1024.0,10:F2} KB   {point.Ns / point.N,10:F1} ns per {one.Unit}   {point.Bytes / point.N,10:F1} B per {one.Unit}"));
			}

			Console.Out.Flush();
		}

		Console.WriteLine();
		Console.WriteLine(problems.Count == 0
			? $"{series} series, none above the exponent {RefusalLadders.Threshold.ToString("F2", invariant)}."
			: $"{series} series, {problems.Count} defective or faulty:\n" + string.Join("\n", problems.Select(static one => "- " + one)));
		Console.WriteLine();
		Console.WriteLine("Not covered by a refusal ladder:");

		foreach (var gap in RefusalLadders.Gaps)
			Console.WriteLine("- " + gap);

		if (baseline)
		{
			Console.WriteLine();
			Console.WriteLine("Baseline, as the guard reads it (one series a line, `parser | shape = class`; a series that is an explosion, not refused or threw is commented out and fails the guard until it is fixed):");

			foreach (var line in lines)
				Console.WriteLine(line);
		}

		return problems.Count;
	}
}
