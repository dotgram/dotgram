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
