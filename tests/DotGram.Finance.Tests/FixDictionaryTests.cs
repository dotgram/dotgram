using System;
using System.IO;
using System.Linq;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

using Fix42 = DotGram.Finance.Fix.Fix42;
using Fix50 = DotGram.Finance.Fix.Fix50;

namespace DotGram.Finance.Tests;

/// <summary>
/// A dictionary as a value: read from a file, merged, edited in code, and applied to a context,
/// which takes what it says at that moment and compiles its checks in the background.
/// </summary>
public sealed class FixDictionaryTests
{
	const string Venue =
		"""
		<fix>
		  <messages>
		    <message name="Logon" msgtype="A">
		      <field name="EncryptMethod" required="Y" />
		      <field name="HeartBtInt" required="Y" />
		      <field name="ResetSeqNumFlag" required="Y" />
		    </message>
		  </messages>
		  <components>
		    <component name="Instrument">
		      <field name="Symbol" required="Y" />
		    </component>
		  </components>
		  <fields>
		    <field number="40" name="OrdType" type="CHAR">
		      <value enum="1" description="MARKET" />
		      <value enum="2" description="LIMIT" />
		    </field>
		  </fields>
		</fix>
		""";

	const string Logon = "98=0|108=30|";

	[Fact]
	public void A_file_is_read_into_its_parts()
	{
		var dictionary = FixDictionary.Parse(Venue);

		Assert.Equal(["A"], dictionary.Messages.Select(one => one.MsgType));
		Assert.Equal("Logon", dictionary.Messages["A"].Name);
		Assert.Equal(["EncryptMethod", "HeartBtInt", "ResetSeqNumFlag"], dictionary.Messages["A"].Members.Select(one => one.Name));
		Assert.All(dictionary.Messages["A"].Members, one => Assert.Equal(FixDictionaryMemberKind.Field, one.Kind));
		Assert.Equal(["Symbol"], dictionary.Components["Instrument"].Members.Select(one => one.Name));
		Assert.Equal("OrdType", dictionary.Fields[40].Name);
		Assert.Equal("CHAR", dictionary.Fields[40].Type);
		Assert.Equal(["1", "2"], dictionary.Fields[40].Codes);
	}

	[Fact]
	public void A_merge_replaces_what_the_later_one_describes_whole_and_shares_nothing()
	{
		var standard = FixDictionary.Parse(Venue);
		var over     = new FixDictionary();

		over.Messages.Add(new FixDictionaryMessage("A", "Logon") { Members = { FixDictionaryMember.Field("HeartBtInt", required: true) } });
		over.Fields[40] = new FixDictionaryField("OrdType", "CHAR") { Codes = { "2" } };

		var merged = standard.Merge(over);

		Assert.Equal(["HeartBtInt"], merged.Messages["A"].Members.Select(one => one.Name));
		Assert.Equal(["2"], merged.Fields[40].Codes);
		Assert.Equal(["Symbol"], merged.Components["Instrument"].Members.Select(one => one.Name));

		// Editing what a merge made edits neither of what it was made from.
		merged.Messages["A"].Members.Clear();
		merged.Components["Instrument"].Members.Clear();

		Assert.Equal(3, standard.Messages["A"].Members.Count);
		Assert.Single(over.Messages["A"].Members);
		Assert.Single(standard.Components["Instrument"].Members);
	}

	[Fact]
	public void A_dictionary_edited_in_code_is_what_the_context_holds_a_message_to()
	{
		var dictionary = FixDictionary.Parse(Venue);

		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(Fix44Context.Default.With(dictionary)));

		// The venue drops its rule for ResetSeqNumFlag: the same Logon now holds.
		dictionary.Messages["A"].Members.RemoveAt(2);

		Assert.True(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(Fix44Context.Default.With(dictionary)));
	}

	[Fact]
	public void What_the_dictionary_says_is_taken_when_it_is_applied()
	{
		var dictionary = FixDictionary.Parse(Venue);
		var context    = Fix44Context.Default.With(dictionary);

		dictionary.Messages["A"].Members.RemoveAt(2);

		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(context));
	}

	/// <summary>
	/// Applying returns before the checks are compiled; a message held to the context at once is held
	/// to the dictionary's check all the same, and after <c>Prepare</c> nothing is left to compile.
	/// </summary>
	[Fact]
	public void A_message_held_before_the_checks_are_ready_is_held_to_them()
	{
		var context = Fix44Context.Default.With(FixDictionary.Parse(Venue));

		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(context));

		context.Prepare();

		Assert.Equal(0, context.Checks.CompileDeferred());
		Assert.False(FixParser.ParseMessage(FixFixtures.Wire("A", Logon)).Validate(context));
	}

	/// <summary>
	/// What a compilation would refuse is asked of the model when the dictionary is applied, so that
	/// no check refuses to compile later, on some message's first reading. Held here over every
	/// dictionary the corpus has that loads: applied, and then every check compiled at once by
	/// <c>Prepare</c>, which surfaces a failure the background would leave to the message.
	/// </summary>
	[Fact]
	public void Every_check_of_a_dictionary_that_applies_compiles()
	{
		var fix44 = Fix44Context.Default.Load([Read("FIX44.xml"), Read("quickfixn-fix44-errata.xml")]);
		var fix42 = Fix42.Fix42Context.Default.Load(Read("FIX42.xml"));
		var fixt  = Fix50.Fix50Context.Default.Load(Read("FIXT11.xml"));

		fix44.Prepare();
		fix42.Prepare();
		fixt.Prepare();

		Assert.Equal(0, fix44.Checks.CompileDeferred());
		Assert.Equal(0, fix42.Checks.CompileDeferred());
		Assert.Equal(0, fixt.Checks.CompileDeferred());
	}

	[Fact]
	public void Prepare_on_a_context_with_no_dictionary_does_nothing()
	{
		Fix44Context.Default.Prepare();

		Assert.Equal(0, Fix44Context.Default.Checks.CompileDeferred());
	}

	[Fact]
	public void A_code_its_field_cannot_hold_is_refused_where_the_dictionary_is_applied()
	{
		var dictionary = FixDictionary.Parse(Venue);

		dictionary.Fields[40].Codes.Add("LIMIT");

		var refused = Assert.Throws<FormatException>(() => Fix44Context.Default.With(dictionary));

		Assert.Equal("The field 'OrdType' lists 'LIMIT', which is not a value of its type, Char.", refused.Message);
	}

	[Fact]
	public void A_name_edited_onto_two_tags_is_refused_where_the_dictionary_is_applied()
	{
		var dictionary = FixDictionary.Parse(Venue);

		dictionary.Fields[44] = new FixDictionaryField("OrdType", "PRICE");

		var refused = Assert.Throws<FormatException>(() => Fix44Context.Default.With(dictionary));

		Assert.Equal("The name 'OrdType' is given to tags 40 and 44.", refused.Message);
	}

	[Fact]
	public void A_member_the_model_does_not_carry_is_refused_where_the_dictionary_is_applied()
	{
		var dictionary = FixDictionary.Parse(Venue);

		dictionary.Messages["A"].Members.Add(FixDictionaryMember.Field("OrderQty"));

		var refused = Assert.Throws<FormatException>(() => Fix44Context.Default.With(dictionary));

		Assert.Equal("The dictionary places the field 'OrderQty' in 'FixMessage.Logon', which has no member of that name.", refused.Message);
	}

	static readonly string Corpus = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "Fix");

	static string Read(string name)
	{
		return File.ReadAllText(Path.Combine(Corpus, name));
	}
}
