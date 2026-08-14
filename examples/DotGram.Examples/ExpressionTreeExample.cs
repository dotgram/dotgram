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
// One node type per operation rather than one carrying the operator as a string. The
// grammar pays for it — `'+'` and `'-'` are two alternatives where they were one set —
// and everything downstream is repaid: a walk names `Add` instead of testing for "+",
// nothing has to handle an operator that cannot occur, and a node that takes different
// operands later (a call, an index, a conditional) is a record beside these rather than
// a string case that does not fit.
//
// The tree is records, so C# reads it back with patterns:
//
//     Evaluate(Expression node) => node switch
//     {
//         Number(var value)   => value,
//         Negate(var operand) => -Evaluate(operand),
//         Add(var l, var r)   => Evaluate(l) + Evaluate(r),
//         …
//     };
//
// A parse tree that only a walker written for it can read is a parse tree somebody has
// to maintain. This one is ordinary C# data.

/// <summary>A node of the tree. Records, so callers read them back with patterns.</summary>
public abstract record Expression;

/// <summary>A literal.</summary>
public sealed record Number(decimal Value) : Expression;

/// <summary>Unary minus.</summary>
public sealed record Negate(Expression Operand) : Expression;

/// <summary><c>a + b</c>.</summary>
public sealed record Add(Expression Left, Expression Right) : Expression;

/// <summary><c>a - b</c>.</summary>
public sealed record Sub(Expression Left, Expression Right) : Expression;

/// <summary><c>a * b</c>.</summary>
public sealed record Mul(Expression Left, Expression Right) : Expression;

/// <summary><c>a / b</c>.</summary>
public sealed record Div(Expression Left, Expression Right) : Expression;

/// <summary><c>a ^ b</c>.</summary>
public sealed record Pow(Expression Left, Expression Right) : Expression;

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

	// Two left-recursive alternatives per level, one per operator. A rule may have as
	// many as it likes: what a match records is which alternative it came through, so
	// they need no common type to be collected in.
	Sum     : @Expression = left: Sum     & '+' & right: Product => @(new Add(left, right))
	                      | left: Sum     & '-' & right: Product => @(new Sub(left, right))
	                      | value: Product                       => @(value)

	Product : @Expression = left: Product & '*' & right: Unary   => @(new Mul(left, right))
	                      | left: Product & '/' & right: Unary   => @(new Div(left, right))
	                      | value: Unary                         => @(value)

	Unary   : @Expression = '-' & operand: Unary                 => @(new Negate(operand))
	                      | value: Power                         => @(value)

	Power   : @Expression = left: Primary & '^' & right: Unary   => @(new Pow(left, right))
	                      | value: Primary                       => @(value)

	Primary : @Expression = '(' & inner: Sum & ')'               => @(inner)
	                      | digits: Digits                       => @(new Number(decimal.Parse(digits, CultureInfo.InvariantCulture)))

	parse Sum as Read
	""")]
public static partial class ExpressionParser
{
	// Read and TryRead are generated here, and hand back an Expression.

	/// <summary>The tree, worked out. One arm per kind of node.</summary>
	/// <exception cref="DivideByZeroException">A division whose right side is zero.</exception>
	public static decimal Evaluate(Expression node) => node switch
	{
		Number(var value)   => value,
		Negate(var operand) => -Evaluate(operand),

		Add(var left, var right) => Evaluate(left) + Evaluate(right),
		Sub(var left, var right) => Evaluate(left) - Evaluate(right),
		Mul(var left, var right) => Evaluate(left) * Evaluate(right),
		Div(var left, var right) => Evaluate(left) / Evaluate(right),
		Pow(var left, var right) => Raise(Evaluate(left), Evaluate(right)),

		// C# has no closed hierarchies, so the compiler cannot check that the arms above
		// cover `Expression` and this one has to be here. It is where a node added later
		// and forgotten here shows up, which is why it says which node rather than just
		// throwing.
		_ => throw new ArgumentOutOfRangeException(nameof(node), node, "Unknown node."),
	};

	/// <summary>The tree, written back out with every grouping made explicit.</summary>
	public static string Print(Expression node) => node switch
	{
		Number(var value)   => value.ToString(CultureInfo.InvariantCulture),
		Negate(var operand) => "-" + Print(operand),

		Add(var left, var right) => Group(left, "+", right),
		Sub(var left, var right) => Group(left, "-", right),
		Mul(var left, var right) => Group(left, "*", right),
		Div(var left, var right) => Group(left, "/", right),
		Pow(var left, var right) => Group(left, "^", right),

		_ => throw new ArgumentOutOfRangeException(nameof(node), node, "Unknown node."),
	};

	static string Group(Expression left, string @operator, Expression right) =>
		$"({Print(left)} {@operator} {Print(right)})";

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
