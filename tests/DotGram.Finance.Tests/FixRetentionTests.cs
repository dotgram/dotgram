using System;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Handwritten.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// <c>maxRetained</c> bounds one field, from its tag through the separator that ends it: a field of
/// exactly that length is read, one element more is an <see cref="IOException"/> that names the limit.
/// The generated and the handwritten parser draw the line in the same place.
/// </summary>
public sealed class FixRetentionTests
{
	const int Limit = 64;

	public static TheoryData<string, bool, int> Readers()
	{
		var data = new TheoryData<string, bool, int>();

		foreach (var parser in new[] { "generated", "handwritten" })
		foreach (var bytes in new[] { false, true })
		foreach (var bufferSize in new[] { 1, 4096 })
			data.Add(parser, bytes, bufferSize);

		return data;
	}

	[Theory]
	[MemberData(nameof(Readers))]
	public void A_field_at_the_limit_is_read(string parser, bool bytes, int bufferSize)
	{
		var fields = Read(parser, bytes, bufferSize, Field(Limit) + Field(10));

		Assert.Equal(2, fields.Length);
		Assert.All(fields, field => Assert.True(field.IsValid));
	}

	[Theory]
	[MemberData(nameof(Readers))]
	public void A_field_past_the_limit_names_it(string parser, bool bytes, int bufferSize)
	{
		var error = Assert.Throws<IOException>(() => Read(parser, bytes, bufferSize, Field(Limit + 1)));

		Assert.Contains(Limit.ToString(), error.Message);
		Assert.Contains("maxRetained", error.Message);
	}

	// A text field, tag and separator included, of exactly the given length.
	static string Field(int length)
	{
		return "58=" + new string('x', length - 4) + (char)1;
	}

	static FixField[] Read(string parser, bool bytes, int bufferSize, string text)
	{
		if (bytes)
		{
			using var stream = new MemoryStream(Encoding.Latin1.GetBytes(text));

			return parser == "generated"
				? FixParser.Parse(stream, null, bufferSize, Limit).ToArray()
				: HandFixParser.Parse(stream, null, bufferSize, Limit).ToArray();
		}

		using var reader = new StringReader(text);

		return parser == "generated"
			? FixParser.Parse(reader, null, bufferSize, Limit).ToArray()
			: HandFixParser.Parse(reader, null, bufferSize, Limit).ToArray();
	}
}
