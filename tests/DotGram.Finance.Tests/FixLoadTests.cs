using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// A dictionary loaded over a context: from a string, from a reader, from a stream, and one over
/// another.
/// </summary>
public sealed class FixLoadTests
{
	const string Logon = "98=0|108=30|";
	const string Order = "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|";

	/// <summary>A fragment that says what one venue adds, and nothing the standard already says.</summary>
	const string LogonWantsReset =
		"""
		<fix>
		  <messages>
		    <message name="Logon" msgtype="A">
		      <field name="ResetSeqNumFlag" required="Y" />
		    </message>
		  </messages>
		</fix>
		""";

	[Fact]
	public void A_fragment_replaces_what_the_standard_asks_of_a_type()
	{
		var context = FixContext.Default.Load(LogonWantsReset);

		// The fragment is the whole of what a Logon is now asked: 141, and no longer 98 or 108.
		var bare = FixParser.ParseMessage(FixFixtures.Wire("A", ""));

		Assert.False(bare.Validate(context));
		Assert.Equal(
			[141],
			bare.InvalidFindings!.Where(one => one.Rule == FixRule.RequiredFieldMissing).Select(one => one.Tag).OrderBy(tag => tag).ToArray());

		// A fragment that wants the standard as well says so.
		const string logonWantsAll =
			"""
			<fix>
			  <messages>
			    <message name="Logon" msgtype="A">
			      <field name="EncryptMethod" required="Y" />
			      <field name="HeartBtInt" required="Y" />
			      <field name="ResetSeqNumFlag" required="Y" />
			    </message>
			  </messages>
			</fix>
			""";

		var all = FixParser.ParseMessage(FixFixtures.Wire("A", ""));

		Assert.False(all.Validate(FixContext.Default.Load(logonWantsAll)));
		Assert.Equal(
			[98, 108, 141],
			all.InvalidFindings!.Where(one => one.Rule == FixRule.RequiredFieldMissing).Select(one => one.Tag).OrderBy(tag => tag).ToArray());

		// A message that has what the fragment wants is valid under it.
		Assert.True(FixParser.ParseMessage(FixFixtures.Wire("A", Logon + "141=Y|")).Validate(context));
	}

