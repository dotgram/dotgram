using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What a consumer writes for the tags and the message types FIX 4.4 does not define: a factory of
/// each, asked with the tag or the MsgType, answering the field or the message to build, or null.
/// </summary>
public sealed class FixCustomFieldsTests
{
	// What a consumer writes: a field class a tag, and a factory that says which.
	sealed class Status() : FixCustomField(25005)
	{
		public string? Value { get; private set; }

		protected internal override bool Read(ReadOnlySpan<char> value)
		{
			Value = value.ToString();

			return Value is "OPEN" or "CLOSED";
		}
	}

	sealed class Payload() : FixCustomField(25001)
	{
		public byte[] Value { get; private set; } = [];

		protected internal override bool Read(ReadOnlySpan<char> value)
		{
			return false;
		}

		protected internal override bool Read(ReadOnlyMemory<byte> data)
		{
			Value = data.ToArray();

			return true;
		}
	}

	sealed class PayloadLength() : FixCustomField(25000)
	{
		protected internal override bool Read(ReadOnlySpan<char> value)
		{
			return long.TryParse(value.ToString(), out _);
		}
	}

	sealed class Venue
	{
		public readonly List<int> Asked = [];

		public FixCustomField? Build(int tag)
		{
			Asked.Add(tag);

			return tag switch
			{
				25005 => new Status(),
				25001 => new Payload(),
				25000 => new PayloadLength(),
				_     => null,
			};
		}
	}

	static FixContext Context(Venue venue, bool pairs = false)
	{
		return new()
		{
			LengthDataPairs = pairs ? new Dictionary<int, int> { [25000] = 25001 } : null,
			FixFieldFactory = venue.Build,
			Framing         = FixFraming.Log,
		};
	}

	[Fact]
	public void A_tag_the_package_knows_never_reaches_the_factory()
	{
		var venue  = new Venue();
		var fields = FixParser.ParseFields("55=AAPL|54=1|", Context(venue));

		Assert.Equal([55, 54], fields.Select(field => field.Tag));
		Assert.IsType<FixField.Symbol>(fields[0]);
		Assert.Empty(venue.Asked);
	}

	[Fact]
	public void A_tag_it_does_not_know_is_built_by_the_factory_from_characters_and_from_bytes()
	{
		foreach (var bytes in new[] { false, true })
		{
			var venue = new Venue();
			var text  = "55=AAPL|25005=OPEN|";

			var fields = bytes
				? FixParser.ParseFields(Encoding.Latin1.GetBytes(text), Context(venue))
				: FixParser.ParseFields(text, Context(venue));

			var status = Assert.IsType<Status>(fields[1]);

			Assert.Equal("OPEN", status.Value);
			Assert.True(status.IsValid);
			Assert.Equal([25005], venue.Asked);
		}
	}

	[Fact]
	public void What_the_field_says_of_its_value_is_whether_it_is_valid()
	{
		var status = Assert.IsType<Status>(Assert.Single(FixParser.ParseFields("25005=MAYBE|", Context(new Venue()))));

		Assert.False(status.IsValid);
	}

	[Fact]
	public void A_tag_the_factory_answers_null_for_is_Invalid_and_keeps_its_tag_and_value()
	{
		var venue   = new Venue();
		var invalid = Assert.IsType<FixField.Invalid>(Assert.Single(FixParser.ParseFields("28905=20261231|", Context(venue))));

		Assert.Equal(28905, invalid.Tag);
		Assert.False(invalid.IsValid);
		Assert.Equal("20261231", Encoding.Latin1.GetString(invalid.RawBytes.Span));
		Assert.Equal([28905], venue.Asked);
	}

	[Fact]
	public void Without_a_factory_a_tag_the_package_does_not_know_is_Invalid()
	{
		var invalid = Assert.IsType<FixField.Invalid>(Assert.Single(FixParser.ParseFields("25005=OPEN|", FixContext.WithLogFraming)));

		Assert.Equal(25005, invalid.Tag);
		Assert.Equal("OPEN", Encoding.Latin1.GetString(invalid.RawBytes.Span));
	}

