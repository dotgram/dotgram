using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// A message type and a class are the same thing, so the rule lives in the class's own field.
/// </summary>
/// <remarks>
/// These tests write static fields, which is process-wide state — D115's price, taken knowingly:
/// one configuration per process. They run in a collection of their own so that nothing else
/// validates while one of them is holding somebody else's schema, and each puts back what it
/// changed.
/// </remarks>
[Collection("the rules of the process")]
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
		Assert.True(FixParser.TryParseMessage(Framed(body), out var message, out var error), error?.Reason);

		return message!;
	}

	const string Header = "35=D|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|";
	const string Order  = "11=ORDER123|21=1|55=AAPL|54=1|60=20260920-12:00:00|38=100|40=2|44=150.25|59=0|";

	static string PublishedPath()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return Path.Combine(at!.FullName, "tests", "Corpus", "Fix", "FIX44.xml");
	}

	static FixDictionary Published()
	{
		using var file = File.OpenRead(PublishedPath());

		return FixDictionary.Load(file);
	}

	/// <summary>The rule a dictionary installs, built without touching what the process holds.</summary>
	static FixMessageRule Rule(FixDictionary dictionary)
	{
		var tables = new DictionaryTables(dictionary);

		return (message, findings) => FixRules.Check(tables, message, findings);
	}

	static FixFinding[] Answer(FixMessageRule rule, FixMessage message)
	{
		var found = new List<FixFinding>();

		rule(message, found);

		return [.. found];
	}

	// ── the field on the class ───────────────────────────────────────────────────────────────

	[Fact]
	public void The_rule_a_class_holds_is_the_rule_that_answers()
	{
		var asked = 0;

		try
		{
			FixMessage.NewOrderSingle.Rule = (message, findings) =>
			{
				asked++;
				findings.Add(new FixFinding(FixRule.InvalidValue, FixScope.Body, 1, 0, -1, 0, "mine"));
			};

			var found = Message(Header + Order).Validate();

			Assert.Equal(1, asked);
			Assert.Equal("mine", Assert.Single(found).Reason);

			// And only that class: another type is still held to what the package compiles in.
			Assert.Empty(Message("35=0|49=S|56=T|34=1|52=20260920-12:00:00|").Validate());
		}
		finally
		{
			FixMessage.NewOrderSingle.Rule = FixValidator.ValidateNewOrderSingle;
		}
	}

	[Fact]
	public void The_last_assignment_wins_and_the_named_rule_puts_it_back()
	{
		try
		{
			FixMessage.NewOrderSingle.Rule = static (_, findings) => findings.Add(
				new FixFinding(FixRule.InvalidValue, FixScope.Body, 1, 0, -1, 0, "first"));
			FixMessage.NewOrderSingle.Rule = static (_, findings) => findings.Add(
				new FixFinding(FixRule.InvalidValue, FixScope.Body, 1, 0, -1, 0, "second"));

			Assert.Equal("second", Assert.Single(Message(Header + Order).Validate()).Reason);
		}
		finally
		{
			FixMessage.NewOrderSingle.Rule = FixValidator.ValidateNewOrderSingle;
		}

		Assert.Empty(Message(Header + Order).Validate());
	}

	// ── a dictionary writes many fields at once ──────────────────────────────────────────────

	[Fact]
	public void A_dictionary_is_refused_at_the_door_rather_than_half_loaded()
	{
		Assert.Throws<ArgumentNullException>(() => FixParser.LoadDictionary((Stream)null!));
		Assert.Throws<ArgumentNullException>(() => FixParser.LoadDictionary((TextReader)null!));

		using var not = new StringReader("<fix major=\"4\" minor=\"4\"></fix>");

		Assert.Throws<FormatException>(() => FixParser.LoadDictionary(not));

		// And nothing moved: a refusal at the door leaves every rule where it was.
		Assert.Equal((FixMessageRule)FixValidator.ValidateNewOrderSingle, FixMessage.NewOrderSingle.Rule);
	}

	[Fact]
	public void Loading_a_dictionary_writes_the_field_of_every_type_it_describes()
	{
		try
		{
			using (var file = File.OpenRead(PublishedPath()))
				FixParser.LoadDictionary(file);

			Assert.NotEqual((FixMessageRule)FixValidator.ValidateNewOrderSingle, FixMessage.NewOrderSingle.Rule);
			Assert.NotEqual((FixMessageRule)FixValidator.ValidateHeartbeat, FixMessage.Heartbeat.Rule);

			// And a message still validates, against the file now.
			Assert.Empty(Message(Header + Order).Validate());
		}
		finally
		{
			FixRuleFields.Compiled();
		}

		Assert.Equal((FixMessageRule)FixValidator.ValidateNewOrderSingle, FixMessage.NewOrderSingle.Rule);
	}

	/// <summary>
	/// EVERY class comes back, not most of them.
	/// </summary>
	/// <remarks>
	/// The restore was a hand-written list, kept beside a second hand-written list of the same set,
	/// and the two disagreed: NewOrderSingle twice, Heartbeat not at all. A load then left Heartbeat
	/// holding a dictionary's rule for the rest of the process, and nothing said so — the suite was
	/// green because of the order the tests happened to run in.
	/// </remarks>
	[Fact]
	public void Every_class_comes_back_from_a_loaded_dictionary()
	{
		var classes = typeof(FixMessage).GetNestedTypes(BindingFlags.Public)
			.Where(one => one.IsSubclassOf(typeof(FixMessage)))
			.ToArray();

		Assert.Equal(94, classes.Length);

		try
		{
			using (var file = File.OpenRead(PublishedPath()))
			{
				FixParser.LoadDictionary(file);
			}
		}
		finally
		{
			FixRuleFields.Compiled();
		}

		foreach (var one in classes)
		{
			var rule     = (FixMessageRule)one.GetField("Rule", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
			var compiled = typeof(FixValidator).GetMethod("Validate" + one.Name)!;

			Assert.True(
				rule.Method == compiled,
				$"{one.Name} came back as {rule.Method.Name} rather than Validate{one.Name}");
		}
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
	/// <para>
	/// The file's rule is built here rather than loaded into the process, because the comparison
	/// needs both answers about one message and a field holds one rule at a time.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_loaded_dictionary_answers_as_the_compiled_tables_do_but_for_the_tags_they_describe_differently()
	{
		var file       = Rule(Published());
		var known      = FixDictionaryTests.CodeDisagreements.Concat(FixDictionaryTests.TypeDisagreements).ToHashSet();
		var structural = new SortedSet<string>(StringComparer.Ordinal);

		foreach (var data in FixFixtures.Messages())
		{
			var message = FixParser.ParseMessage((string)data[1]);
			var ours    = message.Validate();
			var theirs  = Answer(file, message);

			foreach (var (side, finding) in ours.Except(theirs).Select(f => ("ours", f))
				.Concat(theirs.Except(ours).Select(f => ("file", f))))
				if (finding.Tag is not { } tag || !known.Contains(tag))
					structural.Add($"{message.MessageType} {side}: {finding.Rule} {finding.Scope}"
						+ (finding.GroupTag == 0 ? "" : "/" + finding.GroupTag)
						+ " tag " + finding.Tag);
		}

		// Named before compared: a set difference printed as text is what a reader of a failure
		// needs, and the comparison below is still the one that decides.
		Assert.True(structural.SetEquals(Structural),
			"new: "  + string.Join(", ", structural.Except(Structural)) + Environment.NewLine +
			"gone: " + string.Join(", ", Structural.Except(structural)));

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
	/// not: PosAmt inside NoPosAmt in three of them, NoAllocs in AllocationReport, OrigClOrdID
	/// inside NoSides in CrossOrderCancelReplaceRequest.
	/// </para>
	/// <para>
	/// <strong>The cross-order rows have been looked up and are settled.</strong> The FIX
	/// Repository gives NewOrderCross and CrossOrderCancelReplaceRequest one and the same side
	/// component, SideCrossOrdModGrp, and it holds neither OrigClOrdID (41) nor OrigOrdModTime
	/// (586). The file gives the replace message a side group of its own with both. These tables
	/// followed the repository for 41 and the file for 586, which was neither reading whole; they
	/// now follow the repository for both, and the two rows here are what the file says and we do
	/// not.
	/// </para>
	/// <para>
	/// The rest are not looked up. Which reading is FIX 4.4 is the published specification's
	/// answer, and <c>tests/Corpus/FixRepository</c> is now in this repository to give it — what is
	/// missing is the work, not the source. A row that appears or disappears fails this test, which
	/// is what makes it a guard rather than a note.
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
		"t file: RequiredFieldMissing Body/552 tag 41",
		"t ours: FieldNotInScope Body/552 tag 586",
	};

	/// <summary>
	/// A dictionary describing a type this package has no class for does not make it known, and
	/// that is the decision rather than a gap (D115).
	/// </summary>
	/// <remarks>
	/// A rule lives in a class's field, and a type without a class has no field to live in. What a
	/// consumer does about a counterparty's own types is write the class — the message factory
	/// builds it and it validates itself. So a loaded dictionary moves the ninety-three and no
	/// more, and this test stands so that whoever makes it move further has to come and change a
	/// test that says why it does not.
	/// </remarks>
	[Fact]
	public void A_dictionary_that_describes_a_type_we_do_not_leaves_it_unknown()
	{
		var wire = "35=U1|49=S|56=T|34=1|52=20260920-12:00:00|55=AAPL|";

		// Ours calls the type unknown, because it is — to us.
		Assert.Contains(Message(wire).Validate(), f => f.Rule == FixRule.UnknownMessageType);

		try
		{
			using (var text = new StringReader(Venue))
				FixParser.LoadDictionary(text);

			// Still unknown: the file describes U1, and U1 has no class to hold its rule.
			Assert.Contains(Message(wire).Validate(), f => f.Rule == FixRule.UnknownMessageType);
		}
		finally
		{
			FixRuleFields.Compiled();
		}
	}

	/// <summary>A venue's own message, which the compiled tables have never heard of.</summary>
	const string Venue =
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
		""";
}
