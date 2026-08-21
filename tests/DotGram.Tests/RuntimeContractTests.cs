using System;

using DotGram;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Level 3: end to end. The generator ran over this project as an analyzer, so what is
/// used below is what it emitted — the same code a consumer would get.
/// </summary>
/// <remarks>
/// There used to be four support types and this file exercised all of them. Three were
/// used by nothing at all — emitted into every consumer's compilation against features
/// that had not been written — and were deleted rather than versioned. What is left is
/// the one a generated parser reaches for, and it is named through a host class because
/// that is where it now lives: a type in a namespace has to be internal for two assemblies
/// not to collide over it, and internal is what a public method may not hand back.
/// </remarks>
/// <summary>
/// A grammar whose value is where it matched rather than what it matched (§4.1 case 4).
/// </summary>
/// <remarks>
/// Written here because the type only exists in a host that names it: it is emitted per
/// host class, and into the ones that use it. A parser that never mentions a span does not
/// carry the type for one.
/// </remarks>
[Gram("Extent : @SourceSpan = ' '* & ['a'..'z']+\nparse Extent")]
public partial class Extents;

public sealed class RuntimeContractTests
{
	[Theory]
	[InlineData(0,  0,  0)]
	[InlineData(3,  2,  5)]
	[InlineData(10, 4, 14)]
	public void A_span_ends_where_its_length_takes_it(int start, int length, int expectedEnd)
	{
		var span = new Extents.SourceSpan(start, length);

		Assert.Equal(start,       span.Start);
		Assert.Equal(length,      span.Length);
		Assert.Equal(expectedEnd, span.End);
	}

	[Fact]
	public void And_says_so() =>
		Assert.Equal("[3..5)", new Extents.SourceSpan(3, 2).ToString());

	[Fact]
	public void And_a_parser_hands_one_back()
	{
		// What used to be refused: the type was emitted into a namespace, so it had to be
		// internal, so a public method could not return it. Nested in the host, it can.
		const string text = "  abc";

		var extent = Extents.ParseExtent(text);

		Assert.Equal(0, extent.Start);
		Assert.Equal(5, extent.Length);
		Assert.Equal("  abc", extent.On(text).ToString());
	}

	[Fact]
	public void And_reads_the_text_it_was_measured_against()
	{
		// The whole of why it is two integers: what the caller wanted was the characters,
		// and they already have them. Cutting a string out is theirs to ask for.
		const string text = "http://example.com";

		var host = new Extents.SourceSpan(7, 11);

		Assert.True(host.On(text).SequenceEqual("example.com"));
		Assert.Equal("example.com", host.On(text).ToString());
	}
}
