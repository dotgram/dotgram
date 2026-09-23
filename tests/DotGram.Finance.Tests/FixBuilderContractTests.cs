using System;
using System.Linq;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What building a message out of fields answers, and what four of those answers used to be.
/// </summary>
/// <remarks>
/// <para>
/// Reading a FIX message into a model is one act and holding it to a schema is another. This is
/// about the first: which fields land where, how the entries of a repeating group are cut, what an
/// unknown message type gets, and — word for word — what the reader says when the structure is not
/// one it can build.
/// </para>
/// <para>
/// It was written before the message model was rewritten, because those sentences are public: a
/// consumer reads them in a <c>FormatException</c> or a <c>FixParseError</c>, and a change to them
/// changes what their logs say. Four of them have changed, and each is marked below. The reason is
/// the same in all four: a message used to be read as a scope of nodes against the schema, so the
/// schema could be disagreed with while the input was still being read. A message is now its typed
/// fields and one flat list, built by a switch over the tags, and the reading has nothing left to
/// disagree with. What those four refusals said is not lost — each is a rule of <c>FixRule</c> and
/// is answered by validation, once, over the built message.
/// </para>
/// </remarks>
public sealed class FixBuilderContractTests
{
	// ── what the reading no longer refuses ───────────────────────────────────────────────────

	/// <summary>A count that is not a number the entries could match.</summary>
	/// <remarks>
	/// Was: refused, "Invalid or impossible NumInGroup.", tag 453. Nothing is sized by a count
	/// now, so the count is a field like any other and the disagreement is GroupCountMismatch.
	/// </remarks>
	[Fact]
	public void A_count_that_cannot_be_a_count_is_read()
	{
		Assert.True(FixParser.TryParseMessage(
			FixWire("35=D|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=X|453=nine|448=P|447=D|452=1|"),
			out var message, out var error), error?.ToString());

		var order = Assert.IsType<FixMessage.NewOrderSingle>(message);

		Assert.False(order.NoPartyIDs!.IsValid);
		Assert.Equal("P", order.NoPartyIDsGroups![0].PartyID.Value);
	}

	/// <summary>A count larger than the fields that follow it.</summary>
	/// <remarks>Was: the same refusal. Now one entry is read and the count says four hundred.</remarks>
	[Fact]
	public void A_count_larger_than_what_remains_is_read()
	{
		Assert.True(FixParser.TryParseMessage(
			FixWire("35=D|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=X|453=400|448=P|447=D|452=1|"),
			out var message, out var error), error?.ToString());

		var order = Assert.IsType<FixMessage.NewOrderSingle>(message);

		Assert.Equal(400, (int)order.NoPartyIDs!.Value);
		Assert.Single(order.NoPartyIDsGroups!);
	}

	/// <summary>An entry that does not open with the tag its group is cut by.</summary>
	/// <remarks>
	/// Was: refused, "Group entry must begin with its schema delimiter; NumInGroup is not
	/// satisfied.", tag 453. Entries open at the delimiter wherever it appears, so a count of two
	/// with one delimiter is one entry and a count that disagrees with it.
	/// </remarks>
	[Fact]
	public void An_entry_that_does_not_open_with_its_delimiter_is_read()
	{
		Assert.True(FixParser.TryParseMessage(
			FixWire("35=D|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=X|453=2|448=P|447=D|452=1|"),
			out var message, out var error), error?.ToString());

		var order = Assert.IsType<FixMessage.NewOrderSingle>(message);

		Assert.Equal(2, (int)order.NoPartyIDs!.Value);
		Assert.Single(order.NoPartyIDsGroups!);
	}

	/// <summary>A field no scope of the message will take.</summary>
	/// <remarks>
	/// Was: refused, "Field is not permitted in this message scope." There are no scopes to be
	/// outside of now: a tag the message does not place is carried in Fields, and that it has no
	/// place is FieldNotInScope.
	/// </remarks>
	[Fact]
	public void A_field_no_scope_takes_is_read_and_carried()
	{
		Assert.True(FixParser.TryParseMessage(
			FixWire("35=0|49=S|56=T|34=1|52=20260920-12:00:00|112=T|93=3|89=abc|58=late|"),
			out var message, out var error), error?.ToString());

		Assert.IsType<FixMessage.Heartbeat>(message);
		Assert.Contains(message!.Fields, field => field.Tag == 58);
	}

	// ── what an unknown message type gets ────────────────────────────────────────────────────

	/// <summary>
	/// A type the package does not describe comes back whole, flat, and in the order it was written.
	/// </summary>
	/// <remarks>
	/// The schema has nothing to say about it, so nothing is cut into groups and nothing is refused
	/// for being out of place: the standard header and trailer are read as themselves and the rest
	/// is carried as it stood on the wire. This is the behaviour anything built to replace the
	/// reader has to reproduce, and it is easy to lose, because it is what happens when the schema
	/// is silent rather than something the schema says.
	/// </remarks>
	[Fact]
	public void An_unknown_type_keeps_every_field_flat_and_in_order()
	{
		var wire    = FixWire("35=ZZ|49=S|56=T|34=1|52=20260920-12:00:00|453=1|448=P|447=D|9999=x|58=note|");
		var message = FixParser.ParseMessage(wire);

		Assert.Equal("ZZ", message.MessageType);
		Assert.IsType<FixMessage.Invalid>(message);

		// Everything that is not the standard header or the trailer, in wire order.
		Assert.Equal(
			[453, 448, 447, 9999, 58],
			message.Fields.Select(one => one.Tag).Where(one => one is not (8 or 9 or 10 or 34 or 35 or 49 or 52 or 56)).ToArray());

		// The standard header is still read as the header.
		Assert.Equal("S", message.SenderCompID!.Value);
	}

	/// <summary>
	/// The same message read as fields and read as a message hold the same tags in the same order.
	/// </summary>
	[Fact]
	public void An_unknown_type_loses_nothing_the_field_reader_found()
	{
		var wire = FixWire("35=ZZ|49=S|56=T|34=1|52=20260920-12:00:00|453=1|448=P|9999=x|");

		var flat  = FixParser.ParseFields(wire).Select(one => one.Tag).Where(one => one != 8 && one != 9 && one != 10);
		var built = FixParser.ParseMessage(wire).Fields.Select(one => one.Tag).Where(one => one != 8 && one != 9 && one != 10);

		Assert.Equal(flat, built);
	}

	/// <summary>Frames a body written with pipes for the separator.</summary>
	static string FixWire(string body)
	{
		var fields = body.Replace('|', '\u0001');
		var head   = "8=FIX.4.4\u00019=" + fields.Length + "\u0001";
		var sum    = 0;

		foreach (var character in head + fields)
			sum += character;

		return head + fields + "10=" + (sum % 256).ToString("D3") + "\u0001";
	}
}
