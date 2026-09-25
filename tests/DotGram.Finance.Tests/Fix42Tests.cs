using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix42;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// FIX 4.2 through its own door: a message read into its class, its groups placed, the version told
/// from another, a stream of them, and the reference dictionary loaded over the standard.
/// </summary>
/// <remarks>
/// What each message type requires is held against the repository by
/// <see cref="Fix42RepositoryAgreementTests"/>; this is what a consumer of the version does with it.
/// </remarks>
public sealed class Fix42Tests
{
	// An order in FIX 4.2's own words: HandlInst is required, the quantity is still OrderQty's, and
	// the allocations are a group written inline rather than a component.
	const string Order = "11=ORD-1|21=1|55=IBM|54=1|60=20260920-12:00:00|38=300|40=2|44=101.25|78=2|79=ACC-A|80=100|79=ACC-B|80=200|";

	[Fact]
	public void An_order_is_read_into_its_class_with_its_groups()
	{
		var message = FixParser.ParseMessage(Wire("D", Order));
		var order   = Assert.IsType<FixMessage.NewOrderSingle>(message);

		Assert.Equal("ORD-1", order.ClOrdID!.Value);
		Assert.Equal('1', order.HandlInst!.Value);
		Assert.Equal(300m, order.OrderQty!.Value);
		Assert.Equal(2, order.NoAllocs!.Value);
		Assert.Equal(["ACC-A", "ACC-B"], order.NoAllocsGroups!.Select(one => one.AllocAccount.Value));
		Assert.Equal([100m, 200m], order.NoAllocsGroups!.Select(one => one.AllocShares!.Value));

		Assert.True(order.Validate(Fix42Context.Default), string.Join("; ", order.InvalidFindings ?? []));
	}

	[Fact]
	public void The_messages_are_named_as_the_dictionaries_name_them()
	{
		Assert.IsType<FixMessage.IndicationofInterest>(FixParser.ParseMessage(Wire("6", "23=IOI-1|28=N|55=IBM|54=1|27=S|")));
		Assert.IsType<FixMessage.NewOrderList>(FixParser.ParseMessage(Wire("E", "")));
		Assert.IsType<FixMessage.AllocationACK>(FixParser.ParseMessage(Wire("P", "")));
	}

	[Fact]
	public void A_field_FIX_42_does_not_place_in_a_message_is_out_of_scope()
	{
		// MaturityDate (541) came in with FIX 4.3: a tag FixTag names, and no field of a 4.2 order.
		var order = FixParser.ParseMessage(Wire("D", Order + "541=20270101|"));

		Assert.False(order.Validate(Fix42Context.Default));
		Assert.Contains(order.InvalidFindings!, one => one.Rule == FixRule.FieldNotInScope && one.Tag == 541);
	}

	[Fact]
	public void A_message_of_another_version_is_told_by_its_BeginString()
	{
		var order = FixParser.ParseMessage(Wire("D", Order).Replace("8=FIX.4.2", "8=FIX.4.4"));

		Assert.False(order.Validate(Fix42Context.Default));
		Assert.Contains(order.InvalidFindings!, one => one.Rule == FixRule.InvalidValue && one.Tag == FixTag.BeginString);
	}

	[Fact]
	public void A_stream_is_read_message_by_message()
	{
		var bytes = Encoding.Latin1.GetBytes(Wire("0", "") + Wire("D", Order) + Wire("1", "112=PING|"));

		using var stream = new MemoryStream(bytes);

		var types = FixParser.ReadMessages(stream).Select(one => one.GetType().Name).ToArray();

		Assert.Equal(["Heartbeat", "NewOrderSingle", "TestRequest"], types);
	}

	/// <summary>
	/// The FIX 4.2 data dictionary of the common engines (tests/Corpus/Fix/FIX42.xml) loads whole over
	/// the standard, and the order is held to it as it is to the standard.
	/// </summary>
	[Fact]
	public void The_reference_dictionary_loads()
	{
		var context = Fix42Context.Default.Load(File.ReadAllText(Path.Combine(Corpus, "FIX42.xml")));
		var order   = FixParser.ParseMessage(Wire("D", Order));

		Assert.True(order.Validate(context), string.Join("; ", order.InvalidFindings ?? []));
	}

	static readonly string Corpus = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "Fix");

	/// <summary>A FIX 4.2 message of the given type around a body written with '|' for SOH.</summary>
	static string Wire(string type, string body)
	{
		body = ("35=" + type + "|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|" + body).Replace('|', '\u0001');

		var prefix = "8=FIX.4.2\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var sum    = prefix.Sum(one => (int)one) % 256;

		return prefix + "10=" + sum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}
}
