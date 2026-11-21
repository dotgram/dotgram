using System;
using System.Globalization;
using System.IO;
using System.Text;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// Restart by budget, in C (<see cref="SliceAblation.RestartByBudget"/>): the first reading is the
/// parser without resumption, with a counter on every reading again of a region; past the budget
/// the reading is given up and the input read again from the start by the resuming set alone
/// (<see cref="SliceAblation.ResumingSetAlone"/>). A measurement: nothing here is the default.
/// </summary>
/// <remarks>
/// The evaluator restarts by the same rule (<see cref="IrEvaluator.RestartBudget"/>), so that the
/// host calls of a reading that restarts are the evaluator's: its calls without resumption up to the
/// restart, then its calls resuming. Its answers are held to the evaluator's without restart.
/// </remarks>
public sealed class RestartTests(ITestOutputHelper output)
{
	static readonly int[] Sizes = [16000, 32000, 64000];

	// The ladders the resuming evaluator reads in their length without going straight to anything:
	// the inner replays read nothing again before their own decision, and the narrowing leaves the
	// last thing in braces without a place.
	static readonly string[] Unjumped = ["config, inner replays", "last in braces"];

	// The ladders C reads (a recursion 64,000 deep holding a list in every frame is the evaluator's alone).
	static IEnumerable<(Ladders.Shape Shape, IrLowering.Options? Options)> Held()
	{
		foreach (var (name, options) in ResumptionTests.Linear)
		{
			if (name != "stood in a recursion")
				yield return (Ladders.Shapes.Single(one => one.Name == name), options);
		}
	}

	static string Head(IrProgram program, IrOutcome outcome)
	{
		return outcome.Accepted
			? outcome.Line
			: "R " + outcome.Position.ToString(CultureInfo.InvariantCulture) + " " +
				string.Join(",", outcome.Expected.Select(label => program.Expected.IndexOf(label)).Order().Select(static index => index.ToString(CultureInfo.InvariantCulture)));
	}

	static void Report(string file, string text)
	{
		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllText(Path.Combine(report, file), text);
	}

