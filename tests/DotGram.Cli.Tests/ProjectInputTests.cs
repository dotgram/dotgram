using System;
using System.IO.Compression;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// A project's compiler inputs are the ones its build gives the compiler, whatever the project does
/// to get them: every case here is held to the files a build of the same project generates, byte for
/// byte, and a case a build would refuse is refused.
/// </summary>
public sealed class ProjectInputTests
{
	const string Pairs = "Pair = key: Word & '=' & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\n";

	const string PlainHost = """
		using DotGram;

		namespace Demo
		{
			[Gram("Pairs.gram")]
			public static partial class Pairs { }
		}
		""";

	/// <summary>A project in <c>directory</c> with one grammar and its host; its inner items and settings as given.</summary>
	static string App(
		Scratch scratch, string directory, string host = PlainHost, string items = "", string settings = "", string framework = "net10.0", string attributes = "",
		string targets = "", string grammar = Pairs, string grammarName = "Pairs.gram")
	{
		scratch.Write(directory + "/" + grammarName, grammar);
		scratch.Write(directory + "/Pairs.cs", host);

		return scratch.Write(directory + "/p.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk"{attributes}>
			  <PropertyGroup>
			    <TargetFramework>{framework}</TargetFramework>
			    <Nullable>enable</Nullable>
			    {settings}
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="{grammarName}" />
			    {items}
			  </ItemGroup>
			  {Build.AnalyzerItems}
			  {targets}
			</Project>
			""");
	}

	/// <summary>Builds the project and generates it with the tool; the two folders are the same.</summary>
	static ToolRun Same(Scratch scratch, string project, string output = "out", string[]? build = null, params string[] arguments)
	{
		var built = Build.Generated(project, build ?? []);
		var run   = Tool.Run(scratch.Root, ["generate", project, "-o", scratch.Path(output), "--line-paths", "absolute", .. arguments]);

		Assert.True(run.Exit == 0, run.ToString());
		Equality.Same(built, scratch.Path(output));

		return run;
	}

	/// <summary>A generator, compiled here: its source is a class <c>G</c> in a file that opens with the usual usings.</summary>
	static string Generator(Scratch scratch, string name, string body)
	{
		var references = Basic.Reference.Assemblies.Net100.References.All
			.Append(MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location))
			.Append(MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location))
			.ToList();

		var path = scratch.Path("generators", name + ".dll");

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		var result = CSharpCompilation.Create(
				name,
				[CSharpSyntaxTree.ParseText("using System;\nusing System.Linq;\nusing Microsoft.CodeAnalysis;\nusing Microsoft.CodeAnalysis.Text;\n\n[Generator]\npublic sealed class G : IIncrementalGenerator\n{\n" + body + "\n}\n")],
				references,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.Emit(path);

		Assert.True(result.Success, string.Join("\n", result.Diagnostics));

		return path;
	}

	static string Analyzer(string path)
	{
		return $"""<Analyzer Include="{path}" />""";
	}

	/// <summary>A library project whose one source file declares what the application uses.</summary>
	static string Library(Scratch scratch, string directory, string source)
	{
		scratch.Write(directory + "/Lib.cs", source);

		return scratch.Write(directory + "/Lib.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <Nullable>enable</Nullable>
			  </PropertyGroup>
			</Project>
			""");
	}

	const string TracedHost = """
		using DotGram;

		namespace Demo
		{
			[Gram("Pairs.gram", Trace = Lib.Settings.Enabled)]
			public static partial class Pairs { }
		}
		""";

	/// <summary>
	/// An attribute argument that is a constant of a referenced project: the build reads it and the
	/// parser is a trace build; the tool must read it as well, not default. With the reference's
	/// source broken the referenced project does not build, and neither does the run.
	/// </summary>
	[Fact]
	public void A_constant_from_a_referenced_project_reaches_the_generator()
	{
		using var scratch = new Scratch();

		Library(scratch, "lib", "namespace Lib { public static class Settings { public const bool Enabled = true; } }");

		var project = App(scratch, "p", TracedHost, """<ProjectReference Include="../lib/Lib.csproj" />""");

		Same(scratch, project);

		Assert.Contains("GramTrace", File.ReadAllText(scratch.Path("out", "Demo.Pairs.g.cs")), StringComparison.Ordinal);

		scratch.Write("lib/Lib.cs", "namespace Lib { public static class Settings { public const bool Enabled = ; } }");

		var broken = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("broken"));

