using System;
using System.Linq;

using DotGram.Language;
using DotGram.Tests.Fragments;

using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// The fragment tree read under the tool's Roslyn, the newest, where DotGram.Tests reads it under
/// the oldest the generator loads into: what is portable must not depend on which of them asked.
/// </summary>
/// <remarks>
/// The census is the same code in both projects (FragmentCensus.cs, linked), and each holds its
/// table to the one checked in, so that the two are held to each other. Both parse at the language
/// version the lowering fixes; neither asks its parser's own default.
/// </remarks>
public sealed class FragmentLoweringVersionTests(ITestOutputHelper output)
{
	[Fact]
	public void This_process_reads_with_a_newer_Roslyn_than_the_floor()
	{
		Assert.True(typeof(SyntaxFactory).Assembly.GetName().Version >= new Version(5, 0), typeof(SyntaxFactory).Assembly.GetName().Version?.ToString());
	}

	[Fact]
	public void Every_shipped_fragment_is_one_whole_expression_here_too()
	{
		var fragments = FragmentCensus.Fragments();
		var unread    = fragments.Where(static found => RoslynFragmentLowering.Lower(found.Text).Refusals.Any(static one => one.Form == "Unparsable")).ToList();

		Assert.True(fragments.Count > 3000);
		Assert.Empty(unread);
	}

	[Fact]
	public void The_census_is_the_one_checked_in()
	{
		var table = FragmentCensus.Table(FragmentCensus.Fragments());

		output.WriteLine(table);

		var difference = FragmentCensus.Disagreement(table);

		Assert.True(difference is null, difference);
	}
}