	[Fact]
	public void The_context_loaded_over_is_unchanged()
	{
		var before  = FixContext.Default;
		var after   = before.Load(LogonWantsReset);
		var message = FixParser.ParseMessage(FixFixtures.Wire("A", Logon));

		Assert.NotSame(before, after);
		Assert.True(message.Validate(before));
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(after));
	}

	[Fact]
	public void The_later_load_replaces_the_earlier_for_the_same_type()
	{
		const string andMaxMessageSize =
			"""
			<fix>
			  <messages>
			    <message name="Logon" msgtype="A">
			      <field name="MaxMessageSize" required="Y" />
			    </message>
			  </messages>
			</fix>
			""";

		var context = FixContext.Default.Load(LogonWantsReset).Load(andMaxMessageSize);
		var message = FixParser.ParseMessage(FixFixtures.Wire("A", Logon));

		// Only the second fragment speaks for a Logon now; the first spoke for it until then.
		Assert.False(message.Validate(context));
		Assert.Equal([383], message.InvalidFindings!.Select(one => one.Tag).ToArray());
	}

	[Fact]
	public void A_field_fragment_replaces_the_values_a_field_may_hold()
	{
		// The venue adds Z to OrdType and drops 2 from it.
		const string ordType =
			"""
			<fix>
			  <fields>
			    <field number="40" name="OrdType" type="CHAR">
			      <value enum="1" description="MARKET" />
			      <value enum="Z" description="VENUE" />
			    </field>
			  </fields>
			</fix>
			""";

		var context = FixContext.Default.Load(ordType);

		Assert.True(FixParser.ParseMessage(FixFixtures.Wire("D", Order.Replace("40=1|", "40=Z|"))).Validate(context));
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("D", Order.Replace("40=1|", "40=2|"))).Validate(context));

		// And the standard is what it was.
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("D", Order.Replace("40=1|", "40=Z|"))).Validate(FixContext.Default));
	}

	[Fact]
	public void A_block_fragment_reaches_every_carrier()
	{
		const string instrument =
			"""
			<fix>
			  <components>
			    <component name="Instrument">
			      <field name="SecurityID" required="Y" />
			    </component>
			  </components>
			</fix>
			""";

		var context = FixContext.Default.Load(instrument);
		var order   = FixParser.ParseMessage(FixFixtures.Wire("D", Order));
		var quote   = FixParser.ParseMessage(FixFixtures.Wire("S", "117=Q|55=ABC|"));

		Assert.False(order.Validate(context));
		Assert.False(quote.Validate(context));
		Assert.Equal(48, Assert.Single(order.InvalidFindings!).Tag);
		Assert.Equal(48, Assert.Single(quote.InvalidFindings!).Tag);
	}

	[Fact]
	public void A_reader_and_a_stream_load_what_a_string_does()
	{
		var message = FixParser.ParseMessage(FixFixtures.Wire("A", Logon));

		using var reader = new StringReader(LogonWantsReset);
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(LogonWantsReset));

		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(FixContext.Default.Load(reader)));
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(FixContext.Default.Load(stream)));
		Assert.True(message.Validate(FixContext.Default));
	}

	[Fact]
	public void A_fragment_describing_a_group_replaces_the_check_of_its_entries()
	{
		// The venue wants a PartyRole in every party. The parties are a component, and what a party
		// holds is said where the component is described, which is where every carrier finds it.
		const string partiesWantRole =
			"""
			<fix>
			  <components>
			    <component name="Parties">
			      <group name="NoPartyIDs" required="N">
			        <field name="PartyID" required="Y" />
			        <field name="PartyRole" required="Y" />
			      </group>
			    </component>
			  </components>
			</fix>
			""";

		var context = FixContext.Default.Load(partiesWantRole);
		var order   = FixParser.ParseMessage(FixFixtures.Wire("D", Order + "453=2|448=P1|452=1|448=P2|"));

		Assert.False(order.Validate(context));

		var finding = Assert.Single(order.InvalidFindings!);

		Assert.Equal(FixRule.RequiredFieldMissing, finding.Rule);
		Assert.Equal(452, finding.Tag);
		Assert.Equal(1, finding.EntryIndex);
		Assert.Equal(((FixMessage.NewOrderSingle)order).NoPartyIDsGroups![1].PartyID.Position, finding.Position);
	}
	[Fact]
	public void What_a_load_wrote_can_be_kept_as_files()
	{
		var directory = Path.Combine(Path.GetTempPath(), "dotgram-fix-load-" + Guid.NewGuid().ToString("N"));

		try
		{
			FixContext.Default.Load(LogonWantsReset, directory);

			var written = Directory.GetFiles(directory, "*.el").Select(Path.GetFileName).ToArray();

			Assert.Equal(["Logon.el"], written);
			Assert.Contains("(FixContext context, FixMessage.Logon message) =>", File.ReadAllText(Path.Combine(directory, "Logon.el")), StringComparison.Ordinal);
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}
	[Fact]
	public void A_type_this_package_has_no_class_for_is_refused_by_name()
	{
		const string venueOnly =
			"""
			<fix>
			  <messages>
			    <message name="VenueHeartbeat" msgtype="U1">
			      <field name="TestReqID" required="Y" />
			    </message>
			  </messages>
			</fix>
			""";

		var refused = Assert.Throws<FormatException>(() => FixContext.Default.Load(venueOnly));

		Assert.Contains("VenueHeartbeat", refused.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// QuickFIX/n's own FIX 4.4, read whole, does not load alone: it places fields where FIX 4.4 does
	/// not, the checks written from it name a property the model does not have, and the refusal says
	/// which. The body of a QuoteRequestReject is the first such place in the file.
	/// </summary>
	[Fact]
	public void The_reference_dictionary_alone_names_where_it_departs_from_the_standard()
	{
		var refused = Assert.Throws<FormatException>(() => FixContext.Default.Load(File.ReadAllText(Path.Combine(Corpus, "FIX44.xml"))));

		Assert.Contains("could not be compiled", refused.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// The errata: what QuickFIX/n's FIX44.xml places differently from the repository, said the way the
	/// repository says it, in that file's format (tests/Corpus/Fix/quickfixn-fix44-errata.xml). It loads
	/// alone, and read as one with their file it puts the thirteen types back: a standard message of
	/// those types is held to what the standard's messages over their fields hold it to.
	/// </summary>
	[Fact]
	public void The_errata_read_with_the_reference_dictionary_puts_the_thirteen_types_back()
	{
		var errata = File.ReadAllText(Path.Combine(Corpus, "quickfixn-fix44-errata.xml"));

		FixContext.Default.Load(errata);
		FixContext.Default.LoadFile(Path.Combine(Corpus, "quickfixn-fix44-errata.xml"));

		var file   = File.ReadAllText(Path.Combine(Corpus, "FIX44.xml"));
		var mended = FixContext.Default.Load([file, errata]);
		var order  = FixParser.ParseMessage(FixFixtures.Wire("D", Order));

		Assert.True(order.Validate(mended), string.Join("; ", order.InvalidFindings ?? []));

		// What the errata leaves alone is their fields: the values their file lists for a field stand,
		// and differ from the repository's here and there (SymbolSfx, YieldRedemptionPriceType). So the
		// context a mended message is held against is the standard's messages over their fields.
		var from        = file.IndexOf("<fields>", StringComparison.Ordinal);
		var to          = file.IndexOf("</fields>", StringComparison.Ordinal) + "</fields>".Length;
		var theirFields = FixContext.Default.Load("<fix>" + file.Substring(from, to - from) + "</fix>");

		var types = new[]
		{
			"QuoteRequestReject", "CrossOrderCancelReplaceRequest", "AllocationInstruction", "AllocationReport",
			"SettlementInstructions", "TradeCaptureReport", "AssignmentReport", "CollateralRequest",
			"CollateralAssignment", "CollateralResponse", "CollateralReport", "CollateralInquiry", "CollateralInquiryAck",
		};

		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var data in FixFixtures.Messages())
		{
			var (name, wire) = ((string)data[0], (string)data[1]);

			if (Array.IndexOf(types, name) < 0)
				continue;

			var standard = FixParser.ParseMessage(wire);
			var repaired = FixParser.ParseMessage(wire);

			standard.Validate(theirFields);
			repaired.Validate(mended);

			Assert.Equal(
				(standard.InvalidFindings ?? []).Select(one => one.ToString()).Order(),
				(repaired.InvalidFindings ?? []).Select(one => one.ToString()).Order());

			seen.Add(name);
		}

		Assert.Equal(types.OrderBy(one => one, StringComparer.Ordinal), seen.OrderBy(one => one, StringComparer.Ordinal));
	}

	static readonly string Corpus = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "Fix");
}
