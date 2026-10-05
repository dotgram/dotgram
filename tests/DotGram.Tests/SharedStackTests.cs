using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Two members of one rule gathered across turns, of one type: each is its own list, however
/// the carrier keeps them. The immediate carrier gathers onto a stack per type, and each
/// factory takes what the rule pushed since it began, so it does not carry a rule where one
/// factory takes two such members; the tape — whose references carry the slot they were pushed
/// for — does. Two members in alternatives with a factory each never meet, an alternative that
/// fails giving back what it pushed, and the immediate carrier carries them.
/// </summary>
public sealed class SharedStackTests
{
	const string Sequential = "S : @string = a: X* & ';' & b: X* & eof => @(Joined(a, b))";
	const string Grouped    = "S : @string = (a: X* & ';' | b: X* & '!') => @(Joined(a, b))";

	// The shape of the joins and descriptor rules of standard SQL: one stack, two factories.
	const string Apart =
		"S : @string = a: X* & ';' => @(Joined(a, null))\n" +
		"            | b: X* & '!' => @(Joined(null, b))";

	// The first alternative pushes its turns and a turn more before it fails, deeper than the
	// gathered member itself; the second has to start from an empty stack all the same.
	const string FailsLate =
		"S : @string = a: X* & ';' & X & '.' => @(Joined(a, null))\n" +
		"            | b: X* & ';' & X & '!' => @(Joined(null, b))";

	// Two factories, each over a member of its own, beside a third reading neither.
	const string ApartAndPlain =
		"S : @string = a: X* & ';' => @(Joined(a, null))\n" +
		"            | '#' & t: X => @(t)\n" +
		"            | b: X* & '!' => @(Joined(null, b))";

	[Theory]
	[InlineData(Sequential,    "ab;cd")]
	[InlineData(Grouped,       "ab!")]
	[InlineData(Grouped,       "ab;")]
	[InlineData(Apart,         "ab;")]
	[InlineData(Apart,         "ab!")]
	[InlineData(Apart,         "ab?")]
	[InlineData(Apart,         "!")]
	[InlineData(FailsLate,     "ab;c.")]
	[InlineData(FailsLate,     "ab;c!")]
	[InlineData(FailsLate,     "ab;c?")]
	[InlineData(ApartAndPlain, "#q")]
	[InlineData(ApartAndPlain, "abc!")]
	public void Each_member_keeps_its_own_turns(string rule, string input)
	{
		var grammar = Grammar(rule);

		var tape = Parse(grammar, CarrierKind.Tape, input);

		Assert.Equal(tape, Parse(grammar, CarrierKind.Auto, input));
		Assert.Equal(tape, Parse(grammar, CarrierKind.Immediate, input));
	}

	/// <summary>One factory taking two members of one stack is refused, and the tape reads it.</summary>
	[Theory]
	[InlineData(Sequential)]
	[InlineData(Grouped)]
	public void One_factory_over_two_members_of_one_stack_is_refused(string rule)
	{
		var refused = Refusal(Grammar(rule));

		Assert.NotNull(refused);
		Assert.Contains("'S' gathers two members onto one stack (string)", refused, StringComparison.Ordinal);
	}

	/// <summary>Members of one stack in alternatives with a factory each are carried immediately.</summary>
	[Theory]
	[InlineData(Apart)]
	[InlineData(FailsLate)]
	[InlineData(ApartAndPlain)]
	public void Members_of_separate_factories_are_carried(string rule)
	{
		Assert.Null(Refusal(Grammar(rule)));
	}

	/// <summary>Every rule the carrier refuses is named, not the first of them alone.</summary>
	[Fact]
	public void Every_refusing_rule_is_named()
	{
		var refused = Refusal(
			"X : @string = t: ['a'..'z'] => @(t)\n" +
			"S : @string = a: X* & ';' & b: X* => @(Joined(a, b))\n" +
			"T : @string = c: X* & '!' & d: X* => @(Joined(c, d))\n" +
			"U : @string = s: S & t: T => @(s + t)\n" +
			"parse U");

		Assert.NotNull(refused);
		Assert.Contains("'S' gathers two members onto one stack", refused, StringComparison.Ordinal);
		Assert.Contains("'T' gathers two members onto one stack", refused, StringComparison.Ordinal);
	}

	const string Members = """
		static string Joined(string[] a, string[] b) =>
			string.Join(",", a ?? new string[0]) + "|" + string.Join(",", b ?? new string[0]);
		""";

	static string Grammar(string rule)
	{
		return "X : @string = t: ['a'..'z'] => @(t)\n" + rule + "\nparse S";
	}

	/// <summary>What the immediate carrier says when asked for and refusing, or null where it carries the grammar.</summary>
	static string? Refusal(string grammar)
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = CarrierKind.Immediate, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		return compiled.Diagnostics.SingleOrDefault(static one => one.Id == GramCompiler.CarrierRefused)?.Message;
	}

	static string? Parse(string grammar, CarrierKind carrier, string input)
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(compiled.Diagnostics);

		var host  = EmittedCode.Compile(Assert.Single(compiled.Sources).Text, declarationMembers: Members).GetType("Grammar")!;
		var match = host.GetMethod("TryParseS", [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? (string?)match.GetType().GetProperty("Value")!.GetValue(match)
			: "<refused>";
	}
}
