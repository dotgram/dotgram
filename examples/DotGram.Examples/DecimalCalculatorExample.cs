using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// The calculator of CalculatorExample, with the two things that make associativity
// visible: an operator that groups the other way, and a number type that does not throw
// the answer away.
//
// Associativity is not notation here. It is which operand is parsed at the rule's own
// level and which at the next one along:
//
//   Sum   = left: Sum     & op  & right: Product    left at this level    1-2-3 = (1-2)-3 = -4
//   Power = left: Primary & '^' & right: Unary      right at this level   2^3^2 = 2^(3^2) = 512
//
// `Product` is tighter than `Sum`, so the right operand of `-` cannot be another `-` and
// the left one can. `Unary` is looser than `Power` and comes back down to it, so the
// right operand of `^` can be another `^` and the left one cannot — which also lets
// 2^-2 mean 2^(-2). Swap the sides and the answers become 2 and 64. Nothing else
// changes, and nothing has to be declared.
//
// The compiler works none of this out. It asks one structural question of an
// alternative, and only because it has to: does it begin with a call to its own rule?
// That one cannot be compiled as written — it would call itself before consuming
// anything, for ever — so docs/syntax.md §4.3 rewrites it into a loop over its tails,
// and `left` stops being a capture and becomes the value built so far, which that
// alternative's own `=>` receives. Every other call is an ordinary call. There is no
// "right-recursive" anywhere in the compiler, because there is nothing to do about it:
// `Power` never calls `Power`, it calls `Unary`, and the grouping falls out of the order
// the calls return in.
//
// Precedence is levels: one rule per level, each calling the next. `^` binds tighter
// than `*` and looser than unary minus, so -2^2 is -4 rather than 4 — the order Python
// uses, and the reason `Power` sits below `Unary` and not above it.
//
// `: @decimal` is the whole of the arithmetic change: 1/8 is 0.125 here and 0 in the
// int calculator, from the same grammar.
//
// Shadowing `Trivia` has one consequence worth knowing rather than discovering: it goes
// between the operands of every sequence, and `['0'..'9']+ & '.' & ['0'..'9']+` is a
// sequence like any other. Left alone, `1 . 5` would match and `digits` would capture
// the spaces with it. So `Number` lives in a scope that shadows `Trivia` with `none` —
// the same §5 shadowing, used the other way round, and the reason the grammar has a
// `scope` in it at all.

[Gram("""
	@using System.Globalization;
	using Lexical;

	scope Lexical
	{
		// Between digits a space is not nothing, so this scope shadows Trivia with the
		// rule that matches nothing at all. Scoping is lexical: `Number` is declared
		// here, so this is the Trivia it is built with, wherever it is called from.
		Trivia = none

		Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?
	}

	Trivia  = [' ' | '\t']*

	Sum     : @decimal = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
	                   | value: Product                                   => @(value)

	Product : @decimal = left: Product & op: ['*' | '/'] & right: Unary   => @(op == "*" ? left * right : left / right)
	                   | value: Unary                                     => @(value)

	Unary   : @decimal = '-' & operand: Unary                             => @(-operand)
	                   | value: Power                                     => @(value)

	Power   : @decimal = left: Primary & '^' & right: Unary               => @(Raise(left, right))
	                   | value: Primary                                   => @(value)

	Primary : @decimal = '(' & inner: Sum & ')'                           => @(inner)
	                   | digits: Number                                   => @(decimal.Parse(digits, CultureInfo.InvariantCulture))

	parse Sum as Evaluate
	""")]
public static partial class DecimalCalculator
{
	// Evaluate and TryEvaluate are generated here.

	/// <summary>What an expression works out to, or the reason it does not.</summary>
	public static string Explain(string expression)
	{
		var answer = TryEvaluate(expression);

		return answer.IsSuccess
			? expression + " = " + answer.Value.ToString(CultureInfo.InvariantCulture)
			: answer.Error + " at " + answer.Position;
	}

	/// <summary>
	/// Power — the one operator C# does not have, which is why this is the only piece of
	/// arithmetic the grammar cannot write inline.
	/// </summary>
	/// <remarks>
	/// Repeated multiplication while the exponent is a whole number, so 2^3^2 is exactly
	/// 512 and not a double rounded back into a decimal. Anything else goes through
	/// <see cref="Math.Pow"/> and comes back with a double's precision, which is the
	/// honest answer to a question decimal cannot ask.
	/// </remarks>
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
