using System;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The road that reads a message as the octets it arrived as, and what the specification says has
/// to be true on it.
/// </summary>
/// <remarks>
/// <para>
/// FIX counts its envelope in octets: <c>BodyLength</c> is a length in bytes and <c>CheckSum</c> a
/// sum of bytes. A field of type <c>data</c> is raw octets whose end is given by the paired length
/// field and not by the separator — the specification says in as many words that the value may
/// contain the delimiter.
/// </para>
/// <para>
/// So these are not tests that the byte road is cheaper. It is not: a message keeps its source, so
/// the octets are materialised into a string either way. They are tests that it is the road on
/// which the specification's arithmetic is exact, and on which the caller is asked for nothing they
/// can get wrong.
/// </para>
/// </remarks>
public sealed class FixOctetRoadTests
{
	const byte Soh = 1;

	/// <summary>Frames a body given as octets, counting BodyLength and CheckSum in octets.</summary>
	static byte[] Framed(byte[] body)
	{
		var head = Encoding.Latin1.GetBytes("8=FIX.4.4\u00019=" + body.Length + "\u0001");
		var sum  = head.Sum(octet => (int)octet) + body.Sum(octet => (int)octet);
		var tail = Encoding.Latin1.GetBytes("10=" + (sum % 256).ToString("000") + "\u0001");
		var wire = new byte[head.Length + body.Length + tail.Length];

		head.CopyTo(wire, 0);
		body.CopyTo(wire, head.Length);
		tail.CopyTo(wire, head.Length + body.Length);

		return wire;
	}

	/// <summary>The same, from text written with pipes for the separator.</summary>
	static byte[] Framed(string body)
	{
		return Framed(Octets(body));
	}

	/// <summary>A message whose body holds the octets of a data field's payload.</summary>
	static byte[] Framed(string before, byte[] payload, string after)
	{
		return Framed([..Octets(before), ..payload, Soh, ..Octets(after)]);
	}

	/// <summary>Every field as the tag it carries and the extent it was read from.</summary>
	static (FixTag Tag, int Position, int Length)[] Extents(FixMessage message)
	{
		return message.Fields.Select(field => (field.Tag, field.Position, field.Length)).ToArray();
	}

	static byte[] Octets(string text)
	{
		return Encoding.Latin1.GetBytes(text.Replace('|', '\u0001'));
	}

	/// <summary>
	/// The field with that tag wherever it sits — header, body or trailer — because which scope
	/// holds a tag is the schema's answer and not what any of these tests is asking.
	/// </summary>
	static FixField Find(FixMessage message, int tag)
	{
		return message.Fields.First(field => (int)field.Tag == tag);
	}

	const string Head  = "35=D|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|";
	const string Order = "11=ORDER|21=1|55=ABC|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|";
	const string Logon = "35=A|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|98=0|108=30|";

	// ── a data field ends where its length says, not where a separator is ────────────────────

	/// <summary>
	/// RawData carrying the separator itself: the payload is cut by its length, and the message is
	/// whole.
	/// </summary>
	[Fact]
	public void A_raw_data_payload_may_hold_the_separator()
	{
		byte[] payload = [(byte)'a', Soh, (byte)'b', Soh, (byte)'c'];

		var message = FixParser.ParseMessage(Framed(Logon + "95=5|96=", payload, "141=N|"));
		var data    = FixFixtures.Typed<FixField.Data>(FixTag.RawData, Find(message, 96));

		Assert.True(data.IsValid);
		Assert.Equal(payload, data.Value.ToArray());

		// And the field after it is read as a field, which is what "cut by the length" has to mean.
		Assert.False(FixFixtures.Typed<FixField.Boolean>(FixTag.ResetSeqNumFlag, Find(message, 141)).Value);
	}

	/// <summary>XmlData, a header pair, behaves the same way.</summary>
	[Fact]
	public void An_xml_data_payload_may_hold_the_separator()
	{
		var payload = Octets("<a>|</a>");
		var message = FixParser.ParseMessage(Framed(Head + "212=" + payload.Length + "|213=", payload, Order));
		var data    = FixFixtures.Typed<FixField.Data>(FixTag.XmlData, Find(message, 213));

		Assert.Equal(payload, data.Value.ToArray());
	}

