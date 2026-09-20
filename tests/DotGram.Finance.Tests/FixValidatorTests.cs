using System;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The validation layer: everything wrong with a built message, and where.
/// </summary>
/// <remarks>
/// D53 makes validation a layer over a message rather than a mode of building one, and D69 puts
/// the verb on the message: a message is asked whether it is right, against a validator or against
/// the schema this package compiles in. This holds the layer to what the strict mode checked
/// before it, one rule at a time, and to the two things the layer does that a mode could not:
/// answering with every finding at once, and naming which entry of a repeating group a finding is
/// in.
/// </remarks>
public sealed class FixValidatorTests
{
	/// <summary>A message whose framing is right by construction, so a finding is never arithmetic.</summary>
	static string Framed(string body)
	{
		var fields = body.Replace('|', '\u0001');
		var head   = "8=FIX.4.4\u00019=" + fields.Length + "\u0001";
		var sum    = 0;

		foreach (var octet in Encoding.Latin1.GetBytes(head + fields))
			sum += octet;

		return head + fields + "10=" + (sum % 256).ToString("D3") + "\u0001";
	}

	/// <summary>Built leniently, because what is being tested is the layer and not the building.</summary>
	static FixMessage Message(string body)
	{
		Assert.True(
			FixMessages.TryParse(Framed(body), out var message, out var error,
				new FixParseOptions(FixFraming.Wire, FixParseMode.Lenient)),
			error?.Reason);

		return message!;
	}

	const string Header = "35=D|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|";
	const string Order  = "11=ORDER123|21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|";

	[Fact]
	public void A_message_that_is_right_gives_nothing()
	{
		Assert.Empty(Message(Header + Order).Validate());
	}

	[Fact]
	public void Asking_with_no_validator_asks_the_standard_one()
	{
		var message = Message(Header + Order.Replace("11=ORDER123|", ""));

		Assert.Equal(message.Validate(), message.Validate(FixValidator.Standard));
	}

	[Fact]
	public void A_null_validator_is_refused_rather_than_answered()
	{
		Assert.Throws<ArgumentNullException>(() => Message(Header + Order).Validate(null!));
	}

	[Fact]
	public void The_standard_validator_is_one_object_for_every_caller()
	{
		Assert.Same(FixValidator.Standard, FixValidator.Standard);
	}

	[Fact]
	public void A_required_field_that_is_absent_is_named_by_its_tag()
	{
		var found = Message(Header + Order.Replace("11=ORDER123|", "")).Validate();

		var missing = Assert.Single(found, one => one.Rule == FixRule.RequiredFieldMissing);

		Assert.Equal(11, missing.Tag);
		Assert.Equal(FixScope.Body, missing.Scope);
	}

	/// <summary>
	/// A component is an id, and a consumer cannot look an id up, so the finding names a tag anyway.
	/// </summary>
	/// <remarks>
	/// Symbol is not a field of NewOrderSingle: it is the first field of the Instrument component,
	/// which the message requires. Removing it therefore empties the component rather than leaving
	/// a field out of it, and the schema's own answer is that the component is missing. The rule is
	/// the right one; the tag in the sentence is what makes it usable.
	/// </remarks>
	[Fact]
	public void A_required_component_with_none_of_its_fields_names_the_first_of_them()
	{
		var found = Message(Header + Order.Replace("55=AAPL|", "")).Validate();

		var missing = Assert.Single(found, one => one.Rule == FixRule.RequiredComponentMissing);

		Assert.Null(missing.Tag);
		Assert.Contains("tag 55", missing.Reason);
	}

	[Fact]
	public void A_tag_twice_in_one_scope_is_a_duplicate()
	{
		var found = Message(Header + Order + "59=0|").Validate();

		Assert.Contains(found, one => one.Rule == FixRule.DuplicateField && one.Tag == 59);
	}

	[Fact]
	public void A_value_outside_its_code_set_is_named()
	{
		var found = Message(Header + Order.Replace("54=1|", "54=Z|")).Validate();

		var wrong = Assert.Single(found, one => one.Rule == FixRule.InvalidValue);

		Assert.Equal(54, wrong.Tag);
	}

