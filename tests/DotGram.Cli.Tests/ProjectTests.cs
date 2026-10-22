using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// Projects as a user has them: settings the generator reads, the SDK's own defaults for a framework,
/// a generated folder moved elsewhere, and a grammar built on one in another assembly — each held to
/// what a build of the same project generates.
/// </summary>
public sealed class ProjectTests
{
	const string Url = "Url.gram";

	static string Snapshot(string name)
	{
		return File.ReadAllText(Path.Combine(Repository.Snapshots, name));
	}

	/// <summary>A project in <c>p/</c> under the scratch root, with one grammar and its host.</summary>
	static string Project(Scratch scratch, string framework, string grammar, string settings = "", string items = "")
	{
		var name = Path.GetFileNameWithoutExtension(grammar);

		scratch.Write("p/" + grammar, Snapshot(grammar));
		scratch.Write("p/Host.cs", $$"""
			using DotGram;

			namespace Demo
			{
				[Gram("{{grammar}}")]
				public partial class {{name}} { }
			}
			""");

		return scratch.Write("p/p.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{framework}</TargetFramework>
			    <Nullable>enable</Nullable>
			    {settings}
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="{grammar}" />
			    {items}
			  </ItemGroup>
			  {Build.AnalyzerItems}
			</Project>
			""");
	}

	/// <summary>
	/// The language version, a symbol the generator writes text behind, and the trace build property,
	/// set in the project: the evaluation carries all three to the generator.
	/// </summary>
	[Fact]
	public void Settings_in_the_project_reach_the_generator()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0", Url, """
			<LangVersion>10</LangVersion>
			<DefineConstants>$(DefineConstants);DOTGRAM_CHECKS</DefineConstants>
			<DotGramTrace>true</DotGramTrace>
			""");

		var built = Build.Generated(project);
		var run   = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Equality.Same(built, scratch.Path("out"));

		// The trace build is what the property asked for: without it the parser has no sink.
		Assert.Contains("GramTrace", File.ReadAllText(scratch.Path("out", "Demo.Url.g.cs")), StringComparison.Ordinal);
	}

	/// <summary>
	/// The language version and the symbols are a project's own: its build decides them, and an
	/// option that would decide them is for a lone grammar.
	/// </summary>
	[Fact]
	public void The_language_version_and_symbols_are_the_build_s_to_decide()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0", "Lexical.gram");

		foreach (var option in new[] { new[] { "--langversion", "10" }, ["--define", "DOTGRAM_CHECKS"], ["--package", "System.Memory@4.6.3"], ["--reference", "x.dll"] })
		{
			var run = Tool.Run(scratch.Root, ["generate", project, "-o", scratch.Path("out"), .. option]);

			Assert.True(run.Exit == ExitCode.Failure, run.ToString());
			Assert.Contains("a project's build decides this", run.Error, StringComparison.Ordinal);
		}

		// Set in the project, as the build takes them: the same as the build.
		File.WriteAllText(project, File.ReadAllText(project).Replace("<Nullable>", "<LangVersion>10</LangVersion><DefineConstants>$(DefineConstants);DOTGRAM_CHECKS</DefineConstants><Nullable>", StringComparison.Ordinal));

		var built = Build.Generated(project);
		var run2  = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run2.Exit == 0, run2.ToString());
		Equality.Same(built, scratch.Path("out"));
	}

	/// <summary>
	/// <c>dotnet new classlib</c> for net8.0, a grammar added and nothing else: the SDK's C# 12, the
	/// project's implicit usings. The project, and the grammar alone as a <c>.gram</c>, generate what the
	/// build does, and the folder compiles in the project once the generator is gone from it.
	/// </summary>
	[Fact]
	public void A_new_net8_class_library()
	{
		using var scratch = new Scratch();

		var project = NewLibrary(scratch, "net8.0");
		var built   = Build.Generated(project);

		var fromProject = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(fromProject.Exit == 0, fromProject.ToString());
		Equality.Same(built, scratch.Path("out"));

		var fromGrammar = Tool.Run(
			scratch.Root, "generate", scratch.Path("lib", "Csv.gram"), "-o", scratch.Path("gram"), "--line-paths", "absolute",
			"--framework", "net8.0", "--namespace", "Lib", "--class", "Csv", "--public");

		Assert.True(fromGrammar.Exit == 0, fromGrammar.ToString());
		Assert.Equal(File.ReadAllBytes(Path.Combine(built, "Lib.Csv.g.cs")), File.ReadAllBytes(scratch.Path("gram", "Lib.Csv.g.cs")));

		CompilesWithoutTheGenerator(scratch, project);
	}

	/// <summary>
	/// <c>dotnet new classlib</c> for netstandard2.0: C# 7.3 by the SDK's default, which a generated
	/// parser cannot be compiled at: the generator says so in a build as much as here, and the tool
	/// writes nothing. With <c>LangVersion</c> 8 the project generates what its build does, and the
	/// folder compiles.
	/// </summary>
	[Fact]
	public void A_new_netstandard20_class_library()
	{
		using var scratch = new Scratch();

		var project = NewLibrary(scratch, "netstandard2.0", """<PackageReference Include="System.Memory" Version="4.6.3" />""");
		var (exit, text, _) = Build.TryGenerated(project);

		Assert.True(exit != 0 && text.Contains("GRAM0010", StringComparison.Ordinal), text);

		var refused = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(refused.Exit == ExitCode.GrammarErrors && refused.Output.Contains("GRAM0010", StringComparison.Ordinal), refused.ToString());
		Assert.False(Directory.Exists(scratch.Path("out")));

		var fromGrammar = Tool.Run(
			scratch.Root, "generate", scratch.Path("lib", "Csv.gram"), "-o", scratch.Path("gram"),
			"--framework", "netstandard2.0", "--package", "System.Memory@4.6.3", "--namespace", "Lib", "--class", "Csv", "--public");

		Assert.True(fromGrammar.Exit == ExitCode.GrammarErrors && fromGrammar.Output.Contains("GRAM0010", StringComparison.Ordinal), fromGrammar.ToString());
		Assert.False(Directory.Exists(scratch.Path("gram")));

		var at8 = Tool.Run(
			scratch.Root, "generate", scratch.Path("lib", "Csv.gram"), "-o", scratch.Path("gram"), "--line-paths", "absolute", "--langversion", "8",
			"--framework", "netstandard2.0", "--package", "System.Memory@4.6.3", "--namespace", "Lib", "--class", "Csv", "--public");

		Assert.True(at8.Exit == 0, at8.ToString());

		File.WriteAllText(project, File.ReadAllText(project).Replace("</PropertyGroup>", "<LangVersion>8.0</LangVersion></PropertyGroup>", StringComparison.Ordinal));

		var built8 = Build.Generated(project);

		Assert.Equal(File.ReadAllBytes(Path.Combine(built8, "Lib.Csv.g.cs")), File.ReadAllBytes(scratch.Path("gram", "Lib.Csv.g.cs")));

		var fromProject = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(fromProject.Exit == 0, fromProject.ToString());
		Equality.Same(built8, scratch.Path("out"));

		CompilesWithoutTheGenerator(scratch, project);
	}

	static string NewLibrary(Scratch scratch, string framework, string items = "")
	{
		// The SDK's template offers only its own framework and the two netstandard ones: an older .NET
		// is the same project with its TargetFramework changed, which is what a user does too.
		var template = framework.StartsWith("netstandard", StringComparison.Ordinal) ? framework : "net10.0";

		Dotnet.Ok(scratch.Root, "new", "classlib", "-f", template, "-o", "lib", "-n", "Lib");

		File.Delete(scratch.Path("lib", "Class1.cs"));

		scratch.Write("lib/Csv.gram", Snapshot("Csv.gram"));
		scratch.Write("lib/Csv.cs", """
			using DotGram;

			namespace Lib
			{
				[Gram("Csv.gram")]
				public partial class Csv { }
			}
			""");

		var project = scratch.Path("lib", "Lib.csproj");

		File.WriteAllText(project, File.ReadAllText(project).Replace("<TargetFramework>" + template + "<", "<TargetFramework>" + framework + "<", StringComparison.Ordinal));
		File.WriteAllText(project, File.ReadAllText(project).Replace(
			"</Project>",
			$"""
			  <ItemGroup>
			    <AdditionalFiles Include="Csv.gram" />
			    {items}
			  </ItemGroup>
			  {Build.AnalyzerItems}
			</Project>
			""",
			StringComparison.Ordinal));

		return project;
	}

	/// <summary>The project with the generator taken out and the tool's folder put in, built.</summary>
	static void CompilesWithoutTheGenerator(Scratch scratch, string project)
	{
		var text = File.ReadAllText(project);
		var at   = text.IndexOf("<Analyzer ", StringComparison.Ordinal);
		var end  = text.IndexOf("/>", at, StringComparison.Ordinal) + 2;

		File.WriteAllText(project, text.Remove(at, end - at).Replace(
			"<AdditionalFiles Include=\"Csv.gram\" />",
			$"<Compile Include=\"{scratch.Path("out")}/*.cs\" />",
			StringComparison.Ordinal));

		Build.Run(project);
	}

	/// <summary>
	/// Generated with relative <c>#line</c> paths, the tree moved, built without the generator: an
	/// error in a hook names the grammar where it now is, at the hook's line.
	/// </summary>
	[Fact]
	public void A_moved_folder_still_points_at_its_grammar()
	{
		using var scratch = new Scratch();

		scratch.Write("before/Calc.gram", """
			trivia = ' '*

			Number : @int = digits: ['0'..'9']+ => @(digits + 1)

			parse Number
			""");

		scratch.Write("before/Calc.cs", """
			using DotGram;

			namespace Demo
			{
				[Gram("Calc.gram")]
				public static partial class Calc { }
			}
			""");

		var project = scratch.Write("before/Calc.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <Nullable>enable</Nullable>
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Calc.gram" />
			  </ItemGroup>
			</Project>
			""");

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("before", "Generated"));

