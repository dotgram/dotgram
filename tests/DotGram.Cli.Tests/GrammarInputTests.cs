using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>A <c>.gram</c> input: the host written from the command line, <c>--option</c>, <c>--sources</c>.</summary>
public sealed class GrammarInputTests
{
	const string Located = "Located.gram";

	static string Copy(Scratch scratch, string name)
	{
		return scratch.Write(name, File.ReadAllText(Path.Combine(Repository.Snapshots, name)));
	}

	[Fact]
	public void The_options_are_the_attribute_s_own()
	{
		var names = GrammarHost.Options().Select(static option => option.Name).ToList();

		foreach (var expected in new[]
		{
			"Suffix", "PartSize", "Lexical", "Carrier", "ConstructionsMayThrow", "BufferedInput", "BufferedBytes", "SpanCaptures",
			"MaxRetained", "BufferSize", "Stacks", "LocationType", "PerCall", "Portable", "Trace",
		})
			Assert.Contains(expected, names);

		Assert.Equal(["Auto", "Immediate", "Tape"], GrammarHost.Options().Single(static option => option.Name == "Carrier").Members.Order(StringComparer.Ordinal));
	}

	[Theory]
	[InlineData("Lexical", "true", "Lexical = true")]
	[InlineData("lexical", "False", "Lexical = false")]
	[InlineData("PartSize", "200", "PartSize = 200")]
	[InlineData("Stacks", "-1", "Stacks = -1")]
	[InlineData("Suffix", "Fast", "Suffix = \"Fast\"")]
	[InlineData("Suffix", "a \"b\" \\ c", "Suffix = \"a \\\"b\\\" \\\\ c\"")]
	[InlineData("Carrier", "immediate", "Carrier = global::DotGram.GramCarrier.Immediate")]
	[InlineData("LocationType", "Demo.ISpan", "LocationType = typeof(Demo.ISpan)")]
	[InlineData("LocationType", "global::Demo.ISpan<int>", "LocationType = typeof(global::Demo.ISpan<int>)")]
	public void An_option_is_written_by_its_type(string name, string value, string written)
	{
		Assert.Equal(written, GrammarHost.Argument(name, value, GrammarHost.Options()));
	}

