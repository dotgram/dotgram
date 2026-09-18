using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A terminal the lexer begins and a rule ends keeps its kind where a longer pattern starts
/// the same way.
/// </summary>
/// <remarks>
/// <para>
/// A kind is every pattern that accepted the longest string. The automaton reads only the
/// beginning of a terminal whose rest is a rule — the quote of a character string — so where
/// another pattern starts with that quote and goes on (an interval string, <c>'12'</c>), the
/// automaton stopped on the other pattern's kind and never measured the character string at
/// all: <c>'+01:00'</c> stopped being a string literal anywhere in SQL:2023, interval or not
/// (found by sql-ff). The lexer now measures the rest there too.
/// </para>
/// <para>
/// <c>Rest</c> calls itself, so no automaton reads it: that is what makes <c>Text</c> a
/// terminal the lexer begins rather than one more pattern.
/// </para>
/// </remarks>
public sealed class BegunTerminalKindTests
{
	const string Grammar =
		"""
		trivia = { ' '* }
		namespace Lexical
		{
			trivia = none
			Text     = '\'' & Rest
			Rest     = [^ '\'']* & '\'' & ('\'' & Rest)?
			Interval = '\'' & ['0'..'9']+ & '\''
		}
		AsText     = Lexical.Text
		AsInterval = Lexical.Interval
		parse AsText
		parse AsInterval
		""";

	[Theory]
	[InlineData("'12'", true, true)]      // as long either way: both kinds
	[InlineData("'ab'", true, false)]     // only the rule reads it
	[InlineData("'12''3'", true, false)]  // the rule reads further: its kind alone
	[InlineData("'1", false, false)]      // neither ends
	public void A_token_both_read_as_far_is_both(string input, bool text, bool interval)
	{
		var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = true,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = compiled.Sources[0].Text;

		Assert.Contains("Tokenize_DotGram(", source, System.StringComparison.Ordinal);

		var assembly = EmittedCode.Compile(source);

		Assert.Equal(text,     EmittedCode.Match(assembly, "Grammar", "TryParseAsText", input).IsSuccess);
		Assert.Equal(interval, EmittedCode.Match(assembly, "Grammar", "TryParseAsInterval", input).IsSuccess);
	}
}
