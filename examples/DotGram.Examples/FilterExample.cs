using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// A filter language, of the kind an API puts in a query string or a rules engine reads
// from a table:
//
//     Price > 10 AND Country IN ('UK', 'DE') OR NOT Discontinued
//
//     var filter = Filter.Read(text);
//     filter.Matches(row)          // row is IReadOnlyDictionary<string, object?>
//
// It looks like the calculator and is a different problem. The calculator's operands are
// all one type and its result is a number; here the operands are heterogeneous — a
// number, a string, a date, a list — the comparisons are typed, and what comes out is a
// tree somebody evaluates against data they have.
//
// What it exercises that the arithmetic examples do not:
//
//   * literals of several types in one grammar, each becoming the C# type it means;
//   * `IN (…)` — an operator whose right side is a list rather than a value;
//   * precedence between OR, AND and NOT, written with binding powers in one rule
//     (§4.3.1), the same shape the one-rule calculator uses for arithmetic;
//   * an AST that is plain records, so evaluating it is a `switch` a caller could have
//     written and nothing here has to invent a visitor.
//
// The evaluation is deliberately dull — comparison by `IComparable`, `IN` by `Contains`
// — because the point is the shape that comes out of the parser, not what somebody does
// with it. Turning the same tree into a SQL `WHERE` or an `Expression<Func<T, bool>>` is
// the same walk over the same records.

[Gram("""
	@using DotGram.Examples;
	@using System;
	@using System.Globalization;

	Trivia = [' ' | '\t']*

	KeywordBoundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']

	Filter : @Predicate = predicate: Expr & eof => @(predicate)

	// One rule for the whole language, with the strengths saying what binds tighter:
	// OR loosest, then AND, then NOT, then the comparisons.
	Expr : @Predicate = left: Expr & "OR"  & right: Expr << 1 => @(new Any(left, right))
	                  | left: Expr & "AND" & right: Expr << 2 => @(new All(left, right))
	                  | "NOT" & operand: Expr             >> 3 => @(new Not(operand))
	                  | '(' & inner: Expr & ')'                => @(inner)
	                  | field: Name & "IN" & '(' & values: List & ')'
	                      => @(new In(field, values))
	                  | field: Name & op: Op & value: Value    => @(new Compare(field, op, value))
	                  | field: Name                            => @(new Truth(field))

	List : @object[] = Value & (',' & Value)*

	Op : @string = text: (">=" | "<=" | "<>" | "!=" | "=" | ">" | "<") => @(text)

	Value : @object = text: Number => @(text)
	                | text: Text   => @(text)
	                | "true"       => @(true)
	                | "false"      => @(false)
	                // `null` is a value here, and the rule's type is `@object` rather than
	                // `@object?` because a nullable reference is C#'s annotation rather than
	                // a type — so the one alternative that means absence says so with `null!`.
	                | "null"       => @((object)null!)

	Number : @object = digits: (['0'..'9']+ & ('.' & ['0'..'9']+)?)
	                     => @(decimal.Parse(digits, CultureInfo.InvariantCulture))

	Text : @object = '\'' & body: Body & '\'' => @(body)

	Body : @string = text: ([^ '\''] | "''")* => @(text.Replace("''", "'"))

	Name : @string = text: (['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '.']*)
	                   => @(text)

	parse Filter
	""")]
public sealed partial class Filter
{
	/// <summary>Reads a filter, or throws where the text is not one.</summary>
	public static Predicate Read(string text) => ParseFilter(text);
}

/// <summary>One node of a filter. Plain records, so a caller walks it with patterns.</summary>
public abstract record Predicate
{
	/// <summary>Whether this row satisfies the filter.</summary>
	public bool Matches(IReadOnlyDictionary<string, object?> row) => this switch
	{
		Any(var left, var right)     => left.Matches(row) || right.Matches(row),
		All(var left, var right)     => left.Matches(row) && right.Matches(row),
		Not(var operand)             => !operand.Matches(row),
		Truth(var field)             => Read(row, field) is true,
		In(var field, var values)    => values.Any(value => Same(Read(row, field), value)),
		Compare(var f, var op, var v) => Holds(Read(row, f), op, v),

		_ => false,
	};

	static object? Read(IReadOnlyDictionary<string, object?> row, string field) =>
		row.TryGetValue(field, out var value) ? value : null;

	static bool Same(object? left, object? right) =>
		left is null || right is null ? left is null && right is null : Compare(left, right) == 0;

	/// <remarks>
	/// A missing field satisfies nothing, which is the choice every filter language has to
	/// make and the one that surprises least: an absent value is not equal to, greater
	/// than, or less than anything.
	/// </remarks>
	static bool Holds(object? left, string op, object? right)
	{
		if (left is null || right is null)
			return op is "<>" or "!=" ? !(left is null && right is null) : left is null && right is null;

		var order = Compare(left, right);

		return op switch
		{
			"="  => order == 0,
			"<>" => order != 0,
			"!=" => order != 0,
			">"  => order > 0,
			"<"  => order < 0,
			">=" => order >= 0,
			"<=" => order <= 0,
			_    => false,
		};
	}

	/// <remarks>
	/// Numbers compare as numbers however they arrived — a row holding an `int` against a
	/// filter that wrote a decimal is the ordinary case, not an error.
	/// </remarks>
	static int Compare(object left, object right)
	{
		if (left is IConvertible && right is decimal)
			return Convert.ToDecimal(left, CultureInfo.InvariantCulture).CompareTo((decimal)right);

		if (left is IComparable comparable && left.GetType() == right.GetType())
			return comparable.CompareTo(right);

		return string.CompareOrdinal(
			Convert.ToString(left, CultureInfo.InvariantCulture),
			Convert.ToString(right, CultureInfo.InvariantCulture));
	}
}

public sealed record Any    (Predicate Left, Predicate Right)               : Predicate;
public sealed record All    (Predicate Left, Predicate Right)               : Predicate;
public sealed record Not    (Predicate Operand)                             : Predicate;
public sealed record Truth  (string Field)                                  : Predicate;
public sealed record In     (string Field, IReadOnlyList<object?> Values)   : Predicate;
public sealed record Compare(string Field, string Op, object? Value)        : Predicate;
