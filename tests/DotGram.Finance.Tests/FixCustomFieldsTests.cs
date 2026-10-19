using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What a consumer writes for the tags and the message types FIX 4.4 does not define: a dictionary
/// that gives each tag of their own its type, the pairs among them, and a factory of messages asked
/// with the MsgType.
/// </summary>
public sealed class FixCustomFieldsTests
{
	const string Venue =
		"""
		<fix>
		  <fields>
		    <field number="25005" name="VenueStatus" type="STRING" />
		    <field number="25006" name="VenuePrice" type="PRICE" />
		    <field number="25011" name="VenueFlag" type="BOOLEAN" />
		  </fields>
		</fix>
		""";

	static Fix44Context Context(bool pairs = false)
	{
		return new Fix44Context
		{
			LengthDataPairs = pairs ? new Dictionary<int, int> { [25000] = 25001 } : new Dictionary<int, int>(),
			Framing         = FixFraming.Log,
		}.Load(Venue);
	}

	[Fact]
	public void A_loaded_tag_is_read_by_its_type_from_characters_and_from_bytes()
	{
		foreach (var bytes in new[] { false, true })
		{
			const string text = "25005=OPEN|25006=12.50|25011=Y|55=X|";

			var fields = bytes
				? FixParser.ParseFields(Encoding.Latin1.GetBytes(text), Context())
				: FixParser.ParseFields(text, Context());

			Assert.Equal("OPEN", Assert.IsType<FixField.Text>(fields[0]).Value);
			Assert.Equal(25005, fields[0].Tag);
			Assert.Equal(12.50m, Assert.IsType<FixField.Decimal>(fields[1]).Value);
			Assert.True(Assert.IsType<FixField.Boolean>(fields[2]).Value);
			Assert.IsType<FixField.Text>(fields[3]);
		}
	}

	[Fact]
	public void A_value_that_does_not_fit_its_type_is_not_valid()
	{
		var fields = FixParser.ParseFields("25006=abc|", Context());

		Assert.False(Assert.IsType<FixField.Decimal>(Assert.Single(fields)).IsValid);
	}

	[Fact]
	public void A_dictionary_does_not_retype_a_tag_the_standard_defines()
	{
		var context = Fix44Context.WithLogFraming.Load(
			"""
			<fix>
			  <fields>
			    <field number="55" name="Symbol" type="INT" />
			  </fields>
			</fix>
			""");

		var symbol = Assert.IsType<FixField.Text>(Assert.Single(FixParser.ParseFields("55=AAPL|", context)));

		Assert.Equal("AAPL", symbol.Value);
	}

	[Fact]
	public void A_tag_nothing_defines_is_Invalid_and_keeps_its_tag_and_value()
	{
		foreach (var context in new[] { Context(), Fix44Context.WithLogFraming })
		{
			var invalid = Assert.IsType<FixField.Invalid>(Assert.Single(FixParser.ParseFields("28905=20261231|", context)));

			Assert.Equal(28905, invalid.Tag);
			Assert.False(invalid.IsValid);
			Assert.Equal("20261231", invalid.RawText);
			Assert.False(invalid.IsByteInput);

			invalid = Assert.IsType<FixField.Invalid>(Assert.Single(FixParser.ParseFields(Encoding.Latin1.GetBytes("28905=20261231|"), context)));

			Assert.Equal("20261231", Encoding.Latin1.GetString(invalid.RawBytes.Span));
			Assert.True(invalid.IsByteInput);
		}
	}

	[Fact]
	public void A_declared_pair_is_its_length_and_its_payload()
	{
		var fields = FixParser.ParseFields("25000=3|25001=a|b|55=END|", Context(pairs: true));

		Assert.Equal(3, Assert.IsType<FixField.Integer>(fields[0]).Value);
		Assert.Equal("a|b", Encoding.Latin1.GetString(Assert.IsType<FixField.Data>(fields[1]).Value.Span));
		Assert.Equal("END", Assert.IsType<FixField.Text>(fields[2]).Value);
	}

