using System;
using System.IO;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What the Try calls promise: what they leave null, what they refuse by throwing, and that
/// BuildMessage reads whatever terminators the fields it is given say they had.
/// </summary>
public sealed class FixTryContractTests
{
	static string Wire()
	{
		return FixFixtures.Wire("0", "");
	}

	static string Log(string separator)
	{
		return Wire().Replace("\u0001", separator, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("|")]
	[InlineData(" | ")]
	[InlineData("  |   ")]
	[InlineData(" |")]
	[InlineData("| ")]
	public void BuildMessage_takes_a_logs_padded_separators_as_the_fields_have_them(string separator)
	{
		var log     = Log(separator);
		var context = Fix44Context.WithLogFraming;
		var fields  = FixParser.ParseFields(log, context);
		var read    = FixParser.ParseMessage(log, context);
		var built   = FixParser.BuildMessage(log, fields, context);

		Assert.Equal(read.GetType(), built.GetType());
		Assert.Equal(read.Fields.Count, built.Fields.Count);
		Assert.True(built.Validate(context));
		Assert.True(read.Validate(context));

		Assert.True(FixParser.TryBuildMessage(log, fields, out var tried, out var error, context));
		Assert.Null(error);
		Assert.Equal(read.GetType(), tried.GetType());
	}

	[Theory]
	[InlineData("|")]
	[InlineData(" | ")]
	[InlineData("  |   ")]
	public void BuildMessage_reads_the_octets_a_log_was_read_from(string separator)
	{
		var octets  = Encoding.Latin1.GetBytes(Log(separator));
		var context = Fix44Context.WithLogFraming;
		var fields  = FixParser.ParseFields(octets, context);
		var read    = FixParser.ParseMessage(octets, context);
		var built   = FixParser.BuildMessage(octets, fields, context);

		Assert.Equal(read.GetType(), built.GetType());
		Assert.Equal(read.Fields.Count, built.Fields.Count);
		Assert.True(built.Validate(context));

		Assert.True(FixParser.TryBuildMessage(new ReadOnlyMemory<byte>(octets), fields, out var tried, out var error, context));
		Assert.Null(error);
		Assert.Same(fields[0], tried.Fields[0]);
	}

	[Fact]
	public void BuildMessage_reads_the_octets_of_the_wire()
	{
		var octets = Encoding.Latin1.GetBytes(Wire());
		var fields = FixParser.ParseFields(octets);
		var built  = FixParser.BuildMessage(octets, fields);

		Assert.Equal(FixParser.ParseMessage(octets).GetType(), built.GetType());
		Assert.True(built.Validate(Fix44Context.Default));
		Assert.All(fields, field => Assert.Contains(built.Fields, held => ReferenceEquals(held, field)));
	}

	[Fact]
	public void BuildMessage_refuses_a_source_that_is_not_the_one_the_fields_were_read_from()
	{
		var log     = Log(" | ");
		var context = Fix44Context.WithLogFraming;
		var fields  = FixParser.ParseFields(log, context);

		// One terminator no longer the length the field says, and one that holds no separator.
		var longer = log.Replace(" | ", "  | ", StringComparison.Ordinal);
		var spaces = log.Replace(" | ", "   ", StringComparison.Ordinal);
		var other  = log.Replace(" | ", " x ", StringComparison.Ordinal);

		foreach (var source in new[] { longer, spaces, other })
		{
			Assert.False(FixParser.TryBuildMessage(source, fields, out var message, out var error, context));
			Assert.Null(message);
			Assert.NotEmpty(error.Reason);
			Assert.Throws<FormatException>(() => FixParser.BuildMessage(source, fields, context));

			var octets = Encoding.Latin1.GetBytes(source);

			Assert.False(FixParser.TryBuildMessage(new ReadOnlyMemory<byte>(octets), fields, out message, out error, context));
			Assert.Null(message);
			Assert.NotEmpty(error.Reason);
			Assert.Throws<FormatException>(() => FixParser.BuildMessage(octets, fields, context));
		}

		// A log's fields are not the wire's: the terminators say " | ", the context says SOH.
		Assert.False(FixParser.TryBuildMessage(log, fields, out _, out _));
	}

	[Fact]
	public void The_Try_calls_leave_message_and_error_not_null_as_they_say()
	{
		// None of these needs a `!`: the project turns nullable warnings into errors.
		var wire = Wire();

		if (FixParser.TryParseMessage(wire, out var parsed, out var error))
			Assert.Equal("0", parsed.MessageType);
		else
			Assert.Fail(error.ToString());

		var octets = Encoding.Latin1.GetBytes(wire);

		if (FixParser.TryParseMessage(octets, out parsed, out error))
			Assert.Equal("0", parsed.MessageType);
		else
			Assert.Fail(error.ToString());

		if (FixParser.TryParseMessage(new ReadOnlyMemory<byte>(octets), out parsed, out error))
			Assert.Equal("0", parsed.MessageType);
		else
			Assert.Fail(error.ToString());

		if (FixParser.TryBuildMessage(wire, FixParser.ParseFields(wire), out var built, out error))
			Assert.Equal("0", built.MessageType);
		else
			Assert.Fail(error.ToString());

		using (var reader = new StringReader(wire))
		{
			if (FixParser.TryReadMessage(reader, out var read, out error))
				Assert.Equal("0", read.MessageType);
			else
				Assert.Fail(error.ToString());
		}

		using (var stream = new MemoryStream(octets))
		{
			if (FixParser.TryReadMessage(stream, out var read, out error))
				Assert.Equal("0", read.MessageType);
			else
				Assert.Fail(error.ToString());
		}

		if (!FixParser.TryParseMessage("8=FIX.4.4\u0001", out parsed, out error))
			Assert.NotEmpty(error.Reason);
		else
			Assert.Fail(parsed.MessageType);

		var text = Assert.IsType<FixField.Text>(FixParser.ParseFields("58=hello\u0001")[0]);

		if (text.TryGetValue(out var value))
			Assert.Equal(5, value.Length);
		else
			Assert.Fail("A text field is always valid.");
	}

	[Fact]
	public void Every_Try_call_refuses_a_null_input_by_throwing()
	{
		var wire   = Wire();
		var fields = FixParser.ParseFields(wire);

		Assert.Throws<ArgumentNullException>(() => FixParser.TryParseMessage((string)null!, out _, out _));
		Assert.Throws<ArgumentNullException>(() => FixParser.TryParseMessage((byte[])null!, out _, out _));
		Assert.Throws<ArgumentNullException>(() => FixParser.TryReadMessage((TextReader)null!, out _, out _));
		Assert.Throws<ArgumentNullException>(() => FixParser.TryReadMessage((Stream)null!, out _, out _));
		Assert.Throws<ArgumentNullException>(() => FixParser.TryBuildMessage((string)null!, fields, out _, out _));
		Assert.Throws<ArgumentNullException>(() => FixParser.TryBuildMessage(wire, null!, out _, out _));
		Assert.Throws<ArgumentNullException>(() => FixParser.TryBuildMessage(new ReadOnlyMemory<byte>([1]), null!, out _, out _));

		Assert.Throws<ArgumentNullException>(() => FixParser.BuildMessage((string)null!, fields));
		Assert.Throws<ArgumentNullException>(() => FixParser.BuildMessage(wire, null!));
		Assert.Throws<ArgumentNullException>(() => FixParser.BuildMessage(new ReadOnlyMemory<byte>([1]), null!));
	}

	[Fact]
	public void Padding_is_a_log_framings_alone_the_wires_terminator_is_the_separator()
	{
		var log   = "8=FIX.4.4 | 9=5 | 35=0 | 10=000 | ";
		var wire  = log.Replace('|', '\u0001');
		var bare  = "8=FIX.4.4|9=5|35=0|10=000|";
		var logs  = Fix44Context.WithLogFraming;
		var fields = FixParser.ParseFields(log, logs);

		// Fields read as a log, a source that is the same text with SOH for the pipes: not a wire.
		Assert.False(FixParser.TryBuildMessage(wire, fields, out _, out _));
		Assert.False(FixParser.TryBuildMessage(new ReadOnlyMemory<byte>(Encoding.Latin1.GetBytes(wire)), fields, out _, out _));
		Assert.False(FixParser.TryParseMessage(wire, out _, out _));

		// A wire's fields built from the wire, and a log's from the log, bare or padded, in both forms.
		var plain = bare.Replace('|', '\u0001');
		var wireFields = FixParser.ParseFields(plain);

		Assert.True(FixParser.TryBuildMessage(plain, wireFields, out _, out _));
		Assert.True(FixParser.TryBuildMessage(new ReadOnlyMemory<byte>(Encoding.Latin1.GetBytes(plain)), wireFields, out _, out _));
		Assert.True(FixParser.TryBuildMessage(log, fields, out _, out _, logs));
		Assert.True(FixParser.TryBuildMessage(new ReadOnlyMemory<byte>(Encoding.Latin1.GetBytes(log)), fields, out _, out _, logs));
		Assert.True(FixParser.TryBuildMessage(bare, FixParser.ParseFields(bare, logs), out _, out _, logs));

		// Fields that carry a padded terminator, built under wire framing.
		Assert.False(FixParser.TryBuildMessage(log, fields, out _, out _));
		Assert.False(FixParser.TryBuildMessage(new ReadOnlyMemory<byte>(Encoding.Latin1.GetBytes(log)), fields, out _, out _));
	}
}
