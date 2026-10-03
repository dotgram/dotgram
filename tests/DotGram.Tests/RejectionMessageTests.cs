using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a rejected element of a recovering repetition is told about where it went wrong
/// (docs/syntax.md §8.2), and what a refusal says around it: the same place in every rendering,
/// and words a person can act on.
/// </summary>
public sealed class RejectionMessageTests
{
	/// <summary>A price list typed by hand: the third line has a comma, the fourth a letter O for a zero.</summary>
	const string Prices =
		"Espresso       2.40\n" +
		"Cappuccino     3.10\n" +
		"Flat white     3,20\n" +
		"Croissant      2.1O\n" +
		"Muffin         2.80\n";

	const string PriceList = """
		PriceList : @string[] = Entry* recover eol
		    => @(Told(parserLine, parserColumn, parserFailureLine, parserFailureColumn, parserFailurePosition, parserExpected, parserMessage))
		Entry  : @string = item: Item & amount: Amount & ' '* & eol => @(Text(amount))
		Item   = [^ '0'..'9' | '\r' | '\n']+
		Amount = ['0'..'9']+ & '.' & ['0'..'9']{2}

		""";

	const string Helpers = """
		static string Text(string text) { return text; }
		static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); }
		static string Told(int line, int column, int failureLine, int failureColumn, long failurePosition, string expected, string message)
		{
			return line + ":" + column + " " + failureLine + ":" + failureColumn + "@" + failurePosition + " " + expected + " | " + message;
		}
		""";

	public static TheoryData<string, bool, CarrierKind> Renderings => new()
	{
		{ "engine", false, CarrierKind.Tape },
		{ "tape", true, CarrierKind.Tape },
		{ "immediate", true, CarrierKind.Immediate },
	};

	/// <summary>
	/// Where the record began, where reading it stopped, and what would have fit there — the comma
	/// sixteen characters into the third line, the O eighteen into the fourth — in every rendering.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_rejected_record_is_told_where_reading_it_stopped(string rendering, bool direct, CarrierKind carrier)
	{
		var told = Recovered(Compiled(PriceList + "parse PriceList as ParseStart\n", direct, carrier));

		Assert.True(told.Length == 5, rendering + ": " + string.Join(" / ", told));
		Assert.Equal("2.40", told[0]);
		Assert.Equal("3.10", told[1]);
		Assert.Equal("3:1 3:17@56 Amount | Expected Amount at 3:17.", told[2]);
		Assert.Equal("4:1 4:19@78 Amount | Expected Amount at 4:19.", told[3]);
		Assert.Equal("2.80", told[4]);
	}

	/// <summary>
	/// The same from a <c>TextReader</c>, through a window smaller than a line, which moves under
	/// the record while its end is looked for.
	/// </summary>
	[Theory]
	[InlineData(8)]
	[InlineData(4096)]
	public void A_streamed_record_is_told_the_same_place(int window)
	{
		var host = Compiled(PriceList + "parse PriceList as ReadStart stream yield\n", direct: true, CarrierKind.Auto);
		var read = host.GetType("Grammar")!.GetMethods()
			.Single(static one => one.Name == "ReadStart" && one.GetParameters()[0].ParameterType == typeof(TextReader));
		var told = ((IEnumerable)read.Invoke(null, [new StringReader(Prices), window, null])!)
			.Cast<object>().Select(static one => one.ToString()!).ToArray();

		Assert.True(told.Length == 5, string.Join(" / ", told));
		Assert.Equal("3:1 3:17@56 '.' | Expected '.' at 3:17.", told[2]);
		Assert.Equal("4:1 4:19@78 ['0'..'9'] | Expected ['0'..'9'] at 4:19.", told[3]);
	}

