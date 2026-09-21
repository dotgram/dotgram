using System;
using System.Collections.Generic;
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

	/// <summary>Built, which is now unconditional: the schema is read by the layer, not the reader.</summary>
	static FixMessage Message(string body)
	{
		Assert.True(FixParser.TryParseMessage(Framed(body), out var message, out var error), error?.Reason);

		return message!;
	}

	const string Header = "35=D|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|";
	const string Order  = "11=ORDER123|21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|";

	[Fact]
	public void A_message_that_is_right_gives_nothing()
	{
		Assert.Empty(Message(Header + Order).Validate());
	}

	/// <summary>
	/// A message is held to the rule its own class holds, and the class is the key: there is no
	/// table between the two and nothing to pass.
	/// </summary>
	[Fact]
	public void A_message_is_held_to_the_rule_of_its_own_class()
	{
		var message = Message(Header + Order.Replace("11=ORDER123|", ""));
		var found   = new List<FixFinding>();

		FixMessage.NewOrderSingle.Rule(message, found);

		Assert.Equal(message.Validate(), found);
	}

	/// <summary>
	/// What the package compiles in is reached by name, and the name is how a replacement is undone.
	/// </summary>
	/// <remarks>
	/// Equality, not identity: a delegate built from a static method group is cached at the place
	/// the conversion is written, so two places give two objects that are equal and not the same.
	/// The test below pins that, because it is what a consumer asking "is my rule still in place"
	/// has to know.
	/// </remarks>
	[Fact]
	public void Every_class_starts_at_the_rule_this_package_compiles_in()
	{
		Assert.Equal((FixMessageRule)FixValidator.ValidateNewOrderSingle, FixMessage.NewOrderSingle.Rule);
		Assert.Equal((FixMessageRule)FixValidator.ValidateHeartbeat, FixMessage.Heartbeat.Rule);
		Assert.Equal((FixMessageRule)FixValidator.ValidateCustom, FixMessage.Custom.Rule);
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
	/// The break, as a list: what the strict mode refused, the reader now builds and the layer
	/// reports.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every row here is a message that <c>FixParseMode.Strict</c> turned away and that
	/// <c>FixParser.ParseMessage</c> now returns. That is the behaviour change of D53, and this is the
	/// assertion that it lost nothing: the reader stopped refusing and the layer started saying,
	/// about the same input, with a rule a caller can act on.
	/// </para>
	/// <para>
	/// What is NOT here is as much of the statement as what is. Recognition still refuses:
	/// framing, <c>BodyLength</c>, <c>CheckSum</c>, a field the parser could not read, a
	/// length/data pair that does not measure, a <c>NumInGroup</c> that sizes an array past the
	/// fields left, and a group entry that does not begin with its delimiter. Those are how the
	/// reader finds where a message ends, so they cannot wait for one to exist.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("a required field is gone", FixRule.RequiredFieldMissing,
		"21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|")]
	[InlineData("a required component is empty", FixRule.RequiredComponentMissing,
		"11=ORDER123|21=1|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|")]
	[InlineData("a value is not of its code set", FixRule.InvalidValue,
		"11=ORDER123|21=1|55=AAPL|54=Z|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|")]
	[InlineData("a tag appears twice", FixRule.DuplicateField, Order + "59=0|")]
	[InlineData("a tag the schema does not define", FixRule.FieldNotInScope, Order + "9001=X|")]
	[InlineData("a tag the schema defines elsewhere", FixRule.FieldNotInScope, Order + "269=0|")]
	[InlineData("a group's fields are out of order", FixRule.FieldOutOfOrder,
		Order + "453=1|448=A|452=1|447=D|")]
	public void What_the_strict_mode_refused_is_now_built_and_reported(string shape, FixRule rule, string body)
	{
		// Message() asserts that it builds, which is the half of the break the reader owns.
		var found = Message(Header + body).Validate();

		Assert.True(
			Array.Exists(found, one => one.Rule == rule),
			$"{shape}: expected {rule}, got [{string.Join("; ", found)}].");
	}

	/// <summary>An unknown message type, which the strict mode refused before a message existed.</summary>
	[Fact]
	public void An_unknown_message_type_is_built_and_reported()
	{
		var message = Message("35=ZZ|49=S|56=T|34=1|52=20260920-12:00:00|");

		Assert.IsType<FixMessage.Custom>(message);
		Assert.Contains(message.Validate(), one => one.Rule == FixRule.UnknownMessageType);

		// And nothing else: there is no schema to be out of place against, so saying so of every
		// field would bury the one finding that matters.
		Assert.DoesNotContain(message.Validate(), one => one.Rule == FixRule.FieldNotInScope);
	}

	/// <summary>
	/// A rule is compared with <c>==</c>, never with <see cref="object.ReferenceEquals"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Written as a test because two accounts of WHY were offered and the compiler agreed with
	/// neither. "Every conversion of a method group builds a new object" is wrong — the conversion
	/// of a static method group is cached. "Cached at the place the conversion is written, so two
	/// places give two objects" is also wrong: the two conversions below are two places in one
	/// method and they came back as ONE object.
	/// </para>
	/// <para>
	/// What a consumer can rely on is therefore equality and not identity: a rule is the same rule
	/// when it is <c>==</c> to the name, and whether it is also the same object is the compiler's
	/// business and may differ between call sites, assemblies and versions.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_rule_is_compared_by_equality_and_not_by_identity()
	{
		FixMessageRule here  = FixValidator.ValidateNewOrderSingle;
		FixMessageRule there = FixValidator.ValidateNewOrderSingle;

		Assert.True(here == there);
		Assert.Equal(here, there);

		// And a rule wrapped in a lambda is a different rule, however equal its behaviour: what the
		// field holds is the delegate, and this is what a consumer's check would see.
		FixMessageRule wrapped = (message, findings) => FixValidator.ValidateNewOrderSingle(message, findings);

		Assert.NotEqual(here, wrapped);
	}
}
