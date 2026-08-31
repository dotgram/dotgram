using System;
using System.Linq;
using System.Text;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A grammar whose generated method is left past the size the JIT optimizes is told so.
/// </summary>
/// <remarks>
/// The limit is real and measured — past about two thousand basic blocks RyuJIT compiles a
/// method the way it compiles one on its first call, and leaves it that way — and the
/// generator divides its methods to stay under it. Where nothing can divide, the author
/// gets <c>GRAM5003</c>: a warning, because the parser is correct and merely slower, with
/// the numbers it acted under, because the remedy is chosen against them.
/// </remarks>
public sealed class OversizeTests
{
	[Fact]
	public void A_parser_left_past_the_limit_warns_and_still_generates()
	{
		var compiled = GramCompiler.Compile(Huge(literals: 1200), new GramCompilerOptions
		{
			ClassName = "Big",
		});

		Assert.DoesNotContain(compiled.Diagnostics, d => d.Severity == GramSeverity.Error);
		Assert.Single(compiled.Sources);

		var oversized = compiled.Diagnostics.Where(d => d.Id == "GRAM5003").ToArray();

		Assert.NotEmpty(oversized);
		Assert.All(oversized, d => Assert.Equal(GramSeverity.Warning, d.Severity));

		// The message must let the author act: the estimate it measured, the line it
		// measured against, and a remedy.
		Assert.Contains("basic blocks", oversized[0].Message, StringComparison.Ordinal);
		Assert.Contains("2000", oversized[0].Message, StringComparison.Ordinal);
	}

	[Fact]
	public void A_parser_inside_the_limit_does_not_warn()
	{
		var compiled = GramCompiler.Compile(Huge(literals: 20), new GramCompilerOptions
		{
			ClassName = "Small",
		});

		Assert.DoesNotContain(compiled.Diagnostics, d => d.Id == "GRAM5003");
	}

	/// <summary>
	/// One rule that is a single straight-line sequence of distinct literals: it lowers to
	/// one method, and no cut can divide a method whose whole body is one alternative.
	/// </summary>
	static string Huge(int literals)
	{
		var text = new StringBuilder("Start = ");

		for (var i = 0; i < literals; i++)
			text.Append(i == 0 ? "" : " & ").Append("\"a").Append(i).Append('"');

		text.Append("\nparse Start\n");

		return text.ToString();
	}
}
