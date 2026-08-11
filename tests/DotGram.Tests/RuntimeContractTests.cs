using System;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Level 3: end to end. The generator ran over this project as an analyzer, so the
/// support types below are the ones it emitted — the same code a consumer would get.
/// </summary>
public sealed class RuntimeContractTests
{
	[Fact]
	public void Success_carries_value_and_span()
	{
		var result = RecognitionResult<int>.Success(42, new SourceSpan(3, 2));

		Assert.True(result.IsSuccess);
		Assert.Equal(Outcome.Success, result.Outcome);
		Assert.Equal(42, result.Value);
		Assert.Equal(SourceSpan.FromBounds(3, 5), result.Span);
		Assert.Null(result.Diagnostic);
	}

	[Fact]
	public void NoMatch_carries_no_value_and_no_diagnostic()
	{
		var result = RecognitionResult<int>.NoMatch(new SourceSpan(7, 0));

		Assert.False(result.IsSuccess);
		Assert.Equal(Outcome.NoMatch, result.Outcome);
		Assert.Equal(default, result.Value);
		Assert.Null(result.Diagnostic);
	}

	[Fact]
	public void Error_carries_a_diagnostic_and_takes_its_span()
	{
		var span       = new SourceSpan(10, 4);
		var diagnostic = new Diagnostic("unsupported symbol", span);
		var result     = RecognitionResult<int>.Error(diagnostic);

		Assert.Equal(Outcome.Error, result.Outcome);
		Assert.Same(diagnostic, result.Diagnostic);
		Assert.Equal(span, result.Span);
	}

	[Fact]
	public void Outcomes_are_values_not_exceptions()
	{
		// The whole point: no try/catch is needed to ask "did this match".
		var result = RecognitionResult<string>.NoMatch(default);

		Assert.Equal(Outcome.NoMatch, result.Outcome);
	}

	[Theory]
	[InlineData(0, 0, 0)]
	[InlineData(5, 3, 8)]
	public void Span_end_is_start_plus_length(int start, int length, int expectedEnd)
	{
		Assert.Equal(expectedEnd, new SourceSpan(start, length).End);
	}
}
