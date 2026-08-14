using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// The whole of it in one rule: a notation goes in, a typed tree comes out, and the tree
// is the same one ExpressionTreeExample builds out of five.
//
// This is the shape to copy for a small DSL. Everything the language is — which
// operators there are, how tightly each binds, which way each groups, and what each one
// becomes — is eight lines, one per operator, and each line says all four things about
// itself in the order they are read:
//
//     left: Expr & '*' & right: Expr  << 2 => @(new Mul(left, right))
//     ──────────────────────────────  ────    ───────────────────────
//     what it looks like              how it  what it becomes
//                                     binds
//
// Adding an operator is a line. Changing what one binds like is a number on that line.
// Neither touches anything else, which is what a notation still being designed needs —
// and what a stack of levels cannot give, since there precedence is which rule calls
// which and inserting one means a new rule and an edit to each of its neighbours.
//
// The node types are ExpressionTreeExample's, unchanged, and so is what walks them:
// `ExpressionParser.Evaluate` and `.Print` take an `Expression` and neither knows nor
// cares which grammar built it. That is the separation worth taking from all of this —
// how the grammar is written and what `=>` builds are two independent choices.

[Gram("""
	@using System.Globalization;
	@using DotGram.Examples;

	using Lexical;

	scope Lexical
	{
		Trivia = none

		Digits = ['0'..'9']+ & ('.' & ['0'..'9']+)?
	}

	Trivia = [' ' | '\t']*

	Expr : @Expression = left: Expr & '+' & right: Expr  << 1 => @(new Add(left, right))
	                   | left: Expr & '-' & right: Expr  << 1 => @(new Sub(left, right))
	                   | left: Expr & '*' & right: Expr  << 2 => @(new Mul(left, right))
	                   | left: Expr & '/' & right: Expr  << 2 => @(new Div(left, right))
	                   | left: Expr & '^' & right: Expr  >> 3 => @(new Pow(left, right))
	                   | '-' & operand: Expr             >> 3 => @(new Negate(operand))
	                   | '(' & inner: Expr & ')'              => @(inner)
	                   | digits: Digits                       => @(new Number(decimal.Parse(digits, CultureInfo.InvariantCulture)))

	parse Expr as Read
	""")]
public static partial class OneRuleParser
{
	// Read and TryRead are generated here. There is nothing else to write: the walks
	// live in ExpressionParser and work on the same records.
}
