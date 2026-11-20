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

	static readonly IrLowering.Options AllOpen = new(AllResumable: true);

	/// <summary>The shapes held to their length, and the lowering each is held under; the rest are written down only.</summary>
	static readonly (string Shape, IrLowering.Options? Options)[] Linear =
	[
		("loop", null), ("settled config", null), ("nested settled", null), ("arithmetic", AllOpen),
		("(a) settled", null), ("(b) sealed", null), ("(c) called", null),
		("(a') failed turn", null), ("nested braces", null), ("long look", null), ("braces with captures", null),
		("(e) deciding nest", null), ("back-reference", null), ("yaml line", null),
	];

	/// <summary>
	/// A refused input whose replays read again, before the decision each is asked to change,
	/// repetitions, braces, looks, recursions and texts read by reference costs its length once a
	/// replay goes straight past what it read before: twice the turns is twice the statements, from
	/// 16,000 turns to 64,000, and the answer is the generated parser's.
	/// </summary>
	[Fact]
	public void EveryLadderCostsItsLength()
	{
		int[] sizes = [1000, 2000, 4000, 8000, 16000, 32000, 64000];

		var text   = new StringBuilder();
		var failed = new List<string>();
		var report = Environment.GetEnvironmentVariable("IR_REPORT");
		var only   = Environment.GetEnvironmentVariable("LADDER");

		if (report is not null)
			File.WriteAllText(Path.Combine(report, "ladders-resumed.tsv"), "");

		foreach (var shape in Ladders.Shapes.Where(shape => only is null || shape.Name == only))
		{
			foreach (var options in Lowerings)
			{
				var held   = Linear.Any(one => one.Shape == shape.Name && one.Options == options);
				var rungs  = Ladders.Evaluated(shape, options, resumes: true, budget: held ? 400_000_000 : 40_000_000, sizes);
				var whole  = rungs.Where(static rung => !rung.Over).ToList();
				var ratios = whole.Count >= 3 ? (whole[^1].Steps / (double)whole[^2].Steps, whole[^2].Steps / (double)whole[^3].Steps) : (0, 0);

				var table = Ladders.Table(string.Create(CultureInfo.InvariantCulture,
					$"{shape.Name}, evaluator resuming, {Named(options)}{(held ? ", held" : "")}: ratios {ratios.Item2:F2} {ratios.Item1:F2}"), rungs);

				text.Append(table);

				if (report is not null)
					File.AppendAllText(Path.Combine(report, "ladders-resumed.tsv"), table);

				if (held && (whole.Count < sizes.Length || ratios.Item1 is < 1.9 or > 2.1 || ratios.Item2 is < 1.9 or > 2.1))
					failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} ({Named(options)}): {ratios.Item2:F2} {ratios.Item1:F2}, {whole.Count} rungs"));

				// And the host is called as often as the input is long, not as often as it is read again.
				if (held && whole.Count == sizes.Length && whole[^3].HostCalls > 0 &&
					(whole[^1].HostCalls > 2.1 * whole[^2].HostCalls || whole[^2].HostCalls > 2.1 * whole[^3].HostCalls))
					failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} ({Named(options)}): host calls {whole[^3].HostCalls} {whole[^2].HostCalls} {whole[^1].HostCalls}"));
			}

			// The answers at the sizes the generated parser reads quickly.
			var fixture = Cases.Fixtures[shape.Fixture];

			foreach (var turns in new[] { 1000, 2000 })
			{
				var (theirs, _, _) = Reading.Generated(fixture.Parser, fixture.Entry, shape.Input(turns));

				foreach (var options in Lowerings)
				{
					// A shape left quadratic on purpose may not finish here; it is written down above.
					try
					{
						var ours = new IrEvaluator(Reading.Program(fixture.Grammar, options), new Ladders.CountingHost(fixture.Guards)) { Resumes = true, StepBudget = 100_000_000 }
							.Parse(fixture.Entry, shape.Input(turns));

						Assert.Equal((theirs.Accepted, theirs.Position), (ours.Accepted, ours.Position));
					}
					catch (IrStepsExceeded)
					{
					}
				}
			}
		}

		output.WriteLine(text.ToString());

		Assert.True(failed.Count == 0, string.Join("; ", failed));
	}

	/// <summary>
	/// Repetitions nested at one cursor: the inner one's last turn, which fails, begins where the
	/// outer one's next turn does, so that a store keyed by the position on the tape alone holds
	/// the inner one's record where the outer one looks for its own. Each sentinel takes only its
	/// own entry's records, and answers stay the generated parser's; taking the latest record of
	/// any owner instead (the check off, on purpose) answers otherwise or does not end.
	/// </summary>
	/// <remarks>
	/// The calculator's own grammar reads with binding powers, which the tree does not lower; its
	/// twin here reads the same language with a repetition per level and the literal input that
	/// hung the C# repro, <c>(1^0^4)^1/</c>, is one of its inputs.
	/// </remarks>
	[Fact]
	public void NestedRepetitionsEndingAtOneCursor_EachSentinelTakesItsOwnRecords()
	{
		var wrong = 0;
		var read  = 0;

		foreach (var name in new[] { "Arithmetic", "NestedSettled" })
		{
			var fixture = Cases.Fixtures[name];
			var inputs  = Cases.InputsOf(name);

			inputs.AddRange(Ladders.Shapes.Where(shape => shape.Fixture == name).Select(static shape => shape.Input(40)));

			if (name == "Arithmetic")
				inputs.AddRange(["(1^0^4)^1/", "((1^0^4)^1/2)^1/", "1*(2+3^4^5)*6/"]);

			foreach (var input in inputs)
			{
				var (theirs, _, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

				foreach (var options in Lowerings)
				{
					var program = Reading.Program(fixture.Grammar, options);
					var ours    = new IrEvaluator(program, new RecordingHost(fixture.Guards)) { Resumes = true }.Parse(fixture.Entry, input);

					Assert.Equal(theirs.Line, ours.Line);
					read++;

					try
					{
						var mutated = new IrEvaluator(program, new RecordingHost(fixture.Guards)) { Resumes = true, AnyOwner = true, StepBudget = 5_000_000 }.Parse(fixture.Entry, input);

						if (mutated.Line != theirs.Line)
							wrong++;
					}
					catch (Exception thrown) when (thrown is IrStepsExceeded or IrContractViolation or InvalidCastException or ArgumentException or IndexOutOfRangeException)
					{
						wrong++;
					}
				}
			}
		}

		output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"read {read}, with any owner wrong or unending {wrong}"));

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "owner-mutation.txt"), string.Create(CultureInfo.InvariantCulture, $"read {read}, with any owner wrong or unending {wrong}\n"));

		Assert.True(wrong > 0, "taking any owner's record changed nothing: the fixtures do not reach the collision");
	}

	/// <summary>
	/// A construction that throws leaves the reading as it leaves the generated parser's — the same
	/// exception, or none where that parser built nothing there — whether it is built in a turn
	/// that is then given up or in one that stands, with a replay that goes straight to its turns
	/// and with every decision open.
	/// </summary>
	[Fact]
	public void AConstructionThatThrows_ThrowsWhereTheGeneratedParserThrows()
	{
		var fixture = Cases.Fixtures["Throwing"];
		var checkedCount = 0;
		var thrown  = 0;

		foreach (var input in Cases.InputsOf("Throwing").Append("1:2:3:4a,b,c,d!").Append("1:2:3:4a,b,c,d"))
		{
			// Every call a reading that builds throughout makes: the generated parser, which stops
			// building after a second give-back, makes only some of them.
			var everything = new RecordingHost();

			new IrEvaluator(Reading.Program(fixture.Grammar), everything) { BuildsThroughout = true }.Parse(fixture.Entry, input);

			var calls = everything.Log.Distinct().ToList();

			foreach (var call in calls)
			{
				var theirs = Outcome(() => Reading.Generated(fixture.Parser, fixture.Entry, input, call).Outcome);

				foreach (var options in Lowerings)
				{
					var program = Reading.Program(fixture.Grammar, options);
					var ours    = Outcome(() => new IrEvaluator(program, new RecordingHost(throwOn: call)) { Resumes = true }.Parse(fixture.Entry, input));

					Assert.True(theirs == ours, $"{Twin.Quoted(input)} throwing at {call}: the generated parser {theirs}, the evaluator {ours}");
					checkedCount++;

					if (ours.StartsWith("T ", StringComparison.Ordinal))
						thrown++;
				}
			}
		}

		output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"checked {checkedCount}, thrown {thrown}"));

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "throwing.txt"), string.Create(CultureInfo.InvariantCulture, $"checked {checkedCount}, thrown {thrown}\n"));

		Assert.True(thrown > 0);

		static string Outcome(Func<IrOutcome> read)
		{
			try
			{
				return read().Line;
			}
			catch (TwinException exception)
			{
				return "T " + exception.Message;
			}
		}
	}

	[Fact(Skip = "The calculator reads with binding powers, which the tree does not lower yet; its fold-free twin carries the case.")]
	public void TheCalculatorItself_ReadsItsReproAsTheGeneratedParser()
	{
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
