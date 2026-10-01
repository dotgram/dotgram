using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The seeds of the ignore-case fuzz that DotGram.Tests leaves out: seed 1 of each runs there,
/// these on request and in CI.
/// </summary>
public sealed partial class CaseFoldAttackTests
{
	[Theory]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(9)]
	[InlineData(10)]
	[InlineData(11)]
	[InlineData(12)]
	public void Every_rendering_agrees_with_the_semantics_on_unpaired_cases(int seed)
	{
		Fuzz(seed, Unpaired);
	}

	/// <summary>The same over ASCII pairs alone: what is wrong here is not about the fold.</summary>
	[Theory]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	public void Every_rendering_agrees_with_the_semantics_on_ascii_pairs(int seed)
	{
		Fuzz(seed, "aAbBkK1");
	}
}
