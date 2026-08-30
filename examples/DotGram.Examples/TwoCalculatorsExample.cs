using System;
using System.Globalization;

using DotGram;

namespace DotGram.Examples;

// One grammar, two calculators — one that works in `int` and one that works in `decimal`.
//
// The arithmetic is written once. What tells the two apart is a single rule, `Value`, and
// the publication says which one it means:
//
//     parse Sum with (Value = IntNumber)     as EvaluateInt
//     parse Sum with (Value = DecimalNumber) as EvaluateDecimal
//
// A `with` on a publication substitutes a rule across everything that publication reaches
// (docs/syntax.md §5.1) — not only what is written near it. `Sum` calls `Product` calls
// `Unary` calls `Primary` calls `Value`, and every one of them is specialized for each
// publication, so the two share no state and neither knows the other exists.
//
// The types follow. `Sum : Value` says "whatever `Value` produces" (§4.1 case 3), so
// `EvaluateInt` hands back an `int` and `EvaluateDecimal` a `decimal`, from the same four
// rules. Nothing in `Sum` mentions either type, and the `=>` bodies are written once:
// `left + right` is C#'s `+` on whichever type arrived.
//
// This is the whole of the mechanism. There is no generic rule, no type parameter, and
// nothing declared twice — a rebinding is a substitution, and a substitution changes what
// the rules around it produce as readily as what they read.

[Gram("""
	trivia = [' ' | '\t']*

	Digits = ['0'..'9']+

	IntNumber     : @int     = d: Digits                     => @int.Parse(d)
	DecimalNumber : @decimal = d: (Digits & ('.' & Digits)?) => @(Decimal(d))

	// The rule the two publications disagree about. What it says here is what
	// `EvaluateInt` gets; `EvaluateDecimal` never calls it.
	Value : @int = d: Digits => @int.Parse(d)

	Sum     : Value = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
	                | value: Product                                   => @(value)

	Product : Value = left: Product & op: ['*' | '/'] & right: Unary   => @(op == "*" ? left * right : left / right)
	                | value: Unary                                     => @(value)

	Unary   : Value = '-' & operand: Unary                             => @(-operand)
	                | value: Primary                                   => @(value)

	Primary : Value = '(' & inner: Sum & ')'                           => @(inner)
	                | value: Value                                     => @(value)

	parse Sum with (Value = IntNumber)     as EvaluateInt
	parse Sum with (Value = DecimalNumber) as EvaluateDecimal
	""")]
public static partial class TwoCalculators
{
	// EvaluateInt, TryEvaluateInt, EvaluateDecimal and TryEvaluateDecimal are generated
	// here — the first pair over `int`, the second over `decimal`.

	/// <summary>One place for the culture, so the grammar does not have to name it.</summary>
	public static decimal Decimal(string digits) =>
		decimal.Parse(digits, CultureInfo.InvariantCulture);
}
