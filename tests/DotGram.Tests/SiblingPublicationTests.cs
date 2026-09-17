using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

public sealed class SiblingPublicationTests
{
	[Theory]
	[InlineData(8, false, 2)]
	[InlineData(130, false, 1)]
	[InlineData(130, true, 2)]
	public void Shared_rules_preserve_root_results_and_end_of_input(int count, bool guard, int materializers)
	{
		var grammar = string.Join("\n", Enumerable.Range(0, count - 1)
			.Select(i => $"R{i} : @int = n: R{i + 1} => @(n + 1)")) +
			$"\nR{count - 1} : @int = '(' & n: R0 & ')' => @(n + 1) | 'v' => @(1)\n" +
			"Number : @int = n: R0" + (guard ? " & when @(n > 0)" : "") + " => @(n)\n" +
			"Text : @string = n: R0 => @(n.ToString())\nparse Number\nparse Text";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		var methods = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
			.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>();
		Assert.Equal(materializers, methods.Count(m => m.Identifier.Text.StartsWith("Materialize_DotGram_") && m.Identifier.Text.EndsWith("_Direct")));
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(count * 3, EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "((v))").Value);
		Assert.Equal((count * 3).ToString(), EmittedCode.Match(assembly, "Grammar", "TryParseText", "((v))").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "v!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseText", "v!").IsSuccess);
	}

	[Fact]
	public void Joined_publications_preserve_each_roots_failure_expectations()
	{
		var rules = string.Join("\n", Enumerable.Range(0, 129)
			.Select(i => $"R{i} : @int = n: R{i + 1} => @(n + 1)")) +
			"\nR129 : @int = '(' & n: R0 & ')' => @(n + 1) | 'v' => @(1)\n" +
			"Number : @int = 'n' & n: R0 => @(n)\n" +
			"Text : @string = 't' & n: R0 => @(n.ToString())\n";
		var options = new GramCompilerOptions
		{
			Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance,
		};
		var together = GramCompiler.Compile(rules + "parse Number\nparse Text", options);
		EmittedCode.Quiet(together.Diagnostics);
		var combined = EmittedCode.Compile(Assert.Single(together.Sources).Text);

		foreach (var root in new[] { "Number", "Text" })
		{
			var separate = GramCompiler.Compile(rules + "parse " + root, options);
			// The other publication root is deliberately unused in this control grammar.
			EmittedCode.Quiet(separate.Diagnostics.Where(diagnostic => diagnostic.Id != "GRAM4018"));
			var single = EmittedCode.Compile(Assert.Single(separate.Sources).Text);

			foreach (var input in new[] { "", "!", "n!", "t!", "n(v", "t(v" })
			{
				var expected = EmittedCode.Match(single, "Grammar", "TryParse" + root, input);
				var actual = EmittedCode.Match(combined, "Grammar", "TryParse" + root, input);
				Assert.False(actual.IsSuccess);
				Assert.Equal(expected.Position, actual.Position);
				Assert.Equal(expected.Error, actual.Error);
			}
		}
	}
}
