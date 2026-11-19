using System;
using System.Reflection;
using System.Text;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>The readings of <see cref="RefusalSpikeTests"/>, kept out of the classes that run side by side.</summary>
[CollectionDefinition(nameof(RefusalSpikeAlone), DisableParallelization = true)]
public sealed class RefusalSpikeAlone;

/// <summary>
/// What a refusal says, read both ways: where the evaluator and the generated C# parser put it and
/// what each names as expected there. Written to a file for reading, and held to nothing yet.
/// </summary>
/// <remarks>
/// Run apart from the other classes: the allocation test counts the bytes of the whole process
/// while it measures, and these readings would be counted with it.
/// </remarks>
[Collection(nameof(RefusalSpikeAlone))]
public sealed class RefusalSpikeTests
{
	static (string[]? First, string[][]? Tied, string? Error, int Position, bool Accepted) Theirs(Type parser, string entry, string input)
	{
		var method = parser.GetMethod("Try" + entry, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)])!;
		object? match = null;

		Twin.Record(() => match = method.Invoke(null, [input]));

		var type = match!.GetType();

		return (
			(string[]?)type.GetField("_expected", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(match),
			(string[][]?)type.GetField("_tied", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(match),
			(string?)type.GetProperty("Error")!.GetValue(match),
			(int)(long)type.GetProperty("Position")!.GetValue(match)!,
			(bool)type.GetProperty("IsSuccess")!.GetValue(match)!);
	}

	static string Canon(IEnumerable<string> set)
	{
		return "[" + string.Join(" | ", set.Select(static one => one.Length > 0 && one[0] == '\u0002' || one.StartsWith("when ", StringComparison.Ordinal) ? "<guard>" : one)) + "]";
	}

	static List<(string Fixture, string Input)> Refused()
	{
		var cases = new List<(string Fixture, string Input)>();

		foreach (var name in Cases.Fixtures.Keys)
			foreach (var input in Cases.InputsOf(name))
				cases.Add((name, input));

		foreach (var (_, input) in HostTests.JsonCorpus())
			cases.Add(("Json", input));

		return cases;
	}

	/// <summary>
	/// The generated C# parser reads an input quietly and, where it refuses it, a second time for
	/// what the refusal says, building nothing. The evaluator told to read the same two times makes
	/// the very host calls the C# parser makes, in the same order, on every input: nothing of the
	/// second reading is left to be allowed for.
	/// </summary>
	[Fact]
	public void ReadQuietlyFirst_TheEvaluatorMakesTheCallsOfTheGeneratedParser()
	{
		var refused = 0;
		var guards  = 0;

		foreach (var (name, input) in Refused())
		{
			var fixture   = Cases.Fixtures[name];
			var host      = new RecordingHost(fixture.Guards);
			var evaluator = new IrEvaluator(Reading.Program(fixture.Grammar), host);

			var (outcome, quiet, _) = evaluator.ParseQuietlyFirst(fixture.Entry, input);
			var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

			Assert.Equal(theirs.Line, outcome.Line);
			Assert.Equal(theirLog, host.Log);

			if (outcome.Accepted)
				continue;

			refused++;
			guards += host.Log.Count(static call => call.StartsWith('g'));

			// The quiet reading alone already knows where; the second adds only what was expected.
			Assert.Equal(outcome.Position, quiet.Position);

			// And one reading that records from the start says the same as the two.
			var (once, _, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, input, fixture.Guards);

			Assert.Equal(once.Line, outcome.Line);
			Assert.Equal(once.Said, outcome.Said);
		}

		Assert.True(refused > 200, "refused " + refused);
		Assert.True(guards > 0);
	}

	static readonly System.Collections.Concurrent.ConcurrentDictionary<string, IrProgram> _planned = new();

	static IrProgram Planned(string grammar)
	{
		return _planned.GetOrAdd(grammar, static text => RefusalPlan.AsTheCSharpReader(IrLowering.Lower(Graphs.Of(text))));
	}

	/// <summary>What the generated parser kept of a refusal, set by set in its order.</summary>
	static string Kept(string[]? first, string[][]? tied)
	{
		var sets = new List<string>();

		if (first is not null)
			sets.Add(Canon(first));

		foreach (var one in tied ?? [])
			sets.Add(Canon(one));

		return string.Join(" ", sets);
	}

	[Fact]
	public void WhatEachSaysOfARefusal()
	{
		var report = Environment.GetEnvironmentVariable("IR_REPORT");
		var text   = new StringBuilder();
		var counts = new SortedDictionary<string, (int Refused, int Naive, int Planned)>(StringComparer.Ordinal);

		foreach (var (name, input) in Refused())
		{
			var fixture = Cases.Fixtures[name];
			var theirs  = Theirs(fixture.Parser, fixture.Entry, input);
			var (generated, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

			// The facts of the plan change what is said of a refusal and nothing else: the verdict,
			// the value, the place and the host calls stay those of the generated parser.
			var host      = new RecordingHost(fixture.Guards);
			var evaluator = new IrEvaluator(Planned(fixture.Grammar), host);
			var (planned, _, _) = evaluator.ParseQuietlyFirst(fixture.Entry, input);

			Assert.Equal(generated.Line, planned.Line);
			Assert.Equal(theirLog, host.Log);

			if (theirs.Accepted)
				continue;

			var (naive, _, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, input, fixture.Guards);

			var kept  = Kept(theirs.First, theirs.Tied);
			var ours  = string.Join(" ", naive.Said.Select(static one => Canon([one])));
			var plans = string.Join(" ", planned.Said.Select(static one => Canon([one])));
			var count = counts.GetValueOrDefault(name);

			counts[name] = (count.Refused + 1, count.Naive + (ours == kept ? 1 : 0), count.Planned + (plans == kept ? 1 : 0));

			text.Append(name).Append('\t').Append(Twin.Quoted(input.Length > 60 ? input[..60] : input))
				.Append('\t').Append(theirs.Position).Append('\t').Append(planned.Position)
				.Append('\t').Append(theirs.Error)
				.Append('\t').Append(kept)
				.Append('\t').Append(ours)
				.Append('\t').Append(plans)
				.Append('\t').Append(plans == kept ? "=" : "differs")
				.Append('\n');
		}

		var summary = string.Join("\n", counts.Select(static one => $"{one.Key}\t{one.Value.Refused}\t{one.Value.Naive}\t{one.Value.Planned}"));

		if (report is not null)
		{
			File.WriteAllText(Path.Combine(report, "refusals.tsv"), text.ToString());
			File.WriteAllText(Path.Combine(report, "refusals-summary.tsv"), "fixture\trefused\tnaive sets equal\tplanned sets equal\n" + summary + "\n");
		}
	}

	/// <summary>
	/// What a refused repetition costs today, counted: every turn given back is a reading of the
	/// whole body again from its start, in the evaluator and in the generated parser alike.
	/// </summary>
	[Fact]
	public void ARefusedRepetitionIsReadAgainFromItsStart()
	{
		var fixture = Cases.Fixtures["Loop"];
		var lines   = new List<string>();
		var steps   = new List<long>();

		foreach (var turns in new[] { 250, 500, 1000, 2000, 4000 })
		{
			var input = string.Concat(Enumerable.Repeat("a,", turns)) + "!";

			var host      = new RecordingHost();
			var evaluator = new IrEvaluator(Reading.Program(fixture.Grammar), host);
			var outcome   = evaluator.Parse(fixture.Entry, input);

			Assert.False(outcome.Accepted);

			var watch = System.Diagnostics.Stopwatch.StartNew();
			var (theirs, log, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

			watch.Stop();

			Assert.Equal(theirs.Line, outcome.Line);

			steps.Add(evaluator.Steps);
			lines.Add(string.Create(System.Globalization.CultureInfo.InvariantCulture,
				$"{turns}\t{evaluator.Steps}\t{evaluator.Ways.Retries}\t{host.Log.Count}\t{log.Count}\t{watch.Elapsed.TotalMilliseconds:F1}"));
		}

		// Twice the turns is about four times the work.
		Assert.True(steps[^1] > steps[^2] * 3, string.Join("; ", lines));

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } report)
			File.WriteAllLines(Path.Combine(report, "refused-loop.tsv"), lines.Prepend("turns\tevaluator steps\tretries\tevaluator host calls\tC# host calls\tC# ms"));
	}
}
