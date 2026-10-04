using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Over kinds a repetition inside a rule gives no turn back, on the reader and on the engine
/// alike.
/// </summary>
/// <remarks>
/// <para>
/// Over kinds a choice is decided by the token in front of it and never revisited, and only
/// inside a rule marked <c>?</c> is a choice or a repetition revisited when something later in
/// the rule fails (docs/syntax.md §4). A turn that matched is kept, and what the rule wanted
/// after the repetition is not there any more. That is why GRAM5009 reports a repetition
/// whose turn can take what follows it even inside one rule.
/// </para>
/// <para>
/// The engine revisited a turn that matched, as it does over characters, so a rule that fell
/// to it read what the reader refused. It now reads each choice and repetition of a rule not
/// marked <c>?</c> as if braced, and the two renderings answer alike.
/// </para>
/// <para>
/// Documentation as much as a test: if either rendering ever gives turns back, GRAM5009's
/// premise for repetitions inside a rule is false and this is where that shows.
/// </para>
/// </remarks>
public sealed class RepetitionTurnTests
{
	const string Grammar =
		"""
		wordboundary = ['a'..'z' | '_']
		trivia = { ' '* }
		namespace Lexical
		{
			trivia = none
			Name = ['a'..'z' | '_'] & ['a'..'z' | '_']*
		}
		Start = Lexical.Name & ('.' & Lexical.Name)* & '.' & "end"
		parse Start
		""";

	[Theory]
	[InlineData("a.b.end")]
	[InlineData("a.end")]
	public void Neither_the_reader_nor_the_engine_gives_the_turn_back(string input)
	{
		Assert.False(Reads(input, reader: true));
		Assert.False(Reads(input, reader: false));
	}

	/// <summary>And a rule marked <c>?</c> gives it back on both.</summary>
	[Theory]
	[InlineData("a.b.end")]
	[InlineData("a.end")]
	public void A_rule_that_gives_back_gives_the_turn_back_on_both(string input)
	{
		var grammar = Grammar.Replace("Start = ", "Start = Dotted\nDotted? = ", System.StringComparison.Ordinal);

		Assert.True(Reads(input, reader: true, grammar));
		Assert.True(Reads(input, reader: false, grammar));
	}

	static bool Reads(string input, bool reader, string grammar = Grammar)
	{
		var source = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = true, Direct = reader,
		}).Sources[0].Text;

		// The reader is the one asked about, and a grammar it declined would be read by the
		// engine twice.
		Assert.Equal(reader, source.Contains("ref struct Reader_", System.StringComparison.Ordinal));

		return EmittedCode.Match(EmittedCode.Compile(source), "Grammar", "TryParseStart", input).IsSuccess;
	}
}
