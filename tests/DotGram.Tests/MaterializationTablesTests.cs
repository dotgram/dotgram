using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

public sealed class MaterializationTablesTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Independent_publications_only_open_their_own_value_tables(bool direct)
	{
		var result = GramCompiler.Compile("""
			Number : @int = '(' & n: Number & ')' => @(n + 1) | 'n' => @(1)
			Text : @string = '[' & s: Text & ']' => @(s + "x") | 's' => @("s")
			parse Number
			parse Text
			""", new GramCompilerOptions { Direct = direct, Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		var methods = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken).GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
			.OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.Text.StartsWith("Materialize_DotGram")).ToArray();
		Assert.Equal(2, methods.Count(m => m.Identifier.Text is "Materialize_DotGram_Number" or "Materialize_DotGram_Text"
			or "Materialize_DotGram_Number_Direct" or "Materialize_DotGram_Text_Direct"));
		foreach (var method in methods.Where(m => m.Identifier.Text is "Materialize_DotGram_Number" or "Materialize_DotGram_Text"
			or "Materialize_DotGram_Number_Direct" or "Materialize_DotGram_Text_Direct"))
		{
			var tables = method.DescendantNodes().OfType<VariableDeclaratorSyntax>()
				.Count(v => v.Identifier.Text is "values0" or "values1");
			Assert.Equal(1, tables);
		}
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(3, EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "((n))").Value);
		Assert.Equal("sxx", EmittedCode.Match(assembly, "Grammar", "TryParseText", "[[s]]").Value);
	}
}
