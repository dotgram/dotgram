using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every carrier a grammar can be compiled with answers the same on every input — the same
/// verdict, and the same value where there is one.
/// </summary>
/// <remarks>
/// <para>
/// The carriers differ in when the author's constructions run and where the pieces of a
/// value wait; they may not differ in what comes out. This is the agreement test the
/// hand-written readings in <c>benchmarks/DotGram.HandDeferred</c> ran on themselves,
/// asked of the generator: the tape is the reference, because it is the one that keeps
/// §7.3, and everything else has to agree with it or it does not ship.
/// </para>
/// <para>
/// The grammars are the shapes the second carrier had to learn one at a time: a fold, a
/// cycle, members gathered across turns as records and as text, a guard that reads a
/// captured value, and a member that may be missing.
/// </para>
/// </remarks>
public sealed class CarrierTests
{
	static readonly (string Name, string Grammar, string[] Inputs)[] Shapes =
	[
		("a fold",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + bb = 22", "a=1", "a = 1 + bb", "a = ", ""]),

		("a cycle",
			"trivia = ' '*\n" +
			"Start : @string = l: Start & '+' & r: Pair => @(l + \"+\" + r)\n" +
			"                | one: Pair => @(one)\n" +
			"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
			"               | '(' & inner: Start & ')' => @(\"(\" + inner + \")\")\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["a = 1 + (b = 2 + c = 3)", "((a = 1))", "(a = 1", "a = 1 + (b = 2"]),

		("records gathered",
			"Start : @string = first: Name & (',' & rest: Name)* => @(first + \"|\" + string.Join(\"|\", rest))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a,b,c", "a", "a,", ",a"]),

		("text gathered",
			"Start : @string = (parts: ['a'..'z']+ & ','?)+ => @(parts)\n" +
			"parse Start\n",
			["ab,cd,e", "ab", "ab,,cd", ""]),

		("a guard over a record",
			"Start : @string = d: Digits & when @(d.Length < 3) => @(\"<\" + d + \">\")\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["12", "1234", "", "x"]),

		("marks laid over an operand",
			"state : @int\n" +
			"Start : @string = '(' & inner: Start with state @(9) & ';' => @(\"<\" + inner + \">\")\n" +
			"                | '(' & inner: Start with state @(1) & ')' => @(\"(\" + inner + \")\")\n" +
			"                | t: ['a'..'z']+ => @(string.Join(\",\", parserState.ToArray()) + \":\" + t)\n" +
			"parse Start\n",
			["a", "(a)", "((a))", "(a;", "(a"]),

		("a rule that reaches itself",
			"trivia = ' '*\n" +
			"Start : @string = '(' & inner: Start & ')' => @(\"(\" + inner + \")\")\n" +
			"                | t: Name => @(t)\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"parse Start\n",
			["a", "(a)", "( ( a ) )", "(a", ""]),

		("a rule that builds two ways",
			"Start : @string = a: Name & b: Digits? => @(\"n\" + a + (b ?? \"-\"))\n" +
			"                | d: Digits & c: Name? => @(\"d\" + d + (c ?? \"-\"))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["ab12", "12ab", "ab", "12", "", "1a2"]),

		("a member that may be missing",
			"Start : @string = a: Name & b: Digits? => @(a + (b ?? \"-\"))\n" +
			"Name : @string = t: ['a'..'z']+ => @(t)\n" +
			"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
			"parse Start\n",
			["ab12", "ab", "12", ""]),
	];

	public static IEnumerable<object[]> Every() => Shapes.Select(one => new object[] { one.Name });

	/// <summary>The immediate carrier agrees with the tape on every shape and every input.</summary>
	[Theory]
	[MemberData(nameof(Every))]
	public void Immediate_agrees_with_the_tape(string name)
	{
		var (_, grammar, inputs) = Shapes.Single(one => one.Name == name);

		var tape  = Compiled(grammar, CarrierKind.Tape);
		var immediate = Compiled(grammar, CarrierKind.Immediate);

		Assert.Contains("ImmediateValues", immediate.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", immediate.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("DirectValues", immediate.Source, StringComparison.Ordinal);

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(tape.Assembly,  "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(immediate.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(
				expected.IsSuccess == actual.IsSuccess,
				$"{name} on \"{input}\": the tape says {expected.IsSuccess}, immediate says {actual.IsSuccess}.");

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
		}
	}

	/// <summary>
	/// What the mixed carrier carries today, so that the theory above cannot pass by
	/// carrying nothing.
	/// </summary>
	/// <remarks>
	/// This is the list that grows as the shapes are written, and the one line to change
	/// when one is. A rule that builds one way, out of runs of text and of other rules'
	/// values, is the first of them.
	/// </remarks>
	[Fact]
	public void The_mixed_carrier_carries_a_rule_that_builds_one_way()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a member that may be missing").Grammar, CarrierKind.Mixed);

		Assert.Contains("private readonly struct Shape_Start", source, StringComparison.Ordinal);
		Assert.Contains("value = reader.shape_Start.Build(text);", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram", source, StringComparison.Ordinal);
		Assert.DoesNotContain("DirectValues", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// The mixed carrier agrees with the tape on every shape it carries, and leaves the
	/// rest to the tape.
	/// </summary>
	/// <remarks>
	/// One test for both, because which of the two a shape is changes as the carrier is
	/// written and the test should not have to be edited when it does. A shape it carries
	/// is held to the tape's answers; a shape it refuses is held only to being refused,
	/// which is what the fallback below is about.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Every))]
	public void Mixed_agrees_with_the_tape_on_what_it_carries(string name)
	{
		var (_, grammar, inputs) = Shapes.Single(one => one.Name == name);

		var tape  = Compiled(grammar, CarrierKind.Tape);
		var mixed = Compiled(grammar, CarrierKind.Mixed);

		if (!mixed.Source.Contains("Shape_", StringComparison.Ordinal))
		{
			Assert.Contains("Materialize_DotGram", mixed.Source, StringComparison.Ordinal);

			return;
		}

		Assert.DoesNotContain("Materialize_DotGram", mixed.Source, StringComparison.Ordinal);

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(tape.Assembly,  "Carried.Probe", "TryParseStart", input);
			var actual   = EmittedCode.Match(mixed.Assembly, "Carried.Probe", "TryParseStart", input);

			Assert.True(
				expected.IsSuccess == actual.IsSuccess,
				$"{name} on \"{input}\": the tape says {expected.IsSuccess}, mixed says {actual.IsSuccess}.");

			if (expected.IsSuccess)
				Assert.Equal(ValueOf(expected), ValueOf(actual));
		}
	}

	/// <summary>
	/// A rule that builds several ways is one shape and a byte saying which, rather than a
	/// shape for each and a virtual call to build it.
	/// </summary>
	[Fact]
	public void A_shape_of_several_constructions_says_which_it_holds()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a rule that builds two ways").Grammar, CarrierKind.Mixed);

		Assert.Contains("private readonly byte which;", source, StringComparison.Ordinal);
		Assert.Contains("switch (this.which)",          source, StringComparison.Ordinal);
		Assert.Contains("Shape_Start.Of0(",             source, StringComparison.Ordinal);
		Assert.Contains("Shape_Start.Of1(",             source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",    source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A rule that can be reached from itself is a class, because a value cannot contain
	/// itself; everything off every cycle stays a value held inside whatever captured it.
	/// </summary>
	[Fact]
	public void A_shape_that_can_reach_itself_is_a_class()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a rule that reaches itself").Grammar, CarrierKind.Mixed);

		Assert.Contains("private sealed class Shape_Start",   source, StringComparison.Ordinal);
		Assert.Contains("private readonly struct Shape_Name", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",          source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A fold is a base and a run of turns threaded through the turns themselves, and what
	/// it is worth is one loop over that run rather than a frame per turn.
	/// </summary>
	/// <remarks>
	/// A chain built the other way round would read the same language and put a depth limit
	/// where the tape has none: a thousand terms would be a thousand frames.
	/// </remarks>
	[Fact]
	public void A_fold_is_a_run_threaded_through_its_own_turns()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a fold").Grammar, CarrierKind.Mixed);

		Assert.Contains("private sealed class Step_Start",  source, StringComparison.Ordinal);
		Assert.Contains("internal Step_Start? Next;",       source, StringComparison.Ordinal);
		Assert.Contains("private readonly struct Base_Start", source, StringComparison.Ordinal);
		Assert.Contains("for (var turn = 0; turn < this._count; turn++)", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",       source, StringComparison.Ordinal);
	}

	/// <summary>
	/// What a repetition gathers is pushed on a stack as it is read and taken as one array
	/// where the record is written; a run of text keeps where its pieces stand, not what
	/// they say.
	/// </summary>
	[Fact]
	public void What_a_repetition_gathers_is_taken_as_one_array()
	{
		var (records, _) = Compiled(Shapes.Single(one => one.Name == "records gathered").Grammar, CarrierKind.Mixed);
		var (pieces,  _) = Compiled(Shapes.Single(one => one.Name == "text gathered").Grammar,   CarrierKind.Mixed);

		Assert.Contains("sealed class MixedValues",       records, StringComparison.Ordinal);
		Assert.Contains("values.PushShape_Name(",         records, StringComparison.Ordinal);
		Assert.Contains("private readonly Shape_Name[] _g", records, StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",      records, StringComparison.Ordinal);

		Assert.Contains("values.PushSpans(",              pieces,  StringComparison.Ordinal);
		Assert.Contains("private readonly long[] _g",       pieces,  StringComparison.Ordinal);
		Assert.DoesNotContain("Materialize_DotGram",      pieces,  StringComparison.Ordinal);
	}

	/// <summary>
	/// A carrier that cannot carry a grammar is not an error: the tape carries it instead,
	/// and the machine keeps the reason for whoever asks.
	/// </summary>
	/// <remarks>
	/// The mixed carrier carries nothing yet — its shapes are written one at a time — so it
	/// is the one to ask this of. What the test is really about is the fallback, which every
	/// carrier after this one will need on the way to being finished.
	/// </remarks>
	[Fact]
	public void A_carrier_that_refuses_a_grammar_leaves_it_to_the_tape()
	{
		var (source, _) = Compiled(Shapes.Single(one => one.Name == "a guard over a record").Grammar, CarrierKind.Mixed);

		Assert.Contains("DirectValues",         source, StringComparison.Ordinal);
		Assert.Contains("Materialize_DotGram",  source, StringComparison.Ordinal);
		Assert.DoesNotContain("ImmediateValues", source, StringComparison.Ordinal);
	}

	static string? ValueOf(object match) =>
		match.GetType().GetProperty("Value")?.GetValue(match)?.ToString();

	static (string Source, Assembly Assembly) Compiled(string grammar, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Carried",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
		});

		Assert.Empty(result.Diagnostics.Where(one => one.Severity == GramSeverity.Error));

		var source = Assert.Single(result.Sources).Text;

		return (source, EmittedCode.Compile(source, "Probe", "Carried"));
	}
}
