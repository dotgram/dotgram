using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A capture may be named like a rule, and its name never stands for the rule's own name where a
/// recognizer goes (docs/syntax.md §3.5).
/// </summary>
/// <remarks>
/// <para>
/// <c>hour: hour</c> is the most natural capture there is: the right side of a capture is a
/// recognizer, and a capture's name is the grammar's own, with nothing reserved. The binder used to
/// look a bare name up among the rule's captures before its rules wherever it stood, so an operand
/// spelled like a capture of the same rule called nothing at all: the rule went unreached
/// (<c>GRAM4018</c>) and every input was refused with <c>Expected [hour]</c>.
/// </para>
/// <para>
/// A capture is a value. It is read where a value goes — a <c>=&gt;</c>, a <c>when</c>, a
/// <c>switch</c>, a <c>with state</c>, a recovery's factory — and nowhere a piece of grammar goes,
/// so the name there is the rule's whatever the rule captures.
/// </para>
/// </remarks>
public sealed class CaptureNamedAsRuleTests
{
	const string Hour   = "\nhour = ['0'..'9']+";
	const string Digits = Hour + "\nminute = ['0'..'9']+";

	[Theory]
	[InlineData("Start : @string = 'T' & hour: hour => @(hour)" + Hour, "T12", "12")]
	[InlineData("Start : @string = 'T' & Hour: Hour => @(Hour)\nHour = ['0'..'9']+", "T12", "12")]
	[InlineData("Start : @string = 'T' & hour: hour & ':' & minute: minute => @(hour + minute)" + Digits, "T1:2", "12")]

	// The capture is named like a rule written after it, which it is not a capture of.
	[InlineData("Start : @string = 'T' & minute: hour & ':' & m: minute => @(minute + m)" + Digits, "T1:2", "12")]

	// The capture's own rule read again in the same sequence.
	[InlineData("Start : @string = hour: hour & '-' & hour => @(hour)" + Hour, "1-2", "1")]
	[InlineData("Start : @string = hour & '-' & hour: hour => @(hour)" + Hour, "1-2", "2")]

	// Under a repetition the capture is every reading, and the operand is still the rule.
	[InlineData(
		"Start : @string = (hour: hour & ';')* => @(string.Join(\"+\", hour))\nhour : @int = ['0'..'9']+ => @int.Parse(parserText)",
		"1;23;",
		"1+23")]

	// In a group, and with a lookahead over the rule.
	[InlineData("Start : @string = (hour: hour) & ',' & (?=hour & minute) => @(hour)" + Digits, "1,2", "1")]

