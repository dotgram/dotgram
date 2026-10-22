using System.Linq;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The generated code names its own parameters, locals and the methods that call the author's with a
/// <c>_DotGram</c> in the name, and a grammar may not write one (GRAM1012): an external method called
/// <c>argument0_DotGram</c> would hide itself, and a rule or publication called like a generated
/// method would be declared twice.
/// </summary>
public sealed class ReservedNameTests
{
	[Theory]
	[InlineData("Start_DotGram = 'a'")]
	[InlineData("Start = a_DotGram: 'a' & eof")]
	[InlineData("Start = $argument0_DotGram: 'a' & $argument0_DotGram")]
	[InlineData("Start = @argument0_DotGram")]
	[InlineData("Start = 'a'\nparse Start as Host_DotGram_Read_0")]
	public void A_name_with_the_reserved_part_is_refused_where_it_is_written(string grammar)
	{
		var told = GramCompiler.Read(grammar).Diagnostics.Where(static one => one.Id == "GRAM1012").ToArray();

		Assert.NotEmpty(told);
		Assert.All(told, one => Assert.Contains("_DotGram", grammar.Substring(one.Position, one.Length)));
	}

	[Fact]
	public void An_ordinary_name_is_not()
	{
		Assert.DoesNotContain(GramCompiler.Read("Start = a: 'a' & eof\nparse Start as Read").Diagnostics, static one => one.Id == "GRAM1012");
	}
}
