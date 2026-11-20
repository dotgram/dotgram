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
		("records below a cut", null), ("config, inner replays", null),
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

	/// <summary>
	/// Which statements keep a record, decided by count: with every repetition and unbounded
	/// statement kept, how often a replay went past one that each narrower criterion would have
	/// left out. The one decided — a statement keeps where it decides or a decision can open after
	/// it in its replay region — leaves out none that is ever gone past, over every fixture, the
	/// JSON corpus and every ladder, under both lowerings; the narrower one of repetitions with a
	/// decision of their own leaves out repetitions that are.
	/// </summary>
	[Fact]
	public void TheCriterion_LeavesOutNoStatementAReplayGoesPast()
	{
		var cases = new List<(string Fixture, string Input)>();

		foreach (var name in Cases.Fixtures.Keys)
			foreach (var input in Cases.InputsOf(name))
				cases.Add((name, input));

		foreach (var (_, input) in HostTests.JsonCorpus())
			cases.Add(("Json", input));

		foreach (var shape in Ladders.Shapes)
			cases.Add((shape.Fixture, shape.Input(300)));

		var text   = new StringBuilder("fixture\tlowering\tjumps and skips (wide)\twould have (decided)\twould have (narrowed)\tsentinels wide\tsentinels decided\n");
		var totals = new long[5];

		foreach (var group in cases.GroupBy(static one => one.Fixture))
		{
			var fixture = Cases.Fixtures[group.Key];

			foreach (var options in Lowerings)
			{
				var program = Reading.Program(fixture.Grammar, options);
				var row     = new long[5];

				foreach (var (_, input) in group)
				{
					foreach (var measured in new[] { ResumptionCriterion.Decided, ResumptionCriterion.Narrowed })
					{
						var wide = new IrEvaluator(program, new Ladders.CountingHost(fixture.Guards)) { Resumes = true, Criterion = ResumptionCriterion.Wide, Measured = measured, StepBudget = 50_000_000 };

						try
						{
							wide.Parse(fixture.Entry, input);
						}
						catch (IrStepsExceeded)
						{
						}

						if (measured == ResumptionCriterion.Decided)
						{
							row[0] += wide.Jumps + wide.Skips;
							row[1] += wide.WouldHaveJumped;
							row[3] += wide.Sentinels;
						}
						else
							row[2] += wide.WouldHaveJumped;
					}

					var decided = new IrEvaluator(program, new Ladders.CountingHost(fixture.Guards)) { Resumes = true, StepBudget = 50_000_000 };

					try
					{
						decided.Parse(fixture.Entry, input);
					}
					catch (IrStepsExceeded)
					{
					}

					row[4] += decided.Sentinels;
				}

				for (var i = 0; i < row.Length; i++)
					totals[i] += row[i];

				text.Append(group.Key).Append('\t').Append(Named(options)).Append('\t').AppendJoin('\t', row).Append('\n');
			}
		}

		text.Append("all\t\t").AppendJoin('\t', totals).Append('\n');

		// What each criterion leaves out, statement by statement, on the proof-based lowering.
		text.Append("\nleft without a record (proof-based lowering)\nfixture\tcriterion\tstatement\twhy\n");

		foreach (var (name, fixture) in Cases.Fixtures.DistinctBy(static one => one.Value.Grammar))
		{
			var program = Reading.Program(fixture.Grammar);

			foreach (var criterion in new[] { ResumptionCriterion.Decided, ResumptionCriterion.Narrowed })
				foreach (var (stmt, why) in ResumptionSites.Of(program, criterion).Left)
					text.Append(name).Append('\t').Append(criterion).Append('\t').Append(stmt.GetType().Name).Append('\t').Append(why).Append('\n');
		}

		output.WriteLine(text.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "criterion.tsv"), text.ToString());

		Assert.True(totals[0] > 100, "jumps and skips " + totals[0]);
		Assert.Equal(0, totals[1]);
		Assert.True(totals[2] > 0, "the narrowed criterion left out nothing a replay goes past");
	}

	/// <summary>
	/// Turns that write slots and put them back where they fail — captures, bindings rebound every
	/// turn, two lists, one list rule twice — answer as the generated parser does when a replay goes
	/// straight to them, because the record holds what the slots held; with the record's slots
	/// left out on purpose, they answer otherwise. So a repetition that puts its slots back needs no
	/// exclusion of its own here.
	/// </summary>
	[Fact]
	public void TurnsThatPutTheirSlotsBack_AreHeldByTheRecord()
	{
		var wrong = 0;
		var read  = 0;

		foreach (var name in new[] { "CaptureRollback", "CaptureRollbackChoice", "BindingRollback", "Rebinding", "ListsInSequence", "ListRuleTwice", "Loop", "NestedSettled", "SealedCaptureBefore", "YamlLineBefore" })
		{
			var fixture = Cases.Fixtures[name];
			var inputs  = Cases.InputsOf(name);

			inputs.AddRange(Ladders.Shapes.Where(shape => shape.Fixture == name).Select(static shape => shape.Input(30)));

			foreach (var input in inputs)
			{
				var (theirs, _, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

				foreach (var options in Lowerings)
				{
					var program = Reading.Program(fixture.Grammar, options);

					Assert.Equal(theirs.Line, new IrEvaluator(program, new RecordingHost(fixture.Guards)) { Resumes = true }.Parse(fixture.Entry, input).Line);
					read++;

					try
					{
						if (new IrEvaluator(program, new RecordingHost(fixture.Guards)) { Resumes = true, IgnoresHeld = true, StepBudget = 5_000_000 }.Parse(fixture.Entry, input).Line != theirs.Line)
							wrong++;
					}
					catch (Exception thrown) when (thrown is IrStepsExceeded or IrContractViolation or InvalidCastException or ArgumentException or NullReferenceException)
					{
						wrong++;
					}
				}
			}
		}

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "held-mutation.txt"), string.Create(CultureInfo.InvariantCulture, $"read {read}, with the record's slots left out wrong {wrong}\n"));

		Assert.True(wrong > 0, "leaving the record's slots out changed nothing");
	}

	[Fact(Skip = "The calculator reads with binding powers, which the tree does not lower yet; its fold-free twin carries the case.")]
	public void TheCalculatorItself_ReadsItsReproAsTheGeneratedParser()
	{
	}

	/// <summary>
	/// The printed parsers resume as the evaluator does: every ladder costs its length in
	/// TypeScript with each way of keeping frames and in C, by the parsers' own counters, and the
	/// host is called as often as the input is long.
	/// </summary>
	[Fact]
	public void EveryPrintedLadderCostsItsLength()
	{
		int[] sizes = [16000, 32000, 64000];

		var text   = new StringBuilder();
		var failed = new List<string>();
		var only   = Environment.GetEnvironmentVariable("LADDER");

		foreach (var (name, options) in Linear.Where(one => only is null || one.Shape == only))
		{
			var shape = Ladders.Shapes.Single(one => one.Name == name);

			foreach (var frames in new SliceFrames?[] { SliceFrames.None, SliceFrames.Recursive, SliceFrames.All, null })
			{
				var rungs = Ladders.Printed(shape, options, frames, sizes);
				var where = frames is { } kept ? "TypeScript " + kept : "C";

				text.Append(Ladders.Table($"{shape.Name}, {where}, {Named(options)}", rungs));

				var steps = (rungs[2].Steps / (double)rungs[1].Steps, rungs[1].Steps / (double)rungs[0].Steps);

				if (steps.Item1 is < 1.9 or > 2.1 || steps.Item2 is < 1.9 or > 2.1)
					failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} {where}: steps {rungs[0].Steps} {rungs[1].Steps} {rungs[2].Steps}"));

				if (rungs[0].HostCalls > 0 && (rungs[2].HostCalls > 2.1 * rungs[1].HostCalls || rungs[1].HostCalls > 2.1 * rungs[0].HostCalls))
					failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} {where}: host calls {rungs[0].HostCalls} {rungs[1].HostCalls} {rungs[2].HostCalls}"));
			}
		}

		output.WriteLine(text.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "ladders-printed.tsv"), text.ToString());

		Assert.True(failed.Count == 0, string.Join("; ", failed));
	}

	/// <summary>
	/// In the printed parsers the head of each entry's records is kept beside the tape and never
	/// cleared, and a record names its owner: a head left from an earlier owner or reading is told
	/// apart by that. Without the comparison (on purpose) a sentinel takes another entry's record,
	/// and the answer or the host calls differ from the evaluator's somewhere among the nested and
	/// calculator fixtures.
	/// </summary>
	[Fact]
	public void PrintedParsers_TellTheirOwnRecordsApart()
	{
		var differs = new int[2];

		foreach (var name in new[] { "NestedSettled", "Arithmetic" })
		{
			var fixture = Cases.Fixtures[name];
			var inputs  = Cases.InputsOf(name);

			inputs.AddRange(Ladders.Shapes.Where(shape => shape.Fixture == name).Select(static shape => shape.Input(40)));

			if (name == "Arithmetic")
				inputs.AddRange(["(1^0^4)^1/", "((1^0^4)^1/2)^1/"]);

			foreach (var options in Lowerings)
			{
				var program  = Reading.Program(fixture.Grammar, options);
				var expected = inputs.Select(input => Slices.Evaluated(program, fixture.Entry, input, fixture.Guards)).ToList();

				foreach (var anyOwner in new[] { false, true })
				{
					var runs = new List<Func<List<SliceAnswer>>>
					{
						() => Slices.InC(name + "-owner", program, fixture.Entry, inputs, anyOwner: anyOwner),
						() => Slices.TypeScript(name + "-owner", program, fixture.Entry, inputs, frames: SliceFrames.None, anyOwner: anyOwner),
					};

					for (var r = 0; r < runs.Count; r++)
					{
						var run = runs[r];

						List<SliceAnswer> actual;

						// Taking another entry's record may break the order of the stack, which the C
						// parser stops on: that is an answer that differs too.
						try
						{
							actual = run();
						}
						catch (Exception) when (anyOwner)
						{
							differs[r]++;

							continue;
						}

						for (var i = 0; i < inputs.Count; i++)
						{
							var same = i < actual.Count && actual[i].Head == expected[i].Head && actual[i].Log.SequenceEqual(expected[i].Log);

							if (!anyOwner)
								Assert.True(same, $"{name} on {Twin.Quoted(inputs[i])}");
							else if (!same)
								differs[r]++;
						}
					}
				}
			}
		}

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "owner-mutation-printed.txt"), string.Create(CultureInfo.InvariantCulture, $"without the owner comparison, differing answers or runs: C {differs[0]}, TypeScript {differs[1]}\n"));

		Assert.True(differs[0] > 0 && differs[1] > 0, "without the owner comparison nothing differs in C or in TypeScript: the fixtures do not reach a head of another owner");
	}

	/// <summary>
	/// What a refusal of 64,000 turns leaves held: the records at most at once in the evaluator,
	/// and in the printed parsers the bytes of the tape and the records at their largest and after
	/// the reading, when storage grown past 4,096 entries goes back to its first size.
	/// </summary>
	[Fact]
	public void ALongRefusal_LeavesItsStorageBehindOnlyUpToTheBound()
	{
		var text = new StringBuilder("shape\truntime\tmost records (evaluator) / bytes at most\tbytes after\n");

		foreach (var name in new[] { "loop", "settled config", "nested settled", "(b) sealed", "(e) deciding nest" })
		{
			var shape   = Ladders.Shapes.Single(one => one.Name == name);
			var fixture = Cases.Fixtures[shape.Fixture];
			var program = Reading.Program(fixture.Grammar);
			var input   = shape.Input(64000);

			var evaluator = new IrEvaluator(program, new Ladders.CountingHost(fixture.Guards)) { Resumes = true };

			evaluator.Parse(fixture.Entry, input);
			text.Append(name).Append("\tevaluator\t").Append(evaluator.MostKept).Append("\t-\n");

			foreach (var frames in new SliceFrames?[] { SliceFrames.None, null })
			{
				var answer = frames is { } kept
					? Slices.TypeScript("Held-" + shape.Fixture, program, fixture.Entry, [input], frames: kept, counted: true, stackKilobytes: 1_000_000, systemStackKilobytes: 2_000_000)[0]
					: Slices.InC("Held-" + shape.Fixture, program, fixture.Entry, [input], counted: true, stackMegabytes: 2048)[0];

				var counts = answer.Counts!;

				text.Append(name).Append('\t').Append(frames is null ? "C" : "TypeScript").Append('\t').Append(counts[5]).Append('\t').Append(counts[6]).Append('\n');

				Assert.True(counts[6] <= 4096 * 64, $"{name}: {counts[6]} bytes held after the reading");
			}
		}

		output.WriteLine(text.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "retention.tsv"), text.ToString());
	}

	/// <summary>
	/// A grammar that decides nothing pays nothing for resumption: JSON keeps no statement, and its
	/// printed parsers, in every form, hold none of the code that opens sentinels, keeps records or
	/// stops building — they are the parsers they were.
	/// </summary>
	[Fact]
	public void AGrammarThatDecidesNothing_PrintsNothingOfResumption()
	{
		foreach (var program in new[] { Reading.Program(Graphs.WebGrammar("Rfc8259.cs")), Reading.Program(Grammars.Json) })
		{
			Assert.Empty(ResumptionSites.Of(program).Sites);

			var printed = Enum.GetValues<SliceFrames>().Select(frames => SlicePrinter.Print(program, SliceLanguage.TypeScript, frames)).Append(SlicePrinter.Print(program, SliceLanguage.C));

			foreach (var text in printed)
			{
				foreach (var token in new[] { "keep(", "head(", "stood(", "retried(", "undeferred(", "cut(" })
					Assert.DoesNotContain(token, text, StringComparison.Ordinal);

				// Nor a list taken whole only where the reading builds.
				Assert.DoesNotMatch(@"unbuilt == 0\) \S+ = (val_items|\()", text);
			}
		}
	}

	/// <summary>The shipped grammars the accepting path is measured on (the HTTP one aside: its rules decide only in the tree).</summary>
	static readonly string[] WebFiles = ["Rfc3339.cs", "Rfc3986.cs", "Rfc5322.cs", "Rfc5646.cs", "Rfc6265.cs", "Rfc6266.cs", "Rfc6570.cs", "Rfc6901.cs", "Rfc7239.cs", "Rfc8259.cs", "Rfc8288.cs", "Rfc9651.cs"];

	/// <summary>What each shipped grammar keeps: its publications, whether the tree reads all of each, and the statements kept, by why.</summary>
	[Fact]
	public void TheShippedGrammars_KeepWhatTheyKeep()
	{
		var text = new StringBuilder("grammar\tentries (complete)\tdeciding functions\tkept\tby kind\n");

		foreach (var file in WebFiles)
		{
			var program = Reading.Program(Graphs.WebGrammar(file));
			var sites   = ResumptionSites.Of(program);
			var entries = string.Join(" ", program.Entries.Select(static one => one.Key + (one.Value.Complete ? "" : "*")));
			var kinds   = string.Join(", ", sites.Sites.GroupBy(static one => one.Key.GetType().Name + ": " + one.Value).Select(static one => one.Count() + " " + one.Key));

			text.Append(file).Append('\t').Append(entries).Append('\t').Append(program.Functions.Count(static one => one.Decides)).Append('\t').Append(sites.Sites.Count).Append('\t').Append(kinds).Append('\n');
		}

		output.WriteLine(text.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "shipped-sites.tsv"), text.ToString());
	}

	static readonly string[] Addresses =
	[
		"a@b.c", "john.doe@example.com", "\"John Doe\" <john@example.com>", "Mary Smith <mary@x.test>, jdoe@one.test",
		"A Group:Ed Jones <c@a.test>,joe@where.test,John <jdoe@one.test>;", "Pete(A nice \\) chap) <pete(his account)@silly.test(his host)>",
		"<boss@nil.test>, \"Giant; \\\"Big\\\" Box\" <sysservices@example.net>", "jdoe@[192.168.0.1]",
	];

	/// <summary>The rows the accepting path is measured on: a shipped grammar, a publication of it, inputs it accepts.</summary>
	static readonly (string Name, string File, string Entry, string[] Inputs)[] AcceptedRows =
	[
			("Rfc5322 address lists", "Rfc5322.cs", "ParseAddressList", [.. Addresses, string.Join(", ", Enumerable.Repeat("Mary Smith <mary@x.test>", 40))]),
			("Rfc5322 short addr-spec", "Rfc5322.cs", "ParseAddrSpec", ["a@b", "x@y.z", "first.last@sub.example.com", "jdoe@one.test"]),
			("Rfc9651 list", "Rfc9651.cs", "ParseList", ["a, b;c=1, (x y);z", "1, 2.5, \"s\", ?1, :aGVsbG8=:", "(a b c);d=1, e"]),
			("Rfc9651 dictionary", "Rfc9651.cs", "ParseDictionary", ["a=1, b, c=(1 2)", "x=?0;y=1, z=\"t\""]),
			("Rfc6265 cookies", "Rfc6265.cs", "ParseCookies", ["a=b; c=d; e=\"f\"", "SID=31d4d96e407aad42; lang=en-US"]),
			("Rfc7239 forwarded", "Rfc7239.cs", "ParseForwarded", ["for=192.0.2.43, for=198.51.100.17;by=203.0.113.60;proto=http;host=example.com", "for=\"[2001:db8:cafe::17]:4711\""]),
			("Rfc8288 links", "Rfc8288.cs", "ParseLinks", ["<https://example.com/a>; rel=\"next\", <https://example.com/b>; rel=prev; title=\"x\""]),
			("Rfc3986 references", "Rfc3986.cs", "ParseReference", ["http://user@example.com:8080/a/b/c?q=1&r=2#frag", "../x/y", "mailto:a@b.c"]),
			("Rfc5646 tags", "Rfc5646.cs", "ParseTag", ["en-US", "zh-Hant-TW", "sr-Latn-RS", "de-CH-1901"]),
	];

	/// <summary>
	/// Counts on accepted inputs, not timings: how many kept statements a reading enters and how many
	/// sentinels it opens per byte, under the criterion in force and under one that also asks that
	/// something can fail back after the deciding statement; why each braces site of RFC 5322 is
	/// kept; and how many turns and items each entry into a kept repetition reads.
	/// </summary>
	[Fact]
	public void TheAcceptingPath_IsCounted()
	{
		var text  = new StringBuilder("row\tcriterion\tkept statements\tentries per byte\tsentinels per byte\n");
		var whys  = new StringBuilder("row\tstatement\tkept because\tentries\n");
		var loops = new StringBuilder("row\tturns 0\t1\t2\t3\t4-7\t8-15\t16+\t| items read 0\t1\t2-3\t4-7\t8-15\t16+\t| entries\n");

		foreach (var (name, file, entry, inputs) in AcceptedRows.Where(static row => row.File is "Rfc5322.cs" or "Rfc9651.cs" or "Rfc3986.cs"))
		{
			var program = Reading.Program(Graphs.WebGrammar(file));

			foreach (var criterion in new[] { ResumptionCriterion.Decided, ResumptionCriterion.FailsBack })
			{
				var entries   = 0L;
				var sentinels = 0L;
				var bytes     = 0L;
				var by        = new Dictionary<IrStmt, long>(ReferenceEqualityComparer.Instance);
				var turns     = new List<(int Turns, int Read)>();

				foreach (var input in inputs)
				{
					var evaluator = new IrEvaluator(program, new RecordingHost()) { Resumes = true, Criterion = criterion };

					if (!evaluator.Parse(entry, input).Accepted)
						continue;

					entries   += evaluator.SiteEntries;
					sentinels += evaluator.Sentinels;
					bytes     += input.Length;
					turns.AddRange(evaluator.LoopEntries);

					foreach (var (stmt, count) in evaluator.EntriesBy)
						by[stmt] = by.GetValueOrDefault(stmt) + count;
				}

				var sites = ResumptionSites.Of(program, criterion);

				text.Append(name).Append('\t').Append(criterion).Append('\t').Append(sites.Sites.Count).Append('\t')
					.Append((entries / (double)bytes).ToString("F3", CultureInfo.InvariantCulture)).Append('\t')
					.Append((sentinels / (double)bytes).ToString("F3", CultureInfo.InvariantCulture)).Append('\n');

				if (criterion != ResumptionCriterion.Decided)
					continue;

				var decided = ResumptionSites.Of(program);

				foreach (var group in by.GroupBy(one => one.Key.GetType().Name + " in " + Owner(program, one.Key) + "\t" + decided.Kept.GetValueOrDefault(one.Key, "?")).OrderByDescending(static one => one.Sum(static pair => pair.Value)).Take(12))
					whys.Append(name).Append('\t').Append(group.Key).Append('\t').Append(group.Sum(static pair => pair.Value)).Append('\n');

				int Bucket(int value, int[] edges)
				{
					var at = 0;

					while (at < edges.Length && value >= edges[at])
						at++;

					return at;
				}

				int[] turnEdges = [1, 2, 3, 4, 8, 16];
				int[] readEdges = [1, 2, 4, 8, 16];
				var byTurns = new int[turnEdges.Length + 1];
				var byRead  = new int[readEdges.Length + 1];

				foreach (var (count, read) in turns)
				{
					byTurns[Bucket(count, turnEdges)]++;
					byRead[Bucket(read, readEdges)]++;
				}

				loops.Append(name).Append('\t').AppendJoin('\t', byTurns).Append("\t|\t").AppendJoin('\t', byRead).Append("\t|\t").Append(turns.Count).Append('\n');
			}
		}

		var all = text + "\n" + whys + "\n" + loops;

		output.WriteLine(all);

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "accepting-counts.tsv"), all);

		static string Owner(IrProgram program, IrStmt stmt)
		{
			foreach (var function in program.Functions)
				if (Contains(function.Body, stmt))
					return function.Name;

			return "?";
		}

		static bool Contains(IrStmt within, IrStmt stmt)
		{
			return ReferenceEquals(within, stmt) || within.Children.Any(child => Contains(child, stmt));
		}
	}

	/// <summary>
	/// The same rows in C: each parser printed with its sentinels, alone, the code round them alone,
	/// and without doors, compiled by gcc, timed in turns against the parser without resumption.
	/// </summary>
	[Fact]
	public void TheAcceptingPathInC_IsMeasuredByGrammar()
	{
		var text = new StringBuilder("row\tvariant\taccepted\tms on\tms off\ton / off (median)\trange\n");

		foreach (var (name, file, entry, inputs) in AcceptedRows)
		{
			var program = Reading.Program(Graphs.WebGrammar(file));
			var batch   = Enumerable.Repeat(inputs, 20).SelectMany(static one => one).ToList();

			try
			{
				foreach (var line in Slices.AcceptingPathInC("AcceptC-" + file, program, entry, batch))
					text.Append(name).Append('\t').Append(line).Append('\n');
			}
			catch (NotSupportedException unprinted)
			{
				text.Append(name).Append("\tnot printed: ").Append(unprinted.Message).Append('\n');
			}
		}

		output.WriteLine(text.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "accepting-path-c.tsv"), text.ToString());
	}

	/// <summary>
	/// What resuming costs an accepted input, by grammar: the sentinels the evaluator opens per
	/// byte, and the printed TypeScript parser with its sentinels against the same parser without,
	/// in every way of keeping frames — a direction on a shared machine, written down for the gate
	/// and held to nothing here.
	/// </summary>
	[Fact]
	public void TheAcceptingPath_IsMeasuredByGrammar()
	{
		var text = new StringBuilder("row\taccepted by the evaluator\tsentinels per byte\tkept\tframes\taccepted\tms on\tms off\ton / off (median)\trange\n");

		foreach (var (name, file, entry, inputs) in AcceptedRows)
		{
			var program   = Reading.Program(Graphs.WebGrammar(file));
			var accepted  = 0;
			var sentinels = 0L;
			var bytes     = 0L;
			var by        = new Dictionary<string, long>(StringComparer.Ordinal);
			var records   = 0L;
			var retries   = 0L;
			var jumps     = 0L;
			var entered   = 0L;
			var steps     = 0L;

			// Not every shipped grammar is read whole by the tree: some lower in part.
			try
			{
				foreach (var input in inputs)
				{
					var evaluator = new IrEvaluator(program, new RecordingHost()) { Resumes = true };

					if (evaluator.Parse(entry, input).Accepted)
					{
						accepted++;
						sentinels += evaluator.Sentinels;
						bytes     += input.Length;
						records   += evaluator.Records;
						retries   += evaluator.Ways.Retries;
						jumps     += evaluator.Jumps + evaluator.Skips;
						entered   += evaluator.Ways.Entered;
						steps     += evaluator.Steps;

						foreach (var (kind, count) in evaluator.SentinelsBy)
							by[kind] = by.GetValueOrDefault(kind) + count;
					}
				}
			}
			catch (NotSupportedException unread)
			{
				text.Append(name).Append("\tnot read: ").Append(unread.Message).Append('\n');

				continue;
			}

			if (bytes > 0)
				text.Append(name).Append("\tper byte: ").Append(string.Create(CultureInfo.InvariantCulture,
					$"sentinels {sentinels / (double)bytes:F3}, records {records / (double)bytes:F3}, retries {retries / (double)bytes:F3}, jumps and skips {jumps / (double)bytes:F3}, decisions entered {entered / (double)bytes:F3}, statements {steps / (double)bytes:F1}")).Append('\n');

			foreach (var (kind, count) in by.OrderByDescending(static one => one.Value))
				text.Append(name).Append("\tsentinels at\t").Append(kind).Append('\t').Append(count).Append('\n');

			var batch = Enumerable.Repeat(inputs, 20).SelectMany(static one => one).ToList();

			List<string> lines;

			// Not every shipped grammar prints: the slice prints no text that ignores case.
			try
			{
				lines = [.. Slices.AcceptingPath("Accept-" + file, program, entry, batch).Select(static line => "all kept\t" + line),
					.. Slices.AcceptingPath("Accept-" + file, program, entry, batch, ResumptionCriterion.Repetitions).Select(static line => "repetitions only\t" + line)];
			}
			catch (NotSupportedException unprinted)
			{
				lines = ["not printed: " + unprinted.Message];
			}

			foreach (var line in lines)
			{
				text.Append(name).Append('\t').Append(accepted).Append('/').Append(inputs.Length).Append('\t')
					.Append((bytes == 0 ? 0 : sentinels / (double)bytes).ToString("F3", CultureInfo.InvariantCulture)).Append('\t').Append(line).Append('\n');
			}
		}

		output.WriteLine(text.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "accepting-path.tsv"), text.ToString());
	}

	/// <summary>
	/// Inputs no one wrote: for every fixture, strings drawn from the characters of its own inputs
	/// and every input with one character inserted, dropped or changed — read by the evaluator with
	/// and without resumption, under both lowerings, and held to the generated parser's verdict and
	/// position. The hand-written inputs alone never failed inside an alternative or a turn after a
	/// decision opened in it, which is how a reading that dropped those decisions went unseen.
	/// </summary>
	[Fact]
	public void InputsNoOneWrote_AnswerAsTheGeneratedParser()
	{
		var random = new Random(20261120);
		var unfit  = 0L;
		var read   = 0;
		var wrong  = new List<string>();

		foreach (var (name, fixture) in Cases.Fixtures)
		{
			var inputs   = Cases.InputsOf(name);
			var alphabet = inputs.SelectMany(static input => input).Distinct().ToArray();

			if (alphabet.Length == 0)
				continue;

			var tried = new HashSet<string>(StringComparer.Ordinal);

			foreach (var input in inputs.Where(static input => input.Length <= 64))
			{
				for (var i = 0; i <= input.Length; i++)
				{
					foreach (var unit in alphabet)
						tried.Add(input.Insert(i, unit.ToString()));

					if (i < input.Length)
					{
						tried.Add(input.Remove(i, 1));

						foreach (var unit in alphabet)
							tried.Add(input[..i] + unit + input[(i + 1)..]);
					}
				}
			}

			for (var n = 0; n < (name == "Json" ? 200 : 400); n++)
				tried.Add(new string([.. Enumerable.Range(0, random.Next(0, 13)).Select(_ => alphabet[random.Next(alphabet.Length)])]));

			var batch = tried.Take(name == "Json" ? 1500 : 4000).ToList();

			// The printed parsers on the same inputs, held to the evaluator with resumption: answers and host calls.
			var program0 = Reading.Program(fixture.Grammar);
			var expected = batch.Select(input => Slices.Evaluated(program0, fixture.Entry, input, fixture.Guards)).ToList();
			var answers  = expected.SelectMany(static answer => answer.Log).Where(static call => call.StartsWith('g')).Distinct().ToList();

			foreach (var (where, actual) in new[]
			{
				("C", Slices.InC("Differential-" + name, program0, fixture.Entry, batch, guardAnswers: answers)),
				("TypeScript", Slices.TypeScript("Differential-" + name, program0, fixture.Entry, batch, guardAnswers: answers, frames: SliceFrames.None)),
			})
			{
				for (var i = 0; i < batch.Count; i++)
				{
					read++;

					if ((actual[i].Head != expected[i].Head || !actual[i].Log.SequenceEqual(expected[i].Log)) && wrong.Count < 20)
						wrong.Add($"{name} {Twin.Quoted(batch[i])} ({where}): {actual[i].Head}, the evaluator {expected[i].Head}");
				}
			}

			foreach (var input in batch)
			{
				var (theirs, _, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

				foreach (var options in Lowerings)
				{
					foreach (var resumes in new[] { false, true })
					{
						var program = Reading.Program(fixture.Grammar, options);
						string line;

						try
						{
							var evaluator = new IrEvaluator(program, new RecordingHost(fixture.Guards)) { Resumes = resumes, StepBudget = 20_000_000 };

							line   = evaluator.Parse(fixture.Entry, input).Line;
							unfit += evaluator.Stale;
						}
						catch (IrStepsExceeded)
						{
							continue;
						}
						catch (IrContractViolation broken)
						{
							line = "broken: " + broken.Message;
						}

						read++;

						if (line != theirs.Line && wrong.Count < 20)
							wrong.Add($"{name} {Twin.Quoted(input)} ({Named(options)}, {(resumes ? "resuming" : "replaying")}): {line}, the generated parser {theirs.Line}");
					}
				}
			}
		}

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "differential.txt"), string.Create(CultureInfo.InvariantCulture, $"read {read}, differing {wrong.Count}, stale records passed over {unfit}\n") + string.Join("\n", wrong) + "\n");

		Assert.True(wrong.Count == 0, string.Join("; ", wrong));
	}

	/// <summary>
	/// A replay inside a region keeps records ahead of the replay of the region round it, so a
	/// record can lie below newer ones when a cut goes below its key: it stays in the store, stale,
	/// and a lookup passes it over by the serial of the cuts. With that check off on purpose, a
	/// stale record stands for a reading that has changed under it, and answers differ.
	/// </summary>
	[Fact]
	public void RecordsMadeStaleByACut_ArePassedOver()
	{
		var fixture  = Cases.Fixtures[Environment.GetEnvironmentVariable("IR_STALE_FIXTURE") ?? "StaleRecord"];
		var stale    = 0L;
		var wrong    = 0;
		var read     = 0;
		var inputs   = new List<string> { "k=ax;j=ab;!" };
		var name     = Environment.GetEnvironmentVariable("IR_STALE_FIXTURE") ?? "StaleRecord";
		var units    = Cases.InputsOf(name).SelectMany(static input => input).Distinct().ToArray();

		foreach (var input in Cases.InputsOf(name))
		{
			for (var i = 0; i <= input.Length; i++)
			{
				foreach (var unit in units)
				{
					inputs.Add(input.Insert(i, unit.ToString()));

					if (i < input.Length)
						inputs.Add(input[..i] + unit + input[(i + 1)..]);
				}
			}
		}

		foreach (var input in inputs.Distinct())
		{
			var (theirs, _, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

			foreach (var options in Lowerings)
			{
				var program = Reading.Program(fixture.Grammar, options);
				var log     = new RecordingHost();
				var honest  = new IrEvaluator(program, log) { Resumes = true };

				Assert.Equal(theirs.Line, honest.Parse(fixture.Entry, input).Line);
				stale += honest.Stale;
				read++;

				try
				{
					var mutated = new RecordingHost();

					// The answer, or the host calls made on the way to it.
					if (new IrEvaluator(program, mutated) { Resumes = true, NoCutCheck = true, StepBudget = 5_000_000 }.Parse(fixture.Entry, input).Line != theirs.Line ||
						!mutated.Log.SequenceEqual(log.Log))
						wrong++;
				}
				catch (Exception thrown) when (thrown is IrStepsExceeded or IrContractViolation or InvalidCastException or ArgumentException or NullReferenceException)
				{
					wrong++;
				}
			}
		}

		// The printed parsers on the same inputs: with the check, the evaluator's answers and host
		// calls; without it, somewhere not.
		var batch    = inputs.Distinct().ToList();
		var printed  = new int[2];

		foreach (var options in Lowerings)
		foreach (var noCutCheck in new[] { false, true })
		{
			var program0 = Reading.Program(fixture.Grammar, options);
			var expected = batch.Select(input => Slices.Evaluated(program0, fixture.Entry, input, fixture.Guards)).ToList();

			var runs = new List<Func<List<SliceAnswer>>>
			{
				() => Slices.InC("CutCheck", program0, fixture.Entry, batch, noCutCheck: noCutCheck),
				() => Slices.TypeScript("CutCheck", program0, fixture.Entry, batch, frames: SliceFrames.None, noCutCheck: noCutCheck),
			};

			for (var r = 0; r < runs.Count; r++)
			{
				List<SliceAnswer> actual;

				try
				{
					actual = runs[r]();
				}
				catch (Exception) when (noCutCheck)
				{
					printed[r]++;

					continue;
				}

				for (var i = 0; i < batch.Count; i++)
				{
					var same = actual[i].Head == expected[i].Head && actual[i].Log.SequenceEqual(expected[i].Log);

					if (!noCutCheck)
						Assert.True(same, $"{(r == 0 ? "C" : "TypeScript")} on {Twin.Quoted(batch[i])}: {actual[i].Head}, the evaluator {expected[i].Head}");
					else if (!same)
						printed[r]++;
				}
			}
		}

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, "cut-check.txt"), string.Create(CultureInfo.InvariantCulture,
				$"read {read}, stale records passed over {stale}, wrong without the cut check: evaluator {wrong}, C {printed[0]}, TypeScript {printed[1]}\n"));

		Assert.True(stale > 0, "no record was ever stale below the top: the fixture does not reach a cut below a buried record");
		Assert.True(wrong > 0 && printed[0] > 0 && printed[1] > 0, $"without the cut check nothing differs somewhere: evaluator {wrong}, C {printed[0]}, TypeScript {printed[1]}");
	}

	/// <summary>A search, not a test: inputs on which taking stale records changes an answer.</summary>
	[Fact(Explicit = true)]
	public void Search_StaleRecordsThatMatter()
	{
		var random = new Random(7);
		var found  = new List<string>();

		foreach (var (name, fixture) in Cases.Fixtures.Where(static one => one.Key != "Json"))
		{
			var alphabet = Cases.InputsOf(name).SelectMany(static input => input).Distinct().ToArray();

			if (alphabet.Length == 0)
				continue;

			for (var n = 0; n < 20000 && found.Count < 40; n++)
			{
				var input = new string([.. Enumerable.Range(0, random.Next(2, 16)).Select(_ => alphabet[random.Next(alphabet.Length)])]);

				foreach (var options in Lowerings)
				{
					var program = Reading.Program(fixture.Grammar, options);
					var honest  = new IrEvaluator(program, new RecordingHost(fixture.Guards)) { Resumes = true, StepBudget = 5_000_000 };
					string good, bad;

					try
					{
						good = honest.Parse(fixture.Entry, input).Line;

						if (honest.Stale == 0)
							continue;

						bad = new IrEvaluator(program, new RecordingHost(fixture.Guards)) { Resumes = true, NoCutCheck = true, StepBudget = 5_000_000 }.Parse(fixture.Entry, input).Line;
					}
					catch (Exception thrown)
					{
						bad  = "threw " + thrown.GetType().Name;
						good = "";
					}

					if (good != bad)
						found.Add($"{name} {Twin.Quoted(input)} {Named(options)}: {good} / {bad}");
				}
			}
		}

		File.WriteAllLines(Path.Combine(Environment.GetEnvironmentVariable("IR_REPORT")!, "stale-search.txt"), found);
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
