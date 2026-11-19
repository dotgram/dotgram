using System;
using System.Reflection;
using System.Text;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// What a refusal says, read both ways: where the evaluator and the generated C# parser put it and
/// what each names as expected there. Written to a file for reading, and held to nothing yet.
/// </summary>
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

	[Fact]
	public void WhatEachSaysOfARefusal()
	{
		if (Environment.GetEnvironmentVariable("IR_REPORT") is not { } report)
			return;

		var text  = new StringBuilder();
		var cases = new List<(string Fixture, string Input)>();

		foreach (var name in Cases.Fixtures.Keys)
			foreach (var input in Cases.InputsOf(name))
				cases.Add((name, input));

		foreach (var (_, input) in HostTests.JsonCorpus())
			cases.Add(("Json", input));

		foreach (var (name, input) in cases)
		{
			var fixture = Cases.Fixtures[name];
			var theirs  = Theirs(fixture.Parser, fixture.Entry, input);

			if (theirs.Accepted)
				continue;

			var (ours, _, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, input, fixture.Guards);

			var raw = new List<string>();

			if (theirs.First is not null)
				raw.Add("[" + string.Join(" | ", theirs.First) + "]");

			foreach (var tied in theirs.Tied ?? [])
				raw.Add("[" + string.Join(" | ", tied) + "]");

			text.Append(name).Append('\t').Append(Twin.Quoted(input.Length > 60 ? input[..60] : input))
				.Append('\t').Append(theirs.Position).Append('\t').Append(ours.Position)
				.Append('\t').Append(theirs.Error)
				.Append('\t').Append(string.Join(" ", raw))
				.Append('\t').Append(string.Join(" | ", ours.Expected))
				.Append('\t').Append(string.Join(" ", raw.Count == 0 ? [] : (theirs.Tied ?? []).Prepend(theirs.First ?? []).Select(Canon)))
				.Append('\t').Append(string.Join(" ", ours.Said.Select(static one => Canon([one]))))
				.Append('\n');
		}

		File.WriteAllText(Path.Combine(report, "refusals.tsv"), text.ToString());
	}
}
