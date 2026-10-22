using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>What stops a run, and what it says: unresolved types, grammar errors, bad input.</summary>
public sealed class RefusalTests
{
	/// <summary>A project whose grammar builds a type from an assembly the tool is not given.</summary>
	static string ProjectWithForeignType(Scratch scratch, string unrelated = "")
	{
		scratch.Write("p/Rows.gram", """
			Row : @Shapes.Point = x: Digits & ',' & y: Digits => @(new Shapes.Point(int.Parse(x), int.Parse(y)))

			Digits = ['0'..'9']+

			parse Row
			""");

		scratch.Write("p/Rows.cs", $$"""
			using DotGram;

			namespace Demo
			{
				[Gram("Rows.gram")]
				public static partial class Rows
				{
					public static Shapes.Point Origin => default;
					{{unrelated}}
				}
			}
			""");

		return scratch.Write("p/p.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <Nullable>enable</Nullable>
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Rows.gram" />
			  </ItemGroup>
			</Project>
			""");
	}

	/// <summary>The generator itself says the type is not in view; the tool says where it may come from.</summary>
	[Fact]
	public void A_missing_type_a_grammar_names_stops_the_run()
	{
		using var scratch = new Scratch();

		var project = ProjectWithForeignType(scratch);
		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("error GRAM3004: No C# type named 'Shapes.Point'", run.Output, StringComparison.Ordinal);
		Assert.Contains("--reference", run.Output, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>
	/// A type a grammar asks about that its host's C# cannot resolve, where the generator would answer
	/// without an error and the parser could differ from a build's: refused unless allowed.
	/// </summary>
	[Fact]
	public void A_missing_type_a_question_asks_about_stops_the_run()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Calc.gram", "Row = 'a'\n\nparse Row\n");
		var run     = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--option", "LocationType=Shapes.ISpan");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("'Shapes' cannot be resolved", run.Output, StringComparison.Ordinal);
		Assert.Contains("--sources", run.Output, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));

		var allowed = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--option", "LocationType=Shapes.ISpan", "--allow-unresolved");

		// Allowed, and then a parser that does not compile, which in this mode is checked.
		Assert.True(allowed.Exit == ExitCode.GrammarErrors, allowed.ToString());
		Assert.Contains("warning: 'Shapes' cannot be resolved", allowed.Output, StringComparison.Ordinal);
	}

	[Fact]
	public void A_missing_type_no_grammar_asks_about_is_one_line()
	{
		using var scratch = new Scratch();

		scratch.Write("p/Rows.gram", """
			Row = ['0'..'9']+

			parse Row
			""");

		scratch.Write("p/Rows.cs", """
			using DotGram;

			namespace Demo
			{
				[Gram("Rows.gram")]
				public static partial class Rows
				{
					public static Elsewhere.Thing? Unrelated => null;
				}
			}
			""");

		var project = scratch.Write("p/p.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Rows.gram" />
			  </ItemGroup>
			</Project>
			""");

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
		Assert.Contains("unresolved names no grammar asks about", run.Output, StringComparison.Ordinal);
		Assert.Contains("Elsewhere", run.Output, StringComparison.Ordinal);
	}

	[Fact]
	public void A_grammar_error_writes_nothing()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Broken.gram", """
			Row = Missing & ';'

			parse Row
			""");

		var run = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.GrammarErrors, run.ToString());
		Assert.Matches(@"Broken\.gram\(1,\d+\): error GRAM\d{4}: ", run.Output);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	[Theory]
	[InlineData("grammar.txt")]
	[InlineData("Project.vbproj")]
	[InlineData("Host.cs")]
	[InlineData("folder")]
	public void An_input_of_no_known_kind_is_refused(string input)
	{
		using var scratch = new Scratch();

		var run = Tool.Run(scratch.Root, "generate", input, "-o", "out");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("neither a project (.csproj) nor a grammar (.gram)", run.Error, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("generate", "x.gram")]
	[InlineData("generate", "x.gram", "-o")]
	[InlineData("generate", "x.gram", "-o", "a", "--frobnicate")]
	[InlineData("generate", "x.gram", "-o", "a", "-o", "b")]
	[InlineData("generate", "x.gram", "y.gram", "-o", "a")]
	[InlineData("generate", "x.gram", "-o", "a", "--line-paths", "sideways")]
	[InlineData("generate", "x.csproj", "-o", "a", "--namespace", "N")]
	[InlineData("generate", "x.gram", "-o", "a", "--package", "NoVersion")]
	[InlineData("frobnicate")]
	public void Usage_errors_exit_3_and_point_at_help(params string[] arguments)
	{
		using var scratch = new Scratch();

		var run = Tool.Run(scratch.Root, arguments);

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("--help", run.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void A_missing_package_names_restore()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Calc.gram", "Row = 'a'\n\nparse Row\n");
		var run     = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--package", "No.Such.Package@1.2.3");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("dotnet restore", run.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void A_project_that_cannot_be_evaluated_exits_3()
	{
		using var scratch = new Scratch();

		var project = scratch.Write("p/p.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</Target></PropertyGroup></Project>");
		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("evaluating", run.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void A_framework_the_tool_has_no_references_for_is_named()
	{
		using var scratch = new Scratch();

		var project = scratch.Write("p/p.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net472</TargetFramework></PropertyGroup></Project>");
		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("'net472'", run.Error, StringComparison.Ordinal);
	}
}
