using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The message-level cases of the FIX session-level test suite, as QuickFIX/n carries them, run
/// through this package: what a session would have to reject, and the tag and reason it would
/// reject with.
/// </summary>
/// <remarks>
/// <para>
/// The files are <c>tests/Corpus/Fix/quickfixn/14*.def</c>, copied byte for byte. Each is a
/// conversation: an <c>I</c> line is a message the counterparty sends, and the <c>E</c> line
/// after it is what the engine under test is expected to answer. Where the answer is a Reject
/// (35=3) it carries <c>RefTagID</c> (371) and <c>SessionRejectReason</c> (373), and those two
/// are what this package's finding has to be able to say: the tag, and the rule as the
/// protocol names it.
/// </para>
/// <para>
/// This package is not a session, so the logon and logout that open and close each
/// conversation are not asked about, and an <c>E</c> that is not a Reject means only that the
/// message has nothing wrong with it. A wire an <c>I</c> line gives has no BodyLength and no
/// CheckSum, because the harness that runs these frames them, so they are framed here. The
/// schema is QuickFIX/n's own FIX44.xml, loaded over this package's, since the expectations were
/// written against it.
/// </para>
/// </remarks>
public sealed class QuickFixScenarioTests
{
	static readonly string Corpus = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "Fix", "quickfixn");

	static readonly Lazy<FixContext> Schema = new(() =>
		FixContext.Default.Load(File.ReadAllText(Path.Combine(Corpus, "..", "FIX44.xml"))));

	/// <summary>Every exchange of every scenario: the file, the message sent, and the answer expected.</summary>
	public static TheoryData<string, int, string, string> Exchanges()
	{
		var data = new TheoryData<string, int, string, string>();

		foreach (var path in Directory.GetFiles(Corpus, "14*.def").OrderBy(one => one, StringComparer.Ordinal))
		{
			var lines = File.ReadAllLines(path);
			var index = 0;

			for (var i = 0; i < lines.Length; i++)
			{
				if (!lines[i].StartsWith("I8=", StringComparison.Ordinal))
					continue;

				var sent = lines[i].Substring(1);
				var type = Tag(sent, 35);

				// The logon and the logout that frame every conversation are the session's, not the message layer's.
				if (type is "A" or "5")
					continue;

				var expected = lines.Skip(i + 1).First(line => line.StartsWith("E8=", StringComparison.Ordinal)).Substring(1);

				data.Add(Path.GetFileNameWithoutExtension(path), ++index, sent, expected);
			}
		}

		return data;
	}

	[Theory]
	[MemberData(nameof(Exchanges))]
	public void What_a_session_would_reject_this_package_finds(string scenario, int exchange, string sent, string expected)
	{
		var wire = Frame(sent.Replace("<TIME>", "20260922-12:00:00"));

		if (Tag(expected, 35) != "3")
		{
			// Not a Reject: the message is one the counterparty must take, so nothing may be wrong with it.
			var accepted = FixParser.ParseMessage(wire);

			Assert.True(accepted.Validate(Schema.Value), $"{scenario} #{exchange}: {Describe(accepted)}");

			return;
		}

		var tag    = int.Parse(Tag(expected, 371)!, CultureInfo.InvariantCulture);
		var reason = int.Parse(Tag(expected, 373)!, CultureInfo.InvariantCulture);

		if (!FixParser.TryParseMessage(wire, out var message, out var error))
		{
			// Three of the fifteen are refused at the door rather than read and found: a tag that is not
			// a positive number (0, -1) is not a field, and a field with no value is not one either,
			// so the reading stops where the wire stops being FIX and there is no message to have a
			// finding about. A session answers those with a Reject (SessionRejectReason 0 and 4); this
			// package answers with the refusal, which names the offset. The three are listed so that
			// a change on either side, a fourth refusal or one of these read, fails here.
			Assert.Contains((scenario, exchange), RefusedAtTheDoor);
			Assert.Contains("Input does not match", error!.Reason, StringComparison.Ordinal);

			return;
		}

		Assert.DoesNotContain((scenario, exchange), RefusedAtTheDoor);

		message!.Validate(Schema.Value);

		Assert.True(
			(message.InvalidFindings ?? []).Any(one => one.Tag == tag && Rule(reason).Contains(one.Rule)),
			$"{scenario} #{exchange}: expected {string.Join(" or ", Rule(reason))} on tag {tag} (SessionRejectReason {reason}); found {Describe(message)}");
	}

	static readonly (string Scenario, int Exchange)[] RefusedAtTheDoor =
	[
		("14a_BadField", 2),                 // 0=HI
		("14a_BadField", 3),                 // -1=HI
		("14d_TagSpecifiedWithoutValue", 1), // 56=
	];

	/// <summary>The rules of this package that a SessionRejectReason names.</summary>
	static FixRule[] Rule(int reason)
	{
		return reason switch
		{
			0  => [FixRule.FieldNotInScope],                       // Invalid tag number
			1  => [FixRule.RequiredFieldMissing],                  // Required tag missing
			2  => [FixRule.FieldNotInScope],                       // Tag not defined for this message type
			4  => [FixRule.InvalidValue],                          // Tag specified without a value
			5  => [FixRule.InvalidValue],                          // Value is incorrect (out of range) for this tag
			6  => [FixRule.InvalidValue],                          // Incorrect data format for value
			13 => [FixRule.DuplicateField],                        // Tag appears more than once
			14 => [FixRule.FieldOutOfOrder],                       // Tag specified out of required order
			_  => throw new ArgumentOutOfRangeException(nameof(reason), reason, "A SessionRejectReason this suite does not use."),
		};
	}

	static string Describe(FixMessage message)
	{
		return message.InvalidFindings is { Count: > 0 } findings ? string.Join("; ", findings) : "nothing";
	}

	static string? Tag(string wire, int tag)
	{
		var prefix = tag.ToString(CultureInfo.InvariantCulture) + "=";

		foreach (var field in wire.Split('\u0001'))
			if (field.StartsWith(prefix, StringComparison.Ordinal))
				return field.Substring(prefix.Length);

		return null;
	}

	/// <summary>BodyLength and CheckSum put on a wire that has neither, as the suite's own harness does.</summary>
	static string Frame(string sent)
	{
		var head = "8=FIX.4.4\u0001";

		Assert.StartsWith(head, sent);

		var body = sent.Substring(head.Length);
		var head9 = head + "9=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001";
		var sum   = Encoding.Latin1.GetBytes(head9 + body).Sum(octet => (int)octet);

		return head9 + body + "10=" + (sum % 256).ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