	[Theory]
	[InlineData("Frobnicate", "1", "has no option 'Frobnicate'")]
	[InlineData("Lexical", "yes", "not true or false")]
	[InlineData("PartSize", "2.5", "not a whole number")]
	[InlineData("Carrier", "Fast", "not one of Auto, Tape, Immediate")]
	[InlineData("LocationType", "X), Lexical = (true", "not a C# type name")]
	public void A_bad_option_lists_what_there_is(string name, string value, string why)
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Calc.gram", "Row = 'a'\n\nparse Row\n");
		var run     = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--option", name + "=" + value);

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains(why, run.Error, StringComparison.Ordinal);
		Assert.Contains("Carrier (Auto|Tape|Immediate)", run.Error, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>
	/// The host and the types its grammar builds, with <c>LocationType</c> as a type in the sources: the
	/// same grammar and the same options a project's build compiles.
	/// </summary>
	[Fact]
	public void A_grammar_with_sources_generates_what_a_project_does()
	{
		using var scratch = new Scratch();

		var grammar = Copy(scratch, Located);

		var types = scratch.Write("src/Types.cs", """
			namespace Demo
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
			""");

		var run = Tool.Run(
			scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--line-paths", "absolute",
			"--namespace", "Demo", "--public", "--option", "LocationType=ILocated", "--option", "Carrier=Tape", "--sources", scratch.Path("src", "*.cs"));

		Assert.True(run.Exit == 0, run.ToString());

		// The project: the same file, a host saying what the command line said.
		scratch.Write("p/" + Located, File.ReadAllText(grammar));
		File.Copy(types, scratch.Path("p", "Types.cs"));
		scratch.Write("p/Host.cs", """
			using DotGram;

			namespace Demo
			{
				[Gram("Located.gram", LocationType = typeof(ILocated), Carrier = GramCarrier.Tape)]
				public partial class Located { }
			}
			""");

		var project = scratch.Write("p/p.csproj", $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>net10.0</TargetFramework>
			    <Nullable>enable</Nullable>
			  </PropertyGroup>
			  <ItemGroup>
			    <AdditionalFiles Include="{Located}" />
			  </ItemGroup>
			  {Build.AnalyzerItems}
			</Project>
			""");

		var built = Build.Generated(project);

		// The grammar's own path is in every #line: the project's copy and the tool's differ only there.
		Assert.Equal(
			File.ReadAllText(Path.Combine(built, "Demo.Located.g.cs")).Replace(scratch.Path("p", Located), "GRAMMAR", StringComparison.Ordinal),
			File.ReadAllText(scratch.Path("out", "Demo.Located.g.cs")).Replace(grammar, "GRAMMAR", StringComparison.Ordinal));

		Assert.Contains("LocationType = typeof(ILocated)", File.ReadAllText(scratch.Path("out", "Located.Host.g.cs")), StringComparison.Ordinal);
	}

	[Fact]
	public void A_type_the_grammar_builds_and_no_source_declares_is_named()
	{
		using var scratch = new Scratch();

		var grammar = Copy(scratch, Located);
		var run     = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--namespace", "Demo", "--option", "Carrier=Tape");

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("'Entry'", run.Output, StringComparison.Ordinal);
		Assert.Contains("add the file that declares it with --sources (or --reference its assembly)", run.Output, StringComparison.Ordinal);
		Assert.False(Directory.Exists(scratch.Path("out")));
	}

	/// <summary>The members a hook calls, in a partial declaration of the host: accepted, it is not a second host.</summary>
	[Fact]
	public void A_partial_host_in_the_sources_gives_the_hooks_their_members()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Calc.gram", """
			Number : @int = digits: ['0'..'9']+ => @(Twice(int.Parse(digits)))

			parse Number
			""");

		var run = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--namespace", "Demo");

		// Without the member: a C# error in the generated parser, reported at the grammar's line.
		Assert.True(run.Exit == ExitCode.GrammarErrors, run.ToString());
		Assert.Matches(@"Calc\.gram\(1,\d+\): error CS0103: .*'Twice'", run.Output);

		scratch.Write("src/Calc.cs", """
			namespace Demo
			{
				partial class Calc
				{
					static int Twice(int value) { return value * 2; }
				}
			}
			""");

		var with = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--namespace", "Demo", "--sources", scratch.Path("src", "Calc.cs"));

		Assert.True(with.Exit == 0, with.ToString());
	}

	[Fact]
	public void A_second_host_in_the_sources_is_refused()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("Calc.gram", "Row = 'a'\n\nparse Row\n");
		var source  = scratch.Write("src/Calc.cs", """
			namespace Demo
			{
				[DotGram.Gram("Calc.gram")]
				partial class Other { }
			}
			""");

		var run = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--sources", source);

		Assert.True(run.Exit == ExitCode.Failure, run.ToString());
		Assert.Contains("already claims Calc.gram", run.Error, StringComparison.Ordinal);
	}

	[Fact]
	public void The_host_is_written_beside_the_parser()
	{
		using var scratch = new Scratch();

		var grammar = scratch.Write("calc-v2.gram", "Row = 'a'\n\nparse Row\n");

		var bad = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"));

		Assert.True(bad.Exit == ExitCode.Failure, bad.ToString());
		Assert.Contains("pass --class", bad.Error, StringComparison.Ordinal);

		var run = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("out"), "--class", "Calc", "--option", "Suffix=Fast");

		Assert.True(run.Exit == 0, run.ToString());

		var host = File.ReadAllText(scratch.Path("out", "Calc.Host.g.cs"));

		Assert.Contains("[global::DotGram.Gram(\"calc-v2.gram\", Suffix = \"Fast\")]", host, StringComparison.Ordinal);
		Assert.Contains("internal partial class Calc", host, StringComparison.Ordinal);
		Assert.Contains("Calc.Host.g.cs", File.ReadAllText(scratch.Path("out", OutputFolder.ManifestName)), StringComparison.Ordinal);
	}

	[Fact]
	public void Defaults_follow_the_framework()
	{
		using var scratch = new Scratch();

		var grammar = Copy(scratch, "Lexical.gram");

		var net10 = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("net10"), "--option", "Lexical=true");
		var net8  = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("net8"), "--option", "Lexical=true", "--framework", "net8.0");
		var at10  = Tool.Run(scratch.Root, "generate", grammar, "-o", scratch.Path("at10"), "--option", "Lexical=true", "--langversion", "10");

		Assert.True(net10.Exit == 0 && net8.Exit == 0 && at10.Exit == 0, net10 + "\n" + net8 + "\n" + at10);

		// C# 12 and 14 both write the lexer's tables as UTF-8 literals; C# 10 cannot.
		Assert.Contains("u8", File.ReadAllText(scratch.Path("net10", "Lexical.g.cs")), StringComparison.Ordinal);
		Assert.Equal(File.ReadAllText(scratch.Path("net10", "Lexical.g.cs")), File.ReadAllText(scratch.Path("net8", "Lexical.g.cs")));
		Assert.NotEqual(File.ReadAllText(scratch.Path("net10", "Lexical.g.cs")), File.ReadAllText(scratch.Path("at10", "Lexical.g.cs")));
	}

	/// <summary>
	/// A folder under a <c>--sources</c> glob that cannot be read is passed over, as MSBuild's globs pass
	/// it over, rather than ending the tool with an unhandled exception; a hidden folder is still looked into.
	/// </summary>
	[Fact]
	public void A_glob_passes_over_a_folder_it_cannot_read()
	{
		if (OperatingSystem.IsWindows())
		{
			Assert.Skip("file modes are Unix's");
			return;
		}

		using var scratch = new Scratch();

		var kept   = scratch.Write("src/a/Kept.cs", "class Kept { }");
		var hidden = scratch.Write("src/.hidden/Hidden.cs", "class Hidden { }");
		var locked = scratch.Path("src", "locked");

		scratch.Write("src/locked/Locked.cs", "class Locked { }");
		File.SetUnixFileMode(locked, UnixFileMode.None);

		try
		{
			// A process with the rights to read it all the same (root) cannot be shown the path at all.
			Assert.SkipWhen(CanList(locked), "the folder stays readable to this process");

			Assert.Equal([hidden, kept], GrammarHost.Sources([scratch.Path("src") + "/**/*.cs"]));
		}
		finally
		{
			File.SetUnixFileMode(locked, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
		}

		static bool CanList(string folder)
		{
			try
			{
				Directory.EnumerateFileSystemEntries(folder).Any();
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}
	}
}
