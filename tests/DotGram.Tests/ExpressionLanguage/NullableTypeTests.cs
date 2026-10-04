using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// `T?` in a type position — a cast, a declaration, a generic argument, `typeof`, `default`,
/// `is` and `as` — read as C# reads it: `System.Nullable&lt;T&gt;` for a value type, and the
/// reference type itself where `?` is only ever an erased annotation there.
/// </summary>
/// <remarks>
/// `is` and `as` are the one place `?` is also C#'s conditional operator, so a bare `?` there is
/// read as nullable only where what follows it could not be the start of a new expression —
/// `x is int ? 1 : 2` is `(x is int) ? 1 : 2`, not `x is (int?)` with nothing after it, exactly
/// as C# reads it.
/// </remarks>
public sealed class NullableTypeTests
{
	const string Using = "using System; using System.Collections.Generic; ";

	// ── Where a bare `?` has nothing else it could mean ─────────────────────────

	[Fact]
	public void A_parameter_may_be_declared_nullable()
	{
		var made = Both.Compile<Func<int?, int?>>("(int? n) => n");

		Assert.Null(made(null));
		Assert.Equal(5, made(5));
	}

	[Fact]
	public void A_local_may_be_declared_nullable()
	{
		Assert.Null(Both.Compile<Func<int?>>("() => { int? n = null; return n; }")());
	}

	[Fact]
	public void A_cast_may_target_a_nullable_type()
	{
		var made = Both.Compile<Func<object, int?>>(Using + "(object x) => (int?)x");

		Assert.Equal(5, made(5));
	}

	[Fact]
	public void A_generic_argument_may_be_nullable()
	{
		var made = Both.Compile<Func<List<int?>>>(Using + "() => new List<int?> { 1, null }");

		Assert.Equal([1, null], made());
	}

	[Fact]
	public void Typeof_may_name_a_nullable_type()
	{
		Assert.Equal(typeof(int?), Both.Compile<Func<Type>>("() => typeof(int?)")());
	}

	[Fact]
	public void Default_may_name_a_nullable_type()
	{
		Assert.Null(Both.Compile<Func<int?>>("() => default(int?)")());
	}

	[Fact]
	public void An_array_of_a_nullable_type_reads_too()
	{
		Assert.Equal(typeof(int?[]), Both.Parse("(int?[] a) => a").Parameters[0].Type);
	}

	[Fact]
	public void A_nullable_annotated_array_reads_as_the_same_array()
	{
		// `string[]?` is a nullable-REFERENCE annotation on the ARRAY itself, not on its
		// element: the array is a reference type whichever way, so this is `string[]`.
		Assert.Equal(typeof(string[]), Both.Parse(Using + "(string[]? a) => a").Parameters[0].Type);
	}

	[Fact]
	public void A_mark_at_either_level_may_be_stacked_in_any_order()
	{
		// `string?[]?[]`: an array of (a nullable-annotated array of nullable `string`). Every
		// `?` is a reference annotation here except the one right after `string`, so the
		// runtime type is `string[][]` regardless of how many of them are written.
		Assert.Equal(typeof(string[][]), Both.Parse(Using + "(string?[]?[] a) => a").Parameters[0].Type);
	}

	[Fact]
	public void A_reference_type_is_the_same_type_with_or_without_the_mark()
	{
		// `?` on a reference type is a nullable-REFERENCE annotation in C#, erased by the time
		// anything runs: `string?` and `string` are one `Type`. Nothing here tracks the
		// annotation, so there is nothing for it to mean beyond the type already written — in
		// every position C# lets it stand at all; `Typeof_of_a_nullable_reference_type_is_refused`
		// and its neighbours below are the ones that do not.
		Assert.Equal("a", Both.Compile<Func<object, string>>(Using + "(object x) => (string?)x")("a"));
		Assert.Equal(typeof(string), Both.Parse(Using + "(string? x) => x").Parameters[0].Type);
	}

	[Fact]
	public void A_value_type_that_is_already_nullable_is_refused_a_second_mark()
	{
		var result = Both.TryParse(Using + "(Nullable<int> n) => default(Nullable<int>?)");

		Assert.False(result.IsSuccess);
		Assert.Contains("is already nullable", result.Error, StringComparison.Ordinal);
	}

	// ── Where C# refuses the erasure instead of making it (CS8639, CS8628, CS8650, CS8651) ──

