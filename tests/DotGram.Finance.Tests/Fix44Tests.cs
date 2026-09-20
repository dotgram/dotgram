using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class Fix44Tests
{
	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void Every_standard_message_has_a_typed_result(string name, string wire)
	{
		Assert.True(FixMessages.TryParse(wire, out var message, out var error), error?.ToString());
		Assert.Equal(name, message!.GetType().Name);
		Assert.Same(wire, message.OriginalWire);
	}

	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void Semantic_assembler_reconstructs_every_message_from_flat_fields(string name, string wire)
	{
		var original = FixMessages.Parse(wire);
		var flat = original.AllFields.Select(f => new FixNode(f.Tag, f.Position, f.ValuePosition, f.Length)).ToArray();
		Assert.True(FixSemantics.TryBuild(wire, original.MessageType, flat, null, out var rebuilt, out var error), error?.ToString());
		Assert.Equal(name, rebuilt!.GetType().Name);
		Assert.Equal(original.AllFields.Select(f => f.Wire.ToString()), rebuilt.AllFields.Select(f => f.Wire.ToString()));
		Compare(original.Header, rebuilt.Header);
		Compare(original, rebuilt);
		Compare(original.Trailer, rebuilt.Trailer);

		static void Compare(FixFieldSet expected, FixFieldSet actual)
		{
			Assert.Equal(expected.GetType(), actual.GetType());
			Assert.Equal(expected.Fields.Select(f => f.Tag), actual.Fields.Select(f => f.Tag));
			foreach (var field in expected.Fields)
			{
				var left = expected.GetGroup(field.Tag);
				var right = actual.GetGroup(field.Tag);
				Assert.Equal(left.Count, right.Count);
				for (var n = 0; n < left.Count; n++) Compare(left[n], right[n]);
			}
		}
	}

	[Fact]
	public void Public_order_api_and_optional_values()
	{
		var order = Assert.IsType<NewOrderSingle>(FixMessages.Parse(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|")));
		Assert.Equal("ABC", order.Symbol);
		Assert.True(order.OrderQty!.Value.TryGetDecimal(out var quantity));
		Assert.Equal(100m, quantity);
		Assert.Null(order.Account);
	}

	[Fact]
	public void Body_fields_may_be_reordered()
	{
		Assert.True(FixMessages.TryParse(FixFixtures.Wire("D", "40=1|38=100|60=20260915-12:00:00|54=1|55=ABC|11=ORDER|"), out _, out var error), error?.ToString());
	}

	[Fact]
	public void Nested_groups_preserve_entry_boundaries()
	{
		var message = FixMessages.Parse(FixFixtures.Wire("D", "11=ORDER|453=2|448=P1|447=D|452=1|802=2|523=S1|803=1|523=S2|803=2|448=P2|447=D|452=3|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|"));
		var parties = message.GetGroup(453);
		Assert.Equal(2, parties.Count);
		Assert.Equal("P2", parties[1].GetField(448)!.Value.ToString());
		Assert.Equal(2, parties[0].GetGroup(802).Count);
		var order = Assert.IsType<NewOrderSingle>(message);
		Assert.Equal("P1", order.Parties[0].GetField(448)!.Value.ToString());
		Assert.Equal("S2", order.Parties[0].GetGroup(802)[1].GetField(523)!.Value.ToString());
	}

	[Fact]
	public void Every_truncated_prefix_is_rejected_without_throwing()
	{
		var wire = FixFixtures.Wire("0", "112=TEST|");
		for (var length = 0; length < wire.Length; length++)
		{
			Assert.False(FixMessages.TryParse(wire[..length], out var message, out var error));
			Assert.Null(message);
			Assert.NotNull(error);
		}
	}

	/// <summary>What breaks the schema builds, and the layer names it (D53).</summary>
	[Theory]
	[InlineData("1", "", FixRule.RequiredFieldMissing)]
	[InlineData("0", "112=A|112=B|", FixRule.DuplicateField)]
	[InlineData("A", "98=999|108=30|", FixRule.InvalidValue)]
	[InlineData("D", "11=X|55=ABC|54=Z|60=20260915-12:00:00|38=1|40=1|", FixRule.InvalidValue)]
	[InlineData("D", "11=X|55=ABC|54=1|60=20260230-12:00:00|38=1|40=1|", FixRule.InvalidValue)]
	public void Invalid_schema_builds_and_is_reported(string type, string body, FixRule rule)
	{
		Assert.True(FixMessages.TryParse(FixFixtures.Wire(type, body), out var message, out var error), error?.ToString());
		Assert.Contains(message!.Validate(), f => f.Rule == rule);
	}

	/// <summary>A count that does not match the entries after it is still recognition.</summary>
	[Fact]
	public void A_group_with_fewer_entries_than_its_count_is_rejected()
	{
		var wire = FixFixtures.Wire("D", "11=X|453=2|448=P1|447=D|452=1|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|");

		Assert.False(FixMessages.TryParse(wire, out _, out var error));
		Assert.NotNull(error);
		Assert.Equal("D", error.MessageType);
	}

	[Fact]
	public void Unknown_fields_are_preserved_in_order_and_reported()
	{
		var wire = FixFixtures.Wire("0", "9001=A|112=TEST|9002=B|");

		Assert.True(FixMessages.TryParse(wire, out var result, out var error), error?.ToString());
		Assert.Equal(new[] { 9001, 112, 9002 }, result!.Fields.Select(f => f.Tag));

		// The strict mode refused this message. The reader builds it and the layer names the tags.
		Assert.Equal(
			new[] { 9001, 9002 },
			result.Validate().Where(f => f.Rule == FixRule.FieldNotInScope).Select(f => f.Tag!.Value));
	}

	[Fact]
	public void Raw_payload_keeps_embedded_delimiters_and_octets()
	{
		var raw = "a\u0001=\0\u00ff";
		var body = "98=0\u0001108=30\u000195=" + raw.Length + "\u000196=" + raw + "\u0001";
		var wire = FixFixtures.Wire("A", body);
		var message = FixMessages.Parse(wire);
		Assert.Equal(raw, message.GetField(96)!.Value.ToString());
		Assert.Equal(wire, message.OriginalWire);
	}

	[Fact]
	public void Framing_errors_identify_the_field()
	{
		var wire = FixFixtures.Wire("0", "");
		Assert.False(FixMessages.TryParse(wire[..^4] + "999\u0001", out _, out var checksum));
		Assert.Equal(10, checksum!.Tag);
		Assert.False(FixMessages.TryParse(wire.Replace("9=", "9=1", StringComparison.Ordinal), out _, out var length));
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
		Assert.False(FixMessages.TryParse(FixFixtures.Wire("A", body), out _, out _));
	}

	[Fact]
	public void Empty_raw_payload_is_preserved()
	{
		Assert.Equal("", FixMessages.Parse(FixFixtures.Wire("A", "98=0|108=30|95=0|96=|")).GetField(96)!.Value.ToString());
	}

	[Fact]
	public void Custom_fields_inside_groups_are_preserved()
	{
		var wire = FixFixtures.Wire("D", "11=ORDER|453=1|448=P1|9001=X|447=D|452=1|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|");
		Assert.True(FixMessages.TryParse(wire, out var message, out var error), error?.ToString());
		Assert.Equal("X", message!.GetGroup(453)[0].GetField(9001)!.Value.ToString());
		Assert.Equal(wire, string.Concat(message.AllFields.Select(f => f.Tag.ToString(CultureInfo.InvariantCulture) + "=" + f.Value.ToString() + "\u0001")));
	}

	[Fact]
	public void Group_fields_out_of_the_schema_order_build_and_are_a_finding()
	{
		var wire = FixFixtures.Wire("D", "11=ORDER|453=1|448=P1|452=1|447=D|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|");

		// The strict mode refused this message; the order of a group's fields is now a finding.
		Assert.True(FixMessages.TryParse(wire, out var message, out var error), error?.ToString());
		Assert.Contains(message!.Validate(), f => f.Rule == FixRule.FieldOutOfOrder && f.GroupTag == 453);
	}

	[Fact]
	public void Vendor_message_types_have_a_lossless_result()
	{
		var wire = FixFixtures.Wire("U1", "9001=X|9002=Y|");

		// The strict mode refused an unknown MsgType; it is now a finding about a built message.
		Assert.True(FixMessages.TryParse(wire, out var result, out var error), error?.ToString());
		Assert.IsType<CustomFixMessage>(result);
		Assert.Equal(wire, result!.OriginalWire);
		Assert.Contains(result.Validate(), f => f.Rule == FixRule.UnknownMessageType);
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
			FixMessages.TryParse(FixFixtures.Wire("0", new string(body)), out _, out _);
		}
	}

	[Fact]
	public void Unregistered_binary_pairs_are_not_inferred()
	{
		var wire = FixFixtures.Wire("0", "9000=3\u00019001=A\u0001B\u0001");

		// Nothing declares 9000 to measure 9001, so the SOH inside the value ends the field and
		// the wire does not read. That is recognition, not schema, and stays a refusal.
		Assert.False(FixMessages.TryParse(wire, out _, out _));
	}

	[Fact]
	public void Encoded_fields_without_message_encoding_are_a_finding()
	{
		var order = FixFixtures.Wire("D", "11=X|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|354=1|355=X|");

		// The strict mode refused this with tag 347; the layer names the same tag.
		Assert.True(FixMessages.TryParse(order, out var message, out var error), error?.ToString());
		Assert.Contains(message!.Validate(), f => f.Rule == FixRule.MessageEncodingMissing && f.Tag == 347);
	}

	[Fact]
	public void Fixtures_exercise_every_standard_field_and_preserve_every_wire_extent()
	{
		var tags = new HashSet<int>();
		foreach (var data in FixFixtures.Messages())
		{
			var wire = (string)data[1];
			var message = FixMessages.Parse(wire);
			var fields = message.AllFields.ToArray();
			Assert.Equal(wire, string.Concat(fields.Select(f => f.Wire.ToString())));
			foreach (var field in fields) tags.Add(field.Tag);
		}
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
			var wire = (string)data[1];
			var fields = FixMessages.Parse(wire).AllFields.ToArray();
			for (var i = 1; i < fields.Length; i++)
			{
				var current = fields[i];
				var lengthTag = FixSchema.LengthTag(current.Tag);
				if (lengthTag == 0 || !covered.Add(current.Tag)) continue;
				var preceding = fields[i - 1];
				Assert.Equal(lengthTag, preceding.Tag);
				var changed = wire[..preceding.Position] + lengthTag + "=5\u0001" + current.Tag + "=" + payload + "\u0001" + wire[(current.ValuePosition + current.Length + 1)..];
				var result = FixMessages.Parse(Reframe(changed));
				Assert.Equal(payload, result.AllFields.First(f => f.Tag == current.Tag).Value.ToString());
			}
		}
		Assert.Equal(16, covered.Count);
	}

	[Fact]
	public void Every_reachable_group_rejects_an_excess_declared_count()
	{
		var covered = new HashSet<int>();
		foreach (var data in FixFixtures.Messages())
		{
			var wire = (string)data[1];
			var message = FixMessages.Parse(wire);
			foreach (var scope in new FixFieldSet[] { message.Header, message })
				foreach (var node in GroupNodes(scope))
				{
					if (node.Entries!.Count == 0 || !covered.Add(node.GroupId)) continue;
					var changed = wire[..node.ValuePosition] + "2" + wire[(node.ValuePosition + node.Length)..];
					Assert.False(FixMessages.TryParse(Reframe(changed), out _, out _), node.GroupId.ToString());
				}
		}
		Assert.Equal(91, covered.Count);
	}

	static IEnumerable<FixNode> GroupNodes(FixFieldSet scope)
	{
		foreach (var node in scope.Nodes)
			if (node.Entries != null)
			{
				yield return node;
				foreach (var entry in node.Entries)
					foreach (var child in GroupNodes(entry)) yield return child;
			}
	}

	static string Reframe(string wire)
	{
		var start = wire.IndexOf('\u0001', 12) + 1;
		var body = wire.Substring(start, wire.Length - start - 7);
		var prefix = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var checksum = prefix.Aggregate(0, (sum, c) => (sum + c) & 255);
		return prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