	[Fact]
	public void A_pair_declared_after_a_load_keeps_the_types_the_load_gave()
	{
		var context = Context() with { LengthDataPairs = new Dictionary<int, int> { [25000] = 25001 } };
		var fields  = FixParser.ParseFields("25006=1.5|25000=1|25001=||", context);

		Assert.Equal(1.5m, Assert.IsType<FixField.Decimal>(fields[0]).Value);
		Assert.IsType<FixField.Integer>(fields[1]);
		Assert.Equal("|", Encoding.Latin1.GetString(Assert.IsType<FixField.Data>(fields[2]).Value.Span));
	}

	[Fact]
	public void A_message_carries_both_halves_of_a_declared_pair()
	{
		var wire = Framed("35=0|49=A|56=B|34=1|52=20260919-12:00:00|25000=3|25001=xyz|");
		var read = FixParser.TryParseMessage(wire, out var message, out var error, Context(pairs: true) with { Framing = FixFraming.Wire });

		Assert.True(read, error?.Reason);
		Assert.Contains(message!.Fields, field => field is FixField.Integer { Tag: 25000 });
		Assert.Contains(message.Fields, field => field is FixField.Data { Tag: 25001 });
	}

	// ── messages ─────────────────────────────────────────────────────────────────────────────────

	// A venue's message: it keeps its status, and nothing else it does not know belongs to it.
	sealed class VenueQuote : FixCustomMessage
	{
		public FixField.Text? Status { get; private set; }

		protected override bool Place(FixField field)
		{
			if (field is not FixField.Text { Tag: 25005 } status)
				return false;

			Status = status;

			return true;
		}

		protected override void OnValidate(Fix44Context context, ICollection<FixFinding> findings)
		{
			if (Status is null)
				findings.Add(FixFinding.Missing(25005));
		}
	}

	static Fix44Context Venues()
	{
		return new Fix44Context { FixMessageFactory = type => type == "U1" ? new VenueQuote() : null }.Load(Venue);
	}

	[Fact]
	public void A_type_it_does_not_know_is_built_by_the_message_factory_and_handed_its_fields()
	{
		var context = Venues();
		var quote   = Assert.IsType<VenueQuote>(FixParser.ParseMessage(Framed("35=U1|49=A|56=B|34=1|52=20260919-12:00:00|25005=OPEN|"), context));

		Assert.Equal("U1", quote.MessageType);
		Assert.Equal("OPEN", quote.Status!.Value);
		Assert.Equal("A", quote.SenderCompID!.Value);
		Assert.Empty(quote.Validate(context));
	}

	[Fact]
	public void A_field_the_message_does_not_place_is_out_of_scope_and_its_own_check_runs()
	{
		var context = Venues();
		var quote   = Assert.IsType<VenueQuote>(FixParser.ParseMessage(Framed("35=U1|49=A|56=B|34=1|52=20260919-12:00:00|58=note|"), context));

		Assert.Equal(
			[(FixRule.FieldNotInScope, FixTag.Text), (FixRule.RequiredFieldMissing, 25005)],
			quote.Validate(context).Select(one => (one.Rule, one.Tag)));
	}

	// A message that reports what it reads as it places it, and offers the same call afterwards.
	sealed class VenueTicket : FixCustomMessage
	{
		protected override bool Place(FixField field)
		{
			if (field is FixField.Text { Tag: 25005, Value: not "OPEN" } status)
				AddFinding(FixFinding.Of(FixRule.InvalidValue, status));

			return field.Tag == 25005;
		}

		public void Report(FixFinding finding)
		{
			AddFinding(finding);
		}
	}

