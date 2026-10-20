using System;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

using Fix42 = DotGram.Finance.Fix.Fix42;
using Fix50 = DotGram.Finance.Fix.Fix50;

namespace DotGram.Finance.Tests;

/// <summary>
/// What a context carries besides its schema: the largest message a reading takes, the framing,
/// the factory of custom messages, and where the checks a dictionary compiled to are written.
/// </summary>
public sealed class FixContextLimitsTests
{
	static readonly string Heartbeat = FixFixtures.Wire("0", "112=T|");

	static Fix44Context Limited(int length)
	{
		return Fix44Context.Default with { MaxMessageLength = length };
	}

	[Fact]
	public void The_default_limit_is_16_Mi_and_a_limit_must_be_positive()
	{
		Assert.Equal(16 * 1024 * 1024, Fix44Context.Default.MaxMessageLength);
		Assert.Equal(16 * 1024 * 1024, Fix42.Fix42Context.Default.MaxMessageLength);
		Assert.Equal(16 * 1024 * 1024, Fix50.Fix50Context.Default.MaxMessageLength);

		foreach (var value in new[] { 0, -1, int.MinValue })
			Assert.Throws<ArgumentOutOfRangeException>(() => Fix44Context.Default with { MaxMessageLength = value });
	}

	[Fact]
	public void A_message_longer_than_the_limit_is_refused_on_every_road_that_reads_a_stream()
	{
		var bytes = Encoding.Latin1.GetBytes(Heartbeat);
		var tight = Limited(Heartbeat.Length - 1);
		var exact = Limited(Heartbeat.Length);

		Assert.False(FixParser.TryReadMessage(new StringReader(Heartbeat), out _, out var error, tight));
		Assert.Contains("MaxMessageLength", error.Reason, StringComparison.Ordinal);
		Assert.False(FixParser.TryReadMessage(new MemoryStream(bytes), out _, out error, tight));
		Assert.Contains("MaxMessageLength", error.Reason, StringComparison.Ordinal);

		Assert.Throws<FormatException>(() => FixParser.ReadMessage(new StringReader(Heartbeat), tight));
		Assert.Throws<FormatException>(() => FixParser.ReadMessage(new MemoryStream(bytes), tight));
		Assert.Throws<FormatException>(() => FixParser.ReadMessages(new StringReader(Heartbeat), tight).ToArray());
		Assert.Throws<FormatException>(() => FixParser.ReadMessages(new MemoryStream(bytes), tight).ToArray());

		Assert.True(FixParser.TryReadMessage(new StringReader(Heartbeat), out var read, out _, exact));
		Assert.IsType<FixMessage.Heartbeat>(read);
		Assert.IsType<FixMessage.Heartbeat>(FixParser.ReadMessage(new MemoryStream(bytes), exact));
		Assert.Single(FixParser.ReadMessages(new StringReader(Heartbeat), exact));
		Assert.Single(FixParser.ReadMessages(new MemoryStream(bytes), exact));
	}

	[Fact]
	public void A_message_longer_than_the_limit_is_refused_on_every_road_that_reads_a_buffer_of_messages()
	{
		var two   = Heartbeat + Heartbeat;
		var bytes = Encoding.Latin1.GetBytes(two);
		var tight = Limited(Heartbeat.Length - 1);
		var exact = Limited(Heartbeat.Length);

		Assert.Throws<FormatException>(() => FixParser.ParseMessages(two, tight));
		Assert.Throws<FormatException>(() => FixParser.ParseMessages(bytes, tight));
		Assert.Throws<FormatException>(() => FixParser.ParseMessages(new ReadOnlyMemory<byte>(bytes), tight));

		// The limit is of one message, not of the input.
		Assert.Equal(2, FixParser.ParseMessages(two, exact).Length);
		Assert.Equal(2, FixParser.ParseMessages(bytes, exact).Length);
		Assert.Equal(2, FixParser.ParseMessages(new ReadOnlyMemory<byte>(bytes), exact).Length);
	}

	[Fact]
	public void The_limit_is_asked_of_every_message_of_a_run()
	{
		var big = FixFixtures.Wire("0", "112=" + new string('X', 200) + "|");
		var run = Heartbeat + big + Heartbeat;

		var read = FixParser.ReadMessages(new StringReader(run), Limited(Heartbeat.Length)).GetEnumerator();

		Assert.True(read.MoveNext());
		Assert.Throws<FormatException>(() => read.MoveNext());
		Assert.Equal(3, FixParser.ParseMessages(run, Limited(big.Length)).Length);
	}

