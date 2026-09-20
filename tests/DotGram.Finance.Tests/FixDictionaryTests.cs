using System;
using System.IO;
using System.Linq;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The dictionary reader, against a dictionary somebody else published and against ones written
/// here.
/// </summary>
/// <remarks>
/// <para>
/// Two instruments, and they answer different questions. `tests/Corpus/Fix/FIX44.xml` is
/// QuickFIX's published file, byte for byte: 916 tags, 24 components, 59 group names over 226
/// sites and 93 message types is not a shape anyone here would have invented, and it is the only
/// thing that holds the reader to what the format actually contains rather than to what this
/// session imagined it contains.
/// </para>
/// <para>
/// The written ones do the opposite: they hold a case the published file has no example of, and
/// they can be wrong on purpose. A refusal cannot be tested against a file that is correct.
/// </para>
/// </remarks>
public sealed class FixDictionaryTests
{
	static string Published =>
		Path.Combine(Root(AppContext.BaseDirectory), "tests", "Corpus", "Fix", "FIX44.xml");

	static string Root(string from)
	{
		var at = new DirectoryInfo(from);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? from;
	}

	static FixDictionary Load() => FixDictionary.Load(File.OpenRead(Published));

	/// <summary>A dictionary around members, with the fields section last as the published file has it.</summary>
	static string Written(string header, string messages, string components, string fields) =>
		$"""
		<fix major="4" minor="4">
			<header>{header}</header>
			<trailer><field name="CheckSum" required="Y"/></trailer>
			<messages>{messages}</messages>
			<components>{components}</components>
			<fields>
				<field number="10" name="CheckSum" type="STRING"/>
				{fields}
			</fields>
		</fix>
		""";

	[Fact]
	public void The_published_dictionary_reads()
	{
		var dictionary = Load();

		Assert.Equal("FIX.4.4", dictionary.Version);
		Assert.Equal(916, dictionary.Tags.Count);
		Assert.Equal(93, dictionary.MessageTypes.Count);
	}

	[Fact]
	public void A_tag_carries_its_name_its_type_and_its_code_set()
	{
		var dictionary = Load();

		Assert.Equal("ClOrdID", dictionary.Name(11));
		Assert.Equal("STRING", dictionary.Type(11));
		Assert.Null(dictionary.Codes(11));

		Assert.Equal("Side", dictionary.Name(54));
		Assert.Equal("CHAR", dictionary.Type(54));
		Assert.Contains("1", dictionary.Codes(54)!);
		Assert.Contains("2", dictionary.Codes(54)!);
	}

	[Fact]
	public void A_message_carries_its_name()
	{
		var dictionary = Load();

		Assert.Equal("NewOrderSingle", dictionary.MessageName("D"));
		Assert.Equal("Heartbeat", dictionary.MessageName("0"));
		Assert.Null(dictionary.MessageName("not a type"));
	}

	/// <summary>
	/// The compiled tables and the published dictionary describe one schema, and disagree about
	/// ten tags.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the staleness guard seen from inside: it does not say which side is right, it says
	/// that the difference between them is the one recorded here. A new disagreement fails it, and
	/// so does settling an old one — which is the point, because settling one should change this
	/// list in the same commit that changes the tables.
	/// </para>
	/// <para>
	/// <strong>Neither side is authority.</strong> QuickFIX's file is one reading of FIX 4.4 and
	/// ours is another; what settles a row is the published specification, which nobody here has
	/// consulted for these ten. Two of them look like the other side's defect rather than ours —
	/// MiscFeeType has values 10, 11 and 12, which cannot be CHAR, and NoRpts counts messages in a
	/// response rather than entries in a group — but "looks like" is not a reading of the
	/// specification either, and the rows stay listed until one is done.
	/// </para>
	/// </remarks>
	[Fact]
	public void The_published_dictionary_agrees_with_the_compiled_tables_but_for_ten_tags()
	{
		var dictionary = Load();

		var wrong = dictionary.Tags
			.Where(tag => FixSchema.Defines(tag) && dictionary.Type(tag) != null)
			.Where(tag => !string.Equals(dictionary.Type(tag), FixSchema.Type(tag), StringComparison.OrdinalIgnoreCase))
			.OrderBy(tag => tag)
			.Select(tag => $"{tag} {dictionary.Name(tag)}: the file says {dictionary.Type(tag)} and the tables say {FixSchema.Type(tag)}")
			.ToArray();

		Assert.Equal(Disagreements, wrong);
	}

	static readonly string[] Disagreements =
	[
		"82 NoRpts: the file says NUMINGROUP and the tables say int",
		"139 MiscFeeType: the file says CHAR and the tables say String",
		"239 RepoCollateralSecurityType: the file says INT and the tables say String",
		"243 UnderlyingRepoCollateralSecurityType: the file says INT and the tables say String",
		"250 LegRepoCollateralSecurityType: the file says INT and the tables say String",
		"532 MassCancelRejectReason: the file says CHAR and the tables say int",
		"534 NoAffectedOrders: the file says INT and the tables say NumInGroup",
		"576 NoClearingInstructions: the file says INT and the tables say NumInGroup",
		"580 NoDates: the file says INT and the tables say NumInGroup",
		"674 LegAllocAcctIDSource: the file says STRING and the tables say int",
	];

