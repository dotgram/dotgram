using System;
using System.Globalization;
using System.IO;
using System.Text;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// A replay that goes straight to the turn it is asked for, in every repetition it re-reads: what
/// it answers, what it costs, and what it keeps.
/// </summary>
/// <remarks>Run apart from the other classes, for the reason <see cref="RefusalSpikeTests"/> is.</remarks>
[Collection(nameof(RefusalSpikeAlone))]
public sealed class ResumptionTests(ITestOutputHelper output)
{
	static readonly IrLowering.Options?[] Lowerings = [null, new IrLowering.Options(AllResumable: true)];

	static readonly int[] Small = [250, 500, 1000, 2000, 4000, 8000, 16000, 32000, 64000];

	static string Named(IrLowering.Options? options)
	{
		return options is null ? "proofs" : "all open";
	}

	/// <summary>
	/// What every refusal ladder costs the evaluator without resumption, and the generated C#
	/// parser, written down as the base the rest is measured from. Held to nothing.
	/// </summary>
	[Fact]
	public void Baseline_TheLaddersBeforeResumption()
	{
		var text = new StringBuilder();

		// LADDER=<shape name> narrows the run to one shape.
		var only = Environment.GetEnvironmentVariable("LADDER");

		foreach (var shape in Ladders.Shapes.Where(shape => only is null || shape.Name == only))
		{
			foreach (var options in Lowerings)
				text.Append(Ladders.Table($"{shape.Name}, evaluator without resumption, {Named(options)}", Ladders.Evaluated(shape, options, resumes: false, budget: 30_000_000, Small)));

			// The spike's resumption: only the repetitions the plan leaves open go straight to a turn.
			text.Append(Ladders.Table($"{shape.Name}, evaluator with resumption of open repetitions only, proofs", Ladders.Evaluated(shape, null, resumes: true, budget: 30_000_000, Small)));

			int[] printed = [250, 500, 1000, 2000];

			foreach (var frames in Enum.GetValues<SliceFrames>())
				text.Append(Ladders.Table($"{shape.Name}, TypeScript {frames} without resumption, proofs", Ladders.Printed(shape, null, frames, printed)));

			text.Append(Ladders.Table($"{shape.Name}, C without resumption, proofs", Ladders.Printed(shape, null, null, printed)));
			text.Append("# ").Append(shape.Name).Append(", generated C#\nturns\tms\thost calls\tanswer\n");

			foreach (var (turns, milliseconds, calls, line) in Ladders.Generated(shape, TimeSpan.FromSeconds(1.5)))
				text.Append(string.Create(CultureInfo.InvariantCulture, $"{turns}\t{milliseconds:F1}\t{calls}\t{line}\n"));
		}

		output.WriteLine(text.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "ladders-baseline.tsv"), text.ToString());
	}

	/// <summary>The lowered programs of the fixtures, written out to be read.</summary>
	[Fact]
	public void Programs_AreWrittenOut()
	{
		if (Environment.GetEnvironmentVariable("IR_REPORT") is not { } report)
			return;

		var text = new StringBuilder();

		foreach (var (name, fixture) in Cases.Fixtures)
		{
			if (name == "Json")
				continue;

			var program = Reading.Program(fixture.Grammar);

			text.Append("## ").Append(name).Append(" (plan asked ").Append(program.PlanAsked).Append(", missed ").Append(program.PlanMisses).Append(")\n");
			text.Append(program.Dump(fragmentTexts: false)).Append('\n');
		}

		File.WriteAllText(Path.Combine(report, "programs.txt"), text.ToString());
	}
}
