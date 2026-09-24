using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// What the place an expression stands in says about its type, which C# calls target typing
/// and this language did not do before 2026-09-23.
/// </summary>
/// <remarks>
/// <para>
/// Three things were one decision. A switch expression whose arms meet in no type of their own
/// is typed by what is wanted of it. A lambda that leaves its parameter types out takes them
/// from the delegate it is being compiled to. And a conversion into a ref struct is built rather
/// than folded, because folding means asking reflection for the value and a ref struct is the
/// one thing reflection cannot hand back.
/// </para>
/// <para>
/// Where nothing says what is wanted, nothing changes: <c>Parse</c> is handed no delegate, so a
/// switch with no natural type is refused there in the words it was always refused in, and a
/// lambda that says no types is refused there too — as C# refuses one with nothing to convert
/// it to.
/// </para>
/// </remarks>
public sealed class TargetTypedTests
{
	public abstract class Held
	{
		public abstract override string ToString();
	}

	public sealed class Held<T> : Held
	{
		public Held(T value)
		{
			Value = value;
		}

		public T Value { get; }

		public override string ToString()
		{
			return typeof(T).Name + "=" + Value;
		}
	}

	public static Held Pick(Held one)
	{
		return one;
	}

	delegate Held Making(int tag);
	delegate long Widening(int tag);
	delegate object Reading(int tag, string value);
	delegate int Counting(string value);
	delegate int Spanning(ReadOnlySpan<char> value);

	const string Using = "using System; using DotGram.Tests.ExpressionLanguage; " +
		"using DotGram.Tests.ExpressionLanguage.TargetTypedTests; ";

	static string Text(string body)
	{
		return "using System; using DotGram.Tests.ExpressionLanguage; " + body;
	}

	// ── A switch typed by its target ────────────────────────────────────────────

	const string Arms = "tag switch { 1 => new TargetTypedTests.Held<long>(1L), " +
		"2 => new TargetTypedTests.Held<decimal>(2m), _ => null }";

	[Theory]
	// The four places a target comes from: the lambda's body, a `return`, an argument, and a
	// variable of a declared type. Two constructed generics of one open type convert to each
	// other in neither direction, so the arms meet in nothing and only the target can type them.
	[InlineData("(int tag) => " + Arms)]
	[InlineData("(int tag) => { return " + Arms + "; }")]
	[InlineData("(int tag) => TargetTypedTests.Pick(" + Arms + ")")]
	[InlineData("(int tag) => { TargetTypedTests.Held h = " + Arms + "; return h; }")]
	public void A_switch_with_no_natural_type_is_typed_by_the_place_it_stands_in(string body)
	{
		var made = ExpressionParser.Compile<Making>(Text(body), typeof(TargetTypedTests).Assembly);

		Assert.Equal("Int64=1", made(1)!.ToString());
	}

	[Fact]
	public void Arms_that_meet_in_a_type_of_their_own_are_still_typed_by_it()
	{
		var made = ExpressionParser.Parse(
			Text("(int tag) => tag switch { 1 => new TargetTypedTests.Held<long>(1L), _ => null }"),
			typeof(TargetTypedTests).Assembly);

		Assert.Equal(typeof(Held<long>), made.ReturnType);
	}

