using System;
using System.Globalization;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// The lowering asks the generator's plan whether a choice or a repetition writes a way, rather than
/// proving it again: what the C# reader decided is what the tree does, and the calls it makes to the
/// host are the reader's.
/// </summary>
public sealed class PlanTests(ITestOutputHelper output)
{
	/// <summary>
	/// <c>Csv</c> on a refused second row: the plan says the line end of a row (<c>eol</c>, a choice of
	/// texts) needs no way back, as the C# reader reads it, and the tree no longer goes back into the
	/// first row to try it another way, reading that row again and building it a second time. What is
	/// left of the C# parser's calls is its second reading of a refused input, which asks the guards
	/// and builds nothing (<see cref="ContractTests.TheEvaluatorAnswersAsTheGeneratedParser"/>).
	/// </summary>
	[Fact]
	public void TheCsvRefusalCallsTheHostAsTheCSharpReaderDoes()
	{
		var fixture = Cases.Fixtures["Csv"];

		var (ours, ourLog, _)     = Reading.Evaluated(fixture.Grammar, fixture.Entry, "a,1\nb;2\n", fixture.Guards);
		var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, "a,1\nb;2\n");

		Assert.False(ours.Accepted);
		Assert.Equal(theirs.Line, ours.Line);
		Assert.Equal(theirLog.Take(ourLog.Count), ourLog);
		Assert.Equal(["g1(\"a\")=true", "g1(\"b\")=true"], theirLog.Skip(ourLog.Count));
	}

	/// <summary>
	/// How often a fixture's lowering found no answer in the plan and asked its own proofs: a choice
	/// the reader read by a switch on its first character, or a node of a rule the reader wrote in
	/// place where it is called, has none under the rule's own name.
	/// </summary>
	[Fact]
	public void TheMissesOfThePlanAreCounted()
	{
		var asked  = 0;
		var missed = 0;

		// One lowering a grammar: two fixtures may read one grammar from two entries.
		foreach (var (name, fixture) in Cases.Fixtures.DistinctBy(static one => one.Value.Grammar))
		{
			var program = Reading.Program(fixture.Grammar);

			output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{name}: asked {program.PlanAsked}, missed {program.PlanMisses}"));

			asked  += program.PlanAsked;
			missed += program.PlanMisses;
		}

		output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"all: asked {asked}, missed {missed}"));

		Assert.Equal((Asked, Missed), (asked, missed));
	}

	/// <summary>
	/// The choices and repetitions of the fixtures, and those the plan has no reader's answer for: a
	/// grammar the C# generator reads by its flat machine has no readers, and so no such decisions.
	/// </summary>
	const int Asked  = 166;
	const int Missed = 2;
}
