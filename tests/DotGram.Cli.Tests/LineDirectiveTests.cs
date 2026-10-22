using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>The <c>#line</c> rewrite: exactly the directives that name an input, and no other byte.</summary>
public sealed class LineDirectiveTests
{
	static readonly string Root = OperatingSystem.IsWindows() ? @"C:\repo" : "/repo";

	static string In(params string[] parts)
	{
		return Path.Combine([Root, .. parts]);
	}

	static readonly HashSet<string> Inputs = [In("src", "Calc.gram"), In("src", "Host.cs")];

	[Fact]
	public void A_directive_naming_an_input_is_made_relative_to_the_folder()
	{
		var text = $"a\r\n#line 12 \"{In("src", "Calc.gram")}\"\r\n    x;\r\n#line default\r\n#line 3 \"{In("src", "Host.cs")}\"\n";

		Assert.Equal(
			"a\r\n#line 12 \"../Calc.gram\"\r\n    x;\r\n#line default\r\n#line 3 \"../Host.cs\"\n",
			LineDirectives.Relative(text, Inputs, In("src", "Generated")));
	}

	[Fact]
	public void Deeper_and_shallower_folders()
	{
		var text = $"#line 1 \"{In("src", "Calc.gram")}\"";

		Assert.Equal("#line 1 \"Calc.gram\"", LineDirectives.Relative(text, Inputs, In("src")));
		Assert.Equal("#line 1 \"../../src/Calc.gram\"", LineDirectives.Relative(text, Inputs, In("out", "deep")));
	}

	[Theory]
	[InlineData("#line 1 \"x\"")]
	[InlineData("#line 1 \"/elsewhere/Calc.gram\"")]
	[InlineData("#line default")]
	[InlineData("#line hidden")]
	[InlineData(" #line 1 \"PATH\"")]
	[InlineData("#line (1, 1) - (1, 5) \"PATH\"")]
	[InlineData("#line 1 \"PATH\" // trailing")]
	[InlineData("#linex 1 \"PATH\"")]
	[InlineData("// #line 1 \"PATH\"")]
	public void Anything_else_is_left_as_it_is(string line)
	{
		var text = "before\r\n" + line.Replace("PATH", In("src", "Calc.gram"), StringComparison.Ordinal) + "\r\nafter";

		Assert.Same(text, LineDirectives.Relative(text, Inputs, In("src", "Generated")));
	}

	/// <summary>A hook's raw string holding a directive's text, on a line of its own: it names no input.</summary>
	[Fact]
	public void A_directive_s_text_inside_a_hook_string_is_untouched()
	{
		var text = $"var s = \"\"\"\r\n#line 1 \"x\"\r\n\"\"\";\r\n#line 4 \"{In("src", "Calc.gram")}\"\r\n";

		Assert.Equal(
			"var s = \"\"\"\r\n#line 1 \"x\"\r\n\"\"\";\r\n#line 4 \"../Calc.gram\"\r\n",
			LineDirectives.Relative(text, Inputs, In("src", "Generated")));
	}

	/// <summary>The same line with an input's own path, inside a hook's raw string: the hook's text, untouched.</summary>
	[Fact]
	public void A_raw_string_holding_an_input_s_directive_is_untouched()
	{
		var path = In("src", "Calc.gram");
		var text = $"class C\r\n{{\r\n\tstring s = \"\"\"\r\n#line 4 \"{path}\"\r\n\"\"\";\r\n#line 9 \"{path}\"\r\n\tint x;\r\n}}\r\n";

		Assert.Equal(
			text.Replace($"#line 9 \"{path}\"", "#line 9 \"../Calc.gram\"", StringComparison.Ordinal),
			LineDirectives.Relative(text, Inputs, In("src", "Generated")));
	}

	/// <summary>And in an interpolated raw string, whose text is a token of its own.</summary>
	[Fact]
	public void An_interpolated_raw_string_is_untouched()
	{
		var path = In("src", "Calc.gram");
		var text = $"class C\r\n{{\r\n\tstring s = $\"\"\"\r\n{{1}}\r\n#line 4 \"{path}\"\r\n\"\"\";\r\n}}\r\n";

		Assert.Same(text, LineDirectives.Relative(text, Inputs, In("src", "Generated")));
	}

	/// <summary>On a real parser: every byte outside the rewritten paths is the generator's.</summary>
	[Fact]
	public void Nothing_but_the_paths_changes_in_a_generated_parser()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Csv.gram", File.ReadAllText(Path.Combine(Repository.Snapshots, "Csv.gram")));

		Assert.Equal(0, Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("abs"), "--line-paths", "absolute").Exit);
		Assert.Equal(0, Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("rel")).Exit);

		var absolute = File.ReadAllText(scratch.Path("abs", "Csv.g.cs"));
		var relative = File.ReadAllText(scratch.Path("rel", "Csv.g.cs"));

		Assert.Contains("#line ", absolute, StringComparison.Ordinal);
		Assert.Contains("\"../Csv.gram\"", relative, StringComparison.Ordinal);
		Assert.Equal(absolute, relative.Replace("\"../Csv.gram\"", "\"" + grammar + "\"", StringComparison.Ordinal));
	}
}
