using System;
using System.Collections.Generic;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class GuardCaptureRegressionTests
{
	public static IEnumerable<object[]> Readings()
	{
		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate, CarrierKind.Mixed })
			foreach (var direct in new[] { false, true })
				yield return [carrier, direct];
	}

	[Theory]
	[MemberData(nameof(Readings))]
	public void A_guard_sees_every_turn_of_a_text_capture(CarrierKind carrier, bool direct)
	{
		const string grammar = """
			trivia = ' '*
			D = ['0'..'9']
			T : @string = y: D{2} & when @(y == "12") => @(y!)
			parse T
			""";
		var assembly = Compile(grammar, carrier, direct);
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", "12");
		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal("12", match.Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseT", "22").IsSuccess);
	}

	[Theory]
	[MemberData(nameof(Readings))]
	public void A_guard_joins_only_retained_capture_pieces(CarrierKind carrier, bool direct)
	{
		foreach (var (body, input, expected) in new[]
		{
			("(y: D & ','?){2} & when @(y == \"12\")", "1,2", "12"),
			("y: D+ & '2' & when @(y == \"1\")", "12", "1"),
		})
		{
			var grammar = "D = ['0'..'9']\nT : @string = " + body + " => @(y!)\nparse T";
			var match = EmittedCode.Match(Compile(grammar, carrier, direct), "Grammar", "TryParseT", input);
			Assert.True(match.IsSuccess, match.Error);
			Assert.Equal(expected, match.Value);
		}
	}

	[Theory]
	[MemberData(nameof(Readings))]
	public void An_optional_group_beside_a_guard_compiles_and_runs(CarrierKind carrier, bool direct)
	{
		const string grammar = """
			T : @string = h: ['0'..'9'] & ('.' & f: ['0'..'9'])? & when @(true) => @(h! + (f ?? ""))
			parse T
			""";
		var assembly = Compile(grammar, carrier, direct);
		foreach (var (input, expected) in new[] { ("1", "1"), ("1.2", "12") })
		{
			var match = EmittedCode.Match(assembly, "Grammar", "TryParseT", input);
			Assert.True(match.IsSuccess, match.Error);
			Assert.Equal(expected, match.Value);
		}
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseT", "1.").IsSuccess);
	}

	static Assembly Compile(string grammar, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier = carrier,
			Direct = direct,
		});
		EmittedCode.Quiet(result.Diagnostics);
		return EmittedCode.Compile(Assert.Single(result.Sources).Text);
	}
}