	[Fact]
	public void A_custom_message_adds_findings_while_it_is_read_and_not_afterwards()
	{
		var context = new Fix44Context { FixMessageFactory = type => type == "U3" ? new VenueTicket() : null }.Load(Venue);
		var ticket  = Assert.IsType<VenueTicket>(FixParser.ParseMessage(Framed("35=U3|49=A|56=B|34=1|52=20260919-12:00:00|25005=SHUT|"), context));
		var found   = Assert.Single(ticket.Findings);

		Assert.Equal((FixRule.InvalidValue, 25005), (found.Rule, found.Tag));
		Assert.Throws<InvalidOperationException>(() => ticket.Report(FixFinding.Missing(25006)));
		Assert.Single(ticket.Findings);

		// Validating does not open it either: a finding of the validation goes to what OnValidate is given.
		Assert.Same(found, Assert.Single(ticket.Validate(context)));
		Assert.Throws<InvalidOperationException>(() => ticket.Report(FixFinding.Missing(25006)));
	}

	// A message that validates itself while it is read, and goes on adding findings after that.
	sealed class VenueNested : FixCustomMessage
	{
		public Fix44Context? Context { get; init; }
		public bool          Fail    { get; init; }

		protected override bool Place(FixField field)
		{
			if (field.Tag is not (25005 or 25006))
				return false;

			if (field.Tag == 25005)
			{
				try
				{
					Validate(Context!);
				}
				catch (FormatException) when (Fail)
				{
				}
			}

			AddFinding(FixFinding.Of(FixRule.InvalidValue, field));

			return true;
		}

		protected override void OnValidate(Fix44Context context, ICollection<FixFinding> findings)
		{
			if (Fail)
				throw new FormatException("thrown by OnValidate");
		}

