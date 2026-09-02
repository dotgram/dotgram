using System;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A braced rule that matches the empty string and can still refuse.
/// </summary>
/// <remarks>
/// <para>
/// A rule in braces compiles to a scanner, and its caller may skip asking whether the
/// scanner refused — on the reasoning that a rule which matches the empty string cannot.
/// That is the wrong question. <c>?= 'a'</c> matches the empty string when the lookahead
/// passes and refuses when it does not, and so does a rule made of one: nullable and
/// fallible at once.
/// </para>
/// <para>
/// Reading the refusal as a position then puts -1 in the position, and what the parse does
/// next is whatever the rest of the rule does with a negative one. On this grammar it was
/// the right answer by accident — the character test after it fails on a position out of
/// bounds — and on the grammar that found it, the wrong one. So the verdicts below are only
/// half of what is asserted: the other half is that the caller asked at all.
/// </para>
/// </remarks>
public sealed class NullableScannerTests
{
	const string Grammar =
		"""
		Ahead = { ?= 'a' }
		Start = Ahead & ['a'..'z']
		parse Start
		""";

	[Theory]
	[InlineData("a", true)]
	[InlineData("b", false)]
	[InlineData("z", false)]
	[InlineData("", false)]
	public void A_lookahead_in_braces_still_refuses(string input, bool reads)
	{
		var result = GramCompiler.Compile(
			Grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(
			result.Diagnostics,
			static diagnostic => diagnostic.Severity != GramSeverity.Info);

		var source = Assert.Single(result.Sources).Text;

		Assert.True(
			source.Contains("Scan_Ahead", StringComparison.Ordinal),
			"the braced rule did not become a scanner, so this grammar tests nothing");

		// Which branch the caller took: the one that asks whether the scanner refused, or
		// the one that reads its answer straight into `p`.
		Assert.True(
			source.Contains("var scanned = Scan_Ahead(", StringComparison.Ordinal),
			"the caller read the scanner's answer without asking whether it refused");

		var assembly = EmittedCode.Compile(source);

		var (matched, _, _, _) = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

		Assert.Equal(reads, matched);
	}
}
