using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Finance.Fix;

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

		Assert.True(order.Validate(FixContext.Default));
		Assert.True(order.IsValid);
		Assert.Null(order.InvalidFindings);
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

			message.Validate(FixContext.Default);

			foreach (var one in message.InvalidFindings ?? [])
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
		// A NewOrderSingle without its instrument, its quantity or its side.
		var order = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|60=20260915-12:00:00|40=2|"));

		Assert.False(order.Validate(FixContext.Default));
		Assert.Equal(
			[(FixRule.RequiredFieldMissing, 54), (FixRule.RequiredComponentMissing, 55), (FixRule.RequiredComponentMissing, 38)],
			order.InvalidFindings!.Select(one => (one.Rule, one.Tag)).ToArray());
	}

	[Fact]
	public void A_count_that_disagrees_with_its_entries_is_named_by_the_counter()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=3|448=P1|447=D|452=1|"));

		Assert.False(order.Validate(FixContext.Default));

		var finding = Assert.Single(order.InvalidFindings!);

		Assert.Equal(FixRule.GroupCountMismatch, finding.Rule);
		Assert.Equal(453, finding.Tag);
		Assert.Same(order.Fields.First(field => field.Tag == 453), finding.Field);
	}

	/// <summary>An entry is held to what the repository requires of it, and the finding names the entry.</summary>
	[Fact]
	public void A_required_field_of_an_entry_is_named_with_the_entry_and_where_it_began()
	{
		// A NewOrderList: each order in the list must carry a Side and a ListSeqNo; the second has neither.
		var list = (FixMessage.NewOrderList)FixParser.ParseMessage(FixFixtures.Wire("E",
			"66=L1|394=1|68=2|73=2|11=O1|67=1|54=1|55=ABC|38=1|40=1|60=20260915-12:00:00|11=O2|55=ABC|38=1|40=1|60=20260915-12:00:00|"));

		Assert.False(list.Validate(FixContext.Default));
		Assert.Equal(
			[(FixRule.RequiredFieldMissing, 54, 1), (FixRule.RequiredFieldMissing, 67, 1)],
			list.InvalidFindings!.Select(one => (one.Rule, one.Tag, one.EntryIndex)).OrderBy(one => one.Tag).ToArray());
		Assert.All(list.InvalidFindings!, one => Assert.Equal(list.ListOrdGrp![1].ClOrdID.Position, one.Position));
	}

	/// <summary>A count inside an entry is held to the entries that follow it there.</summary>
	[Fact]
	public void A_nested_count_is_held_within_its_entry()
	{
		var order = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|453=1|448=P1|802=2|523=S1|"));

		Assert.False(order.Validate(FixContext.Default));

		var finding = Assert.Single(order.InvalidFindings!);

		Assert.Equal(FixRule.GroupCountMismatch, finding.Rule);
		Assert.Equal(802, finding.Tag);
	}
	/// <summary>
	/// A block is checked once and the check reaches every type that carries it: this replaces what
	/// an Instrument must have, and two message types that were not touched answer differently.
	/// </summary>
	[Fact]
	public void One_block_slot_answers_for_every_carrier()
	{
		var strict = new FixContext
		{
			Validators = new FixValidators
			{
				Instrument = (context, message, instrument) =>
				{
					if (instrument.SecurityID is null)
						message.AddFinding(new FixFinding(FixRule.RequiredFieldMissing, 48, 0, null, -1));

					return message.IsValid;
				},
			},
		};

		var order  = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|"));
		var quote  = FixParser.ParseMessage(FixFixtures.Wire("S", "117=Q|55=ABC|"));

		Assert.True(order.Validate(FixContext.Default));
		Assert.False(quote.Validate(FixContext.Default) == false);

		var strictOrder = FixParser.ParseMessage(FixFixtures.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|"));
		var strictQuote = FixParser.ParseMessage(FixFixtures.Wire("S", "117=Q|55=ABC|"));

		Assert.False(strictOrder.Validate(strict));
		Assert.False(strictQuote.Validate(strict));
		Assert.Equal(48, Assert.Single(strictOrder.InvalidFindings!).Tag);
		Assert.Equal(48, Assert.Single(strictQuote.InvalidFindings!).Tag);
	}

	[Fact]
	public void A_message_is_validated_once()
	{
		var counted = 0;

		var context = new FixContext
		{
			Validators = new FixValidators
			{
				Heartbeat = (ctx, message) =>
				{
					counted++;

					return message.IsValid;
				},
			},
		};

		var heartbeat = FixParser.ParseMessage(FixFixtures.Wire("0", "112=TEST|"));

		Assert.True(heartbeat.Validate(context));
		Assert.True(heartbeat.Validate(context));
		Assert.Equal(1, counted);
	}
}
