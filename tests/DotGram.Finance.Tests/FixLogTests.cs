using System;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixLogTests
{
	[Theory]
	[InlineData("|")]
	[InlineData(" | ")]
	[InlineData("  |   ")]
	[InlineData(" |")]
	[InlineData("| ")]
	public void Log_separators_preserve_values_and_source_locations(string separator)
	{
		var input = "55=T 1 1/8 06/15/18" + separator + "38=2|58=hello world" + separator;

		foreach (var fields in ReadLog(input))
		{
			Assert.Equal(3, fields.Length);
			Assert.Equal("T 1 1/8 06/15/18", Assert.IsType<FixField.Symbol>(fields[0]).Value);
			Assert.Equal("hello world", Assert.IsType<FixField.Text>(fields[2]).Value);
			Assert.Equal(input.IndexOf("38=", StringComparison.Ordinal), fields[1].Position);
			Assert.Equal(input.IndexOf("hello world", StringComparison.Ordinal), fields[2].ValuePosition);
			Assert.Equal("hello world".Length, fields[2].Length);
			Assert.All(fields, field => Assert.True(field.IsValid));
		}
	}

	[Theory]
	[InlineData("")]
	[InlineData(" a | b= ")]
	[InlineData(" | ")]
	[InlineData("abc|")]
	public void Binary_payload_is_never_trimmed_or_split_on_log_separators(string payload)
	{
		var input = "55=START | 95=" + payload.Length + " | 96=" + payload + " | 55=END";

		foreach (var fields in ReadLog(input))
		{
			Assert.Equal(3, fields.Length);
			var binary = Assert.IsType<FixField.RawData>(fields[1]);
			Assert.Equal(Encoding.Latin1.GetBytes(payload), binary.Value.ToArray());
			Assert.Equal(input.IndexOf("95=", StringComparison.Ordinal), binary.Position);
			Assert.Equal(input.IndexOf("96=", StringComparison.Ordinal) + 3, binary.ValuePosition);
			Assert.Equal(payload.Length, binary.Length);
			Assert.Equal("END", Assert.IsType<FixField.Symbol>(fields[2]).Value);
		}
	}

	[Fact]
	public void Custom_binary_pairs_also_accept_padded_separators()
	{
		var options = new FixContext { LengthDataPairs = new Dictionary<int, int> { [5000] = 5001 } };
		const string input = "5000=3 | 5001= |  | 55=END";

		foreach (var fields in ReadLog(input, options))
		{
			Assert.Equal(2, fields.Length);
			Assert.Equal(" | ", Encoding.Latin1.GetString(Assert.IsType<FixField.Custom>(fields[0]).Value.Span));
			Assert.Equal("END", Assert.IsType<FixField.Symbol>(fields[1]).Value);
		}
	}

	[Fact]
	public void Recovery_uses_the_complete_padded_separator()
	{
		const string input = "55=X | broken | 38=2";

		foreach (var fields in ReadLog(input))
		{
			Assert.Equal(3, fields.Length);
			var invalid = Assert.IsType<FixField.Invalid>(fields[1]);
			Assert.Equal("broken", invalid.IsByteInput ? Encoding.Latin1.GetString(invalid.RawBytes.Span) : invalid.RawText);
			Assert.Equal(input.IndexOf("broken", StringComparison.Ordinal), invalid.Position);
			Assert.Equal(input.IndexOf("38=", StringComparison.Ordinal), fields[2].Position);
		}
	}

	[Fact]
	public void Wire_mode_keeps_pipe_characters_and_spaces_in_values()
	{
		const string value = "ABC | DEF ";
		var fields = FixParser.ParseFields("55=" + value + "\u000138=2\u0001");

		Assert.Equal(2, fields.Length);
		Assert.Equal(value, Assert.IsType<FixField.Symbol>(fields[0]).Value);
		Assert.Equal("ABC ", Assert.IsType<FixField.Symbol>(Assert.Single(FixParser.ParseFields("55=ABC ", FixContext.Log))).Value);
	}

	[Theory]
	[InlineData("55=ABC | 38=2")]
	[InlineData("bad | 38=2")]
	[InlineData("95=3 | 96=a|b | 38=2")]
	public void Log_enumeration_only_looks_past_padding_to_the_next_character(string input)
	{
		var limit = input.IndexOf("38=", StringComparison.Ordinal) + 1;
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input)) { ReadLimit = limit };
		using var reader = new GatedReader(input, limit);
		var bytes = FixParser.ReadFields(stream, FixContext.Log, bufferSize: 1);
		var chars = FixParser.ReadFields(reader, FixContext.Log, bufferSize: 1);

		Assert.Equal(0, stream.Position);
		Assert.Equal(0, reader.ReadCount);
		using var byteFields = bytes.GetEnumerator();
		using var charFields = chars.GetEnumerator();
		Assert.True(byteFields.MoveNext());
		Assert.True(charFields.MoveNext());
		Assert.Equal(limit, stream.Position);
		Assert.Equal(limit, reader.ReadCount);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(1024)]
	[InlineData(16384)]
	public void Long_space_runs_preserve_internal_and_eof_spaces(int length)
	{
		var spaces = new string(' ', length);
		var value  = "A" + spaces + "B";

		foreach (var fields in ReadLog("58=" + value + spaces + " | 55=END" + spaces))
		{
			Assert.Equal(2, fields.Length);
			Assert.Equal(value, Assert.IsType<FixField.Text>(fields[0]).Value);
			Assert.Equal(value.Length, fields[0].Length);
			Assert.Equal("END" + spaces, Assert.IsType<FixField.Symbol>(fields[1]).Value);
			Assert.Equal(3 + value.Length + spaces.Length + 3, fields[1].Position);
		}
	}

	[Theory]
	[InlineData("58= | 55=END")]
	[InlineData("58=|55=END")]
	public void Empty_text_before_a_pipe_remains_invalid(string input)
	{
		foreach (var fields in ReadLog(input))
		{
			Assert.Equal(2, fields.Length);
			Assert.IsType<FixField.Invalid>(fields[0]);
			Assert.Equal("END", Assert.IsType<FixField.Symbol>(fields[1]).Value);
		}
	}

	static IEnumerable<FixField[]> ReadLog(string input, FixContext? options = null)
	{
		yield return FixParser.ParseFields(input, (options ?? new FixContext()) with { Framing = FixFraming.Log });
		yield return FixParser.ParseFields(input.AsSpan(), (options ?? new FixContext()) with { Framing = FixFraming.Log });
		yield return FixParser.ParseFields(Encoding.Latin1.GetBytes(input), (options ?? new FixContext()) with { Framing = FixFraming.Log });

		using var reader = new StringReader(input);
		using var stream = new ShortStream(Encoding.Latin1.GetBytes(input));

		yield return FixParser.ReadFields(reader, (options ?? new FixContext()) with { Framing = FixFraming.Log }, bufferSize: 1).ToArray();
		yield return FixParser.ReadFields(stream, (options ?? new FixContext()) with { Framing = FixFraming.Log }, bufferSize: 1).ToArray();
		Assert.True(stream.CanRead);
	}

	sealed class ShortStream(byte[] input) : MemoryStream(input)
	{
		public int ReadLimit { get; init; } = int.MaxValue;

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (Position >= ReadLimit)
				throw new IOException("The rest of the next field has not arrived.");

			return base.Read(buffer, offset, Math.Min(count, 1));
		}
	}

	sealed class GatedReader(string input, int limit) : StringReader(input)
	{
		public int ReadCount { get; private set; }

		public override int Read(char[] buffer, int index, int count)
		{
			if (ReadCount >= limit)
				throw new IOException("The rest of the next field has not arrived.");

			var read = base.Read(buffer, index, Math.Min(count, 1));
			ReadCount += read;

			return read;
		}
	}
}
