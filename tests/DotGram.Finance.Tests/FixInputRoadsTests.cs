using System;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

using Fix42 = DotGram.Finance.Fix.Fix42;
using Fix44 = DotGram.Finance.Fix.Fix44;
using Fix50 = DotGram.Finance.Fix.Fix50;

namespace DotGram.Finance.Tests;

/// <summary>
/// The same input is the same on every road to it: what a tag nothing defines keeps, and what a
/// message of another version is told.
/// </summary>
public sealed class FixInputRoadsTests
{
	[Theory]
	[InlineData("28905=20261231\u0001")]
	[InlineData("55=ABC\u000128905=x y\u0001")]
	public void A_tag_nothing_defines_keeps_its_value_in_the_representation_it_was_read_in(string wire)
	{
		var expected = wire.Split('\u0001')[^2].Split('=')[1];

		foreach (var (context, text) in new[] { (Fix44.Fix44Context.Default, wire), (Fix44.Fix44Context.WithLogFraming, wire.Replace('\u0001', '|')) })
		{
			var fromChars = Assert.IsType<FixField.Invalid>(Assert.Single(Fix44.FixParser.ParseFields(text, context), field => field.Tag == 28905));
			var fromBytes = Assert.IsType<FixField.Invalid>(Assert.Single(Fix44.FixParser.ParseFields(Encoding.Latin1.GetBytes(text), context), field => field.Tag == 28905));

			Assert.False(fromChars.IsByteInput);
			Assert.Equal(expected, fromChars.RawText);
			Assert.True(fromChars.RawBytes.IsEmpty);

			Assert.True(fromBytes.IsByteInput);
			Assert.Null(fromBytes.RawText);
			Assert.Equal(expected, Encoding.Latin1.GetString(fromBytes.RawBytes.Span));

			Assert.Equal(fromBytes.Position, fromChars.Position);
			Assert.Equal(fromBytes.ValuePosition, fromChars.ValuePosition);
			Assert.Equal(fromBytes.Length, fromChars.Length);
		}
	}

	[Fact]
	public void An_unknown_tag_inside_a_message_keeps_its_value_on_both_roads()
	{
		var wire    = FixFixtures.Wire("0", "28905=20261231|");
		var fromStr = FixParser44Fields(Fix44.FixParser.ParseMessage(wire));
		var fromOct = FixParser44Fields(Fix44.FixParser.ParseMessage(Encoding.Latin1.GetBytes(wire)));

		Assert.Equal("20261231", fromStr.RawText);
		Assert.False(fromStr.IsByteInput);
		Assert.Null(fromOct.RawText);
		Assert.True(fromOct.IsByteInput);
		Assert.Equal("20261231", Encoding.Latin1.GetString(fromOct.RawBytes.Span));

		static FixField.Invalid FixParser44Fields(Fix44.FixMessage message)
		{
			return message.Fields.OfType<FixField.Invalid>().Single(field => field.Tag == 28905);
		}
	}

	static string WireOf(string beginString)
	{
		return FixFixtures.Wire("0", "").Replace("8=FIX.4.4", "8=" + beginString, StringComparison.Ordinal);
	}

	static bool IsBeginStringFinding(FixFinding finding)
	{
		return finding is { Rule: FixRule.InvalidValue, Tag: FixTag.BeginString };
	}

	[Theory]
	[InlineData("FIX.4.2")]
	[InlineData("FIXT.1.1")]
	public void Fix44_reads_another_versions_message_from_a_buffer_and_from_every_stream_alike(string beginString)
	{
		var wire   = WireOf(beginString);
		var octets = Encoding.Latin1.GetBytes(wire);

		var messages = new[]
		{
			Fix44.FixParser.ParseMessage(wire),
			Fix44.FixParser.ParseMessage(octets),
			Fix44.FixParser.ReadMessage(new StringReader(wire)),
			Fix44.FixParser.ReadMessage(new MemoryStream(octets)),
			Fix44.FixParser.ReadMessages(new StringReader(wire + wire)).First(),
			Fix44.FixParser.ReadMessages(new MemoryStream(octets)).Single(),
			Fix44.FixParser.ParseMessages(wire + wire)[1],
			Fix44.FixParser.ParseMessages(octets)[0],
		};

		Assert.True(Fix44.FixParser.TryReadMessage(new MemoryStream(octets), out var tried, out var error));
		Assert.Null(error);

		foreach (var message in messages.Append(tried))
		{
			Assert.Equal("0", message.MessageType);
			Assert.Equal(beginString, message.BeginString!.Value);
			Assert.False(message.Validate(Fix44.Fix44Context.Default));
			Assert.Contains(message.InvalidFindings!, IsBeginStringFinding);
		}
	}

