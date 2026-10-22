using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// What makes the tool's compilation the build's where a project has generators of its own: their
/// types resolve, a generator that will not load stops the run, and the output folder is read as its
/// file system reads names.
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

	/// <summary>
	/// A generator whose type cannot be loaded (its base class is in an assembly that is not there):
	/// the run stops, and the message names its assembly.
	/// </summary>
	[Fact]
	public void A_generator_that_does_not_load_stops_the_run()
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

		Assert.True(AnalyzerGenerators.HasGenerator(gen));
		Assert.False(AnalyzerGenerators.HasGenerator(missing));

		var exception = Assert.Throws<ToolException>(() => AnalyzerGenerators.Load([gen]));

		Assert.Equal(ExitCode.Failure, exception.ExitCode);
		Assert.Contains("Gen.dll", exception.Message, StringComparison.Ordinal);
		Assert.Contains("could not be loaded", exception.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A generator built against a compiler newer than the tool's: the run stops and says that the tool
	/// is to be updated, rather than describing a binding failure.
	/// </summary>
	[Fact]
	public void A_generator_built_for_a_newer_compiler_says_to_update_the_tool()
	{
		using var scratch = new Scratch();

		var framework = Basic.Reference.Assemblies.Net100.References.All.ToList();

		// An assembly called Microsoft.CodeAnalysis of version 99, with just the types a generator names.
		var newer = Emit(
			scratch.Path("newer", "Microsoft.CodeAnalysis.dll"),
			"Microsoft.CodeAnalysis",
			"""
			[assembly: System.Reflection.AssemblyVersion("99.0.0.0")]

			namespace Microsoft.CodeAnalysis
			{
				public sealed class GeneratorAttribute : System.Attribute { }

				public struct IncrementalGeneratorInitializationContext { }

				public interface IIncrementalGenerator
				{
					void Initialize(IncrementalGeneratorInitializationContext context);
				}
			}
			""",
			framework);

		var gen = Emit(
			scratch.Path("gen", "Newer.dll"),
			"Newer",
			"""
			using Microsoft.CodeAnalysis;

			[Generator]
			public sealed class G : IIncrementalGenerator
			{
				public void Initialize(IncrementalGeneratorInitializationContext context) { }
			}
			""",
			[.. framework, MetadataReference.CreateFromFile(newer)]);

		Assert.True(AnalyzerGenerators.HasGenerator(gen));

		var exception = Assert.Throws<ToolException>(() => AnalyzerGenerators.Load([gen]));

		Assert.Equal(ExitCode.Failure, exception.ExitCode);
		Assert.Contains("Newer.dll", exception.Message, StringComparison.Ordinal);
		Assert.Contains("newer compiler than DotGram.Cli's: update DotGram.Cli", exception.Message, StringComparison.Ordinal);
	}

	/// <summary>A file that is no assembly at all might hold a generator: loading it says why it will not.</summary>
	[Fact]
	public void A_file_that_is_no_assembly_is_not_ruled_out()
	{
		using var scratch = new Scratch();

		var path = scratch.Write("junk/Junk.dll", "not an assembly");

		Assert.True(AnalyzerGenerators.HasGenerator(path));
		Assert.Throws<ToolException>(() => AnalyzerGenerators.Load([path]));
	}

	static string Emit(string path, string name, string source, IEnumerable<MetadataReference> references)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		var result = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source)], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.Emit(path);

		Assert.True(result.Success, string.Join("\n", result.Diagnostics));

		return path;
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

	/// <summary>Threads asking at once about one folder get one answer from one probe, which leaves nothing behind.</summary>
	[Fact]
	public void The_output_folder_is_probed_once_however_many_ask()
	{
		using var scratch = new Scratch();

		Directory.CreateDirectory(scratch.Path("out"));

		var answers = Enumerable.Range(0, 64).AsParallel().WithDegreeOfParallelism(16).Select(_ => FileSystem.NamesIn(scratch.Path("out"))).ToList();

		Assert.All(answers, answer => Assert.Same(answers[0], answer));
		Assert.Empty(Directory.GetFileSystemEntries(scratch.Path("out")));
	}

	/// <summary>
	/// On a file system that tells case apart, Model.cs and model.cs are two files, and the SDK refuses
	/// them as duplicate Compile items: the build fails, and so does the run, with MSBuild's words.
	/// </summary>
	[Fact]
	public void Two_files_differing_in_case_are_refused_as_the_build_refuses_them()
	{
		using var scratch = new Scratch();

		Directory.CreateDirectory(scratch.Path("p"));

		if (FileSystem.NamesIn(scratch.Path("p")) != StringComparer.Ordinal)
			return;

		var project = Project(scratch, "net10.0", PlainHost, grammar: "Pair : @Model2 = key: Word => @(new Model2(key))\n\nWord = ['a'..'z']+\n\nparse Pair\n");

		scratch.Write("p/Model.cs", "namespace Demo { public sealed class Model1 { } }");
		scratch.Write("p/model.cs", "namespace Demo { public sealed class Model2 { public Model2(string key) { } } }");

		var (exit, text) = Build.Try(project);

		Assert.True(exit != 0 && text.Contains("NETSDK1022", StringComparison.Ordinal), text);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("NETSDK1022", run.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}
}
