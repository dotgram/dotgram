using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

// What held a message to the schema is being rebuilt, so the tests that asked for findings are
// not here: a required field absent, a tag twice in one scope, a value outside its code set, the
// fields of a group out of order, an unknown MsgType, an Encoded field without MessageEncoding,
// and a group count that disagrees with the entries after it. Each of those is a rule of FixRule
// and comes back with Validate(context). What the reading itself answers is what these still ask.
public sealed class Fix44Tests
{
	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void Every_standard_message_has_a_typed_result(string name, string wire)
	{
		Assert.True(FixParser.TryParseMessage(wire, out var message, out var error), error?.ToString());
		Assert.Equal(name, message!.GetType().Name);
	}

	/// <summary>
	/// A tag the message has no place for is said so where it is read, and a standard message has
	/// no such tag — except in three fixtures, which are listed here rather than passed over.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The three were found by this check and each was chased to the two published readings of FIX
	/// 4.4 that the repository holds. They are named as (type, tag) so that a change on either side
	/// fails: a fixture corrected, a case added, or a reading revised.
	/// </para>
	/// <para>
	/// <c>586 OrigOrdModTime</c> in a <c>NewOrderCross</c> is placed by neither the FIX repository
	/// nor QuickFIX, so that one is the fixture. The other two are a disagreement between them:
	/// QuickFIX places <c>586</c> in the sides of a <c>CrossOrderCancelReplaceRequest</c> and
	/// <c>635 ClearingFeeIndicator</c> in the sides of a <c>TradeCaptureReport</c>, and the
	/// repository places neither. This package reads the repository.
	/// </para>
	/// </remarks>
	[Fact]
	public void No_standard_fixture_carries_a_tag_its_message_cannot_place()
	{
		var unplaced = new List<(string Type, int Tag)>();

		foreach (var data in FixFixtures.Messages())
		{
			var message = FixParser.ParseMessage((string)data[1]);

			foreach (var finding in message.InvalidFindings ?? [])
				if (finding.Rule == FixRule.FieldNotInScope)
					unplaced.Add(((string)data[0], finding.Tag));
		}

		Assert.Equal(
			[("CrossOrderCancelReplaceRequest", 586), ("NewOrderCross", 586), ("TradeCaptureReport", 635)],
			unplaced.OrderBy(one => one.Tag).ThenBy(one => one.Type, StringComparer.Ordinal).ToArray());
	}

	[Fact]
	public void A_tag_the_message_has_no_place_for_is_a_finding()
	{
		var message = FixParser.ParseMessage(FixFixtures.Wire("0", "112=TEST|9001=A|44=1.5|"));

		Assert.False(message.IsValid);
		Assert.Equal(
			[9001, 44],
			message.InvalidFindings!.Where(one => one.Rule == FixRule.FieldNotInScope).Select(one => one.Tag).ToArray());
	}

