using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What a consumer writes for the tags and the message types FIX 4.4 does not define: the type of each
/// tag of their own, and a factory of messages asked with the MsgType.
/// </summary>
public sealed class FixCustomFieldsTests
{
	// What a consumer writes when a field wants a class of its own: the standard's type, held to more.
	sealed class Status(int tag, (bool Valid, string Value) value)
		: FixField.Custom<string>(tag, (value.Valid && value.Value is "OPEN" or "CLOSED", value.Value));

	static readonly Dictionary<int, FixCustom> Declared = new()
	{
		[25005] = FixCustom.Text.As(static (tag, value) => new Status(tag, value)),
		[25006] = FixCustom.Decimal,
		[25000] = FixCustom.Integer,
		[25001] = FixCustom.Data,
		[55]    = FixCustom.Integer,
	};

	static FixContext Context(bool pairs = false, List<int>? asked = null)
	{
		return new()
		{
			LengthDataPairs = pairs ? new Dictionary<int, int> { [25000] = 25001 } : new Dictionary<int, int>(),
			FixFieldFactory = tag =>
			{
				asked?.Add(tag);

				return Declared.GetValueOrDefault(tag);
			},
			Framing = FixFraming.Log,
		};
	}

	[Fact]
	public void A_tag_the_package_knows_never_reaches_the_factory()
	{
		var asked  = new List<int>();
		var fields = FixParser.ParseFields("55=AAPL|54=1|25005=OPEN|", Context(asked: asked));

		Assert.Equal("AAPL", Assert.IsType<FixField.Symbol>(fields[0]).Value);
		Assert.Equal([25005], asked);
	}

	[Fact]
	public void A_declared_tag_is_read_by_its_type_from_characters_and_from_bytes()
	{
		foreach (var bytes in new[] { false, true })
		{
			const string text = "25005=OPEN|25006=12.50|";

			var fields = bytes
				? FixParser.ParseFields(Encoding.Latin1.GetBytes(text), Context())
				: FixParser.ParseFields(text, Context());

			var status = Assert.IsType<Status>(fields[0]);

			Assert.Equal("OPEN", status.Value);
			Assert.True(status.IsValid);
			Assert.Equal(12.50m, Assert.IsType<FixField.Custom<decimal>>(fields[1]).Value);
		}
	}

	[Fact]
	public void A_value_that_does_not_fit_is_not_valid()
	{
		var fields = FixParser.ParseFields("25005=MAYBE|25006=abc|", Context());

		Assert.False(Assert.IsType<Status>(fields[0]).IsValid);
		Assert.False(Assert.IsType<FixField.Custom<decimal>>(fields[1]).IsValid);
	}

	[Fact]
	public void A_tag_nothing_declares_is_Invalid_and_keeps_its_tag_and_value()
	{
		foreach (var context in new[] { Context(), FixContext.WithLogFraming })
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

		Assert.Equal(3, Assert.IsType<FixField.Custom<long>>(fields[0]).Value);
		Assert.Equal("a|b", Encoding.Latin1.GetString(Assert.IsType<FixField.Custom<ReadOnlyMemory<byte>>>(fields[1]).Value.Span));
	}

	[Fact]
	public void A_message_carries_both_halves_of_a_declared_pair()
	{
		var wire = Framed("35=0|49=A|56=B|34=1|52=20260919-12:00:00|25000=3|25001=xyz|");
		var read = FixParser.TryParseMessage(wire, out var message, out var error, Context(pairs: true) with { Framing = FixFraming.Wire });

		Assert.True(read, error?.Reason);
		Assert.Contains(message!.Fields, field => field is FixField.Custom<long> { Tag: 25000 });
		Assert.Contains(message.Fields, field => field is FixField.Custom<ReadOnlyMemory<byte>> { Tag: 25001 });
	}

	[Fact]
	public void A_class_built_with_another_tag_is_refused()
	{
		var context = new FixContext
		{
			FixFieldFactory = static _ => FixCustom.Text.As(static (_, value) => new Status(25005, value)),
			Framing      = FixFraming.Log,
		};

		Assert.Throws<InvalidOperationException>(() => FixParser.ParseFields("25006=X|", context));
	}

	[Fact]
	public void A_loaded_dictionary_answers_for_the_fields_the_standard_does_not()
	{
		var context = FixContext.WithLogFraming.Load(
			"""
			<fix>
			  <fields>
			    <field number="25010" name="VenuePrice" type="PRICE" />
			    <field number="25011" name="VenueFlag" type="BOOLEAN" />
			  </fields>
			</fix>
			""");

		var fields = FixParser.ParseFields("25010=1.25|25011=Y|55=X|", context);

		Assert.Equal(1.25m, Assert.IsType<FixField.Custom<decimal>>(fields[0]).Value);
		Assert.True(Assert.IsType<FixField.Custom<bool>>(fields[1]).Value);
		Assert.IsType<FixField.Symbol>(fields[2]);
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

	static FixContext Venues()
	{
		return new() { FixFieldFactory = static tag => Declared.GetValueOrDefault(tag), FixMessageFactory = type => type == "U1" ? new VenueQuote() : null };
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
			[(FixRule.FieldNotInScope, 58), (FixRule.RequiredFieldMissing, 25005)],
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