		public void Report(FixFinding finding)
		{
			AddFinding(finding);
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_custom_message_validated_inside_Place_keeps_adding_findings_until_it_has_been_read(bool fail)
	{
		Fix44Context? context = null;

		context = new Fix44Context { FixMessageFactory = type => type == "U4" ? new VenueNested { Context = context, Fail = fail } : null }.Load(Venue);

		var nested = Assert.IsType<VenueNested>(FixParser.ParseMessage(Framed("35=U4|49=A|56=B|34=1|52=20260919-12:00:00|25005=OPEN|25006=1.5|"), context));

		// The finding after the nested Validate and the one of the next Place are both there.
		Assert.Equal(
			[25005, 25006],
			nested.Findings.Where(one => one.Rule == FixRule.InvalidValue).Select(one => one.Tag));
		Assert.Throws<InvalidOperationException>(() => nested.Report(FixFinding.Missing(25011)));
	}

	[Fact]
	public void A_custom_message_adds_no_finding_after_OnValidate_has_thrown()
	{
		var context = new Fix44Context { FixMessageFactory = type => type == "U4" ? new VenueNested { Fail = true } : null }.Load(Venue);
		var nested  = Assert.IsType<VenueNested>(FixParser.ParseMessage(Framed("35=U4|49=A|56=B|34=1|52=20260919-12:00:00|"), context));

		Assert.Throws<FormatException>(() => nested.Validate(context));
		Assert.Throws<InvalidOperationException>(() => nested.Report(FixFinding.Missing(25011)));
	}

	// A message whose OnValidate runs on two threads at once, both inside it together: each call's
	// findings go to that call, and none of them to the message.
	sealed class VenueRace : FixCustomMessage
	{
		static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

		readonly Barrier _both = new(2);

		public bool Met { get; private set; } = true;

		protected override void OnValidate(Fix44Context context, ICollection<FixFinding> findings)
		{
			findings.Add(FixFinding.Missing(25011));

			if (!_both.SignalAndWait(Limit))
				Met = false;
		}

		public void Report(FixFinding finding)
		{
			AddFinding(finding);
		}
	}

	[Fact]
	public void Two_threads_validating_one_custom_message_each_get_their_own_findings()
	{
		var context = new Fix44Context { FixMessageFactory = type => type == "U5" ? new VenueRace() : null }.Load(Venue);
		var race    = Assert.IsType<VenueRace>(FixParser.ParseMessage(Framed("35=U5|49=A|56=B|34=1|52=20260919-12:00:00|"), context));

		IReadOnlyList<FixFinding>? second = null;

		var other = new Thread(() => second = race.Validate(context));

		other.Start();

		var first = race.Validate(context);

		Assert.True(other.Join(TimeSpan.FromSeconds(30)));
		Assert.True(race.Met);
		Assert.Equal(25011, Assert.Single(first).Tag);
		Assert.Equal(25011, Assert.Single(second!).Tag);
		Assert.NotSame(Assert.Single(first), Assert.Single(second!));
		Assert.Empty(race.Findings);
		Assert.Throws<InvalidOperationException>(() => race.Report(FixFinding.Missing(25011)));
	}

	// A message that, while one thread is inside Place or OnValidate, lets another call its public wrapper
	// around AddFinding: the scope of the callback is that thread's, and the other is refused.
	sealed class VenueHeld : FixCustomMessage
	{
		static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

		public bool InPlace { get; init; }

		public Exception? Other { get; private set; }

		public bool Called { get; private set; }

		protected override bool Place(FixField field)
		{
			if (InPlace && field.Tag == 25005)
				FromAnotherThread();

			return field.Tag == 25005;
		}

		protected override void OnValidate(Fix44Context context, ICollection<FixFinding> findings)
		{
			if (!InPlace)
				FromAnotherThread();
		}

		// This thread waits inside the callback until the other has tried.
		void FromAnotherThread()
		{
			using var both  = new Barrier(2);
			var       other = new Thread(() =>
			{
				both.SignalAndWait(Limit);

				try
				{
					Report(FixFinding.Missing(25011));
				}
				catch (Exception exception)
				{
					Other = exception;
				}

				Called = true;
				both.SignalAndWait(Limit);
			});

			other.Start();

			Assert.True(both.SignalAndWait(Limit));
			Assert.True(both.SignalAndWait(Limit));
			Assert.True(other.Join(Limit));
		}

		public void Report(FixFinding finding)
		{
			AddFinding(finding);
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Another_thread_adds_no_finding_while_one_is_inside_a_callback(bool inPlace)
	{
		var context = new Fix44Context { FixMessageFactory = type => type == "U6" ? new VenueHeld { InPlace = inPlace } : null }.Load(Venue);
		var held    = Assert.IsType<VenueHeld>(FixParser.ParseMessage(Framed("35=U6|49=A|56=B|34=1|52=20260919-12:00:00|25005=OPEN|"), context));

		if (!inPlace)
			Assert.Empty(held.Validate(context));

		Assert.True(held.Called);
		Assert.IsType<InvalidOperationException>(held.Other);
		Assert.Empty(held.Findings);
	}

	[Fact]
	public void A_type_the_factory_answers_null_for_is_an_Unknown_message()
	{
		var message = FixParser.ParseMessage(Framed("35=U2|49=A|56=B|34=1|52=20260919-12:00:00|58=note|"), Venues());

		var invalid = Assert.IsType<FixMessage.Unknown>(message);

		Assert.Equal(FixRule.UnknownMessageType, Assert.Single(invalid.Findings).Rule);
		Assert.Equal("A", invalid.SenderCompID!.Value);
	}

	// A message the framing checks accept: BodyLength and CheckSum computed rather than guessed,
	// so that a test about the factories fails for the factories and not for arithmetic.
	static string Framed(string body)
	{
		var fields = body.Replace('|', '\u0001');
		var head   = "8=FIX.4.4\u00019=" + fields.Length + "\u0001";
		var sum    = 0;

		foreach (var octet in Encoding.Latin1.GetBytes(head + fields))
			sum += octet;

		return head + fields + "10=" + (sum % 256).ToString("D3") + "\u0001";
	}
}
