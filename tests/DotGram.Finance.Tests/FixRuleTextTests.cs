using System;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What a counterparty's dictionary is allowed to do to the text a validator is compiled from,
/// which is nothing.
/// </summary>
/// <remarks>
/// The dictionary is an untrusted file: it names messages, fields and components with whatever
/// its author typed, and those names and values end up inside generated source. Two doors, and
/// both are tested here rather than reasoned about — a value becomes a literal, a name becomes a
/// comment — together with the scanner that hands the compiler a comment-free copy of the same
/// geometry.
/// </remarks>
public sealed class FixRuleTextTests
{
	static string Literal(string value)
	{
		var text = new StringBuilder();

		FixRuleText.Literal(text, value);

		return text.ToString();
	}

	[Fact]
	public void A_value_becomes_a_literal_and_nothing_else()
	{
		Assert.Equal("\"Price\"", Literal("Price"));
		Assert.Equal("\"say \\\"no\\\"\"", Literal("say \"no\""));
		Assert.Equal("\"C:\\\\venue\"", Literal(@"C:\venue"));
		Assert.Equal("\"one\\r\\ntwo\"", Literal("one\r\ntwo"));
		Assert.Equal("\"a\\u0000b\"", Literal("a\0b"));
		Assert.Equal("\"a\\u2028b\"", Literal("a\u2028b"));
	}

	/// <summary>
	/// The case the whole file exists for: a value carrying what looks like code.
	/// </summary>
	/// <remarks>
	/// `// and /*` inside a dictionary value are four characters of somebody's text. A blanker
	/// that searched for `//` instead of scanning would eat the rest of the line from inside the
	/// literal, and the value would arrive at the compiler truncated — silently, because what is
	/// left is still a valid literal.
	/// </remarks>
	[Fact]
	public void A_value_that_looks_like_a_comment_survives_the_blanker()
	{
		var value = "rate // per /* annum */ quoted";
		var text  = new StringBuilder("var note = ");

		FixRuleText.Literal(text, value);

		text.Append(";   // field 223 (CouponRate)");

		var blanked = FixRuleText.Blank(text.ToString());

		Assert.Contains(Literal(value), blanked, StringComparison.Ordinal);
		Assert.DoesNotContain("field 223", blanked, StringComparison.Ordinal);
	}

	[Fact]
	public void A_name_becomes_one_line_of_comment()
	{
		Assert.Equal("NewOrderSingle", FixRuleText.Note("NewOrderSingle"));
		Assert.Equal("two lines", FixRuleText.Note("two\r\nlines"));
		Assert.Equal("a b", FixRuleText.Note("a \t  b"));
		Assert.Equal("padded", FixRuleText.Note("   padded   "));

		// Neither marker leaves the note in either direction, so a comment this package writes
		// cannot be ended, and a block comment it never writes cannot be started.
		Assert.DoesNotContain("*/", FixRuleText.Note("ends */ here"), StringComparison.Ordinal);
		Assert.DoesNotContain("/*", FixRuleText.Note("starts /* here"), StringComparison.Ordinal);

		var long_ = FixRuleText.Note(new string('x', 500));

		Assert.EndsWith("...", long_, StringComparison.Ordinal);
		Assert.True(long_.Length < 200, $"a note ran to {long_.Length} characters");
	}

	/// <summary>
	/// The property the comments are allowed to exist for: the compiler reads a copy with the
	/// same characters in the same places.
	/// </summary>
	/// <remarks>
	/// Asserted character by character rather than by an example, because "line 40, column 5 of
	/// one is line 40, column 5 of the other" is exactly what the expression language's
	/// diagnostics will be trusted to mean.
	/// </remarks>
	[Fact]
	public void The_blanked_copy_holds_every_position()
	{
		var text = string.Join("\r\n",
			"// message D (NewOrderSingle)",
			"(DotGram.Finance.Fix.FixMessage m) =>",
			"{",
			"    var symbol = \"ABC // not a comment\";   // field 55 (Symbol)",
			"    return symbol;",
			"}");

		var blanked = FixRuleText.Blank(text);

		Assert.Equal(text.Length, blanked.Length);

		for (var at = 0; at < text.Length; at++)
			Assert.True(
				blanked[at] == text[at] || blanked[at] == ' ',
				$"position {at}: '{text[at]}' became '{blanked[at]}'");

		// Line breaks stay where they were, which is what holds every following line at its
		// number, and the comments are gone from the copy the compiler reads.
		Assert.Equal(CountLines(text), CountLines(blanked));
		Assert.DoesNotContain("//", blanked.Replace("\"ABC // not a comment\"", ""), StringComparison.Ordinal);
		Assert.Contains("\"ABC // not a comment\"", blanked, StringComparison.Ordinal);
	}

	[Fact]
	public void A_text_with_no_comments_is_the_same_text()
	{
		var text = "(int x) => x * 2";

		Assert.Same(text, FixRuleText.Blank(text));
	}

	[Fact]
	public void An_unterminated_literal_is_left_to_the_compiler()
	{
		// Not an exception here: a malformed text is reported by the thing that compiles it,
		// with a position, and a helper throwing first would replace that with a worse message.
		var blanked = FixRuleText.Blank("var a = \"unterminated // still inside");

		Assert.DoesNotContain("  ", blanked, StringComparison.Ordinal);
	}

	static int CountLines(string text)
	{
		var lines = 1;

		foreach (var character in text)
			if (character == '\n')
				lines++;

		return lines;
	}
}
