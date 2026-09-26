using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix50;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// FIX 5.0 SP2 over FIXT 1.1 through its own door: a message read into its class under the session
/// layer's BeginString, the zoned values it adds, the class a C# name would not allow, a stream of
/// them, and the reference dictionaries read over the standard.
/// </summary>
/// <remarks>
/// What each message type requires is held against the repository by
/// <see cref="Fix50RepositoryAgreementTests"/>; this is what a consumer of the version does with it.
/// </remarks>
public sealed class Fix50Tests
{
	// An order of FIX 5.0 SP2: the application's version in the header, the instrument with its
	// maturity time at an offset, and the parties as a group of a component.
	const string Order = "1128=9|11=ORD-1|55=IBM|541=20270115|1079=13:09+05:30|54=1|60=20260920-12:00:00|38=300|40=2|44=101.25|" +
		"453=2|448=BROKER|447=D|452=1|448=CLIENT|447=D|452=3|";

	[Fact]
	public void An_order_is_read_into_its_class_under_the_session_layers_BeginString()
	{
		var message = FixParser.ParseMessage(Wire("D", Order));
		var order   = Assert.IsType<FixMessage.NewOrderSingle>(message);

		Assert.Equal("FIXT.1.1", order.BeginString!.Value);
		Assert.Equal("9", order.ApplVerID!.Value);
		Assert.Equal(300m, order.OrderQty!.Value);
		Assert.Equal(["BROKER", "CLIENT"], order.NoPartyIDsGroups!.Select(one => one.PartyID.Value));

		Assert.True(order.Validate(Fix50Context.Default), string.Join("; ", order.InvalidFindings ?? []));
	}

	[Fact]
	public void A_zoned_time_keeps_its_offset()
	{
		var order = (FixMessage.NewOrderSingle)FixParser.ParseMessage(Wire("D", Order));

		Assert.Equal((new TimeOnly(13, 9), new TimeSpan(5, 30, 0)), order.MaturityTime!.Value);

		var wrong = (FixMessage.NewOrderSingle)FixParser.ParseMessage(Wire("D", Order.Replace("1079=13:09+05:30", "1079=13:09")));

		Assert.False(wrong.MaturityTime!.IsValid);
		Assert.False(wrong.Validate(Fix50Context.Default));
	}

	[Fact]
	public void A_zoned_timestamp_is_the_instant_at_the_offset_it_was_written_with()
	{
		var report = (FixMessage.TradeCaptureReport)FixParser.ParseMessage(Wire("AE", "1132=20060901-02:39-05|"));

		Assert.Equal(new DateTimeOffset(2006, 9, 1, 2, 39, 0, TimeSpan.FromHours(-5)), report.TZTransactTime!.Value);
		Assert.Equal(TimeSpan.FromHours(-5), report.TZTransactTime.Value.Offset);
	}

	[Fact]
	public void The_message_SecurityStatus_is_the_class_SecurityStatusMessage()
	{
		// It carries the field SecurityStatus, tag 965, and a C# class has no member of its own name.
		var message = Assert.IsType<FixMessage.SecurityStatusMessage>(FixParser.ParseMessage(Wire("f", "55=IBM|965=1|")));

		Assert.Equal("1", message.SecurityStatus!.Value);
	}

	[Fact]
	public void A_message_of_another_version_is_told_by_its_BeginString()
	{
		var order = FixParser.ParseMessage(Wire("D", Order).Replace("8=FIXT.1.1", "8=FIX.4.4"));

		Assert.False(order.Validate(Fix50Context.Default));
		Assert.Contains(order.InvalidFindings!, one => one.Rule == FixRule.InvalidValue && one.Tag == FixTag.BeginString);
	}

	[Fact]
	public void A_stream_is_read_message_by_message()
	{
		var bytes = Encoding.Latin1.GetBytes(Wire("0", "") + Wire("D", Order) + Wire("f", "55=IBM|"));

		using var stream = new MemoryStream(bytes);

		var types = FixParser.ReadMessages(stream).Select(one => one.GetType().Name).ToArray();

		Assert.Equal(["Heartbeat", "NewOrderSingle", "SecurityStatusMessage"], types);
	}

