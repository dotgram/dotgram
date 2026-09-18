using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Over kinds a repetition inside a rule gives no turn back on the reader, and does on the
/// engine.
/// </summary>
/// <remarks>
/// <para>
/// A rule's answer stands over kinds (docs/syntax.md §4), and the reader is what makes it
/// stand: a turn that matched is kept, and what the rule wanted after the repetition is not
/// there any more. The engine revisits a choice that matched when something later fails, so
/// it takes the turn back and reads on. That is the difference GRAM5005 warns of when a rule
/// of a split grammar falls to the engine, and it is why GRAM5009 reports a repetition whose
/// turn can take what follows it even inside one rule: a shape that reads only because its
/// rule went to the engine reads by accident of the rendering.
/// </para>
/// <para>
/// Documentation as much as a test: if the reader ever gives turns back, GRAM5009's premise
/// for repetitions inside a rule is false and this is where that shows.
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
	public void The_reader_keeps_the_turn_and_the_engine_gives_it_back(string input)
	{
		Assert.False(Reads(input, reader: true));
		Assert.True(Reads(input, reader: false));
	}

	static bool Reads(string input, bool reader)
	{
		var source = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = true, Direct = reader,
		}).Sources[0].Text;

		// The reader is the one asked about, and a grammar it declined would be read by the
		// engine twice.
		Assert.Equal(reader, source.Contains("ref struct Reader_", System.StringComparison.Ordinal));

		return EmittedCode.Match(EmittedCode.Compile(source), "Grammar", "TryParseStart", input).IsSuccess;
	}
}
