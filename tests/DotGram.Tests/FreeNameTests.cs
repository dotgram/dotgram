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
	static string[] Free(string expression)
	{
		return [.. RoslynCSharpScanner.Instance.FreeNames(expression)!.OrderBy(name => name, StringComparer.Ordinal)];
	}

	[Fact]
	public void A_name_standing_on_its_own_is_free()
	{
		Assert.Equal(["left", "right"], Free("left + right"));
	}

	/// <summary>Text inside a literal is text, which is what the search got wrong.</summary>
	[Theory]
	[InlineData("Log(\"parserInput\")")]
	[InlineData("\"context\" + \"parserSpan\"")]
	[InlineData("$\"{\"parserInput\"}\"")]
	[InlineData("'x' + \"parserState\"")]
	public void And_a_name_inside_a_literal_is_not_one(string expression)
	{
		Assert.DoesNotContain(
			Free(expression),
			name => name.StartsWith("parser", StringComparison.Ordinal) || name == "context");
	}

	/// <summary>And a member is the other direction the boundary test got wrong.</summary>
	[Fact]
	public void And_a_name_after_a_dot_is_a_member()
	{
		// `other` is asked for; `context` is something `other` has.
		Assert.Equal(["other"], Free("other.context"));
	}

	[Fact]
	public void And_the_same_through_a_null_conditional()
	{
		Assert.Equal(["other"], Free("other?.context"));
	}

	[Fact]
	public void And_a_name_a_lambda_introduced_belongs_to_it()
	{
		// `x` is the lambda's; `items` is asked for.
		Assert.Equal(["items"], Free("items.Select(x => x + 1)"));
	}

	/// <summary>
	/// But only inside that lambda: the same spelling outside it is still asked for. A lambda
	/// parameter may shadow an outer name (C# 8), so the expression is valid C#.
	/// </summary>
	[Theory]
	[InlineData("items.Select(x => x + 1).Sum() + x",          new[] { "items", "x" })]
	[InlineData("x + items.Select(x => x + 1).Sum()",          new[] { "items", "x" })]
	[InlineData("items.Where(x => x > 0).Count(y => y < x)",   new[] { "items", "x" })]
	[InlineData("items.Select((x, i) => x + i).Sum() + i",     new[] { "i", "items" })]
	[InlineData("items.Select(delegate (int x) { return x; }).Sum() + x", new[] { "items", "x" })]
	[InlineData("items.Select(x => { var y = x; return y; }).Sum() + y", new[] { "items", "y" })]
	[InlineData("items.Select(x => x is int n ? n : 0).Sum() + n", new[] { "items", "n" })]
	public void And_a_name_a_lambda_introduced_is_still_free_outside_it(string expression, string[] expected)
	{
		Assert.Equal(expected, Free(expression));
	}

	/// <summary>
	/// A name a pattern or an <c>out var</c> introduced outside any lambda is the expression's
	/// own, wherever in it the name is used.
	/// </summary>
	[Theory]
	[InlineData("o is int n && n > 0",              new[] { "o" })]
	[InlineData("Try(s, out var n) ? n : 0",        new[] { "Try", "s" })]
	[InlineData("o is int n && items.Any(x => x == n)", new[] { "items", "o" })]
	public void And_a_name_a_pattern_introduced_belongs_to_the_whole_expression(string expression, string[] expected)
	{
		Assert.Equal(expected, Free(expression));
	}

	/// <summary>A range variable of a query belongs to the query, and only to it.</summary>
	[Theory]
	[InlineData("(from x in items select x).Sum()",                      new[] { "items" })]
	[InlineData("(from x in items let y = x * 2 select y).Sum() + y",    new[] { "items", "y" })]
	[InlineData("(from x in items select x).Sum() + x",                  new[] { "items", "x" })]
	[InlineData("(from x in x select x).Sum()",                          new[] { "x" })]
	public void And_a_range_variable_belongs_to_its_query(string expression, string[] expected)
	{
		Assert.Equal(expected, Free(expression));
	}

	/// <summary>
	/// A rename reaches the outer name and leaves the lambda's alone, which is what a rename
	/// that took the flat set of every introduced name could not do.
	/// </summary>
	[Theory]
	[InlineData("items.Select(x => x + 1).Sum() + x",   "items.Select(x => x + 1).Sum() + y")]
	[InlineData("x + items.Select(x => x).Sum()",       "y + items.Select(x => x).Sum()")]
	[InlineData("(from x in items select x).Sum() + x", "(from x in items select x).Sum() + y")]
	public void And_a_rename_reaches_the_outer_name_only(string expression, string expected)
	{
		Assert.Equal(
			expected,
			RoslynCSharpScanner.Instance.Renamed(expression, new System.Collections.Generic.Dictionary<string, string> { ["x"] = "y" }));
	}

	/// <summary>
	/// <c>var</c> is the keyword only where a declaration's type stands; anywhere else it is a
	/// name, and a capture may be called that.
	/// </summary>
	[Theory]
	[InlineData("var.Length == 1",                                         new[] { "var" })]
	[InlineData("var + 1",                                                 new[] { "var" })]
	[InlineData("Try(s, out var n) ? n : var",                             new[] { "Try", "s", "var" })]
	[InlineData("items.Select(x => { var y = var; return y; })",           new[] { "items", "var" })]
	[InlineData("items.Select(x => { foreach (var v in x) return v; return 0; })", new[] { "items" })]
	[InlineData("items.Select(x => { ref var y = ref x; return y; })",     new[] { "items" })]
	public void And_var_is_a_name_wherever_no_declaration_is_written_with_it(string expression, string[] expected)
	{
		Assert.Equal(expected, Free(expression));
	}

	/// <summary>
	/// The other contextual keywords are names wherever the parser reads them as names, and C#
	/// binds them to whatever is in scope: a member called <c>nameof</c> is what <c>nameof(x)</c>
	/// calls, and a variable called <c>_</c> is what <c>out _</c> writes. Where the parser reads
	/// them as keywords — <c>async</c> before a lambda, <c>when</c> in a switch arm — they are
	/// tokens and never names.
	/// </summary>
	[Theory]
	[InlineData("nameof(x)",                                   new[] { "nameof", "x" })]
	[InlineData("value + 1",                                   new[] { "value" })]
	[InlineData("Try(s, out _)",                               new[] { "Try", "_", "s" })]
	[InlineData("items.Select(_ => 1).Sum() + _",              new[] { "_", "items" })]
	[InlineData("(dynamic)x",                                  new[] { "dynamic", "x" })]
	[InlineData("when + async",                                new[] { "async", "when" })]
	[InlineData("items.Select(async x => x)",                  new[] { "items" })]
	[InlineData("o switch { int n when n > m => n, _ => 0 }",  new[] { "m", "o" })]
	public void And_other_contextual_keywords_are_names_where_they_are_read_as_names(string expression, string[] expected)
	{
		Assert.Equal(expected, Free(expression));
	}

	/// <summary><c>global::</c> and an extern alias name nothing in scope, nor does what is rooted at them.</summary>
	[Fact]
	public void And_an_alias_qualified_name_asks_for_nothing()
	{
		Assert.Equal(["n"], Free("global::System.Math.Abs(n)"));
	}

	[Fact]
	public void And_a_member_written_in_an_initializer_is_a_member()
	{
		Assert.Equal(["Thing", "value"], Free("new Thing { Source = value }"));
	}

	[Fact]
	public void And_an_argument_named_at_a_call_site_is_a_parameter()
	{
		Assert.Equal(["Make", "value"], Free("Make(source: value)"));
	}

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
	public void And_a_qualifier_is_kept_because_only_a_compilation_could_tell()
	{
		Assert.Equal(["System", "n"], Free("System.Math.Abs(n)"));
	}

	[Fact]
	public void And_an_expression_that_will_not_parse_answers_nothing_rather_than_empty()
	{
		// Null, not an empty set: a caller has to be able to fall back rather than conclude
		// the expression asks for nothing.
		Assert.Null(RoslynCSharpScanner.Instance.FreeNames("left +"));
	}

	[Fact]
	public void And_the_names_a_parser_can_supply_are_found_where_they_are_meant()
	{
		Assert.Equal(
			["context", "parserSpan", "t"],
			Free("context.Say(t, parserSpan)"));
	}
}
