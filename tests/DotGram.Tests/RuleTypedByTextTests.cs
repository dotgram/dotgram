using System;
using System.Collections;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule typed by another rule that gives text, an extent or an array, with more than one alternative.
/// </summary>
/// <remarks>
/// <para>
/// <c>Name : Quoted = Quoted | Bare</c> says that <c>Name</c> has the value of whichever operand matched.
/// Each alternative has to hand that operand back, and an operand is one that has a value: a rule that
/// builds one, and also a rule that builds nothing and captures nothing, whose value is the extent it
/// matched. The extent was not counted, so one alternative with such a rule handed back nothing, the
/// rewrite was dropped for the whole rule, and the generated file built <c>new string()</c> for it
/// (CS1729) or called a method with an argument too many (CS1501).
/// </para>
/// </remarks>
public sealed class RuleTypedByTextTests
{
	const string Quoted = "\nQuoted : @string = '[' & b: Bare & ']' => @(b)";
	const string Words  = "\nBare : @string = ['a'..'z']+";
	const string Plain  = "\nBare = ['a'..'z']+";

	[Theory]
	[InlineData("Start : Quoted = Quoted | Bare" + Quoted + Words, "[abc]", "abc")]
	[InlineData("Start : Quoted = Quoted | Bare" + Quoted + Words, "abc", "abc")]
	[InlineData("Start : Quoted = Bare | Quoted" + Quoted + Words, "[abc]", "abc")]
	[InlineData("Start : Quoted = Quoted | Word" + Quoted + Words + "\nWord : @string = ['a'..'z']+", "abc", "abc")]
	[InlineData("Start : Quoted = Quoted | Bare | '-' & Bare" + Quoted + Words, "-abc", "abc")]
	[InlineData("Start : Quoted = Quoted" + Quoted + Words, "[abc]", "abc")]