	// An element set naming a rule the capture is named like.
	[InlineData("Start : @string = digit: digit & [digit | 'x'] => @(digit)\ndigit = ['0'..'9']", "1x", "1")]
	public void Names_the_rule_where_a_recognizer_goes(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>A call's argument is a recognizer too, and a capture is not one (§4.2).</summary>
	[Theory]
	[InlineData("Start : @string = hour: Pair(hour) => @(hour)\nPair(item) = item & ',' & item" + Hour, "1,2", "1,2")]
	[InlineData("Start : @string = hour: hour & '/' & rest: Pair(hour) => @(hour + rest)\nPair(item) = item & ',' & item" + Hour, "1/2,3", "12,3")]

	// A capture of a parameterized rule named like a rule of the grammar, beside its parameter.
	[InlineData("Start : @string = p: Pair(minute) => @(p)\nPair(item) : @string = hour: item & ',' & hour => @(hour)" + Digits, "1,2", "1")]

	// A capture named like the recognizer parameter it captures: the parameter wins in the grammar,
	// as it always did, and the C# reads the capture.
	[InlineData("Start : @string = p: Twice(hour) => @(p)\nTwice(item) : @string = item: item & '-' & item => @(item)" + Hour, "1-2", "1")]
	public void Names_the_rule_in_a_call(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>
	/// <c>when A is B</c> asks about recognizers, so a capture named like the rule it asks about leaves
	/// the question about the rule: each parser keeps the alternative its own <c>Version</c> allows.
	/// </summary>
	[Fact]
	public void Names_the_rule_in_a_condition()
	{
		var result = Compile(
			"Start : @string = Version: Name & when Version is \"Old\" & '!' => @(Version)\n" +
			" | Version: Name & ';' => @(Version)\n" +
			"Version = \"Old\" | \"New\"\n" +
			"Name = ['a'..'z']+\n" +
			"parse Start with (Version = \"Old\") as Old\n" +
			"parse Start with (Version = \"New\") as New");

		EmittedCode.Quiet(result.Diagnostics);

		var made = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		Assert.True (EmittedCode.Match(made, "Grammar", "TryOld", "ab!").IsSuccess);
		Assert.False(EmittedCode.Match(made, "Grammar", "TryNew", "ab!").IsSuccess);
		Assert.True (EmittedCode.Match(made, "Grammar", "TryNew", "ab;").IsSuccess);
	}

	/// <summary>A guard reads values, and the same name there is the capture.</summary>
	[Fact]
	public void A_guard_still_reads_the_capture()
	{
		var grammar = "Start : @string = hour: hour & when @(hour.Length < 3) & ';' => @(hour)" + Hour;

		Assert.Equal("12", Parsed(grammar, "12;"));
		Assert.Null(Refused(grammar, "123;"));
	}

	/// <summary>Through a namespace, opened or named in full, and through the library's.</summary>
	[Theory]
	[InlineData(
		"using Lexical;\nStart : @string = hour: hour & ':' & minute: minute => @(hour + minute)\n" +
		"namespace Lexical\n{\n\ttrivia = none\n\thour = ['0'..'9']+\n\tminute = ['0'..'9']+\n}",
		"1:2", "12")]
	[InlineData(
		"Start : @string = hour: Lexical.hour & ':' & minute: Lexical.minute => @(hour + minute)\n" +
		"namespace Lexical\n{\n\ttrivia = none\n\thour = ['0'..'9']+\n\tminute = ['0'..'9']+\n}",
		"1:2", "12")]
	[InlineData("using Std;\nStart : @string = Integer: Integer & '+' & Integer => @(Integer.ToString())", "4+5", "4")]
	[InlineData("Start : @string = Integer: Std.Integer & '+' & Std.Integer => @(Integer.ToString())", "4+5", "4")]
	public void Names_the_rule_through_a_namespace(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>A grammar with trivia, whose lexemes the generator reads apart from its syntax.</summary>
	[Fact]
	public void Names_the_rule_in_a_grammar_with_trivia()
	{
		var grammar =
			"trivia = ' '*\n" +
			"Start : @string = hour: hour & ':' & minute: minute & eof => @(hour + minute)\n" +
			"hour = { ['0'..'9']+ }\n" +
			"minute = { ['0'..'9']+ }";

		Assert.Equal("12", Parsed(grammar, " 1 : 2 "));
	}

	/// <summary>
	/// A type the grammar generates holds a member per capture, and a member named like the rule it
	/// reads, the type of which is that rule's.
	/// </summary>
	/// <remarks>
	/// A capture written with a capital is a constructor parameter spelled exactly as its property, and
	/// the constructor assigned the parameter to itself: the member was always null, with a CS1717
	/// warning in the consumer's build.
	/// </remarks>
	[Fact]
	public void Builds_a_generated_type_whose_members_are_named_like_rules()
	{
		var grammar =
			"Start : @string = t: Time => @(t.Hour.Value + \":\" + t.Minute)\n" +
			"Time = 'T' & Hour: Hour & ':' & minute: minute\n" +
			"Hour = Value: Two\n" +
			"Two = ['0'..'9']{2}\n" +
			"minute = ['0'..'9']{2}";

		Assert.Equal("12:34", Parsed(grammar, "T12:34"));
	}

	/// <summary>
	/// A capture's name where a recognizer goes, with no rule of that name, names nothing. It used to
	/// be taken for the capture and lowered to a set of no characters, which compiled without a word
	/// and refused every input.
	/// </summary>
	[Theory]
	[InlineData("Start : @string = a: 'x' & a => @(a)")]
	[InlineData("Start : @string = a: 'x' & (a)* => @(a)")]
	[InlineData("Start : @string = a: 'x' & Pair(a) => @(a)\nPair(item) = item & item")]
	[InlineData("Start : @string = a: 'x' & when a is \"x\" & 'y' => @(a)")]
	public void A_capture_where_a_recognizer_goes_is_undefined(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = Assert.Single(result.Diagnostics, static one => one.Id == GrammarBinder.UndefinedName);

		Assert.Contains("'a' is a capture", found.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// Every place a value goes still reads the rule's captures and the names the parser supplies.
	/// </summary>
	[Theory]
	[InlineData("Start : @string = hour: hour => hour" + Hour, "1", "1")]
	[InlineData("Start : @string = hour: hour & switch @(hour.Length) { case 1: 'a' default: 'b' } => @(hour)" + Hour, "1a", "1")]
	public void A_value_still_reads_the_capture(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	static string? Parsed(string grammar, string input)
	{
		var result = Compile(grammar + "\nparse Start");

		EmittedCode.Quiet(result.Diagnostics);

		var (isSuccess, value, error, _) = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)),
			"Grammar",
			"TryParseStart",
			input);

		Assert.True(isSuccess, error);

		return (string?)value;
	}

	static string? Refused(string grammar, string input)
	{
		var result = Compile(grammar + "\nparse Start");

		EmittedCode.Quiet(result.Diagnostics);

		var (isSuccess, value, _, _) = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)),
			"Grammar",
			"TryParseStart",
			input);

		return isSuccess ? (string?)value : null;
	}

	static GramCompilation Compile(string grammar)
	{
		return GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });
	}
}