	/// <summary>
	/// A rule whose every alternative hands another rule's value on is a value, and the element of
	/// a recovering repetition as written (§6's own spelling), not a choice of its sources.
	/// </summary>
	[Fact]
	public void A_rule_that_forwards_per_alternative_is_an_element_that_recovers()
	{
		const string Lines = """
			Sheet : @string[] = Line* recover eol => @(Told(parserLine, parserColumn, parserFailureLine, parserFailureColumn, parserFailurePosition, parserExpected, parserMessage)) & eof
			Item   : @string = d: ['0'..'9']+ & eol => @(Text(d))
			Remark : @string = '#' & t: [^ '\n']* & eol => @(Text(t))
			parse Sheet as ParseStart

			""";

		var perAlternative = Compiled("Line : @string = v: Item => @(v) | v: Remark => @(v)\n" + Lines, direct: true, CarrierKind.Auto);
		var grouped        = Compiled("Line : @string = (v: Item | v: Remark) => @(v)\n" + Lines, direct: true, CarrierKind.Auto);

		const string Input = "12\n# note\nx2\n4\n";

		Assert.Equal(Recovered(grouped, Input), Recovered(perAlternative, Input));
		Assert.Equal("3:1 3:1@10", string.Join(" ", Recovered(perAlternative, Input)[2].Split(' ').Take(2)));
	}

	/// <summary><c>when</c> begins a guard; a capture called that is told to take another name, once.</summary>
	[Fact]
	public void A_capture_called_when_is_told_to_rename()
	{
		var result = GramCompiler.Compile("Start : @string = when: 'a' & b: 'b' => @(b)\nparse Start\n", new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance,
		});

		var error = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal("GRAM2003", error.Id);
		Assert.Contains("Rename the capture", error.Message, StringComparison.Ordinal);
	}

	/// <summary>Input left after a whole value is what the end of the input was expected at.</summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void Input_left_over_is_where_the_end_was_expected(string rendering, bool direct, CarrierKind carrier)
	{
		var host  = Compiled("Start : @string = '{' & t: ['a'..'z']* & '}' => @(Text(t))\nparse Start as ParseStart\n", direct, carrier);
		var match = EmittedCode.Match(host, "Grammar", "TryParseStart", "{ab}x");

		Assert.False(match.IsSuccess, rendering);
		Assert.Equal(4, match.Position);
		Assert.Equal("Expected end of input.", match.Error);
	}

	/// <summary>
	/// A call whose own first set the choice around it holds is said once, in the choice's words:
	/// <c>Digit</c> is not listed beside the set that holds its digits.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_set_the_choice_holds_is_not_said_twice(string rendering, bool direct, CarrierKind carrier)
	{
		const string Grammar = """
			trivia = ' '*
			Digit  = ['0'..'9']
			Number : @int = d: Digit+ => @(int.Parse(Text(d)))
			Expr   : @int = '(' & e: Expr & ')' => @(e) | '-' & e: Expr => @(-e) | n: Number => @(n)
			parse Expr as ParseStart

			""";

		var host = Compiled(Grammar, direct, carrier);

		foreach (var input in new[] { "x", "- x", "(x" })
		{
			var match = EmittedCode.Match(host, "Grammar", "TryParseStart", input);

			Assert.False(match.IsSuccess, rendering + ": " + input);
			Assert.Equal("Expected ['(' | '-' | '0'..'9'].", match.Error);
		}
	}

	/// <summary>What a method without <c>Try</c> throws ends where a sentence ends, with the place inside it.</summary>
	[Fact]
	public void A_refusal_thrown_says_where_before_its_full_stop()
	{
		var host  = Compiled("Start : @string = 'a' & t: 'b' => @(Text(t))\nparse Start as ParseStart\n", direct: true, CarrierKind.Auto);
		var parse = host.GetType("Grammar")!.GetMethod("ParseStart", [typeof(string)])!;

		var thrown = Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, ["ax"]));

		Assert.Equal("Expected 'b' at 1.", Assert.IsType<FormatException>(thrown.InnerException).Message);
	}

	static Assembly Compiled(string grammar, bool direct, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Helpers);
	}

	static string[] Recovered(Assembly host, string input = Prices)
	{
		var match = EmittedCode.Match(host, "Grammar", "TryParseStart", input);

		Assert.True(match.IsSuccess, match.Error);

		return [.. ((IEnumerable)match.Value!).Cast<object>().Select(static one => one?.ToString() ?? "<null>")];
	}
}