	[Fact]
	public void Every_tag_the_tables_define_is_in_the_published_dictionary()
	{
		var dictionary = Load();

		var missing = Enumerable.Range(1, FixSchema.TagLimit - 1)
			.Where(FixSchema.Defines)
			.Where(tag => dictionary.Name(tag) == null)
			.ToArray();

		Assert.Empty(missing);
	}

	// ── what the published file has no example of ────────────────────────────────────────────

	[Fact]
	public void A_member_naming_a_field_the_dictionary_never_declares_is_refused()
	{
		var text = Written(
			"""<field name="CheckSum" required="Y"/>""",
			"""<message name="Order" msgtype="D"><field name="NotDeclared" required="Y"/></message>""",
			"", "");

		var refused = Assert.Throws<FormatException>(() => FixDictionary.Parse(text));

		Assert.Contains("NotDeclared", refused.Message);
		Assert.Contains("the message 'Order'", refused.Message);
	}

	[Fact]
	public void A_member_naming_a_component_the_dictionary_never_declares_is_refused()
	{
		var text = Written(
			"""<field name="CheckSum" required="Y"/>""",
			"""<message name="Order" msgtype="D"><component name="NotDeclared" required="Y"/></message>""",
			"", "");

		Assert.Contains("NotDeclared", Assert.Throws<FormatException>(() => FixDictionary.Parse(text)).Message);
	}

	[Fact]
	public void One_name_given_to_two_tags_is_refused()
	{
		var text = Written(
			"""<field name="CheckSum" required="Y"/>""", "", "",
			"""
			<field number="11" name="Same" type="STRING"/>
			<field number="12" name="Same" type="STRING"/>
			""");

		var refused = Assert.Throws<FormatException>(() => FixDictionary.Parse(text));

		Assert.Contains("'Same'", refused.Message);
		Assert.Contains("11", refused.Message);
		Assert.Contains("12", refused.Message);
	}

	[Fact]
	public void Two_messages_of_one_type_are_refused()
	{
		var text = Written(
			"""<field name="CheckSum" required="Y"/>""",
			"""
			<message name="First" msgtype="D"/>
			<message name="Second" msgtype="D"/>
			""", "", "");

		var refused = Assert.Throws<FormatException>(() => FixDictionary.Parse(text));

		Assert.Contains("'First'", refused.Message);
		Assert.Contains("'Second'", refused.Message);
	}

	/// <summary>
	/// The format has two presences and Orchestra has five; a third value is refused, not guessed.
	/// </summary>
	[Fact]
	public void A_presence_the_format_does_not_have_is_refused()
	{
		var text = Written(
			"""<field name="CheckSum" required="Y"/>""",
			"""<message name="Order" msgtype="D"><field name="ClOrdID" required="ignored"/></message>""",
			"",
			"""<field number="11" name="ClOrdID" type="STRING"/>""");

		var refused = Assert.Throws<FormatException>(() => FixDictionary.Parse(text));

		Assert.Contains("ignored", refused.Message);
		Assert.Contains("only Y and N", refused.Message);
	}

	[Fact]
	public void A_group_with_no_members_is_refused()
	{
		var text = Written(
			"""<field name="CheckSum" required="Y"/>""",
			"""<message name="Order" msgtype="D"><group name="NoParties" required="N"></group></message>""",
			"",
			"""<field number="453" name="NoParties" type="NUMINGROUP"/>""");

		Assert.Contains("no entry could begin", Assert.Throws<FormatException>(() => FixDictionary.Parse(text)).Message);
	}

	[Fact]
	public void A_file_that_is_not_a_dictionary_is_refused()
	{
		Assert.Throws<FormatException>(() => FixDictionary.Parse("<notfix/>"));
		Assert.Throws<FormatException>(() => FixDictionary.Parse("""<fix major="4" minor="4"/>"""));
	}

	[Fact]
	public void A_null_input_is_refused_rather_than_answered()
	{
		Assert.Throws<ArgumentNullException>(() => FixDictionary.Load((Stream)null!));
		Assert.Throws<ArgumentNullException>(() => FixDictionary.Load((TextReader)null!));
		Assert.Throws<ArgumentNullException>(() => FixDictionary.Parse(null!));
	}

	/// <summary>A dictionary that declares no version but the two numbers still names one.</summary>
	[Fact]
	public void The_version_is_built_from_what_the_root_carries()
	{
		Assert.Equal("FIX.4.4", FixDictionary.Parse(Written("", "", "", "")).Version);
		Assert.Equal("FIXT.1.1", FixDictionary.Parse(
			"""<fix type="FIXT" major="1" minor="1"><fields><field number="10" name="CheckSum" type="STRING"/></fields></fix>""").Version);
	}
}
