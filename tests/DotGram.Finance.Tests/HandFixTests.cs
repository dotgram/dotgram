using System;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;
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
		Assert.Equal(name, FixParser.BuildMessage(wire, HandFixParser.Parse(wire)).GetType().Name);
	}

	[Theory]
	[MemberData(nameof(FixFixtures.KnownCodes), MemberType = typeof(FixFixtures))]
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
		var options = new Fix44Context { LengthDataPairs = new Dictionary<int, int> { [5000] = 5001 } };
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
	public void Framing_comes_from_the_context_and_recovery_resumes_after_the_next_separator()
	{
		// The log separator is ' '* & '|' & ' '*. Recovery skips to its next match, which is the
		// run of spaces before the pipe, not the long run after "broken" that no pipe follows.
		var spaces = new string(' ', 4096);
		var input  = "broken" + spaces + "x | 55=END";

		var fields = HandFixParser.Parse(input, Fix44Context.WithLogFraming);

		Assert.Equal(2, fields.Length);
		var invalid = Assert.IsType<FixField.Invalid>(fields[0]);
		Assert.Equal("broken" + spaces + "x", invalid.RawText);
		Assert.Equal(0, invalid.Position);
		Assert.Equal("END", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, fields[1]).Value);

		Compare(input, true);

		// Every buffered form, of either parser, reads what the whole input reads, whatever the buffer.
		var bytes      = Encoding.Latin1.GetBytes(input);
		var fromBytes  = HandFixParser.Parse(bytes, Fix44Context.WithLogFraming);
		foreach (var bufferSize in new[] { 1, 3, 4095, 4096, 4097 })
		{
			var context = Fix44Context.WithLogFraming with { BufferSize = bufferSize };
			Equal(fields,    HandFixParser.Parse(new StringReader(input), context));
			Equal(fields,    FixParser.ReadFields(new StringReader(input), context));
			Equal(fromBytes, HandFixParser.Parse(new MemoryStream(bytes, false), context));
			Equal(fromBytes, FixParser.ReadFields(new MemoryStream(bytes, false), context));
		}

		// A retention bound too small for the rejected element and the separator it resumes after
		// is refused by both, and the smallest one that holds them is enough for both.
		var tight = Fix44Context.WithLogFraming with { BufferSize = 3, MaxRetained = input.Length - "55=END".Length };
		Assert.Throws<IOException>(() => HandFixParser.Parse(new StringReader(input), tight).ToArray());
		Assert.Throws<IOException>(() => FixParser.ReadFields(new StringReader(input), tight).ToArray());
		Assert.Throws<IOException>(() => HandFixParser.Parse(new MemoryStream(bytes, false), tight).ToArray());
		Assert.Throws<IOException>(() => FixParser.ReadFields(new MemoryStream(bytes, false), tight).ToArray());
		var enough = tight with { MaxRetained = tight.MaxRetained + 1 };
		Equal(fields,    HandFixParser.Parse(new StringReader(input), enough));
		Equal(fields,    FixParser.ReadFields(new StringReader(input), enough));
		Equal(fromBytes, HandFixParser.Parse(new MemoryStream(bytes, false), enough));
		Equal(fromBytes, FixParser.ReadFields(new MemoryStream(bytes, false), enough));
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
		var source = HandFixParser.ParseLog(stream, FixFixtures.Reading(true, 1, 32));
		Assert.Equal(0, stream.Position);
		using (var fields = source.GetEnumerator())
		{
			Assert.True(fields.MoveNext());
			Assert.Equal(3, FixFixtures.Typed<FixField.Integer>(FixTag.RawDataLength, fields.Current).Value);
			Assert.True(fields.MoveNext());
			Assert.Equal("a|b", Encoding.Latin1.GetString(FixFixtures.Typed<FixField.Data>(FixTag.RawData, fields.Current).Value.Span));
			Assert.Equal(first.Length + 1, stream.Position);
		}
		Assert.True(stream.CanRead);

		using var many = new StringReader(string.Concat(Enumerable.Repeat("55=ABC|", 10000)));
		Assert.Equal(10000, HandFixParser.ParseLog(many, FixFixtures.Reading(true, 3, 32)).Count());
		using var large = new StringReader("55=" + new string('X', 100));
		Assert.Throws<IOException>(() => HandFixParser.ParseLog(large, FixFixtures.Reading(true, 3, 16)).ToArray());
	}

	[Fact]
	public void Concurrent_enumerators_do_not_share_binary_state()
	{
		using var one = new StringReader("95=3|96=a|b|55=A");
		using var two = new StringReader("95=1|96=X|55=B");
		using var a = HandFixParser.ParseLog(one, FixFixtures.Reading(true, 1)).GetEnumerator();
		using var b = HandFixParser.ParseLog(two, FixFixtures.Reading(true, 1)).GetEnumerator();
		Assert.True(a.MoveNext());
		Assert.True(b.MoveNext());
		Assert.True(a.MoveNext());
		Assert.True(b.MoveNext());
		Assert.True(a.MoveNext());
		Assert.True(b.MoveNext());
		Assert.Equal("A", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, a.Current).Value);
		Assert.Equal("B", FixFixtures.Typed<FixField.Text>(FixTag.Symbol, b.Current).Value);
	}

	static void Compare(string input, bool log, Fix44Context? options = null)
	{
		// Both parsers are told the framing the same way: by the context, never by the method.
		var context = (options ?? new Fix44Context()) with { Framing = log ? FixFraming.Log : FixFraming.Wire };
		var expected = FixParser.ParseFields(input, context);
		Equal(expected, HandFixParser.Parse(input, context));
		Equal(expected, HandFixParser.Parse(input.AsSpan(), context));
		using var reader = new StringReader(input);
		Equal(expected, HandFixParser.Parse(reader, context with { BufferSize = 1 }));
		Assert.Equal(-1, reader.Peek());

		var bytes = Encoding.Latin1.GetBytes(input);
		var expectedBytes = FixParser.ParseFields(bytes, context);
		Equal(expectedBytes, HandFixParser.Parse(bytes, context));
		using var stream = new ShortStream(bytes);
		Equal(expectedBytes, HandFixParser.Parse(stream, context with { BufferSize = 3 }));
		Assert.True(stream.CanRead);

		// The buffered forms of the generated parser agree with its whole-input form.
		using var generatedReader = new StringReader(input);
		Equal(expected, FixParser.ReadFields(generatedReader, context with { BufferSize = 1 }));
		using var generatedStream = new ShortStream(bytes);
		Equal(expectedBytes, FixParser.ReadFields(generatedStream, context with { BufferSize = 3 }));
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
