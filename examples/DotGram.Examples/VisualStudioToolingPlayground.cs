using System;

using DotGram;

namespace DotGram.Examples;

// Manual Visual Studio tooling playground.
//
// Keep new IDE scenarios in this file so they can be tested without searching through
// unrelated examples. The file is compiled with DotGram.Examples: a green build also
// proves that the grammar itself remains valid.
//
// Run the checks in the experimental Visual Studio instance (RootSuffix DotGram):
//
//  1. Hover `Sum`, `Product`, or `Primary` in the grammar.
//     Expected: classified DotGram Quick Info; referenced rules expand by link.
//
//  2. Put the caret on a rule reference and press F12 / Shift+F12.
//     Expected: rule declaration / all declaration and reference locations.
//
//  3. Put the caret on the capture `left` in Product and press Shift+F12.
//     Expected: the definition and both `left` uses inside `@(...)` are listed.
//     Ctrl+R, Ctrl+R should rename all three occurrences together.
//
//  4. Put the caret on `decimal`, `Parse`, or `Raise` inside C# transitions and press
//     F12. Expected: BCL symbols open Metadata-as-Source; `Raise` opens the C# method
//     below. Hover shows Roslyn Quick Info.
//
//  5. Temporarily type after an `@` or inside `@(...)`, then press Ctrl+Space.
//     Examples: `@dec`, `@(decimal.Pa`, `@(Raise(left, ri`.
//     Expected: completion items come from Roslyn. Undo the temporary edit afterward.
//
//  6. Move the caret through the grammar and use the DotGram navigation dropdown.
//     Expected: the current rule stays selected and choosing another rule navigates.
//
//  7. Click beside matching `()`, `[]`, or `{}` and collapse multiline rules/comments.
//     Expected: brace matching and folding work without affecting C# outside the string.

[Gram("""
	@using System.Globalization;

	trivia = [' ' | '\t']*

	Sum     : @decimal = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
	                   | value: Product                                   => @(value)

	Product : @decimal = left: Product & op: ['*' | '/'] & right: Unary   => @(op == "*" ? left * right : left / right)
	                   | value: Unary                                     => @(value)

	Unary   : @decimal = '-' & operand: Unary                             => @(-operand)
	                   | value: Primary                                   => @(value)

	Primary : @decimal = '(' & inner: Sum & ')'                           => @(inner)
	                   | digits: ['0'..'9']+                              => @(decimal.Parse(digits, CultureInfo.InvariantCulture))

	// F12 on Raise should navigate to the method below. Shift+F12 on `left` should
	// include this C# occurrence as well as its capture definition.
	Power   : @decimal = left: Primary & '^' & right: Unary               => @(Raise(left, right))
	                   | value: Primary                                   => @(value)

	parse Sum as ToolingEvaluate
	""")]
public static partial class VisualStudioToolingPlayground
{
	// Target for F12 from the Power rule above.
	static decimal Raise(decimal value, decimal exponent) =>
		(decimal)Math.Pow((double)value, (double)exponent);
}
