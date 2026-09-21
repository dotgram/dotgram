using System;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix;
using DotGram.Handwritten.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class HandFixTests
{
	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void Fixtures_match_fields_and_explicit_message_semantics(string name, string wire)
	{
		Compare(wire, false);
		Assert.Equal(name, FixMessages.Build(wire, HandFixParser.Parse(wire)).GetType().Name);
	}

	[Theory]
	[MemberData(nameof(PrimitiveTests.KnownCodes), MemberType = typeof(PrimitiveTests))]
	public void Known_code_values_match(int tag, string value)
	{
		Compare(tag + "=" + value, true);
	}

	[Theory]
	[InlineData("")]
	[InlineData("|")]
	[InlineData(" || | ")]
	[InlineData("55=ABC|bad|38=2|0=X|55=END|tail")]
	[InlineData("55= | 38=bad | 55=END ")]
	[InlineData("01=X|2147483648=X|2147483647=Y")]
	[InlineData("95=0003 | 96=a|b | 55=END")]
	[InlineData("95=0000|96=")]
	[InlineData("95=3|55=X|96=abc|")]
	[InlineData("95=4|96=abc")]
	[InlineData("95=1|96=ab|55=X")]
	[InlineData("95=2147483648|96=x|")]
	[InlineData("95=2147483647|96=x|")]
	[InlineData("95=1|2147483648=x|")]
	[InlineData("95=-1|96=x|55=END")]
	[InlineData("96=abc|")]
	[InlineData("38=bad|54=Z|55=ABC")]
	[InlineData("ошибка|55=текст")]
	public void Boundaries_recovery_and_invalid_conversions_match(string wire)
	{
		Compare(wire, true);
		Compare(wire.Replace('|', '\u0001'), false);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(17)]
	[InlineData(4097)]
	public void Binary_payloads_and_custom_pairs_cross_buffer_boundaries(int length)
	{
		var payload = new string(Enumerable.Range(0, length).Select(i => " |=\0ÿ"[i % 5]).ToArray());
		Compare("55=A | 95=" + length + " | 96=" + payload + " | 55=Z", true);
		Compare("55=A\u000195=" + length + "\u000196=" + payload + "\u000155=Z", false);
		var options = new FixFieldOptions(new Dictionary<int, int> { [5000] = 5001 });
		Compare("5000=" + length + " | 5001=" + payload + " | 55=Z", true, options);
		Compare("5000=2|5002=ab|5001=X|55=Z", true, options);
	}

	[Fact]
	public void Long_spaces_are_linear_and_preserved_at_eof()
	{
		var spaces = new string(' ', 16384);
		Compare("58=A" + spaces + "B" + spaces + " | 55=END" + spaces, true);
	}

	[Fact]
	public void Deterministic_malformed_inputs_match_recovery_boundaries()
	{
		var random = new Random(817);
		const string alphabet = "01958=| ab\0";
		for (var i = 0; i < 200; i++)
		{
			var input = new string(Enumerable.Range(0, random.Next(1, 60)).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
			Compare(input, true);
		}
	}

	[Fact]
	public void Streaming_is_lazy_and_retention_is_per_field()
	{
		const string first = "95=3 | 96=a|b | ";
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(first + "55=END")) { Limit = first.Length + 1 };
		var source = HandFixParser.ParseLog(stream, bufferSize: 1, maxRetained: 32);
		Assert.Equal(0, stream.Position);
		using (var fields = source.GetEnumerator())
		{
			Assert.True(fields.MoveNext());
			Assert.Equal("a|b", Encoding.Latin1.GetString(Assert.IsType<FixField.RawData>(fields.Current).Value.Span));
			Assert.Equal(first.Length + 1, stream.Position);
		}
		Assert.True(stream.CanRead);

		using var many = new StringReader(string.Concat(Enumerable.Repeat("55=ABC|", 10000)));
		Assert.Equal(10000, HandFixParser.ParseLog(many, bufferSize: 3, maxRetained: 32).Count());
		using var large = new StringReader("55=" + new string('X', 100));
		Assert.Throws<IOException>(() => HandFixParser.ParseLog(large, bufferSize: 3, maxRetained: 16).ToArray());
	}

	[Fact]
	public void Concurrent_enumerators_do_not_share_binary_state()
	{
		using var one = new StringReader("95=3|96=a|b|55=A");
		using var two = new StringReader("95=1|96=X|55=B");
		using var a = HandFixParser.ParseLog(one, bufferSize: 1).GetEnumerator();
		using var b = HandFixParser.ParseLog(two, bufferSize: 1).GetEnumerator();
		Assert.True(a.MoveNext());
		Assert.True(b.MoveNext());
		Assert.True(a.MoveNext());
		Assert.True(b.MoveNext());
		Assert.Equal("A", Assert.IsType<FixField.Symbol>(a.Current).Value);
		Assert.Equal("B", Assert.IsType<FixField.Symbol>(b.Current).Value);
	}

	static void Compare(string input, bool log, FixFieldOptions? options = null)
	{
		var expected = log ? FixParser.Parse(input, (options ?? new FixFieldOptions()).With(FixFraming.Log)) : FixParser.Parse(input, options);
		Equal(expected, log ? HandFixParser.ParseLog(input, options) : HandFixParser.Parse(input, options));
		Equal(expected, log ? HandFixParser.ParseLog(input.AsSpan(), options) : HandFixParser.Parse(input.AsSpan(), options));
		using var reader = new StringReader(input);
		Equal(expected, log ? HandFixParser.ParseLog(reader, options, 1) : HandFixParser.Parse(reader, options, 1));
		Assert.Equal(-1, reader.Peek());

		var bytes = Encoding.Latin1.GetBytes(input);
		var expectedBytes = log ? FixParser.Parse(bytes, (options ?? new FixFieldOptions()).With(FixFraming.Log)) : FixParser.Parse(bytes, options);
		Equal(expectedBytes, log ? HandFixParser.ParseLog(bytes, options) : HandFixParser.Parse(bytes, options));
		using var stream = new ShortStream(bytes);
		Equal(expectedBytes, log ? HandFixParser.ParseLog(stream, options, 3) : HandFixParser.Parse(stream, options, 3));
		Assert.True(stream.CanRead);
	}

	static void Equal(IEnumerable<FixField> expected, IEnumerable<FixField> actual)
	{
		Assert.Equal(expected.Select(Describe), actual.Select(Describe));
	}

	static string Describe(FixField field)
	{
		var metadata = $"{field.GetType().Name}:{field.Tag}:{field.Position}:{field.ValuePosition}:{field.Length}:{field.IsValid}";
		if (field is FixField.Invalid invalid)
		{
			Assert.NotEmpty(invalid.Message);
			return metadata + ":" + invalid.RawText + ":" + Convert.ToHexString(invalid.RawBytes.Span);
		}
		return field.IsValid ? metadata + JsonSerializer.Serialize(field, field.GetType()) : metadata;
	}

	sealed class ShortStream(byte[] input) : MemoryStream(input)
	{
		public int Limit { get; init; } = int.MaxValue;

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (Position >= Limit)
				throw new InvalidOperationException("Read beyond the first field.");
			return base.Read(buffer, offset, Math.Min(count, 1));
		}
	}
}
