using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// What makes the tool's compilation the build's: the project's other generators, its packages from
/// what restore wrote, the SDK's own symbols — and nothing of the project's run to get them.
/// </summary>
public sealed class BuildEqualTests
{
	const string Pairs = "Pair = key: Word & '=' & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\n";

	static string Project(Scratch scratch, string framework, string host, string settings = "", string items = "", string grammar = Pairs)
	{
		scratch.Write("p/Pairs.gram", grammar);
		scratch.Write("p/Pairs.cs", host);

		return scratch.Write("p/p.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{framework}</TargetFramework>
			    <Nullable>enable</Nullable>
			    {settings}
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Pairs.gram" />
			    {items}
			  </ItemGroup>
			</Project>
			""");
	}

	/// <summary>The generator as the build's analyzer, inside an item group.</summary>
	static string Analyzer => Build.AnalyzerItems.Replace("<ItemGroup>", "", StringComparison.Ordinal).Replace("</ItemGroup>", "", StringComparison.Ordinal);

	const string PlainHost = """
		using DotGram;

		namespace Demo
		{
			[Gram("Pairs.gram")]
			public static partial class Pairs { }
		}
		""";

	[Fact]
	public void A_project_not_restored_is_refused()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0", PlainHost);
		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("run `dotnet restore`", run.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("p", "obj")));
	}

	/// <summary>
	/// A host whose code names what DotGram generates — the parser's own class of a rule — and a
	/// package generator's types on netstandard2.0 (Meziantou.Polyfill's attributes and
	/// IsExternalInit), with no flag: the package and its generator come from what restore wrote.
	/// </summary>
	[Fact]
	public void Names_from_generators_are_excused_and_nothing_else()
	{
		using var scratch = new Scratch();

		var project = Project(
			scratch,
			"netstandard2.0",
			"""
			using System.Diagnostics.CodeAnalysis;

			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					public static Pair? Last { get; set; }

					public static bool Has([NotNullWhen(true)] string? text) { return text is not null; }
				}

				public sealed record Point(int X, int Y);
			}
			""",
			"<LangVersion>latest</LangVersion><MeziantouPolyfill_IncludedPolyfills>T:System.Diagnostics.CodeAnalysis.NotNullWhenAttribute|T:System.Runtime.CompilerServices.IsExternalInit</MeziantouPolyfill_IncludedPolyfills>",
			"""
			<PackageReference Include="Meziantou.Polyfill" Version="1.0.165" PrivateAssets="all" />
			<PackageReference Include="System.Memory" Version="4.6.3" />
			""" + Analyzer);

		var built = Build.Generated(project);
		var run   = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Assert.DoesNotContain("cannot be resolved", run.Output, StringComparison.Ordinal);
		Equality.Same(built, scratch.Path("out"));
	}

	/// <summary>An analyzer project reference is the one generator the tool cannot run, and says so.</summary>
	[Fact]
	public void An_analyzer_project_reference_is_warned_about()
	{
		using var scratch = new Scratch();

		scratch.Write("gen/Gen.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>netstandard2.0</TargetFramework></PropertyGroup></Project>");

		var project = Project(scratch, "net10.0", PlainHost, items: """<ProjectReference Include="../gen/Gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />""");

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
		Assert.Contains("Gen.csproj is an analyzer project reference: a project generator is not run", run.Output, StringComparison.Ordinal);
	}

	[Fact]
	public void The_newest_roslyn_folder_the_tool_can_load_wins()
	{
		Assert.Equal(
			["analyzers/dotnet/roslyn4.8/cs/G.dll"],
			ProjectAssets.AnalyzerFiles(
			[
				"analyzers/dotnet/cs/G.dll",
				"analyzers/dotnet/roslyn3.11/cs/G.dll",
				"analyzers/dotnet/roslyn4.8/cs/G.dll",
				"analyzers/dotnet/roslyn4.8/vb/G.dll",
				"analyzers/dotnet/roslyn99.0/cs/G.dll",
				"lib/netstandard2.0/G.dll",
			]));

		Assert.Equal(
			["analyzers/dotnet/cs/G.dll", "analyzers/dotnet/H.dll"],
			ProjectAssets.AnalyzerFiles(["analyzers/dotnet/cs/G.dll", "analyzers/dotnet/H.dll", "analyzers/dotnet/vb/G.dll"]));
	}

	/// <summary>
	/// A generator whose type cannot be loaded (its base class is in an assembly that is not there):
	/// a warning naming it, and no generator.
	/// </summary>
	[Fact]
	public void A_generator_that_does_not_load_is_a_warning()
	{
		using var scratch = new Scratch();

		var references = Basic.Reference.Assemblies.Net100.References.All.Append(MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location)).ToList();

		var missing = Emit(scratch.Path("missing", "Missing.dll"), "Missing", "namespace M { public abstract class Base { } }", references);
		var gen     = Emit(
			scratch.Path("gen", "Gen.dll"),
			"Gen",
			"""
			using Microsoft.CodeAnalysis;

			[Generator]
			public sealed class G : M.Base, IIncrementalGenerator
			{
				public void Initialize(IncrementalGeneratorInitializationContext context) { }
			}
			""",
			[.. references, MetadataReference.CreateFromFile(missing)]);

		Assert.True(OtherGenerators.HasGenerator(gen));
		Assert.False(OtherGenerators.HasGenerator(missing));

		var (generators, warnings) = OtherGenerators.Load([gen]);

		Assert.Empty(generators);
		Assert.Contains(warnings, warning => warning.Contains("Gen.dll could not be loaded", StringComparison.Ordinal));
	}

	static string Emit(string path, string name, string source, IEnumerable<MetadataReference> references)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		var result = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source)], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.Emit(path);

		Assert.True(result.Success, string.Join("\n", result.Diagnostics));

		return path;
	}

	/// <summary>
	/// The symbols the tool works out are the build's, for every pair of the two properties that take
	/// them away, in both configurations: held to the build's own final DefineConstants.
	/// </summary>
	[Theory]
	[InlineData("false", "false", "Release")]
	[InlineData("true", "false", "Release")]
	[InlineData("false", "true", "Release")]
	[InlineData("true", "true", "Release")]
	[InlineData("true", "true", "Debug")]
	[InlineData("false", "false", "Debug")]
	public async Task The_symbols_are_the_build_s(string noFramework, string noTracing, string configuration)
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0", PlainHost, $"""
			<DisableImplicitFrameworkDefines>{noFramework}</DisableImplicitFrameworkDefines>
			<DisableDiagnosticTracing>{noTracing}</DisableDiagnosticTracing>
			<DefineConstants>$(DefineConstants);MINE</DefineConstants>
			""", Analyzer);

		var built = Dotnet.Ok(scratch.Path("p"), "msbuild", project, "-restore", "-nologo", "-nodeReuse:false", "-t:Build", "-p:Configuration=" + configuration, "-getProperty:DefineConstants").Trim();
		var tool  = await ProjectEvaluation.RunAsync(project, "net10.0", configuration);

		Assert.Equal(
			built.Split(';', StringSplitOptions.RemoveEmptyEntries).Order(StringComparer.Ordinal),
			tool.DefineConstants.Order(StringComparer.Ordinal));
	}

	[Fact]
	public async Task A_platform_framework_has_its_platform_s_symbols()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0-windows", PlainHost, "<EnableWindowsTargeting>true</EnableWindowsTargeting>");
		var tool    = await ProjectEvaluation.RunAsync(project, "net10.0-windows", "Release");

		Assert.Contains("WINDOWS", tool.DefineConstants);
		Assert.Contains("NET10_0_OR_GREATER", tool.DefineConstants);
	}

	/// <summary>The SDK's targets in the tool's own project need nothing restored: it never is.</summary>
	[Fact]
	public async Task The_symbols_need_no_restore()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net8.0", PlainHost);
		var (defines, _) = await ProjectEvaluation.ImplicitDefinesAsync(project, "net8.0", "Release", ["TRACE", "RELEASE"], new Dictionary<string, string?>());

		Assert.Contains("NET8_0_OR_GREATER", defines);
		Assert.DoesNotContain("NET9_0_OR_GREATER", defines);
		Assert.False(Directory.Exists(scratch.Path("p", "obj")));
	}

	/// <summary>
	/// A project whose InitialTargets write a file: generating it runs none of its targets, so nothing
	/// appears under it that was not there before.
	/// </summary>
	[Fact]
	public void None_of_the_project_s_targets_runs()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0", PlainHost);

		File.WriteAllText(project, File.ReadAllText(project)
			.Replace("<Project Sdk=\"Microsoft.NET.Sdk\">", "<Project Sdk=\"Microsoft.NET.Sdk\" InitialTargets=\"Stamp\">", StringComparison.Ordinal)
			.Replace("</Project>", """
				  <Target Name="Stamp">
				    <WriteLinesToFile File="$(MSBuildProjectDirectory)/obj/stamp" Lines="ran" Overwrite="true" />
				  </Target>
				</Project>
				""", StringComparison.Ordinal));

		Dotnet.Restore(project);
		File.Delete(scratch.Path("p", "obj", "stamp"));

		var before = Listing(scratch.Path("p"));
		var run    = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
		Assert.False(File.Exists(scratch.Path("p", "obj", "stamp")));
		Assert.Equal(before, Listing(scratch.Path("p")));
	}

	static List<string> Listing(string directory)
	{
		return [.. Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(directory, path) + " " + File.GetLastWriteTimeUtc(path).Ticks)
			.Order(StringComparer.Ordinal)];
	}

	[Fact]
	public void The_output_folder_is_asked_how_it_compares_names()
	{
		using var scratch = new Scratch();

		Directory.CreateDirectory(scratch.Path("out"));

		var probed = FileSystem.NamesIn(scratch.Path("out"));

		// The probe leaves nothing behind.
		Assert.Empty(Directory.GetFileSystemEntries(scratch.Path("out")));

		File.WriteAllText(scratch.Path("out", "Case"), "");

		var insensitive = File.Exists(scratch.Path("out", "CASE"));

		Assert.Same(insensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal, probed);
	}

	/// <summary>On a file system that tells case apart, Model.cs and model.cs are two files, and both are compiled.</summary>
	[Fact]
	public void Two_files_differing_in_case_are_both_compiled()
	{
		using var scratch = new Scratch();

		Directory.CreateDirectory(scratch.Path("p"));

		if (FileSystem.NamesIn(scratch.Path("p")) != StringComparer.Ordinal)
			return;

		var project = Project(scratch, "net10.0", PlainHost, grammar: "Pair : @Model2 = key: Word => @(new Model2(key))\n\nWord = ['a'..'z']+\n\nparse Pair\n");

		scratch.Write("p/Model.cs", "namespace Demo { public sealed class Model1 { } }");
		scratch.Write("p/model.cs", "namespace Demo { public sealed class Model2 { public Model2(string key) { } } }");

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
	}
}
