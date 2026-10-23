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

		Dotnet.Restore(project);

		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("error GRAM3004: No C# type named 'Shapes.Point'", run.Output, StringComparison.Ordinal);
		Assert.Contains("reference the project or assembly that declares it", run.Output, StringComparison.Ordinal);
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
		Assert.Contains("error CS0246: The type or namespace name 'Shapes' could not be found", run.Output, StringComparison.Ordinal);
		Assert.Contains("--sources", run.Output, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));

		var allowed = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--option", "LocationType=Shapes.ISpan", "--allow-unresolved");

		// Allowed, and then a parser that does not compile, which in this mode is checked.
		Assert.True(allowed.Exit == ExitCode.GrammarErrors, allowed.ToString());
		Assert.Contains("warning CS0246: The type or namespace name 'Shapes' could not be found", allowed.Output, StringComparison.Ordinal);
	}

	[Fact]
	public void Any_name_the_build_would_not_resolve_stops_the_run()
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

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		// The build would fail on it too, whether a grammar asks about it or not.
		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Matches("Rows\\.cs\\(\\d+,\\d+\\): error CS0246: The type or namespace name 'Elsewhere' could not be found", run.Output);
		Assert.Contains("your project's build reports this too", run.Output, StringComparison.Ordinal);
		Assert.Contains("a half-written file does too", run.Output, StringComparison.Ordinal);

		var allowed = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--allow-unresolved");

		Assert.True(allowed.Exit == 0, allowed.ToString());
		Assert.Contains("warning CS0246: The type or namespace name 'Elsewhere' could not be found", allowed.Output, StringComparison.Ordinal);
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
	[InlineData("generate", "x.csproj", "-o", "a", "--define", "A")]
	[InlineData("generate", "x.csproj", "-o", "a", "--langversion", "10")]
	[InlineData("generate", "x.csproj", "-o", "a", "--reference", "r.dll")]
	[InlineData("generate", "x.csproj", "-o", "a", "--package", "Name@1.0.0")]
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
	public void Options_a_project_s_build_decides_are_for_a_gram_only()
	{
		using var scratch = new Scratch();

		var run = Tool.Run(scratch.Root, "generate", "x.csproj", "-o", "a", "--define", "A", "--langversion", "10");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("--define, --langversion: only for a .gram input; a project's build decides this", run.Error, StringComparison.Ordinal);
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

	/// <summary>A project whose grammar builds Node from the sources, Node and what it rests on declared as given.</summary>
	static string ProjectBuildingNode(Scratch scratch, string declarations, string outside = "")
	{
		scratch.Write("p/Nodes.gram", """
			Node : @Node = name: Word => @(new Node(name))

			Word = ['a'..'z']+

			parse Node
			""");

		scratch.Write("p/Nodes.cs", $$"""
			using DotGram;
			{{outside}}

			namespace Demo
			{
				[Gram("Nodes.gram")]
				public static partial class Nodes { }

				{{declarations}}
			}
			""");

		return scratch.Write("p/p.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Nodes.gram" />
			  </ItemGroup>
			</Project>
			""");
	}

	/// <summary>
	/// A name nothing resolves, wherever it stands in the sources — a base, an alias to one, a delegate's
	/// return, a type in another namespace of the same simple name as the parser's, a constructor's
	/// initializer, a body: the build would fail on it, and the run stops.
	/// </summary>
	[Theory]
	[InlineData("public class Node : Models.Base { public Node(string name) { } }")]
	[InlineData("public class Node : Base { public Node(string name) { } } public class Base : Models.Root { }")]
	[InlineData("public class Node { public Node(string name) { } public Models.Kind Kind { get; set; } }")]
	[InlineData("public class Node : Parent { public Node(string name) { } } public class Base : Models.Root { }", "using Parent = Demo.Base;")]
	[InlineData("public class Node { public Node(string name) { } public Factory? Make { get; set; } } public delegate Models.Kind Factory();")]
	[InlineData("public class Node : Foreign.Demo { public Node(string name) { } }", "namespace Foreign { public class Other { } }")]
	[InlineData("public class Node { public Node(string name) : this(new Models.Thing()) { } Node(object value) { } }")]
	[InlineData("public class Node { public Node(string name) { var x = new Models.Thing(); } }")]
	public void A_missing_name_anywhere_in_the_sources_stops_the_run(string declarations, string outside = "")
	{
		using var scratch = new Scratch();

		var project = ProjectBuildingNode(scratch, declarations, outside);

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Matches("error CS\\d{4}: .*'(Models|Foreign|Demo)'", run.Output);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	[Fact]
	public void A_compile_item_that_does_not_exist_exits_3()
	{
		using var scratch = new Scratch();

		var project = scratch.Write("p/p.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			  </PropertyGroup>
			  <ItemGroup>
			    <Compile Include="Missing.cs" />
			  </ItemGroup>
			</Project>
			""");

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("cannot read", run.Error, StringComparison.Ordinal);
		Assert.Contains("Missing.cs", run.Error, StringComparison.Ordinal);
	}

	/// <summary>
	/// Each framework its own references: System.Threading.Lock came with .NET 9, and a grammar that
	/// builds one is refused for net8.0 as a net8.0 build refuses it.
	/// </summary>
	[Fact]
	public void A_type_a_later_framework_added_is_missing_from_an_earlier_one()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Locks.gram", "Gate : @System.Threading.Lock = 'g' => @(new System.Threading.Lock())\n\nparse Gate\n");

		var net9 = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("net9"), "--framework", "net9.0");
		var net8 = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("net8"), "--framework", "net8.0");

		Assert.True(net9.Exit == 0, net9.ToString());
		Assert.True(net8.Exit == ExitCode.Failure, net8.ToString());
		Assert.Contains("System.Threading.Lock", net8.Output, StringComparison.Ordinal);

		Assert.NotSame(Framework.Of("net8.0")!.References[0], Framework.Of("net10.0")!.References[0]);
	}

	[Fact]
	public void A_platform_framework_for_a_lone_grammar_is_refused()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Calc.gram", "Row = 'a'\n\nparse Row\n");
		var run     = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--framework", "net8.0-windows");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("only a project can say", run.Error, StringComparison.Ordinal);
	}

	/// <summary>
	/// <c>--framework</c> naming one the project does not target is said so, not reported as a project
	/// that is not restored (what MSBuild concludes from the missing assets).
	/// </summary>
	[Fact]
	public void A_framework_the_project_does_not_target_is_named_as_such()
	{
		using var scratch = new Scratch();

		var project = ProjectWithForeignType(scratch);

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--framework", "net8.0");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		// The project's path is printed as MSBuild gives it, which on Windows can differ from the one passed
		// (a short or a long form of the temporary folder), so only its file name is compared.
		Assert.Contains("--framework net8.0: ", run.Error, StringComparison.Ordinal);
		Assert.Contains(Path.GetFileName(project) + " targets net10.0.", run.Error, StringComparison.Ordinal);
		Assert.DoesNotContain("not restored", run.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>
	/// A project that names no framework of its own is generated at the <c>--framework</c> it is given, as
	/// <c>dotnet build -f</c> builds it: only a framework the project does list is checked.
	/// </summary>
	[Fact]
	public void A_project_that_names_no_framework_is_taken_at_the_one_given()
	{
		using var scratch = new Scratch();

		var project = ProjectWithForeignType(scratch);

		File.WriteAllText(project, File.ReadAllText(project).Replace("<TargetFramework>net10.0</TargetFramework>", "", StringComparison.Ordinal));
		Dotnet.Ok(scratch.Root, "restore", project, "-nologo", "-p:TargetFramework=net10.0");

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--framework", "net10.0");

		// The grammar's type is not in view, so the run stops there: past the frameworks, as it should.
		Assert.DoesNotContain("names no target framework", run.Error, StringComparison.Ordinal);
		Assert.Contains("GRAM3004", run.Output, StringComparison.Ordinal);
	}

	/// <summary>
	/// A project whose path has <c>%XX</c>, which MSBuild reads as an escaped character, is refused as
	/// that, not as a project that is not restored.
	/// </summary>
	[Fact]
	public void A_project_path_MSBuild_would_unescape_is_said_so()
	{
		using var scratch = new Scratch();

		var project = scratch.Write("pct %41/p.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("'%41', which MSBuild reads as an escaped character", run.Error, StringComparison.Ordinal);
		Assert.DoesNotContain("not restored", run.Error, StringComparison.Ordinal);
	}
}