	[Fact]
	public void Fix42_and_Fix50_read_a_fix44_message_from_a_stream_as_from_a_buffer()
	{
		var wire   = WireOf("FIX.4.4");
		var octets = Encoding.Latin1.GetBytes(wire);

		foreach (var message in new Fix42.FixMessage[]
		{
			Fix42.FixParser.ParseMessage(wire),
			Fix42.FixParser.ReadMessage(new StringReader(wire)),
			Fix42.FixParser.ReadMessages(new MemoryStream(octets)).Single(),
		})
		{
			Assert.False(message.Validate(Fix42.Fix42Context.Default));
			Assert.Contains(message.InvalidFindings!, IsBeginStringFinding);
		}

		foreach (var message in new Fix50.FixMessage[]
		{
			Fix50.FixParser.ParseMessage(octets),
			Fix50.FixParser.ReadMessage(new MemoryStream(octets)),
			Fix50.FixParser.ReadMessages(new StringReader(wire)).Single(),
		})
		{
			Assert.False(message.Validate(Fix50.Fix50Context.Default));
			Assert.Contains(message.InvalidFindings!, IsBeginStringFinding);
		}
	}

	[Fact]
	public void Another_versions_frame_is_still_cut_by_its_BodyLength()
	{
		var wire   = WireOf("FIXT.1.1");
		var octets = Encoding.Latin1.GetBytes(wire + wire);

		using var stream = new MemoryStream(octets);

		Assert.Equal(2, Fix44.FixParser.ReadMessages(stream).Count());
		Assert.Equal(octets.Length, stream.Position);
	}

	[Theory]
	[InlineData("9=5\u000135=0\u0001")]
	[InlineData("8=\u00019=5\u000135=0\u0001")]
	[InlineData("8=FIX.4.4\u000135=0\u00019=5\u0001")]
	[InlineData("8=FIX.4.4\u0001")]
	public void A_frame_that_does_not_begin_with_a_BeginString_and_a_BodyLength_is_refused(string wire)
	{
		Assert.False(Fix44.FixParser.TryReadMessage(new StringReader(wire), out var message, out var error));
		Assert.Null(message);
		Assert.NotEmpty(error.Reason);

		Assert.False(Fix44.FixParser.TryReadMessage(new MemoryStream(Encoding.Latin1.GetBytes(wire)), out message, out error));
		Assert.Null(message);
		Assert.NotEmpty(error.Reason);
	}

	[Fact]
	public void A_long_BeginString_is_read_by_a_buffer_and_a_stream_alike()
	{
		var wire   = WireOf("ABCDEFGHIJKLMNOPQ");
		var octets = Encoding.Latin1.GetBytes(wire);

		Assert.True(Fix44.FixParser.TryParseMessage(wire, out var buffer, out _));
		Assert.True(Fix44.FixParser.TryReadMessage(new StringReader(wire), out var chars, out _));
		Assert.True(Fix44.FixParser.TryReadMessage(new MemoryStream(octets), out var bytes, out _));

		foreach (var message in new[] { buffer, chars, bytes })
		{
			Assert.False(message.Validate(Fix44.Fix44Context.Default));
			Assert.Contains(message.InvalidFindings!, IsBeginStringFinding);
		}
	}

	[Fact]
	public void A_BeginString_that_never_ends_is_bounded_by_maxMessageLength()
	{
		var wire = "8=" + new string('A', 5000);

		Assert.False(Fix44.FixParser.TryReadMessage(new StringReader(wire), out _, out var error, maxMessageLength: 64));
		Assert.Contains("maxMessageLength", error.Reason, StringComparison.Ordinal);
		Assert.False(Fix44.FixParser.TryReadMessage(new MemoryStream(Encoding.Latin1.GetBytes(wire)), out _, out error, maxMessageLength: 64));
		Assert.Contains("maxMessageLength", error.Reason, StringComparison.Ordinal);
	}
}
