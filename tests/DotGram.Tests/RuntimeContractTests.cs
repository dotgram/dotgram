using System;

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
/// the one a generated parser reaches for.
/// </remarks>
public sealed class RuntimeContractTests
{
	[Theory]
	[InlineData(0,  0,  0)]
	[InlineData(3,  2,  5)]
	[InlineData(10, 4, 14)]
	public void A_span_ends_where_its_length_takes_it(int start, int length, int expectedEnd)
	{
		var span = new SourceSpan(start, length);

		Assert.Equal(start,       span.Start);
		Assert.Equal(length,      span.Length);
		Assert.Equal(expectedEnd, span.End);
	}

	[Fact]
	public void And_says_so() =>
		Assert.Equal("[3..5)", new SourceSpan(3, 2).ToString());
}
