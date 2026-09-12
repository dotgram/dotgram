using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// What a member is, asked without a text to parse.
/// </summary>
/// <remarks>
/// <para>
/// Every question here used to need a lambda written for it: to find out whether `Max` over
/// two <c>int</c>s picks the right overload, a text had to be composed, read, built and
/// often compiled and called. What that tested was the parser as much as the answer, and
/// what it could not test at all was a question no syntax reaches — a `params` tail against
/// an array written whole, an argument list nothing applies to, the literal <c>null</c>
/// standing where two overloads take it differently.
/// </para>
/// <para>
/// The resolver needs two things from a reading and no more: the assembly the text is read
/// for, and the namespaces its `using`s named. Both are handed over here directly, which is
/// the whole of what makes these tests possible.
/// </para>
/// </remarks>
public sealed class MemberResolverTests
{
	static readonly Assembly Here = typeof(MemberResolverTests).Assembly;

	static ExpressionParser.MemberResolver Asking(params string[] imports) => new(Here, imports);

	static ConstantExpression Value(object value) => Expression.Constant(value);

	// ── A method by the types it is handed ──────────────────────────────────────

	[Fact]
	public void A_method_is_chosen_by_the_types_it_is_handed()
	{
		var chosen = Asking().Static(typeof(Math), "Max", [Value(1), Value(2)]);

		Assert.Equal(
			[typeof(int), typeof(int)],
			((MethodInfo)chosen.Member).GetParameters().Select(one => one.ParameterType));
	}

	[Fact]
	public void And_the_arguments_come_back_as_that_method_takes_them()
	{
		// `Max(int, double)` is the `(double, double)` overload, and the `int` is converted
		// where it stands rather than left for the caller to notice.
		var chosen = Asking().Static(typeof(Math), "Max", [Value(1), Value(2.5)]);

		Assert.Equal(
			[typeof(double), typeof(double)],
			chosen.Arguments.Select(one => one.Type));
	}

	[Fact]
	public void And_a_params_tail_is_gathered_into_the_array_it_is()
	{
		var chosen = Asking().Static(typeof(Choices), "Sum", [Value(1), Value(2), Value(3)]);

		var only = Assert.Single(chosen.Arguments);
		var made = Assert.IsAssignableFrom<NewArrayExpression>(only);

		Assert.Equal(typeof(int[]), made.Type);
		Assert.Equal(3, made.Expressions.Count);
	}

	[Fact]
	public void And_a_parameter_left_out_becomes_the_default_it_was_given()
	{
		var chosen = Asking().Static(typeof(Choices), "Some", [Value(1)]);

		Assert.Equal(2, chosen.Arguments.Length);
		Assert.Equal(5, Assert.IsAssignableFrom<ConstantExpression>(chosen.Arguments[1]).Value);
	}

	[Fact]
	public void And_the_literal_null_goes_to_what_can_hold_it()
	{
		// One of the two overloads takes something that can be null and the other does not,
		// which is a question about the value and not only about its type.
		var chosen = Asking().Static(typeof(Choices), "Took", [ExpressionParser.Null]);

		Assert.Equal(typeof(string), ((MethodInfo)chosen.Member).GetParameters()[0].ParameterType);
	}

	// ── And what it refuses ─────────────────────────────────────────────────────

	[Fact]
	public void But_two_that_neither_is_better_than_are_refused() =>
		// Each converts one argument exactly and widens the other, so neither is the better
		// function member and C# refuses to choose.
		Assert.Contains(
			"ambiguous",
			Assert.Throws<InvalidOperationException>(
				() => Asking().Static(typeof(Choices), "Two", [Value(1), Value(2)])).Message);

	[Fact]
	public void And_a_method_that_is_not_there_says_so_with_what_was_asked() =>
		Assert.Contains(
			"has no method 'Nope'",
			Assert.Throws<InvalidOperationException>(
				() => Asking().Static(typeof(Math), "Nope", [])).Message);

	// ── Extensions, which are what the `using`s are for ─────────────────────────

	[Fact]
	public void An_extension_is_found_through_the_imports_it_was_given()
	{
		var chosen = Asking("DotGram.Tests.ExpressionLanguage").Method(Value(1), "Doubled", []);

		// An extension comes back as the static method it is, the receiver standing first
		// among the arguments — which is what an extension method is.
		Assert.True(((MethodInfo)chosen.Member).IsStatic);
		Assert.Equal(typeof(int), Assert.Single(chosen.Arguments).Type);
	}

	[Fact]
	public void But_a_method_the_type_declares_is_the_one_chosen()
	{
		var chosen = Asking("DotGram.Tests.ExpressionLanguage")
			.Method(Expression.New(typeof(Held)), "Twice", []);

		Assert.False(((MethodInfo)chosen.Member).IsStatic);
		Assert.Empty(chosen.Arguments);
	}

