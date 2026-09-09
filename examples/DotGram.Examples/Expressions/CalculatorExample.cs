using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// A calculator written once and published three times: over `int`, over `decimal`, and
// as a tree of its own.
//
// One rule with eight alternatives, and the precedence written down rather than implied
// by which rule calls which (docs/syntax.md §4.3.1):
//
//   << n   the operand to the right is read one strength tighter → groups left
//   >> n   the operand to the right is read at n                 → groups right
//
// Higher binds tighter, and the numbers need not be contiguous: the gaps are where a
// level goes in later, as a number, with nothing else touched. The same language spelled
// as a stack of rules is in tests/DotGram.Tests/Calculators, where the two are held
// against each other expression by expression.
//
// What `with` substitutes is `Value`, the one rule that reads a number. Everything else
// is written once, and `Expr : Value` carries the substituted type out to each published
// method — so the three differ in what they hand back and in nothing else.
//
// The actions are not written three ways either. `left + right` is C#: over `int` it
// adds, over `decimal` it adds, and over `Node` it is the operator declared below, which
// builds a node instead. The grammar says where an operator goes and which operands it
// takes; what it then means is not the grammar's business.
//
// `^` is the exception that shows what the rule rests on. C# has no `^` for `decimal`,
// and its `^` on `int` is exclusive-or rather than power, so that one is a method —
// overloaded the three ways the operators are overloaded once.

[Gram("""
	@using System.Globalization;

	trivia = Std.Spacing?

	Value : @int = d: Std.Digits => @(int.Parse(d))
	Point        = Std.Digits & ('.' & Std.Digits)?

	Expr : Value = left: Expr & '+' & right: Expr  << 1 => @(left + right)
	             | left: Expr & '-' & right: Expr  << 1 => @(left - right)
	             | left: Expr & '*' & right: Expr  << 2 => @(left * right)
	             | left: Expr & '/' & right: Expr  << 2 => @(left / right)
	             | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
	             | '-' & operand: Expr             >> 3 => @(-operand)
	             | '(' & inner: Expr & ')'              => @(inner)
	             | value: Value                         => @(value)

	IntNumber     : @int     = d: Std.Digits => @(int.Parse(d))
	DecimalNumber : @decimal = d: Point      => @(decimal.Parse(d, CultureInfo.InvariantCulture))
	NodeNumber    : @Node    = d: Point      => @(new Node.Number(decimal.Parse(d, CultureInfo.InvariantCulture)))

	parse Expr with (Value = IntNumber)     as EvaluateInt
	parse Expr with (Value = DecimalNumber) as EvaluateDecimal
	parse Expr with (Value = NodeNumber)    as BuildTree
	""")]
public static partial class Calculator
{
	/// <summary>The tree the third parser builds, and the operators that build it.</summary>
	public abstract record Node
	{
		public sealed record Number(decimal Of)                     : Node;
		public sealed record Binary(char Op, Node Left, Node Right) : Node;
		public sealed record Negate(Node Of)                        : Node;

		public static Node operator +(Node left, Node right) => new Binary('+', left, right);
		public static Node operator -(Node left, Node right) => new Binary('-', left, right);
		public static Node operator *(Node left, Node right) => new Binary('*', left, right);
		public static Node operator /(Node left, Node right) => new Binary('/', left, right);
		public static Node operator -(Node of)               => new Negate(of);
	}

	/// <summary>What an expression works out to, or the reason it does not.</summary>
	public static string Explain(string expression)
	{
		var answer = TryEvaluateInt(expression);

		return answer.IsSuccess
			? expression + " = " + answer.Value
			: answer.Error + " at " + answer.Position;
	}

	// Power, three ways, because no operator will do: `^` is exclusive-or on `int` and is
	// not defined for `decimal` at all. A decimal power written to be exact is longer than
	// this — tests/DotGram.Tests/Calculators/DecimalCalculator.cs has one.
	static int     Raise(int     left, int     right) => (int)Math.Pow(left, right);
	static decimal Raise(decimal left, decimal right) => (decimal)Math.Pow((double)left, (double)right);
	static Node    Raise(Node    left, Node    right) => new Node.Binary('^', left, right);
}
