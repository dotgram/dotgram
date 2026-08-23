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
// tree means is somebody else's business — Expression.cs, which mentions no parser.
//
//     var tree = ExpressionParser.Read("1 + 2 * 3");
//
//     tree.Evaluate()   // 7
//     tree.Print()      // (1 + (2 * 3))
//
// A parse tree that only a walker written for it can read is a parse tree somebody has
// to maintain. This one is ordinary C# data: records, value equality, and patterns for
// anything the tree does not already know how to do.
//
// OneRuleTreeExample builds the identical tree from one rule of binding powers. How a
// grammar is written and what its `=>` builds are independent choices, and those two
// examples are the same second choice made over different firsts.

[Gram("""
	@using System.Globalization;
	@using DotGram.Examples;

	using Lexical;

	context Lexical
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
	// Read and TryRead are generated here, and hand back an Expression. There is nothing
	// else to write: what the tree does is on the tree.
}
