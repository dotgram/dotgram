using System;
using System.Linq;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What building a message out of fields answers today, pinned before anything is rewritten.
/// </summary>
/// <remarks>
/// <para>
/// Reading a FIX message into a model is one act and holding it to a schema is another. This is
/// about the first: which fields land in the header, the body and the trailer, how the entries of
/// a repeating group are cut, what an unknown message type gets, and — word for word — what the
/// reader says when the structure is not one it can build.
/// </para>
/// <para>
/// It is written before the work rather than after it because those sentences are public: a
/// consumer reads them in a <c>FormatException</c> or a <c>FixParseError</c>, and a rewrite that
/// changes them changes what their logs say. A test written afterwards records what the new code
/// does; this one records what the old code promised.
/// </para>
/// </remarks>
public sealed class FixBuilderContractTests
{
	// ── the three sentences the builder refuses with ─────────────────────────────────────────

	/// <summary>A count that is not a number the entries could match.</summary>
	[Fact]
	public void A_count_that_cannot_be_a_count_is_named_as_one()
	{
		Assert.False(FixParser.TryParseMessage(
			FixWire("35=D|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=X|453=nine|448=P|447=D|452=1|"),
			out _, out var error));

		Assert.NotNull(error);
		Assert.Equal("Invalid or impossible NumInGroup.", error!.Reason);
		Assert.Equal(453, error.Tag);
	}

	/// <summary>A count larger than the fields that follow it.</summary>
	[Fact]
	public void A_count_larger_than_what_remains_is_the_same_refusal()
	{
		Assert.False(FixParser.TryParseMessage(
			FixWire("35=D|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=X|453=400|448=P|447=D|452=1|"),
			out _, out var error));

		Assert.NotNull(error);
		Assert.Equal("Invalid or impossible NumInGroup.", error!.Reason);
	}

	/// <summary>An entry that does not open with the tag its group is cut by.</summary>
	[Fact]
	public void An_entry_that_does_not_open_with_its_delimiter_is_named_as_one()
	{
		Assert.False(FixParser.TryParseMessage(
			FixWire("35=D|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=X|453=2|448=P|447=D|452=1|"),
			out _, out var error));

		Assert.NotNull(error);
		Assert.Equal("Group entry must begin with its schema delimiter; NumInGroup is not satisfied.", error!.Reason);
		Assert.Equal(453, error.Tag);
	}

	/// <summary>A field no scope of the message will take.</summary>
	/// <remarks>
	/// It has to come after a trailer field: the body takes a tag nobody lists, so a field is only
	/// left over once the trailer has started, and the trailer takes nothing it does not list.
	/// </remarks>
	[Fact]
	public void A_field_no_scope_takes_is_named_as_one()
	{
		Assert.False(FixParser.TryParseMessage(
			FixWire("35=0|49=S|56=T|34=1|52=20260920-12:00:00|112=T|93=3|89=abc|58=late|"),
			out _, out var error));

		Assert.NotNull(error);
		Assert.Equal("Field is not permitted in this message scope.", error!.Reason);
	}

	// ── what an unknown message type gets ────────────────────────────────────────────────────

	/// <summary>
	/// A type the package does not describe comes back whole, flat, and in the order it was written.
	/// </summary>
	/// <remarks>
	/// The schema has nothing to say about it, so nothing is cut into groups and nothing is refused
	/// for being out of place: the header and the trailer are the standard's and the rest is the
	/// body as it stood on the wire. This is the behaviour anything built to replace the reader has
	/// to reproduce, and it is easy to lose, because it is what happens when the schema is silent
	/// rather than something the schema says.
	/// </remarks>
	[Fact]
	public void An_unknown_type_keeps_every_field_flat_and_in_order()
	{
		var wire    = FixWire("35=ZZ|49=S|56=T|34=1|52=20260920-12:00:00|453=1|448=P|447=D|9999=x|58=note|");
		var message = FixParser.ParseMessage(wire);

		Assert.Equal("ZZ", message.MessageType);
		Assert.IsType<FixMessage.Custom>(message);

		// The body holds what is not the standard header or trailer, in wire order.
		Assert.Equal([453, 448, 447, 9999, 58], message.Fields.Select(one => one.Tag).ToArray());

		// 453 counts a group everywhere the schema knows it, and here it counts nothing: the
		// schema does not describe this type, so there is no group to cut.
		Assert.Empty(message.GetGroup(453));

		// And the header is still read as the header.
		Assert.Equal("S", message.Header.SenderCompID);
	}

	/// <summary>
	/// The same message read as fields and read as a message hold the same tags in the same order.
	/// </summary>
	[Fact]
	public void An_unknown_type_loses_nothing_the_field_reader_found()
	{
		var wire = FixWire("35=ZZ|49=S|56=T|34=1|52=20260920-12:00:00|453=1|448=P|9999=x|");

		var flat = FixParser.ParseFields(wire).Select(one => one.Tag).Where(one => one != 8 && one != 9 && one != 10);
		var built = FixParser.ParseMessage(wire).AllFields.Select(one => one.Tag).Where(one => one != 8 && one != 9 && one != 10);

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
