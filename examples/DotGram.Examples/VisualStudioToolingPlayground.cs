using System;

using DotGram;

namespace DotGram.Examples;

// Manual Visual Studio tooling playground.
//
// Keep new IDE scenarios in this file so they can be tested without searching through
// unrelated examples. The file is compiled with DotGram.Examples: a green build also
// proves that the grammar itself remains valid.
// Run the checks below in the experimental Visual Studio instance (RootSuffix DotGram).

[Gram("""
	@using System.Globalization;

	trivia = [' ' | '\t']*

	// Hover Sum or its Product references: classified Quick Info should appear and
	// referenced rules should expand by link. F12/Shift+F12 on Product should navigate
	// to its declaration/show every occurrence. The navigation dropdown should track
	// the current rule and navigate when another rule is selected.
	Sum     : @decimal = left: Sum     & op: ['+' | '-'] & right: Product => @(op == "+" ? left + right : left - right)
	                   | value: Product                                   => @(value)

	// Shift+F12 on the capture `left` should list its definition and both uses inside
	// @(...). Ctrl+R, Ctrl+R should rename all three occurrences together.
	Product : @decimal = left: Product & op: ['*' | '/'] & right: Unary   => @(op == "*" ? left * right : left / right)
	                   | value: Unary                                     => @(value)

	// Temporarily type after @ or inside @(...), then press Ctrl+Space. For example:
	// @dec, @(decimal.Pa), or @(Raise(operand, ri). Roslyn completion should appear;
	// undo the temporary edit afterward.
	Unary   : @decimal = '-' & operand: Unary                             => @(-operand)
	                   | value: Primary                                   => @(value)

	// F12 on decimal or Parse should open Metadata-as-Source; hover should show
	// classified Roslyn Quick Info. Click beside (), [], or {} and collapse this
	// multiline rule/comment to check brace matching and folding.
	Primary : @decimal = '(' & inner: Sum & ')'                           => @(inner)
	                   | digits: ['0'..'9']+                              => @(decimal.Parse(digits, CultureInfo.InvariantCulture))

	// F12 on Raise should navigate to the C# method below. Shift+F12 on `left` should
	// include its occurrence inside this C# expression and its capture definition.
	Power   : @decimal = left: Primary & '^' & right: Unary               => @(Raise(left, right))
	                   | value: Primary                                   => @(value)

	// F12 on ToolingEvaluate should open the generated C# publication method.
	parse Sum as ToolingEvaluate
	""")]
[GramLanguage("dotgram.tooling.playground")]
[GramClassify("Sum", GramClassification.Function)]
[GramClassify("Product.left", GramClassification.Variable)]
// Uncomment each line separately: only the target text inside the quotes should be
// underlined, with GRAM5002 for an unknown rule and GRAM5004 for an unknown capture.
//[GramClassify("Missing", GramClassification.Keyword)]
//[GramClassify("Product.missing", GramClassification.Variable)]
public static partial class VisualStudioToolingPlayground
{
	// Target for F12 from the Power rule above.
	static decimal Raise(decimal value, decimal exponent) =>
		(decimal)Math.Pow((double)value, (double)exponent);

	// F12 on ToolingEvaluate should return to its publication inside the Gram string.
	public static decimal EvaluateForTooling(string expression) =>
		ToolingEvaluate(expression);
}

// Custom-attribute DSL check. In the ToolingQuery string below, `select` should use the
// standard keyword color and `customer` the standard local/variable color. Replacing
// `customer` with `123` should underline the failure position with GRAM5101.
[Gram("""
	trivia    = [' ' | '\t']*
	Keyword   = "select"
	Identifier = ['a'..'z']+
	Query     = Keyword & field: (Identifier)
	parse Query
	""")]
[GramLanguage("dotgram.tooling.query")]
[GramClassify("Keyword", GramClassification.Keyword)]
[GramClassify("Query.field", GramClassification.Variable)]
[GramLanguageMarker(typeof(ToolingQueryAttribute))]
public static partial class ToolingQueryLanguage
{
}

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ToolingQueryAttribute : Attribute
{
}

public sealed class ToolingQuery
{
	public ToolingQuery([ToolingQuery] string text) => Text = text;

	public string Text { get; }
}

public static class ToolingQueryExample
{
	public static readonly ToolingQuery Query = new("select customer");

	// Generated publication calls route their input without a marker attribute.
	// Both strings should have the same DSL colors and diagnostics as Query above.
	public static object ParseDirect() => ToolingQueryLanguage.ParseQuery("select customer");
	public static object TryParseDirect() => ToolingQueryLanguage.TryParseQuery("select customer");
}

// Multiple-publication DSL check. Each generated method must select its own entry rule.
[Gram("""
	trivia       = [' ' | '\t']*
	SelectWord   = "select" | "choose"
	CountWord    = "count"
	Identifier   = ['a'..'z']+
	Operator     = '+' | '-'
	SelectQuery  = SelectWord & field: (Identifier)
	CountQuery   = CountWord & field: (Identifier)
	Operation    = left: (Identifier) & Operator & right: (Identifier)
	parse SelectQuery
	parse CountQuery
	parse Operation
	""")]
[GramLanguage("dotgram.tooling.multi-query")]
[GramClassify("SelectWord", GramClassification.Keyword)]
[GramClassify("CountWord", GramClassification.Keyword)]
[GramClassify("Operator", GramClassification.Operator)]
[GramClassify("Operation.left", GramClassification.Variable)]
[GramClassify("Operation.right", GramClassification.Variable)]
[GramClassify("SelectQuery.field", GramClassification.Variable)]
[GramClassify("CountQuery.field", GramClassification.Variable)]
public static partial class MultiQueryLanguage
{
}

public static class MultiQueryExample
{
	public static object Select() => MultiQueryLanguage.ParseSelectQuery("select customer");
	public static object Count() => MultiQueryLanguage.ParseCountQuery("count customer");

	// Clear the string, put the caret between the quotes and press Ctrl+Space:
	// `select` and `choose` should be offered as DotGram literals.
	public static object CompleteSelect() => MultiQueryLanguage.ParseSelectQuery("select");

	// Put the caret after the space and press Ctrl+Space: `+` and `-` should be offered.
	public static object CompleteOperator() => MultiQueryLanguage.ParseOperation("customer ");
}