		Assert.True(broken.Exit == ExitCode.Failure, broken.ToString());
		Assert.Contains("MSBuild failed", broken.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("broken")));
	}

	/// <summary>A referenced project that was never built is built by the run; the project is restored, nothing else.</summary>
	[Fact]
	public void A_referenced_project_that_was_never_built_is_built()
	{
		using var scratch = new Scratch();

		Library(scratch, "lib", "namespace Lib { public static class Settings { public const bool Enabled = true; } }");

		var project = App(scratch, "p", TracedHost, """<ProjectReference Include="../lib/Lib.csproj" />""");

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Assert.True(File.Exists(scratch.Path("lib", "bin", "Release", "net10.0", "Lib.dll")));

		var built = Build.Generated(project);

		Equality.Same(built, scratch.Path("out"));
	}

	/// <summary>
	/// A generator whose post-initialization output adds a constructor to the partial class the grammar
	/// builds, named by <c>&lt;Analyzer Include&gt;</c>: it is loaded and run, so that what it adds is
	/// there when the sources are checked; unloadable, the run stops and names it.
	/// </summary>
	[Fact]
	public void A_generator_the_project_names_is_run_and_one_that_will_not_load_stops_the_run()
	{
		using var scratch = new Scratch();

		var generator = Generator(scratch, "Constructor", """
			public void Initialize(IncrementalGeneratorInitializationContext context)
			{
				context.RegisterPostInitializationOutput(static output => output.AddSource("Node.Ctor.g.cs",
					"namespace Demo { public partial class Node { public Node(string name) { Name = name; } public string Name { get; } } }"));
			}
			""");

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Nodes.gram")]
				public static partial class Nodes { }

				public partial class Node
				{
					public static string Shout(Node node) { return node.Name.ToUpperInvariant(); }
				}
			}
			""", Analyzer(generator), grammar: "Node : @Node = name: Word => @(new Node(name))\n\nWord = ['a'..'z']+\n\nparse Node\n", grammarName: "Nodes.gram");

		Same(scratch, project);

		File.WriteAllBytes(generator, [1, 2, 3, 4]);

		var broken = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("broken"));

		Assert.True(broken.Exit == ExitCode.Failure, broken.ToString());
		Assert.Contains("Constructor.dll", broken.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("broken")));
	}

	/// <summary>
	/// An analyzer project reference: the run builds it (the build would), loads its generator, and the
	/// names it supplies resolve.
	/// </summary>
	[Fact]
	public void An_analyzer_project_reference_is_built_and_run()
	{
		using var scratch = new Scratch();

		scratch.Write("gen/G.cs", """
			using Microsoft.CodeAnalysis;

			[Generator]
			public sealed class G : IIncrementalGenerator
			{
				public void Initialize(IncrementalGeneratorInitializationContext context)
				{
					context.RegisterPostInitializationOutput(static output => output.AddSource("Supplied.g.cs", "namespace Demo { public static class Supplied { public const int Value = 1; } }"));
				}
			}
			""");

		scratch.Write("gen/Gen.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>netstandard2.0</TargetFramework>
			    <LangVersion>latest</LangVersion>
			    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" PrivateAssets="all" />
			  </ItemGroup>
			</Project>
			""");

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					public const int Twice = Supplied.Value * 2;
				}
			}
			""", """<ProjectReference Include="../gen/Gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />""");

		Same(scratch, project);
	}

	/// <summary>
	/// A package that has an assembly in <c>analyzers/dotnet/cs</c> and another in
	/// <c>analyzers/dotnet/roslyn4.8/cs</c>: the build runs what the SDK picks of them, and so does the run.
	/// </summary>
	[Fact]
	public void The_generators_of_a_package_are_the_ones_the_build_picks()
	{
		using var scratch = new Scratch();

		string Supplier(string name, string type)
		{
			return Generator(scratch, name, $$"""
				public void Initialize(IncrementalGeneratorInitializationContext context)
				{
					context.RegisterPostInitializationOutput(static output => output.AddSource("{{type}}.g.cs", "namespace Demo { public static class {{type}} { public const int Value = 1; } }"));
				}
				""");
		}

		var feed = scratch.Path("feed");

		Directory.CreateDirectory(feed);

		using (var package = ZipFile.Open(Path.Combine(feed, "Suppliers.1.0.0.nupkg"), ZipArchiveMode.Create))
		{
			void Add(string name, byte[] bytes)
			{
				using var stream = package.CreateEntry(name).Open();

				stream.Write(bytes);
			}

			Add("Suppliers.nuspec", Encoding.UTF8.GetBytes("""
				<?xml version="1.0"?>
				<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
				  <metadata>
				    <id>Suppliers</id>
				    <version>1.0.0</version>
				    <authors>test</authors>
				    <description>test</description>
				    <developmentDependency>true</developmentDependency>
				  </metadata>
				</package>
				"""));

			Add("analyzers/dotnet/cs/Generic.dll", File.ReadAllBytes(Supplier("Generic", "FromGeneric")));
			Add("analyzers/dotnet/roslyn4.8/cs/Versioned.dll", File.ReadAllBytes(Supplier("Versioned", "FromVersioned")));
		}

		var project = App(
			scratch, "p",
			"""
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					public const int Sum = FromGeneric.Value + FromVersioned.Value;
				}
			}
			""",
			"""<PackageReference Include="Suppliers" Version="1.0.0" PrivateAssets="all" />""",
			$"<RestoreSources>{feed}</RestoreSources><RestorePackagesPath>{scratch.Path("packages")}</RestorePackagesPath>");

		Same(scratch, project);
	}

	/// <summary>An alias given to a reference is kept: <c>extern alias</c> in the sources binds.</summary>
	[Fact]
	public void An_alias_on_a_reference_is_kept()
	{
		using var scratch = new Scratch();

		var library = Library(scratch, "lib", "namespace Ext { public sealed class Thing { } }");

		Build.Run(library, "OutputPath=" + scratch.Path("lib-bin") + "/");

		var project = App(scratch, "p", """
			extern alias Lib;

			using DotGram;

			using Thing = Lib::Ext.Thing;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					public static Thing? Made { get; set; }
				}
			}
			""", $"""<Reference Include="{scratch.Path("lib-bin", "Lib.dll")}" Aliases="Lib" />""");

		Same(scratch, project);
	}

	/// <summary>An <c>AdditionalFiles</c> item that is no grammar reaches the project's other generators.</summary>
	[Fact]
	public void An_additional_file_reaches_another_generator()
	{
		using var scratch = new Scratch();

		var generator = Generator(scratch, "Json", """
			public void Initialize(IncrementalGeneratorInitializationContext context)
			{
				var texts = context.AdditionalTextsProvider
					.Where(static text => text.Path.EndsWith(".json", StringComparison.Ordinal))
					.Select(static (text, token) => text.GetText(token)!.ToString().Trim())
					.Collect();

				context.RegisterSourceOutput(texts, static (output, all) =>
					output.AddSource("Seen.g.cs", "namespace Demo { static class Seen { public const int " + string.Join("_", all) + " = 1; } }"));
			}
			""");

		scratch.Write("p/settings.json", "Json_on\n");

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					public const int Value = Seen.Json_on;
				}
			}
			""", Analyzer(generator) + """<AdditionalFiles Include="settings.json" />""");

		Same(scratch, project);
	}

	/// <summary>
	/// What the build tells a generator through analyzer configuration: a compiler-visible property, a
	/// global configuration file, an <c>.editorconfig</c> section for a source file.
	/// </summary>
	[Fact]
	public void Analyzer_configuration_reaches_another_generator()
	{
		using var scratch = new Scratch();

		var generator = Generator(scratch, "Options", """
			public void Initialize(IncrementalGeneratorInitializationContext context)
			{
				var inputs = context.CompilationProvider.Combine(context.AnalyzerConfigOptionsProvider);

				context.RegisterSourceOutput(inputs, static (output, pair) =>
				{
					var (compilation, provider) = pair;

					provider.GlobalOptions.TryGetValue("build_property.Mine", out var property);
					provider.GlobalOptions.TryGetValue("my_global", out var global);

					var tree = compilation.SyntaxTrees.First(static tree => tree.FilePath.EndsWith("Pairs.cs", StringComparison.Ordinal));

					provider.GetOptions(tree).TryGetValue("my_key", out var key);

					output.AddSource("Seen.g.cs", "namespace Demo { static class Seen { public const int " + property + "_" + global + "_" + key + " = 1; } }");
				});
			}
			""");

		scratch.Write("p/mine.globalconfig", "is_global = true\nmy_global = global\n");
		scratch.Write("p/.editorconfig", "root = true\n\n[Pairs.cs]\nmy_key = key\n");

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					public const int Value = Seen.property_global_key;
				}
			}
			""", Analyzer(generator) + """<CompilerVisibleProperty Include="Mine" /><GlobalAnalyzerConfigFiles Include="mine.globalconfig" />""", "<Mine>property</Mine>");

		Same(scratch, project);
	}

	/// <summary>A project that was restored and never built: the run restores nothing and leaves it as it found it.</summary>
	[Fact]
	public void A_project_not_restored_is_refused_and_nothing_is_restored()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p");
		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("dotnet restore", run.Error, StringComparison.Ordinal);
		Assert.False(File.Exists(scratch.Path("p", "obj", "project.assets.json")));
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>Directories with a blank in their names, and with letters outside ASCII.</summary>
	[Theory]
	[InlineData("a project/with blanks")]
	[InlineData("projekt üß/日本語")]
	public void A_path_with_blanks_or_non_ascii_letters_is_the_builds(string directory)
	{
		using var scratch = new Scratch();

		Same(scratch, App(scratch, directory));
	}

	/// <summary>
	/// A project whose <c>InitialTargets</c> write a file, never built. The run asks MSBuild for the
	/// compiler's inputs as an IDE does, which runs the project's initial targets and those before the
	/// compiler: the file is written. No assembly of the project is.
	/// </summary>
	[Fact]
	public void Initial_targets_run_and_no_assembly_is_made()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", attributes: " InitialTargets=\"Stamp\"", targets: """
			<Target Name="Stamp">
			  <WriteLinesToFile File="$(MSBuildProjectDirectory)/obj/stamp" Lines="ran" Overwrite="true" />
			</Target>
			""");

		// Restore evaluates the project and runs its initial targets too: the stamp is the run's to write.
		Dotnet.Restore(project);
		File.Delete(scratch.Path("p", "obj", "stamp"));

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
		Assert.True(File.Exists(scratch.Path("p", "obj", "stamp")));

		Assert.Empty(Directory.GetFiles(scratch.Path("p", "obj"), "p.dll", SearchOption.AllDirectories));
		Assert.Empty(Directory.Exists(scratch.Path("p", "bin")) ? Directory.GetFiles(scratch.Path("p", "bin"), "*", SearchOption.AllDirectories) : []);
	}

	/// <summary>A target before the compiler adds a source file: it is in the tool's inputs, as in the build's.</summary>
	[Fact]
	public void A_file_a_target_before_the_compiler_adds_is_compiled()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					public const int Value = Added.Value;
				}
			}
			""", targets: """
			<Target Name="Add" BeforeTargets="CoreCompile">
			  <WriteLinesToFile File="$(MSBuildProjectDirectory)/obj/Added.cs" Lines="namespace Demo { static class Added { public const int Value = 1%3B } }" Overwrite="true" />
			  <ItemGroup>
			    <Compile Include="$(MSBuildProjectDirectory)/obj/Added.cs" />
			  </ItemGroup>
			</Target>
			""");

		Same(scratch, project);
	}

	/// <summary>The project built, then generated twice: both are the build's.</summary>
	[Fact]
	public void A_project_already_built_is_generated_all_the_same()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p");
		var built   = Build.Generated(project);

		foreach (var folder in new[] { "first", "second" })
		{
			var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path(folder), "--line-paths", "absolute");

			Assert.True(run.Exit == 0, run.ToString());
			Equality.Same(built, scratch.Path(folder));
		}
	}


	/// <summary>
	/// <c>/codepage:</c> (the project's <c>CodePage</c>) is how the compiler reads a source without a
	/// byte order mark, and the additional files with it: a Windows-1252 host with an <c>\u00e9</c> in an
	/// inline grammar is the build's with the code page told and without it (no code page: a
	/// replacement character, as csc reads it), and a UTF-8 host is read in the code page when one is
	/// told, as csc reads it.
	/// </summary>
	[Theory]
	[InlineData("1252", 1252)]
	[InlineData("", 1252)]
	[InlineData("1252", 65001)]
	[InlineData("", 65001)]
	public void A_source_in_a_code_page_is_read_as_the_compiler_reads_it(string codePage, int encoding)
	{
		using var scratch = new Scratch();

		var project = App(
			scratch,
			"p",
			"using DotGram;\n\nnamespace Demo\n{\n\t[Gram(@\"Pair = key: Word & \"\"\u00e9\"\" & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\")]\n\tpublic static partial class Pairs { }\n}\n",
			settings: codePage.Length > 0 ? $"<CodePage>{codePage}</CodePage>" : "");

		File.WriteAllBytes(
			scratch.Path("p", "Pairs.cs"),
			(encoding == 65001 ? new UTF8Encoding(false) : System.Text.CodePagesEncodingProvider.Instance.GetEncoding(encoding)!).GetBytes(File.ReadAllText(scratch.Path("p", "Pairs.cs"))));

		Same(scratch, project);

		// The letter survives where the file's encoding is the one the compiler reads it in.
		var written = File.ReadAllText(scratch.Path("out", "Demo.Pairs.g.cs"));

		Assert.Equal(codePage.Length > 0 ? encoding == 1252 : encoding == 65001, written.Contains('\u00e9', StringComparison.Ordinal));
	}

	/// <summary>
	/// Without a code page a file is UTF-8 where it is valid UTF-8, and otherwise in the system's default
	/// code page, as the compiler falls back to it (UTF-8 itself on Linux and macOS, the ANSI page on
	/// Windows); a byte order mark and an explicit code page decide before that.
	/// </summary>
	[Fact]
	public void A_file_that_is_no_utf8_is_read_in_the_default_code_page()
	{
		using var scratch = new Scratch();

		System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

		var invalid = scratch.Path("invalid.cs");

		File.WriteAllBytes(invalid, [(byte)'a', 0xE9, (byte)'b']);

		Assert.Equal(System.Text.Encoding.GetEncoding(0).GetString([(byte)'a', 0xE9, (byte)'b']), GenerationInputs.Read(invalid).ToString());

		var valid = scratch.Path("valid.cs");

		File.WriteAllBytes(valid, new UTF8Encoding(false).GetBytes("a\u00e9b"));

		Assert.Equal("a\u00e9b", GenerationInputs.Read(valid).ToString());

		// A code page told to the compiler is used for bytes that are valid UTF-8 as well.
		Assert.Equal(
			System.Text.Encoding.Latin1.GetString(new UTF8Encoding(false).GetBytes("a\u00e9b")),
			GenerationInputs.Read(valid, System.Text.Encoding.Latin1).ToString());

		// A byte order mark is read as the mark says.
		var marked = scratch.Path("marked.cs");

		File.WriteAllBytes(marked, [.. new byte[] { 0xEF, 0xBB, 0xBF }, .. new UTF8Encoding(false).GetBytes("a\u00e9b")]);

		Assert.Equal("a\u00e9b", GenerationInputs.Read(marked, System.Text.Encoding.Latin1).ToString());
	}

	/// <summary>The same for an additional file: a grammar saved in Windows-1252 with the project's <c>CodePage</c>.</summary>
	[Fact]
	public void An_additional_file_in_a_code_page_is_read_as_the_compiler_reads_it()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", settings: "<CodePage>1252</CodePage>");

		File.WriteAllBytes(
			scratch.Path("p", "Pairs.gram"),
			System.Text.CodePagesEncodingProvider.Instance.GetEncoding(1252)!.GetBytes("Pair = key: Word & '\u00e9' & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\n"));

		Same(scratch, project);

		Assert.Contains("\u00e9", File.ReadAllText(scratch.Path("out", "Demo.Pairs.g.cs")), StringComparison.Ordinal);
	}

	/// <summary>
	/// The file's encoding is the compiler's for what is read, and the output is written as the build
	/// writes it: UTF-8 with a byte order mark, which the comparison holds by bytes. A source with
	/// non-ASCII text that is UTF-8 with no mark is read as UTF-8.
	/// </summary>
	[Fact]
	public void Non_ascii_in_utf8_is_written_as_the_build_writes_it()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", grammar: "Pair = key: Word & \"\u00e9\u4e2d\" & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\n");

		Same(scratch, project);

		Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, File.ReadAllBytes(scratch.Path("out", "Demo.Pairs.g.cs")).Take(3));
	}

	/// <summary>
	/// The tool runs what an IDE runs, as a design-time build: a target that follows <c>Compile</c> and
	/// skips design-time builds is not run.
	/// </summary>
	[Fact]
	public void A_target_after_compile_that_skips_design_time_builds_does_not_run()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", targets: """
			<Target Name="AfterTheCompiler" AfterTargets="Compile" Condition="'$(DesignTimeBuild)' != 'true'">
			  <WriteLinesToFile File="$(MSBuildProjectDirectory)/obj/after-compile" Lines="ran" Overwrite="true" />
			</Target>
			""");

		Dotnet.Restore(project);

		Directory.CreateDirectory(scratch.Path("p", "obj"));
		File.Delete(scratch.Path("p", "obj", "after-compile"));

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
		Assert.False(File.Exists(scratch.Path("p", "obj", "after-compile")));
	}

	/// <summary>
	/// A target that prints a message with a brace in it does not stand in for MSBuild's answer, on
	/// the SDKs that print it instead of writing it to a file.
	/// </summary>
	[Fact]
	public void A_brace_in_what_a_target_prints_is_not_the_answer()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", targets: """
			<Target Name="Say" BeforeTargets="CoreCompile">
			  <Warning Text="Using {profile}" />
			  <Message Importance="high" Text="{profile}" />
			</Target>
			""");

		Same(scratch, project);
	}

	[Theory]
	[InlineData("MSBUILD : warning : Using {profile}\n{\n  \"Items\": { \"A\": [] }\n}\n")]
	[InlineData("{profile}\n{ \"x\": 1\n{\n  \"Items\": { \"A\": [] }\n}\n")]
	[InlineData("{ not json\n{\n  \"Items\": { \"A\": [] }\n}\n")]
	[InlineData("{\n  \"Items\": { \"A\": [] }\n}\n")]
	public void The_answer_is_the_json_that_runs_to_the_end_of_the_output(string printed)
	{
		using var answer = MSBuild.Answer(printed);

		Assert.NotNull(answer);
		Assert.True(answer.RootElement.GetProperty("Items").TryGetProperty("A", out _));
	}

	[Fact]
	public void Output_with_no_json_in_it_has_no_answer()
	{
		Assert.Null(MSBuild.Answer("MSBUILD : warning : Using {profile}\n{ \"x\": 1 } and more\n"));
	}

	/// <summary>
	/// The check refuses on the errors the compiler reports by default. A warning the project's build
	/// turns into an error (<c>TreatWarningsAsErrors</c>) is the build's to fail on, not the tool's
	/// to refuse: the files are written, and are the build's.
	/// </summary>
	[Fact]
	public void A_warning_the_build_makes_an_error_does_not_stop_the_run()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					static void Unused() { int never = 0; }
				}
			}
			""", settings: "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>");

		var (exit, text, built) = Build.TryGenerated(project);

		Assert.True(exit != 0 && text.Contains("CS0219", StringComparison.Ordinal), text);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Equality.Same(built, scratch.Path("out"));
	}

	/// <summary>A grammar with a rule nothing reaches, which the generator warns about (GRAM4018).</summary>
	const string Unreached = Pairs + "\nSpare = 'x'\n";

	const string NoWarn = "<NoWarn>$(NoWarn);GRAM4018</NoWarn>";
	const string AsError = "<WarningsAsErrors>$(WarningsAsErrors);GRAM4018</WarningsAsErrors>";
	const string AllAsErrors = "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>";
	const string AllButOne = AllAsErrors + "<WarningsNotAsErrors>GRAM4018</WarningsNotAsErrors>";

	/// <summary>
	/// The generator's own diagnostics take the severity the build gives them: the <c>.editorconfig</c>
	/// sections that apply to the C# file the diagnostic is in (a diagnostic in a grammar file has none),
	/// the global configuration, <c>/nowarn</c> and <c>/warnaserror</c>, in the order the compiler applies
	/// them: the project's <c>NoWarn</c> and <c>WarningsAsErrors</c> win over any configuration file. The tool's word for GRAM4018 is the build's, and it stops the run when it is an error.
	/// </summary>
	[Theory]
	[InlineData(false, "", "", "", "warning")]
	[InlineData(false, "[*.gram]\ndotnet_diagnostic.GRAM4018.severity = error", "", "", "warning")]
	[InlineData(false, "", "error", "", "error")]
	[InlineData(false, "", "none", "", "none")]
	[InlineData(false, "", "suggestion", "", "none")]
	[InlineData(false, "", "", NoWarn, "none")]
	[InlineData(false, "", "", AsError, "error")]
	[InlineData(false, "", "", AllAsErrors, "error")]
	[InlineData(false, "", "warning", AllAsErrors, "error")]
	[InlineData(false, "", "error", NoWarn, "none")]
	[InlineData(false, "", "", AllButOne, "warning")]
	[InlineData(false, "", "suggestion", AllAsErrors, "none")]
	[InlineData(false, "", "none", AsError, "error")]
	[InlineData(true, "", "", "", "warning")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = error", "", "", "error")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = none", "", "", "none")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = suggestion", "", "", "none")]
	[InlineData(true, "[*.gram]\ndotnet_diagnostic.GRAM4018.severity = error", "", "", "warning")]
	[InlineData(true, "", "error", "", "error")]
	[InlineData(true, "", "", NoWarn, "none")]
	[InlineData(true, "", "", AsError, "error")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = warning", "", AllAsErrors, "error")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = error", "", NoWarn, "none")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = none", "", AsError, "error")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = none", "error", "", "none")]
	[InlineData(true, "[*.cs]\ndotnet_diagnostic.GRAM4018.severity = warning", "error", "", "warning")]
	[InlineData(true, "", "error", NoWarn, "none")]
	[InlineData(true, "", "none", AsError, "error")]
	public void The_generators_diagnostics_take_the_severity_of_the_build(bool inline, string editorConfig, string globalSeverity, string settings, string expected)
	{
		using var scratch = new Scratch();

		var host = inline
			? "using DotGram;\n\nnamespace Demo\n{\n\t[Gram(@\"" + Unreached.Replace("\"", "\"\"", StringComparison.Ordinal) + "\")]\n\tpublic static partial class Pairs { }\n}\n"
			: PlainHost;

		var project = App(
			scratch,
			"p",
			host,
			items: globalSeverity.Length > 0 ? "<GlobalAnalyzerConfigFiles Include=\"severities.globalconfig\" />" : "",
			settings: settings,
			grammar: inline ? Pairs : Unreached);

		if (editorConfig.Length > 0)
			scratch.Write("p/.editorconfig", "root = true\n\n" + editorConfig + "\n");

		if (globalSeverity.Length > 0)
			scratch.Write("p/severities.globalconfig", $"is_global = true\n\ndotnet_diagnostic.GRAM4018.severity = {globalSeverity}\n");

		var (_, text, built) = Build.TryGenerated(project);
		var run              = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(expected == SeverityOf(text, "GRAM4018"), "the build:\n" + text);
		Assert.True(expected == SeverityOf(run.Output, "GRAM4018"), run.ToString());
		Assert.Equal(expected == "error" ? ExitCode.GrammarErrors : ExitCode.Done, run.Exit);

		if (expected != "error")
			Equality.Same(built, scratch.Path("out"));
	}

	/// <summary>The word the output gives a diagnostic (<c>error</c> or <c>warning</c>), or <c>none</c> where it does not show it.</summary>
	static string SeverityOf(string output, string id)
	{
		var match = System.Text.RegularExpressions.Regex.Match(output, @"\b(error|warning) " + id + ":");

		return match.Success ? match.Groups[1].Value : "none";
	}

	/// <summary>
	/// A compiler older than the DotGram the tool carries cannot run it: the build skips DotGram
	/// (CS9057) and generates nothing, and so does not the tool. The compiler's version is the one of
	/// the <c>Microsoft.CodeAnalysis</c> beside the project's compiler.
	/// </summary>
	[Fact]
	public void A_compiler_older_than_dotgram_needs_is_refused()
	{
		using var scratch = new Scratch();

		var older = new Version(AnalyzerGenerators.DotGramNeeds.Major - 1, 0, 0, 0);

		// MSBuild wants the compiler's executable to be there, though a design-time build never runs it.
		scratch.Write("compiler/csc", "");

		var stub = CSharpCompilation.Create(
				"Microsoft.CodeAnalysis",
				[CSharpSyntaxTree.ParseText($"[assembly: System.Reflection.AssemblyVersion(\"{older}\")]", cancellationToken: TestContext.Current.CancellationToken)],
				Basic.Reference.Assemblies.Net100.References.All,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.Emit(scratch.Path("compiler", "Microsoft.CodeAnalysis.dll"), cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(stub.Success, string.Join("\n", stub.Diagnostics));

		var project = App(scratch, "p", settings: $"<CscToolPath>{scratch.Path("compiler")}</CscToolPath>");

		Dotnet.Restore(project);

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.Equal(ExitCode.Failure, run.Exit);
		Assert.True(run.Error.Contains($"this SDK's compiler (Roslyn {older}) cannot run DotGram", StringComparison.Ordinal), run.ToString());
		Assert.Contains("CS9057", run.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>A compiler as new as DotGram's, or one that cannot be told, is not refused.</summary>
	[Fact]
	public void A_compiler_as_new_as_dotgram_needs_is_accepted()
	{
		var needed = AnalyzerGenerators.DotGramNeeds;

		AnalyzerGenerators.RequireCompilerFor(needed, needed, "x");
		AnalyzerGenerators.RequireCompilerFor(new Version(needed.Major + 1, 0), needed, "x");
		AnalyzerGenerators.RequireCompilerFor(null, needed, "x");

		var exception = Assert.Throws<ToolException>(() => AnalyzerGenerators.RequireCompilerFor(new Version(needed.Major - 1, 0), needed, "x"));

		Assert.Equal(ExitCode.Failure, exception.ExitCode);
	}

	/// <summary>No restore warning: a no-op restore does not rewrite the assets file, so its age says nothing.</summary>
	[Fact]
	public void A_project_file_newer_than_the_restore_is_not_warned_about()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p");
		var props   = scratch.Write("p/Directory.Build.props", "<Project />");

		Dotnet.Restore(project);
		File.SetLastWriteTimeUtc(props, DateTime.UtcNow.AddHours(1));

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == 0, run.ToString());
		Assert.DoesNotContain("restore may be stale", run.Output, StringComparison.Ordinal);
	}

	/// <summary><c>#if DEBUG</c> in a host: the default configuration is Release, and Debug is asked for.</summary>
	[Theory]
	[InlineData("Release")]
	[InlineData("Debug")]
	public void The_configuration_decides_as_it_does_in_the_build(string configuration)
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs { }

			#if DEBUG
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
			""", grammar: "Pair : @Pair = key: Word & '=' & value: Word\n\nWord = ['a'..'z']+\n\nparse Pair\n");

		var arguments = configuration == "Release" ? [] : new[] { "--configuration", "Debug" };

		Same(scratch, project, build: ["Configuration=" + configuration], arguments: arguments);
	}

	/// <summary>A global.json that pins an SDK the machine has not: the run stops, and says so.</summary>
	[Fact]
	public void An_sdk_that_is_not_there_stops_the_run()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p");

		scratch.Write("p/global.json", """{ "sdk": { "version": "7.0.100", "rollForward": "disable" } }""");

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"));

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("SDK", run.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>
	/// <c>-getItem</c> exists from SDK 8.0.100; an older one is said to be too old, wherever the
	/// version is read from.
	/// </summary>
	[Theory]
	[InlineData("8.0.100", true)]
	[InlineData("8.0.100-rc.1.23455.8", true)]
	[InlineData("10.0.401", true)]
	[InlineData("11.0.100-rc.2.26504.105", true)]
	[InlineData("7.0.410", false)]
	[InlineData("6.0.428", false)]
	public void An_sdk_older_than_8_0_100_is_refused(string version, bool accepted)
	{
		if (accepted)
		{
			MSBuild.RequireSdk(version);
			return;
		}

		var exception = Assert.Throws<ToolException>(() => MSBuild.RequireSdk(version));

		Assert.Equal(ExitCode.Failure, exception.ExitCode);
		Assert.Contains("needs .NET SDK 8.0.100 or newer", exception.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A project that references another DotGram than the tool carries: generated with the tool's, in
	/// one warning naming both, and the files are what the project's own build of this version writes.
	/// </summary>
	[Fact]
	public void Another_version_of_dotgram_in_the_project_is_one_warning()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p");
		var built   = Build.Generated(project);

		var stub = StubDotGram(scratch, "0.0.1");

		File.WriteAllText(project, File.ReadAllText(project).Replace(Build.Generator, stub, StringComparison.Ordinal));

		var run = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Assert.Contains($"warning: the project references DotGram 0.0.1; generating with {Cli.GeneratorVersion}", run.Output, StringComparison.Ordinal);
		Assert.Equal(1, run.Output.Split("the project references DotGram", StringSplitOptions.None).Length - 1);
		Equality.Same(built, scratch.Path("out"));
	}

	/// <summary>
	/// The same, where the tool's output does not compile in the project (it declares what DotGram's
	/// parser declares): refused with both versions named.
	/// </summary>
	[Fact]
	public void Output_of_another_version_that_does_not_compile_is_refused_with_both_versions()
	{
		using var scratch = new Scratch();

		var project = App(scratch, "p", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs
				{
					sealed class Ways { }
				}
			}
			""");

		Dotnet.Restore(project);

		var stub = StubDotGram(scratch, "0.0.1");

		File.WriteAllText(project, File.ReadAllText(project).Replace(Build.Generator, stub, StringComparison.Ordinal));

		var other = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("other"));

		Assert.True(other.Exit == ExitCode.Failure, other.ToString());
		Assert.Contains("0.0.1", other.Error, StringComparison.Ordinal);
		Assert.Contains(Cli.GeneratorVersion, other.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("other")));
	}

	/// <summary>An assembly named DotGram of a version of its own, with a generator that adds nothing.</summary>
	static string StubDotGram(Scratch scratch, string version)
	{
		var references = Basic.Reference.Assemblies.Net100.References.All
			.Append(MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location))
			.ToList();

		var path = scratch.Path("stub", "DotGram.dll");

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		var result = CSharpCompilation.Create(
				"DotGram",
				[CSharpSyntaxTree.ParseText($$"""
					using Microsoft.CodeAnalysis;

					[assembly: System.Reflection.AssemblyInformationalVersion("{{version}}")]

					namespace Stub
					{
						[Generator]
						public sealed class G : IIncrementalGenerator
						{
							public void Initialize(IncrementalGeneratorInitializationContext context) { }
						}
					}
					""")],
				references,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.Emit(path);

		Assert.True(result.Success, string.Join("\n", result.Diagnostics));

		return path;
	}
}