	[Fact]
	public void An_unknown_message_type_is_a_finding_and_not_a_refusal()
	{
		var found = Message("35=ZZ|49=S|56=T|34=1|52=20260920-12:00:00|").Validate();

		Assert.Contains(found, one => one.Rule == FixRule.UnknownMessageType);
	}

	[Fact]
	public void Every_finding_comes_back_and_not_the_first()
	{
		// Two faults at once: the order has no identifier and the side is not one of its code set.
		var found = Message(
			Header + Order.Replace("11=ORDER123|", "").Replace("54=1|", "54=Z|")).Validate();

		Assert.Contains(found, one => one.Rule == FixRule.RequiredFieldMissing && one.Tag == 11);
		Assert.Contains(found, one => one.Rule == FixRule.InvalidValue        && one.Tag == 54);
	}

	[Fact]
	public void A_finding_inside_a_group_names_the_group_and_the_entry()
	{
		// Two parties, and the second one's role is not a number.
		var parties = "453=2|448=A|447=D|452=1|448=B|447=D|452=nine|";
		var found   = Message(Header + Order + parties).Validate();

		var wrong = Assert.Single(found, one => one.Rule == FixRule.InvalidValue && one.Tag == 452);

		Assert.Equal(453, wrong.GroupTag);
		Assert.Equal(1, wrong.EntryIndex);
		Assert.Contains("453[1]", wrong.ToString());
	}

	[Fact]
	public void Fields_of_a_group_entry_out_of_the_schema_order_are_named_with_the_entry()
	{
		var parties = "453=2|448=A|447=D|452=1|447=D|448=B|452=2|";
		var found   = Message(Header + Order + parties).Validate();

		var wrong = Assert.Single(found, one => one.Rule == FixRule.FieldOutOfOrder);

		Assert.Equal(453, wrong.GroupTag);
		Assert.Equal(0, wrong.EntryIndex);
	}

	/// <summary>
	/// The one shape of a count mismatch that building does not refuse first.
	/// </summary>
	/// <remarks>
	/// A count that disagrees with the entries after it cannot reach this layer: building cuts the
	/// entries by the count, so it refuses the wire before a message exists, leniently too. What
	/// survives is a required group that announces no entries at all, which is well formed as a
	/// wire and wrong as a message. MarketDataRequest requires NoMDEntryTypes; this says it has
	/// none.
	/// </remarks>
	[Fact]
	public void A_required_group_that_announces_no_entries_is_a_finding()
	{
		var request = "35=V|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|" +
		              "262=REQ1|263=1|264=0|267=0|146=1|55=AAPL|";

		var wrong = Assert.Single(Message(request).Validate(), one => one.Rule == FixRule.GroupCountMismatch);

		Assert.Equal(267, wrong.Tag);
	}

	/// <summary>
	/// The move itself: what the strict mode refuses, the layer finds.
	/// </summary>
	/// <remarks>
	/// This is the assertion that makes D53 safe to land in two steps. Until building stops
	/// refusing (1b), the strict mode and the layer are two readings of one schema, and they must
	/// agree about every message: anything the mode turns away, the layer has something to say
	/// about.
	/// </remarks>
	[Theory]
	[InlineData("a required field is gone", Header, "21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|")]
	[InlineData("a required component is empty", Header, "11=ORDER123|21=1|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|")]
	[InlineData("a value is not of its code set", Header, "11=ORDER123|21=1|55=AAPL|54=Z|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|")]
	[InlineData("a tag appears twice", Header, Order + "59=0|")]
	public void What_the_strict_mode_refuses_the_layer_finds(string shape, string header, string body)
	{
		var wire = Framed(header + body);

		var strict = FixMessages.TryParse(wire, out _, out var error, new FixParseOptions(FixParseMode.Strict));

		Assert.False(strict, $"{shape}: the strict mode accepted it, so there is nothing to compare.");

		var found = Message(header + body).Validate();

		Assert.True(found.Length > 0,
			$"{shape}: the strict mode refused it ({error!.Reason}) and the layer found nothing.");
	}
}
