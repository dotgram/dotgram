using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
			Assert.Equal("20261231", Encoding.Latin1.GetString(invalid.RawBytes.Span));
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

		protected override void OnValidate(Fix44Context context)
		{
			if (Status is null)
				AddFinding(new FixFinding(FixRule.RequiredFieldMissing, 25005, 0, null, -1));
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
		Assert.True(quote.Validate(context), string.Join("; ", quote.InvalidFindings ?? []));
	}

	[Fact]
	public void A_field_the_message_does_not_place_is_out_of_scope_and_its_own_check_runs()
	{
		var context = Venues();
		var quote   = Assert.IsType<VenueQuote>(FixParser.ParseMessage(Framed("35=U1|49=A|56=B|34=1|52=20260919-12:00:00|58=note|"), context));

		Assert.False(quote.Validate(context));
		Assert.Equal(
			[(FixRule.FieldNotInScope, FixTag.Text), (FixRule.RequiredFieldMissing, 25005)],
			quote.InvalidFindings!.Select(one => (one.Rule, one.Tag)));
	}

	[Fact]
	public void A_type_the_factory_answers_null_for_is_an_Invalid_message()
	{
		var message = FixParser.ParseMessage(Framed("35=U2|49=A|56=B|34=1|52=20260919-12:00:00|58=note|"), Venues());

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