	[Fact]
	public void Public_order_api_and_optional_values()
	{
		var order = Assert.IsType<FixMessage.NewOrderSingle>(FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|")));

		Assert.Equal("ABC", order.Symbol!.Value);
		Assert.Equal(100m,  order.OrderQty!.Value);
		Assert.Null(order.Account);
	}

	[Fact]
	public void Body_fields_may_be_reordered()
	{
		Assert.True(FixParser.TryParseMessage(FixFixtures.Wire("D", "40=1|38=100|60=20260915-12:00:00|54=1|55=ABC|11=ORDER|"), out _, out var error), error?.ToString());
	}

	[Fact]
	public void Nested_groups_preserve_entry_boundaries()
	{
		var order = Assert.IsType<FixMessage.NewOrderSingle>(FixParser.ParseMessage(
			FixFixtures.Wire("D", "11=ORDER|453=2|448=P1|447=D|452=1|802=2|523=S1|803=1|523=S2|803=2|448=P2|447=D|452=3|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|")));

		Assert.Equal(2,    order.Parties!.Count);
		Assert.Equal("P1", order.Parties[0].PartyID.Value);
		Assert.Equal("P2", order.Parties[1].PartyID.Value);
		Assert.Equal(2,    order.Parties[0].PtysSubGrp!.Count);
		Assert.Equal("S2", order.Parties[0].PtysSubGrp![1].PartySubID.Value);
	}

	/// <summary>
	/// The counter of a group inside a group belongs to the entry that holds it, so two entries
	/// carrying the same nested group each keep their own count.
	/// </summary>
	[Fact]
	public void A_nested_count_belongs_to_the_entry_it_counts_in()
	{
		var order = Assert.IsType<FixMessage.NewOrderSingle>(FixParser.ParseMessage(
			FixFixtures.Wire("D", "11=ORDER|453=2|448=P1|802=2|523=S1|803=1|523=S2|803=2|448=P2|802=1|523=S3|803=3|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|")));

		Assert.Equal(2, (int)order.Parties![0].NoPartySubIDs!.Value);
		Assert.Equal(1, (int)order.Parties[1].NoPartySubIDs!.Value);
		Assert.Equal(2, order.Parties[0].PtysSubGrp!.Count);
		Assert.Single(order.Parties[1].PtysSubGrp!);
		Assert.Equal("S3", order.Parties[1].PtysSubGrp![0].PartySubID.Value);
	}

	[Fact]
	public void Every_truncated_prefix_is_rejected_without_throwing()
	{
		var wire = FixFixtures.Wire("0", "112=TEST|");

		for (var length = 0; length < wire.Length; length++)
		{
			Assert.False(FixParser.TryParseMessage(wire[..length], out var message, out var error));
			Assert.Null(message);
			Assert.NotNull(error);
		}
	}

	[Fact]
	public void Unknown_fields_are_preserved_in_order()
	{
		var wire = FixFixtures.Wire("0", "9001=A|112=TEST|9002=B|");

		Assert.True(FixParser.TryParseMessage(wire, out var result, out var error), error?.ToString());
		Assert.Equal(new[] { 9001, 112, 9002 }, Body(result!).Select(f => f.Tag));
	}

	[Fact]
	public void Raw_payload_keeps_embedded_delimiters_and_octets()
	{
		var raw     = "a\u0001=\0\u00ff";
		var wire    = FixFixtures.Wire("A", "98=0\u0001108=30\u000195=" + raw.Length + "\u000196=" + raw + "\u0001");
		var message = FixParser.ParseMessage(wire);

		Assert.Equal(raw, Encoding.Latin1.GetString(Assert.IsType<FixField.RawData>(Field(message, 96)).Value.Span));
	}

	[Fact]
	public void Framing_errors_identify_the_field()
	{
		var wire = FixFixtures.Wire("0", "");

		Assert.False(FixParser.TryParseMessage(wire[..^4] + "999\u0001", out _, out var checksum));
		Assert.Equal(10, checksum!.Tag);
		Assert.False(FixParser.TryParseMessage(wire.Replace("9=", "9=1", StringComparison.Ordinal), out _, out var length));
		Assert.Equal(9, length!.Tag);
	}

	[Theory]
	[InlineData("98=0|108=30|95=3|")]
	[InlineData("98=0|108=30|95=2|96=ABC|")]
	[InlineData("98=0|108=30|95=999999999999999999999999|96=A|")]
	[InlineData("98=0|108=30|96=A|")]
	public void Invalid_length_data_pairs_are_refused(string body)
	{
		// A length and the data it measures are how the reader knows where a field ends, so this
		// stays a refusal and does not become a finding: there is no message to have one about.
		Assert.False(FixParser.TryParseMessage(FixFixtures.Wire("A", body), out _, out _));
	}

	[Fact]
	public void Empty_raw_payload_is_preserved()
	{
		var message = FixParser.ParseMessage(FixFixtures.Wire("A", "98=0|108=30|95=0|96=|"));

		Assert.Equal(0, Assert.IsType<FixField.RawData>(Field(message, 96)).Value.Length);
	}

	[Fact]
	public void Custom_fields_inside_groups_are_preserved()
	{
		var wire = FixFixtures.Wire("D", "11=ORDER|453=1|448=P1|9001=X|447=D|452=1|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|");

		Assert.True(FixParser.TryParseMessage(wire, out var message, out var error), error?.ToString());

		// A tag the schema does not place has no property to sit in, so the flat list is where it
		// survives; the entry around it is still read.
		var order = Assert.IsType<FixMessage.NewOrderSingle>(message);

		Assert.Equal("P1", order.Parties![0].PartyID.Value);
		Assert.Contains(order.Fields, f => f.Tag == 9001);
	}

	[Fact]
	public void Vendor_message_types_are_read_as_Custom()
	{
		var wire = FixFixtures.Wire("U1", "9001=X|9002=Y|");

		Assert.True(FixParser.TryParseMessage(wire, out var result, out var error), error?.ToString());

		var custom = Assert.IsType<FixMessage.Custom>(result);

		Assert.Equal("U1", custom.MessageType);
		Assert.Equal(new[] { 9001, 9002 }, Body(custom).Select(f => f.Tag));
	}

	[Fact]
	public void Adversarial_field_syntax_does_not_throw()
	{
		var random = new Random(20260915);
		const string alphabet = "0123456789=|-+.ABC";

		for (var n = 0; n < 1000; n++)
		{
			var body = new char[random.Next(1, 150)];

			for (var i = 0; i < body.Length; i++) body[i] = alphabet[random.Next(alphabet.Length)];

			FixParser.TryParseMessage(FixFixtures.Wire("0", new string(body)), out _, out _);
		}
	}

	[Fact]
	public void Unregistered_binary_pairs_are_not_inferred()
	{
		var wire = FixFixtures.Wire("0", "9000=3\u00019001=A\u0001B\u0001");

		// Nothing declares 9000 to measure 9001, so the SOH inside the value ends the field and
		// the wire does not read. That is recognition, not schema, and stays a refusal.
		Assert.False(FixParser.TryParseMessage(wire, out _, out _));
	}

	[Fact]
	public void Fixtures_exercise_every_standard_field()
	{
		var tags = new HashSet<int>();

		foreach (var data in FixFixtures.Messages())
			foreach (var field in FixParser.ParseMessage((string)data[1]).Fields)
				tags.Add(field.Tag);

		using var cases = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "FieldCases.json")));

		var expected = cases.RootElement.GetProperty("tags").EnumerateArray().Select(x => x.GetInt32()).Order().ToArray();

		Assert.Equal(912, expected.Length);
		Assert.Equal(expected, tags.Order().ToArray());
	}

	[Fact]
	public void Every_standard_data_pair_accepts_embedded_delimiters()
	{
		var covered = new HashSet<int>();
		const string payload = "A\u0001=\0\u00ff";

		foreach (var data in FixFixtures.Messages())
		{
			var wire   = (string)data[1];
			var fields = FixParser.ParseMessage(wire).Fields;

			for (var i = 1; i < fields.Count; i++)
			{
				var current   = fields[i];
				var lengthTag = FixContext.Default.LengthTag(current.Tag);

				if (lengthTag == 0 || !covered.Add(current.Tag)) continue;

				var preceding = fields[i - 1];

				Assert.Equal(lengthTag, preceding.Tag);

				var changed = wire[..preceding.Position] + lengthTag + "=5\u0001" + current.Tag + "=" + payload + "\u0001" + wire[(current.ValuePosition + current.Length + 1)..];
				var result  = FixParser.ParseMessage(Reframe(changed));
				var built   = (FixField.Typed<ReadOnlyMemory<byte>>)result.Fields.First(f => f.Tag == current.Tag);

				Assert.Equal(payload, Encoding.Latin1.GetString(built.Value.Span));
			}
		}

		Assert.Equal(16, covered.Count);
	}

	/// <summary>The field with that tag, wherever the message put it.</summary>
	static FixField Field(FixMessage message, int tag)
	{
		return message.Fields.First(field => field.Tag == tag);
	}

	/// <summary>The fields a message carries of its own, without the standard header and trailer.</summary>
	static IEnumerable<FixField> Body(FixMessage message)
	{
		return message.Fields.Where(field => field.Tag is not (8 or 9 or 10 or 34 or 35 or 49 or 52 or 56));
	}

	static string Reframe(string wire)
	{
		var start    = wire.IndexOf('\u0001', 12) + 1;
		var body     = wire.Substring(start, wire.Length - start - 7);
		var prefix   = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var checksum = prefix.Aggregate(0, (sum, c) => (sum + c) & 255);

		return prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
