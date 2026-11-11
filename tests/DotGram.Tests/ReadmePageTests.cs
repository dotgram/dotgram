using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotGram.Examples.Expressions;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The repository README's examples, held to the example files they are copied from.
/// </summary>
/// <remarks>
/// <para>
/// The README is the first page a reader copies from, and it is not a page a package ships, so
/// nothing else runs it. Its grammars are copies of two files in <c>examples/DotGram.Examples</c>,
/// which <c>ExampleTests</c> run: so each grammar the README shows is held to its file line for line,
/// the calls it makes are compiled against the examples assembly as they are written, and the
/// calculator's calls are evaluated and held to the values the README's comments give.
/// </para>
/// <para>
/// The calculator in the README shows its grammar and leaves out the C# beside it (the tree type, the
/// power function and two options), saying so in the class body; the grammar itself is not
/// abridged, so it is compared whole.
/// </para>
/// </remarks>
public sealed class ReadmePageTests
{
	[Theory]
	[InlineData("CssColor", "Formats/CssColorExample.cs")]
	[InlineData("Calculator", "Expressions/CalculatorExample.cs")]
	public void A_grammar_of_the_readme_is_the_grammar_of_its_example_file(string host, string file)
	{
		var shown = Blocks().Where(block => ShippedPages.HostIn(block) == host).ToList();

		Assert.True(shown.Count == 1, $"README.md declares 'partial class {host}' {shown.Count} times, not once.");

		var example = File.ReadAllText(Path.Combine(SourceRoot.Directory, "examples", "DotGram.Examples", file));

		Assert.Equal(GrammarOf(example), GrammarOf(shown[0]));
	}

	/// <summary>
	/// Every block of calls in the README — the blocks that declare no class — compiles against the
	/// examples assembly as written, with the namespace of the examples imported for it.
	/// </summary>
	[Fact]
	public void The_calls_of_the_readme_compile_against_the_examples()
	{
		var calls = Blocks().Where(static block => ShippedPages.HostIn(block) is null).ToList();

		Assert.NotEmpty(calls);

		foreach (var block in calls)
		{
			var said = ShippedPages.Compiles(
				"using System;\nusing DotGram.Examples.Formats;\nusing DotGram.Examples.Expressions;\n" + block);

			Assert.True(said is null, "A block of calls in README.md does not compile:\n" + block + "\n" + said);
		}
	}

	/// <summary>The calculator's calls answer what the README's comments say they answer.</summary>
	[Fact]
	public void The_calculator_of_the_readme_answers_what_its_comments_say()
	{
		var claims = Blocks()
			.SelectMany(static block => Regex.Matches(block, @"Calculator\.(EvaluateInt|EvaluateDecimal)\(""([^""]*)""\);\s*// (\S+)"))
			.ToList();

		Assert.True(claims.Count >= 3, $"README.md evaluates {claims.Count} calculator expressions; expected at least 3.");

		foreach (var claim in claims)
		{
			var text   = claim.Groups[2].Value;
			var answer = claim.Groups[1].Value == "EvaluateInt"
				? Calculator.EvaluateInt(text).ToString(CultureInfo.InvariantCulture)
				: Calculator.EvaluateDecimal(text).ToString(CultureInfo.InvariantCulture);

			Assert.True(answer == claim.Groups[3].Value.TrimEnd(':'), $"README.md says {claim.Groups[1].Value}(\"{text}\") is {claim.Groups[3].Value}; it is {answer}.");
		}
	}

	/// <summary>The fenced C# of the README, in order.</summary>
	static IReadOnlyList<string> Blocks()
	{
		return ShippedPages.Blocks(Path.Combine(SourceRoot.Directory, "README.md"));
	}

	/// <summary>
	/// The grammar inside a block's <c>[Gram(…)]</c>, its lines without the indentation the raw string
	/// strips. The closing quotes may be followed by the attribute's options, which are not grammar.
	/// </summary>
	static string GrammarOf(string block)
	{
		var lines  = block.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
		var opens  = Array.FindIndex(lines, static line => line.TrimStart().StartsWith("[Gram(" + Quotes, StringComparison.Ordinal));
		var closes = Array.FindIndex(lines, opens + 1, static line => line.TrimStart().StartsWith(Quotes, StringComparison.Ordinal));

		Assert.True(opens >= 0 && closes > opens, "No [Gram(\"\"\" … \"\"\" in:\n" + block);

		var margin = lines[closes].Length - lines[closes].TrimStart().Length;

		return string.Join(
			"\n",
			lines
				.Skip(opens + 1)
				.Take(closes - opens - 1)
				.Select(line => line.Length >= margin ? line.Substring(margin) : line.TrimStart()));
	}

	const string Quotes = "\"\"\"";
}
