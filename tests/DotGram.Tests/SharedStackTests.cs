using System;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Two members of one rule gathered across turns, of one type: each is its own list, however
/// the carrier keeps them. The immediate carrier gathers onto a stack per type and takes what the
/// rule pushed since it began, so it does not carry such a rule, and the tape — whose references
/// carry the slot they were pushed for — does.
/// </summary>
public sealed class SharedStackTests
{
	[Theory]
	[InlineData("S : @string = a: X* & ';' & b: X* & eof => @(Joined(a, b))", "ab;cd")]
	[InlineData("S : @string = (a: X* & ';' | b: X* & '!') => @(Joined(a, b))", "ab!")]
	[InlineData("S : @string = (a: X* & ';' | b: X* & '!') => @(Joined(a, b))", "ab;")]
	public void Each_member_keeps_its_own_turns(string rule, string input)
	{
		var grammar = "X : @string = t: ['a'..'z'] => @(t)\n" + rule + "\nparse S";

		var tape = Parse(grammar, CarrierKind.Tape, input);

		Assert.Equal(tape, Parse(grammar, CarrierKind.Auto, input));
		Assert.Equal(tape, Parse(grammar, CarrierKind.Immediate, input));
	}

	const string Members = """
		static string Joined(string[] a, string[] b) =>
			string.Join(",", a ?? new string[0]) + "|" + string.Join(",", b ?? new string[0]);
		""";

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