	/// <summary>Every length/data pair the standard defines is read by its length, as one field.</summary>
	/// <remarks>
	/// Asked of the reader at field level rather than of a message, because which scope a tag
	/// belongs to is a separate question with a separate answer. A declared pair is <em>one</em>
	/// field carrying the data tag, with the length field standing in its wire prefix; a pair the
	/// reader did not know about would be three fields instead — the length, the payload cut at the
	/// separator inside it, and the remainder recovered as invalid.
	/// </remarks>
	[Theory]
	[InlineData( 90,  91)]  // SecureDataLen    / SecureData
	[InlineData( 93,  89)]  // SignatureLength  / Signature
	[InlineData( 95,  96)]  // RawDataLength    / RawData
	[InlineData(212, 213)]  // XmlDataLen       / XmlData
	[InlineData(348, 349)]  // EncodedIssuerLen / EncodedIssuer
	[InlineData(354, 355)]  // EncodedTextLen   / EncodedText
	public void The_standards_pairs_are_read_by_length(int lengthTag, int dataTag)
	{
		byte[] payload = [(byte)'x', Soh, (byte)'y'];

		var input  = Octets(lengthTag + "=" + payload.Length + "|" + dataTag + "=")
			.Concat(payload)
			.Concat([Soh])
			.ToArray();
		var fields = FixParser.ParseFields(input);

		Assert.Equal([lengthTag, dataTag], fields.Select(one => (int)one.Tag));

		var field = fields[1];

		Assert.Equal(payload, Assert.IsAssignableFrom<FixField.Typed<ReadOnlyMemory<byte>>>(field).Value.ToArray());
	}

	/// <summary>
	/// An Encoded field's octets come back as they were, and MessageEncoding is a value to read
	/// rather than an instruction anything here carries out.
	/// </summary>
	[Fact]
	public void Encoded_fields_are_octets_and_nothing_is_decoded()
	{
		// Three characters in Shift_JIS, which is not text this package will interpret.
		byte[] payload = [0x83, 0x65, 0x83, 0x58, 0x83, 0x67];

		var message = FixParser.ParseMessage(
			Framed(Head + "347=Shift_JIS|" + Order + "58=note|354=" + payload.Length + "|355=", payload, ""));

		Assert.Equal("Shift_JIS", FixFixtures.Typed<FixField.Text>(FixTag.MessageEncoding, Find(message, 347)).Value);
		Assert.Equal(payload, FixFixtures.Typed<FixField.Data>(FixTag.EncodedText, Find(message, 355)).Value.ToArray());
	}

	// ── what the octet road asks of a caller, and what the string road asks ──────────────────

	/// <summary>
	/// A value inside the octet range is read from octets and refused from a string decoded as
	/// UTF-8 — by a message about the length, which points at the counterparty's framing when the
	/// fault is in the caller's decoding.
	/// </summary>
	/// <remarks>
	/// This is the test that says why the octet road exists. A string input is a claim that somebody
	/// has already decoded the wire one character to one octet, and reading a file as UTF-8 — the
	/// ordinary thing to do — breaks that claim. Where the characters stay under U+0100 nothing
	/// notices but the envelope arithmetic, which counts characters against a length written in
	/// octets. Octets carry no such claim, so there is nothing to break.
	/// </remarks>
	[Fact]
	public void A_value_inside_the_octet_range_reads_from_octets_and_misleads_from_a_utf8_string()
	{
		var body = Encoding.UTF8.GetBytes("35=D|49=S|56=T|34=1|52=20260920-12:00:00|58=é|".Replace('|', '\u0001'));
		var wire = Framed(body);

		// The octets: read, and the value is the two octets it was.
		var message = FixParser.ParseMessage(wire);

		Assert.Equal(2, Find(message, 58).Length);

		// Latin-1: the claim the string road makes is true, so it reads the same message.
		Assert.Equal(Extents(message), Extents(FixParser.ParseMessage(Encoding.Latin1.GetString(wire))));

		// UTF-8: the claim is false, and what is found is the length rather than the decoding.
		var misread = FixParser.ParseMessage(Encoding.UTF8.GetString(wire));

		Assert.False(misread.Validate(FixContext.Default));
		Assert.Contains(misread.InvalidFindings!, finding => finding.Rule == FixRule.BodyLengthMismatch);
	}

	/// <summary>
	/// Where the decoding lifts a character above the octet range, the same length is found wrong.
	/// </summary>
	[Fact]
	public void A_value_above_the_octet_range_is_found_by_its_length()
	{
		var body = Encoding.UTF8.GetBytes("35=D|49=S|56=T|34=1|52=20260920-12:00:00|58=€|".Replace('|', '\u0001'));
		var wire = Framed(body);

		Assert.Equal(3, Find(FixParser.ParseMessage(wire), 58).Length);

		var misread = FixParser.ParseMessage(Encoding.UTF8.GetString(wire));

		Assert.False(misread.Validate(FixContext.Default));
		Assert.Contains(misread.InvalidFindings!, finding => finding.Rule == FixRule.BodyLengthMismatch);
	}

	// ── the doors of the octet road answer alike ─────────────────────────────────────────────

