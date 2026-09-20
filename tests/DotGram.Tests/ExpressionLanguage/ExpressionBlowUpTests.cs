using System;
using System.Diagnostics;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// An unclosed string is refused in the time its length takes, not in the time its compositions
/// take (D57).
/// </summary>
/// <remarks>
/// <para>
/// Every string body of the grammar repeats a piece whose plain-text alternative is a run of one
/// or more characters — <c>(A+)*</c>, the shape that reads a text every way it can be cut into
/// pieces once something after it fails. What fails after it is the closing quote, so the input
/// that drives it is a string that never closes: before the runs were made atomic, twenty-four
/// characters after <c>$"</c> took twenty seconds, and each further character doubled it.
/// </para>
/// <para>
/// The plain and verbatim strings have the same shape and were never reachable: they are whole
/// lexemes, read by the lexer's automaton, which has no way back to take. Only the four forms
/// the automaton merely begins — the interpolated ones, where a rule reads the rest — could be
/// driven, and they are the four held here.
/// </para>
/// <para>
/// The bound is a hundredfold of what the fix leaves and a ten-millionth of what the defect
/// took, so a machine under load cannot fail it and the defect cannot pass it. Sixty-four
/// characters is chosen for the same reason: were the shape back, this would not be slow, it
/// would not finish.
/// </para>
/// </remarks>
public sealed class ExpressionBlowUpTests
{
	[Theory]
	// The four beginnings, with a plain run after each.
	[InlineData("$\"", "a")]
	[InlineData("$@\"", "a")]
	[InlineData("$\"\"\"", "a")]
	[InlineData("$$\"\"\"", "a")]
	// And doubled braces, which are a second shape and not the first one's input: `{{a}}` is
	// two escaped braces around a run, and it was also a hole holding `{a}` until the hole was
	// told it cannot begin with a brace. Found by driving the fixed places with texts other
	// than the one the first defect was found with, which is the only thing that shows a shape
	// is gone rather than an input.
	[InlineData("$\"", "{{a}}")]
	[InlineData("$@\"", "{{a}}")]
	public void An_unclosed_string_is_refused_at_once(string opens, string piece)
	{
		var text  = "(int x) => " + opens + Repeated(piece, 64);
		var watch = Stopwatch.StartNew();

		var match = ExpressionParser.TryParse(text);

		Assert.False(match.IsSuccess);
		Assert.True(
			watch.Elapsed.TotalSeconds < 5,
			$"{opens} with 64 of `{piece}` after it took {watch.Elapsed.TotalSeconds:F1} s.");
	}

	static string Repeated(string piece, int times)
	{
		var built = new System.Text.StringBuilder(piece.Length * times);

		for (var at = 0; at < times; at++)
			built.Append(piece);

		return built.ToString();
	}
}