	[Fact]
	public void And_without_the_import_there_is_no_such_method() =>
		Assert.Contains(
			"has no method 'Doubled'",
			Assert.Throws<InvalidOperationException>(
				() => Asking().Method(Value(1), "Doubled", [])).Message);

	[Fact]
	public void And_a_constraint_the_inference_breaks_leaves_no_candidate() =>
		// `Sized<T>` wants a value type and the receiver is a string: what
		// `MakeGenericMethod` will not make is a method that is not there.
		Assert.Contains(
			"has no method 'Sized'",
			Assert.Throws<InvalidOperationException>(
				() => Asking("DotGram.Tests.ExpressionLanguage").Method(Value("a"), "Sized", [])).Message);

	// ── Which of two is better, with the C# compiler as the oracle ──────────────
	//
	// Each of these calls a pair of overloads in ordinary C#, where the compiler chooses and
	// the chosen one says so, and asks the resolver the same question. What is asserted is
	// that the two agree — so a test cannot hold a misreading of §12.6.4 in place, because
	// the compiler beside it did not read anything.

	/// <summary>The first parameter's type of the overload the resolver chose.</summary>
	static string Chose(string name, params Expression[] arguments) =>
		((MethodInfo)Asking().Static(typeof(Choosing), name, arguments).Member)
			.GetParameters()[0].ParameterType.Name;

	[Fact]
	public void An_exact_match_beats_a_widening() =>
		Assert.Equal(Choosing.Near(1), Chose("Near", Value(1)));

	[Fact]
	public void And_where_neither_is_exact_the_nearer_target_wins() =>
		// `long` converts to `double` and not back, so it is the better target of the two.
		Assert.Equal(Choosing.Wider((short)1), Chose("Wider", Value((short)1)));

	[Fact]
	public void And_where_neither_target_converts_to_the_other_the_signed_one_wins() =>
		// A `byte` reaches both `int` and `uint`, and neither of those reaches the other.
		Assert.Equal(Choosing.Signed((byte)1), Chose("Signed", Value((byte)1)));

	[Fact]
	public void And_the_one_leaving_nothing_to_a_default_wins() =>
		Assert.Equal(
			Choosing.Filled(1),
			((MethodInfo)Asking().Static(typeof(Choosing), "Filled", [Value(1)]).Member)
				.GetParameters().Length.ToString());

	[Fact]
	public void And_a_normal_form_beats_an_expanded_one()
	{
		var chosen = Asking().Static(typeof(Choosing), "Spread", [Value(1), Value(2)]);

		Assert.Equal(
			Choosing.Spread(1, 2),
			chosen.Arguments[1].Type.IsArray ? "expanded" : "normal");
	}

	[Fact]
	public void And_a_conversion_of_the_author_s_own_is_weighed_with_the_rest() =>
		// `int` reaches `Money` by the conversion `Money` declares and `decimal` by the one
		// the language has. Which of the two targets is better is the question, and the
		// compiler beside this test has already answered it.
		Assert.Equal(Choosing.Given(1), Chose("Given", Value(1)));

	// ── A generic method, an indexer, a constructor, a delegate ─────────────────

	[Fact]
	public void A_generic_method_gets_its_type_arguments_from_what_it_is_handed()
	{
		var list = Expression.Parameter(typeof(List<int>), "l");
		var test = Expression.Lambda<Func<int, bool>>(
			Expression.Constant(true), Expression.Parameter(typeof(int), "n"));

		var chosen = Asking("System.Linq").Method(list, "Where", [test]);
		var method = (MethodInfo)chosen.Member;

		Assert.True(method.IsGenericMethod);
		Assert.Equal([typeof(int)], method.GetGenericArguments());
	}

	[Fact]
	public void An_indexer_is_chosen_the_way_a_call_is()
	{
		var chosen = Asking().Indexer(Expression.Parameter(typeof(string), "s"), [Value(0)]);

		var indexer = Assert.IsAssignableFrom<PropertyInfo>(chosen.Member);

		Assert.Equal(typeof(char), indexer.PropertyType);
	}

	[Fact]
	public void A_constructor_too()
	{
		var chosen = Asking().Constructor(typeof(StringBuilder), [Value(16)]);

		Assert.Equal(
			[typeof(int)],
			((ConstructorInfo)chosen.Member).GetParameters().Select(one => one.ParameterType));
	}

	[Fact]
	public void And_a_delegate_is_asked_what_its_invoke_takes()
	{
		var held  = Expression.Parameter(typeof(Func<int, int>), "f");
		var chosen = Asking().Delegated(held, [Value(1)]);

		Assert.Equal("Invoke", chosen.Member.Name);
		Assert.Equal(typeof(int), Assert.Single(chosen.Arguments).Type);
	}
}
