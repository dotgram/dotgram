using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

public sealed class CommittedPrefixTests
{
	const string Grammar = """
		trivia = { ' '* }
		Start : @int
			= "if" & '(' & test: Condition & ')' & then: Branch & "else" & other: Branch => @(test * 100 + then * 10 + other)
			| "if" & '(' & test: Condition & ')' & then: Statement => @(test * 100 + then)
		Condition : @int = "x" & '+' & "x" => @(++Calls) | "x" => @(++Calls)
		Branch : @int = s: Statement => @(s) | "v" => @(2)
		Statement : @int = "s" => @(1)
		parse Start
		""";

	[Theory]
	[InlineData("if(x)s", 101)]
	[InlineData("if(x+x)s", 101)]
	[InlineData("if(x)s else s", 111)]
	[InlineData("if(x)v else s", 121)]
	[InlineData("if(x)s else", null)]
	public void Token_condition_is_built_once_and_kept_across_tails(string input, int? value)
	{
		var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical = true,
			Carrier = CarrierKind.Immediate,
		});
		EmittedCode.Quiet(compiled.Diagnostics);
		var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text,
			declarationMembers: "public static int Calls;");
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);
		Assert.Equal(value.HasValue, match.IsSuccess);
		if (value.HasValue)
			Assert.Equal(value.Value, match.Value);
		// Once per reading: a refused input is read a second time, recording (Q7.2).
		Assert.Equal(value.HasValue ? 1 : 2, assembly.GetType("Grammar")!.GetField("Calls")!.GetValue(null));
	}

	[Theory]
	[InlineData(false, "")]
	[InlineData(true, "?")]
	public void Prefixes_with_multiple_readings_keep_alternative_priority(bool lexical, string replay)
	{
		var grammar = """
			trivia = { ' '* }
			Start : @string = '(' & t: Item & "x" => @("first:" + t)
				| '(' & t: Item & "y" => @("second:" + t)
			ItemREPLAY : @string = "a" & "x" => @("long") | "a" => @("short")
			parse Start
			""".Replace("REPLAY", replay, StringComparison.Ordinal);
		foreach (var carrier in new[] { CarrierKind.Immediate, CarrierKind.Tape })
		{
			var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName = "Grammar", Lexical = lexical, Carrier = carrier,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});
			EmittedCode.Quiet(compiled.Diagnostics);
			var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text);
			if (!lexical)
			{
				Assert.Equal("first:short", EmittedCode.Match(assembly, "Grammar", "TryParseStart", "(a x").Value);
				Assert.Equal("second:long", EmittedCode.Match(assembly, "Grammar", "TryParseStart", "(a x y").Value);
			}

			// Different capture names prevent sharing without changing recognition.
			var baseline = GramCompiler.Compile(grammar.Replace(
				"t: Item & \"y\" => @(\"second:\" + t)",
				"u: Item & \"y\" => @(\"second:\" + u)", StringComparison.Ordinal),
				new GramCompilerOptions
				{
					ClassName = "Grammar", Lexical = lexical, Carrier = carrier,
					CSharpScanner = RoslynCSharpScanner.Instance,
				});
			var reference = EmittedCode.Compile(Assert.Single(baseline.Sources).Text);
			foreach (var input in new[] { "(a x", "(a x y", "(ax", "(axy", "(a y", "(a", "(x" })
				Assert.Equal(
					EmittedCode.Match(reference, "Grammar", "TryParseStart", input),
					EmittedCode.Match(assembly, "Grammar", "TryParseStart", input));
		}
	}
}
