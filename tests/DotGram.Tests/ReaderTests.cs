using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The reader (<c>Machine.Reader.cs</c>) against the rendering it is replacing.
/// </summary>
/// <remarks>
/// <para>
/// Two renderings of one grammar have to read the same language, and the only way to
/// believe that is to compile both and ask them the same questions. So every case here is
/// the same grammar twice — once written the way it has been written until now, once by
/// the reader — and the answers are compared rather than either being checked against what
/// the test author expected. <see cref="The_reader_writes_no_jumps"/> is the one case that
/// looks at the code instead, because a reader that quietly declined would pass every
/// other test in this file.
/// </para>
/// <para>
/// What is here is what the reader can write so far: recognition over kinds, with no value
/// kept. It grows one construct at a time and this file grows with it.
/// </para>
/// </remarks>
public sealed class ReaderTests
{
	/// <summary>A split grammar, which is what the reader is for: over kinds a rule's answer stands (§4).</summary>
	const string Lexical =
		"trivia = { ' '* }\n" +
		"namespace Lexical\n" +
		"{\n" +
		"\ttrivia = none\n" +
		"\tName = ['a'..'z']+\n" +
		"\tDigits = ['0'..'9']+\n" +
		"}\n";

	[Theory]
	[InlineData("a b")]
	[InlineData("a")]
	[InlineData("")]
	[InlineData("a b c")]
	public void A_sequence(string input) =>
		Both("Start = Lexical.Name & Lexical.Name & eof", input);

	[Theory]
	[InlineData("a")]
	[InlineData("1")]
	[InlineData("+")]
	public void A_choice_the_first_token_divides(string input) =>
		Both("Start = (Lexical.Name | Lexical.Digits) & eof", input);

	[Theory]
	[InlineData("")]
	[InlineData("a a a")]
	[InlineData("a 1")]
	public void A_repetition(string input) =>
		Both("Start = Lexical.Name* & eof", input);

	[Theory]
	[InlineData("a = 1")]
	[InlineData("a = 1 , b = 2")]
	[InlineData("a =")]
	public void A_rule_calling_a_rule(string input) =>
		Both("Start = Pair & (',' & Pair)* & eof\nPair = Lexical.Name & '=' & Lexical.Digits", input);

	[Theory]
	[InlineData("a 1")]
	[InlineData("a")]
	[InlineData("1")]
	public void A_choice_of_alternatives_that_begin_alike(string input) =>
		Both("Start = (Lexical.Name & Lexical.Digits | Lexical.Name) & eof", input);

	[Theory]
	[InlineData("a")]
	[InlineData("1")]
	public void A_lookahead(string input) =>
		Both("Start = ?!Lexical.Digits & Lexical.Name & eof", input);

	/// <summary>A value built from a captured rule and captured text.</summary>
	/// <remarks>
	/// A capture gathered across the turns of a repetition lives on the side stack until
	/// the rule ends, and the reader does not keep one yet, so there is none here. Neither
	/// do the alternatives of <c>Value</c> begin alike: a head they shared would be read
	/// and captured before the choice, and an alternative written as a method of its own
	/// cannot see a local of the method that called it.
	/// </remarks>
	[Theory]
	[InlineData("a b 1")]
	[InlineData("22")]
	[InlineData("a b")]
	public void A_value_built_from_captures(string input) =>
		Same(Valued, input);

	/// <summary>Two rules that keep a value, one built out of the other.</summary>
	const string Valued =
		"Start : @string = only: Value & eof => @(only)\n" +
		"Value : @string = a: Lexical.Name & b: Value => @(a + b) | c: Lexical.Digits => @(c)";

	/// <summary>A value the alternative that failed had already begun to build.</summary>
	/// <remarks>
	/// <c>Pair</c> writes its record and then <c>Item</c>'s first alternative fails on what
	/// follows, so the second one builds over a tape that is not empty. Nothing of the
	/// abandoned record may reach the answer.
	/// </remarks>
	[Theory]
	[InlineData("a 1")]
	[InlineData("a")]
	[InlineData("1")]
	public void A_value_left_behind_by_an_alternative_that_failed(string input) =>
		Same(
			"Start : @string = only: Item & eof => @(only)" + Line +
			"Item : @string = x: Pair & Lexical.Digits => @(x) | y: Lexical.Name => @(y)" + Line +
			"Pair : @string = n: Lexical.Name => @(n)",
			input);

