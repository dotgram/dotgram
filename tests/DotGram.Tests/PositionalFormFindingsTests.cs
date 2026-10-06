using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Defects met while testing ignore-case literals, none of them about case: each is the same
/// without ignore-case literals and over ASCII alone.
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
	/// The same cause by another route: a published set of terminals called from elsewhere
	/// is still entered by name, and reads one token over kinds.
	/// </summary>
	[Theory]
	[InlineData("trivia = { ' '* }\nStart = X & Y\nX = (\"ac\" | \"b\")\nY = (\"c\" | \"dd\")\nparse Start\nparse X\n", "ParseX", "ac")]
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
	/// A published rule that is no token of its own over kinds — a rule of the seam, a helper
	/// a terminal calls — is not read over kinds: the grammar is read over characters and says
	/// so, rather than throwing.
	/// </summary>
	[Theory]
	[InlineData("trivia = { Sp }\nSp = ' '*\nStart = \"a\" & \"bb\"\nparse Start\nparse Sp\n", "TryParseSp", "  ", true)]
	[InlineData("trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tDigit = ['0'..'9']\n\tNumber = Digit & Digit*\n}\nStart = Lex.Number & ',' & Lex.Number\nparse Start\nparse Lex.Digit\n", "TryParseDigit", "1", true)]
	[InlineData("trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tDigit = ['0'..'9']\n\tNumber = Digit & Digit*\n}\nStart = Lex.Number & ',' & Lex.Number\nparse Start\nparse Lex.Digit\n", "TryParseDigit", " 1", false)]
	public void A_published_rule_that_is_no_token_of_its_own_is_read_over_characters(string grammar, string method, string input, bool read)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.Contains(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);
		Assert.Equal(read, EmittedCode.Match(EmittedCode.Compile(result.Sources[0].Text), "Grammar", method, input).IsSuccess);
	}

	/// <summary>
	/// The window form over tokens answers as the form from a position does where no token is
	/// left to begin a reading, and never moves the position back to the start of the text.
	/// </summary>
	/// <remarks>
	/// <c>?! 'a'i?</c> is <c>(?! 'a'i)?</c> (§3.8): it reads nothing and succeeds. A character no
	/// token begins with is not input that ran out, so over tokens it answers as over characters
	/// (§6.3): the rule reads nothing there, where the window form once read nothing out of a
	/// window cut into no token and moved <c>at</c> to 0.
	/// </remarks>
	[Fact]
	public void A_window_over_tokens_with_no_token_in_it_reads_nothing_where_the_rule_may()
	{
		var result = GramCompiler.Compile(
			"trivia = { ' '* }\nStart = ?! 'a'i?\nparse Start\n",
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		Assert.Equal((true, 0), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "bb", 0)));
		Assert.Equal((true, 2), Answered(EmittedCode.Answered(assembly, "Grammar", "TryParseStart", "##bb##", 2, 2)));
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

	/// <summary>
	/// A published rule of a namespace without trivia, beside <c>parse Start</c>, reads over tokens
	/// as it does over characters: one token and no trivia around it — and the grammar is still
	/// cut into tokens for it.
	/// </summary>
	/// <remarks>
	/// Over tokens such a publication is one token test, compiled as a plain method, so it has
	/// only the whole forms; over characters it has them too, and those are compared.
	/// </remarks>
	[Theory]
	[InlineData("1.5")]
	[InlineData(" 1.5")]
	[InlineData("1.5 ")]
	[InlineData(" 1.5 ")]
	[InlineData("1 .5")]
	[InlineData("1. 5")]
	[InlineData("12")]
	[InlineData("")]
	public void A_published_rule_of_a_lexical_namespace_reads_as_over_characters(string input)
	{
		const string grammar = """
			trivia = { ' '* }
			namespace Lex
			{
				trivia = none
				Digits = ['0'..'9'] & ['0'..'9']*
				Number = Digits & ('.' & Digits)?
			}
			Start = Lex.Number & (',' & Lex.Number)*
			parse Start
			parse Lex.Number
			""";

		var characters = Whole(grammar, lexical: false, direct: true, input);

		foreach (var direct in new[] { false, true })
			Assert.Equal(characters, Whole(grammar, lexical: true, direct: direct, input));
	}

	static string Whole(string grammar, bool lexical, bool direct, string input)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", Lexical = lexical, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);
		var match    = EmittedCode.Match(assembly, "Grammar", "TryParseNumber", input);
		var start    = EmittedCode.Match(assembly, "Grammar", "TryParseStart", " 1.5 , 2 ");
		var type     = assembly.GetType("Grammar")!;
		var only     = type.GetMethods().Single(one =>
			one.Name == "TryParseNumber" && one.GetParameters() is { Length: 2 } taken && taken[1].IsOut);
		var answered = new object?[] { input, null };
		var read     = (bool)only.Invoke(null, answered)!;

		return $"{match.IsSuccess} {(match.IsSuccess ? match.Value : null)}; {read} {answered[1]}; start {start.IsSuccess}";
	}

	/// <summary>
	/// An alternative after an optional one can never be taken, and the generated parser said so
	/// with CS0162 — a warning in the consumer's build, an error under warnings-as-errors, from a
	/// file they did not write. In braces the choice is scanned, and the scanner ends it there;
	/// without them it is not scanned at all, and the later alternative is still reached by
	/// giving back.
	/// </summary>
	[Theory]
	[InlineData("\"abc\"?", true)]
	[InlineData("\"abc\"i?", true)]
	[InlineData("'b'?", true)]
	[InlineData("\"\u0130\u00B5\u017F\"?", true)]
	[InlineData("\"\u0130\u00B5\u017F\"i?", true)]
	[InlineData("'b'?", false)]
	[InlineData("\"\u0130\u00B5\u017F\"i?", false)]
	public void An_alternative_after_an_optional_one_leaves_no_unreachable_code(string optional, bool braced)
	{
		var choice = $"({optional} | 'a'i)";
		var result = GramCompiler.Compile(
			$"trivia = {{ ' '* }}\nStart = R1\nR1 = {(braced ? "{ " + choice + " }" : choice)}\nparse Start\n",
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);
		EmittedCode.Compile(result.Sources[0].Text);
	}

	/// <summary>
	/// The same dead code by other shapes: an alternative that cannot fail anywhere in a
	/// choice a scanner reads, the choices of the seam included.
	/// </summary>
	[Theory]
	[InlineData("trivia = { ' '* }\nStart = R1\nR1 = { ('b'* | 'a') }\nparse Start\n", false)]
	[InlineData("trivia = { ' '* }\nStart = R1\nR1 = { ('d' | 'b'? | 'a' | 'c') }\nparse Start\n", false)]
	[InlineData("trivia = { ' '* }\nStart = R1\nR1 = { (('b' | 'c')? | 'a') }\nparse Start\n", false)]
	[InlineData("trivia = { (' '* | '\\t') }\nStart = 'a' & 'c'\nparse Start\n", false)]
	[InlineData("trivia = { (' '* | '\\t') }\nStart = 'a' & 'c'\nparse Start\n", true)]
	public void An_alternative_that_cannot_fail_ends_a_scanned_choice(string grammar, bool lexical)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);
		EmittedCode.Compile(result.Sources[0].Text);
	}

	static (bool Read, int At) Answered((bool Read, object? Value, int At) answer)
	{
		return (answer.Read, answer.At);
	}
}
