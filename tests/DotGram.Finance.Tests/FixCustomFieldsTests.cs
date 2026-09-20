using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixCustomFieldsTests
{
	// What a consumer writes: their own switch, and one line to fall through for a tag it does
	// not recognise. Every form records how it was reached, so a test can tell the three apart.
	sealed class Venue : FixCustomFields
	{
		public readonly List<string> Asked = [];

		public override FixField Text(int tag, ReadOnlySpan<char> value)
		{
			Asked.Add($"char {tag}");

			return tag == 25005 ? new Status(value.ToString()) : Spare(tag, value);
		}

		public override FixField Text(int tag, ReadOnlySpan<byte> value)
		{
			Asked.Add($"byte {tag}");

			return tag == 25005 ? new Status(Encoding.Latin1.GetString(value)) : Spare(tag, value);
		}

		public override FixField Binary(int tag, ReadOnlyMemory<byte> value)
		{
			Asked.Add($"binary {tag}");

			return tag == 25001 ? new Payload(value) : Spare(tag, value);
		}

		internal sealed class Status(string value) : FixField.Typed<string>(25005, value);

		internal sealed class Payload(ReadOnlyMemory<byte> value) : FixField.Typed<ReadOnlyMemory<byte>>(25001, value);
	}

	static FixFieldOptions Options(Venue venue, bool pairs = false) =>
		new(pairs ? new Dictionary<int, int> { [25000] = 25001 } : null, venue);

	[Fact]
	public void A_tag_the_package_knows_never_reaches_the_seam()
	{
		var venue = new Venue();

		var fields = FixParser.ParseLog("55=AAPL|54=1|", Options(venue));

		Assert.Equal([55, 54], fields.Select(field => field.Tag));
		Assert.IsType<FixField.Symbol>(fields[0]);
		Assert.Empty(venue.Asked);
	}

	[Fact]
	public void A_tag_it_does_not_know_is_built_by_the_consumer_in_both_text_forms()
	{
		foreach (var bytes in new[] { false, true })
		{
			var venue = new Venue();
			var text  = "55=AAPL|25005=OPEN|";

			var fields = bytes
				? FixParser.ParseLog(Encoding.Latin1.GetBytes(text), Options(venue))
				: FixParser.ParseLog(text, Options(venue));

			var status = Assert.IsType<Venue.Status>(fields[1]);

			Assert.Equal("OPEN", status.Value);
			Assert.Equal([$"{(bytes ? "byte" : "char")} 25005"], venue.Asked);
		}
	}

	[Fact]
	public void A_tag_the_consumer_does_not_know_either_falls_through_to_the_package()
	{
		var venue = new Venue();

		var fields = FixParser.ParseLog("28905=20261231|", Options(venue));

		var spare = Assert.IsType<FixField.Custom>(Assert.Single(fields));

		Assert.Equal(28905, spare.Tag);
		Assert.Equal("20261231", Encoding.Latin1.GetString(spare.Value.Span));
		Assert.Equal(["char 28905"], venue.Asked);
	}

	[Fact]
	public void The_payload_of_a_declared_pair_is_built_by_the_consumer()
	{
		var venue = new Venue();

		var fields = FixParser.ParseLog("25000=3|25001=a|b|55=END|", Options(venue, pairs: true));

		var payload = Assert.IsType<Venue.Payload>(fields[0]);

		Assert.Equal("a|b", Encoding.Latin1.GetString(payload.Value.Span));
		Assert.Contains("binary 25001", venue.Asked);
		Assert.DoesNotContain(venue.Asked, one => one.EndsWith(" 25000", StringComparison.Ordinal));
	}

	[Fact]
	public void The_length_half_of_a_declared_pair_is_built_by_the_consumer_too()
	{
		// The parser returns only the data half; the message model builds the length half itself.
		// Without the seam reaching there, one half of a consumer's own pair would be theirs and
		// the other ours.
		var venue  = new Venue();
		var wire   = Framed("35=0|49=A|56=B|34=1|52=20260919-12:00:00|25000=3|25001=xyz|");
		var read   = FixMessages.TryParse(wire, out _, out var error, new FixParseOptions(FixFraming.Wire, Options(venue, pairs: true)));

		Assert.True(read, error?.Reason);
		Assert.Contains("char 25000", venue.Asked);
		Assert.Contains("binary 25001", venue.Asked);
	}

	// A message the framing checks accept: BodyLength and CheckSum computed rather than guessed,
	// so that a test about the seam fails for the seam and not for arithmetic.
	static string Framed(string body)
	{
		var fields = body.Replace('|', '\u0001');
		var head   = "8=FIX.4.4\u00019=" + fields.Length + "\u0001";
		var sum    = 0;

		foreach (var octet in Encoding.Latin1.GetBytes(head + fields))
			sum += octet;

		return head + fields + "10=" + (sum % 256).ToString("D3") + "\u0001";
	}

	[Fact]
	public void Without_a_consumer_the_package_builds_what_it_always_built()
	{
		var fields = FixParser.ParseLog("25005=OPEN|", new FixFieldOptions());

		var spare = Assert.IsType<FixField.Custom>(Assert.Single(fields));

		Assert.Equal(25005, spare.Tag);
	}
}
