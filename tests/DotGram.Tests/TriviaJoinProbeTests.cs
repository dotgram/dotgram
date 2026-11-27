using System.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>Scratch probes: what trivia gives in a string joined from its pieces.</summary>
public sealed class TriviaJoinProbeTests
{
	[Fact]
	public void Written_trivia_before_a_built_piece_gives_nothing()
	{
		Assert.Equal("W:ab", Parsed(
			"trivia = { ' '* }\n" +
			"Lex(item) : item = trivia & item\n" +
			"Word : @string = c: ['a'..'z']+ => @(\"W:\" + c.ToString())\n" +
			"Start : Word = Lex(Word)",
			" ab"));
	}

	[Fact]
	public void Woven_trivia_between_two_pieces_gives_its_text()
	{
		Assert.Equal("a . b", Parsed(
			"trivia = { ' '* }\n" +
			"Start : @string = Ident & '.' & Ident\n" +
			"Ident = ['a'..'z']+",
			"a . b"));
	}

	static string? Parsed(string grammar, string input)
	{
		var result = GramCompiler.Compile(
			grammar + "\nparse Start",
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != "GRAM4018"));

		var (isSuccess, value, error, _) = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)),
			"Grammar",
			"TryParseStart",
			input);

		Assert.True(isSuccess, error);

		return (string?)value;
	}
}
