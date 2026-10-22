using System;
using System.Text;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// Every grammar of <c>tests/Snapshots</c> with a host of its own in one project: what the tool
/// writes for the project is what a build of it generates, byte for byte, on net10.0 and on
/// netstandard2.0.
/// </summary>
public sealed class SnapshotGrammarTests
{
	/// <summary>The one snapshot grammar whose host has to say something: its location type and carrier, and the types it builds.</summary>
	const string LocatedHost = """
		[Gram("Located.gram", LocationType = typeof(Located.ILocated), Carrier = GramCarrier.Tape)]
		public partial class Located
		{
			public interface ILocated
			{
				void Locate(int at, int length);
			}

			public sealed class Place : ILocated
			{
				public Place(string name) { Name = name; }

				public string Name { get; }
				public int At { get; private set; }
				public int Length { get; private set; }

				public void Locate(int at, int length) { At = at; Length = length; }
			}

			public sealed class Entry : ILocated
			{
				public Entry(Place key, string value) { Key = key; Value = value; }

				public Place Key { get; }
				public string Value { get; }
				public int At { get; private set; }
				public int Length { get; private set; }

				public void Locate(int at, int length) { At = at; Length = length; }
			}
		}
		""";

	/// <summary>A project holding every snapshot grammar, each with its host.</summary>
	static string Project(Scratch scratch, string framework, string extra)
	{
		var hosts = new StringBuilder("using DotGram;\n\nnamespace DotGram.Snapshots\n{\n");

		foreach (var grammar in Directory.GetFiles(Repository.Snapshots, "*.gram").Order(StringComparer.Ordinal))
		{
			var name = Path.GetFileNameWithoutExtension(grammar);

			File.Copy(grammar, scratch.Path("Grammars", name + ".gram").Also(static path => Directory.CreateDirectory(Path.GetDirectoryName(path)!)));

			hosts.Append(name == "Located" ? LocatedHost : $"[Gram(\"{name}.gram\")]\npublic partial class {name} {{ }}\n").Append('\n');
		}

		scratch.Write("Hosts.cs", hosts.Append("}\n").ToString());

		return scratch.Write("Snapshots.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{framework}</TargetFramework>
			    <Nullable>enable</Nullable>
			    {extra}
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="Grammars/*.gram" />
			  </ItemGroup>
			  {Build.AnalyzerItems}
			</Project>
			""");
	}

	[Fact]
	public void Net10_project_generates_what_its_build_generates()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "net10.0", "");
		var built   = Build.Generated(project);
		var run     = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute");

		Assert.True(run.Exit == 0, run.ToString());
		Equality.Same(built, scratch.Path("out"));
	}

	/// <summary>At the C# 8 floor, with the span package an older framework compiles a parser against.</summary>
	[Fact]
	public void NetStandard20_project_generates_what_its_build_generates()
	{
		using var scratch = new Scratch();

		var project = Project(scratch, "netstandard2.0", "<LangVersion>8.0</LangVersion>");

		File.WriteAllText(project, File.ReadAllText(project).Replace(
			"</Project>",
			"""<ItemGroup><PackageReference Include="System.Memory" Version="4.6.3" /></ItemGroup></Project>"""));

		var built = Build.Generated(project);
		var run   = Tool.Run(scratch.Root, "generate", project, "-o", scratch.Path("out"), "--line-paths", "absolute", "--package", "System.Memory@4.6.3");

		Assert.True(run.Exit == 0, run.ToString());
		Equality.Same(built, scratch.Path("out"));
	}
}

static class Fluent
{
	/// <summary>The value, after doing something with it.</summary>
	public static T Also<T>(this T value, Action<T> action)
	{
		action(value);

		return value;
	}
}
