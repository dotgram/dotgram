using System;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// A dictionary loaded over a context: from a string, from a reader, from a stream, and one over
/// another.
/// </summary>
public sealed class FixLoadTests
{
	const string Logon = "35=A|49=S|56=T|34=1|52=20260915-12:00:00|98=0|108=30|";
	const string Order = "35=D|49=S|56=T|34=1|52=20260915-12:00:00|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=1|40=1|";

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
	public void A_fragment_adds_to_what_the_standard_asks()
	{
		var context = FixContext.Default.Load(LogonWantsReset);

		// The standard still asks for 98 and 108, and the venue asks for 141 as well.
		var bare = FixParser.ParseMessage(FixFixtures.Wire("A", "35=A|49=S|56=T|34=1|52=20260915-12:00:00|".Substring(5)));

		Assert.False(bare.Validate(context));
		Assert.Equal(
			[98, 108, 141],
			bare.InvalidFindings!.Where(one => one.Rule == FixRule.RequiredFieldMissing).Select(one => one.Tag).OrderBy(tag => tag).ToArray());

		// A message that has what the venue wants is as valid as it was.
		var full = FixParser.ParseMessage(FixFixtures.Wire("A", Logon.Substring(5) + "141=Y|"));

		Assert.True(full.Validate(context));
	}

	[Fact]
	public void The_context_loaded_over_is_unchanged()
	{
		var before  = FixContext.Default;
		var after   = before.Load(LogonWantsReset);
		var message = FixParser.ParseMessage(FixFixtures.Wire("A", Logon.Substring(5)));

		Assert.NotSame(before, after);
		Assert.True(message.Validate(before));
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon.Substring(5))).Validate(after));
	}

	[Fact]
	public void Loads_compose_one_over_another()
	{
		const string andHeartbeat =
			"""
			<fix>
			  <messages>
			    <message name="Logon" msgtype="A">
			      <field name="MaxMessageSize" required="Y" />
			    </message>
			  </messages>
			</fix>
			""";

		var context = FixContext.Default.Load(LogonWantsReset).Load(andHeartbeat);
		var message = FixParser.ParseMessage(FixFixtures.Wire("A", Logon.Substring(5)));

		Assert.False(message.Validate(context));
		Assert.Equal(
			[141, 383],
			message.InvalidFindings!.Select(one => one.Tag).OrderBy(tag => tag).ToArray());
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

		Assert.True(FixParser.ParseMessage(FixFixtures.Wire("D", Order.Substring(5).Replace("40=1|", "40=Z|"))).Validate(context));
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("D", Order.Substring(5).Replace("40=1|", "40=2|"))).Validate(context));

		// And the standard is what it was.
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("D", Order.Substring(5).Replace("40=1|", "40=Z|"))).Validate(FixContext.Default));
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
		var order   = FixParser.ParseMessage(FixFixtures.Wire("D", Order.Substring(5)));
		var quote   = FixParser.ParseMessage(FixFixtures.Wire("S", "117=Q|55=ABC|"));

		Assert.False(order.Validate(context));
		Assert.False(quote.Validate(context));
		Assert.Equal(48, Assert.Single(order.InvalidFindings!).Tag);
		Assert.Equal(48, Assert.Single(quote.InvalidFindings!).Tag);
	}

	[Fact]
	public void A_reader_and_a_stream_load_what_a_string_does()
	{
		var message = FixParser.ParseMessage(FixFixtures.Wire("A", Logon.Substring(5)));

		using var reader = new StringReader(LogonWantsReset);
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(LogonWantsReset));

		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon.Substring(5))).Validate(FixContext.Default.Load(reader)));
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon.Substring(5))).Validate(FixContext.Default.Load(stream)));
		Assert.True(message.Validate(FixContext.Default));
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

	[Fact]
	public void The_whole_of_the_reference_dictionary_loads_over_the_standard()
	{
		// QuickFIX's own FIX 4.4, the file the stand compares against, read whole. It asks for
		// more than the repository in places, and every one of those is a check added, not a
		// refusal at the door.
		var path    = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "Fix", "FIX44.xml");
		var context = FixContext.Default.Load(File.ReadAllText(path));
		var order   = FixParser.ParseMessage(FixFixtures.Wire("D", Order.Substring(5)));

		Assert.True(order.Validate(context), string.Join("; ", order.InvalidFindings ?? []));
	}
}
