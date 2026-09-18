using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 8259, held to JSONTestSuite's parsing cases.
/// </summary>
/// <remarks>
/// <para>
/// <c>JsonTestSuite/</c> is nst/JSONTestSuite's <c>test_parsing/</c> as it stood at 1ef36fa, under its own
/// licence beside it. A file named <c>y_</c> is a text a parser must accept, <c>n_</c> one it must refuse,
/// and <c>i_</c> one the RFC leaves to the implementation — which has to be answered, whichever answer it is.
/// </para>
/// <para>
/// The files are bytes, and the parser reads characters: a file is decoded as strict UTF-8 first, as §8.1
/// says JSON between systems is, and one that does not decode is a text not accepted.
/// </para>
/// </remarks>
public sealed class Rfc8259Tests
{
	[Theory]
	[MemberData(nameof(Cases))]
	public void Every_case_of_the_suite_is_answered_as_it_says(string file)
	{
		var accepted = Read(file) is { } text && Both.TryJson(text, out _);

		switch (file[0])
		{
			case 'y':
				Assert.True(accepted, $"{file} must be read, and was refused.");
				break;

			case 'n':
				Assert.False(accepted, $"{file} must be refused, and was read.");
				break;
		}
	}

	/// <summary>What was read writes back as JSON that reads as the same value.</summary>
	[Theory]
	[MemberData(nameof(Cases))]
	public void What_is_read_writes_back_as_the_same_value(string file)
	{
		if (Read(file) is not { } text || !Both.TryJson(text, out var read))
			return;

		var written = read.ToString();

		Assert.Equal(written, Both.Json(written).ToString());
	}

	[Fact]
	public void A_value_comes_apart_as_the_RFC_divides_it()
	{
		var value = (JsonValue.Object)Both.Json("""
			{ "name": "a\u00e9\"b", "list": [1, -0.5e+3, true, false, null], "empty": {}, "name": 2 }
			""");

		Assert.Equal(["name", "list", "empty", "name"], value.Members.Select(member => member.Key).ToArray());
		Assert.Equal("a\u00e9\"b", ((JsonValue.String)value.Members[0].Value).Value);

		var list = (JsonValue.Array)value.Members[1].Value;

		Assert.Equal("-0.5e+3", ((JsonValue.Number)list.Items[1]).Text);
		Assert.Equal(-500.0, ((JsonValue.Number)list.Items[1]).ToDouble());
		Assert.Same(JsonValue.Boolean.True, list.Items[2]);
		Assert.Same(JsonValue.Null.Instance, list.Items[4]);
		Assert.Empty(((JsonValue.Object)value.Members[2].Value).Members);
	}

	/// <summary>A number keeps its text, which is how precision past a double survives (§6).</summary>
	[Fact]
	public void A_number_keeps_its_text()
	{
		var number = (JsonValue.Number)Both.Json("3.141592653589793238462643383279");

		Assert.Equal("3.141592653589793238462643383279", number.Text);
		Assert.False(number.TryToDecimal(out _));
		Assert.False(number.TryToInt64(out _));
	}

	/// <summary>
	/// A decimal is given only where it holds the number exactly: a digit lost to its precision or its
	/// range is a refusal, and a trailing zero after the point is not a lost digit.
	/// </summary>
	[Theory]
	[InlineData("1234567890123456789012345678",          true,  "1234567890123456789012345678")]
	[InlineData("12345678901234567890123456789",         true,  "12345678901234567890123456789")]
	[InlineData("1.2345678901234567890123456789",        true,  "1.2345678901234567890123456789")]
	[InlineData("9.9999999999999999999999999999",        false, null)]
	[InlineData("12345678901234567890123456789012345",   false, null)]
	[InlineData("0.12345678901234567890123456789012345", false, null)]
	[InlineData("79228162514264337593543950335",         true,  "79228162514264337593543950335")]
	[InlineData("-79228162514264337593543950335",        true,  "-79228162514264337593543950335")]
	[InlineData("79228162514264337593543950336",         false, null)]
	[InlineData("1e28",                                  true,  "10000000000000000000000000000")]
	[InlineData("1e29",                                  false, null)]
	[InlineData("1E+2",                                  true,  "100")]
	[InlineData("1e-28",                                 true,  "0.0000000000000000000000000001")]
	[InlineData("1e-30",                                 false, null)]
	[InlineData("1.5000000000000000000000000000000000",  true,  "1.5")]
	[InlineData("1.500",                                 true,  "1.5")]
	[InlineData("1500e-3",                               true,  "1.5")]
	[InlineData("-0.5e+3",                               true,  "-500")]
	[InlineData("-1.25",                                 true,  "-1.25")]
	[InlineData("-0",                                    true,  "0")]
	[InlineData("0e-99999999999999999999",               true,  "0")]
	[InlineData("1e-99999999999999999999",               false, null)]
	public void A_decimal_is_given_only_where_it_is_exact(string text, bool exact, string? expected)
	{
		var number = (JsonValue.Number)Both.Json(text);

		Assert.Equal(exact, number.TryToDecimal(out var value));

		if (exact)
			Assert.Equal(decimal.Parse(expected!, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture), value);
		else
			Assert.Equal(0m, value);
	}

	/// <summary>An escaped lone surrogate is kept and written back escaped (§8.2).</summary>
	[Fact]
	public void A_lone_surrogate_survives()
	{
		var text = (JsonValue.String)Both.Json("\"\\uDEAD\"");

		Assert.Equal("\uDEAD", text.Value);
		Assert.Equal("\"\\udead\"", text.ToString());
	}

	/// <summary>Nesting as deep as the suite's deepest refusal is read on as many stacks as it takes.</summary>
	[Fact]
	public void Deep_nesting_is_read()
	{
		var depth = 100_000;
		var text  = new string('[', depth) + new string(']', depth);
		var value = Both.Json(text);

		for (var level = 1; level < depth; level++)
			value = ((JsonValue.Array)value).Items[0];

		Assert.Empty(((JsonValue.Array)value).Items);
	}

	/// <summary>A value is equal to another read from the same text, as a value and not as an object.</summary>
	[Fact]
	public void Values_are_equal_by_what_they_hold()
	{
		const string Text = """{ "a": [1, { "b": null }, "c"], "a": 2 }""";

		Assert.Equal(Both.Json(Text), Both.Json(Text));
		Assert.Equal(Both.Json(Text).GetHashCode(), Both.Json(Text).GetHashCode());

		Assert.NotEqual(Both.Json("""{ "a": [1] }"""), Both.Json("""{ "a": [2] }"""));
		Assert.NotEqual(Both.Json("""{ "a": 1, "b": 2 }"""), Both.Json("""{ "b": 2, "a": 1 }"""));
		Assert.NotEqual(Both.Json("""{ "a": 1 }"""), Both.Json("""{ "a": 1, "a": 1 }"""));
		Assert.NotEqual(Both.Json("[1.0]"), Both.Json("[1]"));
	}

	public static TheoryData<string> Cases =>
		[.. Directory.GetFiles(Suite, "*.json").Select(path => Path.GetFileName(path)!).OrderBy(one => one, StringComparer.Ordinal)];

	static string? Read(string file)
	{
		try
		{
			return Strict.GetString(File.ReadAllBytes(Path.Combine(Suite, file)));
		}
		catch (DecoderFallbackException)
		{
			return null;
		}
	}

	static readonly UTF8Encoding Strict = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	static string Suite =>
		Path.Combine(Path.GetDirectoryName(ThisFile)!, "JsonTestSuite");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
