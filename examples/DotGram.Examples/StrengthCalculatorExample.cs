using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// DecimalCalculatorExample written the other way: one rule, eight alternatives, the same
// language and the same answers on every input.
//
// There it is five rules stacked — Sum calls Product calls Unary calls Power calls
// Primary — and precedence is the stacking. That is the cheaper option and the one to
// reach for first. What it costs is that precedence is nowhere written down: it is
// implied by which rule calls which, so reading it off means following the chain, and
// inserting a level means a new rule and an edit to each of its neighbours.
//
// Here the alternative says its own strength instead (docs/syntax.md §4.3.1):
//
//   << n   the operand to the right is read one strength tighter → groups left
//   >> n   the operand to the right is read at n                 → groups right
//
// Higher binds tighter. The numbers are the author's and need not be contiguous — the
// gaps are where a level goes in later, as a number, with nothing else touched.
//
// The shape this buys is `left: Expr & op & right: Expr`, the same rule on both sides.
// Levels refuse it: ordered choice has nothing to settle the grouping with, so the
// trailing call would take everything to the right, and §4.3 says to write the operands
// at the next level down instead — which is what forces a rule per level. A strength is
// exactly the information that was missing, so the shape a diagnostic rejects there is
// the ordinary case here.
//
// A rule uses one convention or the other. Levels and strengths in one rule would be two
// answers to the same question, and the compiler says so rather than choosing.
//
// The tests check the two expression by expression, including the one that looks as
// though it could not survive the translation: `^` binds tighter than unary minus on its
// left (-2^2 is -(2^2)) and looser on its right (2^-2 parses), which the five-rule
// version says by naming two different rules either side of the operator.
//
// One number says both, because a strength is not symmetric to begin with. It is the
// strength the operand to the *right* is read at, and nothing else. A prefix has no left
// operand, so it is a base — one of the alternatives that start an expression — and a
// base is entered whatever strength was asked for, since there is nothing to its left
// for anything to bind more tightly than. So `^` and unary minus at the same 3 gives
// -(2^2) on one side and 2^(-2) on the other.

[Gram("""
	@using System.Globalization;

	using Lexical;

	scope Lexical
	{
		Trivia = none

		Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?
	}

	Trivia = [' ' | '\t']*

	Expr : @decimal = left: Expr & '+' & right: Expr  << 1 => @(left + right)
	                | left: Expr & '-' & right: Expr  << 1 => @(left - right)
	                | left: Expr & '*' & right: Expr  << 2 => @(left * right)
	                | left: Expr & '/' & right: Expr  << 2 => @(left / right)
	                | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
	                | '-' & operand: Expr             >> 3 => @(-operand)
	                | '(' & inner: Expr & ')'               => @(inner)
	                | digits: Number                        => @(decimal.Parse(digits, CultureInfo.InvariantCulture))

	parse Expr as Evaluate
	""")]
public static partial class StrengthCalculator
{
	/// <summary>What an expression works out to, or the reason it does not.</summary>
	public static string Explain(string expression)
	{
		var answer = TryEvaluate(expression);

		return answer.IsSuccess
			? expression + " = " + answer.Value.ToString(CultureInfo.InvariantCulture)
			: answer.Error + " at " + answer.Position;
	}

	/// <summary>Power. See DecimalCalculatorExample for why it is not one line.</summary>
	static decimal Raise(decimal value, decimal exponent)
	{
		if (exponent != decimal.Truncate(exponent))
			return (decimal)Math.Pow((double)value, (double)exponent);

		if (exponent < 0)
			return Raise(1m / value, -exponent);

		var result = 1m;

		for (var i = 0; i < exponent; i++)
			result *= value;

		return result;
	}
}