	/// <summary>Octets, a Latin-1 string and a stream over the same octets give the same message.</summary>
	[Fact]
	public void The_three_roads_agree_on_one_message()
	{
		var wire = Framed(Head + Order);

		var fromOctets = FixParser.ParseMessage(wire);
		var fromString = FixParser.ParseMessage(Encoding.Latin1.GetString(wire));

		using var stream = new MemoryStream(wire, writable: false);

		var fromStream = FixParser.ReadMessage(stream);

		Assert.Equal(Extents(fromOctets), Extents(fromString));
		Assert.Equal(Extents(fromOctets), Extents(fromStream));
		Assert.Equal(fromOctets.MessageType, fromStream.MessageType);
	}

	/// <summary>
	/// An array and memory over a slice of a larger one are the same input to every door that takes
	/// octets, and positions count from the start of the slice.
	/// </summary>
	[Fact]
	public void An_array_and_memory_over_a_slice_read_alike()
	{
		var wire   = Framed(Head + Order);
		var read   = FixParser.ParseMessage(wire);
		var padded = new byte[wire.Length + 6];

		wire.CopyTo(padded, 3);

		var slice = new ReadOnlyMemory<byte>(padded, 3, wire.Length);

		Assert.Equal(Extents(read), Extents(FixParser.ParseMessage(slice)));

		Assert.True(FixParser.TryParseMessage(slice, out var message, out var error));
		Assert.Null(error);
		Assert.Equal(Extents(read), Extents(message!));

		Assert.Equal(
			FixParser.ParseFields(wire).Select(field => (field.Tag, field.Position, field.Length)),
			FixParser.ParseFields(slice).Select(field => (field.Tag, field.Position, field.Length)));

		Assert.Equal(
			FixParser.ParseMessages(wire).Length,
			FixParser.ParseMessages(slice).Length);
	}

	/// <summary>Concatenated messages come back in order, and each keeps its own wire.</summary>
	[Fact]
	public void Every_message_of_a_buffer_of_octets_is_read_in_order()
	{
		var first  = Framed(Head + Order);
		var second = Framed(Logon + "141=N|");
		var both   = first.Concat(second).ToArray();

		var messages = FixParser.ParseMessages(both);

		Assert.Equal(2, messages.Length);
		Assert.Equal("D", messages[0].MessageType);
		Assert.Equal("A", messages[1].MessageType);
		Assert.Equal(Extents(FixParser.ParseMessage(Encoding.Latin1.GetString(first))),  Extents(messages[0]));
		Assert.Equal(Extents(FixParser.ParseMessage(Encoding.Latin1.GetString(second))), Extents(messages[1]));
	}

	// ── the refusals ─────────────────────────────────────────────────────────────────────────

	/// <summary>A length that does not measure its payload is refused, not read to the separator.</summary>
	[Fact]
	public void A_data_length_that_does_not_measure_its_payload_is_refused()
	{
		byte[] payload = [(byte)'a', Soh, (byte)'b'];

		Assert.Throws<FormatException>(
			() => FixParser.ParseMessage(Framed(Logon + "95=99|96=", payload, "141=N|")));
	}

	/// <summary>A checksum that does not match the octets is found on the octet road too.</summary>
	[Fact]
	public void A_wrong_checksum_is_found_from_octets()
	{
		var wire = Framed(Head + Order);

		wire[^2] = (byte)(wire[^2] == (byte)'9' ? '8' : '9');

		var message = FixParser.ParseMessage(wire);

		Assert.False(message.Validate(FixContext.Default));
		Assert.Contains(message.InvalidFindings!, finding => finding.Rule == FixRule.CheckSumMismatch);
	}

	/// <summary>The trying door answers a message cut short with a diagnostic rather than throwing.</summary>
	[Fact]
	public void A_malformed_message_of_octets_comes_back_as_a_diagnostic()
	{
		var wire = Framed(Head + Order)[..^1];

		Assert.False(FixParser.TryParseMessage(wire, out var message, out var error));
		Assert.Null(message);
		Assert.NotNull(error);
		Assert.Contains("CheckSum", error!.ToString(), StringComparison.Ordinal);
	}

	/// <summary>Null octets are refused by the call, and by the trying door with a diagnostic.</summary>
	[Fact]
	public void Null_octets_are_refused_at_the_door()
	{
		Assert.Throws<ArgumentNullException>(() => FixParser.ParseMessage((byte[])null!));
		Assert.Throws<ArgumentNullException>(() => FixParser.ParseMessages((byte[])null!));
		Assert.Throws<ArgumentNullException>(() => FixParser.ParseFields((byte[])null!));

		Assert.False(FixParser.TryParseMessage((byte[]?)null, out var message, out var error));
		Assert.Null(message);
		Assert.NotNull(error);
	}
}