	[Fact]
	public void No_context_reads_with_the_default_limit()
	{
		Assert.Equal(2, FixParser.ParseMessages(Heartbeat + Heartbeat).Length);
		Assert.IsType<FixMessage.Heartbeat>(FixParser.ReadMessage(new StringReader(Heartbeat)));
	}

	[Fact]
	public void The_limit_belongs_to_the_other_versions_contexts_too()
	{
		var tight = Fix42.Fix42Context.Default with { MaxMessageLength = 10 };

		Assert.False(Fix42.FixParser.TryReadMessage(new StringReader(Heartbeat), out _, out _, tight));
		Assert.False(Fix50.FixParser.TryReadMessage(new StringReader(Heartbeat), out _, out _, Fix50.Fix50Context.Default with { MaxMessageLength = 10 }));
	}

	[Fact]
	public void A_framing_that_is_neither_wire_nor_log_is_refused_where_it_is_set()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => Fix44Context.Default with { Framing = (FixFraming)2 });
		Assert.Throws<ArgumentOutOfRangeException>(() => Fix44Context.Default with { Framing = (FixFraming)(-1) });
		Assert.Equal(FixFraming.Log, (Fix44Context.Default with { Framing = FixFraming.Log }).Framing);
		Assert.Equal(FixFraming.Wire, Fix44Context.Default.Framing);
	}

	[Fact]
	public void The_log_context_is_one_value_that_reads_a_log()
	{
		Assert.Same(Fix44Context.Log, Fix44Context.Log);
		Assert.Equal(FixFraming.Log, Fix44Context.Log.Framing);
		Assert.Equal(FixFraming.Log, Fix42.Fix42Context.Log.Framing);
		Assert.Equal(FixFraming.Log, Fix50.Fix50Context.Log.Framing);
		Assert.Same(Fix42.Fix42Context.Log, Fix42.Fix42Context.Log);
		Assert.Same(Fix50.Fix50Context.Log, Fix50.Fix50Context.Log);
		Assert.Equal(FixFraming.Wire, Fix44Context.Default.Framing);

		Assert.Equal("ABC", Assert.IsType<FixField.Text>(Assert.Single(FixParser.ParseFields("55=ABC|", Fix44Context.Log))).Value);
	}

	[Fact]
	public void The_factory_of_custom_messages_builds_a_type_the_package_has_no_class_for()
	{
		var wire    = FixFixtures.Wire("U1", "58=hello|");
		var context = Fix44Context.Default with { MessageFactory = type => type == "U1" ? new Quote() : null };

		Assert.IsType<Quote>(FixParser.ParseMessage(wire, context));
		Assert.IsType<FixMessage.Unknown>(FixParser.ParseMessage(wire));
		Assert.IsType<FixMessage.Unknown>(FixParser.ParseMessage(FixFixtures.Wire("U2", "58=hello|"), context));
		Assert.IsType<Quote>(FixParser.ReadMessage(new StringReader(wire), context));
	}

	[Fact]
	public void The_checks_of_a_dictionary_are_written_where_the_context_says_and_nowhere_otherwise()
	{
		const string dictionary = "<fix><messages><message name=\"Logon\" msgtype=\"A\"><field name=\"ResetSeqNumFlag\" required=\"Y\" /></message></messages></fix>";

		var directory = Path.Combine(Path.GetTempPath(), "dotgram-fix-limits-" + Guid.NewGuid().ToString("N"));

		try
		{
			Assert.Null(Fix44Context.Default.CheckSourceDirectory);

			Fix44Context.Default.Load(dictionary);
			Assert.False(Directory.Exists(directory));

			var context = Fix44Context.Default with { CheckSourceDirectory = directory };

			// What is set is kept by the contexts made from it, so a second dictionary is written there too.
			context.Load(dictionary).Load(dictionary.Replace("Logon", "Heartbeat").Replace("\"A\"", "\"0\"").Replace("ResetSeqNumFlag", "TestReqID"));

			Assert.Equal(["Heartbeat.el", "Logon.el"], Directory.GetFiles(directory, "*.el").Select(Path.GetFileName).Order().ToArray());
		}
		finally
		{
			if (Directory.Exists(directory))
				Directory.Delete(directory, true);
		}
	}

	sealed class Quote : FixCustomMessage
	{
		protected override bool Place(FixField field)
		{
			return false;
		}
	}
}