	[Fact]
	public void The_payload_of_a_declared_pair_is_built_by_the_factory()
	{
		var venue   = new Venue();
		var fields  = FixParser.ParseFields("25000=3|25001=a|b|55=END|", Context(venue, pairs: true));
		var payload = Assert.IsType<Payload>(fields[0]);

		Assert.Equal("a|b", Encoding.Latin1.GetString(payload.Value));
		Assert.Equal([25001], venue.Asked);
	}

	[Fact]
	public void The_length_half_of_a_declared_pair_is_built_by_the_factory_too()
	{
		// The reader returns the data half alone; the message puts the length half back itself, and
		// asks the same factory, so that both halves of a consumer's pair are the consumer's.
		var venue = new Venue();
		var wire  = Framed("35=0|49=A|56=B|34=1|52=20260919-12:00:00|25000=3|25001=xyz|");
		var read  = FixParser.TryParseMessage(wire, out var message, out var error, Context(venue, pairs: true) with { Framing = FixFraming.Wire });

		Assert.True(read, error?.Reason);
		Assert.Contains(message!.Fields, field => field is PayloadLength);
		Assert.Contains(message.Fields, field => field is Payload);
	}

	[Fact]
	public void A_factory_that_builds_a_field_of_another_tag_is_refused()
	{
		var context = new FixContext { FixFieldFactory = _ => new Status(), Framing = FixFraming.Log };

		Assert.Throws<InvalidOperationException>(() => FixParser.ParseFields("25006=X|", context));
	}

	// ── messages ─────────────────────────────────────────────────────────────────────────────────

	// A venue's message: it keeps its Status, and nothing else it does not know belongs to it.
	sealed class VenueQuote : FixCustomMessage
	{
		public Status? Status { get; private set; }

		protected override bool Place(FixField field)
		{
			if (field is not Status status)
				return false;

			Status = status;

			return true;
		}

		protected override void OnValidate(FixContext context)
		{
			if (Status is null)
				AddFinding(new FixFinding(FixRule.RequiredFieldMissing, 25005, 0, null, -1));
		}
	}

	static FixContext Venues(Venue venue)
	{
		return new() { FixFieldFactory = venue.Build, FixMessageFactory = type => type == "U1" ? new VenueQuote() : null };
	}

	[Fact]
	public void A_type_it_does_not_know_is_built_by_the_message_factory_and_handed_its_fields()
	{
		var context = Venues(new Venue());
		var quote   = Assert.IsType<VenueQuote>(FixParser.ParseMessage(Framed("35=U1|49=A|56=B|34=1|52=20260919-12:00:00|25005=OPEN|"), context));

		Assert.Equal("U1", quote.MessageType);
		Assert.Equal("OPEN", quote.Status!.Value);
		Assert.Equal("A", quote.SenderCompID!.Value);
		Assert.True(quote.Validate(context), string.Join("; ", quote.InvalidFindings ?? []));
	}

	[Fact]
	public void A_field_the_message_does_not_place_is_out_of_scope_and_its_own_check_runs()
	{
		var context = Venues(new Venue());
		var quote   = Assert.IsType<VenueQuote>(FixParser.ParseMessage(Framed("35=U1|49=A|56=B|34=1|52=20260919-12:00:00|58=note|"), context));

		Assert.False(quote.Validate(context));
		Assert.Equal(
			[(FixRule.FieldNotInScope, 58), (FixRule.RequiredFieldMissing, 25005)],
			quote.InvalidFindings!.Select(one => (one.Rule, one.Tag)));
	}

	[Fact]
	public void A_type_the_factory_answers_null_for_is_an_Invalid_message()
	{
		var message = FixParser.ParseMessage(Framed("35=U2|49=A|56=B|34=1|52=20260919-12:00:00|58=note|"), Venues(new Venue()));

		var invalid = Assert.IsType<FixMessage.Invalid>(message);

		Assert.False(invalid.IsValid);
		Assert.Equal(FixRule.UnknownMessageType, Assert.Single(invalid.InvalidFindings!).Rule);
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
