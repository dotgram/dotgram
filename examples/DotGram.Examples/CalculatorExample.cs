using System;

using DotGram;

namespace DotGram.Examples;

// An arithmetic calculator: precedence, associativity, parentheses, and a result that
// is an int rather than a parse tree somebody still has to walk.
//
// Three things carry it, and none of them is notation invented for the purpose.
//
//   Precedence is levels. One rule per level, each calling the next.
//
//   Associativity is which side the rule recurses on. `left: Sum & …` is
//   left-recursive, so `1-2-3` groups as `(1-2)-3` — what that shape has meant since
//   BNF. Nothing is written to say so.
//
//   The value is the rule's own type. `: @int` names it and `=>` builds it, so a
//   published method hands back an int.
//
// Spaces are insignificant because `Trivia` is an ordinary rule and this grammar
// shadows it. That is the whole of the mechanism.

[Gram("""
	Trivia  = [' ' | '\t']*

	Sum     : @int = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
	               | value: Product                                   => @(value)

	Product : @int = left: Product & op: ['*' | '/'] & right: Unary   => @(op == "*" ? left * right : left / right)
	               | value: Unary                                     => @(value)

	Unary   : @int = '-' & operand: Unary                             => @(-operand)
	               | value: Primary                                   => @(value)

	Primary : @int = '(' & inner: Sum & ')'                           => @(inner)
	               | digits: ['0'..'9']+                              => @int.Parse(digits)

	parse Sum as Evaluate
	""")]
public static partial class Calculator
{
	// Evaluate and TryEvaluate are generated here. Nothing else is needed: the grammar
	// is the whole of the calculator, and the arithmetic is the C# in it.

	/// <summary>What an expression works out to, or the reason it does not.</summary>
	public static string Explain(string expression)
	{
		var answer = TryEvaluate(expression);

		return answer.IsSuccess
			? expression + " = " + answer.Value
			: answer.Error + " at " + answer.Position;
	}
}