	/// <summary>
	/// The resuming set alone, entered at the top for the whole input, reads every ladder C holds in
	/// its length: twice the turns twice the steps; it resumes (turns gone straight to, where the
	/// resuming evaluator goes straight to any), with the
	/// jumps and readings asked for again of the resuming evaluator and the steps of the whole
	/// resuming C parser; and it answers as the evaluator.
	/// </summary>
	[Fact(Explicit = true)]
	public void TheResumingSetAlone_ReadsEveryLadderInItsLength()
	{
		var text   = new StringBuilder("ladder\tturns\tsteps alone\tsteps on\tjumps alone\tjumps evaluator\tretries alone\tretries evaluator\tanswer\n");
		var failed = new List<string>();

		foreach (var (shape, options) in Held())
		{
			var fixture = Cases.Fixtures[shape.Fixture];
			var program = Reading.Program(fixture.Grammar, options);
			var inputs  = Sizes.Select(shape.Input).ToList();
			var alone   = Slices.InC("Alone-" + shape.Fixture, program, fixture.Entry, inputs, counted: true, stackMegabytes: 2048, ablation: SliceAblation.ResumingSetAlone);
			var on      = Slices.InC("On-" + shape.Fixture, program, fixture.Entry, inputs, counted: true, stackMegabytes: 2048, ablation: SliceAblation.Whole);

			for (var i = 0; i < inputs.Count; i++)
			{
				var evaluator = new IrEvaluator(program, new Ladders.CountingHost(fixture.Guards)) { Resumes = true, StepBudget = 400_000_000 };
				var outcome   = evaluator.Parse(fixture.Entry, inputs[i]);
				var counts    = alone[i].Counts!;
				var jumps     = evaluator.Jumps + evaluator.Skips;

				text.Append(string.Create(CultureInfo.InvariantCulture,
					$"{shape.Name}\t{Sizes[i]}\t{counts[0]}\t{on[i].Counts![0]}\t{counts[3]}\t{jumps}\t{counts[2]}\t{evaluator.Ways.Retries}\t{alone[i].Head}\n"));

				if (alone[i].Head != Head(program, outcome))
					failed.Add($"{shape.Name} {Sizes[i]}: alone {alone[i].Head}, evaluator {Head(program, outcome)}");

				if (counts[3] != jumps || counts[3] == 0 && !Unjumped.Contains(shape.Name) || counts[2] != evaluator.Ways.Retries || counts[0] != on[i].Counts![0])
					failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} {Sizes[i]}: jumps {counts[3]} / {jumps}, retries {counts[2]} / {evaluator.Ways.Retries}, steps {counts[0]} / {on[i].Counts![0]}"));
			}

			var ratios = (alone[1].Counts![0] / (double)alone[0].Counts![0], alone[2].Counts![0] / (double)alone[1].Counts![0]);

			if (ratios.Item1 is < 1.9 or > 2.1 || ratios.Item2 is < 1.9 or > 2.1)
				failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name}: steps ratios {ratios.Item1:F2} {ratios.Item2:F2}"));
		}

		output.WriteLine(text.ToString());
		Report("r1-ladders.tsv", text.ToString());

		Assert.True(failed.Count == 0, string.Join("; ", failed));
	}

	/// <summary>
	/// Every ladder C holds, read by the parser that restarts, early (budget 2) and late (8), and at 4:
	/// twice the turns twice the steps, at most the budget and two times the steps of the resuming set
	/// alone, the restarts the evaluator's restarting by the same rule, and the answer the evaluator's
	/// without restart.
	/// </summary>
	[Fact(Explicit = true)]
	public void RestartByBudget_ReadsEveryLadderInItsLength()
	{
		var text   = new StringBuilder("ladder\tbudget\tturns\tsteps\tsteps alone\tsteps / alone\trestarts\trestarts evaluator\tfurthest at the restart / length\tsteps given up / steps (evaluator)\tanswer\n");
		var failed = new List<string>();

		foreach (var (shape, options) in Held())
		{
			var fixture = Cases.Fixtures[shape.Fixture];
			var program = Reading.Program(fixture.Grammar, options);
			var inputs  = Sizes.Select(shape.Input).ToList();
			var alone   = Slices.InC("Alone-" + shape.Fixture, program, fixture.Entry, inputs, counted: true, stackMegabytes: 2048, ablation: SliceAblation.ResumingSetAlone);

			foreach (var budget in new[] { 2, 4, 8 })
			{
				var restart = Slices.InC("Restart-" + shape.Fixture + "-" + budget.ToString(CultureInfo.InvariantCulture), program, fixture.Entry, inputs,
					counted: true, stackMegabytes: 2048, ablation: SliceAblation.RestartByBudget, budget: budget);

				for (var i = 0; i < inputs.Count; i++)
				{
					var evaluator = new IrEvaluator(program, new Ladders.CountingHost(fixture.Guards)) { RestartBudget = budget, StepBudget = 4_000_000_000 };
					var outcome   = evaluator.Parse(fixture.Entry, inputs[i]);
					var plain     = new IrEvaluator(program, new Ladders.CountingHost(fixture.Guards)) { Resumes = true, StepBudget = 400_000_000 }.Parse(fixture.Entry, inputs[i]);
					var counts    = restart[i].Counts!;
					var steps     = counts[0];
					var reference = alone[i].Counts![0];
					var restarts  = counts[7];

					text.Append(string.Create(CultureInfo.InvariantCulture,
						$"{shape.Name}\t{budget}\t{Sizes[i]}\t{steps}\t{reference}\t{steps / (double)reference:F2}\t{restarts}\t{evaluator.Restarts}\t{(evaluator.Restarts > 0 ? FurthestAt(evaluator) / (double)inputs[i].Length : 0):F3}\t{(evaluator.Steps == 0 ? 0 : evaluator.StepsGivenUp / (double)evaluator.Steps):F3}\t{restart[i].Head}\n"));

					if (restart[i].Head != Head(program, plain) || Head(program, outcome) != Head(program, plain))
						failed.Add($"{shape.Name} {budget} {Sizes[i]}: restarting {restart[i].Head}, evaluator restarting {Head(program, outcome)}, without {Head(program, plain)}");

					if (restarts != evaluator.Restarts)
						failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} {budget} {Sizes[i]}: restarts {restarts}, evaluator {evaluator.Restarts}"));

					if (steps > (budget + 2) * reference)
						failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} {budget} {Sizes[i]}: steps {steps} over ({budget} + 2) x {reference}"));
				}

				var ratios = (restart[1].Counts![0] / (double)restart[0].Counts![0], restart[2].Counts![0] / (double)restart[1].Counts![0]);

				if (ratios.Item1 is < 1.9 or > 2.1 || ratios.Item2 is < 1.9 or > 2.1)
					failed.Add(string.Create(CultureInfo.InvariantCulture, $"{shape.Name} {budget}: steps ratios {ratios.Item1:F2} {ratios.Item2:F2}"));
			}
		}

		output.WriteLine(text.ToString());
		Report("r2-ladders.tsv", text.ToString());

		Assert.True(failed.Count == 0, string.Join("; ", failed));

		static long FurthestAt(IrEvaluator evaluator)
		{
			return evaluator.FurthestAtRestart;
		}
	}

	/// <summary>
	/// What the ladders cost in instructions (<c>perf stat -e instructions:u</c>, the parser built
	/// without counters, with the recording host), a size a run: the resuming set alone and the
	/// parser that restarts at budgets 2, 4 and 8. Needs <c>IR_KEEP</c>, where the builds are left
	/// to be run.
	/// </summary>
	[Fact(Explicit = true)]
	public void RestartByBudget_LadderInstructions()
	{
		var kept = Environment.GetEnvironmentVariable("IR_KEEP") ?? throw new InvalidOperationException("IR_KEEP names no directory.");
		var text = new StringBuilder("ladder\tturns\talone\trestart 2\trestart 4\trestart 8\n");

		foreach (var (shape, options) in Held())
		{
			var fixture = Cases.Fixtures[shape.Fixture];
			var program = Reading.Program(fixture.Grammar, options);

			foreach (var turns in Sizes)
			{
				var row = new List<string>();

				foreach (var (form, budget) in new (SliceAblation, int)[] { (SliceAblation.ResumingSetAlone, 4), (SliceAblation.RestartByBudget, 2), (SliceAblation.RestartByBudget, 4), (SliceAblation.RestartByBudget, 8) })
				{
					var name = string.Create(CultureInfo.InvariantCulture, $"I-{shape.Fixture}-{form}-{budget}-{turns}");

					Slices.InC(name, program, fixture.Entry, [shape.Input(turns)], stackMegabytes: 2048, ablation: form, budget: budget);

					var directory = Path.Combine(kept, name + "-c");
					var (exit, _, error) = Slices.Run("perf", ["stat", "-x,", "-e", "instructions:u", Path.Combine(directory, "slice"), fixture.Entry, "batch.bin", "answers.txt"], directory,
						new Dictionary<string, string> { ["IR_STACK_MB"] = "2048" });
					var line = error.Split('\n').FirstOrDefault(static one => one.Contains("instructions", StringComparison.Ordinal));

					row.Add(exit == 0 && line is not null ? line.Split(',')[0] : "-");
					Directory.Delete(directory, recursive: true);
				}

				text.Append(shape.Name).Append('\t').Append(turns.ToString(CultureInfo.InvariantCulture)).Append('\t').AppendJoin('\t', row).Append('\n');
			}
		}

		output.WriteLine(text.ToString());
		Report("r3-ladder-instructions.tsv", text.ToString());
	}

	/// <summary>
	/// Every fixture's inputs and their ladders at forty turns, read by the parser that restarts at
	/// budgets 2 and 8: the answers the evaluator's without restart (resuming and not), the host calls
	/// those of the evaluator restarting by the same rule — its calls without resumption up to the
	/// restart, then resuming. How many readings restarted is written down.
	/// </summary>
	[Fact(Explicit = true)]
	public void RestartByBudget_ReadsEveryFixtureAsTheEvaluator()
	{
		var text = new StringBuilder("budget\tfixture\treadings\trestarted\n");

		foreach (var budget in new[] { 2, 8 })
		{
			var total = 0;

			foreach (var (name, fixture) in Cases.Fixtures)
			{
				var inputs = Cases.InputsOf(name);

				inputs.AddRange(Ladders.Shapes.Where(shape => shape.Fixture == name).SelectMany(static shape => new[] { shape.Input(10), shape.Input(40) }));

				if (name == "Json")
					inputs.AddRange(HostTests.JsonCorpus().Select(static one => one.Text));

				var program  = Reading.Program(fixture.Grammar);
				var expected = new List<SliceAnswer>();
				var restarts = 0;

				foreach (var input in inputs)
				{
					var before = Slices.EvaluatedRestarts;

					expected.Add(Slices.EvaluatedRestarting(program, fixture.Entry, input, fixture.Guards, budget));

					restarts += (int)(Slices.EvaluatedRestarts - before);
				}

				var answers = expected.SelectMany(static answer => answer.Log).Where(static call => call.StartsWith('g')).Distinct().ToList();
				var actual  = Slices.InC("RestartFixture-" + name, program, fixture.Entry, inputs, guardAnswers: answers, ablation: SliceAblation.RestartByBudget, budget: budget);

				Assert.Equal(expected.Count, actual.Count);

				for (var i = 0; i < expected.Count; i++)
				{
					Assert.True(expected[i].Head == actual[i].Head, $"{name}, budget {budget}, on {Twin.Quoted(inputs[i])}: expected {expected[i].Head}, read {actual[i].Head}");
					Assert.True(expected[i].Log.SequenceEqual(actual[i].Log), $"{name}, budget {budget}, on {Twin.Quoted(inputs[i])}: the host calls differ");
				}

				total += restarts;
				text.Append(string.Create(CultureInfo.InvariantCulture, $"{budget}\t{name}\t{inputs.Count}\t{restarts}\n"));
			}

			text.Append(string.Create(CultureInfo.InvariantCulture, $"{budget}\tall\t\t{total}\n"));
		}

		output.WriteLine(text.ToString());
		Report("r2-fixtures.tsv", text.ToString());
	}

	/// <summary>
	/// A refusal's second reading (the one that says what was expected) has a budget of its own: a
	/// reading that restarts in it says what the evaluator without restart says; and where the quiet
	/// reading restarted, the second reads resuming from the start, and says the same.
	/// </summary>
	[Fact]
	public void ARestartInTheSecondReading_SaysWhatWasExpected()
	{
		var seen = 0;

		foreach (var (shape, options) in Held())
		{
			var fixture = Cases.Fixtures[shape.Fixture];
			var program = Reading.Program(fixture.Grammar, options);
			var input   = shape.Input(60);

			var (plain, _, _) = new IrEvaluator(program, new RecordingHost(fixture.Guards)).ParseQuietlyFirst(fixture.Entry, input);

			if (plain.Accepted)
				continue;

			// The quiet reading with a budget it never passes, the second with the least one.
			var second = new IrEvaluator(program, new RecordingHost(fixture.Guards)) { RestartBudget = 1_000_000, SecondReadingBudget = 1 };
			var (late, quiet, _) = second.ParseQuietlyFirst(fixture.Entry, input);

			Assert.Equal(plain.Line, late.Line);
			Assert.Equal(plain.Expected, late.Expected);
			Assert.Equal(plain.Said, late.Said);
			Assert.False(quiet.Accepted);

			// Both readings with the least budget: the quiet one gives up, the second resumes from the start.
			var both = new IrEvaluator(program, new RecordingHost(fixture.Guards)) { RestartBudget = 1 };
			var (early, _, _) = both.ParseQuietlyFirst(fixture.Entry, input);

			Assert.Equal(plain.Line, early.Line);
			Assert.Equal(plain.Expected, early.Expected);
			Assert.Equal(plain.Said, early.Said);

			if (second.Restarts == 1 && both.Restarts == 1)
				seen++;
		}

		output.WriteLine(seen.ToString(CultureInfo.InvariantCulture) + " ladders restarted in the second reading alone, and in the quiet one alone");

		Assert.True(seen > 0, "No ladder restarted in its second reading.");
	}
}
