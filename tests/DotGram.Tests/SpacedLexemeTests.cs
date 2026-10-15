using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule spelled from single characters, declared where trivia is not empty, is told
/// (GRAM4043): <c>1 . 5</c> would be one number and <c>i f</c> one name.
/// </summary>
/// <remarks>
/// The condition is the narrow one §4.5 describes. What it must not tell is as much of the
/// contract as what it must: a run (<c>Word</c>, <c>Std.Digits</c>) may be a dotted name on
/// purpose, a literal of several characters has no seam inside it, and a rule that writes
/// <c>trivia</c> or <c>~</c> has said where the space goes.
/// </remarks>
public sealed class SpacedLexemeTests
{
	const string Spaced = "trivia = Std.Whitespace*\n";

	[Theory]
	[InlineData("Std.Digit+ & ('.' & Std.Digit+)?")]
	[InlineData("Std.Letter & (Std.Letter | Std.Digit)*")]
	[InlineData("'\\\\' & any")]
	[InlineData("Std.Digit+ & '.'")]
	[InlineData("{ Std.Digit+ & ('.' & Std.Digit+)? }")]
	[InlineData("digits: Std.Digit+ & ('.' & Std.Digit+)?")]
	[InlineData("['0'..'9']+ & ('.' & ['0'..'9']+)?")]
	[InlineData("['a'..'z'] & $x")]

	// The single-digit toy calculator, meant to be spaced: a false positive that is accepted.
	// A grammar that means it writes `trivia` where the space goes, or silences the number.
	[InlineData("Std.Digit & ('+' & Std.Digit)*")]
	public void A_rule_of_single_characters_is_told(string body)
	{
		if (body.Contains("$x", StringComparison.Ordinal))
			body = body.Replace("$x", "'x'", StringComparison.Ordinal);

		var told = Told(Spaced + $"R = {body}\nStart = R\nparse Start\n");

		var one = Assert.Single(told);

		Assert.Equal(GramSeverity.Warning, one.Severity);
		Assert.Contains("'R'", one.Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("\"if\"")]
	[InlineData("'<' & '>'")]
	[InlineData("'a' & 'b'")]
	[InlineData("Word & ('.' & Word)*")]
	[InlineData("Std.Digits & ('.' & Std.Digits)?")]
	[InlineData("['0'..'9']+")]
	[InlineData("Std.Digit+")]
	[InlineData("Std.Digit+ ~ ('.' ~ Std.Digit+)?")]
	[InlineData("{ Std.Digit+ ~ ('.' ~ Std.Digit+)? }")]
	[InlineData("Std.Digit+ & trivia & Std.Digit+")]
	[InlineData("Std.Digit & (trivia & '+' & trivia & Std.Digit)*")]
	[InlineData("List(['a'..'z'], ',')")]
	[InlineData("Std.Digit & ?!'-'")]
	public void What_is_meant_or_cannot_be_told_is_not(string body)
	{
		var told = Told(
			Spaced + "Word = Std.Letter+\nList(item, sep) = item & (sep & item)*\n" +
			$"R = {body}\nStart = R\nparse Start\n");

		Assert.Empty(told);
	}

	[Fact]
	public void A_rule_where_trivia_is_none_is_not_told()
	{
		var told = Told(
			Spaced +
			"namespace Lexical\n{\n\ttrivia = none\n\tNumber = Std.Digit+ & ('.' & Std.Digit+)?\n}\n" +
			"Start = Lexical.Number\nparse Start\n");

		Assert.Empty(told);
	}

	[Fact]
	public void A_rule_declared_where_trivia_is_none_by_default_is_not_told()
	{
		Assert.Empty(Told("Number = Std.Digit+ & ('.' & Std.Digit+)?\nStart = Number\nparse Start\n"));
	}

	[Fact]
	public void The_rules_trivia_is_made_of_are_not_told()
	{
		// Their seams are taken out again, so nothing can stand in them.
		var told = Told(
			"trivia = (Blank | Comment)*\nBlank = [' ' | '\\t'] & [' ' | '\\t']*\nComment = '#' & [^ '\\n']*\n" +
			"Start = \"ab\"\nparse Start\n");

		Assert.Empty(told);
	}

	[Fact]
	public void A_call_with_arguments_makes_a_rule_that_is_not_judged()
	{
		var told = Told(
			Spaced + "Pair(a, b) = a & b\nR = Pair(['a'..'z'], ['0'..'9'])\nStart = R\nparse Start\n");

		Assert.Empty(told);
	}

	[Fact]
	public void It_is_told_on_the_rule_name_once_whatever_calls_it()
	{
		var grammar = Spaced + "R = Std.Digit+ & '.'\nStart = R & R\nparse Start\n";
		var told    = Assert.Single(Told(grammar));

		Assert.Equal(grammar.IndexOf("R =", StringComparison.Ordinal), told.Position);
	}

	/// <summary>
	/// Over kinds such a rule would read <c>1 . 5</c> as three tokens, the same defect; the
	/// check does not depend on whether the cut into tokens succeeds.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_lexical_grammar_is_told_the_same(bool lexical)
	{
		var options = new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = lexical,
		};

		var told = GramCompiler.Compile(
				Spaced + "Number = Std.Digit+ & ('.' & Std.Digit+)?\nStart = Number\nparse Start\n", options)
			.Diagnostics
			.Where(static one => one.Id == GrammarNormalizer.SpacedLexeme)
			.ToArray();

		Assert.Single(told);
	}

	/// <summary>The path a build takes: the generator over a host file, its warning reported.</summary>
	[Fact]
	public void The_generator_reports_it_in_a_build()
	{
		var parse  = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var source =
			"[DotGram.Gram(\"trivia = Std.Whitespace*\\nNumber = Std.Digit+ & ('.' & Std.Digit+)?\\n" +
			"Start = Number\\nparse Start as Read\")] public partial class Parser;";

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.SpacedLexeme",
			[CSharpSyntaxTree.ParseText(source, parse, cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var run = CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGenerators(compilation, TestContext.Current.CancellationToken)
			.GetRunResult();

		var told = Assert.Single(run.Diagnostics, static one => one.Id == GrammarNormalizer.SpacedLexeme);

		Assert.Equal(DiagnosticSeverity.Warning, told.Severity);
	}

	static GramDiagnostic[] Told(string grammar)
	{
		return GramCompiler.Compile(
				grammar,
				new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance })
			.Diagnostics
			.Where(static one => one.Id == GrammarNormalizer.SpacedLexeme)
			.ToArray();
	}
}
