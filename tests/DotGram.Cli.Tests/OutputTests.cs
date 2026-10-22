using System;
using System.Text;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>The output folder: what is written, and what is deleted.</summary>
public sealed class OutputTests
{
	const string Grammar = """
		trivia = ' '*

		Pair = key: Word & '=' & value: Word

		Word = ['a'..'z']+

		parse Pair
		""";

	static (Scratch Scratch, string Grammar, string[] Arguments) Generated()
	{
		var scratch = new Scratch();
		var grammar = scratch.Write("Pairs.gram", Grammar);
		var output  = scratch.Path("out");

		string[] arguments = ["generate", grammar, "-o", output, "--namespace", "Demo"];

		var run = Tool.Run(scratch.Root, arguments);

		Assert.True(run.Exit == 0, run.ToString());

		return (scratch, grammar, arguments);
	}

	[Fact]
	public void The_files_are_UTF8_with_a_byte_order_mark_and_listed_in_the_manifest()
	{
		var (scratch, _, _) = Generated();

		using var _ = scratch;

		var names = Directory.GetFiles(scratch.Path("out")).Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray();

		Assert.Equal(
			[".dotgram.json", "Demo.Pairs.g.cs", "DotGram.Attributes.g.cs", "Microsoft.CodeAnalysis.EmbeddedAttribute.cs", "Pairs.Host.g.cs"],
			names);

		foreach (var name in names.Where(static name => name!.EndsWith(".cs", StringComparison.Ordinal)))
			Assert.Equal([0xEF, 0xBB, 0xBF], File.ReadAllBytes(scratch.Path("out", name!)).Take(3));

		var manifest = File.ReadAllText(scratch.Path("out", OutputFolder.ManifestName));

		Assert.StartsWith("{\n  \"format\": 1,\n", manifest, StringComparison.Ordinal);
		Assert.Contains("\"generator\": \"" + Cli.GeneratorVersion + "\"", manifest, StringComparison.Ordinal);

		foreach (var name in names.Where(static name => name != OutputFolder.ManifestName))
			Assert.Contains("\"name\": \"" + name + "\"", manifest, StringComparison.Ordinal);
	}

	/// <summary>A file the manifest lists and the run would not write: a part of a parser that has since shrunk, say.</summary>
	[Fact]
	public void A_file_left_over_is_deleted_and_nothing_else()
	{
		var (scratch, _, arguments) = Generated();

		using var _ = scratch;

		var manifest = scratch.Path("out", OutputFolder.ManifestName);

		File.WriteAllText(manifest, File.ReadAllText(manifest).Replace(
			"\"files\": [\n",
			"\"files\": [\n    { \"name\": \"Demo.Pairs.part-0001.g.cs\" },\n",
			StringComparison.Ordinal));

		File.WriteAllText(scratch.Path("out", "Demo.Pairs.part-0001.g.cs"), "// a part\r\n");
		File.WriteAllText(scratch.Path("out", "notes.txt"), "mine\n");

		var again = Tool.Run(scratch.Root, arguments);

		Assert.True(again.Exit == 0, again.ToString());
		Assert.Contains("1 deleted", again.Output, StringComparison.Ordinal);
		Assert.False(File.Exists(scratch.Path("out", "Demo.Pairs.part-0001.g.cs")));

		// What the manifest never listed is not the tool's.
		Assert.True(File.Exists(scratch.Path("out", "notes.txt")));
	}

	[Fact]
	public void An_unchanged_file_is_not_written_again()
	{
		var (scratch, _, arguments) = Generated();

		using var _ = scratch;

		var path  = scratch.Path("out", "Demo.Pairs.g.cs");
		var stamp = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		File.SetLastWriteTimeUtc(path, stamp);

		var run = Tool.Run(scratch.Root, arguments);

		Assert.True(run.Exit == 0, run.ToString());
		Assert.Contains("Wrote 0 files", run.Output, StringComparison.Ordinal);
		Assert.Equal(stamp, File.GetLastWriteTimeUtc(path));
	}

	/// <summary>
	/// What a project's earlier run wrote into a folder the project compiles is not handed to the
	/// generator again: the second run sees what the first saw.
	/// </summary>
	[Fact]
	public void A_folder_inside_the_project_is_not_read_back()
	{
		using var scratch = new Scratch();

		scratch.Write("p/Pairs.gram", Grammar);
		scratch.Write("p/Pairs.cs", """
			using DotGram;

			namespace Demo
			{
				[Gram("Pairs.gram")]
				public static partial class Pairs { }
			}
			""");

		var project = scratch.Write("p/p.csproj", """
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Pairs.gram" />
			  </ItemGroup>
			</Project>
			""");

		string[] arguments = ["generate", project, "-o", scratch.Path("p", "Generated")];

		Assert.True(Tool.Run(scratch.Root, arguments).Exit == 0);

		var run = Tool.Run(scratch.Root, arguments);

		Assert.True(run.Exit == 0, run.ToString());
		Assert.Contains("Wrote 0 files", run.Output, StringComparison.Ordinal);
	}
}
