using System;

using DotGram.GrammarLibrary;

using Xunit;

namespace DotGram.Tests;

public sealed class ReferencedGrammarTests
{
	[Fact]
	public void An_include_reads_GramSource_from_a_real_project_reference()
	{
		Assert.Equal(42, ImportedGrammar.ParseSum("19+23"));
		Assert.False(ImportedGrammar.TryParseSum("19+x").IsSuccess);
	}

	[Fact]
	public void A_base_reads_GramSource_from_a_real_project_reference()
	{
		Assert.Equal(42, DerivedGrammar.ParseStart("42!"));
		Assert.False(DerivedGrammar.TryParseStart("42?").IsSuccess);
	}
}

[GramInclude(typeof(Lexemes), As = "L")]
[Gram("""
	using L;
	Sum : @int = a: L.Number & '+' & b: L.Number => @(a + b)
	parse Sum
	""")]
partial class ImportedGrammar;

[Gram("""
	using Lexemes;
	Start : @int = n: Number & '!' => @(n)
	parse Start
	""")]
partial class DerivedGrammar : Lexemes;