	// Neither operand builds a value, and each declares text: what an alternative says is the operand's, not its own extent.
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Words, "<abc>", "abc")]
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Words, "abc", "abc")]
	[InlineData("Start : Bare = '<' & Bare & '>'" + Words, "<abc>", "abc")]
	[InlineData("Start : Bare = Bare | Other" + Words + "\nOther : @string = ['0'..'9']+", "123", "123")]
	[InlineData("Start : Bare = '(' & Bare & ')' | Bare" + Words, "(abc)", "abc")]

	// Through a rule that is itself typed by a rule.
	[InlineData("Start : Mid = Mid | Bare\nMid : Bare = Bare | '!' & Bare" + Words, "!abc", "abc")]
	[InlineData("Start : Mid = Mid | Bare\nMid : Bare = Bare | '!' & Bare" + Words, "abc", "abc")]
	public void Gives_the_text_of_the_operand_that_matched(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>
	/// Operands that declare nothing say nothing of their value, so the rule's is case 4's: the extent it
	/// matched, however many alternatives it has.
	/// </summary>
	[Theory]
	[InlineData("Start : Bare = Bare | Other" + Plain + "\nOther = ['0'..'9']+", "123", "123")]
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Plain, "<abc>", "<abc>")]
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Plain, "abc", "abc")]
	[InlineData("Start : Mid = Mid\nMid : Bare = '!' & Bare | Bare" + Plain, "!abc", "!abc")]
	public void Gives_the_extent_where_no_operand_declares_a_value(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>
	/// Some alternatives have an operand that produces the value and some have none, so there is no one value
	/// to give: told on the rule, not left to the C# compiler.
	/// </summary>
	[Theory]
	[InlineData("Start : Quoted = Quoted | Bare" + Quoted + Plain)]
	[InlineData("Start : Quoted = Quoted | Word" + Quoted + Words + "\nWord = ['a'..'z']+")]
	[InlineData("Start : Quoted = Quoted | Bare & Bare" + Quoted + Words)]
	[InlineData("Start : Mid = Mid | Bare\nMid : Bare = Bare | '!' & Bare" + Plain)]
	[InlineData("Start : Span = Span | Digits\nSpan : @SourceSpan = ['a'..'z']+\nDigits = ['0'..'9']+")]

	// An operand that is not one on every route: several, repeated, optional, or written in two branches of a group.
	[InlineData("Start : Bare = Bare & ' ' & Bare" + Words)]
	[InlineData("Start : Bare = (Bare & ',')+" + Words)]
	[InlineData("Start : Bare = '<' & (Bare & ';')* recover ';' & '>'" + Words)]
	[InlineData("Start : Bare = Bare{2}" + Words)]
	[InlineData("Start : Bare = '<' & Bare? & '>'" + Words)]
	[InlineData("Start : Bare = '-'? & ('<' & Bare & '>' | Bare)" + Words)]
	[InlineData("Start : Span = Span & ' ' & Span\nSpan : @SourceSpan = ['a'..'z']+")]

	// A left-recursive rule says how it is built with a '=>' of its own.
	[InlineData("Start : Bare = Start & '+' & Bare | Bare" + Words)]
	[InlineData("Start : Bare = Start & '+' & Bare | Bare" + Plain)]
	[InlineData("Start : Span = Start & '+' & Span | Span\nSpan : @SourceSpan = ['a'..'z']+")]
	public void Is_told_where_the_alternatives_do_not_each_give_the_value(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltConstruction, found.Id);
		Assert.Contains("'Start' takes the type of the rule it names", found.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A capture of the rule's own says it builds something, so the declared type is not given up for the
	/// extent: it is refused as it always was, and the rule does not become a generated type.
	/// </summary>
	[Theory]
	[InlineData("Start : Bare = '<' & x: Bare & '>'" + Words)]
	[InlineData("Start : Bare = '<' & x: Bare & '>' | '[' & x: Bare & ']'" + Words)]
	[InlineData("Start : Bare = '<' & x: Bare & '>'" + Plain)]
	public void A_capture_without_a_construction_does_not_change_the_type(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltConstruction, found.Id);
	}

	/// <summary>A left-recursive rule that builds its value with a '=>' of its own is as it was.</summary>
	[Fact]
	public void A_left_recursive_rule_builds_with_its_own_construction()
	{
		Assert.Equal("b", Parsed("Start : Bare = Start & '+' & b: Bare => @(b) | b: Bare => @(b)" + Words, "a+b"));
	}

	[Theory]
	[InlineData("Start : Span = Span | Digits\nSpan : @SourceSpan = ['a'..'z']+\nDigits : @SourceSpan = ['0'..'9']+", "123", "3")]
	[InlineData("Start : Span = '<' & Span & '>' | Span\nSpan : @SourceSpan = ['a'..'z']+", "<abc>", "3")]
	public void Gives_the_extent_of_the_operand_that_matched(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Length(grammar, input));
	}

	[Theory]
	[InlineData("Start : Words = Words | Digits\nWords : @string[] = Word+\nDigits : @string[] = Digit+\nWord : @string = c: ['a'..'z'] => @(c)\nDigit : @string = c: ['0'..'9'] => @(c)", "ab", "a,b")]
	[InlineData("Start : Words = Words | Digits\nWords : @string[] = Word+\nDigits : @string[] = Digit+\nWord : @string = c: ['a'..'z'] => @(c)\nDigit : @string = c: ['0'..'9'] => @(c)", "12", "1,2")]
	[InlineData("Start : Words = '<' & Words & '>' | Words\nWords : @string[] = Word+\nWord : @string = c: ['a'..'z'] => @(c)", "<ab>", "a,b")]
	[InlineData("Start : Words = Words | Digits\nWords : @int = ['a'..'z']+ => @(1)\nDigits : @int = ['0'..'9']+ => @(2)", "12", "2")]
	public void Gives_the_value_of_the_operand_that_matched(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Show(Parsed(grammar, input, kind: true)));
	}

	static string Show(object? value)
	{
		return value is IEnumerable sequence and not string ? string.Join(",", sequence.Cast<object>()) : "" + value;
	}

	static string? Parsed(string grammar, string input)
	{
		return (string?)Parsed(grammar, input, kind: true);
	}

	static object? Parsed(string grammar, string input, bool kind)
	{
		var result = Compile(grammar + "\nparse Start");

		EmittedCode.Quiet(result.Diagnostics);

		var (isSuccess, value, error, _) = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)),
			"Grammar",
			"TryParseStart",
			input);

		Assert.True(isSuccess, error);

		return value;
	}

	static string Length(string grammar, string input)
	{
		var value = Parsed(grammar, input, kind: true)!;

		return "" + value.GetType().GetProperty("Length")!.GetValue(value);
	}

	static GramCompilation Compile(string grammar)
	{
		return GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });
	}
}