	[Fact]
	public void With_no_target_at_all_such_a_switch_is_refused_as_before()
	{
		var thrown = Assert.Throws<InvalidOperationException>(
			() => ExpressionParser.Parse(Text("(int tag) => " + Arms), typeof(TargetTypedTests).Assembly));

		Assert.Contains("Type of switch expression cannot be determined", thrown.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void An_arm_that_converts_to_the_target_in_no_way_is_refused_naming_it()
	{
		var thrown = Assert.Throws<InvalidOperationException>(
			() => ExpressionParser.Compile<Making>(
				Text("(int tag) => tag switch { 1 => new TargetTypedTests.Held<long>(1L), 2 => \"text\", _ => null }"),
				typeof(TargetTypedTests).Assembly));

		Assert.Contains("does not convert to", thrown.Message, StringComparison.Ordinal);
	}

	// ── A lambda typed by the delegate it is compiled to ────────────────────────

	[Fact]
	public void Parameters_left_out_are_taken_from_the_delegate()
	{
		var made = ExpressionParser.Compile<Reading>(
			Text("(tag, value) => value.Length"), typeof(TargetTypedTests).Assembly);

		Assert.Equal(4, made(25010, "1.25"));
	}

	[Fact]
	public void One_parameter_needs_no_brackets_either()
	{
		var made = ExpressionParser.Compile<Counting>(
			Text("value => value.Length"), typeof(TargetTypedTests).Assembly);

		Assert.Equal(4, made("1.25"));
	}

	[Fact]
	public void One_text_compiles_to_two_delegates_of_different_shapes()
	{
		const string text = "using System; value => value.Length";

		Assert.Equal(4, ExpressionParser.Compile<Counting>(text, typeof(TargetTypedTests).Assembly)("1.25"));
		Assert.Equal(4, ExpressionParser.Compile<Spanning>(text, typeof(TargetTypedTests).Assembly)("1.25".AsSpan()));
	}

	/// <summary>The form the package's README shows, held so that the page cannot go stale.</summary>
	[Fact]
	public void The_form_the_readme_shows_reads()
	{
		var made = ExpressionParser.Compile<Func<int, int>>("x => x * x", typeof(TargetTypedTests).Assembly);

		Assert.Equal(9, made(3));
	}

	[Fact]
	public void Parameters_written_with_types_are_read_as_they_always_were()
	{
		var made = ExpressionParser.Compile<Reading>(
			Text("(int tag, string value) => value.Length"), typeof(TargetTypedTests).Assembly);

		Assert.Equal(4, made(25010, "1.25"));
	}

	/// <summary>Where no delegate is named, the way that reads such a lambda is not there at all.</summary>
	/// <remarks>
	/// It refuses as a parse refuses, and not with a message of its own, deliberately: a guard
	/// standing before anything is read leaves every other text refused exactly where and in the
	/// words it was refused before this way existed. The whole record of refusals, and the
	/// hand-written parser held beside this one, are what that is worth.
	/// </remarks>
	[Fact]
	public void A_lambda_that_says_no_types_is_refused_where_no_delegate_says_them()
	{
		Assert.Throws<FormatException>(
			() => ExpressionParser.Parse(Text("(tag, value) => tag"), typeof(TargetTypedTests).Assembly));

		Assert.False(ExpressionParser.TryParse(Text("value => value.Length")).IsSuccess);
	}

	[Fact]
	public void As_many_parameters_as_the_delegate_takes_and_no_other_number()
	{
		var thrown = Assert.Throws<InvalidOperationException>(
			() => ExpressionParser.Compile<Reading>(Text("tag => tag"), typeof(TargetTypedTests).Assembly));

		Assert.Contains("takes", thrown.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void The_body_is_still_converted_to_what_the_delegate_gives_back()
	{
		var made = ExpressionParser.Compile<Widening>(Text("(int tag) => tag"), typeof(TargetTypedTests).Assembly);

		Assert.Equal(1L, made(1));
	}

	// ── A conversion into a ref struct is built, never folded ───────────────────

	delegate ReadOnlySpan<char> Giving();

	[Fact]
	public void A_constant_converted_into_a_ref_struct_keeps_its_call()
	{
		var made = ExpressionParser.Compile<Giving>("() => \"abc\"", typeof(TargetTypedTests).Assembly);

		Assert.Equal(3, made().Length);
	}

	[Fact]
	public void The_same_text_still_compiles_to_the_string_it_is()
	{
		var made = ExpressionParser.Compile<Func<string>>("() => \"abc\"", typeof(TargetTypedTests).Assembly);

		Assert.Equal("abc", made());
	}
}
