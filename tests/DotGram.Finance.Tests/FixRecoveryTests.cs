using System;
using System.Text;

using DotGram.Examples.Finance;
using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixRecoveryTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Recovery_returns_errors_in_order_with_original_input_and_absolute_positions(bool dispatch)
	{
		const string input = "55=ABC|bad|38=2|0=X|55=END|tail";
		var text = dispatch ? FixParser.ParseLog(input) : Fix44.ParseLog(input);
		FixField[]? parsed;
		FixParseError? error;
		var success = dispatch
			? FixParser.TryParse(input, out parsed, out error, new FixOptions('|'))
			: Fix44.TryParse(input, out parsed, out error, new FixOptions('|'));
		Assert.False(success);
		Assert.NotNull(error);
		Assert.Equal(6, parsed!.Length);
		using var reader = new StringReader(input);
		using var stream = new MemoryStream(Encoding.Latin1.GetBytes(input));
		var chars = dispatch ? FixParser.Parse(reader, new FixOptions('|'), 1, 16) : Fix44.Parse(reader, new FixOptions('|'), 1, 16);
		var bytes = dispatch ? FixParser.Parse(stream, new FixOptions('|'), 1, 16) : Fix44.Parse(stream, new FixOptions('|'), 1, 16);
		Check(text, false);
		Check(chars, false);
		Check(bytes, true);
		Assert.True(stream.CanRead);

		static void Check(IEnumerable<FixField> source, bool bytes)
		{
			var fields = source.ToArray();
			Assert.Equal(new[] { 55, 0, 38, 0, 55, 0 }, fields.Select(field => field.Tag));
			Assert.Equal("END", Assert.IsType<FixField.Symbol>(fields[4]).Value);
			var invalid = fields.OfType<FixField.Invalid>().ToArray();
			Assert.Equal(new[] { 7, 16, 27 }, invalid.Select(field => field.Position));
			Assert.Equal(new[] { "bad", "0=X", "tail" }, invalid.Select(field => bytes ? Encoding.Latin1.GetString(field.RawBytes.Span) : field.RawText));
			Assert.All(invalid, field =>
			{
				Assert.False(field.IsValid);
				Assert.Equal(bytes, field.IsByteInput);
				Assert.Equal(field.Position, field.ValuePosition);
				Assert.Equal(bytes ? field.RawBytes.Length : field.RawText!.Length, field.Length);
				Assert.NotEmpty(field.Message);
			});
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Raw_error_data_preserves_high_bytes_and_unicode_text(bool dispatch)
	{
		var wire = new byte[] { 255, 0, 124, 53, 53, 61, 88 };
		var bytes = dispatch ? FixParser.Parse(wire, new FixOptions('|')) : Fix44.Parse(wire, new FixOptions('|'));
		Assert.Equal(new byte[] { 255, 0 }, Assert.IsType<FixField.Invalid>(bytes[0]).RawBytes.ToArray());
		Assert.Equal("X", Assert.IsType<FixField.Symbol>(bytes[1]).Value);
		var fields = dispatch ? FixParser.ParseLog("ошибка|55=X") : Fix44.ParseLog("ошибка|55=X");
		Assert.Equal("ошибка", Assert.IsType<FixField.Invalid>(fields[0]).RawText);
	}
}