	/// <summary>One line of a grammar written as one string.</summary>
	const string Line = "\n";

	/// <summary>That the reader wrote the one that keeps a value too.</summary>
	[Fact]
	public void The_reader_writes_no_jumps_where_it_keeps_a_value()
	{
		var written = Written(Lexical + Valued + "\nparse Start", reader: true);

		Assert.Contains("ways.Begin", Reading(written, "Read_Value"), StringComparison.Ordinal);
		Assert.DoesNotContain("goto", Reading(written, "Read_Value"), StringComparison.Ordinal);
	}

	/// <summary>
	/// That the reader wrote it, and not the rendering it stands beside.
	/// </summary>
	/// <remarks>
	/// Every other test here passes if the reader declines and the old rendering answers
	/// for it, which is exactly the failure that would go unnoticed. A jump is the thing
	/// the reader exists not to write, so counting them says which one ran.
	/// </remarks>
	[Fact]
	public void The_reader_writes_no_jumps()
	{
		var grammar =
			"Start = Pair & (',' & Pair)* & eof\n" +
			"Pair = Lexical.Name & '=' & Value\n" +
			"Value = Lexical.Digits & Lexical.Name | Lexical.Digits | Lexical.Name";
		var written = Written(Lexical + grammar + "\nparse Start", reader: true);
		var before  = Written(Lexical + grammar + "\nparse Start", reader: false);

		// The lexer is one automaton either way and jumps between its states; what differs
		// is the reader, so the reader is what is looked at.
		Assert.Contains("goto", Reading(before, "Recognize_Start_Read"), StringComparison.Ordinal);
		Assert.DoesNotContain("goto", Reading(written, "Read_Start"), StringComparison.Ordinal);
		Assert.DoesNotContain("goto", Reading(written, "Read_Value"), StringComparison.Ordinal);
	}

	/// <summary>One method out of an emitted file, by its braces.</summary>
	static string Reading(string source, string name)
	{
		var at = source.IndexOf("static int " + name + "(", StringComparison.Ordinal);

		Assert.True(at >= 0, name + " is not in the emitted file.");

		var depth = 0;

		for (var i = at; i < source.Length; i++)
		{
			depth += source[i] == '{' ? 1 : source[i] == '}' ? -1 : 0;

			if (depth == 0 && source[i] == '}')
				return source.Substring(at, i - at + 1);
		}

		return source.Substring(at);
	}

	/// <summary>
	/// The same grammar both ways, and the same value out of both.
	/// </summary>
	/// <remarks>
	/// What two renderings agreeing actually means: not that they said yes to the same
	/// inputs, but that they built the same thing out of them.
	/// </remarks>
	static void Same(string grammar, string input)
	{
		var whole = Lexical + grammar + "\nparse Start";

		Assert.Equal(Built(whole, input, reader: false), Built(whole, input, reader: true));
	}

	static string Built(string grammar, string input, bool reader)
	{
		var match = EmittedCode.Match(
			EmittedCode.Compile(Written(grammar, reader)), "Grammar", "TryParseStart", input);

		return match.IsSuccess ? match.Value?.ToString() ?? "<null>" : "<refused>";
	}

	static void Both(string grammar, string input)
	{
		var whole = Lexical + grammar + "\nparse Start";

		Assert.Equal(Reads(whole, input, reader: false), Reads(whole, input, reader: true));
	}

	static bool Reads(string grammar, string input, bool reader) =>
		EmittedCode
			.Match(EmittedCode.Compile(Written(grammar, reader)), "Grammar", "TryParseStart", input)
			.IsSuccess;

	static string Written(string grammar, bool reader)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName     = "Grammar",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Lexical       = true,
				Reader        = reader,
			});

		Assert.Empty(result.Diagnostics.Where(one => one.Severity == GramSeverity.Error));

		return result.Sources[0].Text;
	}
}
