using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// The same calculator, not calculating. `=>` builds a node instead of a number, and
// what comes back is a tree the caller can look at, rewrite, walk twice, or compile.
//
// That is the whole of the difference. The grammar is the one from
// DecimalCalculatorExample with its arithmetic replaced by constructors, and it is the
// shape every small DSL wants: a notation goes in, a typed tree comes out, and what the
// tree means is C#'s business rather than the parser's.
//
// The tree is records, so C# reads it back with patterns:
//
//     Evaluate(Expression node) => node switch
//     {
//         Number(var value)               => value,
//         Negate(var operand)             => -Evaluate(operand),
//         Binary(var op, var l, var r)    => Apply(op, Evaluate(l), Evaluate(r)),
//     };
//
// A parse tree that only a walker written for it can read is a parse tree somebody has
// to maintain. This one is ordinary C# data, and the compiler checks the walk is total.

/// <summary>A node of the tree. Records, so callers read them back with patterns.</summary>
public abstract record Expression;

/// <summary>A literal.</summary>
public sealed record Number(decimal Value) : Expression;

/// <summary>Unary minus.</summary>
public sealed record Negate(Expression Operand) : Expression;

/// <summary>One of <c>+ - * / ^</c>, and what it is applied to.</summary>
public sealed record Binary(string Operator, Expression Left, Expression Right) : Expression;

[Gram("""
	@using System.Globalization;
	@using DotGram.Examples;

	using Lexical;

	scope Lexical
	{
		// Between digits a space is not nothing. See DecimalCalculatorExample.
		Trivia = none

		Digits = ['0'..'9']+ & ('.' & ['0'..'9']+)?
	}

	Trivia  = [' ' | '\t']*

	Sum     : @Expression = left: Sum     & op: ['+' | '-'] & right: Product => @(new Binary(op, left, right))
	                      | value: Product                                   => @(value)

	Product : @Expression = left: Product & op: ['*' | '/'] & right: Unary   => @(new Binary(op, left, right))
	                      | value: Unary                                     => @(value)

	Unary   : @Expression = '-' & operand: Unary                             => @(new Negate(operand))
	                      | value: Power                                     => @(value)

	Power   : @Expression = left: Primary & op: '^' & right: Unary           => @(new Binary(op, left, right))
	                      | value: Primary                                   => @(value)

	Primary : @Expression = '(' & inner: Sum & ')'                           => @(inner)
	                      | digits: Digits                                   => @(new Number(decimal.Parse(digits, CultureInfo.InvariantCulture)))

	parse Sum as Read
	""")]
public static partial class ExpressionParser
{
	// Read and TryRead are generated here, and hand back an Expression.

	/// <summary>The tree, worked out. One pattern per kind of node, and no default.</summary>
	/// <exception cref="DivideByZeroException">A division whose right side is zero.</exception>
	public static decimal Evaluate(Expression node) => node switch
	{
		Number(var value)                    => value,
		Negate(var operand)                  => -Evaluate(operand),
		Binary(var op, var left, var right)  => Apply(op, Evaluate(left), Evaluate(right)),

		// Unreachable while `Expression` has the three shapes above, and the reason it is
		// written: add a fourth and this is what says where the walk stopped being total.
		_ => throw new ArgumentOutOfRangeException(nameof(node), node, "Unknown node."),
	};

	/// <summary>The tree, written back out with every grouping made explicit.</summary>
	public static string Print(Expression node) => node switch
	{
		Number(var value)                    => value.ToString(CultureInfo.InvariantCulture),
		Negate(var operand)                  => "-" + Print(operand),
		Binary(var op, var left, var right)  => $"({Print(left)} {op} {Print(right)})",

		_ => throw new ArgumentOutOfRangeException(nameof(node), node, "Unknown node."),
	};

	static decimal Apply(string op, decimal left, decimal right) => op switch
	{
		"+" => left + right,
		"-" => left - right,
		"*" => left * right,
		"/" => left / right,
		_   => Raise(left, right),
	};

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
