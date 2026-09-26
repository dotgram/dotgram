using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// What a reading that was given up declared is not there afterwards.
/// </summary>
/// <remarks>
/// <para>
/// A reading writes into the state as it goes — a parameter, a local, the variable of a
/// `foreach` — and a reading the parse abandons takes all of it back (§7.7). The declarations
/// are a list, and taking them back is truncating it.
/// </para>
/// <para>
/// There is an index of declarations by name beside that list, holding PLACES in it, so that a
/// use of a name does not walk every declaration in the text. Truncating the list does not
/// touch the index, deliberately: rebuilding it on every rollback was measured and cost two to
/// three times on every text, a reading being given up constantly. So a place that is no longer
/// there is passed over where it is read, and this is the check that it is.
/// </para>
/// <para>
/// Written against the state directly rather than through a text. The thing under test is what
/// a rollback leaves, and a text that provokes one is a text about something else as well — it
/// would pass for reasons of its own and go on passing if this rule were dropped.
/// </para>
/// </remarks>
public sealed class RollbackTests
{
	static ExpressionParser.State Reading()
	{
		return new ExpressionParser.State(typeof(RollbackTests).Assembly) { Text = "(int q) => q" };
	}

	[Fact]
	public void A_declaration_a_reading_gave_up_is_not_found_afterwards()
	{
		var state = Reading();
		var wrote = new ExpressionParser.SourceSpan(1, 1);
		var uses  = new ExpressionParser.SourceSpan(11, 1);

		var before = state.Mark();

		Assert.True(state.Takes(typeof(int), "q", wrote));
		Assert.True(state.Knows("q", uses), "a declaration is found while its reading stands.");

		state.Rollback(before);

		Assert.False(state.Knows("q", uses), "a declaration a reading gave up was found afterwards.");
	}

	[Fact]
	public void A_place_a_later_declaration_took_answers_for_that_one_and_not_the_first()
	{
		var state = Reading();
		var wrote = new ExpressionParser.SourceSpan(1, 1);
		var uses  = new ExpressionParser.SourceSpan(11, 1);

		var before = state.Mark();

		Assert.True(state.Takes(typeof(int), "q", wrote));

		state.Rollback(before);

		// The place the rolled-back `q` held is now taken by a declaration of another name.
		// Nothing may answer for `q` there, and `r` must answer for itself.
		Assert.True(state.Takes(typeof(string), "r", wrote));

		Assert.False(state.Knows("q", uses), "a place taken by another name still answered for the first.");
		Assert.True(state.Knows("r", uses), "the declaration that took the place did not answer.");
	}

	[Fact]
	public void A_place_a_declaration_of_the_same_name_took_answers_for_it()
	{
		var state = Reading();
		var wrote = new ExpressionParser.SourceSpan(1, 1);
		var uses  = new ExpressionParser.SourceSpan(11, 1);

		var before = state.Mark();

		Assert.True(state.Takes(typeof(int), "q", wrote));

		state.Rollback(before);

		// Named twice in the index and declared once: the answer is the declaration that is
		// there, and naming it twice changes nothing, since what is chosen among them is the
		// innermost and latest.
		Assert.True(state.Takes(typeof(string), "q", wrote));

		Assert.True(state.Knows("q", uses), "a declaration written after a rollback was not found.");
	}
}
