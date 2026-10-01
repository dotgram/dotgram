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

	/// <summary>
	/// The window form over tokens answers as the form from a position does where nothing is
	/// left to read, and never moves the position back to the start of the text.
	/// </summary>
	/// <remarks>
	/// <c>?! 'a'i?</c> is <c>(?! 'a'i)?</c> (§3.8): it reads nothing and succeeds. Over tokens
	/// a reading begins at a token, and where none is left the answer is Starved — which the
	/// form from a position said for <c>bb</c> and the window form did not: it read nothing
	/// out of a window cut into no token, and moved <c>at</c> to 0.
	/// </remarks>
	[Fact]
	public void A_window_over_tokens_with_no_token_in_it_is_refused_and_leaves_the_position()
	{
		var result = GramCompiler.Compile(
			"trivia = { ' '* }\nStart = ?! 'a'i?\nparse Start\n",
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal((false, 0), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "bb", 0)));
		Assert.Equal((false, 2), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "##bb##", 2, 2)));
	}

	/// <summary>
	/// A reading of nothing from a position ends where it began — at the first token at or after
	/// the position — in every form that begins where it is told, rather than at the start of the
	/// text or at the end of the token before it.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_reading_of_nothing_over_tokens_ends_where_it_began(bool direct)
	{
		var result = GramCompiler.Compile(
			"trivia = { ' '* }\nStart = ('b' | 'b' & 'c')?\nOther = 'a' & 'c'\nparse Start\nparse Other\n",
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal((true, 3), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "a  a", 2)));
		Assert.Equal((true, 3), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "a  a", 2, 2)));
		Assert.Equal((true, 3), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "a  ab", 2, 2)));
		Assert.Equal((true, 4), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "a  b b", 2)));

		Assert.Equal((true, (string?)null, 3L, 0L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "a  a", 2));
		Assert.Equal((true, (string?)null, 3L, 0L), EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "a  a", 2, 2));

		var empty = EmittedCode.Positioned(assembly, "Grammar", "TryParseStart", "##  ##", 2, 2);

		Assert.False(empty.IsSuccess);
		Assert.Equal(4L, empty.Position);
	}

	static (bool Read, int At) Answered((bool Read, object? Value, int At) answer)
	{
		return (answer.Read, answer.At);
	}
}
