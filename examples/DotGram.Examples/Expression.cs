using System;
using System.Globalization;

namespace DotGram.Examples;

// The tree two of the examples build, and everything it can do.
//
// Not an example of a grammar — there is none in this file — but the half a grammar
// hands its work to. ExpressionTreeExample builds these out of five rules and
// OneRuleTreeExample out of one, and neither is mentioned here: what the tree means is
// C#'s business, and keeping it in a file of its own is what makes that true rather than
// merely said.
//
// One record per operation, so a walk names `Add` instead of testing a string against
// "+", nothing has to handle an operator that cannot occur, and a node taking different
// operands later — a call, an index, a conditional — is a record beside these rather
// than a case that does not fit.
//
// `Evaluate` and `Print` are abstract on `Expression` and overridden on each node. That
// costs a line per node and buys the check C# cannot make on a `switch`: a hierarchy is
// never closed, so a `switch` over one always needs a default arm and can never be
// proved total, while a node that forgets to override does not compile. What is left for
// patterns is everything the tree does *not* know how to do — see the rewrite in
// ExampleTests, which doubles every literal without either of these knowing about it.

/// <summary>An arithmetic expression: a literal, or an operation on smaller ones.</summary>
public abstract record Expression
{
	/// <summary>What it works out to.</summary>
	/// <exception cref="DivideByZeroException">A division whose right side is zero.</exception>
	public abstract decimal Evaluate();

	/// <summary>It written back out, with every grouping made explicit.</summary>
	public abstract string Print();

	/// <summary>An infix operation in brackets — how all of them print.</summary>
	protected static string Group(Expression left, string @operator, Expression right) =>
		$"({left.Print()} {@operator} {right.Print()})";
}

/// <summary>A literal.</summary>
public sealed record Number(decimal Value) : Expression
{
	public override decimal Evaluate() => Value;
	public override string  Print()    => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>Unary minus.</summary>
public sealed record Negate(Expression Operand) : Expression
{
	public override decimal Evaluate() => -Operand.Evaluate();
	public override string  Print()    => "-" + Operand.Print();
}

/// <summary><c>a + b</c>.</summary>
public sealed record Add(Expression Left, Expression Right) : Expression
{
	public override decimal Evaluate() => Left.Evaluate() + Right.Evaluate();
	public override string  Print()    => Group(Left, "+", Right);
}

/// <summary><c>a - b</c>.</summary>
public sealed record Sub(Expression Left, Expression Right) : Expression
{
	public override decimal Evaluate() => Left.Evaluate() - Right.Evaluate();
	public override string  Print()    => Group(Left, "-", Right);
}

/// <summary><c>a * b</c>.</summary>
public sealed record Mul(Expression Left, Expression Right) : Expression
{
	public override decimal Evaluate() => Left.Evaluate() * Right.Evaluate();
	public override string  Print()    => Group(Left, "*", Right);
}

/// <summary><c>a / b</c>.</summary>
public sealed record Div(Expression Left, Expression Right) : Expression
{
	public override decimal Evaluate() => Left.Evaluate() / Right.Evaluate();
	public override string  Print()    => Group(Left, "/", Right);
}

/// <summary><c>a ^ b</c>.</summary>
public sealed record Pow(Expression Left, Expression Right) : Expression
{
	public override decimal Evaluate() => Raise(Left.Evaluate(), Right.Evaluate());
	public override string  Print()    => Group(Left, "^", Right);

	/// <summary>
	/// Power — the one operator C# does not have.
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