		Assert.True(run.Exit == 0, run.ToString());

		Directory.Move(scratch.Path("before"), scratch.Path("after"));

		var (exit, output) = Build.Try(scratch.Path("after", "Calc.csproj"));

		// `digits + 1` is a string, and the rule promises an int: the compiler says so at the grammar's line 3.
		Assert.True(exit != 0, output);
		Assert.Contains(scratch.Path("after", "Calc.gram") + "(3,", output, StringComparison.Ordinal);
		Assert.Contains("CS0029", output, StringComparison.Ordinal);
	}

	/// <summary>
	/// A grammar built on one in a referenced assembly (<c>[GramInclude]</c>): the generator reads the
	/// included grammar from it, as in a build.
	/// </summary>
	[Fact]
	public void A_grammar_included_from_an_assembly()
	{
		using var scratch = new Scratch();

		scratch.Write("lexemes/Lexemes.cs", """"
			using DotGram;

			namespace Lexicon
			{
				[Gram("""
					Word   = ['a'..'z']+
					Digits = ['0'..'9']+
					""")]
				public static partial class Lexemes { }
			}
			"""");

		var library = scratch.Write("lexemes/Lexemes.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <Nullable>enable</Nullable>
			  </PropertyGroup>
			  {Build.AnalyzerItems}
			</Project>
			""");

		Build.Run(library, "OutputPath=" + scratch.Path("lexemes-bin") + "/");

		var assembly = scratch.Path("lexemes-bin", "Lexemes.dll");

		scratch.Write("settings/Settings.cs", """"
			using DotGram;

			namespace Demo
			{
				[GramInclude(typeof(Lexicon.Lexemes), As = "Lex")]
				[Gram("""
					trivia = ' '*

					Setting : @string = key: Lex.Word & '=' & value: Lex.Digits => @(key + " is " + value)

					parse Setting
					""")]
				public static partial class Settings { }
			}
			"""");

		var consumer = scratch.Write("settings/Settings.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <Nullable>enable</Nullable>
			  </PropertyGroup>
			  <ItemGroup>
			    <Reference Include="{assembly}" />
			  </ItemGroup>
			  {Build.AnalyzerItems}
			</Project>
			""");

		var built = Build.Generated(consumer);
		var run   = Tool.Run(scratch.Root, "generate", consumer, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Equality.Same(built, scratch.Path("out"));

		// And without the reference the include is a type that cannot be found, which the build reports too.
		File.WriteAllText(consumer, File.ReadAllText(consumer).Replace($"<Reference Include=\"{assembly}\" />", "", StringComparison.Ordinal));

		var without = Tool.Run(scratch.Root, "generate", consumer, "-o", scratch.Path("without"));

		Assert.True(without.Exit != 0, without.ToString());
		Assert.False(Directory.Exists(scratch.Path("without")));
	}

	/// <summary>A project that targets two frameworks gets one folder, the two generations being equal.</summary>
	[Fact]
	public void Every_framework_of_a_project_is_generated()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0", Url);

		File.WriteAllText(project, File.ReadAllText(project)
			.Replace("<TargetFramework>net10.0</TargetFramework>", "<TargetFrameworks>net8.0;net10.0</TargetFrameworks>", StringComparison.Ordinal));

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
		Assert.Contains("  net8.0: ", run.Output, StringComparison.Ordinal);
		Assert.Contains("  net10.0: ", run.Output, StringComparison.Ordinal);
	}

	/// <summary>
	/// A host whose types the grammar builds exist only under a symbol the SDK adds for the framework or
	/// platform: the symbols are the build's, so the parser is.
	/// </summary>
	[Theory]
	[InlineData("net8.0-windows", "WINDOWS", "<EnableWindowsTargeting>true</EnableWindowsTargeting>")]
	[InlineData("net8.0-windows", "WINDOWS7_0_OR_GREATER", "<EnableWindowsTargeting>true</EnableWindowsTargeting>")]
	[InlineData("net10.0", "NET10_0_OR_GREATER", "")]
	[InlineData("net10.0", "!NET10_0_OR_GREATER", "<DisableImplicitFrameworkDefines>true</DisableImplicitFrameworkDefines>")]
	public void The_framework_s_symbols_are_the_build_s(string framework, string symbol, string settings)
	{
		using var scratch = new Scratch();

		scratch.Write("p/Pairs.gram", "Pair : @Pair = key: Word & '=' & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\n");
		scratch.Write("p/Pairs.cs", $$"""
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs { }

			#if {{symbol}}
				public sealed class Pair
				{
					public Pair(string key, string value) { Key = key; Value = value; }

					public string Key { get; }
					public string Value { get; }
				}
			#else
				public sealed class Pair
				{
					public string Key { get; set; } = "";
					public string Value { get; set; } = "";
				}
			#endif
			}
			""");

		var project = scratch.Write("p/p.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{framework}</TargetFramework>
			    <Nullable>enable</Nullable>
			    {settings}
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Pairs.gram" />
			  </ItemGroup>
			  {Build.AnalyzerItems}
			</Project>
			""");

		var built = Build.Generated(project);
		var run   = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Equality.Same(built, scratch.Path("out"));

		// And the symbol decided something: with the other Pair the parser builds it otherwise.
		var host = scratch.Path("p", "Pairs.cs");

		File.WriteAllText(host, File.ReadAllText(host).Replace("#if " + symbol, "#if !(" + symbol + ")", StringComparison.Ordinal));

		var other = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("other"), "--line-paths", "absolute");

		Assert.True(other.Exit == 0, other.ToString());
		Assert.NotEqual(File.ReadAllText(scratch.Path("out", "Demo.Pairs.g.cs")), File.ReadAllText(scratch.Path("other", "Demo.Pairs.g.cs")));
	}

	/// <summary>Two frameworks that generate different parsers for one folder: refused, naming both.</summary>
	[Fact]
	public void Frameworks_that_generate_differently_are_refused()
	{
		using var scratch = new Scratch();

		scratch.Write("p/Pairs.gram", "Pair : @Pair = key: Word & '=' & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\n");
		scratch.Write("p/Pairs.cs", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs { }

			#if NET10_0_OR_GREATER
				public sealed class Pair
				{
					public Pair(string key, string value) { Key = key; Value = value; }

					public string Key { get; }
					public string Value { get; }
				}
			#else
				public sealed class Pair
				{
					public string Key { get; set; } = "";
					public string Value { get; set; } = "";
				}
			#endif
			}
			""");

		var project = scratch.Write("p/p.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Pairs.gram" />
			  </ItemGroup>
			</Project>
			""");

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("the parser generated for net8.0 differs from the one for net10.0", run.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}
}
