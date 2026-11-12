using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A repetition that recovers with a <c>=&gt;</c> under a trivia that reads something: each turn is the
/// element with the trivia woven before it, and the element still builds the value the rejection takes
/// the place of. It was refused as a repetition that collects text (GRAM4010).
/// </summary>
public sealed class RecoveryUnderTriviaTests
{
	const string Script =
		"Let    : @string   = \"let\" & name: { ['a'..'z']+ } & ';' => name\n" +
		"Script : @string[] = Let* recover ';' => @(\"?\")\n" +
		"parse Script\n";

	[Theory]
	[InlineData("trivia = { ' '* }\n", "let a; let 1; let c;")]
	[InlineData("", "leta;let1;letc;")]
	public void A_recovery_factory_puts_the_rejection_among_the_values_with_trivia_or_without(string trivia, string input)
	{
		foreach (var carrier in new[] { CarrierKind.Immediate, CarrierKind.Deferred })
		{
			var result = GramCompiler.Compile(trivia + Script, new GramCompilerOptions
			{
				Carrier       = carrier,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text);
			var value    = (string[])EmittedCode.Match(assembly, "Grammar", "TryParseScript", input).Value!;

			Assert.Equal(["a", "?", "c"], value);
		}
	}

	[Fact]
	public void While_a_repetition_of_text_under_trivia_is_still_refused_its_factory()
	{
		var result = GramCompiler.Compile(
			"trivia = { ' '* }\nWord = ['a'..'z']+ & ';'\nScript : @string = Word* recover ';' => @(\"?\")\nparse Script",
			new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.Contains(result.Diagnostics, static one => one.Id == "GRAM4010");
	}

	/// <summary>
	/// Without a factory, what is reported for the rejected element names the element, <c>Let</c>, under
	/// trivia as without it; it used to be "an element" there, the turn read as a group.
	/// </summary>
	[Theory]
	[InlineData("trivia = { ' '* }\n", "let a; let 1; let c;")]
	[InlineData("", "leta;let1;letc;")]
	public void Without_a_factory_the_rejected_element_is_reported_by_its_rule(string trivia, string input)
	{
		const string Members =
			"public static string Said = \"\"; " +
			"static partial void OnRecovered(string rule, string text, long position, int line, int column, int ordinal, string message) " +
			"{ Said += rule + \";\"; }";

		var grammar =
			trivia +
			"Let    : @string   = \"let\" & name: { ['a'..'z']+ } & ';' => name\n" +
			"Script : @string[] = Let* recover ';'\n" +
			"parse Script\n";

		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members);

		Assert.Equal(["a", "c"], (string[])EmittedCode.Match(assembly, "Grammar", "TryParseScript", input).Value!);
		Assert.Equal("Let;", assembly.GetType("Grammar")!.GetField("Said")!.GetValue(null));
	}
}
