using System;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Four defects met while testing ignore-case literals, none of them about case: each is the
/// same on the parent commit and over ASCII alone.
/// </summary>
public sealed class PositionalFormFindingsTests
{
	/// <summary>A grammar cut into tokens whose one rule is all tokens throws in the generator.</summary>
	[Fact]
	public void A_grammar_cut_into_tokens_whose_start_is_one_choice_of_keywords_generates()
	{
		var result = GramCompiler.Compile(
			"trivia = { ' '* }\nStart = (\"baa\"i | \"A1\"i)\nparse Start\n",
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.NotEmpty(result.Sources);
	}

	/// <summary>
	/// The same cause by other routes: a published rule the lexer holds — a set of terminals,
	/// a terminal of its own — is still entered by name, and reads one token over kinds.
	/// </summary>
	[Theory]
	[InlineData("trivia = { ' '* }\nStart = X & Y\nX = (\"ac\" | \"b\")\nY = (\"c\" | \"dd\")\nparse Start\nparse X\n", "ParseX", "ac")]
	[InlineData("trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tDigits = ['0'..'9'] & ['0'..'9']*\n}\nStart = Lex.Digits & ',' & Lex.Digits\nparse Start\nparse Lex.Digits\n", "ParseDigits", "12")]
	public void A_published_rule_the_lexer_holds_reads_one_token(string grammar, string method, string input)
	{
		foreach (var direct in new[] { false, true })
		{
			var result = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions { ClassName = "Grammar", Lexical = true, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance });

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(result.Sources[0].Text);
			var parse    = assembly.GetType("Grammar")!.GetMethod(method, [typeof(string)])!;

			Assert.Equal(input, parse.Invoke(null, [input]));
		}
	}

	/// <summary>
	/// A published rule the seam is made of has no kind: the lexer skips it rather than reads
	/// it. The grammar is read over characters and says so, rather than throwing.
	/// </summary>
	[Fact]
	public void A_published_rule_of_the_seam_is_read_over_characters()
	{
		var result = GramCompiler.Compile(
			"trivia = { Sp }\nSp = ' '*\nStart = \"a\" & \"bb\"\nparse Start\nparse Sp\n",
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.Contains(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);
		Assert.True(EmittedCode.Match(EmittedCode.Compile(result.Sources[0].Text), "Grammar", "TryParseSp", "  ").IsSuccess);
	}
}