	[Fact]
	public void Typeof_of_a_nullable_reference_type_is_refused()
	{
		var result = Both.TryParse(Using + "() => typeof(string?)");

		Assert.False(result.IsSuccess);
		Assert.Contains("not legal to use nullable reference type", result.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void A_constructor_call_on_a_nullable_reference_type_is_refused()
	{
		var result = Both.TryParse("() => new object?()");

		Assert.False(result.IsSuccess);
		Assert.Contains("not legal to use nullable reference type", result.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void An_object_initializer_with_no_parentheses_on_a_nullable_reference_type_is_refused()
	{
		var result = Both.TryParse(Using + "() => new List<int>? { 1 }");

		Assert.False(result.IsSuccess);
		Assert.Contains("not legal to use nullable reference type", result.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void Default_and_a_cast_still_erase_the_mark_rather_than_refuse_it()
	{
		// `typeof`, `new` and `is`/`as` refuse it; a declaration, `default` and a cast do not —
		// the same split C# makes (CS8639/CS8628/CS8650/CS8651 against none of these).
		Assert.Null(Both.Compile<Func<string?>>(Using + "() => default(string?)")());
		Assert.Equal("a", Both.Compile<Func<object, string?>>(Using + "(object x) => (string?)x")("a"));
	}

	// ── `is` and `as`, where C#'s `?:` also reads a `?` ─────────────────────────

	[Theory]
	// The issue's own example: `?` belongs to the conditional, and `is` reads a plain `int`.
	[InlineData("(object x, int a, int b) => x is int ? a : b", 5, 1, 2, 1)]
	[InlineData("(object x, int a, int b) => x is string ? a : b", 5, 1, 2, 2)]
	public void Is_followed_by_a_value_reads_the_ternary_and_not_a_nullable_type(
		string text, object argument, int a, int b, int expected)
	{
		var made = Both.Compile<Func<object, int, int, int>>(text);

		Assert.Equal(expected, made(argument, a, b));
	}

	[Fact]
	public void As_followed_by_a_value_reads_the_ternary_and_not_a_nullable_type()
	{
		// If the `?` were read as nullable, `x as int?` would be a complete `as`-expression on
		// its own and the ` a : b` left dangling after it would be a syntax error. Reaching the
		// `as` operator's own refusal of a non-nullable value type instead (BCL, "as" needs a
		// reference or nullable type) is what shows the `?` went to the ternary, as C# reads
		// `a as T ? b : c`.
		var result = Both.TryParse("(object x, int a, int b) => x as int ? a : b");

		Assert.False(result.IsSuccess);
		Assert.Contains("reference or nullable type", result.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void A_bare_as_nullable_at_the_end_of_the_text_reads_as_nullable()
	{
		var made = Both.Compile<Func<object, int?>>("(object x) => x as int?");

		Assert.Equal(5, made(5));
		Assert.Null(made("not an int"));
	}

	[Fact]
	public void As_nullable_closed_by_a_parenthesis_reads_as_nullable()
	{
		var made = Both.Compile<Func<object, bool>>("(object x) => (x as int?) == 5");

		Assert.True(made(5));
		Assert.False(made(6));
	}

	[Fact]
	public void As_nullable_followed_by_a_comparison_keeps_the_mark_and_the_ternary_after_it_too()
	{
		// `==` can never start a new expression, so the `?` right after `int` stays nullable —
		// and the SECOND `?`, the real ternary, is still there afterwards for `a : b` to read.
		var made = Both.Compile<Func<object, int, int, int>>(
			"(object x, int a, int b) => x as int? == 5 ? a : b");

		Assert.Equal(1, made(5, 1, 2));
		Assert.Equal(2, made(6, 1, 2));
	}

	[Fact]
	public void Is_nullable_closed_by_a_parenthesis_reads_as_nullable()
	{
		Assert.True(Both.Compile<Func<object, bool>>("(object x) => (x is int?)")(5));
	}

	[Fact]
	public void As_nullable_may_be_an_array_of_the_nullable_type()
	{
		var made = Both.Compile<Func<object, int?[]>>("(object x) => x as int?[]");

		Assert.Equal([1, null], made(new int?[] { 1, null }));
		Assert.Null(made("not an array"));
	}

	[Fact]
	public void Is_nullable_may_be_an_array_of_the_nullable_type()
	{
		Assert.True(Both.Compile<Func<object, bool>>("(object x) => x is int?[]")(new int?[] { 1 }));
	}

	[Fact]
	public void As_nullable_followed_by_the_coalesce_operator_keeps_the_mark()
	{
		// A SEPARATE `?` right after the nullable one is never this one's other half (that pair
		// is lexically one token, `??`, and never reaches this far) — so it is read as the
		// coalesce operator's own, exactly as `x as int? < 5` above reads a separate `<`.
		var made = Both.Compile<Func<object, int>>("(object x) => x as int? ?? 5");

		Assert.Equal(1, made(1));
		Assert.Equal(5, made("not an int"));
	}

	[Fact]
	public void As_int_followed_immediately_by_the_coalesce_operator_is_still_refused()
	{
		// `int??5` is one token, `??`, before any of this is asked — the lexer never hands
		// `Tested` a `?` to keep or let go of, so this reads as `(x as int) ?? 5` and refuses on
		// `as` needing a reference or nullable type, as it did with none of this nearby at all.
		var result = Both.TryParse("(object x) => x as int??5");

		Assert.False(result.IsSuccess);
		Assert.Contains("reference or nullable type", result.Error, StringComparison.Ordinal);
	}
}
