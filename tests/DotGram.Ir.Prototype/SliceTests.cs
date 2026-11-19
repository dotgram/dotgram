using System;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// The generated slice: each fixture's tree printed as a TypeScript parser and as a C parser,
/// run, and held to the evaluator — the same verdict, the same value, the same furthest refusal
/// with the same things expected there, and the same host calls in the same order — and through
/// the evaluator to the C# parser the generator wrote.
/// </summary>
/// <remarks>
/// The host of both is the recording one: a fragment is a call of a host function by number.
/// A guard's answer decides the reading, so the host has to give the right one: it replays what
/// the guard answered over the same arguments on the C# side.
/// </remarks>
public sealed class SliceTests
{
	public static TheoryData<string> Names()
	{
		return [.. Cases.Fixtures.Keys];
	}

	static (IrProgram Program, List<SliceAnswer> Expected, List<string> GuardAnswers) Expected(Cases.Fixture fixture, IReadOnlyList<string> inputs)
	{
		var program  = Reading.Program(fixture.Grammar);
		var expected = inputs.Select(input => Slices.Evaluated(program, fixture.Entry, input, fixture.Guards)).ToList();

		return (program, expected, [.. expected.SelectMany(static answer => answer.Log).Where(static call => call.StartsWith('g')).Distinct()]);
	}

	static void Same(IReadOnlyList<string> inputs, List<SliceAnswer> expected, List<SliceAnswer> actual)
	{
		Assert.Equal(expected.Count, actual.Count);

		for (var i = 0; i < expected.Count; i++)
		{
			Assert.True(expected[i].Head == actual[i].Head, $"On {Twin.Quoted(inputs[i])}: expected {expected[i].Head}, read {actual[i].Head}");
			Assert.Equal(expected[i].Log, actual[i].Log);
		}
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void TypeScriptReadsAsTheEvaluator(string name)
	{
		var fixture = Cases.Fixtures[name];
		var inputs  = Cases.InputsOf(name);

		var (program, expected, answers) = Expected(fixture, inputs);

		Same(inputs, expected, Slices.TypeScript(name, program, fixture.Entry, inputs, guardAnswers: answers));
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void CReadsAsTheEvaluator(string name)
	{
		var fixture = Cases.Fixtures[name];
		var inputs  = Cases.InputsOf(name);

		var (program, expected, answers) = Expected(fixture, inputs);

		Same(inputs, expected, Slices.InC(name, program, fixture.Entry, inputs, guardAnswers: answers));
	}

	/// <summary>
	/// The JSON grammar on nst/JSONTestSuite's parsing cases, with the recording host: the
	/// evaluator against the generated C# parser, and each printed parser against the evaluator.
	/// </summary>
	[Fact]
	public void JsonOnItsCorpus_TheEvaluatorAnswersAsTheGeneratedParser()
	{
		var fixture = Cases.Fixtures["Json"];

		foreach (var (name, text) in HostTests.JsonCorpus())
		{
			var (ours, ourLog, _)     = Reading.Evaluated(fixture.Grammar, fixture.Entry, text);
			var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, text);

			Assert.True(theirs.Line == ours.Line, $"{name}: the generated parser says {theirs.Line}, the evaluator {ours.Line}");
			Assert.Equal(theirLog, ourLog);
		}
	}

	[Fact]
	public void JsonOnItsCorpus_TypeScriptReadsAsTheEvaluator()
	{
		var fixture = Cases.Fixtures["Json"];
		var inputs  = HostTests.JsonCorpus().Select(static one => one.Text).ToList();

		var (program, expected, answers) = Expected(fixture, inputs);

		Same(inputs, expected, Slices.TypeScript("JsonCorpus", program, fixture.Entry, inputs, guardAnswers: answers));
	}

	[Fact]
	public void JsonOnItsCorpus_CReadsAsTheEvaluator()
	{
		var fixture = Cases.Fixtures["Json"];
		var inputs  = HostTests.JsonCorpus().Select(static one => one.Text).ToList();

		var (program, expected, answers) = Expected(fixture, inputs);

		Same(inputs, expected, Slices.InC("JsonCorpus", program, fixture.Entry, inputs, guardAnswers: answers));
	}

	/// <summary>
	/// What a construction throws in TypeScript leaves the reading as it does in C#: at once, with
	/// the calls made so far and no other reading tried. C has nothing to throw, and what a host
	/// function that cannot build is to do there is not decided by this slice.
	/// </summary>
	[Fact]
	public void AnExceptionLeavesTheTypeScriptReadingAsItLeavesTheCSharpOne()
	{
		var answer = Slices.TypeScript("Throws", Reading.Program(Grammars.ActionTiming), "ParseStart", ["xy"], throwOn: "c1(\"xy\")").Single();

		Assert.Equal("T thrown by c1(\"xy\")", answer.Head);
		Assert.Equal(["c1(\"xy\")"], answer.Log);
	}
}
