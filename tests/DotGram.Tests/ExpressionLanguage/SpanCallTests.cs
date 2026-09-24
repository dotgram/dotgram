using System;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Calls that take a <c>ReadOnlySpan&lt;T&gt;</c>, which the language refused until the premise
/// behind the refusal was put to the runtime.
/// </summary>
/// <remarks>
/// <para>
/// Overload resolution dropped every method with a ref-struct parameter, on the ground that a
/// compiled expression tree cannot hold one. Asked directly, <c>System.Linq.Expressions</c> holds
/// a <c>ReadOnlySpan&lt;char&gt;</c> as a parameter, as a local and as a return, and compiles a
/// call taking one — on .NET, and on .NET Framework 4.7.2 through the netstandard2.0 build,
/// alike. The ground was not there, and with the guard gone every shape below binds.
/// </para>
/// <para>
/// What stays refused is a parameter taken by reference, there being nothing in this language
/// that spells <c>ref</c> at a call. That half of the guard is what the last test holds, so that
/// removing the rest of it does not go unnoticed.
/// </para>
/// </remarks>
public sealed class SpanCallTests
{
	delegate object Reading(int tag, ReadOnlySpan<char> value);

	const string Using = "using System; using DotGram.Tests.ExpressionLanguage; ";

	[Theory]
	// A span's own members read before any of this, and are here as the control: were they to
	// stop reading, the rows below would say nothing about the change that let them.
	[InlineData("(int tag, ReadOnlySpan<char> value) => value.Length", 4)]
	[InlineData("(int tag, ReadOnlySpan<char> value) => value.ToString()", "1.25")]
	// A static method taking a span, of characters and of bytes — the report itself.
	[InlineData("(int tag, ReadOnlySpan<char> value) => Spans.Length(value)", 4)]
	[InlineData("(int tag, ReadOnlySpan<char> value) => Spans.Bytes(System.Text.Encoding.UTF8.GetBytes(value.ToString()))", 4)]
	// An extension method whose receiver is the span, written as a call on it and as the static
	// call it is. Extensions are found through the text's own `using`s, which is why every text
	// here carries one.
	[InlineData("(int tag, ReadOnlySpan<char> value) => value.Doubled()", 8)]
	[InlineData("(int tag, ReadOnlySpan<char> value) => Spans.Doubled(value)", 8)]
	// The shape the report came from: a tag switched over, and the span read in one arm.
	[InlineData("(int tag, ReadOnlySpan<char> value) => tag switch { 25010 => Spans.Number(value), _ => 0.0 }", 1.25)]
	public void A_span_reaches_the_method_that_takes_it(string text, object expected)
	{
		var made = ExpressionParser.Compile<Reading>(Using + text, typeof(SpanCallTests).Assembly);

		Assert.Equal(expected, made(25010, "1.25".AsSpan()));
	}

	[Fact]
	public void A_parameter_taken_by_reference_is_still_no_candidate()
	{
		var text = Using + "(int tag, ReadOnlySpan<char> value) => Spans.Counted(tag)";

		Assert.Throws<InvalidOperationException>(
			() => ExpressionParser.Compile<Reading>(text, typeof(SpanCallTests).Assembly));
	}
}

/// <summary>
/// What the texts above call: each method here exists for one row, and says which.
/// </summary>
/// <remarks>
/// An extension method has to be declared in a static class that is not nested, which is why this
/// sits beside <see cref="SpanCallTests"/> rather than within it. What each method does with its
/// span does not matter — that it takes one is the whole of the question.
/// </remarks>
static class Spans
{
	/// <summary>A static method taking a span of characters.</summary>
	public static int Length(ReadOnlySpan<char> raw)
	{
		return raw.Length;
	}

	/// <summary>The same for a span of bytes, which an array reaches by a conversion.</summary>
	public static int Bytes(ReadOnlySpan<byte> raw)
	{
		return raw.Length;
	}

	/// <summary>What the switch's arm calls: a span read as a number.</summary>
	public static double Number(ReadOnlySpan<char> raw)
	{
		return double.Parse(raw.ToString(), System.Globalization.CultureInfo.InvariantCulture);
	}

	/// <summary>An extension method over a span, for the <c>value.Doubled()</c> form.</summary>
	public static int Doubled(this ReadOnlySpan<char> raw)
	{
		return raw.Length * 2;
	}

	/// <summary>The only overload by that name, and it takes its argument by reference.</summary>
	public static int Counted(ref int value)
	{
		return value;
	}
}
