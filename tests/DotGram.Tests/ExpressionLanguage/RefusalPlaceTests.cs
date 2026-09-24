using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Where a refusal says it is, for the refusals that come from a factory rather than from the
/// parse.
/// </summary>
/// <remarks>
/// <para>
/// A parse refusal has a position because the parse knows where it stopped. A SEMANTIC one had
/// none: the factory is handed a tree and the names on it, and none of those knows what text it
/// was read from, so every one of them came back as "somewhere in this text". Over the corpus cut
/// every way it can be cut, that was 635 refusals with no place at all.
/// </para>
/// <para>
/// The construction says it now, by recording before it builds: <c>context.Here(parserSpan)</c>
/// stands where <c>context</c> already stood in the argument list, and C# evaluates arguments
/// left to right. That covers 588 of the 635. What is left is the constructions that are handed
/// no state {D} the binary operators, the literals, and the one that closes the whole lambda {D}
/// and they would each cost a signature to reach, which is a trade rather than an oversight.
/// </para>
/// </remarks>
public sealed class RefusalPlaceTests
{
	delegate object Reading(int tag, string value);

	static string Text(string body)
	{
		return "using System; using DotGram.Tests.ExpressionLanguage; " + body;
	}

	[Theory]
	// The member that is not there, the call whose overload is not there, the static one, and
	// an index: each says where the thing it refused was written, not where the text began.
	[InlineData("(int tag, string value) => value.Leng", "value.Leng")]
	[InlineData("(int tag, string value) => value.Trim(1, 2, 3)", "value.Trim(1, 2, 3)")]
	[InlineData("(int tag, string value) => Math.Sqrtt(1.0)", "Math.Sqrtt(1.0)")]
	// A generic type whose arity nothing has. A type nothing declares at all is refused by
	// `NamedType`'s guard and so by the PARSE, which places it itself; this one passes the
	// guard, because a name written with type arguments is not looked up until it is built.
	[InlineData("(int tag, string value) => new System.Collections.Generic.List<int, int>()",
		"System.Collections.Generic.List<int, int>")]
	// An interpolated string whose hole does not read.
	[InlineData("(int tag, string value) => $\"a{value.Leng}b\"", "$\"a{value.Leng}b\"")]
	public void A_refusal_from_a_factory_says_where_it_is(string body, string part)
	{
		var text  = Text(body);
		var match = ExpressionParser.TryParse(text, typeof(RefusalPlaceTests).Assembly);

		Assert.False(match.IsSuccess, text);

		// Where the offending part stands in the whole text, which is what the refusal must
		// point at. Held as a span of the text rather than as a number, so that editing the
		// prefix above cannot quietly make the assertion about something else.
		var at = text.IndexOf(part, StringComparison.Ordinal);

		Assert.True(at > 0, "the test's own part is not in its own text: " + part);

		// Inside the part, rather than exactly at its start: a construction nested in it may
		// record a place of its own, and a refusal that points at the hole inside an
		// interpolated string says more than one pointing at the string. What is asserted is
		// that it points at the thing it refused and not at the start of the text.
		Assert.InRange(match.Position, at, at + part.Length);
	}

	[Fact]
	public void A_refusal_from_the_parse_still_says_where_the_parse_stopped()
	{
		var match = ExpressionParser.TryParse(Text("(int tag, string value) => value +"), typeof(RefusalPlaceTests).Assembly);

		Assert.False(match.IsSuccess);
		Assert.Equal(Text("(int tag, string value) => value +").Length, match.Position);
	}

	/// <summary>A refusal at the very start is placed there, and that is a fact and not a gap.</summary>
	[Fact]
	public void Offset_zero_is_an_answer_where_the_text_fails_at_its_first_character()
	{
		var match = ExpressionParser.TryParse("int x) => x", typeof(RefusalPlaceTests).Assembly);

		Assert.False(match.IsSuccess);
		Assert.Equal(0, match.Position);
	}
}
