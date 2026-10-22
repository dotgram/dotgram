using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>Response files, the effective arguments, <c>--help</c>, <c>--skill</c> and <c>--version</c>.</summary>
public sealed class CommandLineTests
{
	[Theory]
	[InlineData("a b c", new[] { "a", "b", "c" })]
	[InlineData("  a\tb  ", new[] { "a", "b" })]
	[InlineData("\"a b\" c", new[] { "a b", "c" })]
	[InlineData("pre\"a b\"post x", new[] { "prea bpost", "x" })]
	[InlineData("\"\"", new[] { "" })]
	[InlineData("\"open to the end", new[] { "open to the end" })]
	[InlineData("--define A#B", new[] { "--define", "A#B" })]
	[InlineData("", new string[0])]
	public void A_line_splits_as_csc_splits_it(string line, string[] expected)
	{
		Assert.Equal(expected, ResponseFile.Split(line));
	}

	[Fact]
	public void Comment_lines_and_line_ends_of_every_kind()
	{
		Assert.Equal(
			["generate", "x.csproj", "-o", "out dir", "--define", "A"],
			ResponseFile.Split("# the project\r\ngenerate x.csproj\n  # indented comment\r-o \"out dir\"\n\n--define A\n"));
	}

	/// <summary>Every token of a fixture as it was meant, in order, nested file included, paths read against each file.</summary>
	[Fact]
	public void A_response_file_round_trips_every_token()
	{
		using var scratch = new Scratch();

		scratch.Write("config/grammars.rsp", """
			# What CI and the developer both run.
			generate ../src/Grammars.csproj
			-o "../src/Generated Files"
			--define DOTGRAM_CHECKS --define "WITH SPACE"
			@more/common.rsp
			--line-paths absolute
			""");

		scratch.Write("config/more/common.rsp", """
			--reference refs/Lib.dll
			--package System.Memory@4.6.3
			""");

		var expanded = ResponseFile.Expand(["@config/grammars.rsp", "--allow-unresolved"], scratch.Root);

		Assert.Equal(
			[
				"generate", "../src/Grammars.csproj", "-o", "../src/Generated Files", "--define", "DOTGRAM_CHECKS", "--define", "WITH SPACE",
				"--reference", "refs/Lib.dll", "--package", "System.Memory@4.6.3", "--line-paths", "absolute", "--allow-unresolved",
			],
			expanded.Select(static argument => argument.Text));

		var options = GenerateOptions.Parse(expanded.Skip(1).ToList());

		Assert.Equal(scratch.Path("src", "Grammars.csproj"), options.Input);
		Assert.Equal(scratch.Path("src", "Generated Files"), options.Output);
		Assert.Equal([scratch.Path("config", "more", "refs", "Lib.dll")], options.References);
		Assert.Equal(["DOTGRAM_CHECKS", "WITH SPACE"], options.Defines);
		Assert.Equal(["System.Memory@4.6.3"], options.Packages);
		Assert.True(options.AllowUnresolved);
		Assert.Equal(LinePaths.Absolute, options.LinePaths);

		// And the line a run begins with says all of it, so that a token the file lost is seen.
		Assert.Equal(
			$"generate {scratch.Path("src", "Grammars.csproj")} --output \"{scratch.Path("src", "Generated Files")}\" --configuration Release " +
			$"--reference {scratch.Path("config", "more", "refs", "Lib.dll")} --package System.Memory@4.6.3 --define DOTGRAM_CHECKS --define \"WITH SPACE\" --line-paths absolute --allow-unresolved",
			options.Describe());
	}

	[Fact]
	public void A_response_file_that_includes_itself_is_refused()
	{
		using var scratch = new Scratch();

		scratch.Write("a.rsp", "generate @b.rsp\n");
		scratch.Write("b.rsp", "@a.rsp\n");

		var run = Tool.Run(scratch.Root, "@a.rsp");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("includes itself", run.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void A_missing_response_file_is_refused()
	{
		using var scratch = new Scratch();

		var run = Tool.Run(scratch.Root, "generate", "@nowhere.rsp");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("cannot read response file", run.Error, StringComparison.Ordinal);
	}

	/// <summary>The effective arguments are the first thing a run prints.</summary>
	[Fact]
	public void A_run_begins_with_its_arguments()
	{
		using var scratch = new Scratch();

		scratch.Write("Calc.gram", "Row = 'a'\n\nparse Row\n");
		scratch.Write("calc.rsp", "generate Calc.gram -o out --define EXTRA\n");

		var run = Tool.Run(scratch.Root, "@calc.rsp");

		Assert.True(run.Exit == 0, run.ToString());
		Assert.StartsWith(
			$"dotnet dotgram generate {scratch.Path("Calc.gram")} --output {scratch.Path("out")} --configuration Release --define EXTRA --line-paths relative",
			run.Output,
			StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("--help")]
	[InlineData("-h")]
	public void Help_lists_every_option_and_the_exit_codes(string flag)
	{
		var run = Tool.Run(Directory.GetCurrentDirectory(), flag);

		Assert.Equal(0, run.Exit);

		foreach (var option in new[]
		{
			"--output", "--framework", "--configuration", "--reference", "--package", "--define", "--langversion",
			"--line-paths", "--allow-unresolved", "--namespace", "--class", "--public", "--option", "--sources", "@file.rsp",
			"--skill", "--version", "Exit codes:",
		})
			Assert.Contains(option, run.Output, StringComparison.Ordinal);
	}

	[Fact]
	public void Generate_help_is_generate_s()
	{
		var run = Tool.Run(Directory.GetCurrentDirectory(), "generate", "--help");

		Assert.Equal(0, run.Exit);
		Assert.StartsWith("dotnet dotgram generate <project.csproj | grammar.gram>", run.Output, StringComparison.Ordinal);
	}

	[Fact]
	public void Skill_prints_the_embedded_guide()
	{
		var run = Tool.Run(Directory.GetCurrentDirectory(), "--skill");

		Assert.Equal(0, run.Exit);
		Assert.StartsWith("---\r\nname: dotgram-cli\r\n", run.Output, StringComparison.Ordinal);
		Assert.Equal(
			File.ReadAllText(Path.Combine(Repository.Root, "src", "DotGram.Cli", "SKILL.md")),
			run.Output);
	}

	[Fact]
	public void Version_names_the_tool_and_the_generator()
	{
		var run = Tool.Run(Directory.GetCurrentDirectory(), "--version");

		Assert.Equal(0, run.Exit);
		Assert.Equal($"dotnet-dotgram {Cli.ToolVersion} (generator {Cli.GeneratorVersion})", run.Output.TrimEnd());
		Assert.DoesNotContain("+", Cli.ToolVersion, StringComparison.Ordinal);
	}

	[Fact]
	public void No_arguments_is_a_usage_error()
	{
		var run = Tool.Run(Directory.GetCurrentDirectory());

		Assert.Equal(ExitCode.Failure, run.Exit);
		Assert.Contains("Usage:", run.Error, StringComparison.Ordinal);
	}
}
