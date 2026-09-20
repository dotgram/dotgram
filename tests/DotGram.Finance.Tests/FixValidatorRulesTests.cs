using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The rules are a table the consumer writes, and a dictionary is a call that writes many entries.
/// </summary>
public sealed class FixValidatorRulesTests
{
	static string Framed(string body)
	{
		var fields = body.Replace('|', '\u0001');
		var head   = "8=FIX.4.4\u00019=" + fields.Length + "\u0001";
		var sum    = 0;

		foreach (var octet in Encoding.Latin1.GetBytes(head + fields))
			sum += octet;

		return head + fields + "10=" + (sum % 256).ToString("D3") + "\u0001";
	}

	static FixMessage Message(string body)
	{
		Assert.True(FixMessages.TryParse(Framed(body), out var message, out var error), error?.Reason);

		return message!;
	}

	const string Header = "35=D|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|";
	const string Order  = "11=ORDER123|21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|";

	static FixDictionary Published()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		using var file = File.OpenRead(Path.Combine(at!.FullName, "tests", "Corpus", "Fix", "FIX44.xml"));

		return FixDictionary.Load(file);
	}

	// ── the table ────────────────────────────────────────────────────────────────────────────

	[Fact]
	public void A_type_nobody_has_written_an_entry_for_is_held_to_the_compiled_rule()
	{
		Assert.Same(FixValidator.Compiled, new FixValidator()["D"]);
		Assert.Same(FixValidator.Compiled, new FixValidator()["a type nobody has"]);
	}

	[Fact]
	public void A_rule_written_for_a_type_is_the_rule_that_answers()
	{
		var validator = new FixValidator();
		var asked     = 0;

		validator["D"] = (message, findings) =>
		{
			asked++;
			findings.Add(new FixFinding(FixRule.InvalidValue, FixScope.Body, 1, 0, -1, 0, "mine"));
		};

		var found = Message(Header + Order).Validate(validator);

		Assert.Equal(1, asked);
		Assert.Equal("mine", Assert.Single(found).Reason);

		// And only that type: another is still the compiled rule.
		Assert.Empty(Message("35=0|49=S|56=T|34=1|52=20260920-12:00:00|").Validate(validator));
	}

	[Fact]
	public void The_last_write_wins_and_the_compiled_rule_can_be_put_back()
	{
		var validator = new FixValidator();

		validator["D"] = static (_, findings) => findings.Add(
			new FixFinding(FixRule.InvalidValue, FixScope.Body, 1, 0, -1, 0, "first"));
		validator["D"] = static (_, findings) => findings.Add(
			new FixFinding(FixRule.InvalidValue, FixScope.Body, 1, 0, -1, 0, "second"));

		Assert.Equal("second", Assert.Single(Message(Header + Order).Validate(validator)).Reason);

		validator["D"] = FixValidator.Compiled;

		Assert.Empty(Message(Header + Order).Validate(validator));
	}

	[Fact]
	public void The_shared_standard_cannot_be_written_to()
	{
		var refused = Assert.Throws<InvalidOperationException>(
			() => FixValidator.Standard["D"] = FixValidator.Compiled);

		Assert.Contains("new FixValidator()", refused.Message);

		Assert.Throws<InvalidOperationException>(() => FixValidator.Standard.Load(Published()));

		// And it still answers what it answered.
		Assert.Empty(Message(Header + Order).Validate());
	}

	[Fact]
	public void A_null_type_or_rule_is_refused_rather_than_stored()
	{
		var validator = new FixValidator();

		Assert.Throws<ArgumentNullException>(() => validator[null!]);
		Assert.Throws<ArgumentNullException>(() => validator[null!] = FixValidator.Compiled);
		Assert.Throws<ArgumentNullException>(() => validator["D"] = null!);
		Assert.Throws<ArgumentNullException>(() => validator.Load(null!));
	}

	[Fact]
	public void A_validator_of_my_own_starts_where_the_standard_one_does()
	{
		var message = Message(Header + Order.Replace("11=ORDER123|", ""));

		Assert.Equal(message.Validate(), message.Validate(new FixValidator()));
	}

	// ── a dictionary is many entries at once ─────────────────────────────────────────────────

	[Fact]
	public void Loading_a_dictionary_writes_an_entry_for_every_type_it_describes()
	{
		var dictionary = Published();
		var validator  = new FixValidator();

		validator.Load(dictionary);

		foreach (var type in dictionary.MessageTypes)
			Assert.NotSame(FixValidator.Compiled, validator[type]);

		// A type it does not describe is left where it was.
		Assert.Same(FixValidator.Compiled, validator["not a type"]);
	}

	/// <summary>
	/// The published dictionary and the compiled tables answer the same, but for the tags they are
	/// already known to describe differently.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Two independent readings of FIX 4.4, held against each other over every fixture this package
	/// keeps — the check Q25 is about, taken over messages rather than over tables, which is the
	/// stronger of the two because it reaches the walk and not only the data.
	/// </para>
	/// <para>
	/// What it asserts is that every difference is about a tag already listed in
	/// <see cref="FixDictionaryTests"/> as one the two readings describe differently. A difference
	/// about any OTHER tag is a defect in the walk rather than a difference in the data, and that
	/// is what this catches: the data disagreements are known and enumerated, so anything outside
	/// them came from the code.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_loaded_dictionary_answers_as_the_compiled_tables_do_but_for_the_tags_they_describe_differently()
	{
		var loaded = new FixValidator();

		loaded.Load(Published());

		var known      = FixDictionaryTests.CodeDisagreements.Concat(FixDictionaryTests.TypeDisagreements).ToHashSet();
		var structural = new SortedSet<string>(StringComparer.Ordinal);

		foreach (var data in FixFixtures.Messages())
		{
			var message = FixMessages.Parse((string)data[1]);
			var ours    = message.Validate();
			var theirs  = message.Validate(loaded);

			foreach (var (side, finding) in ours.Except(theirs).Select(f => ("ours", f))
				.Concat(theirs.Except(ours).Select(f => ("file", f))))
				if (finding.Tag is not { } tag || !known.Contains(tag))
					structural.Add($"{message.MessageType} {side}: {finding.Rule} {finding.Scope}"
						+ (finding.GroupTag == 0 ? "" : "/" + finding.GroupTag)
						+ " tag " + finding.Tag);
		}

		Assert.Equal(Structural, structural);
	}

	/// <summary>
	/// Where the two readings of FIX 4.4 describe a message differently, rather than a tag.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every row is the file's validator saying something ours does not, over the fixtures this
	/// package keeps. They are all composition: which fields a scope holds and which of them it
	/// requires, which is a different kind of disagreement from the types and code sets listed in
	/// <see cref="FixDictionaryTests"/> and a more interesting one, because the walk had to be
	/// right for either side to say anything at all.
	/// </para>
	/// <para>
	/// Two shapes. QuoteRequestReject's NoRelatedSym entry holds twenty tags in our tables that the
	/// file does not place there. And four messages require something the file requires and we do
	/// not: PosAmt inside NoPosAmt in three of them, NoAllocs in AllocationReport, OrigClOrdID and
	/// OrigOrdModTime inside NoSides in the two cross-order messages.
	/// </para>
	/// <para>
	/// Which reading is FIX 4.4 is the published specification's answer and nobody here has looked
	/// it up, so the list is pinned rather than resolved. A row that appears or disappears fails
	/// this test, which is what makes it a guard rather than a note.
	/// </para>
	/// </remarks>
	static readonly SortedSet<string> Structural = new(StringComparer.Ordinal)
	{
		"AG file: FieldNotInScope Body/146 tag 44",
		"AG file: FieldNotInScope Body/146 tag 60",
		"AG file: FieldNotInScope Body/146 tag 126",
		"AG file: FieldNotInScope Body/146 tag 218",
		"AG file: FieldNotInScope Body/146 tag 220",
		"AG file: FieldNotInScope Body/146 tag 221",
		"AG file: FieldNotInScope Body/146 tag 222",
		"AG file: FieldNotInScope Body/146 tag 236",
		"AG file: FieldNotInScope Body/146 tag 423",
		"AG file: FieldNotInScope Body/146 tag 453",
		"AG file: FieldNotInScope Body/146 tag 640",
		"AG file: FieldNotInScope Body/146 tag 662",
		"AG file: FieldNotInScope Body/146 tag 663",
		"AG file: FieldNotInScope Body/146 tag 692",
		"AG file: FieldNotInScope Body/146 tag 696",
		"AG file: FieldNotInScope Body/146 tag 697",
		"AG file: FieldNotInScope Body/146 tag 698",
		"AG file: FieldNotInScope Body/146 tag 699",
		"AG file: FieldNotInScope Body/146 tag 701",
		"AG file: FieldNotInScope Body/146 tag 735",
		"AM file: RequiredFieldMissing Body/753 tag 708",
		"AP file: RequiredFieldMissing Body/753 tag 708",
		"AS file: RequiredFieldMissing Body tag 78",
		"AW file: RequiredFieldMissing Body/753 tag 708",
		"s file: FieldNotInScope Body/552 tag 586",
		"t file: RequiredFieldMissing Body/552 tag 41",
	};

	[Fact]
	public void A_dictionary_that_describes_a_type_we_do_not_makes_it_known()
	{
		// A venue's own message: the compiled tables have never heard of it, the dictionary has.
		var venue = FixDictionary.Parse(
			"""
			<fix major="4" minor="4">
				<header>
					<field name="BeginString" required="Y"/>
					<field name="BodyLength" required="Y"/>
					<field name="MsgType" required="Y"/>
					<field name="SenderCompID" required="Y"/>
					<field name="TargetCompID" required="Y"/>
					<field name="MsgSeqNum" required="Y"/>
					<field name="SendingTime" required="Y"/>
				</header>
				<trailer><field name="CheckSum" required="Y"/></trailer>
				<messages>
					<message name="VenueQuote" msgtype="U1">
						<field name="Symbol" required="Y"/>
					</message>
				</messages>
				<components/>
				<fields>
					<field number="8" name="BeginString" type="STRING"/>
					<field number="9" name="BodyLength" type="LENGTH"/>
					<field number="10" name="CheckSum" type="STRING"/>
					<field number="34" name="MsgSeqNum" type="SEQNUM"/>
					<field number="35" name="MsgType" type="STRING"/>
					<field number="49" name="SenderCompID" type="STRING"/>
					<field number="52" name="SendingTime" type="UTCTIMESTAMP"/>
					<field number="55" name="Symbol" type="STRING"/>
					<field number="56" name="TargetCompID" type="STRING"/>
				</fields>
			</fix>
			""");

		var wire = "35=U1|49=S|56=T|34=1|52=20260920-12:00:00|55=AAPL|";

		// Ours calls the type unknown, because it is — to us.
		Assert.Contains(Message(wire).Validate(), f => f.Rule == FixRule.UnknownMessageType);

		var validator = new FixValidator();

		validator.Load(venue);

		Assert.Empty(Message(wire).Validate(validator));

		// And the same dictionary reports what it does require.
		Assert.Contains(
			Message("35=U1|49=S|56=T|34=1|52=20260920-12:00:00|").Validate(validator),
			f => f.Rule == FixRule.RequiredFieldMissing && f.Tag == 55);
	}
}
