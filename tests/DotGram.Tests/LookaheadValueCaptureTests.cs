using System.Collections.Generic;
using System.IO;
using System.Text;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// §3.4: <c>?=X</c> produces <c>X</c>'s value without moving the input, and that value can be
/// captured. A captured lookahead at a rule that builds a value holds the value; at a pattern, or
/// at a rule whose value is its text, it holds the text the lookahead saw.
/// </summary>
public sealed class LookaheadValueCaptureTests
{
	public static IEnumerable<object[]> Readings()
	{
		return EmptyRunCaptureTests.Readings();
	}

	[Theory]
	[MemberData(nameof(Readings))]
	public void A_captured_lookahead_at_a_valued_rule_holds_the_rules_value(CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = EmptyRunCaptureTests.Compile(
			"""
			R : @string = v: ?=W & W => @(v)
			W : @string = 'b' => @("built")
			parse R
			""", carrier, direct, buffered);

		Same(assembly, "TryParseR", "b", "built", buffered);
	}

	/// <summary>
	/// The value outlives what the parse gives up around it: an alternative abandoned after its
	/// lookahead took a value leaves nothing behind for the one that stands, and under a repetition
	/// each turn's lookahead is one element of the sequence the capture collects.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_captured_lookahead_value_survives_backtracking_and_collects_under_a_repetition(
		CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = EmptyRunCaptureTests.Compile(
			"""
			R : @string = v: ?=W & W & 'x' => @(v) | v: ?=W & W & 'y' => @(v + "!")
			L : @string = (v: ?=W & W & ','?)* => @(string.Join("|", v))
			W : @string = 'b' => @("built")
			parse R
			parse L
			""", carrier, direct, buffered);

		Same(assembly, "TryParseR", "bx", "built", buffered);
		Same(assembly, "TryParseR", "by", "built!", buffered);
		Same(assembly, "TryParseL", "b,b,b", "built|built|built", buffered);
		Same(assembly, "TryParseL", "", "", buffered);
	}

	/// <summary>A value of another type than the text: what made the generated code not compile.</summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_captured_lookahead_at_a_rule_of_another_type_holds_that_type(CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = EmptyRunCaptureTests.Compile(
			"""
			R : @string = v: ?=N & N & '+' & w: N => @((v + w).ToString())
			N : @int = d: ['0'..'9']+ => @(int.Parse(Text(d)))
			parse R
			""", carrier, direct, buffered, Host);

		Same(assembly, "TryParseR", "12+30", "42", buffered);
	}

	/// <summary>
	/// The example of §3.6, as written there: a guard handed the value a lookahead produced decides
	/// whether the rule begins here, before anything is consumed.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void The_small_number_of_the_guard_section_reads_as_documented(CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = EmptyRunCaptureTests.Compile(
			"""
			Value : @int = small: SmallNumber => small | other: Large => other
			SmallNumber : @int = n: ?=Number & when @IsSmall(n) & value: Number => value
			Number : @int = d: ['0'..'9']+ => @(int.Parse(Text(d)))
			Large : @int = ['0'..'9']+ => @(-1)
			parse Value
			parse SmallNumber
			""", carrier, direct, buffered, Host + " static bool IsSmall(int n) { return n < 10; }");

		Same(assembly, "TryParseValue", "7", 7, buffered);
		Same(assembly, "TryParseValue", "42", -1, buffered);
		Same(assembly, "TryParseSmallNumber", "9", 9, buffered);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseSmallNumber", "10").IsSuccess);
	}

	/// <summary>
	/// What stays text: a lookahead over a pattern, and over a rule whose value is the text it
	/// matched, and a negative lookahead, which produces nothing (§3.4) and so is empty text.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readings))]
	public void A_captured_lookahead_at_text_holds_the_text(CarrierKind carrier, bool direct, bool buffered)
	{
		var assembly = EmptyRunCaptureTests.Compile(
			"""
			R : @string = v: ?=('a' & 'b') & T & n: ?!'x' & 'c' => @(Text(v) + "/" + Text(n))
			T = 'a' & 'b'
			S : @string = v: ?=T & T => @(Text(v))
			parse R
			parse S
			""", carrier, direct, buffered, Host);

		Same(assembly, "TryParseR", "abc", "ab/", buffered);
		Same(assembly, "TryParseS", "ab", "ab", buffered);
	}

	const string Host =
		EmptyRunCaptureTests.Host + " " +
		"static string Text(string text) { return text ?? \"null\"; } " +
		"static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); } " +
		"static string Text(global::System.ReadOnlySpan<byte> text) { return global::System.Text.Encoding.ASCII.GetString(text.ToArray()); }";

	static void Same(System.Reflection.Assembly assembly, string method, string input, object expected, bool buffered)
	{
		var match = EmittedCode.Match(assembly, "Grammar", method, input);

		Assert.True(match.IsSuccess, $"{method}(\"{input}\"): {match.Error}");
		Assert.Equal(expected, match.Value);

		if (!buffered)
			return;

		var type = assembly.GetType("Grammar")!;

		foreach (var (over, argument) in new (System.Type, object)[]
		{
			(typeof(TextReader), new StringReader(input)),
			(typeof(Stream), new MemoryStream(Encoding.ASCII.GetBytes(input))),
			(typeof(byte[]), Encoding.ASCII.GetBytes(input)),
		})
		{
			var found = over == typeof(byte[])
				? type.GetMethod(method, [over])!.Invoke(null, [argument])!
				: type.GetMethod(method, [over, typeof(int?), typeof(int?)])!.Invoke(null, [argument, null, null])!;

			Assert.True(
				(bool)found.GetType().GetProperty("IsSuccess")!.GetValue(found)!,
				$"{method} over {over.Name}(\"{input}\"): {found.GetType().GetProperty("Error")!.GetValue(found)}");
			Assert.Equal(expected, found.GetType().GetProperty("Value")!.GetValue(found));
		}
	}
}
