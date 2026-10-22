using System;
using System.Linq;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Determinism.Tests;

/// <summary>
/// Which names a grammar may write is the generator's Unicode table's, not the runtime's.
/// </summary>
/// <remarks>
/// U+1C89, CYRILLIC CAPITAL LETTER TJE, is a letter in Unicode 16.0 and unassigned in what .NET 8
/// reads: a rule named with it is one name here as it is on .NET 10, not an unexpected character.
/// </remarks>
public sealed class NamesOnNet8Tests
{
	[Fact]
	public void A_letter_of_Unicode_16_names_a_rule()
	{
		var result = GramCompiler.Compile("AᲉb = 'a'+\nparse AᲉb\n");

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);
		Assert.Contains(result.Sources, static source => source.Text.Contains("TryParseAᲉb(", StringComparison.Ordinal));
	}
}
