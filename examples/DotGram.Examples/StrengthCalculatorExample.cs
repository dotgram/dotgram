using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// The third calculator, and the shortest: one rule, seven alternatives, a whole
// expression language.
//
// DecimalCalculatorExample says precedence by stacking rules — Sum calls Product calls
// Unary calls Power calls Primary — and associativity by choosing which operand sits at
// the rule's own level. That works, it is cheap, and it is what to reach for first. What
// it cannot say is a unary operator stronger than a binary one that shares its
// character: `-1-2` needs the leading `-` to bind tighter than the one in the middle,
// and stacking cannot put one rule both above and below another.
//
// So the alternative says its own strength (docs/syntax.md §4.3.1):
//
//   << n   the operand to the right is read one strength tighter → groups left
//   >> n   the operand to the right is read at n                 → groups right
//
// Higher binds tighter. The numbers are the author's; the gaps are where a level is
// inserted later without renumbering anything.
//
// That is also why an alternative recursive on both sides is written here and refused in
// the other two. `left: E & op & right: E` gives ordered choice nothing to settle the
// grouping with — the trailing call would take everything to the right. The strength is
// exactly that missing information, which is why the same shape is a diagnostic there
// and the ordinary case here.
//
// A rule uses one convention or the other. Levels and strengths in one rule would be two
// answers to the same question, and the compiler says so rather than choosing.
//
// What levels can say and this cannot: a strength is one number, so it says the same
// thing about both sides of an operator. Python's `**` binds tighter than unary minus on
// its left (-2**2 is -4) and looser on its right (2**-1 parses), and levels say that by
// naming two different rules either side of it — `left: Primary & '^' & right: Unary`.
// Here `^` is 3 and unary minus is 4, which keeps 2^-2 working and makes -2^2 read as
// (-2)^2 — the spreadsheet reading rather than the mathematical one. Swapping the two
// numbers trades one for the other. Neither is wrong; one number cannot be both.

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
	                | '-' & operand: Expr             >> 4 => @(-operand)
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
