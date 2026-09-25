using System;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class Fix44StreamingTests
{
	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void All_messages_match_contiguous_input(string name, string wire)
	{
		using var chars = new ShortReader(wire + wire);
		using var bytes = new ShortStream(ToBytes(wire + wire));
		var messages = FixParser.ReadMessages(chars).Concat(FixParser.ReadMessages(bytes)).ToArray();
		Assert.Equal(4, messages.Length);
		foreach (var parsed in messages)
		{
			Assert.Equal(name, parsed.GetType().Name);
			Assert.Equal(Extents(FixParser.ParseMessage(wire)), Extents(parsed));
		}
	}

	[Fact]
	public void Single_reads_leave_next_message_and_input_open()
	{
		var wire = (string)FixFixtures.Messages().First()[1];
		using var input = new MemoryStream(ToBytes(wire + wire));
		Assert.Equal(Extents(FixParser.ParseMessage(wire)), Extents(FixParser.ReadMessage(input)));
		Assert.Equal(wire.Length, input.Position);
		using (var iterator = FixParser.ReadMessages(input).GetEnumerator()) Assert.True(iterator.MoveNext());
		Assert.True(input.CanRead);
		Assert.Empty(FixParser.ReadMessages(input));
		Assert.False(FixParser.TryReadMessage(input, out _, out var error));
		Assert.NotNull(error);
	}

	[Fact]
	public void Truncation_limits_and_checksum_are_rejected()
	{
		var wire = (string)FixFixtures.Messages().First()[1];
		for (var i = 0; i < wire.Length; i++)
		{
			Assert.False(FixParser.TryReadMessage(new StringReader(wire.Substring(0, i)), out _, out var error));
			Assert.NotNull(error);
		}
		Assert.False(FixParser.TryReadMessage(new StringReader(wire), out _, out _, maxMessageLength: wire.Length - 1));
		Assert.Equal(Extents(FixParser.ParseMessage(wire)), Extents(FixParser.ReadMessage(new StringReader(wire), maxMessageLength: wire.Length)));
		var damaged = FixParser.ReadMessage(new StringReader(wire.Substring(0, wire.Length - 4) + "999\u0001"));
		Assert.False(damaged.Validate(Fix44Context.Default));
		Assert.Contains(damaged.InvalidFindings!, finding => finding.Rule == FixRule.CheckSumMismatch);
		Assert.Throws<FormatException>(() => FixParser.ReadMessages(new StringReader(wire + "8=")).ToArray());
		foreach (var length in new[] { "", "-1", "x", "999999999999999999999" })
			Assert.False(FixParser.TryReadMessage(new StringReader("8=FIX.4.4\u00019=" + length + "\u0001"), out _, out _));
	}

	[Fact]
	public void Standard_raw_octets_survive_framing()
	{
		var body = "35=A|49=S|56=T|34=1|52=20260915-12:00:00|98=0|108=30|95=8|96=\0\u00ff\u000110=00|".Replace('|', '\u0001');
		var prefix = "8=FIX.4.4\u00019=" + body.Length + "\u0001" + body;
		var wire = prefix + "10=" + (prefix.Sum(c => (int)c) & 255).ToString("000", System.Globalization.CultureInfo.InvariantCulture) + "\u0001";
		Fix44Context? options = null;
		using var bytes = new ShortStream(ToBytes(wire + wire));
		Assert.Equal(2, FixParser.ReadMessages(bytes, options).Count());
		Assert.Equal(Extents(FixParser.ParseMessage(wire, options)), Extents(FixParser.ReadMessage(new StringReader(wire), options)));
	}

	/// <summary>Every field as the tag it carries and the extent it was read from.</summary>
	static (int Tag, int Position, int Length)[] Extents(FixMessage message)
	{
		return message.Fields.Select(field => (field.Tag, field.Position, field.Length)).ToArray();
	}

	static byte[] ToBytes(string text)
	{
		return text.Select(c => checked((byte)c)).ToArray();
	}

	sealed class ShortReader : TextReader
	{
		readonly string text;
		int position;
		public ShortReader(string text)
		{
			this.text = text;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			count = Math.Min(Math.Min(count, 3), text.Length - position);
			text.CopyTo(position, buffer, index, count);
			position += count;
			return count;
		}
	}

	sealed class ShortStream : Stream
	{
		readonly MemoryStream inner;
		public ShortStream(byte[] data)
		{
			inner = new MemoryStream(data);
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override int Read(byte[] buffer, int offset, int count)
		{
			return inner.Read(buffer, offset, Math.Min(3, count));
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
	}
}
