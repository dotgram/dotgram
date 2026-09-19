using System;

using Xunit;

namespace DotGram.Tests;

/// <summary>The whole refusal record (<see cref="RefusalCorpus"/>), every shape in every rendering.</summary>
/// <remarks>
/// What a change to how failures are recorded is gated on: <c>DotGram.Tests</c> holds one
/// reading in five of it, and this all of them, writing what differs beside the record.
/// </remarks>
public sealed class RefusalCorpusTests
{
	[Fact]
	public void Every_refusal_is_the_one_it_was() =>
		RefusalCorpus.AssertWhole();
}