	/// <summary>
	/// The reference dictionaries of the common engines — FIXT 1.1's session layer and FIX 5.0 SP2's
	/// application (tests/Corpus/Fix/FIXT11.xml and FIX50SP2.xml) — are of a later edition than the
	/// repository this package is written from, and read as one over the standard they are refused at the
	/// first place they depart from it, the same on every run: UserNotification, which carries Username
	/// there and not in the repository.
	/// </summary>
	/// <remarks>
	/// The groups they write as components, MsgTypeGrp and a hundred more, are read before that: a
	/// component the version has no interface for is checked where it is carried.
	/// </remarks>
	[Fact]
	public void The_reference_dictionaries_name_where_they_depart_from_the_standard()
	{
		for (var run = 0; run < 3; run++)
		{
			var refused = Assert.Throws<FormatException>(() => Fix50Context.Default.Load([
				File.ReadAllText(Path.Combine(Corpus, "FIXT11.xml")),
				File.ReadAllText(Path.Combine(Corpus, "FIX50SP2.xml")),
			]));

			Assert.Equal("The dictionary places the field 'Username' in 'FixMessage.UserNotification', which has no member of that name.", refused.Message);
		}
	}

	/// <summary>
	/// With the errata read over them (tests/Corpus/Fix/quickfixn-fix50sp2-errata.xml: UsernameGrp as the
	/// repository's group) the reference dictionaries apply once the two message types the model has no
	/// class for are removed; a file read over another cannot remove, so that is code. A standard order
	/// and a UserNotification with its group are held to them as to the standard.
	/// </summary>
	[Fact]
	public void The_errata_and_two_removals_make_the_reference_dictionaries_apply()
	{
		var merged = FixDictionary.Parse(File.ReadAllText(Path.Combine(Corpus, "FIXT11.xml")))
			.Merge(FixDictionary.Parse(File.ReadAllText(Path.Combine(Corpus, "FIX50SP2.xml"))))
			.Merge(FixDictionary.LoadFile(Path.Combine(Corpus, "quickfixn-fix50sp2-errata.xml")));

		// The errata alone leaves the two later message types, which the model has no class for.
		var refused = Assert.Throws<FormatException>(() => Fix50Context.Default.With(merged));

		Assert.StartsWith("The dictionary describes 'PartyDetailsList", refused.Message, StringComparison.Ordinal);

		merged.Messages.Remove("CF");
		merged.Messages.Remove("CG");

		var context = Fix50Context.Default.With(merged);
		var order   = FixParser.ParseMessage(Wire("D", Order));
		var notice  = FixParser.ParseMessage(Wire("CB", "809=2|553=ALICE|553=BOB|926=1|"));

		Assert.True(order.Validate(context), string.Join("; ", order.InvalidFindings ?? []));
		Assert.True(notice.Validate(context), string.Join("; ", notice.InvalidFindings ?? []));

		// And every other check they describe compiles: nothing is left to fail on a later message's first reading.
		context.Prepare();

		Assert.Equal(0, context.Checks.CompileDeferred());
	}

	/// <summary>The session layer's dictionary alone loads, and the order is held to it as to the standard.</summary>
	[Fact]
	public void The_session_layers_dictionary_loads()
	{
		var context = Fix50Context.Default.Load(File.ReadAllText(Path.Combine(Corpus, "FIXT11.xml")));
		var order   = FixParser.ParseMessage(Wire("D", Order));

		Assert.True(order.Validate(context), string.Join("; ", order.InvalidFindings ?? []));
	}

	/// <summary>
	/// A fragment may name a field it does not describe by the name this version gives it, which is
	/// not always <see cref="FixTag"/>'s: FixTag takes the newest version's names, and a message the dictionaries call SecurityStatus is the class SecurityStatusMessage.
	/// </summary>
	[Fact]
	public void A_fragment_names_an_undescribed_field_by_the_versions_name()
	{
		const string fragment =
			"""
			<fix>
			  <messages>
			    <message name="SecurityStatus" msgtype="f">
			      <field name="HaltReasonInt" required="Y" />
			    </message>
			  </messages>
			</fix>
			""";

		var context = Fix50Context.Default.Load(fragment);
		var message = FixParser.ParseMessage(Wire("f", "55=IBM|"));

		Assert.False(message.Validate(context));
		Assert.Contains(message.InvalidFindings!, one => one.Rule == FixRule.RequiredFieldMissing && one.Tag == 327);
	}

	static readonly string Corpus = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "Fix");

	/// <summary>A FIXT 1.1 message of the given type around a body written with '|' for SOH.</summary>
	static string Wire(string type, string body)
	{
		body = ("35=" + type + "|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|" + body).Replace('|', '\u0001');

		var prefix = "8=FIXT.1.1\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var sum    = prefix.Sum(one => (int)one) % 256;

		return prefix + "10=" + sum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
