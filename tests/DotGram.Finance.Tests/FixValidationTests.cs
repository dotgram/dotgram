using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What a context answers, and what replacing one of its slots reaches.
/// </summary>
public sealed class FixValidationTests
{
	[Fact]
	public void A_message_of_the_standard_holds_to_the_standard()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|"));

		Assert.Empty(order.Validate(Fix44Context.Default));
		Assert.Empty(order.Findings);
	}

	/// <summary>Every fixture of the standard, held to the standard, reports these three and no more.</summary>
	/// <remarks>
	/// One hundred and eighty-six messages through every rule this package has: required fields,
	/// required blocks, the values a field is limited to, and every group count against its entries.
	/// What comes back is the three tags the fixtures place where the repository does not, which
	/// Fix44Tests names and chases to the two published readings. Nothing else — which is what makes
	/// this worth keeping: it is the whole layer asked at once, of input written before it existed.
	/// </remarks>
	[Fact]
	public void The_standard_fixtures_report_what_they_report()
	{
		var found = new List<(string Type, FixRule Rule, int Tag)>();

		foreach (var data in FixFixtures.Messages())
		{
			var message = FixParser.ParseMessage((string)data[1]);

			foreach (var one in message.Validate(Fix44Context.Default))
				found.Add(((string)data[0], one.Rule, one.Tag));
		}

		Assert.Equal(
			[
				"FieldNotInScope 586 in CrossOrderCancelReplaceRequest",
				"FieldNotInScope 586 in NewOrderCross",
				"FieldNotInScope 635 in TradeCaptureReport",
			],
			found.Select(one => one.Rule + " " + one.Tag + " in " + one.Type).Distinct().Order().ToArray());
	}

	[Fact]
	public void A_required_field_and_a_required_block_are_named_by_their_tags()
	{
		// A NewOrderSingle without its instrument, its quantity or its side, found in the order the
		// repository lists them: Instrument, Side, OrderQtyData.
		var order = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|60=20260915-12:00:00|40=2|"));

		var findings = order.Validate(Fix44Context.Default);

		Assert.Equal(
			[(FixRule.RequiredComponentMissing, FixTag.Symbol), (FixRule.RequiredFieldMissing, FixTag.Side), (FixRule.RequiredComponentMissing, FixTag.OrderQty)],
			findings.Select(one => (one.Rule, one.Tag)).ToArray());

		// The message has no place for any of them: no position, and no path.
		Assert.All(findings, one => Assert.Equal(-1, one.Position));
		Assert.All(findings, one => Assert.Empty(one.Path));
		Assert.Equal("tag 54: RequiredFieldMissing", findings[1].ToString());
	}

	[Fact]
	public void A_count_that_disagrees_with_its_entries_is_named_by_the_counter()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=3|448=P1|447=D|452=1|"));

		var finding = Assert.Single(order.Validate(Fix44Context.Default));

		Assert.Equal(FixRule.GroupCountMismatch, finding.Rule);
		Assert.Equal(FixTag.NoPartyIDs, finding.Tag);
		Assert.Same(order.Fields.First(field => field.Tag == FixTag.NoPartyIDs), finding.Field);
	}

	/// <summary>An entry is held to what the repository requires of it, and the finding names the entry.</summary>
	[Fact]
	public void A_required_field_of_an_entry_is_named_with_the_entry_and_where_it_began()
	{
		// A NewOrderList: each order in the list must carry a Side and a ListSeqNo; the second has neither.
		var list = (FixMessage.NewOrderList)FixParser.ParseMessage(FixFixtures.Wire("E",
			"66=L1|394=1|68=2|73=2|11=O1|67=1|54=1|55=ABC|38=1|40=1|60=20260915-12:00:00|11=O2|55=ABC|38=1|40=1|60=20260915-12:00:00|"));

		var findings = list.Validate(Fix44Context.Default);

		Assert.Equal(
			[(FixRule.RequiredFieldMissing, FixTag.Side), (FixRule.RequiredFieldMissing, FixTag.ListSeqNo)],
			findings.Select(one => (one.Rule, one.Tag)).OrderBy(one => one.Tag).ToArray());
		Assert.All(findings, one => Assert.Equal([new FixGroupStep(FixTag.NoOrders, 1)], one.Path));
		Assert.All(findings, one => Assert.Equal(list.NoOrdersGroups![1].ClOrdID.Position, one.Position));
	}

	/// <summary>A count inside an entry is held to the entries that follow it there.</summary>
	[Fact]
	public void A_nested_count_is_held_within_its_entry()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=1|448=P1|802=2|523=S1|"));

		var finding = Assert.Single(order.Validate(Fix44Context.Default));

		Assert.Equal(FixRule.GroupCountMismatch, finding.Rule);
		Assert.Equal(FixTag.NoPartySubIDs, finding.Tag);
		Assert.Equal([new FixGroupStep(FixTag.NoPartyIDs, 0)], finding.Path);
	}

	/// <summary>A finding deep in groups names every group and entry on the way to it, outermost first.</summary>
	[Fact]
	public void A_finding_inside_nested_groups_names_the_way_to_it()
	{
		// Two parties; the second's first sub-ID entry carries PartySubIDType twice.
		var order = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=2|448=P1|452=1|448=P2|452=3|802=1|523=S1|803=1|803=2|"));

		var finding = Assert.Single(order.Findings);

		Assert.Equal(FixRule.DuplicateField, finding.Rule);
		Assert.Equal([new FixGroupStep(FixTag.NoPartyIDs, 1), new FixGroupStep(FixTag.NoPartySubIDs, 0)], finding.Path);
		Assert.Equal($"453[1].802[0] tag 803 at {finding.Position}: DuplicateField", finding.ToString());
		Assert.Same(finding, Assert.Single(order.Validate(Fix44Context.Default)));

		// A bad value there is said with the same path, by the check rather than the reading.
		var value = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=2|448=P1|452=1|448=P2|452=3|802=1|523=S1|803=x|"));

		var invalid = Assert.Single(value.Validate(Fix44Context.Default));

		Assert.Equal(FixRule.InvalidValue, invalid.Rule);
		Assert.Equal("453[1].802[0]", string.Join(".", invalid.Path));
	}
	/// <summary>
	/// A block is checked once and the check reaches every type that carries it: this replaces what
	/// an Instrument must have, and two message types that were not touched answer differently.
	/// </summary>
	[Fact]
	public void One_block_slot_answers_for_every_carrier()
	{
		var strict = new Fix44Context
		{
			Validators = new FixValidator44
			{
				Instrument = (context, check, instrument) =>
				{
					if (instrument.SecurityID is null)
						FixValidator.Missing(check, FixTag.SecurityID);
				},
			},
		};

		var order  = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|"));
		var quote  = FixParser.ParseMessage(FixFixtures.Wire("S", "117=Q|55=ABC|"));

		Assert.Empty(order.Validate(Fix44Context.Default));
		Assert.Empty(quote.Validate(Fix44Context.Default));

		var strictOrder = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|"));
		var strictQuote = FixParser.ParseMessage(FixFixtures.Wire("S", "117=Q|55=ABC|"));

		Assert.Equal(FixTag.SecurityID, Assert.Single(strictOrder.Validate(strict)).Tag);
		Assert.Equal(FixTag.SecurityID, Assert.Single(strictQuote.Validate(strict)).Tag);
	}

	/// <summary>Validate keeps nothing on the message: every call asks again, of the context it is given.</summary>
	[Fact]
	public void A_message_is_validated_again_on_every_call_against_the_context_it_is_given()
	{
		var counted = 0;

		var context = new Fix44Context
		{
			Validators = new FixValidator44
			{
				Heartbeat = (ctx, check, message) =>
				{
					counted++;
				},
			},
		};

		var heartbeat = FixParser.ParseMessage(FixFixtures.Wire("0", "112=TEST|"));

		Assert.Empty(heartbeat.Validate(context));
		Assert.Empty(heartbeat.Validate(context));
		Assert.Equal(2, counted);

		// One message, two schemas: each call answers its own.
		var order  = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|"));
		var strict = Fix44Context.Default.Load(
			"""
			<fix>
			  <messages>
			    <message name="NewOrderSingle" msgtype="D">
			      <field name="Account" required="Y" />
			    </message>
			  </messages>
			</fix>
			""");

		Assert.Equal(FixTag.Account, Assert.Single(order.Validate(strict)).Tag);
		Assert.Empty(order.Validate(Fix44Context.Default));
		Assert.Equal(FixTag.Account, Assert.Single(order.Validate(strict)).Tag);
	}

	/// <summary>A path is a view: cast back to an array or a list it cannot be written, so no finding moves another.</summary>
	[Fact]
	public void A_finding_path_cannot_be_written_through_a_cast()
	{
		// A duplicate the reading finds and two bad values the check finds, all in party 1's sub-ID entry.
		var order = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=2|448=P1|452=1|448=P2|452=3|802=1|523=S1|803=1|803=2|"));
		var value = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=2|448=P1|452=1|448=P2|452=x|802=1|523=S1|803=x|"));

		foreach (var finding in order.Validate(Fix44Context.Default).Concat(value.Validate(Fix44Context.Default)))
		{
			Assert.NotEmpty(finding.Path);
			Assert.False(finding.Path is FixGroupStep[]);
			Assert.False(finding.Path is IList<FixGroupStep> { IsReadOnly: false });
			Assert.Throws<NotSupportedException>(() => ((IList<FixGroupStep>)finding.Path)[0] = new FixGroupStep(999, 99));
		}

		Assert.Equal("453[1].802[0]", string.Join(".", Assert.Single(order.Findings).Path));
		Assert.Equal("453[1].802[0]", string.Join(".", Assert.Single(order.Validate(Fix44Context.Default)).Path));
	}

	/// <summary>A field of an entry that was never begun is said in the entries that are open around it, and no deeper.</summary>
	[Fact]
	public void A_field_of_an_unopened_entry_names_the_entries_open_around_it()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=2|448=P1|452=1|448=P2|452=3|802=0|803=1|"));

		var finding = Assert.Single(order.Findings);

		Assert.Equal((FixRule.GroupCountMismatch, FixTag.PartySubIDType), (finding.Rule, finding.Tag));
		Assert.Equal([new FixGroupStep(FixTag.NoPartyIDs, 1)], finding.Path);

		// Outside every entry there is nothing open, and nothing to say.
		var bare = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|452=3|"));

		Assert.Empty(Assert.Single(bare.Findings).Path);
	}

	/// <summary>A finding of the whole message about a field inside an entry carries that entry's path.</summary>
	[Fact]
	public void A_finding_of_the_whole_message_names_the_entry_its_field_is_in()
	{
		// EncodedLegIssuerLen (618) in the second leg, followed by LegRatioQty (623) and not its data.
		var legs = FixParser.ParseMessage(FixFixtures.Wire("AB",
			"11=ML|54=1|55=ABC|60=20260915-12:00:00|40=1|555=2|600=L1|600=L2|618=1|623=1|"));

		var length = Assert.Single(legs.Validate(Fix44Context.Default), one => one.Rule == FixRule.LengthFieldNotBeforeData);

		Assert.Equal(FixTag.EncodedLegIssuerLen, length.Tag);
		Assert.Equal([new FixGroupStep(FixTag.NoLegs, 1)], length.Path);

		// EncodedLegIssuer with no MessageEncoding, in the first leg.
		var encoded = FixParser.ParseMessage(FixFixtures.Wire("AB",
			"11=ML|54=1|55=ABC|60=20260915-12:00:00|40=1|555=2|600=L1|618=2|619=ab|600=L2|"));

		var missing = Assert.Single(encoded.Validate(Fix44Context.Default), one => one.Rule == FixRule.MessageEncodingMissing);

		Assert.Equal([new FixGroupStep(FixTag.NoLegs, 0)], missing.Path);

		// The same rule about a field of the message itself has no path.
		var top = FixParser.ParseMessage(FixFixtures.Wire("B", "148=NEWS|354=3|355=abc|33=0|"));

		Assert.Empty(Assert.Single(top.Validate(Fix44Context.Default), one => one.Rule == FixRule.MessageEncodingMissing).Path);
	}

	/// <summary>A coded field whose text did not read is an invalid value, never an exception.</summary>
	[Theory]
	[InlineData("D", "11=X|55=X|54=XYZ|60=20260915-12:00:00|38=1|40=1|", FixTag.Side)]
	[InlineData("8", "37=O|17=E|150=XY|39=0|55=X|54=1|151=0|14=0|6=0|", FixTag.ExecType)]
	public void A_coded_value_that_did_not_read_is_a_finding(string type, string body, int tag)
	{
		var message = FixParser.ParseMessage(FixFixtures.Wire(type, body));

		Assert.Contains(message.Validate(Fix44Context.Default), one => one.Rule == FixRule.InvalidValue && one.Tag == tag);
	}

	/// <summary>The appending form adds what the other answers, after what the collection holds, and counts it.</summary>
	[Fact]
	public void Validate_adds_to_a_callers_collection_and_says_how_many()
	{
		// A tag the type has no place for, which reading finds, and a code OrdType does not list.
		var order    = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=Z|9001=A|"));
		var mine     = FixFinding.Of(FixRule.InvalidValue, 25005);
		var findings = new List<FixFinding> { mine };

		Assert.Equal(2, order.Validate(Fix44Context.Default, findings));
		Assert.Equal([FixRule.InvalidValue, FixRule.FieldNotInScope, FixRule.InvalidValue], findings.Select(one => one.Rule));
		Assert.Same(mine, findings[0]);
		Assert.Same(Assert.Single(order.Findings), findings[1]);
		Assert.Equal(findings.Skip(1).Select(one => one.ToString()), order.Validate(Fix44Context.Default).Select(one => one.ToString()));

		var valid = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|"));

		Assert.Equal(0, valid.Validate(Fix44Context.Default, findings));
		Assert.Equal(3, findings.Count);
	}

	/// <summary>What reading found is the message's: a caller reads it and cannot change it.</summary>
	[Fact]
	public void The_findings_of_the_reading_are_read_only()
	{
		var message = FixParser.ParseMessage(FixFixtures.Wire("0", "112=TEST|9001=A|"));

		Assert.Single(message.Findings);
		Assert.False(message.Findings is ICollection<FixFinding> { IsReadOnly: false });
		Assert.False(message.Findings is System.Collections.IList { IsReadOnly: false });

		var answer = message.Validate(Fix44Context.Default);

		Assert.NotSame(message.Findings, answer);
		Assert.Single(message.Findings);

		// Nor the path of one: a write through a cast would follow the message into every later Validate.
		var order = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=2|448=P1|452=1|448=P2|452=3|802=1|523=S1|803=1|803=2|"));
		var path  = Assert.Single(order.Findings).Path;

		Assert.False(path is FixGroupStep[]);
		Assert.Throws<NotSupportedException>(() => ((IList<FixGroupStep>)path)[0] = new FixGroupStep(999, 99));
		Assert.Equal("453[1].802[0]", string.Join(".", Assert.Single(order.Validate(Fix44Context.Default)).Path));
	}

	/// <summary>A message validated from many threads at once answers each of them alike.</summary>
	[Fact]
	public void One_message_validated_on_many_threads_answers_each_alike()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=Z|453=3|448=P1|452=1|448=P2|452=x|"));

		var expected = string.Join("; ", order.Validate(Fix44Context.Default));
		var answers  = new string[2000];

		Parallel.For(0, answers.Length, at =>
		{
			answers[at] = string.Join("; ", order.Validate(Fix44Context.Default));
		});

		Assert.Equal(3, order.Validate(Fix44Context.Default).Count);
		Assert.All(answers, one => Assert.Equal(expected, one));
	}

	/// <summary>Validating a valid message, groups and all, allocates nothing once the thread has validated one.</summary>
	[Fact]
	public void Validating_a_valid_message_allocates_nothing()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D",
			"11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=2|448=P1|452=1|448=P2|452=3|802=1|523=S1|803=1|"));

		var findings = new List<FixFinding>();

		Assert.Empty(order.Validate(Fix44Context.Default));
		Assert.Equal(0, order.Validate(Fix44Context.Default, findings));

		var before = GC.GetAllocatedBytesForCurrentThread();

		for (var i = 0; i < 100; i++)
		{
			order.Validate(Fix44Context.Default);
			order.Validate(Fix44Context.Default, findings);
		}

		Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
	}

	/// <summary>
	/// A finding names the field it is about, and the field's location says where in the input it
	/// is: what a consumer who has an error to show asks next.
	/// </summary>
	[Fact]
	public void The_field_a_finding_names_says_where_it_was_read()
	{
		var wire  = FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=Z|");
		var order = FixParser.ParseMessage(wire);

		IFixLocation where = Assert.Single(order.Validate(Fix44Context.Default)).Field!;

		Assert.Equal("40=Z", wire.Substring(where.Position, where.ValuePosition - where.Position + where.Length));
		Assert.Equal("Z", wire.Substring(where.ValuePosition, where.Length));
	}
}
