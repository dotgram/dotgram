using System;
using System.Runtime.CompilerServices;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>What <c>--help</c> says and what <c>docs/cli.md</c> says is the option list the parser reads.</summary>
public sealed class DocumentationTests
{
	[Fact]
	public void Every_option_the_parser_reads_is_in_the_help_and_in_the_reference()
	{
		var reference = File.ReadAllText(Path.Combine(RepositoryRoot(), "docs", "cli.md"));
		var options   = CommandLine.Valued.Concat(CommandLine.Flags).Where(static name => name.StartsWith("--", StringComparison.Ordinal)).ToList();

		Assert.True(options.Count >= 15, "the parser's option list was not read");

		foreach (var option in options)
		{
			Assert.Contains(option, Help.Text);
			Assert.Contains("`" + option, reference);
		}

		// The contract's sentence, in the words both of them use.
		foreach (var phrase in new[] { "byte for byte", "design-time pass", "--line-paths absolute" })
		{
			Assert.Contains(phrase, Help.Text);
			Assert.Contains(phrase, reference);
		}
	}

	[Fact]
	public void The_skill_tells_the_contract_and_the_exit_codes_as_the_help_does()
	{
		var skill = Help.Skill();

		foreach (var phrase in new[] { "byte for byte", "design-time pass", "exit 2", "exits 3" })
			Assert.Contains(phrase, skill);

		Assert.Contains("--line-paths absolute", skill);
		Assert.Contains("--line-paths absolute", Help.Text);
	}

	static string RepositoryRoot([CallerFilePath] string source = "")
	{
		var at = new DirectoryInfo(Path.GetDirectoryName(source)!);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("DotGram.slnx was not found above the test's source.");
	}
}
