using System;
using System.Linq;

using DotGram.Generation;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The names an embedded C# expression asks the parser for.
/// </summary>
/// <remarks>
/// <para>
/// This used to be a substring search, and it was wrong in both directions:
/// <c>@(Log("parserInput"))</c> claimed the whole input — which then refused the grammar its
/// flat rendering, so spelling decided a compilation strategy — and <c>@(other.context)</c>
/// claimed the context, because a dot is not an identifier character.
/// </para>
/// <para>
/// Syntax alone answers all of it, and the cases below are the ones that were wrong or that
/// a reader would want to check before believing they are not.
/// </para>
/// </remarks>
public sealed class FreeNameTests
{
	static string[] Free(string expression) =>
		[.. RoslynCSharpScanner.Instance.FreeNames(expression)!.OrderBy(name => name, StringComparer.Ordinal)];

	[Fact]
	public void A_name_standing_on_its_own_is_free() =>
		Assert.Equal(["left", "right"], Free("left + right"));

	/// <summary>Text inside a literal is text, which is what the search got wrong.</summary>
	[Theory]
	[InlineData("Log(\"parserInput\")")]
	[InlineData("\"context\" + \"parserSpan\"")]
	[InlineData("$\"{\"parserInput\"}\"")]
	[InlineData("'x' + \"parserState\"")]
	public void And_a_name_inside_a_literal_is_not_one(string expression) =>
		Assert.DoesNotContain(
			Free(expression),
			name => name.StartsWith("parser", StringComparison.Ordinal) || name == "context");

	/// <summary>And a member is the other direction the boundary test got wrong.</summary>
	[Fact]
	public void And_a_name_after_a_dot_is_a_member() =>
		// `other` is asked for; `context` is something `other` has.
		Assert.Equal(["other"], Free("other.context"));

	[Fact]
	public void And_the_same_through_a_null_conditional() =>
		Assert.Equal(["other"], Free("other?.context"));

	[Fact]
	public void And_a_name_a_lambda_introduced_belongs_to_it() =>
		// `x` is the lambda's; `items` is asked for.
		Assert.Equal(["items"], Free("items.Select(x => x + 1)"));

	[Fact]
	public void And_a_member_written_in_an_initializer_is_a_member() =>
		Assert.Equal(["Thing", "value"], Free("new Thing { Source = value }"));

	[Fact]
	public void And_an_argument_named_at_a_call_site_is_a_parameter() =>
		Assert.Equal(["Make", "value"], Free("Make(source: value)"));

	/// <summary>
	/// A name before a dot is kept, because telling a namespace from a variable needs a
	/// compilation.
	/// </summary>
	/// <remarks>
	/// `System.Math.Abs(n)` asks for `System`, which nothing will ever supply — and that
	/// costs a name nobody hands over rather than a name wrongly handed over. The question
	/// this answers is only ever "does the expression ask for *this* name", against a short
	/// list the parser can supply, so a surplus `System` is never consulted.
	/// </remarks>
	[Fact]
	public void And_a_qualifier_is_kept_because_only_a_compilation_could_tell() =>
		Assert.Equal(["System", "n"], Free("System.Math.Abs(n)"));

	[Fact]
	public void And_an_expression_that_will_not_parse_answers_nothing_rather_than_empty() =>
		// Null, not an empty set: a caller has to be able to fall back rather than conclude
		// the expression asks for nothing.
		Assert.Null(RoslynCSharpScanner.Instance.FreeNames("left +"));

	[Fact]
	public void And_the_names_a_parser_can_supply_are_found_where_they_are_meant() =>
		Assert.Equal(
			["context", "parserSpan", "t"],
			Free("context.Say(t, parserSpan)"));
}
